using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Put : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("Dynamic evaluation of fn:put() is not supported");
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Put.PutFnElaborator();
        }
        public class PutFnElaborator : Saxon.Hej.expr.elab.UpdateElaborator {
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.Put targetFunction = (Saxon.Hej.functions.Put)(fnc.getTargetFunction());
                Saxon.Hej.expr.elab.ItemEvaluator arg0 = fnc.getArg(0).makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator arg1 = fnc.getArg(1).makeElaborator().elaborateForItem();
                return (context, pul) => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/arg0(context));
                    int kind = node.getNodeKind();
                    if (((kind != Saxon.Hej.type.Type.ELEMENT) && (kind != Saxon.Hej.type.Type.DOCUMENT))) {
                        throw new Saxon.Hej.trans.XPathException("Node in put() must be a document or element node", "FOUP0001", context);
                    }
                    string relative = /*Delegate.eval()*/arg1(context).getStringValue();
                    string abs;
                    try {
                        System.Uri resolved = Saxon.Hej.functions.ResolveURI.makeAbsolute(relative, targetFunction.getStaticBaseUriString());
                        abs = resolved.ToString();
                    } catch (System.UriFormatException err) {
                        throw new Saxon.Hej.trans.XPathException(((("Base URI "+Saxon.Hej.trans.Err.wrap(targetFunction.getStaticBaseUriString()))+" is invalid: ")+err.Message), "FOUP0002", context);
                    }
                    context.getConfiguration().getLogger().info(("fn:put writing to "+abs));
                    pul.addPutAction(node, abs, fnc);
                } ;
            }
        
        }
    
    }
}