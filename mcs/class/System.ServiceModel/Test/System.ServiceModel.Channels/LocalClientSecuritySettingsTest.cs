//
// LocalClientSecuritySettingsTest.cs
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
#if !MOBILE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class LocalClientSecuritySettingsTest
	{
		[Test]
		public void DefaultValues ()
		{
			LocalClientSecuritySettings lc = new LocalClientSecuritySettings ();
			Assert.IsNotNull (lc, "#1");
			Assert.AreEqual (true, lc.CacheCookies, "#2");
			Assert.AreEqual (60, lc.CookieRenewalThresholdPercentage, "#3");
			Assert.AreEqual (true, lc.DetectReplays, "#4");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.MaxClockSkew, "#5");
			Assert.AreEqual (TimeSpan.MaxValue, lc.MaxCookieCachingTime, "#6");
			Assert.AreEqual (true, lc.ReconnectTransportOnFailure, "#7");
			Assert.AreEqual (900000, lc.ReplayCacheSize, "#8");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.ReplayWindow, "#9");
			Assert.AreEqual (TimeSpan.FromHours (10), lc.SessionKeyRenewalInterval, "#10");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.SessionKeyRolloverInterval, "#11");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.TimestampValidityDuration, "#12");
			// FIXME: IdentityVerifier
			Assert.IsNotNull (lc.IdentityVerifier, "#13");
		}
	}
}
#endif