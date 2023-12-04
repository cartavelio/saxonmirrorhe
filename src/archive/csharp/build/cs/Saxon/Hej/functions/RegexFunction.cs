using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class RegexFunction : Saxon.Hej.functions.SystemFunction, Saxon.Hej.functions.StatefulSystemFunction {
        private Saxon.Hej.regex.RegularExpression staticRegex;
        public Saxon.Hej.regex.RegularExpression getStaticRegex() {
            return staticRegex;
        }
        private void tryToBindRegularExpression(Saxon.Hej.expr.Expression[] arguments) {
            if (((arguments[1] is Saxon.Hej.expr.StringLiteral) && (arguments[(arguments.Length-1)] is Saxon.Hej.expr.StringLiteral))) {
                try {
                    Saxon.Hej.Configuration config = getRetainedStaticContext().getConfiguration();
                    Saxon.Hej.str.UnicodeString re = ((Saxon.Hej.expr.StringLiteral)(arguments[1])).getGroundedValue().getUnicodeStringValue();
                    string flags = ((Saxon.Hej.expr.StringLiteral)(arguments[(arguments.Length-1)])).stringify();
                    string hostLang = "XP30";
                    if ((config.getXsdVersion() == Saxon.Hej.Configuration.XSD11)) {
                        hostLang += "/XSD11";
                    }
                    System.Collections.Generic.IList<string> warnings = new System.Collections.Generic.List<string>(1);
                    staticRegex = config.compileRegularExpression(re, flags, hostLang, warnings);
                    if ((!(allowRegexMatchingEmptyString()) && staticRegex.matches(Saxon.Hej.str.EmptyUnicodeString.getInstance()))) {
                        staticRegex = null;
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                }
            }
        }
        public Saxon.Hej.functions.SystemFunction copy() {
            Saxon.Hej.functions.RegexFunction copy = (Saxon.Hej.functions.RegexFunction)(Saxon.Hej.functions.SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity()));
            copy.staticRegex = staticRegex;
            return copy;
        }
        protected internal abstract bool allowRegexMatchingEmptyString();
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            tryToBindRegularExpression(arguments);
            return base.makeFunctionCall(arguments);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            tryToBindRegularExpression(arguments);
            return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
        }
        protected internal Saxon.Hej.regex.RegularExpression getRegularExpression(Saxon.Hej.om.Sequence[] args, int regexPos, int flagsPos) {
            if ((staticRegex != null)) {
                return staticRegex;
            }
            Saxon.Hej.Configuration config = getRetainedStaticContext().getConfiguration();
            Saxon.Hej.om.Item regexItem = args[regexPos].head();
            if ((regexItem == null)) {
                if ((getRetainedStaticContext().getPackageData().getHostLanguageVersion()>=40)) {
                    return null;
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Regular expression argument must not be an empty sequence (unless 4.0 is enabled)", "XPTY0004");
                    err.setIsTypeError(true);
                    throw err;
                }
            }
            Saxon.Hej.str.UnicodeString regexArg = regexItem.getUnicodeStringValue();
            string flags = "";
            if ((flagsPos<args.Length)) {
                Saxon.Hej.om.Item flagsItem = args[flagsPos].head();
                if (((flagsItem == null) && (getRetainedStaticContext().getPackageData().getHostLanguageVersion()<40))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Flags argument must not be an empty sequence (unless 4.0 is enabled)", "XPTY0004");
                    err.setIsTypeError(true);
                    throw err;
                }
                flags = ((flagsItem == null) ? "" : flagsItem.getStringValue());
            }
            string hostLang = "XP30";
            if ((config.getXsdVersion() == Saxon.Hej.Configuration.XSD11)) {
                hostLang += "/XSD11";
            }
            System.Collections.Generic.IList<string> warnings = new System.Collections.Generic.List<string>(1);
            Saxon.Hej.regex.RegularExpression regex = config.compileRegularExpression(regexArg, flags, hostLang, warnings);
            if ((!(allowRegexMatchingEmptyString()) && regex.matches(Saxon.Hej.str.EmptyUnicodeString.getInstance()))) {
                throw new Saxon.Hej.trans.XPathException("The regular expression must not be one that matches a zero-length string", "FORX0003");
            }
            return regex;
        }
    
    }
}