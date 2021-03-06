// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace ILCompiler.DependencyAnalysis
{
    public partial class NodeFactory
    {
        private InitialInterfaceDispatchStubNode _initialInterfaceDispatchStubNode;

        public InitialInterfaceDispatchStubNode InitialInterfaceDispatchStub
        {
            get
            {
                if (_initialInterfaceDispatchStubNode == null)
                {
                    _initialInterfaceDispatchStubNode = new InitialInterfaceDispatchStubNode();
                }

                return _initialInterfaceDispatchStubNode;
            }
        }

    }
}
