namespace System.Web.Mvc {
    using System;
    using System.Reflection;
    using System.Web;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class HttpPutAttribute : ActionMethodSelectorAttribute {

        private static readonly AcceptVerbsAttribute _innerAttribute = new AcceptVerbsAttribute(HttpVerbs.Put);

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
            return _innerAttribute.IsValidForRequest(controllerContext, methodInfo);
        }
    }
}
