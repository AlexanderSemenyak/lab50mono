// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security;
using System.Security.Permissions;
namespace System.Diagnostics
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Event,
    AllowMultiple = true, Inherited = false)]
    public class PerformanceCounterPermissionAttribute : CodeAccessSecurityAttribute
    {
        public PerformanceCounterPermissionAttribute(SecurityAction action) : base(action) { }
        public string CategoryName { get { return null; } set { } }
        public string MachineName { get { return null; } set { } }
        public PerformanceCounterPermissionAccess PermissionAccess { get; set; }
        public override IPermission CreatePermission() { return null; }
    }
}
