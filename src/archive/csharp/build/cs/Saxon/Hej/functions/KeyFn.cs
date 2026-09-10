using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class KeyFn : Saxon.Hej.functions.SystemFunction, Saxon.Hej.functions.StatefulSystemFunction {
        private Saxon.Hej.trans.KeyDefinitionSet staticKeySet = null;
        public Saxon.Hej.trans.KeyManager getKeyManager() {
            return getRetainedStaticContext().getPackageData().getKeyManager();
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return getRetainedStaticContext();
        }
        public static Saxon.Hej.expr.Expression internalKeyCall(Saxon.Hej.trans.KeyManager keyManager, Saxon.Hej.trans.KeyDefinitionSet keySet, string name, Saxon.Hej.expr.Expression value, Saxon.Hej.expr.Expression doc, Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            Saxon.Hej.functions.KeyFn fn = (Saxon.Hej.functions.KeyFn)(Saxon.Hej.functions.SystemFunction.makeFunction("key", rsc, 3));
            System.Diagnostics.Debug.Assert((fn != null));
            fn.staticKeySet = keySet;
            try {
                fn.fixArguments(new Saxon.Hej.expr.StringLiteral(name), value, doc);
            } catch (Saxon.Hej.trans.XPathException e) {
            }
            return fn.makeFunctionCall(new Saxon.Hej.expr.StringLiteral(name), value, doc);
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int prop = ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            if ((((getArity() == 2)) || (((arguments[2].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0))) {
                prop |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            return prop;
        }
        public Saxon.Hej.functions.SystemFunction copy() {
            Saxon.Hej.functions.KeyFn k2 = (Saxon.Hej.functions.KeyFn)(Saxon.Hej.functions.SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity()));
            k2.staticKeySet = staticKeySet;
            return k2;
        }
        public override Saxon.Hej.expr.Expression fixArguments(params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments[0] is Saxon.Hej.expr.StringLiteral) && (staticKeySet == null))) {
                Saxon.Hej.trans.KeyManager keyManager = getKeyManager();
                string keyName = ((Saxon.Hej.expr.StringLiteral)(arguments[0])).stringify();
                staticKeySet = getKeyDefinitionSet(keyManager, keyName);
            }
            return null;
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if ((staticKeySet != null)) {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
                foreach (Saxon.Hej.trans.KeyDefinition kd in staticKeySet.getKeyDefinitions()) {
                    Saxon.Hej.pattern.Pattern pat = kd.getMatch();
                    if ((pat is Saxon.Hej.pattern.NodeSetPattern)) {
                        Saxon.Hej.expr.Expression selector = ((Saxon.Hej.pattern.NodeSetPattern)(pat)).getSelectionExpression();
                        Saxon.Hej.expr.parser.PathMap.PathMapNodeSet selected = selector.addToPathMap(pathMap, pathMapNodeSet);
                        Saxon.Hej.expr.Expression use = kd.getUse();
                        Saxon.Hej.expr.parser.PathMap.PathMapNodeSet used = use.addToPathMap(pathMap, selected);
                        result.addNodeSet(selected);
                    } else {
                        throw new Saxon.Ejava.lang.IllegalStateException("Can't add key() call to pathmap");
                    }
                }
                return result;
            } else {
                throw new Saxon.Ejava.lang.IllegalStateException("Can't add dynamic key() call to pathmap");
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo origin;
            if ((arguments.Length == 3)) {
                origin = (Saxon.Hej.om.NodeInfo)(getOrigin(context, arguments[2]));
            } else {
                origin = getContextRoot(context);
            }
            if ((origin.getRoot().getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                throw new Saxon.Hej.trans.XPathException((("In the key() function,"+" the node supplied in the third argument (or the context node if absent)")+" must be in a tree whose root is a document node"), "XTDE1270", context);
            }
            Saxon.Hej.trans.KeyDefinitionSet selectedKeySet = staticKeySet;
            Saxon.Hej.trans.KeyManager keyManager = getKeyManager();
            if ((selectedKeySet == null)) {
                selectedKeySet = getKeyDefinitionSet(keyManager, arguments[0].head().getStringValue());
            }
            return search(keyManager, context, arguments[1], origin, selectedKeySet);
        }
        private static Saxon.Hej.om.NodeInfo getContextRoot(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item contextItem = context.getContextItem();
            if ((contextItem == null)) {
                throw new Saxon.Hej.trans.XPathException("Cannot call the key() function when there is no context item", "XTDE1270", context);
            } else {
                if (!(((contextItem is Saxon.Hej.om.NodeInfo)))) {
                    throw new Saxon.Hej.trans.XPathException("Cannot call the key() function when the context item is not a node", "XTDE1270", context);
                }
            }
            return ((Saxon.Hej.om.NodeInfo)(contextItem)).getRoot();
        }
        private static Saxon.Hej.om.Item getOrigin(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence argument2) {
            Saxon.Hej.om.Item arg2;
            try {
                arg2 = argument2.head();
            } catch (Saxon.Hej.trans.XPathException e) {
                string code = e.getErrorCodeLocalPart();
                if (("XPDY0002".Equals(code) && (argument2 is Saxon.Hej.expr.RootExpression))) {
                    throw new Saxon.Hej.trans.XPathException("Cannot call the key() function when there is no context node", "XTDE1270", context);
                } else {
                    if ("XPDY0050".Equals(code)) {
                        throw new Saxon.Hej.trans.XPathException((("In the key() function,"+" the node supplied in the third argument (or the context node if absent)")+" must be in a tree whose root is a document node"), "XTDE1270", context);
                    } else {
                        if (("XPTY0020".Equals(code) || "XPTY0019".Equals(code))) {
                            throw new Saxon.Hej.trans.XPathException("Cannot call the key() function when the context item is an atomic value", "XTDE1270", context);
                        }
                    }
                }
                throw;
            }
            return arg2;
        }
        private Saxon.Hej.trans.KeyDefinitionSet getKeyDefinitionSet(Saxon.Hej.trans.KeyManager keyManager, string keyName) {
            Saxon.Hej.trans.KeyDefinitionSet selectedKeySet;
            Saxon.Hej.om.StructuredQName qName = null;
            try {
                qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(keyName, false, true, getNamespaceResolver());
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.trans.XPathException(("Invalid key name: "+err.Message), "XTDE1260");
            }
            selectedKeySet = keyManager.getKeyDefinitionSet(qName);
            if ((selectedKeySet == null)) {
                throw new Saxon.Hej.trans.XPathException((("Key '"+keyName)+"' has not been defined"), "XTDE1260");
            }
            return selectedKeySet;
        }
        protected internal static Saxon.Hej.om.Sequence search(Saxon.Hej.trans.KeyManager keyManager, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence sought, Saxon.Hej.om.NodeInfo origin, Saxon.Hej.trans.KeyDefinitionSet selectedKeySet) {
            Saxon.Hej.om.NodeInfo doc = origin.getRoot();
            if (selectedKeySet.isComposite()) {
                Saxon.Hej.om.SequenceIterator soughtKey = sought.iterate();
                Saxon.Hej.om.SequenceIterator all = keyManager.selectByCompositeKey(selectedKeySet, doc.getTreeInfo(), soughtKey, context);
                if (origin.Equals(doc)) {
                    return new Saxon.Hej.om.LazySequence(all);
                }
                return new Saxon.Hej.om.LazySequence(Saxon.Hej.expr.ItemMappingIterator.filter(all, item => Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(origin, (Saxon.Hej.om.NodeInfo)(item))));
            } else {
                Saxon.Hej.om.SequenceIterator allResults = null;
                if ((sought is Saxon.Hej.value.AtomicValue)) {
                    Saxon.Hej.om.SequenceIterator results = keyManager.selectByKey(selectedKeySet, doc.getTreeInfo(), (Saxon.Hej.value.AtomicValue)(sought), context);
                    if ((results is Saxon.Hej.tree.iter.EmptyIterator)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        if ((results is Saxon.Hej.tree.iter.SingletonIterator)) {
                            Saxon.Hej.om.NodeInfo result = (Saxon.Hej.om.NodeInfo)(results.next());
                            if ((doc.Equals(origin) || Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(origin, result))) {
                                return result;
                            } else {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                        } else {
                            if (doc.Equals(origin)) {
                                return new Saxon.Hej.om.LazySequence(results);
                            } else {
                                new Saxon.Hej.om.LazySequence(Saxon.Hej.expr.ItemMappingIterator.filter(results, item => Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(origin, (Saxon.Hej.om.NodeInfo)(item))));
                            }
                        }
                    }
                }
                Saxon.Hej.om.SequenceIterator keys = sought.iterate();
                Saxon.Hej.value.AtomicValue keyValue;
                System.Collections.Generic.IList<Saxon.Hej.om.SequenceIterator> allKeyIterators = new System.Collections.Generic.List<Saxon.Hej.om.SequenceIterator>();
                while (((keyValue = (Saxon.Hej.value.AtomicValue)(keys.next())) != null)) {
                    Saxon.Hej.om.SequenceIterator someResults = keyManager.selectByKey(selectedKeySet, doc.getTreeInfo(), keyValue, context);
                    allKeyIterators.Add(someResults);
                }
                if (allKeyIterators.Count==0) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    if ((allKeyIterators.Count == 1)) {
                        allResults = allKeyIterators[0];
                    } else {
                        allResults = new Saxon.Hej.expr.UnionIterator(allKeyIterators, Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
                    }
                }
                if (origin.Equals(doc)) {
                    return new Saxon.Hej.om.LazySequence(allResults);
                }
                return new Saxon.Hej.om.LazySequence(Saxon.Hej.expr.ItemMappingIterator.filter(allResults, item => Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(origin, (Saxon.Hej.om.NodeInfo)(item))));
            }
        }
    
    }
}