// These suppressions were added when FxCop was re-enabled during Dev11

using System.Diagnostics.CodeAnalysis;

#region Suppressions for Previously Shipped APIs

[module: SuppressMessage("Microsoft.Naming","CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="MultiPart", Scope="member", Target="System.Web.Configuration.AsyncPreloadModeFlags.#FormMultiPart", Justification="This API previously shipped; modification would be a breaking change.")]
[module: SuppressMessage("Microsoft.Design","CA1056:UriPropertiesShouldNotBeStrings", Scope="member", Target="System.Web.UnvalidatedRequestValues.#RawUrl", Justification="This API previously shipped; modification would be a breaking change.")]
[module: SuppressMessage("Microsoft.Design","CA1056:UriPropertiesShouldNotBeStrings", Scope="member", Target="System.Web.UnvalidatedRequestValuesBase.#RawUrl", Justification="This API previously shipped; modification would be a breaking change.")]

#endregion



#region Security Suppressions - To Be Reviewed

[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.UnsafeNativeMethods.#StartPrefetchActivity(System.UInt32)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.Hosting.UnsafeIISMethods.#MgdCreateNativeConfigSystem(System.IntPtr&)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.Hosting.UnsafeIISMethods.#MgdExecuteUrl(System.IntPtr,System.String,System.Boolean,System.Boolean,System.Byte[],System.UInt32,System.String,System.Int32,System.String[],System.String[],System.Boolean)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.Hosting.UnsafeIISMethods.#MgdExplicitFlush(System.IntPtr,System.Boolean,System.Boolean&)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.Hosting.UnsafeIISMethods.#MgdGetModuleCollection(System.IntPtr,System.IntPtr,System.IntPtr&,System.Int32&)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.Hosting.UnsafeIISMethods.#MgdGetSiteNameFromId(System.IntPtr,System.UInt32,System.IntPtr&,System.Int32&)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Scope="member", Target="System.Web.Compilation.MultiTargetingUtil.#IsSupportedVersion(System.Runtime.Versioning.FrameworkName)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Scope="member", Target="System.Web.Hosting.IIS7WorkerRequest.#ScheduleExecuteUrl(System.String,System.String,System.String,System.Boolean,System.Byte[],System.Collections.Specialized.NameValueCollection,System.Boolean)", Justification="Needs review.")]
[module: SuppressMessage("Microsoft.Security","CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Scope="member", Target="System.Web.Hosting.UnsafeIISMethods.#MgdInsertEntityBody(System.IntPtr,System.Byte[],System.Int32,System.Int32)", Justification="Needs review.")]

#endregion



#region Low-priority items for review

// breaking
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="Databinding", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="HotSpot", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="Logon", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="MultiView", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="PlaceHolder", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="PlaceHolders", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="SideBar", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="Sitemap", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="Sitemap", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="SubMenu", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="Username", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="Usernames", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="checkbox", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="checkboxes", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="databind", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="databinding", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="dropdown", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="filename", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="newline", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="readonly", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="scrollbars", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="sitemap", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="stylesheet", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="textbox", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="textboxes", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId="username", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]

// non-breaking
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi", Scope="member", Target="System.Web.Configuration.AsyncPreloadModeFlags.#FormMultiPart", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Unvalidated", Scope="member", Target="System.Web.HttpRequest.#Unvalidated", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Unvalidated", Scope="member", Target="System.Web.HttpRequestBase.#Unvalidated", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Bufferless", Scope="member", Target="System.Web.ReadEntityBodyMode.#Bufferless", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Unvalidated", Scope="type", Target="System.Web.UnvalidatedRequestValues", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Unvalidated", Scope="type", Target="System.Web.UnvalidatedRequestValuesBase", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Unvalidated", Scope="type", Target="System.Web.UnvalidatedRequestValuesWrapper", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Prefetch", Scope="member", Target="System.Web.Configuration.CompilationSection.#EnablePrefetchOptimization", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="ACLs", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Appdomain", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Compat", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Databindings", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Databound", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Etag", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Eval", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Multi", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Nonalphanumeric", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Param", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Personalizable", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Policygt", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Querystring", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Runat", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Servergt", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Serverless", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Theming", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Util", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Viewstate", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="abbr", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="adspath", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="alg", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="appdomain", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="aspcompat", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="aspnetregsql", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="aspnetstate", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="autogenerates", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="axd", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="classname", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="databound", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="etag", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="fwlink", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="gt", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="inetsrv", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="kbid", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="linkbutton", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="mdb", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="metabase", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="msdb", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="nativerd", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="overline", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="pagesgt", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="personalizable", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="placholder", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="postback", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="postbacks", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="precompilation", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="px", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="resourcekey", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="runat", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="servername", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="src", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="tablename", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="tablesection", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="tcpip", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="themeable", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="theming", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="unselectable", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="validators", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="viewstate", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="webresource", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="windir", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Naming","CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId="Bufferless", Scope="resource", Target="System.Web.resources", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Reliability","CA2006:UseSafeHandleToEncapsulateNativeResources", Scope="member", Target="System.Web.Configuration.NativeConfig.#_nativeConfig", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Usage","CA2237:MarkISerializableTypesWithSerializable", Scope="type", Target="System.Web.Security.PassportPrincipal", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="System.Web.UnsafeNativeMethods.EndPrefetchActivity(System.UInt32)", Scope="member", Target="System.Web.HttpApplication.#ReleaseAppInstance()", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Usage","CA2213:DisposableFieldsShouldBeDisposed", MessageId="_rawContent", Scope="member", Target="System.Web.HttpBufferlessInputStream.#Dispose(System.Boolean)", Justification="Baselined from previous FxCop.")]
[module: SuppressMessage("Microsoft.Usage","CA1806:DoNotIgnoreMethodResults", MessageId="System.Web.UnsafeNativeMethods.StartPrefetchActivity(System.UInt32)", Scope="member", Target="System.Web.HttpRuntime.#HostingInit(System.Web.Hosting.HostingEnvironmentFlags,System.Security.Policy.PolicyLevel,System.Exception)", Justification="Baselined from previous FxCop.")]

#endregion