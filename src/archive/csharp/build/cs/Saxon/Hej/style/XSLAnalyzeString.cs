using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLAnalyzeString : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.expr.Expression regex;
        private Saxon.Hej.expr.Expression flags;
        private Saxon.Hej.style.StyleElement matching;
        private Saxon.Hej.style.StyleElement nonMatching;
        private Saxon.Hej.regex.RegularExpression pattern;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainFallback() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string regexAtt = null;
            string flagsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "regex":
                    regexAtt = value;
                    regex = makeAttributeValueTemplate(regexAtt, att);
                    break ;
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "flags":
                    flagsAtt = value;
                    flags = makeAttributeValueTemplate(flagsAtt, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((selectAtt == null)) {
                reportAbsence("select");
                select = makeExpression(".", null);
            }
            if ((regexAtt == null)) {
                reportAbsence("regex");
                regex = makeAttributeValueTemplate("xxx", null);
            }
            if ((flagsAtt == null)) {
                flagsAtt = "";
                flags = makeAttributeValueTemplate("", null);
            }
            if (((regex is Saxon.Hej.expr.StringLiteral) && (flags is Saxon.Hej.expr.StringLiteral))) {
                try {
                    Saxon.Hej.str.UnicodeString regex = ((Saxon.Hej.expr.StringLiteral)(this.regex)).getString();
                    string flagstr = ((Saxon.Hej.expr.StringLiteral)(flags)).stringify();
                    System.Collections.Generic.IList<string> warnings = new System.Collections.Generic.List<string>();
                    pattern = getConfiguration().compileRegularExpression(regex, flagstr, ((getEffectiveVersion()>=30) ? "XP30" : "XP20"), warnings);
                    foreach (string w in warnings) {
                        issueWarning(w, this);
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                    if ("FORX0001".Equals(err.getErrorCodeLocalPart())) {
                        invalidFlags(("Error in regular expression flags: "+err.Message));
                    } else {
                        invalidRegex(("Error in regular expression: "+err.Message));
                    }
                }
            }
        }
        private void invalidRegex(string message) {
            compileErrorInAttribute(message, "XTDE1140", "regex");
            setDummyRegex();
        }
        private void invalidFlags(string message) {
            compileErrorInAttribute(message, "XTDE1145", "flags");
            setDummyRegex();
        }
        private void setDummyRegex() {
            try {
                pattern = getConfiguration().compileRegularExpression(Saxon.Hej.str.BMPString.of("x"), "", "XP20", null);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Ejava.lang.IllegalStateException();
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            bool foundFallback = false;
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((curr is Saxon.Hej.style.XSLFallback)) {
                    foundFallback = true;
                } else {
                    if ((curr is Saxon.Hej.style.XSLMatchingSubstring)) {
                        bool b = curr.getLocalPart().Equals("matching-substring");
                        if (b) {
                            if ((((matching != null) || (nonMatching != null)) || foundFallback)) {
                                compileError("xsl:matching-substring element must come first", "XTSE0010");
                            }
                            matching = (Saxon.Hej.style.StyleElement)(curr);
                        } else {
                            if (((nonMatching != null) || foundFallback)) {
                                compileError("xsl:non-matching-substring cannot appear here", "XTSE0010");
                            }
                            nonMatching = (Saxon.Hej.style.StyleElement)(curr);
                        }
                    } else {
                        compileError("Only xsl:matching-substring and xsl:non-matching-substring are allowed here", "XTSE0010");
                    }
                }
            }
            if (((matching == null) && (nonMatching == null))) {
                compileError("At least one xsl:matching-substring or xsl:non-matching-substring element must be present", "XTSE1130");
            }
            select = typeCheck("select", select);
            regex = typeCheck("regex", regex);
            flags = typeCheck("flags", flags);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression matchingBlock = null;
            if ((matching != null)) {
                matchingBlock = matching.compileSequenceConstructor(exec, decl, false);
            }
            Saxon.Hej.expr.Expression nonMatchingBlock = null;
            if ((nonMatching != null)) {
                nonMatchingBlock = nonMatching.compileSequenceConstructor(exec, decl, false);
            }
            try {
                return new Saxon.Hej.expr.instruct.AnalyzeString(select, regex, flags, ((matchingBlock == null) ? null : matchingBlock.simplify()), ((nonMatchingBlock == null) ? null : nonMatchingBlock.simplify()), pattern);
            } catch (Saxon.Hej.trans.XPathException e) {
                compileError(e);
                return null;
            }
        }
    
    }
}