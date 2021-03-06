// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Validation;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ComplexModelDtoResultTest
    {
        [Fact]
        public void Constructor_ThrowsIfValidationNodeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new ComplexModelDtoResult("some string", null),
                "validationNode");
        }

        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            ModelValidationNode validationNode = GetValidationNode();

            // Act
            ComplexModelDtoResult result = new ComplexModelDtoResult("some string", validationNode);

            // Assert
            Assert.Equal("some string", result.Model);
            Assert.Equal(validationNode, result.ValidationNode);
        }

        private static ModelValidationNode GetValidationNode()
        {
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = provider.GetMetadataForType(null, typeof(object));
            return new ModelValidationNode(metadata, "someKey");
        }
    }
}
