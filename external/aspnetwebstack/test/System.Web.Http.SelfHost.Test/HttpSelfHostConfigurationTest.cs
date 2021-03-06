// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web.Http.SelfHost.Channels;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.SelfHost
{
    public class HttpSelfHostConfigurationTest
    {
        [Fact]
        public void HttpSelfHostConfiguration_NullBaseAddressString_Throws()
        {
            Assert.ThrowsArgumentNull(() => new HttpSelfHostConfiguration((string)null), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_RelativeBaseAddressString_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration("relative"), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_QueryBaseAddressString_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration("http://localhost?somequery"), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_FragmentBaseAddressString_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration("http://localhost#somefragment"), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_InvalidSchemeBaseAddressString_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration("ftp://localhost"), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_NullBaseAddress_Throws()
        {
            Assert.ThrowsArgumentNull(() => new HttpSelfHostConfiguration((Uri)null), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_RelativeBaseAddress_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration(new Uri("relative", UriKind.Relative)), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_QueryBaseAddress_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration(new Uri("http://localhost?somequery")), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_FragmentBaseAddress_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration(new Uri("http://localhost#somefragment")), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_InvalidSchemeBaseAddress_Throws()
        {
            Assert.ThrowsArgument(() => new HttpSelfHostConfiguration(new Uri("ftp://localhost")), "baseAddress");
        }

        [Fact]
        public void HttpSelfHostConfiguration_BaseAddress_IsSet()
        {
            // Arrange
            Uri baseAddress = new Uri("http://localhost");

            // Act
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);

            // Assert
            Assert.Same(baseAddress, config.BaseAddress);
        }

        [Fact]
        public void HttpSelfHostConfiguration_MaxConcurrentRequests_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.MaxConcurrentRequests,
                expectedDefaultValue: GetDefaultMaxConcurrentRequests(),
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 10);
        }

        [Fact]
        public void HttpSelfHostConfiguration_MaxBufferSize_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.MaxBufferSize,
                expectedDefaultValue: 64 * 1024,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 10);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1024, 1024)]
        [InlineData(Int32.MaxValue - 1, Int32.MaxValue - 1)]
        [InlineData(Int32.MaxValue, Int32.MaxValue)]
        [InlineData(Int64.MaxValue - 1, Int32.MaxValue)]
        [InlineData(Int64.MaxValue, Int32.MaxValue)]
        public void HttpSelfHostConfiguration_MaxBufferSize_TracksMaxReceivedMessageSizeWhenNotSet(long maxReceivedMessageSize, int expectedMaxBufferSize)
        {
            // Arrange
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost");
            config.MaxReceivedMessageSize = maxReceivedMessageSize;

            // Act & Assert
            Assert.Equal(expectedMaxBufferSize, config.MaxBufferSize);
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(1025, 1024)]
        [InlineData(Int64.MaxValue, Int32.MaxValue)]
        public void HttpSelfHostConfiguration_MaxBufferSize_DoesNotTrackMaxReceivedMessageSizeWhenSet(long maxReceivedMessageSize, int maxBufferSize)
        {
            // Arrange
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost");
            config.MaxBufferSize = maxBufferSize;
            config.MaxReceivedMessageSize = maxReceivedMessageSize;

            // Act & Assert
            Assert.Equal(maxBufferSize, config.MaxBufferSize);
            Assert.Equal(maxReceivedMessageSize, config.MaxReceivedMessageSize);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1024)]
        [InlineData(Int64.MaxValue)]
        public void HttpSelfHostConfiguration_MaxBufferSize_DoesNotTrackMaxReceivedMessageIfNotBuffered(long maxReceivedMessageSize)
        {
            // Arrange
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost");
            config.TransferMode = TransferMode.Streamed;
            config.MaxReceivedMessageSize = maxReceivedMessageSize;

            // Act & Assert
            Assert.Equal(maxReceivedMessageSize, config.MaxReceivedMessageSize);
            Assert.Equal(64 * 1024, config.MaxBufferSize);
        }

        [Fact]
        public void HttpSelfHostConfiguration_MaxReceivedMessageSize_RoundTrips()
        {
            Assert.Reflection.IntegerProperty(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.MaxReceivedMessageSize,
                expectedDefaultValue: 64 * 1024,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 10);
        }

        [Fact]
        public void HttpSelfHostConfiguration_UseWindowsAuthentication_RoundTrips()
        {
            Assert.Reflection.BooleanProperty(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.UseWindowsAuthentication,
                expectedDefaultValue: false);
        }

        [Fact]
        public void HttpSelfHostConfiguration_UserNamePasswordValidator_RoundTrips()
        {
            // Arrange
            UserNamePasswordValidator userNamePasswordValidator = new Mock<UserNamePasswordValidator>().Object;

            Assert.Reflection.Property(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.UserNamePasswordValidator,
                expectedDefaultValue: null,
                allowNull: true,
                roundTripTestValue: userNamePasswordValidator);
        }

        [Fact]
        public void HttpSelfHostConfiguration_TransferMode_RoundTrips()
        {
            Assert.Reflection.EnumProperty(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.TransferMode,
                expectedDefaultValue: TransferMode.Buffered,
                illegalValue: (TransferMode)999,
                roundTripTestValue: TransferMode.Streamed);
        }

        [Fact]
        public void HttpSelfHostConfiguration_HostNameComparisonMode_RoundTrips()
        {
            Assert.Reflection.EnumProperty(
                new HttpSelfHostConfiguration("http://localhost"),
                c => c.HostNameComparisonMode,
                expectedDefaultValue: HostNameComparisonMode.StrongWildcard,
                illegalValue: (HostNameComparisonMode)999,
                roundTripTestValue: HostNameComparisonMode.Exact);
        }

        [Fact]
        public void HttpSelfHostConfiguration_Settings_PropagateToBinding()
        {
            // Arrange
            HttpBinding binding = new HttpBinding();
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost")
            {
                MaxBufferSize = 10,
                MaxReceivedMessageSize = 11,
                TransferMode = TransferMode.StreamedResponse,
                HostNameComparisonMode = HostNameComparisonMode.WeakWildcard
            };

            // Act
            config.ConfigureBinding(binding);

            // Assert
            Assert.Equal(10, binding.MaxBufferSize);
            Assert.Equal(11, binding.MaxReceivedMessageSize);
            Assert.Equal(TransferMode.StreamedResponse, binding.TransferMode);
            Assert.Equal(HostNameComparisonMode.WeakWildcard, binding.HostNameComparisonMode);
        }

        [Theory]
        [InlineData("http://localhost", HttpBindingSecurityMode.TransportCredentialOnly)]
        [InlineData("https://localhost", HttpBindingSecurityMode.Transport)]
        public void HttpSelfHostConfiguration_UseWindowsAuth_PropagatesToHttpBinding(string address, HttpBindingSecurityMode mode)
        {
            // Arrange
            HttpBinding binding = new HttpBinding();
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(address)
            {
                UseWindowsAuthentication = true
            };

            // Act
            BindingParameterCollection parameters = config.ConfigureBinding(binding);

            // Assert
            Assert.NotNull(parameters);
            ServiceCredentials serviceCredentials = parameters.Find<ServiceCredentials>();
            Assert.NotNull(serviceCredentials);
            Assert.Equal(HttpClientCredentialType.Windows, binding.Security.Transport.ClientCredentialType);
            Assert.Equal(mode, binding.Security.Mode);
        }

        [Theory]
        [InlineData("http://localhost", HttpBindingSecurityMode.TransportCredentialOnly)]
        [InlineData("https://localhost", HttpBindingSecurityMode.Transport)]
        public void HttpSelfHostConfiguration_UserNamePasswordValidator_PropagatesToBinding(string address, HttpBindingSecurityMode mode)
        {
            // Arrange
            HttpBinding binding = new HttpBinding();
            UserNamePasswordValidator validator = new Mock<UserNamePasswordValidator>().Object;
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(address)
            {
                UserNamePasswordValidator = validator
            };

            // Act
            BindingParameterCollection parameters = config.ConfigureBinding(binding);

            // Assert
            Assert.NotNull(parameters);
            ServiceCredentials serviceCredentials = parameters.Find<ServiceCredentials>();
            Assert.NotNull(serviceCredentials);
            Assert.Equal(HttpClientCredentialType.Basic, binding.Security.Transport.ClientCredentialType);
            Assert.Equal(mode, binding.Security.Mode);
        }

        private static int GetDefaultMaxConcurrentRequests()
        {
            try
            {
                return Math.Max(Environment.ProcessorCount * 100, 100);
            }
            catch
            {
                return 100;
            }
        }
    }
}
