using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class Comment : Saxon.Hej.expr.instruct.SimpleNodeConstructor {
        public Comment()  {
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_COMMENT;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.COMMENT;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.Comment exp = new Saxon.Hej.expr.instruct.Comment();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            exp.setSelect(getSelect().copy(rebindings));
            return exp;
        }
        public override void localTypeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((getSelect() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.str.UnicodeString s = ((Saxon.Hej.expr.Literal)(getSelect())).getGroundedValue().getUnicodeStringValue();
                Saxon.Hej.str.UnicodeString s2 = checkContent(s, visitor.getStaticContext().makeEarlyEvaluationContext());
                if (!(s2.Equals(s))) {
                    setSelect(new Saxon.Hej.expr.StringLiteral(s2));
                }
            }
        }
        public override void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString comment = checkContent(value, context);
            output.comment(comment, getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override Saxon.Hej.str.UnicodeString checkContent(Saxon.Hej.str.UnicodeString comment, Saxon.Hej.expr.XPathContext context) {
            if (isXSLT()) {
                return checkContentXSLT(comment);
            } else {
                try {
                    return checkContentXQuery(comment);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.setXPathContext(context);
                    err.setLocation(getLocation());
                    throw;
                }
            }
        }
        public static Saxon.Hej.str.UnicodeString checkContentXSLT(Saxon.Hej.str.UnicodeString comment) {
            string message = invalidity(comment);
            if ((message != null)) {
                long hh;
                while (((hh = comment.indexOf(TWO_HYPHENS, 0))>=0)) {
                    comment = comment.substring(0, (hh+1)).concat(Saxon.Hej.str.StringConstants.SINGLE_SPACE).concat(comment.substring((hh+1)));
                }
                if ((comment.codePointAt((comment.length()-1)) == '-')) {
                    comment = comment.concat(Saxon.Hej.str.StringConstants.SINGLE_SPACE);
                }
            }
            return comment;
        }
        public static Saxon.Hej.str.UnicodeString checkContentXQuery(Saxon.Hej.str.UnicodeString comment) {
            string message = invalidity(comment);
            if ((message != null)) {
                throw new Saxon.Hej.trans.XPathException(message, "XQDY0072");
            }
            return comment;
        }
        private static string invalidity(Saxon.Hej.str.UnicodeString comment) {
            if ((comment.indexOf(TWO_HYPHENS, 0)>=0)) {
                return "Invalid characters (--) in comment";
            }
            if (((comment.length()>0) && (comment.codePointAt((comment.length()-1)) == '-'))) {
                return "Comment cannot end in '-'";
            }
            return null;
        }
        private readonly static Saxon.Hej.str.UnicodeString TWO_HYPHENS = new Saxon.Hej.str.Twine8(Saxon.Hej.str.StringConstants.TWO_HYPHENS);
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("comment", this);
            string flags = "";
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            getSelect().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.Comment.CommentElaborator();
        }
        private class CommentElaborator : Saxon.Hej.expr.elab.SimpleNodePushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.Comment expr = (Saxon.Hej.expr.instruct.Comment)(getExpression());
                Saxon.Hej.s9api.Location loc = expr.getLocation();
                Saxon.Hej.expr.elab.UnicodeStringEvaluator contentEval = expr.getSelect().makeElaborator().elaborateForUnicodeString(true);
                if (expr.isXSLT()) {
                    return (@out, context) => {
                        Saxon.Hej.str.UnicodeString content = /*Delegate.eval()*/contentEval(context);
                        content = Saxon.Hej.expr.instruct.Comment.checkContentXSLT(content);
                        @out.comment(content, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                        return null;
                    } ;
                } else {
                    return (@out, context) => {
                        Saxon.Hej.str.UnicodeString content = /*Delegate.eval()*/contentEval(context);
                        Saxon.Hej.expr.instruct.Comment.checkContentXQuery(content);
                        @out.comment(content, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                        return null;
                    } ;
                }
            }
        
        }
    
    }
}