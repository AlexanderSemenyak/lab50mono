// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using System.Diagnostics.Contracts;

namespace System.ComponentModel.Composition.ReflectionModel
{
#if FEATURE_CAS_APTCA
    [System.Security.SecurityCritical]
#endif //FEATURE_CAS_APTCA
    public static class ReflectionModelServices
    {
        public static Lazy<Type> GetPartType(ComposablePartDefinition partDefinition)
        {
            Requires.NotNull(partDefinition, "partDefinition");
            Contract.Ensures(Contract.Result<Lazy<Type>>() != null);

            ReflectionComposablePartDefinition reflectionPartDefinition = partDefinition as ReflectionComposablePartDefinition;
            if (reflectionPartDefinition == null)
            {
                throw ExceptionBuilder.CreateReflectionModelInvalidPartDefinition("partDefinition", partDefinition.GetType());
            }

            return reflectionPartDefinition.GetLazyPartType();
        }

        public static bool IsDisposalRequired(ComposablePartDefinition partDefinition)
        {
            Requires.NotNull(partDefinition, "partDefinition");

            ReflectionComposablePartDefinition reflectionPartDefinition = partDefinition as ReflectionComposablePartDefinition;
            if (reflectionPartDefinition == null)
            {
                throw ExceptionBuilder.CreateReflectionModelInvalidPartDefinition("partDefinition", partDefinition.GetType());
            }

            return reflectionPartDefinition.IsDisposalRequired;
        }

        public static LazyMemberInfo GetExportingMember(ExportDefinition exportDefinition)
        {
            Requires.NotNull(exportDefinition, "exportDefinition");

            ReflectionMemberExportDefinition reflectionExportDefinition = exportDefinition as ReflectionMemberExportDefinition;
            if (reflectionExportDefinition == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidExportDefinition, exportDefinition.GetType()),
                    "exportDefinition");
            }

