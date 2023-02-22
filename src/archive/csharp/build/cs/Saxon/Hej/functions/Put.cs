using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Put : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_d1122e88(this, this, arguments);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("Dynamic evaluation of fn:put() is not supported");
        }
    
        private class Inner_SystemFunctionCall_d1122e88 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly Put outerInstance;
            public Inner_SystemFunctionCall_d1122e88(Put outerInstance, Put arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public string staticBaseUri() {
                return /*superMethod*/getTargetFunction().getStaticBaseUriString();
            }
            public override  /* REPLACING public */ bool isUpdatingExpression() {
                return true;
            }
            public override  /* REPLACING public */ void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*superMethod*/getArg(0).evaluateItem(/*localparam*/context));
                int kind = /*localvar*/node.getNodeKind();
                if (((/*localvar*/kind != Saxon.Hej.type.Type.ELEMENT) && (/*localvar*/kind != Saxon.Hej.type.Type.DOCUMENT))) {
                    throw new Saxon.Hej.trans.XPathException("Node in put() must be a document or element node", "FOUP0001", /*localparam*/context);
                }
                string relative = /*superMethod*/getArg(1).evaluateItem(/*localparam*/context).getStringValue();
                string abs;
                try {
                    System.Uri resolved = Saxon.Hej.functions.ResolveURI.makeAbsolute(/*localvar*/relative, /*method*/staticBaseUri());
                    /*localvar*/abs = /*localvar*/resolved.ToString();
                } catch (System.UriFormatException err) {
                    throw new Saxon.Hej.trans.XPathException(((("Base URI "+Saxon.Hej.trans.Err.wrap(/*method*/staticBaseUri()))+" is invalid: ")+/*exception*/err.Message), "FOUP0002", /*localparam*/context);
                }
                /*localparam*/context.getConfiguration().getLogger().info(("fn:put writing to "+/*localvar*/abs));
                /*localparam*/pul.addPutAction(/*localvar*/node, /*localvar*/abs, this);
            }
        }

    }
}