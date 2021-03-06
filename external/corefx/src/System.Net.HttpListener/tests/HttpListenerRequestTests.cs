// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Net.Tests
{
    public class HttpListenerRequestTests : IDisposable
    {
        private HttpListenerFactory Factory { get; }
        private Socket Client { get; }

        public HttpListenerRequestTests()
        {
            Factory = new HttpListenerFactory();
            Client = Factory.GetConnectedSocket();
        }

        public void Dispose()
        {
            Factory?.Dispose();
            Client?.Dispose();
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("Accept: Test", new string[] { "Test" })]
        [InlineData("Accept: Test, Test2,Test3 ,  Test4", new string[] { "Test", "Test2", "Test3 ", " Test4" })]
        [InlineData("Accept: Test", new string[] { "Test" })]
        [InlineData("Accept: ", new string[] { "" })]
        [InlineData("Unknown-Header: ", null)]
        public async Task AcceptTypes_GetProperty_ReturnsExpected(string acceptString, string[] expected)
        {
            HttpListenerRequest request = await GetRequest("POST", "", new string[] { acceptString });
            Assert.Equal(request.AcceptTypes, request.AcceptTypes);
            if (expected != null)
            {
                Assert.NotSame(request.AcceptTypes, request.AcceptTypes);
            }
            Assert.Equal(expected, request.AcceptTypes);
        }

        public static IEnumerable<object[]> ContentEncoding_TestData()
        {
            // User-Agent and x-up-devcap-post-charset.
            yield return new object[] { "User-Agent: UPTest\r\nx-up-devcap-post-charset:unicode", Encoding.Unicode };
            yield return new object[] { "User-Agent: UPTest\r\nx-up-devcap-post-charset: unicode", Encoding.Unicode };
            yield return new object[] { "User-Agent: UPTest\r\nx-up-devcap-post-charset:" + Encoding.Unicode.CodePage, Encoding.Default };
            yield return new object[] { "User-Agent: UPTest\r\nx-up-devcap-post-charset:NoSuchEncoding", Encoding.Default };
            yield return new object[] { "User-Agent: UPTest\r\nx-up-devcap-post-charset:", Encoding.Default };
            yield return new object[] { "User-Agent: UPTest", Encoding.Default };
            yield return new object[] { "User-Agent: Test", Encoding.Default };

            // Prefers x-up-devcap-post-charset to Content-Type
            yield return new object[] { "User-Agent: UPTest\r\nx-up-devcap-post-charset:unicode\r\nContent-Type:application/json;charset=utf-8", Encoding.Unicode };

            // Content-Type
            yield return new object[] { "Content-Type:application/json;charset=unicode", Encoding.Unicode };
            yield return new object[] { "Content-Type:application.json,charset=unicode", Encoding.Unicode };
            yield return new object[] { "content-type:application/json;CHARSET=UNICODE", Encoding.Unicode };
            yield return new object[] { "Content-Type:application/json  ;  charset         =   unicode", Encoding.Unicode };
            yield return new object[] { "Content-Type:application.json;charset=\"unicode\"", Encoding.Unicode };
            yield return new object[] { "Content-Type:charset;charset=unicode", Encoding.Unicode };
            yield return new object[] { "Content-Type:;charset=unicode", Encoding.Unicode };
            yield return new object[] { "Content-Type:charset=unicode", Encoding.Default };

            yield return new object[] { "Content-Type:", Encoding.Default };
            yield return new object[] { "Content-Type:;", Encoding.Default };
            yield return new object[] { "Content-Type:application/json", Encoding.Default };
            yield return new object[] { "Content-Type:application/json;", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset  ", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset;", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset=", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset=\"", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset=,", Encoding.Default };
            yield return new object[] { "Content-Type:application.json;charset=\"unicode", Encoding.Default };
            yield return new object[] { "Content-Type:application/json;charset=NoSuchEncoding", Encoding.Default };
            yield return new object[] { "Content-Type:application/json;charset=unicode; boundary=something", Encoding.Default };

            yield return new object[] { "Unknown-Header: Test", Encoding.Default };
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [MemberData(nameof(ContentEncoding_TestData))]
        public async Task ContentEncoding_GetProperty_ReturnsExpected(string header, Encoding expected)
        {
            HttpListenerRequest request = await GetRequest("POST", "", new string[] { header });
            Assert.Equal(expected, request.ContentEncoding);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task ContentEncoding_NoBody_ReturnsDefault()
        {
            HttpListenerRequest request = await GetRequest("POST", "", new string[] { "Content-Length: 0", "Content-Type:application/json;charset=unicode" }, content: null);
            Assert.Equal(Encoding.Default, request.ContentEncoding);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("POST", "Content-Length: 9223372036854775807", 9223372036854775807, true)] // long.MaxValue
        [InlineData("POST", "Content-Length: 9223372036854775808", 0, false)] // long.MaxValue + 1
        [InlineData("POST", "Content-Length: 18446744073709551615 ", 0, false)] // ulong.MaxValue
        [InlineData("POST", "Content-Length: 0", 0, false)]
        [InlineData("PUT", "Content-Length: 0", 0, false)]
        [InlineData("PUT", "Content-Length: 1", 1, true)]
        [InlineData("PUT", "Content-Length: 1\nContent-Length: 1", 1, true)]
        [InlineData("POST", "Transfer-Encoding: chunked", -1, true)]
        [InlineData("PUT", "Transfer-Encoding: chunked", -1, true)]
        [InlineData("PUT", "Transfer-Encoding: chunked", -1, true)]
        [InlineData("PUT", "Content-Length: 10\nTransfer-Encoding: chunked", -1, true)]
        [InlineData("PUT", "Transfer-Encoding: chunked\nContent-Length: 10", -1, true)]
        public async Task ContentLength_GetProperty_ReturnsExpected(string method, string contentLengthString, long expected, bool hasEntityBody)
        {
            HttpListenerRequest request = await GetRequest(method, "", contentLengthString.Split('\n'), content: "\r\n");
            Assert.Equal(expected, request.ContentLength64);
            Assert.Equal(hasEntityBody, request.HasEntityBody);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData(100)]
        [InlineData("-100")]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("9223372036854775808")]
        [ActiveIssue(20294, TargetFrameworkMonikers.Netcoreapp)]
        public async Task ContentLength_ManuallySetInHeaders_ReturnsExpected(string newValue)
        {
            HttpListenerRequest request = await GetRequest("POST", null, new string[] { "Content-Length: 1" }, content: "\r\n");
            Assert.Equal("1", request.Headers["Content-Length"]);

            request.Headers.Set("Content-Length", newValue);
            Assert.Equal(newValue, request.Headers["Content-Length"]);
            Assert.Equal(1, request.ContentLength64);

            Assert.True(request.HasEntityBody);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [ActiveIssue(20294, TargetFrameworkMonikers.Netcoreapp)]
        public async Task ContentLength_ManuallyRemovedFromHeaders_DoesNotAffect()
        {
            HttpListenerRequest request = await GetRequest("POST", null, new string[] { "Content-Length: 1" }, content: "\r\n");
            Assert.Equal("1", request.Headers["Content-Length"]);

            request.Headers.Remove("Content-Length");
            Assert.Equal(1, request.ContentLength64);

            Assert.True(request.HasEntityBody);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task ContentLength_SetInHeadersAfterAccessingProperty_DoesNothing()
        {
            HttpListenerRequest request = await GetRequest("POST", null, new string[] { "Content-Length: 1" }, content: "\r\n");

            Assert.Equal("1", request.Headers["Content-Length"]);
            Assert.Equal(1, request.ContentLength64);

            request.Headers.Set("Content-Length", "1000");
            Assert.Equal(1, request.ContentLength64);

            Assert.True(request.HasEntityBody);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("Referer: http://microsoft.com", "http://microsoft.com/")]
        [InlineData("referer: /relativePath", "/relativePath")]
        [InlineData("Referer: NoSuchSite", "NoSuchSite")]
        [InlineData("Referer: ", "")]
        [InlineData("Referer: http://microsoft.com<>", null)]
        [InlineData("Unknown-Header: ", null)]
        public async Task Referer_GetProperty_ReturnsExpected(string refererString, string expected)
        {
            HttpListenerRequest request = await GetRequest("POST", "", new string[] { refererString });
            Assert.Equal(expected, request.UrlReferrer?.ToString());
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("User-Agent: Test", "Test")]
        [InlineData("user-agent: Test", "Test")]
        [InlineData("User-Agent: ", "")]
        [InlineData("Unknown-Header: Test", null)]
        public async Task UserAgent_GetProperty_ReturnsExpected(string userAgentString, string expected)
        {
            HttpListenerRequest request = await GetRequest("POST", "", new string[] { userAgentString });
            Assert.Equal(expected, request.UserAgent);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task UserHostName_GetProperty_ReturnsExpected()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Equal("localhost", request.UserHostName);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task EndPointProperties_GetProperty_ReturnsExpected()
        {
            HttpListenerRequest request = await GetRequest("POST", "", null);
            Assert.Equal(Client.RemoteEndPoint.ToString(), request.UserHostAddress);

            Assert.Equal(Client.RemoteEndPoint, request.LocalEndPoint);
            Assert.Same(request.LocalEndPoint, request.LocalEndPoint);

            Assert.Equal(Client.LocalEndPoint, request.RemoteEndPoint);
            Assert.Same(request.RemoteEndPoint, request.RemoteEndPoint);

            Assert.Equal(Factory.ListeningUrl, request.Url.ToString());
            Assert.Same(request.Url, request.Url);

            Assert.Equal($"/{Factory.Path}/", request.RawUrl);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task ServiceName_GetNoSpn_ReturnsExpected()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Null(request.ServiceName);
        }
        
        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task RequestTraceIdentifier_GetWindows_ReturnsExpected()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.NotEqual(Guid.Empty, request.RequestTraceIdentifier);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("Connection: ", false)]
        [InlineData("Connection: Connection\r\nUpgrade: ", false)]
        [InlineData("Connection: Test1\r\nUpgrade: Test2", false)]
        [InlineData("Connection: Upgrade\r\nUpgrade: Test2", false)]
        [InlineData("Connection: Upgrade\r\nUpgrade: websocket", true)]
        [InlineData("Unknown-Header: Test", false)]
        public async Task IsWebSocketRequest_GetProperty_ReturnsExpected(string webSocketString, bool expected)
        {
            if (PlatformDetection.IsWindows7)
            {
                return;
            }

            HttpListenerRequest request = await GetRequest("POST", "", new string[] { webSocketString });
            Assert.Equal(expected, request.IsWebSocketRequest);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("Accept-Language: Lang1,Lang2,Lang3", new string[] { "Lang1", "Lang2", "Lang3" })]
        [InlineData("Accept-Language: Lang1, Lang2, Lang3", new string[] { "Lang1", "Lang2", "Lang3" })]
        [InlineData("Accept-Language: Lang1,,Lang3", new string[] { "Lang1", "", "Lang3" })]
        [InlineData("Accept-Language: Lang1,  Lang2  ,  Lang3  ", new string[] { "Lang1", " Lang2  ", " Lang3" })]
        [InlineData("Accept-Language:", new string[] { "" })]
        [InlineData("Accept-Language: ", new string[] { "" })]
        [InlineData("Unknown-Header: Test", null)]
        public async Task UserLanguages_GetProperty_ReturnsExpected(string userLanguageString, string[] expected)
        {
            HttpListenerRequest request = await GetRequest("POST", "", new string[] { userLanguageString });
            Assert.Equal(request.UserLanguages, request.UserLanguages);
            if (expected != null)
            {
                Assert.NotSame(request.UserLanguages, request.UserLanguages);
            }
            Assert.Equal(expected, request.UserLanguages);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task ClientCertificateError_GetNotInitialized_ThrowsInvalidOperationException()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Throws<InvalidOperationException>(() => request.ClientCertificateError);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task GetClientCertificate_NoCertificate_ReturnsNull()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Null(request.GetClientCertificate());
            Assert.Equal(0, request.ClientCertificateError);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task GetClientCertificateAsync_NoCertificate_ReturnsNull()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Null(request.GetClientCertificateAsync().Result);
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task EndGetClientCertificate_NullAsyncResult_ThrowsArgumentException()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            AssertExtensions.Throws<ArgumentNullException>("asyncResult", () => request.EndGetClientCertificate(null));
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task EndGetClientCertificate_InvalidAsyncResult_ThrowsArgumentException()
        {
            HttpListenerRequest request1 = await GetRequest("POST", null, null);
            using (var requestTests = new HttpListenerRequestTests())
            {
                HttpListenerRequest request2 = await requestTests.GetRequest("POST", null, null);
                IAsyncResult beginGetClientCertificateResult1 = request1.BeginGetClientCertificate(null, null);

                AssertExtensions.Throws<ArgumentException>("asyncResult", () => request2.EndGetClientCertificate(new CustomAsyncResult()));
                AssertExtensions.Throws<ArgumentException>("asyncResult", () => request2.EndGetClientCertificate(beginGetClientCertificateResult1));
            }
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task EndGetClientCertificate_AlreadyCalled_ThrowsInvalidOperationException()
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            IAsyncResult beginGetClientCertificateResult = request.BeginGetClientCertificate(null, null);
            request.EndGetClientCertificate(beginGetClientCertificateResult);
                
            Assert.Throws<InvalidOperationException>(() => request.EndGetClientCertificate(beginGetClientCertificateResult));
        }

        [ConditionalFact(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        public async Task TransportContext_GetChannelBinding_ReturnsExpected()
        {
            // This might not work on other devices:
            // "The Security Service Providers don't support extended protection. Please install the
            // latest Security Service Providers update."
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Null(request.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint));
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData(ChannelBindingKind.Unique)]
        public async Task TransportContext_GetChannelBindingInvalid_ThrowsNotSupportedException(ChannelBindingKind kind)
        {
            HttpListenerRequest request = await GetRequest("POST", null, null);
            Assert.Throws<NotSupportedException>(() => request.TransportContext.GetChannelBinding(kind));
        }

        public static IEnumerable<object[]> QueryString_TestData()
        {
            yield return new object[]
            {
                "?query", new NameValueCollection
                {
                    { null, "query" }
                }
            };

            yield return new object[]
            {
                "?name=value", new NameValueCollection
                {
                    { "name", "value" }
                }
            };

            yield return new object[]
            {
                "?name=value=value2", new NameValueCollection
                {
                    { "name", "value=value2" }
                }
            };

            yield return new object[]
            {
                "?name1=value1&name2=value2&name1=value3&name3=value&name3=value", new NameValueCollection
                {
                    { "name1", "value1" },
                    { "name1", "value3" },
                    { "name2", "value2" },
                    { "name3", "value" },
                    { "name3", "value" }
                }
            };

            // Unicode queries are destroyed by HttpListener.
            // [ActiveIssue(19967, TargetFrameworkMonikers.NetFramework)]
            // 
            if (!PlatformDetection.IsFullFramework)
            {
                yield return new object[]
                {
                    "?name1=+&name2=\u1234&\u0100=value&name3=\u00FF", new NameValueCollection
                    {
                        { "name1", " " },
                        { "name2", "á\u0088´" },
                        { "Ä\u0080", "value" },
                        { "name3", "Ã¿" }
                    }
                };
            }

            yield return new object[] { "", new NameValueCollection() };
            yield return new object[] { "?", new NameValueCollection() };

            yield return new object[]
            {
                "?name=", new NameValueCollection()
                {
                    { "name", "" }
                }
            };

            yield return new object[]
            {
                "?&", new NameValueCollection()
                {
                    { null, "" },
                    { null, "" }
                }
            };

            yield return new object[]
            {
                "?&&", new NameValueCollection()
                {
                    { null, "" },
                    { null, "" },
                    { null, "" }
                }
            };
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [MemberData(nameof(QueryString_TestData))]
        public async Task QueryString_GetProperty_ReturnsExpected(string query, NameValueCollection expected)
        {
            HttpListenerRequest request = await GetRequest("POST", query, null);
            NameValueCollection queryString = request.QueryString;
            Assert.Equal(expected.Count, queryString.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected.GetKey(i), queryString.GetKey(i));
                Assert.Equal(expected.GetValues(i), queryString.GetValues(i));
            }
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("POST")]
        [InlineData("PATCH")]
        [InlineData("get")]
        [InlineData("NOSUCH")]
        public async Task HttpMethod_GetProperty_ReturnsExpected(string httpMethod)
        {
            HttpListenerRequest request = await GetRequest(httpMethod, null, null);
            Assert.Equal(httpMethod, request.HttpMethod);
            Assert.Equal(request.HttpMethod, request.HttpMethod);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("1.1", new string[] { "Proxy-Connection: random" }, true)]
        [InlineData("1.1", new string[] { "Proxy-Connection: close" }, false)]
        [InlineData("1.1", new string[] { "proxy-connection: CLOSE" }, false)]
        [InlineData("1.1", new string[] { "Proxy-Connection: keep-alive" }, true)]
        [InlineData("1.1", new string[] { "Proxy-Connection: " }, true)]
        [InlineData("1.1", new string[] { "Connection: random" }, true)]
        [InlineData("1.1", new string[] { "Connection: close" }, false)]
        [InlineData("1.1", new string[] { "connection: CLOSE" }, false)]
        [InlineData("1.1", new string[] { "Connection: keep-alive" }, true)]
        [InlineData("1.1", new string[] { "Connection: " }, true)]
        [InlineData("1.1", new string[] { "Keep-Alive: true" }, true)]
        [InlineData("1.1", new string[] { "Proxy-Connection: ", "Connection: close" }, false)]
        [InlineData("1.1", new string[] { "Proxy-Connection: ", "Connection: close", "Keep-Alive: true" }, false)]
        [InlineData("1.1", new string[] { "Connection: close", "Keep-Alive: true" }, false)]
        [InlineData("1.1", new string[] { "UnknownHeader: random" }, true)]
        [InlineData("1.0", new string[] { "Keep-Alive: true" }, true)]
        [InlineData("1.0", new string[] { "Keep-Alive: " }, false)]
        [InlineData("1.0", new string[] { "UnknownHeader: random" }, false)]
        public async Task KeepAlive_GetProperty_ReturnsExpected(string httpVersion, string[] headers, bool expected)
        {
            HttpListenerRequest request = await GetRequest("POST", "", headers, httpVersion: httpVersion);
            Assert.Equal(request.KeepAlive, request.KeepAlive);
            Assert.Equal(expected, request.KeepAlive);
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [InlineData("1.0")]
        [InlineData("1.1")]
        [InlineData("1.2")]
        [InlineData("1.3")]
        [InlineData("1.4")]
        [InlineData("1.5")]
        [InlineData("1.6")]
        [InlineData("1.7")]
        [InlineData("1.8")]
        [InlineData("1.9")]
        public async Task ProtocolVersion_GetProperty_ReturnsExpected(string httpVersion)
        {
            var version = new Version(httpVersion);

            HttpListenerRequest request = await GetRequest("POST", "", new string[0], httpVersion: httpVersion);
            Assert.Equal(version, request.ProtocolVersion);
        }

        public static IEnumerable<object[]> Cookies_TestData()
        {
            yield return new object[]
            {
                "cookie: name=value", new CookieCollection
                {
                    new Cookie("name", "value")
                }
            };

            // Not added if the cookie already exists.
            yield return new object[]
            {
                "cookie: name=value,name=value", new CookieCollection
                {
                    new Cookie("name", "value")
                }
            };

            yield return new object[]
            {
                "cookie: name=value,name=value2", new CookieCollection
                {
                    new Cookie("name", "value2")
                }
            };

            yield return new object[]
            {
                "cookie: name=value,name=value;$port=\"200\"", new CookieCollection
                {
                    new Cookie("name", "value") { Port = "\"200\"" }
                }
            };

            // Cookie with a greater variant (e.g. Rfc2109) is preferred over a lower variant (e.g. Plain).
            yield return new object[]
            {
                "cookie: name=value;$port=\"200\",name=value", new CookieCollection
                {
                    new Cookie("name", "value") { Port = "\"200\"" }
                }
            };

            yield return new object[]
            {
                "cookie: name1=value1,name2=value2;name3=value3", new CookieCollection
                {
                    new Cookie("name1", "value1"),
                    new Cookie("name2", "value2"),
                    new Cookie("name3", "value3")
                }
            };

            yield return new object[]
            {
                "cookie: name=value;$port=\"80\";$Path=path;$Domain=domain", new CookieCollection
                {
                    new Cookie("name", "value") { Port = "\"80\"", Path = "path", Domain = "domain" }
                }
            };

            yield return new object[] { "cookie: =value", new CookieCollection() };

            yield return new object[] { "cookie: $Path", new CookieCollection() };
            yield return new object[] { "cookie: $Domain", new CookieCollection() };
            yield return new object[] { "cookie: $Port", new CookieCollection() };

            yield return new object[]
            {
                "cookie:name=value; domain=.domain.com", new CookieCollection
                {
                    new Cookie("name", "value"),
                    new Cookie("domain", ".domain.com")
                }
            };
            yield return new object[]
            {
                "cookie:name=value; expires=invaliddate",
                new CookieCollection
                {
                    new Cookie("name", "value"),
                    new Cookie("expires", "invaliddate")
                }
            };

            yield return new object[] { "cookie: ", new CookieCollection() };
            yield return new object[] { "Unknown-Header: Test", new CookieCollection() };
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [MemberData(nameof(Cookies_TestData))]
        public async Task Cookies_GetProperty_ReturnsExpected(string cookieString, CookieCollection expected)
        {
            HttpListenerRequest request = await GetRequest("POST", null, new[] { cookieString });

            Assert.Equal(expected.Count, request.Cookies.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].Name, request.Cookies[i].Name);
                Assert.Equal(expected[i].Value, request.Cookies[i].Value);
                Assert.Equal(expected[i].Port, request.Cookies[i].Port);
                Assert.Equal(expected[i].Path, request.Cookies[i].Path);
                Assert.Equal(expected[i].Domain, request.Cookies[i].Domain);
            }
        }

        public static IEnumerable<object[]> Headers_TestData()
        {
            yield return new object[] { new string[] { "name:value" }, new WebHeaderCollection() { { "name", "value" } } };
            yield return new object[] { new string[] { "name:val?ue" }, new WebHeaderCollection() { { "name", "val?ue" } } };
        }

        [ConditionalTheory(nameof(Helpers) + "." + nameof(Helpers.IsNotNanoServer))]
        [MemberData(nameof(Headers_TestData))]
        public async Task Headers_Get_ReturnsExpected(string[] headers, WebHeaderCollection expected)
        {
            HttpListenerRequest request = await GetRequest("POST", null, headers);
            foreach (string name in expected)
            {
                Assert.Equal(expected[name], request.Headers[name]);
                Assert.Same(request.Headers, request.Headers);
            }
        }

        private async Task<HttpListenerRequest> GetRequest(string requestType, string query, string[] headers, string content = "Text\r\n", string httpVersion = "1.1")
        {
            Client.Send(Factory.GetContent(httpVersion, requestType, query, content, headers, true));

            HttpListener listener = Factory.GetListener();
            return (await listener.GetContextAsync()).Request;
        }
    }
}
