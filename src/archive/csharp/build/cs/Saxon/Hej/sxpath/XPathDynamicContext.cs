using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public class XPathDynamicContext {
        private readonly Saxon.Hej.type.ItemType contextItemType;
        private readonly Saxon.Hej.expr.XPathContextMajor contextObject;
        private readonly Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        protected internal XPathDynamicContext(Saxon.Hej.type.ItemType contextItemType, Saxon.Hej.expr.XPathContextMajor contextObject, Saxon.Hej.expr.instruct.SlotManager stackFrameMap)  {
            this.contextItemType = contextItemType;
            this.contextObject = contextObject;
            this.stackFrameMap = stackFrameMap;
        }
        public void setContextItem(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                if (!(((Saxon.Hej.om.NodeInfo)(item)).getConfiguration().isCompatible(contextObject.getConfiguration()))) {
                    throw new Saxon.Hej.trans.XPathException("Supplied node must be built using the same or a compatible Configuration", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
                }
            }
            Saxon.Hej.type.TypeHierarchy th = contextObject.getConfiguration().getTypeHierarchy();
            if (!(contextItemType.matches(item, th))) {
                throw new Saxon.Hej.trans.XPathException(("Supplied context item does not match required context item type "+contextItemType));
            }
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator(item);
            contextObject.setCurrentIterator(iter);
            if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getSystemId() != null))) {
                Saxon.Hej.Controller controller = contextObject.getController();
                if ((controller != null)) {
                    Saxon.Hej.om.DocumentPool pool = controller.getDocumentPool();
                    Saxon.Hej.om.DocumentKey key = new Saxon.Hej.om.DocumentKey(((Saxon.Hej.om.NodeInfo)(item)).getSystemId());
                    if ((pool.find(key) == null)) {
                        pool.add(((Saxon.Hej.om.NodeInfo)(item)).getTreeInfo(), key);
                    }
                }
            }
        }
        public Saxon.Hej.om.Item getContextItem() {
            return contextObject.getContextItem();
        }
        public void setVariable(Saxon.Hej.sxpath.XPathVariable variable, Saxon.Hej.om.Sequence value) {
            Saxon.Hej.value.SequenceType requiredType = variable.getRequiredType();
            if ((requiredType != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                Saxon.Hej.trans.XPathException err = Saxon.Hej.expr.parser.TypeChecker.testConformance(value, requiredType, contextObject);
                if ((err != null)) {
                    throw err;
                }
            }
            Saxon.Hej.om.SequenceIterator iter = value.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                if (((item is Saxon.Hej.om.NodeInfo) && !(((Saxon.Hej.om.NodeInfo)(item)).getConfiguration().isCompatible(contextObject.getConfiguration())))) {
                    throw new Saxon.Hej.trans.XPathException("Supplied node must be built using the same or a compatible Configuration", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
                }
            }
            int slot = variable.getLocalSlotNumber();
            Saxon.Hej.om.StructuredQName expectedName = ((slot>=stackFrameMap.getNumberOfVariables()) ? null : stackFrameMap.getVariableMap()[slot]);
            if (!(variable.getVariableQName().Equals(expectedName))) {
                throw new Saxon.Hej.trans.XPathException("Supplied XPathVariable is bound to the wrong slot: perhaps it was created using a different static context");
            }
            contextObject.setLocalVariable(slot, value);
        }
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            contextObject.setResourceResolver(resolver);
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return contextObject.getResourceResolver();
        }
        public Saxon.Hej.lib.CollectionFinder getCollectionFinder() {
            return contextObject.getController().getCollectionFinder();
        }
        public void setCollectionFinder(Saxon.Hej.lib.CollectionFinder cf) {
            contextObject.getController().setCollectionFinder(cf);
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter listener) {
            contextObject.setErrorReporter(listener);
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return contextObject.getErrorReporter();
        }
        public Saxon.Hej.expr.XPathContext getXPathContextObject() {
            return contextObject;
        }
        public void setUnparsedTextURIResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            contextObject.getController().setUnparsedTextURIResolver(resolver);
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return contextObject.getController().getUnparsedTextURIResolver();
        }
        protected internal void checkExternalVariables(Saxon.Hej.expr.instruct.SlotManager stackFrameMap, int numberOfExternals) {
            Saxon.Hej.om.Sequence[] stack = contextObject.getStackFrame().getStackFrameValues();
            for (int i = 0; (i<numberOfExternals); i++) {
                if ((stack[i] == null)) {
                    Saxon.Hej.om.StructuredQName qname = stackFrameMap.getVariableMap()[i];
                    throw new Saxon.Hej.trans.XPathException(("No value has been supplied for variable $"+qname.getDisplayName()));
                }
            }
        }
    
    }
}