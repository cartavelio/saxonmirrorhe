using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class XQueryParserExtensionEE : Saxon.Eej.expr.XQueryParserExtensionPE {
        private bool acceptUpdateSyntax = false;
        private bool foundRevalidationDeclaration = false;
        public XQueryParserExtensionEE(bool updating)  {
            acceptUpdateSyntax = updating;
        }
        public override void parseRevalidationDeclaration(Saxon.Hej.query.XQueryParser p) {
            checkXQueryUpdate(p);
            if (foundRevalidationDeclaration) {
                p.grumble("declare revalidation appears more than once", "XUST0003");
            }
            foundRevalidationDeclaration = true;
            p.nextToken();
            p.expect(Saxon.Hej.expr.parser.Token.NAME);
            int val;
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            if ("strict".Equals(t.currentTokenValue)) {
                val = Saxon.Hej.lib.Validation.STRICT;
            } else {
                if ("lax".Equals(t.currentTokenValue)) {
                    val = Saxon.Hej.lib.Validation.LAX;
                } else {
                    if ("skip".Equals(t.currentTokenValue)) {
                        val = Saxon.Hej.lib.Validation.SKIP;
                    } else {
                        p.grumble("revalidation mode must be 'strict', 'lax', or 'skip'");
                        val = Saxon.Hej.lib.Validation.SKIP;
                    }
                }
            }
            ((Saxon.Hej.query.QueryModule)(p.getStaticContext())).setRevalidationMode(val);
            p.nextToken();
        }
        public override void parseUpdatingFunctionDeclaration(Saxon.Hej.query.XQueryParser p) {
            p.parseFunctionDeclaration(Saxon.Hej.query.AnnotationList.singleton(new Saxon.Hej.query.Annotation(Saxon.Hej.query.Annotation.UPDATING)));
        }
        protected internal override Saxon.Hej.expr.Expression parseExtendedExprSingle(Saxon.Hej.expr.parser.XPathParser p) {
            if (acceptUpdateSyntax) {
                Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.COPY:
                    return /*134*/Saxon.Eej.ee.update.UpdateParser.parseTransformExpression(p);
                  case Saxon.Hej.expr.parser.Token.INSERT_NODE:
                    return /*134*/Saxon.Eej.ee.update.UpdateParser.parseInsertExpression(p);
                  case Saxon.Hej.expr.parser.Token.DELETE_NODE:
                    return /*134*/Saxon.Eej.ee.update.UpdateParser.parseDeleteExpression(p);
                  case Saxon.Hej.expr.parser.Token.REPLACE_NODE:
                    return /*134*/Saxon.Eej.ee.update.UpdateParser.parseReplaceNodeExpression(p);
                  case Saxon.Hej.expr.parser.Token.REPLACE_VALUE:
                    return /*134*/Saxon.Eej.ee.update.UpdateParser.parseReplaceValueExpression(p);
                  case Saxon.Hej.expr.parser.Token.RENAME_NODE:
                    return /*134*/Saxon.Eej.ee.update.UpdateParser.parseRenameExpression(p);
                  default:
                    break ;
                }
            }
            return base.parseExtendedExprSingle(p);
        }
        private void checkXQueryUpdate(Saxon.Hej.expr.parser.XPathParser p) {
            if (!(acceptUpdateSyntax)) {
                p.grumble("XQuery update syntax encountered in a non-updating query");
            }
        }
    
    }
}