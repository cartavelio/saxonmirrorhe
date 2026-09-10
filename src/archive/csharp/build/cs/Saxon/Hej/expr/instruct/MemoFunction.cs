using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class MemoFunction : Saxon.Hej.expr.instruct.UserFunction {
        public override void computeEvaluationMode() {
            evaluator = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluator(getBody());
        }
        public override bool isMemoFunction() {
            return true;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] actualArgs) {
            string key = getCombinedKey(actualArgs);
            Saxon.Hej.Controller controller = context.getController();
            System.Collections.Generic.Dictionary<string,Saxon.Hej.om.Sequence> map = (System.Collections.Generic.Dictionary<string,Saxon.Hej.om.Sequence>)(controller.getUserData(this, "memo-function-cache"));
            Saxon.Hej.om.Sequence value = ((map == null) ? null : Saxon.Impl.Helpers.MapUtils.Get(map, key));
            if ((value != null)) {
                return value;
            }
            value = base.call(context, actualArgs);
            if ((map == null)) {
                map = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>(32);
                controller.setUserData(this, "memo-function-cache", map);
            }
            Saxon.Impl.Helpers.MapUtils.Put(map, key, value);
            return value;
        }
        private static string getCombinedKey(Saxon.Hej.om.Sequence[] @params) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.Sequence val in @params) {
                Saxon.Hej.om.SequenceIterator iter = val.iterate();
                Saxon.Hej.om.Item item;
                while (((item = iter.next()) != null)) {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                        node.generateId(sb);
                    } else {
                        if ((item is Saxon.Hej.value.QNameValue)) {
                            sb.append(Saxon.Hej.type.Type.displayTypeName(item)).append('/').append(((Saxon.Hej.value.QNameValue)(item)).getClarkName());
                        } else {
                            if ((item is Saxon.Hej.value.AtomicValue)) {
                                sb.append(Saxon.Hej.type.Type.displayTypeName(item)).append('/').append(item.getUnicodeStringValue());
                            } else {
                                if ((item is Saxon.Hej.om.Function)) {
                                    sb.append(item.GetType().Name).append("@").append((""+Saxon.Ejava.lang.System.identityHashCode(item)));
                                }
                            }
                        }
                    }
                    sb.append('\u0001');
                }
                sb.append('\u0002');
            }
            return sb.ToString();
        }
    
    }
}