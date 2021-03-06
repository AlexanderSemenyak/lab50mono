// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Razor.Parser.SyntaxTree;

namespace System.Web.Razor
{
    /// <summary>
    /// Represents the results of parsing a Razor document
    /// </summary>
    public class ParserResults
    {
        public ParserResults(Block document, IList<RazorError> parserErrors)
            : this(parserErrors == null || parserErrors.Count == 0, document, parserErrors)
        {
        }

        protected ParserResults(bool success, Block document, IList<RazorError> errors)
        {
            Success = success;
            Document = document;
            ParserErrors = errors ?? new List<RazorError>();
        }

        /// <summary>
        /// Indicates if parsing was successful (no errors)
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The root node in the document's syntax tree
        /// </summary>
        public Block Document { get; private set; }

        /// <summary>
        /// The list of errors which occurred during parsing.
        /// </summary>
        public IList<RazorError> ParserErrors { get; private set; }
    }
}
