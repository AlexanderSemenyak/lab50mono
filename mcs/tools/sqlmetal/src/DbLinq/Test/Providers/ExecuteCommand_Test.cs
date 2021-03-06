#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

using nwind;

// test ns 
#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP
#elif ORACLE
    namespace Test_NUnit_Oracle
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict
#elif MSSQL
    namespace Test_NUnit_MsSql
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#endif
{
    [TestFixture]
    public class ExecuteCommand_Test : TestBase
    {
#if !DEBUG && (MSSQL && L2SQL)
        // DataContext.ExecuteScalar() working with 'SELECT COUNT(*)' is a DbLinq extension.
        // Exclude from Linq2Sql comparison tests.
        [Explicit]
#endif
        [Test]
        public void A2_ProductsTableHasEntries()
        {
            Northwind db = CreateDB();
            int result = db.ExecuteCommand("SELECT count(*) FROM \"Products\"");
            AssertHelper.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

        /// <summary>
        /// like above, but includes one parameter.
        /// </summary>
#if !DEBUG && (MSSQL && L2SQL)
        // DataContext.ExecuteScalar() working with 'SELECT COUNT(*)' is a DbLinq extension.
        // Exclude from Linq2Sql comparison tests.
        [Explicit]
#endif
        [Test]
        public void A3_ProductCount_Param()
        {
            Northwind db = CreateDB();
            int result = db.ExecuteCommand("SELECT count(*) FROM [Products] WHERE [ProductID]>{0}", 3);
            //long iResult = base.ExecuteScalar(sql);
            AssertHelper.Greater(result, 0, "Expecting some rows in Products table, got:" + result);
        }

    }
}
