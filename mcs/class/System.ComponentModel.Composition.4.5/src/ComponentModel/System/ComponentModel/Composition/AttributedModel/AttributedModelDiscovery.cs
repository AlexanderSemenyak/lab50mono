// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Diagnostics;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.AttributedModel
{
    internal static class AttributedModelDiscovery
    {
        public static ComposablePartDefinition CreatePartDefinitionIfDiscoverable(Type type, ICompositionElement origin)
        {
            AttributedPartCreationInfo creationInfo = new AttributedPartCreationInfo(type, null, false, origin);
            if (!creationInfo.IsPartDiscoverable())
            {
                return null;
            }

            return new ReflectionComposablePartDefinition(creationInfo);
        }

        public static ReflectionComposablePartDefinition CreatePartDefinition(Type type, PartCreationPolicyAttribute partCreationPolicy, bool ignoreConstructorImports, ICompositionElement origin)
        {
            Assumes.NotNull(type);

            AttributedPartCreationInfo creationInfo = new AttributedPartCreationInfo(type, partCreationPolicy, ignoreConstructorImports, origin);

            return new ReflectionComposablePartDefinition(creationInfo);
        }

        public static ReflectionComposablePart CreatePart(object attributedPart)
        {
            Assumes.NotNull(attributedPart);

            // If given an instance then we want to pass the default composition options because we treat it as a shared part
            // TODO: ICompositionElement Give this def an origin indicating that it was added directly to the ComposablePartExportProvider.

            ReflectionComposablePartDefinition definition = AttributedModelDiscovery.CreatePartDefinition(attributedPart.GetType(), PartCreationPolicyAttribute.Shared, true, (ICompositionElement)null);

            return new ReflectionComposablePart(definition, attributedPart);
        }

#if FEATURE_REFLECTIONCONTEXT
        public static ReflectionComposablePart CreatePart(object attributedPart, ReflectionContext reflectionContext)
        {
            Assumes.NotNull(attributedPart);
            Assumes.NotNull(reflectionContext);

            // If given an instance then we want to pass the default composition options because we treat it as a shared part
            // TODO: ICompositionElement Give this def an origin indicating that it was added directly to the ComposablePartExportProvider.

            var mappedType = reflectionContext.MapType(IntrospectionExtensions.GetTypeInfo(attributedPart.GetType()));
            if (mappedType.Assembly.ReflectionOnly)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Argument_ReflectionContextReturnsReflectionOnlyType, "reflectionContext"), "reflectionContext");
            }

            ReflectionComposablePartDefinition definition = AttributedModelDiscovery.CreatePartDefinition(mappedType, PartCreationPolicyAttribute.Shared, true, (ICompositionElement)null);

            return CreatePart(definition, attributedPart);
        }
