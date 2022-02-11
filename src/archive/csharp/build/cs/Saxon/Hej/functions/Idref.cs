using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Idref : Saxon.Hej.functions.SystemFunction {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int prop = ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            if ((((getArity() == 1)) || arguments[1].hasSpecialProperty(Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET))) {
                prop |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            return prop;
        }
        public static Saxon.Hej.om.SequenceIterator getIdrefMultiple(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.om.SequenceIterator keys, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.functions.Idref.IdrefMappingFunction map = new Saxon.Hej.functions.Idref.IdrefMappingFunction();
            map.document = doc;
            map.keyContext = context;
            map.keyManager = context.getController().getExecutable().getTopLevelPackage().getKeyManager();
            map.keySet = map.keyManager.getKeyDefinitionSet(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XS_IDREFS));
            Saxon.Hej.om.SequenceIterator allValues = new Saxon.Hej.expr.MappingIterator(keys, map);
            return new Saxon.Hej.expr.sort.DocumentOrderIterator(allValues, Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo start = ((arguments.Length == 1) ? getContextNode(context) : (Saxon.Hej.om.NodeInfo)(arguments[1].head()));
            Saxon.Hej.om.NodeInfo arg2 = start.getRoot();
            if ((arg2.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                throw new Saxon.Hej.trans.XPathException(("In the idref() function,"+" the tree being searched must be one whose root is a document node"), "FODC0001", context);
            }
            return new Saxon.Hej.om.LazySequence(getIdrefMultiple(arg2.getTreeInfo(), arguments[0].iterate(), context));
        }
        private class IdrefMappingFunction : Saxon.Hej.expr.MappingFunction {
            public Saxon.Hej.om.TreeInfo document;
            public Saxon.Hej.expr.XPathContext keyContext;
            public Saxon.Hej.trans.KeyManager keyManager;
            public Saxon.Hej.trans.KeyDefinitionSet keySet;
            public Saxon.Hej.om.SequenceIterator map(Saxon.Hej.om.Item item) {
                return keyManager.selectByKey(keySet, document, (Saxon.Hej.value.StringValue)(item), keyContext);
            }
        
        }
    
    }
}