            return reflectionExportDefinition.ExportingLazyMember;
        }

        public static LazyMemberInfo GetImportingMember(ImportDefinition importDefinition)
        {
            Requires.NotNull(importDefinition, "importDefinition");

            ReflectionMemberImportDefinition reflectionMemberImportDefinition = importDefinition as ReflectionMemberImportDefinition;
            if (reflectionMemberImportDefinition == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidMemberImportDefinition, importDefinition.GetType()),
                    "importDefinition");
            }

            return reflectionMemberImportDefinition.ImportingLazyMember;
        }

        public static Lazy<ParameterInfo> GetImportingParameter(ImportDefinition importDefinition)
        {
            Requires.NotNull(importDefinition, "importDefinition");
            Contract.Ensures(Contract.Result<Lazy<ParameterInfo>>() != null);

            ReflectionParameterImportDefinition reflectionParameterImportDefinition = importDefinition as ReflectionParameterImportDefinition;
            if (reflectionParameterImportDefinition == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidParameterImportDefinition, importDefinition.GetType()),
                    "importDefinition");
            }

            return reflectionParameterImportDefinition.ImportingLazyParameter;
        }

        public static bool IsImportingParameter(ImportDefinition importDefinition)
        {
            Requires.NotNull(importDefinition, "importDefinition");

            ReflectionImportDefinition reflectionImportDefinition = importDefinition as ReflectionImportDefinition;
            if (reflectionImportDefinition == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidImportDefinition, importDefinition.GetType()),
                    "importDefinition");
            }

            return (importDefinition is ReflectionParameterImportDefinition);
        }

        public static bool IsExportFactoryImportDefinition(ImportDefinition importDefinition)
        {
            Requires.NotNull(importDefinition, "importDefinition");

            return (importDefinition is IPartCreatorImportDefinition);
        }

        public static ContractBasedImportDefinition GetExportFactoryProductImportDefinition(ImportDefinition importDefinition)
        {
            Requires.NotNull(importDefinition, "importDefinition");
            Contract.Ensures(Contract.Result<ContractBasedImportDefinition>() != null);

            IPartCreatorImportDefinition partCreatorImportDefinition = importDefinition as IPartCreatorImportDefinition;
            if (partCreatorImportDefinition == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidImportDefinition, importDefinition.GetType()),
                    "importDefinition");
            }

            return partCreatorImportDefinition.ProductImportDefinition;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ComposablePartDefinition CreatePartDefinition(
            Lazy<Type> partType,
            bool isDisposalRequired,
            Lazy<IEnumerable<ImportDefinition>> imports,
            Lazy<IEnumerable<ExportDefinition>> exports,
            Lazy<IDictionary<string, object>> metadata,
            ICompositionElement origin)
        {
            Requires.NotNull(partType, "partType");
            Contract.Ensures(Contract.Result<ComposablePartDefinition>() != null);

            return new ReflectionComposablePartDefinition(
                new ReflectionPartCreationInfo(
                    partType,
                    isDisposalRequired,
                    imports,
                    exports,
                    metadata,
                    origin));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ExportDefinition CreateExportDefinition(
            LazyMemberInfo exportingMember,
            string contractName,
            Lazy<IDictionary<string, object>> metadata,
            ICompositionElement origin)
        {
            Requires.NotNullOrEmpty(contractName, "contractName");
            Requires.IsInMembertypeSet(exportingMember.MemberType, "exportingMember", MemberTypes.Field | MemberTypes.Property | MemberTypes.NestedType | MemberTypes.TypeInfo | MemberTypes.Method);
            Contract.Ensures(Contract.Result<ExportDefinition>() != null);

            return new ReflectionMemberExportDefinition(
                exportingMember,
                new LazyExportDefinition(contractName, metadata),
                origin);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ContractBasedImportDefinition CreateImportDefinition(
            LazyMemberInfo importingMember,
            string contractName,
            string requiredTypeIdentity,
            IEnumerable<KeyValuePair<string, Type>> requiredMetadata,
            ImportCardinality cardinality,
            bool isRecomposable,
            CreationPolicy requiredCreationPolicy,
            ICompositionElement origin)
        {
            return CreateImportDefinition(importingMember, contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable, requiredCreationPolicy, MetadataServices.EmptyMetadata, false, origin);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ContractBasedImportDefinition CreateImportDefinition(
            LazyMemberInfo importingMember,
            string contractName,
            string requiredTypeIdentity,
            IEnumerable<KeyValuePair<string, Type>> requiredMetadata,
            ImportCardinality cardinality,
            bool isRecomposable,
            CreationPolicy requiredCreationPolicy,
            IDictionary<string, object> metadata,
            bool isExportFactory,
            ICompositionElement origin)
        {
            return CreateImportDefinition(
                importingMember,
                contractName,
                requiredTypeIdentity,
                requiredMetadata,
                cardinality,
                isRecomposable,
                false,
                requiredCreationPolicy,
                metadata,
                isExportFactory,
                origin);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ContractBasedImportDefinition CreateImportDefinition(
            LazyMemberInfo importingMember,
            string contractName,
            string requiredTypeIdentity,
            IEnumerable<KeyValuePair<string, Type>> requiredMetadata,
            ImportCardinality cardinality,
            bool isRecomposable,
            bool isPreRequisite,
            CreationPolicy requiredCreationPolicy,
            IDictionary<string, object> metadata,
            bool isExportFactory,
            ICompositionElement origin)
        {
            Requires.NotNullOrEmpty(contractName, "contractName");
            Requires.IsInMembertypeSet(importingMember.MemberType, "importingMember", MemberTypes.Property | MemberTypes.Field);
            Contract.Ensures(Contract.Result<ContractBasedImportDefinition>() != null);

            if (isExportFactory)
            {
                return new PartCreatorMemberImportDefinition(
                    importingMember,
                    origin,
                    new ContractBasedImportDefinition(
                        contractName,
                        requiredTypeIdentity,
                        requiredMetadata,
                        cardinality,
                        isRecomposable,
                        isPreRequisite,
                        CreationPolicy.NonShared,
                        metadata));
            }
            else
            {
                return new ReflectionMemberImportDefinition(
                    importingMember,
                    contractName,
                    requiredTypeIdentity,
                    requiredMetadata,
                    cardinality,
                    isRecomposable,
                    isPreRequisite,
                    requiredCreationPolicy,
                    metadata,
                    origin);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ContractBasedImportDefinition CreateImportDefinition(
            Lazy<ParameterInfo> parameter,
            string contractName,
            string requiredTypeIdentity,
            IEnumerable<KeyValuePair<string, Type>> requiredMetadata,
            ImportCardinality cardinality,
            CreationPolicy requiredCreationPolicy,
            ICompositionElement origin)
        {
            return CreateImportDefinition(parameter, contractName, requiredTypeIdentity, requiredMetadata, cardinality, requiredCreationPolicy, MetadataServices.EmptyMetadata, false, origin);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static ContractBasedImportDefinition CreateImportDefinition(
            Lazy<ParameterInfo> parameter,
            string contractName,
            string requiredTypeIdentity,
            IEnumerable<KeyValuePair<string, Type>> requiredMetadata,
            ImportCardinality cardinality,
            CreationPolicy requiredCreationPolicy,
            IDictionary<string, object> metadata,
            bool isExportFactory,
            ICompositionElement origin)
        {
            Requires.NotNull(parameter, "parameter");
            Requires.NotNullOrEmpty(contractName, "contractName");
            Contract.Ensures(Contract.Result<ContractBasedImportDefinition>() != null);

            if (isExportFactory)
            {
                return new PartCreatorParameterImportDefinition(
                    parameter,
                    origin,
                    new ContractBasedImportDefinition(
                        contractName,
                        requiredTypeIdentity,
                        requiredMetadata,
                        cardinality,
                        false,
                        true,
                        CreationPolicy.NonShared,
                        metadata));
            }
            else
            {
                return new ReflectionParameterImportDefinition(
                    parameter,
                    contractName,
                    requiredTypeIdentity,
                    requiredMetadata,
                    cardinality,
                    requiredCreationPolicy,
                    metadata,
                    origin);
            }
        }

        public static bool TryMakeGenericPartDefinition(ComposablePartDefinition partDefinition, IEnumerable<Type> genericParameters, out ComposablePartDefinition specialization)
        {
            Requires.NotNull(partDefinition, "partDefinition");

            specialization = null;
            ReflectionComposablePartDefinition reflectionPartDefinition = partDefinition as ReflectionComposablePartDefinition;
            if (reflectionPartDefinition == null)
            {
                throw ExceptionBuilder.CreateReflectionModelInvalidPartDefinition("partDefinition", partDefinition.GetType());
            }

            return reflectionPartDefinition.TryMakeGenericPartDefinition(genericParameters.ToArray(), out specialization);
        }
    }



    internal class ReflectionPartCreationInfo : IReflectionPartCreationInfo
    {
        private readonly Lazy<Type> _partType;
        private readonly Lazy<IEnumerable<ImportDefinition>> _imports;
        private readonly Lazy<IEnumerable<ExportDefinition>> _exports;
        private readonly Lazy<IDictionary<string, object>> _metadata;
        private readonly ICompositionElement _origin;
        private ConstructorInfo _constructor;
        private bool _isDisposalRequired;

        public ReflectionPartCreationInfo(
            Lazy<Type> partType,
            bool isDisposalRequired,
            Lazy<IEnumerable<ImportDefinition>> imports,
            Lazy<IEnumerable<ExportDefinition>> exports,
            Lazy<IDictionary<string, object>> metadata,
            ICompositionElement origin)
        {
            Assumes.NotNull(partType);

            this._partType = partType;
            this._isDisposalRequired = isDisposalRequired;
            this._imports = imports;
            this._exports = exports;
            this._metadata = metadata;
            this._origin = origin;
        }

        public Type GetPartType()
        {
            return this._partType.GetNotNullValue("type");
        }

        public Lazy<Type> GetLazyPartType()
        {
            return this._partType;
        }

        public ConstructorInfo GetConstructor()
        {
            if (this._constructor == null)
            {
                ConstructorInfo[] constructors = null;
                constructors = this.GetImports()
                    .OfType<ReflectionParameterImportDefinition>()
                    .Select(parameterImport => parameterImport.ImportingLazyParameter.Value.Member)
                    .OfType<ConstructorInfo>()
                    .Distinct()
                    .ToArray();

                if (constructors.Length == 1)
                {
                    this._constructor = constructors[0];
                }
                else if (constructors.Length == 0)
                {
                    this._constructor = this.GetPartType().GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                }
            }
            return this._constructor;
        }

        public bool IsDisposalRequired
        {
            get
            {
                return this._isDisposalRequired;
            }
        }

        public IDictionary<string, object> GetMetadata()
        {
            return (this._metadata != null) ? this._metadata.Value : null;
        }

        public IEnumerable<ExportDefinition> GetExports()
        {
            if (this._exports == null)
            {
                yield break;
            }

            IEnumerable<ExportDefinition> exports = this._exports.Value;

            if (exports == null)
            {
                yield break;
            }

            foreach (ExportDefinition export in exports)
            {
                ReflectionMemberExportDefinition reflectionExport = export as ReflectionMemberExportDefinition;
                if (reflectionExport == null)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidExportDefinition, export.GetType()));
                }
                yield return reflectionExport;
            }
        }

        public IEnumerable<ImportDefinition> GetImports()
        {
            if (this._imports == null)
            {
                yield break;
            }

            IEnumerable<ImportDefinition> imports = this._imports.Value;

            if (imports == null)
            {
                yield break;
            }

            foreach (ImportDefinition import in imports)
            {
                ReflectionImportDefinition reflectionImport = import as ReflectionImportDefinition;
                if (reflectionImport == null)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidMemberImportDefinition, import.GetType()));
                }
                yield return reflectionImport;
            }
        }

        public string DisplayName
        {
            get { return this.GetPartType().GetDisplayName(); }
        }

        public ICompositionElement Origin
        {
            get { return this._origin; }
        }
    }

    internal class LazyExportDefinition : ExportDefinition
    {
        private readonly Lazy<IDictionary<string, object>> _metadata;

        public LazyExportDefinition(string contractName, Lazy<IDictionary<string, object>> metadata)
            : base(contractName, (IDictionary<string, object>)null)
        {
            this._metadata = metadata;
        }

        public override IDictionary<string, object> Metadata
        {
            get
            {
                return this._metadata.Value ?? MetadataServices.EmptyMetadata;
            }
        }
    }
}
