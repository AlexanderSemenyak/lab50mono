// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using Xunit;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpImplicitExpressionTest : CsHtmlCodeParserTestBase
    {
        private const string TestExtraKeyword = "model";

        public override ParserBase CreateCodeParser()
        {
            return new CSharpCodeParser();
        }

        [Fact]
        public void NestedImplicitExpression()
        {
            ParseBlockTest("if (true) { @foo }",
                           new StatementBlock(
                               Factory.Code("if (true) { ").AsStatement(),
                               new ExpressionBlock(
                                   Factory.CodeTransition(),
                                   Factory.Code("foo")
                                       .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                       .Accepts(AcceptedCharacters.NonWhiteSpace)),
                               Factory.Code(" }").AsStatement()));
        }

        [Fact]
        public void ParseBlockAcceptsNonEnglishCharactersThatAreValidIdentifiers()
        {
            ImplicitExpressionTest("हळूँजद॔.", "हळूँजद॔");
        }

        [Fact]
        public void ParseBlockOutputsZeroLengthCodeSpanIfInvalidCharacterFollowsTransition()
        {
            ParseBlockTest("@/",
                           new ExpressionBlock(
                               Factory.CodeTransition(),
                               Factory.EmptyCSharp()
                                   .AsImplicitExpression(KeywordSet)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                           new RazorError(
                               String.Format(RazorResources.ParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS, "/"),
                               new SourceLocation(1, 0, 1)));
        }

        [Fact]
        public void ParseBlockOutputsZeroLengthCodeSpanIfEOFOccursAfterTransition()
        {
            ParseBlockTest("@",
                           new ExpressionBlock(
                               Factory.CodeTransition(),
                               Factory.EmptyCSharp()
                                   .AsImplicitExpression(KeywordSet)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                           new RazorError(
                               RazorResources.ParseError_Unexpected_EndOfFile_At_Start_Of_CodeBlock,
                               new SourceLocation(1, 0, 1)));
        }

        [Fact]
        public void ParseBlockSupportsSlashesWithinComplexImplicitExpressions()
        {
            ImplicitExpressionTest("DataGridColumn.Template(\"Years of Service\", e => (int)Math.Round((DateTime.Now - dt).TotalDays / 365))");
        }

        [Fact]
        public void ParseBlockMethodParsesSingleIdentifierAsImplicitExpression()
        {
            ImplicitExpressionTest("foo");
        }

        [Fact]
        public void ParseBlockMethodDoesNotAcceptSemicolonIfExpressionTerminatedByWhitespace()
        {
            ImplicitExpressionTest("foo ;", "foo");
        }

        [Fact]
        public void ParseBlockMethodIgnoresSemicolonAtEndOfSimpleImplicitExpression()
        {
            RunTrailingSemicolonTest("foo");
        }

        [Fact]
        public void ParseBlockMethodParsesDottedIdentifiersAsImplicitExpression()
        {
            ImplicitExpressionTest("foo.bar.baz");
        }

        [Fact]
        public void ParseBlockMethodIgnoresSemicolonAtEndOfDottedIdentifiers()
        {
            RunTrailingSemicolonTest("foo.bar.baz");
        }

        [Fact]
        public void ParseBlockMethodDoesNotIncludeDotAtEOFInImplicitExpression()
        {
            ImplicitExpressionTest("foo.bar.", "foo.bar");
        }

        [Fact]
        public void ParseBlockMethodDoesNotIncludeDotFollowedByInvalidIdentifierCharacterInImplicitExpression()
        {
            ImplicitExpressionTest("foo.bar.0", "foo.bar");
            ImplicitExpressionTest("foo.bar.</p>", "foo.bar");
        }

        [Fact]
        public void ParseBlockMethodDoesNotIncludeSemicolonAfterDot()
        {
            ImplicitExpressionTest("foo.bar.;", "foo.bar");
        }

        [Fact]
        public void ParseBlockMethodTerminatesAfterIdentifierUnlessFollowedByDotOrParenInImplicitExpression()
        {
            ImplicitExpressionTest("foo.bar</p>", "foo.bar");
        }

        [Fact]
        public void ParseBlockProperlyParsesParenthesesAndBalancesThemInImplicitExpression()
        {
            ImplicitExpressionTest(@"foo().bar(""bi\""z"", 4)(""chained method; call"").baz(@""bo""""z"", '\'', () => { return 4; }, (4+5+new { foo = bar[4] }))");
        }

        [Fact]
        public void ParseBlockProperlyParsesBracketsAndBalancesThemInImplicitExpression()
        {
            ImplicitExpressionTest(@"foo.bar[4 * (8 + 7)][""fo\""o""].baz");
        }

        [Fact]
        public void ParseBlockTerminatesImplicitExpressionAtHtmlEndTag()
        {
            ImplicitExpressionTest("foo().bar.baz</p>zoop", "foo().bar.baz");
        }

        [Fact]
        public void ParseBlockTerminatesImplicitExpressionAtHtmlStartTag()
        {
            ImplicitExpressionTest("foo().bar.baz<p>zoop", "foo().bar.baz");
        }

        [Fact]
        public void ParseBlockTerminatesImplicitExpressionBeforeDotIfDotNotFollowedByIdentifierStartCharacter()
        {
            ImplicitExpressionTest("foo().bar.baz.42", "foo().bar.baz");
        }

        [Fact]
        public void ParseBlockStopsBalancingParenthesesAtEOF()
        {
            ImplicitExpressionTest("foo(()", "foo(()",
                                   acceptedCharacters: AcceptedCharacters.Any,
                                   errors: new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), new SourceLocation(4, 0, 4)));
        }

        [Fact]
        public void ParseBlockTerminatesImplicitExpressionIfCloseParenFollowedByAnyWhiteSpace()
        {
            ImplicitExpressionTest("foo.bar() (baz)", "foo.bar()");
        }

        [Fact]
        public void ParseBlockTerminatesImplicitExpressionIfIdentifierFollowedByAnyWhiteSpace()
        {
            ImplicitExpressionTest("foo .bar() (baz)", "foo");
        }

        [Fact]
        public void ParseBlockTerminatesImplicitExpressionAtLastValidPointIfDotFollowedByWhitespace()
        {
            ImplicitExpressionTest("foo. bar() (baz)", "foo");
        }

        [Fact]
        public void ParseBlockOutputExpressionIfModuleTokenNotFollowedByBrace()
        {
            ImplicitExpressionTest("module.foo()");
        }

        private void RunTrailingSemicolonTest(string expr)
        {
            ParseBlockTest(SyntaxConstants.TransitionString + expr + ";",
                           new ExpressionBlock(
                               Factory.CodeTransition(),
                               Factory.Code(expr)
                                   .AsImplicitExpression(KeywordSet)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)
                               ));
        }
    }
}
