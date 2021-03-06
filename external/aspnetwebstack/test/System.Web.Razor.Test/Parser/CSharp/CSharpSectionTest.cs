// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using Xunit;

namespace System.Web.Razor.Test.Parser.CSharp
{
    public class CSharpSectionTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void ParseSectionBlockCapturesNewlineImmediatelyFollowing()
        {
            ParseDocumentTest(@"@section
",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator(String.Empty),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section\r\n"))),
                new RazorError(
                    String.Format(
                        RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                        RazorResources.ErrorComponent_EndOfFile),
                    10, 1, 0));
        }

        [Fact]
        public void ParseSectionBlockCapturesWhitespaceToEndOfLineInSectionStatementMissingOpenBrace()
        {
            ParseDocumentTest(@"@section Foo         
    ",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("Foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section Foo         \r\n")),
                    Factory.Markup(@"    ")),
                new RazorError(RazorResources.ParseError_MissingOpenBraceAfterSection, 12, 0, 12));
        }

        [Fact]
        public void ParseSectionBlockCapturesWhitespaceToEndOfLineInSectionStatementMissingName()
        {
            ParseDocumentTest(@"@section         
    ",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator(String.Empty),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section         \r\n")),
                    Factory.Markup(@"    ")),
                new RazorError(
                    String.Format(
                        RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                        RazorResources.ErrorComponent_EndOfFile),
                    23, 1, 4));
        }

        [Fact]
        public void ParseSectionBlockIgnoresSectionUnlessAllLowerCase()
        {
            ParseDocumentTest("@Section foo",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("Section")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.Markup(" foo")));
        }

        [Fact]
        public void ParseSectionBlockReportsErrorAndTerminatesSectionBlockIfKeywordNotFollowedByIdentifierStartCharacter()
        {
            ParseDocumentTest("@section 9 { <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator(String.Empty),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section ")),
                    Factory.Markup("9 { <p>Foo</p> }")),
                new RazorError(
                    String.Format(
                        RazorResources.ParseError_Unexpected_Character_At_Section_Name_Start,
                        String.Format(RazorResources.ErrorComponent_Character, "9")),
                    9, 0, 9));
        }

        [Fact]
        public void ParseSectionBlockReportsErrorAndTerminatesSectionBlockIfNameNotFollowedByOpenBrace()
        {
            ParseDocumentTest("@section foo-bar { <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo")),
                    Factory.Markup("-bar { <p>Foo</p> }")),
                new RazorError(RazorResources.ParseError_MissingOpenBraceAfterSection, 12, 0, 12));
        }

        [Fact]
        public void ParserOutputsErrorOnNestedSections()
        {
            ParseDocumentTest("@section foo { @section bar { <p>Foo</p> } }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo {")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup(" "),
                            new SectionBlock(new SectionCodeGenerator("bar"),
                                Factory.CodeTransition(),
                                Factory.MetaCode("section bar {")
                                       .AutoCompleteWith(null, atEndOfSpan: true),
                                new MarkupBlock(
                                    Factory.Markup(" <p>Foo</p> ")),
                                Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()),
                new RazorError(
                    String.Format(
                        RazorResources.ParseError_Sections_Cannot_Be_Nested,
                        RazorResources.SectionExample_CS),
                    23, 0, 23));
        }

        [Fact]
        public void ParseSectionBlockHandlesEOFAfterOpenBrace()
        {
            ParseDocumentTest("@section foo {",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo {")
                               .AutoCompleteWith("}", atEndOfSpan: true),
                        new MarkupBlock())),
                new RazorError(
                    String.Format(RazorResources.ParseError_Expected_X, "}"),
                    14, 0, 14));
        }

        [Fact]
        public void ParseSectionBlockHandlesUnterminatedSection()
        {
            ParseDocumentTest("@section foo { <p>Foo{}</p>",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo {")
                               .AutoCompleteWith("}", atEndOfSpan: true),
                        new MarkupBlock(
                // Need to provide the markup span as fragments, since the parser will split the {} into separate symbols.
                            Factory.Markup(" <p>Foo", "{", "}", "</p>")))),
                new RazorError(
                    String.Format(RazorResources.ParseError_Expected_X, "}"),
                    27, 0, 27));
        }

        [Fact]
        public void ParseSectionBlockReportsErrorAndAcceptsWhitespaceToEndOfLineIfSectionNotFollowedByOpenBrace()
        {
            ParseDocumentTest(@"@section foo      
",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo      \r\n"))),
                new RazorError(RazorResources.ParseError_MissingOpenBraceAfterSection, 12, 0, 12));
        }

        [Fact]
        public void ParseSectionBlockAcceptsOpenBraceMultipleLinesBelowSectionName()
        {
            ParseDocumentTest(@"@section foo      





{
<p>Foo</p>
}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo      \r\n\r\n\r\n\r\n\r\n\r\n{")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup("\r\n<p>Foo</p>\r\n")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockParsesNamedSectionCorrectly()
        {
            ParseDocumentTest("@section foo { <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo {")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup(" <p>Foo</p> ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockDoesNotRequireSpaceBetweenSectionNameAndOpenBrace()
        {
            ParseDocumentTest("@section foo{ <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo{")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup(" <p>Foo</p> ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockBalancesBraces()
        {
            ParseDocumentTest("@section foo { <script>(function foo() { return 1; })();</script> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo {")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup(" <script>(function foo() { return 1; })();</script> ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockAllowsBracesInCSharpExpression()
        {
            ParseDocumentTest("@section foo { I really want to render a close brace, so here I go: @(\"}\") }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section foo {")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup(" I really want to render a close brace, so here I go: "),
                            new ExpressionBlock(
                                Factory.CodeTransition(),
                                Factory.MetaCode("(").Accepts(AcceptedCharacters.None),
                                Factory.Code("\"}\"").AsExpression(),
                                Factory.MetaCode(")").Accepts(AcceptedCharacters.None)),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void SectionIsCorrectlyTerminatedWhenCloseBraceImmediatelyFollowsCodeBlock()
        {
            ParseDocumentTest(@"@section Foo {
@if(true) {
}
}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new SectionBlock(new SectionCodeGenerator("Foo"),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section Foo {")
                               .AutoCompleteWith(null, atEndOfSpan: true),
                        new MarkupBlock(
                            Factory.Markup("\r\n"),
                            new StatementBlock(
                                Factory.CodeTransition(),
                                Factory.Code("if(true) {\r\n}\r\n").AsStatement()
                            )),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }
    }
}
