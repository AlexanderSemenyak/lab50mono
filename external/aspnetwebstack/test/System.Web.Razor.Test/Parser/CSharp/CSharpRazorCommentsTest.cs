// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Tokenizer.Symbols;
using Xunit;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpRazorCommentsTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void UnterminatedRazorComment()
        {
            ParseDocumentTest("@*",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            String.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any))),
                new RazorError(RazorResources.ParseError_RazorComment_Not_Terminated, 0, 0, 0));
        }

        [Fact]
        public void EmptyRazorComment()
        {
            ParseDocumentTest("@**@",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            String.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void RazorCommentInImplicitExpressionMethodCall()
        {
            ParseDocumentTest(@"@foo(
@**@
",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("foo(\r\n")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                        new CommentBlock(
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.Span(SpanKind.Comment, new CSharpSymbol(
                                Factory.LocationTracker.CurrentLocation,
                                String.Empty,
                                CSharpSymbolType.Unknown))
                                   .Accepts(AcceptedCharacters.Any),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None)),
                        Factory.Code("\r\n")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords))),
                new RazorError(
                    String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"),
                    4, 0, 4));
        }

        [Fact]
        public void UnterminatedRazorCommentInImplicitExpressionMethodCall()
        {
            ParseDocumentTest("@foo(@*",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("foo(")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                        new CommentBlock(
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.Span(SpanKind.Comment, new CSharpSymbol(
                                Factory.LocationTracker.CurrentLocation,
                                String.Empty,
                                CSharpSymbolType.Unknown))
                                    .Accepts(AcceptedCharacters.Any)))),
                new RazorError(RazorResources.ParseError_RazorComment_Not_Terminated, 5, 0, 5),
                new RazorError(String.Format(RazorResources.ParseError_Expected_CloseBracket_Before_EOF, "(", ")"), 4, 0, 4));
        }

        [Fact]
        public void RazorCommentInVerbatimBlock()
        {
            ParseDocumentTest(@"@{
    <text
    @**@
}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        Factory.Code("\r\n").AsStatement(),
                        new MarkupBlock(
                            Factory.Markup("    "),
                            Factory.MarkupTransition("<text").Accepts(AcceptedCharacters.Any),
                            Factory.Markup("\r\n    "),
                            new CommentBlock(
                                Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                                       .Accepts(AcceptedCharacters.None),
                                Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                                       .Accepts(AcceptedCharacters.None),
                                Factory.Span(SpanKind.Comment, new HtmlSymbol(
                                    Factory.LocationTracker.CurrentLocation,
                                    String.Empty,
                                    HtmlSymbolType.Unknown))
                                       .Accepts(AcceptedCharacters.Any),
                                Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                                       .Accepts(AcceptedCharacters.None),
                                Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                                       .Accepts(AcceptedCharacters.None)),
                            Factory.Markup("\r\n}")))),
                new RazorError(RazorResources.ParseError_TextTagCannotContainAttributes, 8, 1, 4),
                new RazorError(String.Format(RazorResources.ParseError_MissingEndTag, "text"), 8, 1, 4),
                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, RazorResources.BlockName_Code, "}", "{"), 1, 0, 1));
        }

        [Fact]
        public void UnterminatedRazorCommentInVerbatimBlock()
        {
            ParseDocumentTest("@{@*",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        Factory.EmptyCSharp()
                               .AsStatement(),
                        new CommentBlock(
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.Span(SpanKind.Comment, new CSharpSymbol(Factory.LocationTracker.CurrentLocation,
                                                                        String.Empty,
                                                                        CSharpSymbolType.Unknown))
                                   .Accepts(AcceptedCharacters.Any)))),
                new RazorError(RazorResources.ParseError_RazorComment_Not_Terminated, 2, 0, 2),
                new RazorError(String.Format(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF, RazorResources.BlockName_Code, "}", "{"), 1, 0, 1));
        }
    }
}
