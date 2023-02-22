using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class SuperId : Saxon.Hej.functions.SystemFunction {
        public const int ID = 0;
        public const int ELEMENT_WITH_ID = 1;
        public abstract int getOp();
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int prop = ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            if ((((getArity() == 1)) || (((arguments[1].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0))) {
                prop |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            return prop;
        }
        public static Saxon.Hej.om.SequenceIterator getIdSingle(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.str.UnicodeString idrefs, int operation) {
            if (Saxon.Hej.value.Whitespace.containsWhitespace(idrefs.codePoints())) {
                Saxon.Hej.value.Whitespace.Tokenizer tokens = new Saxon.Hej.value.Whitespace.Tokenizer(idrefs);
                Saxon.Hej.functions.SuperId.IdMappingFunction map = new Saxon.Hej.functions.SuperId.IdMappingFunction();
                map.document = doc;
                map.operation = operation;
                Saxon.Hej.om.SequenceIterator result = new Saxon.Hej.expr.MappingIterator(tokens, map);
                return new Saxon.Hej.expr.sort.DocumentOrderIterator(result, Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
            } else {
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(doc.selectID(idrefs.ToString(), (operation == ELEMENT_WITH_ID)));
            }
        }
        public static Saxon.Hej.om.SequenceIterator getIdMultiple(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.om.SequenceIterator idrefs, int operation) {
            Saxon.Hej.functions.SuperId.IdMappingFunction map = new Saxon.Hej.functions.SuperId.IdMappingFunction();
            map.document = doc;
            map.operation = operation;
            Saxon.Hej.om.SequenceIterator result = new Saxon.Hej.expr.MappingIterator(idrefs, map);
            return new Saxon.Hej.expr.sort.DocumentOrderIterator(result, Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo start = ((arguments.Length == 1) ? getContextNode(context) : (Saxon.Hej.om.NodeInfo)(arguments[1].head()));
            Saxon.Hej.om.NodeInfo arg1 = start.getRoot();
            if ((arg1.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                throw new Saxon.Hej.trans.XPathException(((("In the "+getFunctionName().getLocalPart())+"() function,")+" the tree being searched must be one whose root is a document node"), "FODC0001", context);
            }
            Saxon.Hej.om.TreeInfo doc = arg1.getTreeInfo();
            Saxon.Hej.om.SequenceIterator result;
            if ((arguments[0] is Saxon.Hej.value.AtomicValue)) {
                result = getIdSingle(doc, ((Saxon.Hej.value.AtomicValue)(arguments[0])).getUnicodeStringValue(), getOp());
            } else {
                Saxon.Hej.om.SequenceIterator idrefs = arguments[0].iterate();
                result = getIdMultiple(doc, idrefs, getOp());
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(result);
        }
        private class IdMappingFunction : Saxon.Hej.expr.MappingFunction {
            public Saxon.Hej.om.TreeInfo document;
            public int operation;
            public Saxon.Hej.om.SequenceIterator map(Saxon.Hej.om.Item item) {
                Saxon.Hej.str.UnicodeString idrefs = Saxon.Hej.value.Whitespace.trim(item.getUnicodeStringValue());
                if (Saxon.Hej.value.Whitespace.containsWhitespace(idrefs.codePoints())) {
                    Saxon.Hej.value.Whitespace.Tokenizer tokens = new Saxon.Hej.value.Whitespace.Tokenizer(idrefs);
                    Saxon.Hej.functions.SuperId.IdMappingFunction submap = new Saxon.Hej.functions.SuperId.IdMappingFunction();
                    submap.document = document;
                    submap.operation = operation;
                    return new Saxon.Hej.expr.MappingIterator(tokens, submap);
                } else {
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(document.selectID(idrefs.ToString(), (operation == ELEMENT_WITH_ID)));
                }
            }
        
        }
        public class Id : Saxon.Hej.functions.SuperId {
            public override int getOp() {
                return ID;
            }
        
        }
        public class ElementWithId : Saxon.Hej.functions.SuperId {
            public override int getOp() {
                return ELEMENT_WITH_ID;
            }
        
        }
    
    }
}