#endif //FEATURE_REFLECTIONCONTEXT

        public static ReflectionComposablePart CreatePart(ComposablePartDefinition partDefinition, object attributedPart)
        {
            Assumes.NotNull(partDefinition);
            Assumes.NotNull(attributedPart);

            return new ReflectionComposablePart((ReflectionComposablePartDefinition)partDefinition, attributedPart);
        }

        public static ReflectionParameterImportDefinition CreateParameterImportDefinition(ParameterInfo parameter, ICompositionElement origin)
        {
            Requires.NotNull(parameter, "parameter");

            ReflectionParameter reflectionParameter = parameter.ToReflectionParameter();
            IAttributedImport attributedImport = AttributedModelDiscovery.GetAttributedImport(reflectionParameter, parameter);
            ImportType importType = new ImportType(reflectionParameter.ReturnType, attributedImport.Cardinality);

            if (importType.IsPartCreator)
            {
                return new PartCreatorParameterImportDefinition(
                    new Lazy<ParameterInfo>(() => parameter),
                    origin,
                    new ContractBasedImportDefinition(
                        attributedImport.GetContractNameFromImport(importType),
                        attributedImport.GetTypeIdentityFromImport(importType),
                        CompositionServices.GetRequiredMetadata(importType.MetadataViewType),
                        attributedImport.Cardinality,
                        false,
                        true,
                        (attributedImport.RequiredCreationPolicy != CreationPolicy.NewScope) ? CreationPolicy.NonShared : CreationPolicy.NewScope,
                        CompositionServices.GetImportMetadata(importType, attributedImport)));
            }
            else
            {
                // A Standard import is not allowed to be marked as requiring NewScope at this time.
                if(attributedImport.RequiredCreationPolicy == CreationPolicy.NewScope)
                {
                    throw new ComposablePartException(
                        String.Format(CultureInfo.CurrentCulture,
                            Strings.InvalidPartCreationPolicyOnImport,
                            attributedImport.RequiredCreationPolicy),
                        origin);
                }
                return new ReflectionParameterImportDefinition(
                    new Lazy<ParameterInfo>(() => parameter),
                    attributedImport.GetContractNameFromImport(importType),
                    attributedImport.GetTypeIdentityFromImport(importType),
                    CompositionServices.GetRequiredMetadata(importType.MetadataViewType),
                    attributedImport.Cardinality,
                    attributedImport.RequiredCreationPolicy,
                    CompositionServices.GetImportMetadata(importType, attributedImport),
                    origin);
            }
        }

        public static ReflectionMemberImportDefinition CreateMemberImportDefinition(MemberInfo member, ICompositionElement origin)
        {
            Requires.NotNull(member, "member");

            ReflectionWritableMember reflectionMember = member.ToReflectionWritableMember();
            IAttributedImport attributedImport = AttributedModelDiscovery.GetAttributedImport(reflectionMember, member);
            ImportType importType = new ImportType(reflectionMember.ReturnType, attributedImport.Cardinality);

            if (importType.IsPartCreator)
            {
                return new PartCreatorMemberImportDefinition(
                    new LazyMemberInfo(member),
                    origin,
                    new ContractBasedImportDefinition(
                        attributedImport.GetContractNameFromImport(importType),
                        attributedImport.GetTypeIdentityFromImport(importType),
                        CompositionServices.GetRequiredMetadata(importType.MetadataViewType),
                        attributedImport.Cardinality,
                        attributedImport.AllowRecomposition,
                        false,
                        (attributedImport.RequiredCreationPolicy != CreationPolicy.NewScope) ? CreationPolicy.NonShared : CreationPolicy.NewScope,
                        CompositionServices.GetImportMetadata(importType, attributedImport)));
            }
            else
            {
                // A Standard parameter import is not allowed to be marked as requiring NewScope at this time.
                if(attributedImport.RequiredCreationPolicy == CreationPolicy.NewScope)
                {
                    throw new ComposablePartException(
                        String.Format(CultureInfo.CurrentCulture,
                            Strings.InvalidPartCreationPolicyOnImport,
                            attributedImport.RequiredCreationPolicy),
                        origin);
                }

                //Does this Import re-export the value if so, make it a rpe-requisite
                bool isPrerequisite = member.GetAttributes<ExportAttribute>().Length > 0;
                return new ReflectionMemberImportDefinition(
                    new LazyMemberInfo(member),
                    attributedImport.GetContractNameFromImport(importType),
                    attributedImport.GetTypeIdentityFromImport(importType),
                    CompositionServices.GetRequiredMetadata(importType.MetadataViewType),
                    attributedImport.Cardinality,
                    attributedImport.AllowRecomposition,
                    isPrerequisite,
                    attributedImport.RequiredCreationPolicy,
                    CompositionServices.GetImportMetadata(importType, attributedImport),
                    origin);
            }
        }

        private static IAttributedImport GetAttributedImport(ReflectionItem item, ICustomAttributeProvider attributeProvider)
        {
            IAttributedImport[] imports = attributeProvider.GetAttributes<IAttributedImport>(false);

            // For constructor parameters they may not have an ImportAttribute
            if (imports.Length == 0)
            {
                return new ImportAttribute();
            }

            if (imports.Length > 1)
            {
                CompositionTrace.MemberMarkedWithMultipleImportAndImportMany(item);
            }

            // Regardless of how many imports, always return the first one
            return imports[0];
        }
    }
}
