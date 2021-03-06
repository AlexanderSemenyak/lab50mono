// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;

namespace System.Web.Razor.Tokenizer
{
    public static class CSharpHelpers
    {
        // CSharp Spec §2.4.2
        public static bool IsIdentifierStart(char character)
        {
            return Char.IsLetter(character) ||
                   character == '_' ||
                   Char.GetUnicodeCategory(character) == UnicodeCategory.LetterNumber; // Ln
        }

        public static bool IsIdentifierPart(char character)
        {
            return Char.IsDigit(character) ||
                   IsIdentifierStart(character) ||
                   IsIdentifierPartByUnicodeCategory(character);
        }

        public static bool IsRealLiteralSuffix(char character)
        {
            return character == 'F' ||
                   character == 'f' ||
                   character == 'D' ||
                   character == 'd' ||
                   character == 'M' ||
                   character == 'm';
        }

        private static bool IsIdentifierPartByUnicodeCategory(char character)
        {
            UnicodeCategory category = Char.GetUnicodeCategory(character);
            return category == UnicodeCategory.NonSpacingMark || // Mn
                   category == UnicodeCategory.SpacingCombiningMark || // Mc
                   category == UnicodeCategory.ConnectorPunctuation || // Pc
                   category == UnicodeCategory.Format; // Cf
        }
    }
}
