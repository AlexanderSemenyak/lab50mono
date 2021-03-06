// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Tests.System
{
    public class AssertExtensionTests
    {
        private const string Null = "<null>";

        [Theory,
            InlineData(0, 0, false),
            InlineData(1, 1, false),
            InlineData(0, 1, false),
            InlineData(0, -1, true),
            InlineData(-1, 0, false),
            InlineData(1, 0, true),
            InlineData(1, -1, true),
            InlineData(-1, 1, false)
            ]
        public void GreaterThan_ValueType(int actual, int greaterThan, bool expected)
        {
            try
            {
                AssertExtensions.GreaterThan(actual, greaterThan);
                Assert.True(expected, $"{actual} > {greaterThan} should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"{actual} > {greaterThan} should *not* have failed the assertion.");
            }
        }

        [Theory,
            // Null is always less than anything other than null
            InlineData("", null, true),
            InlineData(null, null, false),
            InlineData(null, "", false),
            InlineData("b", "b", false),
            InlineData("b", "c", false),
            InlineData("b", "a", true),
            InlineData("a", "b", false),
            InlineData("c", "b", true),
            InlineData("c", "a", true),
            InlineData("a", "c", false)
            ]
        public void GreaterThan_ReferenceType(string actual, string greaterThan, bool expected)
        {
            try
            {
                AssertExtensions.GreaterThan(actual, greaterThan);
                Assert.True(expected, $"{actual ?? Null} > {greaterThan ?? Null} should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"{actual ?? Null} > {greaterThan ?? Null} should *not* have failed the assertion.");
            }
        }

        [Theory,
            InlineData(0, 0, false),
            InlineData(1, 1, false),
            InlineData(0, 1, true),
            InlineData(0, -1, false),
            InlineData(-1, 0, true),
            InlineData(1, 0, false),
            InlineData(1, -1, false),
            InlineData(-1, 1, true)
            ]
        public void LessThan_ValueType(int actual, int lessThan, bool expected)
        {
            try
            {
                AssertExtensions.LessThan(actual, lessThan);
                Assert.True(expected, $"{actual} < {lessThan} should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"{actual} < {lessThan} should *not* have failed the assertion.");
            }
        }

        [Theory,
            // Null is always less than anything other than null
            InlineData("", null, false),
            InlineData(null, null, false),
            InlineData(null, "", true),
            InlineData("b", "b", false),
            InlineData("b", "c", true),
            InlineData("b", "a", false),
            InlineData("a", "b", true),
            InlineData("c", "b", false),
            InlineData("c", "a", false),
            InlineData("a", "c", true)
            ]
        public void LessThan_ReferenceType(string actual, string lessThan, bool expected)
        {
            try
            {
                AssertExtensions.LessThan(actual, lessThan);
                Assert.True(expected, $"'{actual ?? Null}' < '{lessThan ?? Null}' should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"'{actual ?? Null}' < '{lessThan ?? Null}' should *not* have failed the assertion.");
            }
        }

        [Theory,
            InlineData(0, 0, true),
            InlineData(1, 1, true),
            InlineData(0, 1, false),
            InlineData(0, -1, true),
            InlineData(-1, 0, false),
            InlineData(1, 0, true),
            InlineData(1, -1, true),
            InlineData(-1, 1, false)
            ]
        public void GreaterThanOrEqualTo_ValueType(int actual, int greaterThanOrEqualTo, bool expected)
        {
            try
            {
                AssertExtensions.GreaterThanOrEqualTo(actual, greaterThanOrEqualTo);
                Assert.True(expected, $"{actual} >= {greaterThanOrEqualTo} should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"{actual} >= {greaterThanOrEqualTo} should *not* have failed the assertion.");
            }
        }

        [Theory,
            // Null is always less than anything other than null
            InlineData("", null, true),
            InlineData(null, null, true),
            InlineData(null, "", false),
            InlineData("b", "b", true),
            InlineData("b", "c", false),
            InlineData("b", "a", true),
            InlineData("a", "b", false),
            InlineData("c", "b", true),
            InlineData("c", "a", true),
            InlineData("a", "c", false)
            ]
        public void GreaterThanOrEqualTo_ReferenceType(string actual, string greaterThanOrEqualTo, bool expected)
        {
            try
            {
                AssertExtensions.GreaterThanOrEqualTo(actual, greaterThanOrEqualTo);
                Assert.True(expected, $"'{actual ?? Null}' >= '{greaterThanOrEqualTo ?? Null}' should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"'{actual ?? Null}' >= '{greaterThanOrEqualTo ?? Null}' should *not* have failed the assertion.");
            }
        }

        [Theory,
            InlineData(0, 0, true),
            InlineData(1, 1, true),
            InlineData(0, 1, true),
            InlineData(0, -1, false),
            InlineData(-1, 0, true),
            InlineData(1, 0, false),
            InlineData(1, -1, false),
            InlineData(-1, 1, true)
            ]
        public void LessThanOrEqualTo_ValueType(int actual, int lessThanOrEqualTo, bool expected)
        {
            try
            {
                AssertExtensions.LessThanOrEqualTo(actual, lessThanOrEqualTo);
                Assert.True(expected, $"{actual} <= {lessThanOrEqualTo} should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"{actual} <= {lessThanOrEqualTo} should *not* have failed the assertion.");
            }
        }

        [Theory,
            // Null is always less than anything other than null
            InlineData("", null, false),
            InlineData(null, null, true),
            InlineData(null, "", true),
            InlineData("b", "b", true),
            InlineData("b", "c", true),
            InlineData("b", "a", false),
            InlineData("a", "b", true),
            InlineData("c", "b", false),
            InlineData("c", "a", false),
            InlineData("a", "c", true)
            ]
        public void LessThanOrEqualTo_ReferenceType(string actual, string lessThanOrEqualTo, bool expected)
        {
            try
            {
                AssertExtensions.LessThanOrEqualTo(actual, lessThanOrEqualTo);
                Assert.True(expected, $"'{actual ?? Null}' >= '{lessThanOrEqualTo ?? Null}' should have failed the assertion.");
            }
            catch (XunitException)
            {
                Assert.False(expected, $"'{actual ?? Null}' >= '{lessThanOrEqualTo ?? Null}' should *not* have failed the assertion.");
            }
        }
    }
}
