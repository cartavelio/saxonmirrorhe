using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CollatingFunctionFree : Saxon.Hej.functions.SystemFunction {
        private int getCollationArgument() {
            return (getArity()-1);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression c = arguments[(arguments.Length-1)];
            if ((c is Saxon.Hej.expr.StringLiteral)) {
                string coll = ((Saxon.Hej.expr.StringLiteral)(c)).stringify();
                try {
                    System.Uri collUri = new System.Uri(coll, System.UriKind.RelativeOrAbsolute);
                    if (!(collUri.IsAbsoluteUri)) {
                        collUri = Saxon.Hej.functions.ResolveURI.makeAbsolute(coll, getStaticBaseUriString());
                        coll = collUri.OriginalString;
                    }
                } catch (System.UriFormatException e) {
                    visitor.getStaticContext().issueWarning(("Cannot resolve relative collation URI "+coll), c.getLocation());
                }
                Saxon.Hej.functions.CollatingFunctionFixed fn = bindCollation(coll);
                Saxon.Hej.expr.Expression[] newArgs = new Saxon.Hej.expr.Expression[(arguments.Length-1)];
                Saxon.Ejava.lang.System.arraycopy(arguments, 0, newArgs, 0, newArgs.Length);
                return fn.makeFunctionCall(newArgs);
            }
            return null;
        }
        public Saxon.Hej.functions.CollatingFunctionFixed bindCollation(string collationName) {
            Saxon.Hej.Configuration config = getRetainedStaticContext().getConfiguration();
            Saxon.Hej.functions.CollatingFunctionFixed @fixed = (Saxon.Hej.functions.CollatingFunctionFixed)(config.makeSystemFunction(getFunctionName().getLocalPart(), (getArity()-1)));
            @fixed.setRetainedStaticContext(getRetainedStaticContext());
            @fixed.setCollationName(collationName);
            return @fixed;
        }
        public static string expandCollationURI(string collationName, System.Uri expressionBaseURI) {
            try {
                System.Uri collationURI = new System.Uri(collationName, System.UriKind.RelativeOrAbsolute);
                if (!(collationURI.IsAbsoluteUri)) {
                    if ((expressionBaseURI == null)) {
                        throw new Saxon.Hej.trans.XPathException((("Cannot resolve relative collation URI '"+collationName)+"': unknown or invalid base URI"), "FOCH0002");
                    }
                    collationURI = new System.Uri(expressionBaseURI, collationURI);
                    collationName = collationURI.ToString();
                }
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException((("Collation name '"+collationName)+"' is not a valid URI"), "FOCH0002");
            }
            return collationName;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            int c = getCollationArgument();
            string collation = args[c].head().getStringValue();
            collation = expandCollationURI(collation, getRetainedStaticContext().getStaticBaseUri());
            Saxon.Hej.functions.CollatingFunctionFixed @fixed = bindCollation(collation);
            Saxon.Hej.om.Sequence[] retainedArgs = new Saxon.Hej.om.Sequence[(args.Length-1)];
            Saxon.Ejava.lang.System.arraycopy(args, 0, retainedArgs, 0, c);
            if (((c+1)<getArity())) {
                Saxon.Ejava.lang.System.arraycopy(args, (c+1), retainedArgs, c, (getArity()-c));
            }
            return @fixed.call(context, retainedArgs);
        }
        public override string getStreamerName() {
            try {
                return bindCollation(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI).getStreamerName();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
    
    }
}