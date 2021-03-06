// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition
{
    // Provides helpers for creating and dealing with Exports
    internal static partial class ExportServices
    {
        private static readonly MethodInfo _createStronglyTypedLazyOfTM = typeof(ExportServices).GetMethod("CreateStronglyTypedLazyOfTM", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _createStronglyTypedLazyOfT = typeof(ExportServices).GetMethod("CreateStronglyTypedLazyOfT", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _createSemiStronglyTypedLazy = typeof(ExportServices).GetMethod("CreateSemiStronglyTypedLazy", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly Type DefaultMetadataViewType = typeof(IDictionary<string, object>);
        internal static readonly Type DefaultExportedValueType = typeof(object);

        internal static bool IsDefaultMetadataViewType(Type metadataViewType)
        {
            Assumes.NotNull(metadataViewType);

            // Consider all types that IDictionary<string, object> derives from, such
            // as ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>> 
            // and IEnumerable, as default metadata view
            return metadataViewType.IsAssignableFrom(DefaultMetadataViewType);
        }

        internal static bool IsDictionaryConstructorViewType(Type metadataViewType)
        {
            Assumes.NotNull(metadataViewType);

            // Does the view type have a constructor that is a Dictionary<string, object>
            return metadataViewType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                    Type.DefaultBinder,
                                                    new Type[] { typeof(IDictionary<string, object>) },
                                                    new ParameterModifier[0]) != null;
        }

        internal static Func<Export, object> CreateStronglyTypedLazyFactory(Type exportType, Type metadataViewType)
        {
            MethodInfo genericMethod = null;
            if (metadataViewType != null)
            {
                genericMethod = _createStronglyTypedLazyOfTM.MakeGenericMethod(exportType ?? ExportServices.DefaultExportedValueType, metadataViewType);
            }
            else
            {
                genericMethod = _createStronglyTypedLazyOfT.MakeGenericMethod(exportType ?? ExportServices.DefaultExportedValueType);
            }
            Assumes.NotNull(genericMethod);
            return (Func<Export, object>)Delegate.CreateDelegate(typeof(Func<Export, object>), genericMethod);
        }

        internal static Func<Export, Lazy<object, object>> CreateSemiStronglyTypedLazyFactory(Type exportType, Type metadataViewType)
        {
            MethodInfo genericMethod = _createSemiStronglyTypedLazy.MakeGenericMethod(
                exportType ?? ExportServices.DefaultExportedValueType,
                metadataViewType ?? ExportServices.DefaultMetadataViewType);
            Assumes.NotNull(genericMethod);
            return (Func<Export, Lazy<object, object>>)Delegate.CreateDelegate(typeof(Func<Export, Lazy<object,object>>), genericMethod);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static Lazy<T, M> CreateStronglyTypedLazyOfTM<T, M>(Export export)
        {
            IDisposable disposable = export as IDisposable;
            if (disposable != null)
            {
                return new DisposableLazy<T, M>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    disposable,
                    LazyThreadSafetyMode.PublicationOnly);
            }
            else
            {
                return new Lazy<T, M>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    LazyThreadSafetyMode.PublicationOnly);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static Lazy<T> CreateStronglyTypedLazyOfT<T>(Export export)
        {
            IDisposable disposable = export as IDisposable;
            if (disposable != null)
            {
                return new DisposableLazy<T>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    disposable,
                    LazyThreadSafetyMode.PublicationOnly);
            }
            else
            {
                return new Lazy<T>(() => ExportServices.GetCastedExportedValue<T>(export), LazyThreadSafetyMode.PublicationOnly);

            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static Lazy<object, object> CreateSemiStronglyTypedLazy<T, M>(Export export)
        {
            IDisposable disposable = export as IDisposable;
            if (disposable != null)
            {
                return new DisposableLazy<object, object>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    disposable,
                    LazyThreadSafetyMode.PublicationOnly);
            }
            else
            {
                return new Lazy<object, object>(
                    () => ExportServices.GetCastedExportedValue<T>(export),
                    AttributedModelServices.GetMetadataView<M>(export.Metadata),
                    LazyThreadSafetyMode.PublicationOnly);
            }
        }

        internal static T GetCastedExportedValue<T>(Export export)
        {
            return CastExportedValue<T>(export.ToElement(), export.Value);
        }

        internal static T CastExportedValue<T>(ICompositionElement element, object exportedValue)
        {
            object typedExportedValue = null;

            bool succeeded = ContractServices.TryCast(typeof(T), exportedValue, out typedExportedValue);
            if (!succeeded)
            {
                throw new CompositionContractMismatchException(string.Format(CultureInfo.CurrentCulture,
                    Strings.ContractMismatch_ExportedValueCannotBeCastToT,
                    element.DisplayName,
                    typeof(T)));
            }

            return (T)typedExportedValue;
        }

        internal static ExportCardinalityCheckResult CheckCardinality<T>(ImportDefinition definition, IEnumerable<T> enumerable)
        {
            EnumerableCardinality actualCardinality = (enumerable != null) ? enumerable.GetCardinality() : EnumerableCardinality.Zero;

            return MatchCardinality(actualCardinality, definition.Cardinality);
        }


        private static ExportCardinalityCheckResult MatchCardinality(EnumerableCardinality actualCardinality, ImportCardinality importCardinality)
        {
            switch (actualCardinality)
            {
                case EnumerableCardinality.Zero:
                    if (importCardinality == ImportCardinality.ExactlyOne)
                    {
                        return ExportCardinalityCheckResult.NoExports;
                    }
                    break;

                case EnumerableCardinality.TwoOrMore:
                    if (importCardinality.IsAtMostOne())
                    {
                        return ExportCardinalityCheckResult.TooManyExports;
                    }
                    break;

                default:
                    Assumes.IsTrue(actualCardinality == EnumerableCardinality.One);
                    break;

            }

            return ExportCardinalityCheckResult.Match;
        }
    }
}
