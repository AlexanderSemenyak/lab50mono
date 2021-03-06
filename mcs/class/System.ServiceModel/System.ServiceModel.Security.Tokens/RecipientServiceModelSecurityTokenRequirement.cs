//
// RecipientServiceModelSecurityTokenRequirement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace System.ServiceModel.Security.Tokens
{
	public sealed class RecipientServiceModelSecurityTokenRequirement
		: ServiceModelSecurityTokenRequirement
	{
		public RecipientServiceModelSecurityTokenRequirement ()
		{
		}

		public AuditLogLocation AuditLogLocation {
			get {
				AuditLogLocation ret;
				TryGetProperty<AuditLogLocation> (AuditLogLocationProperty, out ret);
				return ret;
			}
			set { Properties [AuditLogLocationProperty] = value; }
		}

		public Uri ListenUri {
			get {
				Uri ret;
				TryGetProperty<Uri> (ListenUriProperty, out ret);
				return ret;
			}
			set { Properties [ListenUriProperty] = value; }
		}

		public AuditLevel MessageAuthenticationAuditLevel {
			get {
				AuditLevel ret;
				TryGetProperty<AuditLevel> (MessageAuthenticationAuditLevelProperty, out ret);
				return ret;
			}
			set { Properties [MessageAuthenticationAuditLevelProperty] = value; }
		}

		public bool SuppressAuditFailure {
			get {
				bool ret;
				TryGetProperty<bool> (SuppressAuditFailureProperty, out ret);
				return ret;
			}
			set { Properties [SuppressAuditFailureProperty] = value; }
		}

		public override string ToString ()
		{
			return Dump ();
		}
	}
}
