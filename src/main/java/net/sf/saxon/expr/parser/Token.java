////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.parser;

import net.sf.saxon.value.NumericValue;

/**
 * This class holds static constants and methods defining the lexical tokens used in
 * XPath and XQuery, and associated keywords.
 */

public class Token {

    private final String display;
    private final OperatorSymbol operatorSymbol;
    public Token(String display) {
        this.display = display;
        this.operatorSymbol = OperatorSymbol.NOT_AN_OPERATOR;
    }

    public Token(String display, OperatorSymbol operator) {
        this.display = display;
        this.operatorSymbol = operator;
    }

    public String toString() {
        return display;
    }

    public OperatorSymbol getOperatorSymbol() {
        return operatorSymbol;
    }



    /**
     * Pseudo-token representing the end of the expression
     */
    public static final Token EOF = new Token("<eof>");
    public static final Token VBAR = new Token("|");
    public static final Token SLASH = new Token("/");
    public static final Token AT = new Token("@");
    public static final Token LSQB = new Token("[");
    public static final Token LPAREN = new Token("(");
    public static final Token LCURLY = new Token("{");
    public static final Token EQUALS = new Token("=", OperatorSymbol.EQUALS);
    public static final Token COMMA = new Token(",");
    public static final Token SLASH_SLASH = new Token("//");
    public static final Token GT = new Token(">", OperatorSymbol.GT);
    public static final Token LT = new Token("<", OperatorSymbol.LT);
    public static final Token GE = new Token(">=", OperatorSymbol.GE);
    public static final Token LE = new Token("<=", OperatorSymbol.LE);
    public static final Token NE = new Token("!=", OperatorSymbol.NE);
    public static final Token PLUS = new Token("+", OperatorSymbol.PLUS);
    public static final Token MINUS = new Token("-", OperatorSymbol.MINUS);
    public static final Token MATH_MULT = new Token("×", OperatorSymbol.TIMES);
    public static final Token MATH_DIVIDE = new Token("÷", OperatorSymbol.DIV);

    public static final Token MAPPING_ARROW = new Token("=!>", OperatorSymbol.MAPPING_ARROW);
    public static final Token DOLLAR = new Token("$");
    public static final Token CONCAT = new Token("||", OperatorSymbol.CONCAT);
    public static final Token COLON = new Token(":");
    public static final Token FAT_ARROW = new Token("=>", OperatorSymbol.FAT_ARROW);
    public static final Token BANG = new Token("!", OperatorSymbol.BANG);
    public static final Token COLON_COLON = new Token("::");
    public static final Token COLON_STAR = new Token(":*");
    public static final Token HASH = new Token("#");
    public static final Token QMARK_QMARK = new Token("??", OperatorSymbol.DEEP_LOOKUP);
    public static final Token BANG_BANG = new Token("!!");
    public static final Token COLON_EQUALS = new Token(":=");

    public static final Token PERCENT = new Token("%");
    public static final Token RSQB = new Token("]");

    public static final Token RPAREN = new Token(")");

    public static final Token DOT = new Token(".");

    public static final Token DOT_DOT = new Token("..");
    public static final Token STAR = new Token("*", OperatorSymbol.TIMES);
    public static final Token FOLLOWS = new Token(">>", OperatorSymbol.FOLLOWS);
    public static final Token PRECEDES = new Token("<<", OperatorSymbol.PRECEDES);
    public static final Token SEMICOLON = new Token(";");



    /**
     * Keyword "replace" as used in 4.0 "update map|array EXPR replace EXPR with EXPR"; needs
     * disambiguating from its use in XQuery Update
     */

    public static final int REPLACE = 97;




    /**
     * "after"
     */
    public static final int AFTER = 153;
    /**
     * "before"
     */
    public static final int BEFORE = 154;
    /**
     * "into"
     */
    public static final int INTO = 155;
    /**
     * "with"
     */
    public static final int WITH = 156;



    public static final int UPDATE_MAP = 166;
    public static final int UPDATE_ARRAY = 167;




    /**
     * Constant identifying the token number of the last token to be classified as an operator
     */
    public static final int LAST_OPERATOR = 180;

    // Tokens that set "operator" context, so an immediately following "div" is recognized
    // as an operator, not as an element name

    /**
     * Name token (a QName, in general)
     */
    public static final int NAME = 201;

    /**
     * "prefix:*" token
     */
    public static final int PREFIX = 208;    // e.g. prefix:*
    /**
     * Numeric literal
     */
    public static final int NUMBER = 209;


    /**
     * Question mark symbol. That is, "?"
     */
    public static final Token QMARK = new Token("?");
    /**
     * Question mark symbol followed by '['. That is, "?["
     */
    public static final Token QMARK_LSQB = new Token("?[");
    /**
     * "}" symbol (XQuery only)
     */
    public static final Token RCURLY = new Token("}");

