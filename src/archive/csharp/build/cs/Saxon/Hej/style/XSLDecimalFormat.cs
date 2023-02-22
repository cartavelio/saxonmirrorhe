using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLDecimalFormat : Saxon.Hej.style.StyleElement {
        internal bool prepared = false;
        internal string name;
        internal string decimalSeparator;
        internal string groupingSeparator;
        internal string exponentSeparator;
        internal string infinity;
        internal string minusSign;
        internal string NaN;
        internal string percent;
        internal string perMille;
        internal string zeroDigit;
        internal string digit;
        internal string patternSeparator;
        internal Saxon.Hej.trans.DecimalSymbols symbols;
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            if (prepared) {
                return ;
            }
            prepared = true;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "name":
                    name = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "decimal-separator":
                    decimalSeparator = value;
                    break ;
                  case "grouping-separator":
                    groupingSeparator = value;
                    break ;
                  case "infinity":
                    infinity = value;
                    break ;
                  case "minus-sign":
                    minusSign = value;
                    break ;
                  case "NaN":
                    NaN = value;
                    break ;
                  case "percent":
                    percent = value;
                    break ;
                  case "per-mille":
                    perMille = value;
                    break ;
                  case "zero-digit":
                    zeroDigit = value;
                    break ;
                  case "digit":
                    digit = value;
                    break ;
                  case "exponent-separator":
                    exponentSeparator = value;
                    break ;
                  case "pattern-separator":
                    patternSeparator = value;
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkTopLevel("XTSE0010", false);
            checkEmpty();
            int precedence = decl.getPrecedence();
            if ((symbols == null)) {
                return ;
            }
            if ((decimalSeparator != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.DECIMAL_SEPARATOR, decimalSeparator, precedence);
            }
            if ((groupingSeparator != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.GROUPING_SEPARATOR, groupingSeparator, precedence);
            }
            if ((infinity != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.INFINITY, infinity, precedence);
            }
            if ((minusSign != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.MINUS_SIGN, minusSign, precedence);
            }
            if ((NaN != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.NAN, NaN, precedence);
            }
            if ((percent != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.PERCENT, percent, precedence);
            }
            if ((perMille != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.PER_MILLE, perMille, precedence);
            }
            if ((zeroDigit != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.ZERO_DIGIT, zeroDigit, precedence);
            }
            if ((digit != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.DIGIT, digit, precedence);
            }
            if ((exponentSeparator != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.EXPONENT_SEPARATOR, exponentSeparator, precedence);
            }
            if ((patternSeparator != null)) {
                setProp(Saxon.Hej.trans.DecimalSymbols.PATTERN_SEPARATOR, patternSeparator, precedence);
            }
        }
        private void setProp(int propertyCode, string value, int precedence) {
            try {
                symbols.setProperty(propertyCode, value, precedence);
            } catch (Saxon.Hej.trans.XPathException err) {
                string attName = Saxon.Hej.trans.DecimalSymbols.propertyNames[propertyCode];
                err.setLocation(new Saxon.Hej.tree.AttributeLocation(this, Saxon.Hej.om.StructuredQName.fromClarkName(attName)));
                throw;
            }
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            prepareAttributes();
            Saxon.Hej.trans.DecimalFormatManager dfm = getCompilation().getPrincipalStylesheetModule().getDecimalFormatManager();
            if ((name == null)) {
                symbols = dfm.getDefaultDecimalFormat();
            } else {
                Saxon.Hej.om.StructuredQName formatName = makeQName(name, null, "name");
                symbols = dfm.obtainNamedDecimalFormat(formatName);
                symbols.setHostLanguage(Saxon.Api.HostLanguage.XSLT, 30);
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
        }
    
    }
}