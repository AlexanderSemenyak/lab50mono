// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Diagnostics;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class CatalogExportProvider : ExportProvider, IDisposable
    {
        private class InnerCatalogExportProvider : ExportProvider
        {
            Func<ImportDefinition, AtomicComposition, IEnumerable<Export>> _getExportsCore;
            
            public InnerCatalogExportProvider(Func<ImportDefinition, AtomicComposition, IEnumerable<Export>> getExportsCore )
            {
                this._getExportsCore = getExportsCore;
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
            {
                Assumes.NotNull(this._getExportsCore);
                return this._getExportsCore(definition, atomicComposition);
            }
        }

        private readonly CompositionLock _lock;
        private Dictionary<ComposablePartDefinition, CatalogPart> _activatedParts = new Dictionary<ComposablePartDefinition, CatalogPart>();
        private HashSet<ComposablePartDefinition> _rejectedParts = new HashSet<ComposablePartDefinition>();
        private ConditionalWeakTable<object, List<ComposablePart>> _gcRoots;
        private HashSet<IDisposable> _partsToDispose = new HashSet<IDisposable>();
        private ComposablePartCatalog _catalog;
        private volatile bool _isDisposed = false;
        private volatile bool _isRunning = false;
        private ExportProvider _sourceProvider;
        private ImportEngine _importEngine;
        private CompositionOptions _compositionOptions;
        private ExportProvider _innerExportProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogExportProvider"/> class.
        /// </summary>
        /// <param name="catalog">
        ///     The <see cref="ComposablePartCatalog"/> that the <see cref="CatalogExportProvider"/>
        ///     uses to produce <see cref="Export"/> objects.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="catalog"/> is <see langword="null"/>.
        /// </exception>
        public CatalogExportProvider(ComposablePartCatalog catalog)
            : this(catalog, CompositionOptions.Default)
        {
        }

        public CatalogExportProvider(ComposablePartCatalog catalog, bool isThreadSafe)
            : this(catalog, isThreadSafe ? CompositionOptions.IsThreadSafe : CompositionOptions.Default)
        {
        }

        public CatalogExportProvider(ComposablePartCatalog catalog, CompositionOptions compositionOptions)
        {
            Requires.NotNull(catalog, "catalog");
            if (compositionOptions > (CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService))
            {
                throw new ArgumentOutOfRangeException("compositionOptions");
            }

            this._catalog = catalog;
            this._compositionOptions = compositionOptions;
            var notifyCatalogChanged = this._catalog as INotifyComposablePartCatalogChanged;
            if (notifyCatalogChanged != null)
            {
                notifyCatalogChanged.Changing += this.OnCatalogChanging;
            }

            CompositionScopeDefinition scopeDefinition = this._catalog as CompositionScopeDefinition;
            if (scopeDefinition != null)
            {
                this._innerExportProvider = new AggregateExportProvider(new ScopeManager(this, scopeDefinition), new InnerCatalogExportProvider(InternalGetExportsCore));
            }
            else
            {
                this._innerExportProvider = new InnerCatalogExportProvider(InternalGetExportsCore);
            }            
            this._lock = new CompositionLock(compositionOptions.HasFlag(CompositionOptions.IsThreadSafe));
        }


        /// <summary>
        ///     Gets the composable part catalog that the provider users to 
        ///     produce exports.
        /// </summary>
        /// <value>
        ///     The <see cref="ComposablePartCatalog"/> that the 
        ///     <see cref="CatalogExportProvider"/>
        ///     uses to produce <see cref="Export"/> objects.
        /// </value>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="CompositionContainer"/> has been disposed of.
        /// </exception>
        public ComposablePartCatalog Catalog
        {
            get
            {
                ThrowIfDisposed();
                Contract.Ensures(Contract.Result<ComposablePartCatalog>() != null);

                return this._catalog;
            }
        }

        /// <summary>
        ///     Gets the export provider which provides the provider access to additional
        ///     exports.
        /// </summary>
        /// <value>
        ///     The <see cref="ExportProvider"/> which provides the 
        ///     <see cref="CatalogExportProvider"/> access to additional
        ///     <see cref="Export"/> objects. The default is <see langword="null"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     This property has already been set.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     The methods on the <see cref="CatalogExportProvider"/> 
        ///     have already been accessed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="CatalogExportProvider"/> has been disposed of.
        /// </exception>
        /// <remarks>
        ///     This property must be set before accessing any methods on the 
        ///     <see cref="CatalogExportProvider"/>.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="EnsureCanSet ensures that the property is set only once, Dispose is not required")]
        public ExportProvider SourceProvider
        {
            get
            {
                this.ThrowIfDisposed();
                using (this._lock.LockStateForRead())
                {
                    return this._sourceProvider;
                }
            }
            set
            {
                this.ThrowIfDisposed();

                Requires.NotNull(value, "value");

                ImportEngine newImportEngine = null;
                AggregateExportProvider aggregateExportProvider = null;
                ExportProvider sourceProvider = value;

                bool isThrowing = true;
                try
                {
                    newImportEngine = new ImportEngine(sourceProvider, this._compositionOptions);

                    sourceProvider.ExportsChanging += this.OnExportsChangingInternal;

                    using (this._lock.LockStateForWrite())
                    {
                        this.EnsureCanSet(this._sourceProvider);

                        this._sourceProvider = sourceProvider;
                        this._importEngine = newImportEngine;

                        isThrowing = false;
                    }
                }
                finally
                {
                    if (isThrowing)
                    {
                        sourceProvider.ExportsChanging -= this.OnExportsChangingInternal;
                        newImportEngine.Dispose();
                        if (aggregateExportProvider != null)
                        {
                            aggregateExportProvider.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this._isDisposed)
                {
                    bool disposeLock = false;
                    INotifyComposablePartCatalogChanged catalogToUnsubscribeFrom = null;
                    HashSet<IDisposable> partsToDispose = null;
                    ImportEngine importEngine = null;
                    ExportProvider sourceProvider = null;
                    AggregateExportProvider aggregateExportProvider = null;
                    try
                    {
                        using (this._lock.LockStateForWrite())
                        {
                            if (!this._isDisposed)
                            {
                                catalogToUnsubscribeFrom = this._catalog as INotifyComposablePartCatalogChanged;
                                this._catalog = null;

                                aggregateExportProvider = this._innerExportProvider as AggregateExportProvider;
                                this._innerExportProvider = null;

                                sourceProvider = this._sourceProvider;
                                this._sourceProvider = null;

                                importEngine = this._importEngine;
                                this._importEngine = null;

                                partsToDispose = this._partsToDispose;
                                this._gcRoots = null;

                                disposeLock = true;
                                this._isDisposed = true;
                                
                            }
                        }
                    }
                    finally
                    {
                        if (catalogToUnsubscribeFrom != null)
                        {
                            catalogToUnsubscribeFrom.Changing -= this.OnCatalogChanging;
                        }

                        if (aggregateExportProvider != null)
                        {
                            aggregateExportProvider.Dispose();
                        }

                        if (sourceProvider != null)
                        {
                            sourceProvider.ExportsChanging -= this.OnExportsChangingInternal;
                        }

                        if (importEngine != null)
                        {
                            importEngine.Dispose();
                        }

                        if (partsToDispose != null)
                        {
                            foreach (var part in partsToDispose)
                            {
                                part.Dispose();
                            }
                        }

                        if (disposeLock)
                        {
                            this._lock.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all exports that match the conditions of the specified import.
        /// </summary>
        /// <param name="definition">The <see cref="ImportDefinition"/> that defines the conditions of the
        /// <see cref="Export"/> to get.</param>
        /// <returns></returns>
        /// <result>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Export"/> objects that match
        /// the conditions defined by <see cref="ImportDefinition"/>, if found; otherwise, an
        /// empty <see cref="IEnumerable{T}"/>.
        /// </result>
        /// <remarks>
        /// 	<note type="inheritinfo">
        /// The implementers should not treat the cardinality-related mismatches as errors, and are not
        /// expected to throw exceptions in those cases.
        /// For instance, if the import requests exactly one export and the provider has no matching exports or more than one,
        /// it should return an empty <see cref="IEnumerable{T}"/> of <see cref="Export"/>.
        /// </note>
        /// </remarks>
        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            this.ThrowIfDisposed();
            this.EnsureRunning();

            Assumes.NotNull(this._innerExportProvider);

            IEnumerable<Export> exports;
            this._innerExportProvider.TryGetExports(definition, atomicComposition, out exports);
            return exports;
        }

        private IEnumerable<Export> InternalGetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            this.ThrowIfDisposed();
            this.EnsureRunning();
            
            // Use the version of the catalog appropriate to this atomicComposition
            ComposablePartCatalog currentCatalog = atomicComposition.GetValueAllowNull(this._catalog);

            IPartCreatorImportDefinition partCreatorDefinition = definition as IPartCreatorImportDefinition;
            bool isExportFactory = false;

            if (partCreatorDefinition != null)
            {
                definition = partCreatorDefinition.ProductImportDefinition;
                isExportFactory = true;
            }

            CreationPolicy importPolicy = definition.GetRequiredCreationPolicy();

            List<Export> exports = new List<Export>();
            foreach (var partDefinitionAndExportDefinition in currentCatalog.GetExports(definition))
            {
                if (!this.IsRejected(partDefinitionAndExportDefinition.Item1, atomicComposition))
                {
                    exports.Add(this.CreateExport(partDefinitionAndExportDefinition.Item1, partDefinitionAndExportDefinition.Item2, isExportFactory, importPolicy));
                }
            }

            return exports;
        }

        private Export CreateExport(ComposablePartDefinition partDefinition, ExportDefinition exportDefinition, bool isExportFactory, CreationPolicy importPolicy)
        {
            if (isExportFactory)
            {
                return new PartCreatorExport(this,
                            partDefinition,
                            exportDefinition);
            }
            else
            {
                return CatalogExport.CreateExport(this,
                            partDefinition,
                            exportDefinition,
                            importPolicy);
            }
        }



        private void OnExportsChangingInternal(object sender, ExportsChangeEventArgs e)
        {
            UpdateRejections(e.AddedExports.Concat(e.RemovedExports), e.AtomicComposition);
        }

        private static ExportDefinition[] GetExportsFromPartDefinitions(IEnumerable<ComposablePartDefinition> partDefinitions)
        {
            List<ExportDefinition> exports = new List<ExportDefinition>();

            foreach (var partDefinition in partDefinitions)
            {
                foreach (var export in partDefinition.ExportDefinitions)
                {
                    exports.Add(export);

                    // While creating a PartCreatorExportDefinition for every changed definition may not be the most
                    // efficient way to do this the PartCreatorExportDefinition is very efficient and doesn't do any
                    // real work unless its metadata is pulled on. If this turns out to be a bottleneck then we
                    // will need to start tracking all the PartCreator's we hand out and only send those which we 
                    // have handed out. In fact we could do the same thing for all the Exports if we wished but 
                    // that requires a cache management which we don't want to do at this point.
                    exports.Add(new PartCreatorExportDefinition(export));
                }
            }

            return exports.ToArray();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void OnCatalogChanging(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            using (var atomicComposition = new AtomicComposition(e.AtomicComposition))
            {
                // Save the preview catalog to use in place of the original while handling
                // this event
                atomicComposition.SetValue(this._catalog,
                    new CatalogChangeProxy(this._catalog, e.AddedDefinitions, e.RemovedDefinitions));

                IEnumerable<ExportDefinition> addedExports = GetExportsFromPartDefinitions(e.AddedDefinitions);
                IEnumerable<ExportDefinition> removedExports = GetExportsFromPartDefinitions(e.RemovedDefinitions);

                // Remove any parts based on eliminated definitions (in a atomicComposition-friendly
                // fashion)
                foreach (var definition in e.RemovedDefinitions)
                {
                    CatalogPart removedPart = null;
                    bool removed = false;

                    using (this._lock.LockStateForRead())
                    {
                        removed = this._activatedParts.TryGetValue(definition, out removedPart);
                    }

                    if (removed)
                    {
                        var capturedDefinition = definition;
                        this.ReleasePart(null, removedPart, atomicComposition);
                        atomicComposition.AddCompleteActionAllowNull(() =>
                        {
                            using (this._lock.LockStateForWrite())
                            {
                                this._activatedParts.Remove(capturedDefinition);
                            }
                        });
                    }
                }

                UpdateRejections(addedExports.ConcatAllowingNull(removedExports), atomicComposition);

                this.OnExportsChanging(
                    new ExportsChangeEventArgs(addedExports, removedExports, atomicComposition));

                atomicComposition.AddCompleteAction(() => this.OnExportsChanged(
                    new ExportsChangeEventArgs(addedExports, removedExports, null)));

                atomicComposition.Complete();
            }
        }

        private CatalogPart GetComposablePart(ComposablePartDefinition partDefinition, bool isSharedPart)
        {
            this.ThrowIfDisposed();
            this.EnsureRunning();

            CatalogPart catalogPart = null;

            if (isSharedPart)
            {
                catalogPart = this.GetSharedPart(partDefinition);
            }
            else
            {
                ComposablePart part = partDefinition.CreatePart();
                catalogPart = new CatalogPart(part);

                IDisposable disposablePart = part as IDisposable;
                if (disposablePart != null)
                {
                    using (this._lock.LockStateForWrite())
                    {
                        this._partsToDispose.Add(disposablePart);
                    }
                }
            }

            return catalogPart;
        }

        private CatalogPart GetSharedPart(ComposablePartDefinition partDefinition)
        {
            CatalogPart catalogPart = null;

            // look up the part
            using (this._lock.LockStateForRead())
            {
                if (this._activatedParts.TryGetValue(partDefinition, out catalogPart))
                {
                    return catalogPart;
                }
            }

            // create a part outside of the lock
            ComposablePart newPart = partDefinition.CreatePart();
            IDisposable disposableNewPart = newPart as IDisposable;

            using (this._lock.LockStateForWrite())
            {
                // check if the part is still not there
                if (!this._activatedParts.TryGetValue(partDefinition, out catalogPart))
                {
                    catalogPart = new CatalogPart(newPart);
                    this._activatedParts.Add(partDefinition, catalogPart);
                    if (disposableNewPart != null)
                    {
                        this._partsToDispose.Add(disposableNewPart);
                    }

                    // indiacate the the part has been added
                    newPart = null;
                    disposableNewPart = null;
                }
            }

            // if disposableNewPart != null, this means we have created a new instance of something disposable and not used it
            // Dispose of it now
            if (disposableNewPart != null)
            {
                disposableNewPart.Dispose();
            }

            return catalogPart;
        }

        private object GetExportedValue(CatalogPart part, ExportDefinition export, bool isSharedPart)
        {
            this.ThrowIfDisposed();
            this.EnsureRunning();

            Assumes.NotNull(part, export);

            // We don't protect against thread racing here, as "importsSatisfied" is merely an optimization
            // if two threads satisfy imports twice, the results is the same, just the perf hit is heavier.

            bool importsSatisfied = part.ImportsSatisfied;
            ImportEngine importEngine = importsSatisfied ? null : this._importEngine;

            object exportedValue = CompositionServices.GetExportedValueFromComposedPart(
                importEngine, part.Part, export);

            if (!importsSatisfied)
            {
                // and set "ImportsSatisfied" to true
                part.ImportsSatisfied = true;
            }

            // Only hold conditional references for recomposable non-shared parts because we are 
            // already holding strong references to the shared parts.
            if (exportedValue != null && !isSharedPart && part.Part.IsRecomposable())
            {
                this.PreventPartCollection(exportedValue, part.Part);
            }

            return exportedValue;
        }

        private void ReleasePart(object exportedValue, CatalogPart catalogPart, AtomicComposition atomicComposition)
        {
            this.ThrowIfDisposed();
            this.EnsureRunning();

            Assumes.NotNull(catalogPart);

            this._importEngine.ReleaseImports(catalogPart.Part, atomicComposition);

            if (exportedValue != null)
            {
                atomicComposition.AddCompleteActionAllowNull(() =>
                {
                    this.AllowPartCollection(exportedValue);
                });
            }

            IDisposable diposablePart = catalogPart.Part as IDisposable;
            if (diposablePart != null)
            {
                atomicComposition.AddCompleteActionAllowNull(() =>
                {
                    bool removed = false;
                    using (this._lock.LockStateForWrite())
                    {
                        removed = this._partsToDispose.Remove(diposablePart);
                    }
                    if (removed)
                    {
                        diposablePart.Dispose();
                    }
                });
            }
        }

        private void PreventPartCollection(object exportedValue, ComposablePart part)
        {
            Assumes.NotNull(exportedValue, part);

            using (this._lock.LockStateForWrite())
            {
                List<ComposablePart> partList;

                ConditionalWeakTable<object, List<ComposablePart>> gcRoots = this._gcRoots;
                if (gcRoots == null)
                {
                    gcRoots = new ConditionalWeakTable<object, List<ComposablePart>>();
                }

                if (!gcRoots.TryGetValue(exportedValue, out partList))
                {
                    partList = new List<ComposablePart>();
                    gcRoots.Add(exportedValue, partList);
                }

                partList.Add(part);

                if (this._gcRoots == null)
                {
                    Thread.MemoryBarrier();
                    this._gcRoots = gcRoots;
                }
            }
        }

        private void AllowPartCollection(object gcRoot)
        {
            if (this._gcRoots != null)
            {
                using (this._lock.LockStateForWrite())
                {
                    this._gcRoots.Remove(gcRoot);
                }
            }
        }

        private bool IsRejected(ComposablePartDefinition definition, AtomicComposition atomicComposition)
        {
            // Check to see if we're currently working on the definition in question.
            // Recursive queries always answer optimistically, as if the definition hasn't
            // been rejected - because if it is we can discard all decisions that were based
            // on the faulty assumption in the first place.
            var forceRejectionTest = false;
            if (atomicComposition != null)
            {
                var atomicCompositionQuery = GetAtomicCompositionQuery(atomicComposition);
                AtomicCompositionQueryState state = atomicCompositionQuery(definition);
                switch (state)
                {
                    case AtomicCompositionQueryState.TreatAsRejected:
                        return true;
                    case AtomicCompositionQueryState.TreatAsValidated:
                        return false;
                    case AtomicCompositionQueryState.NeedsTesting:
                        forceRejectionTest = true;
                        break;
                    default:
                        Assumes.IsTrue(state == AtomicCompositionQueryState.Unknown);
                        // Need to do the work to determine the state
                        break;
                }
            }

            if (!forceRejectionTest)
            {
                // Next, anything that has been activated is not rejected
                using (this._lock.LockStateForRead())
                {
                    if (this._activatedParts.ContainsKey(definition))
                    {
                        return false;
                    }

                    // Last stop before doing the hard work: check a specific registry of rejected parts
                    if (this._rejectedParts.Contains(definition))
                    {
                        return true;
                    }
                }
            }

            // Determine whether or not the definition's imports can be satisfied
            return DetermineRejection(definition, atomicComposition);
        }

        private bool DetermineRejection(ComposablePartDefinition definition, AtomicComposition parentAtomicComposition)
        {
            ChangeRejectedException exception = null;

            using (var localAtomicComposition = new AtomicComposition(parentAtomicComposition))
            {
                // The part definition we're currently working on is treated optimistically
                // as if we know it hasn't been rejected.  This handles recursion, and if we
                // later decide that it has been rejected we'll discard all nested progress so
                // all side-effects of the mistake are erased.
                //
                // Note that this means that recursive failures that would be detected by the
                // import engine are not discovered by rejection currently.  Loops among
                // prerequisites, runaway import chains involving factories, and prerequisites
                // that cannot be fully satisfied still result in runtime errors.  Doing
                // otherwise would be possible but potentially expensive - and could be a v2
                // improvement if deemed worthwhile.
                UpdateAtomicCompositionQuery(localAtomicComposition,
                    def => definition.Equals(def), AtomicCompositionQueryState.TreatAsValidated);

                var newPart = definition.CreatePart();
                try
                {
                    this._importEngine.PreviewImports(newPart, localAtomicComposition);

                    // Reuse the partially-fleshed out part the next time we need a shared
                    // instance to keep the expense of pre-validation to a minimum.  Note that
                    // _activatedParts holds references to both shared and non-shared parts.
                    // The non-shared parts will only be used for rejection purposes only but
                    // the shared parts will be handed out when requested via GetExports as 
                    // well as be used for rejection purposes.
                    localAtomicComposition.AddCompleteActionAllowNull(() =>
                    {
                        using (this._lock.LockStateForWrite())
                        {
                            if (!this._activatedParts.ContainsKey(definition))
                            {
                                this._activatedParts.Add(definition, new CatalogPart(newPart));
                                IDisposable newDisposablePart = newPart as IDisposable;
                                if (newDisposablePart != null)
                                {
                                    this._partsToDispose.Add(newDisposablePart);
                                }
                            }
                        }
                    });

                    // Success! Complete any recursive work that was conditioned on this part's validation
                    localAtomicComposition.Complete();

                    return false;
                }
                catch (ChangeRejectedException ex)
                {
                    exception = ex;
                }
            }

            // If we've reached this point then this part has been rejected so we need to 
            // record the rejection in our parent composition or execute it immediately if 
            // one doesn't exist.
            parentAtomicComposition.AddCompleteActionAllowNull(() =>
            {
                using (this._lock.LockStateForWrite())
                {
                    this._rejectedParts.Add(definition);
                }

                CompositionTrace.PartDefinitionRejected(definition, exception);

            });
            if (parentAtomicComposition != null)
            {
                UpdateAtomicCompositionQuery(parentAtomicComposition,
                    def => definition.Equals(def), AtomicCompositionQueryState.TreatAsRejected);
            }

            return true;
        }

        private void UpdateRejections(IEnumerable<ExportDefinition> changedExports, AtomicComposition atomicComposition)
        {
            using (var localAtomicComposition = new AtomicComposition(atomicComposition))
            {
                // Reconsider every part definition that has been previously
                // rejected to see if any of them can be added back.
                var affectedRejections = new HashSet<ComposablePartDefinition>();
                var atomicCompositionQuery = GetAtomicCompositionQuery(localAtomicComposition);

                ComposablePartDefinition[] rejectedParts;
                using (this._lock.LockStateForRead())
                {
                    rejectedParts = this._rejectedParts.ToArray();
                }
                foreach (var definition in rejectedParts)
                {
                    if (atomicCompositionQuery(definition) == AtomicCompositionQueryState.TreatAsValidated)
                    {
                        continue;
                    }

                    foreach (var import in definition.ImportDefinitions.Where(ImportEngine.IsRequiredImportForPreview))
                    {
                        if (changedExports.Any(export => import.IsConstraintSatisfiedBy(export)))
                        {
                            affectedRejections.Add(definition);
                            break;
                        }
                    }
                }
                UpdateAtomicCompositionQuery(localAtomicComposition,
                    def => affectedRejections.Contains(def), AtomicCompositionQueryState.NeedsTesting);

                // Determine if any of the resurrectable parts is now available so that we can
                // notify listeners of the exact changes to exports
                var resurrectedExports = new List<ExportDefinition>();

                foreach (var partDefinition in affectedRejections)
                {
                    if (!IsRejected(partDefinition, localAtomicComposition))
                    {
                        // Notify listeners of the newly available exports and
                        // prepare to remove the rejected part from the list of rejections
                        resurrectedExports.AddRange(partDefinition.ExportDefinitions);

                        // Capture the local so that the closure below refers to the current definition
                        // in the loop and not the value of 'partDefinition' when the closure executes
                        var capturedPartDefinition = partDefinition;
                        localAtomicComposition.AddCompleteAction(() =>
                        {
                            using (this._lock.LockStateForWrite())
                            {
                                this._rejectedParts.Remove(capturedPartDefinition);                                
                            }

                            CompositionTrace.PartDefinitionResurrected(capturedPartDefinition);
                        });
                    }
                }

                // Notify anyone sourcing exports that the resurrected exports have appeared
                if (resurrectedExports.Any())
                {
                    this.OnExportsChanging(
                        new ExportsChangeEventArgs(resurrectedExports, new ExportDefinition[0], localAtomicComposition));

                    localAtomicComposition.AddCompleteAction(() => this.OnExportsChanged(
                        new ExportsChangeEventArgs(resurrectedExports, new ExportDefinition[0], null)));
                }

                localAtomicComposition.Complete();
            }
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage("Microsoft.Contracts", "CC1053", Justification = "Suppressing warning because this validator has no public contract")]
        private void ThrowIfDisposed()
        {
            if (this._isDisposed)
            {
                throw ExceptionBuilder.CreateObjectDisposed(this);
            }
        }

        /// <summary>
        ///  EnsureCanRun must be called from within a lock.
        /// </summary>
        [DebuggerStepThrough]
        private void EnsureCanRun()
        {
            if ((this._sourceProvider == null) || (this._importEngine == null))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ObjectMustBeInitialized, "SourceProvider")); // NOLOC
            }
        }

        [DebuggerStepThrough]
        private void EnsureRunning()
        {
            if (!this._isRunning)
            {
                using (this._lock.LockStateForWrite())
                {
                    if (!this._isRunning)
                    {
                        this.EnsureCanRun();
                        this._isRunning = true;
                    }
                }
            }
        }

        /// <summary>
        ///  EnsureCanSet<T> must be called from within a lock.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentValue"></param>
        [DebuggerStepThrough]
        private void EnsureCanSet<T>(T currentValue)
            where T : class
        {
            if ((this._isRunning) || (currentValue != null))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ObjectAlreadyInitialized));
            }
        }

        private Func<ComposablePartDefinition, AtomicCompositionQueryState> GetAtomicCompositionQuery(AtomicComposition atomicComposition)
        {
            Func<ComposablePartDefinition, AtomicCompositionQueryState> atomicCompositionQuery;
            atomicComposition.TryGetValue(this, out atomicCompositionQuery);

            if (atomicCompositionQuery == null)
            {
                return (definition) => AtomicCompositionQueryState.Unknown;
            }

            return atomicCompositionQuery;
        }

        private void UpdateAtomicCompositionQuery(
            AtomicComposition atomicComposition,
            Func<ComposablePartDefinition, bool> query,
            AtomicCompositionQueryState state)
        {
            var parentQuery = GetAtomicCompositionQuery(atomicComposition);
            Func<ComposablePartDefinition, AtomicCompositionQueryState> newQuery = definition =>
            {
                if (query(definition))
                {
                    return state;
                }
                return parentQuery(definition);
            };

            atomicComposition.SetValue(this, newQuery);
        }

        private enum AtomicCompositionQueryState
        {
            Unknown,
            TreatAsRejected,
            TreatAsValidated,
            NeedsTesting
        };

        private class CatalogPart
        {
            private volatile bool _importsSatisfied = false;
            public CatalogPart(ComposablePart part)
            {
                this.Part = part;
            }
            public ComposablePart Part { get; private set; }

            public bool ImportsSatisfied
            {
                get
                {
                    return this._importsSatisfied;
                }
                set
                {
                    this._importsSatisfied = value;
                }
            }
        }
    }
}