    /**
     * "&lt;" at the start of a direct element constructor XQuery only). The pseudo-XML syntax that
     * follows is read character-by-character by the XQuery parser
     */
    public static final Token START_TAG = new Token("<___>"); // TODO: Temporary
    /**
     * A token representing an XQuery pragma.
     * This construct "(# .... #)" is regarded as a single token, for the QueryParser to sort out.
     */
    public static final Token PRAGMA = new Token("(#pragma#)");


    /**
     * Complete string constructor with no embedded expressions
     */
    public static final int STRING_LITERAL_BACKTICKED = 222;

    /**
     * Backtick (introducing a 4.0 string template)
     */

    public static final Token BACKTICK = new Token("`");

    public static final int HEX_INTEGER = 224;
    public static final int BINARY_INTEGER = 225;

    public static final int DIRECT_PI_CONSTRUCTOR = 231;

    public static final int NODE_TEST = 240;



    /**
     * The following strings are used to represent tokens in error messages
     */

    public final static String[] tokens = new String[300];

    /**
     * Pseudo-token representing the start of the expression
     */
    public static final Token UNKNOWN = new Token("<unknown>");

    public static class NameToken extends Token {

        public NameToken(String name) {
            super(name, operatorForName(name));
        }

        public String toString() {
            return("`" + super.toString() + "`");
        }

        public String getValue() {
            return super.toString();
        }

        private static OperatorSymbol operatorForName(String name) {
            switch (name) {
                case "and":
                    return OperatorSymbol.AND;
                case "or":
                    return OperatorSymbol.OR;
                case "div":
                    return OperatorSymbol.DIV;
                case "idiv":
                    return OperatorSymbol.IDIV;
                case "mod":
                    return OperatorSymbol.MOD;
                case "eq":
                    return OperatorSymbol.FEQ;
                case "ne":
                    return OperatorSymbol.FNE;
                case "lt":
                    return OperatorSymbol.FLT;
                case "le":
                    return OperatorSymbol.FLE;
                case "gt":
                    return OperatorSymbol.FGT;
                case "ge":
                    return OperatorSymbol.FGE;
                case "is":
                    return OperatorSymbol.IS;
                case "union":
                    return OperatorSymbol.UNION;
                case "intersect":
                    return OperatorSymbol.INTERSECT;
                case "except":
                    return OperatorSymbol.EXCEPT;
                case "otherwise":
                    return OperatorSymbol.OTHERWISE;
                case "to":
                    return OperatorSymbol.TO;
                default:
                    return OperatorSymbol.NOT_AN_OPERATOR;
            }
        }
    }

    /**
     * Token representing a string literal. Note that expansion of built-in entities
     * such as <code>&amp;lt;</code> is left to the parser.
     */
    public static class StringLiteral extends Token {
        public StringLiteral(String value) {
            super(value);
        }
        public String toString() {
            return '"' + super.toString() + '"';
        }
        public String getValue() {
            return super.toString();
        }
    }

    /**
     * Token representing a numeric literal.
     */

    public static class NumericLiteral extends Token {
        private final NumericValue value;
        public NumericLiteral(String lexicalForm, NumericValue value) {
            super(lexicalForm);
            this.value = value;
        }
        public NumericValue getValue() {
            return value;
        }
    }

    /**
     * Token representing a partial wildcard: prefix:*, Q{uri}*, or *:suffix
     */

    public static class Wildcard extends Token {

        private final String prefix;
        private final String suffix;
        public Wildcard(String prefix, String suffix) {
            super(prefix.equals("*") ? "*:" + suffix : prefix + ":*");
            this.prefix = prefix;
            this.suffix = suffix;
        }
        public String getPrefix() {
            return prefix;
        }
        public String getSuffix() {
            return suffix;
        }
    }

    public static class DirectCommentConstructor extends Token {

        public DirectCommentConstructor(String content) {
            super(content);
        }

        public String toString() {
            return ("<!--" + super.toString() + "-->");
        }
    }

    public static class DirectProcessingInstructionConstructor extends Token {

        public DirectProcessingInstructionConstructor(String content) {
            super(content);
        }

        public String toString() {
            return ("<?" + super.toString() + "?>");
        }

    }

    public static abstract class ComplexToken extends Token {

        public ComplexToken(String display) {
            super(display);
        }
    }

    public static class DirectElementConstructor extends ComplexToken {

        public DirectElementConstructor(Tokenizer tokenizer, String input, int startOffset) {
            super(input.substring(startOffset, Integer.min(input.length(), startOffset+30)));
        }
    }


}

