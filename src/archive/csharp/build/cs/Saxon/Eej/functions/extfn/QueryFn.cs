using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class QueryFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AnyExternalObject queryExpression = (Saxon.Hej.value.AnyExternalObject)(arguments[0].head());
            if ((queryExpression == null)) {
                return null;
            }
            Saxon.Hej.query.XQueryExpression exp = (Saxon.Hej.query.XQueryExpression)(queryExpression.getWrappedObject());
            Saxon.Hej.query.DynamicQueryContext dqc = new Saxon.Hej.query.DynamicQueryContext(context.getConfiguration());
            if ((arguments.Length>=2)) {
                Saxon.Hej.om.Item source = arguments[1].head();
                if ((source != null)) {
                    dqc.setContextItem(source);
                }
            } else {
                Saxon.Hej.om.Item co = context.getContextItem();
                if ((co != null)) {
                    dqc.setContextItem(context.getContextItem());
                }
            }
            if ((arguments.Length == 3)) {
                Saxon.Hej.om.SequenceIterator iter = arguments[2].iterate();
                Saxon.Hej.om.Item param;
                while (((param = iter.next()) != null)) {
                    if ((param is Saxon.Hej.om.NodeInfo)) {
                        switch (((Saxon.Hej.om.NodeInfo)(param)).getNodeKind()) {
                          case Saxon.Hej.type.Type.ELEMENT:
                          case Saxon.Hej.type.Type.ATTRIBUTE:
                            Saxon.Hej.om.AtomicSequence val = param.atomize();
                            dqc.setParameter(Saxon.Hej.om.NameOfNode.makeName((Saxon.Hej.om.NodeInfo)(param)).getStructuredQName(), val);
                            break ;
                          case Saxon.Hej.type.Type.DOCUMENT:
                            foreach (Saxon.Hej.om.NodeInfo kid in ((Saxon.Hej.om.NodeInfo)(param)).children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                                Saxon.Hej.om.AtomicSequence val2 = kid.atomize();
                                dqc.setParameter(Saxon.Hej.om.NameOfNode.makeName(kid).getStructuredQName(), val2);
                            }
                            break ;
                          default:
                            throw new Saxon.Hej.trans.XPathException("Parameters passed to saxon:query() must be element, attribute, or document nodes");
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException("Parameters passed to saxon:query() must be nodes");
                    }
                }
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(exp.iterator(dqc));
        }
    
    }
}