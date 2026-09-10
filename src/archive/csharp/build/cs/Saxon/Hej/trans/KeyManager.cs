using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class KeyManager {
        private readonly Saxon.Hej.expr.PackageData packageData;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.KeyDefinitionSet> keyDefinitions;
        private Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo,Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>>> docIndexes;
        public KeyManager(Saxon.Hej.Configuration config, Saxon.Hej.expr.PackageData pack)  {
            packageData = pack;
            keyDefinitions = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.trans.KeyDefinitionSet>(10);
            docIndexes = new Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo, Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>>>(10);
            registerIdrefKey(config);
        }
        private void registerIdrefKey(Saxon.Hej.Configuration config) { lock(this) {
            Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XS_IDREFS);
            if ((Saxon.Impl.Helpers.MapUtils.Get(keyDefinitions, qName) == null)) {
                Saxon.Hej.pattern.BasePatternWithPredicate pp = new Saxon.Hej.pattern.BasePatternWithPredicate(new Saxon.Hej.pattern.NodeTestPattern(new Saxon.Hej.pattern.MultipleNodeKindTest(Saxon.Hej.type.UType.ELEMENT_OR_ATTRIBUTE)), Saxon.Hej.functions.IntegratedFunctionLibrary.makeFunctionCall(new Saxon.Hej.functions.IsIdRef(), new Saxon.Hej.expr.Expression[]{}));
                try {
                    Saxon.Hej.sxpath.IndependentContext sc = new Saxon.Hej.sxpath.IndependentContext(config);
                    sc.setPackageData(packageData);
                    sc.setXPathLanguageLevel(31);
                    Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(sc);
                    Saxon.Hej.expr.Expression sf = Saxon.Hej.functions.SystemFunction.makeCall("string", rsc, new Saxon.Hej.expr.ContextItemExpression());
                    Saxon.Hej.expr.Expression use = Saxon.Hej.functions.SystemFunction.makeCall("tokenize", rsc, sf);
                    Saxon.Hej.trans.SymbolicName symbolicName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_KEY, qName);
                    Saxon.Hej.trans.KeyDefinition key = new Saxon.Hej.trans.KeyDefinition(symbolicName, pp, use, null, null);
                    key.setPackageData(packageData);
                    key.setIndexedItemType(Saxon.Hej.type.BuiltInAtomicType.STRING);
                    addKeyDefinition(qName, key, true, config);
                } catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Ejava.lang.AssertionError(err);
                }
            }
        }}
        public void preRegisterKeyDefinition(Saxon.Hej.om.StructuredQName keyName) { lock(this) {
            Saxon.Hej.trans.KeyDefinitionSet keySet = Saxon.Impl.Helpers.MapUtils.Get(keyDefinitions, keyName);
            if ((keySet == null)) {
                keySet = new Saxon.Hej.trans.KeyDefinitionSet(keyName, keyDefinitions.Count);
                Saxon.Impl.Helpers.MapUtils.Put(keyDefinitions, keyName, keySet);
            }
        }}
        public void addKeyDefinition(Saxon.Hej.om.StructuredQName keyName, Saxon.Hej.trans.KeyDefinition keydef, bool reusable, Saxon.Hej.Configuration config) { lock(this) {
            Saxon.Hej.trans.KeyDefinitionSet keySet = Saxon.Impl.Helpers.MapUtils.Get(keyDefinitions, keyName);
            if ((keySet == null)) {
                keySet = new Saxon.Hej.trans.KeyDefinitionSet(keyName, keyDefinitions.Count);
                Saxon.Impl.Helpers.MapUtils.Put(keyDefinitions, keyName, keySet);
            }
            keySet.addKeyDefinition(keydef);
            if (!(reusable)) {
                keySet.setReusable(false);
            }
            bool backwardsCompatible = keySet.isBackwardsCompatible();
            if (backwardsCompatible) {
                System.Collections.Generic.IList<Saxon.Hej.trans.KeyDefinition> v = keySet.getKeyDefinitions();
                foreach (Saxon.Hej.trans.KeyDefinition kd in v) {
                    kd.setBackwardsCompatible(true);
                    if (!(kd.getBody().getItemType().Equals(Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                        Saxon.Hej.expr.AtomicSequenceConverter exp = new Saxon.Hej.expr.AtomicSequenceConverter(kd.getBody(), Saxon.Hej.type.BuiltInAtomicType.STRING);
                        exp.allocateConverterStatically(config, false);
                        kd.setBody(exp);
                    }
                }
            }
        }}
        public Saxon.Hej.trans.KeyDefinitionSet getKeyDefinitionSet(Saxon.Hej.om.StructuredQName qName) {
            return Saxon.Impl.Helpers.MapUtils.Get(keyDefinitions, qName);
        }
        public Saxon.Hej.trans.KeyDefinitionSet findKeyDefinition(Saxon.Hej.pattern.Pattern finder, Saxon.Hej.expr.Expression use, string collationName) {
            foreach (Saxon.Hej.trans.KeyDefinitionSet keySet in keyDefinitions.Values) {
                if ((keySet.getKeyDefinitions().Count == 1)) {
                    foreach (Saxon.Hej.trans.KeyDefinition keyDef in keySet.getKeyDefinitions()) {
                        if (((keyDef.getMatch().isEqual(finder) && keyDef.getUse().isEqual(use)) && keyDef.getCollationName().Equals(collationName))) {
                            return keySet;
                        }
                    }
                }
            }
            return null;
        }
        private Saxon.Hej.trans.KeyIndex buildIndex(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) { lock(this) {
            Saxon.Hej.trace.Instrumentation.count("buildIndex");
            Saxon.Hej.trans.KeyIndex index = new Saxon.Hej.trans.KeyIndex(keySet.isRangeKey());
            index.buildIndex(keySet, doc, context);
            return index;
        }}
        private void buildIndex(Saxon.Hej.trans.KeyIndex index, Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) {
            index.buildIndex(keySet, doc, context);
        }
        public Saxon.Hej.om.SequenceIterator selectByKey(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.value.AtomicValue soughtValue, Saxon.Hej.expr.XPathContext context) {
            if ((soughtValue == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
            }
            if (keySet.isBackwardsCompatible()) {
                Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
                soughtValue = Saxon.Hej.type.Converter.convert(soughtValue, Saxon.Hej.type.BuiltInAtomicType.STRING, rules).asAtomic();
            } else {
                Saxon.Hej.type.BuiltInAtomicType itemType = soughtValue.getPrimitiveType();
                if (((itemType.Equals(Saxon.Hej.type.BuiltInAtomicType.INTEGER) || itemType.Equals(Saxon.Hej.type.BuiltInAtomicType.DECIMAL)) || itemType.Equals(Saxon.Hej.type.BuiltInAtomicType.FLOAT))) {
                    soughtValue = new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(soughtValue)).getDoubleValue());
                }
            }
            Saxon.Hej.trans.KeyIndex index = obtainIndex(keySet, doc, context);
            return index.getNodes(soughtValue);
        }
        public Saxon.Hej.om.SequenceIterator selectByCompositeKey(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.om.SequenceIterator soughtValue, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.KeyIndex index = obtainIndex(keySet, doc, context);
            return index.getComposite(soughtValue);
        }
        public Saxon.Hej.trans.KeyIndex obtainIndex(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) {
            if (keySet.isReusable()) {
                return obtainSharedIndex(keySet, doc, context);
            } else {
                return obtainLocalIndex(keySet, doc, context);
            }
        }
        private Saxon.Hej.trans.KeyIndex obtainSharedIndex(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.KeyIndex index;
            int keySetNumber = keySet.getKeySetNumber();
            index = getSharedIndex(doc, keySetNumber);
            if ((index != null)) {
                Saxon.Hej.trans.KeyIndex.Status status = index.getStatus();
                if ((status == /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.UNDER_CONSTRUCTION)) {
                    if (index.isCreatedInThisThread()) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Key definition "+keySet.getKeyName().getDisplayName())+" is circular"));
                        de.setXPathContext(context);
                        de.setErrorCode("XTDE0640");
                        throw de;
                    } else {
                        index = null;
                    }
                } else {
                    if ((status == /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.FAILED)) {
                        throw new Saxon.Hej.trans.XPathException((("Construction of index for key "+keySet.getKeyName().getDisplayName())+" was unsuccessful"));
                    }
                }
            }
            if ((index == null)) {
                index = new Saxon.Hej.trans.KeyIndex(keySet.isRangeKey());
                lock (this) {
                    index.setStatus(/*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.UNDER_CONSTRUCTION);
                    Saxon.Hej.trans.KeyIndex index2 = putSharedIndex(doc, keySetNumber, index, context);
                    if ((index2.getStatus() == /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.BUILT)) {
                        return index2;
                    } else {
                        index = index2;
                    }
                }
                buildIndex(index, keySet, doc, context);
                lock (this) {
                    index.setStatus(/*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.BUILT);
                    index = putSharedIndex(doc, keySetNumber, index, context);
                }
            }
            return index;
        }
        private Saxon.Hej.trans.KeyIndex obtainLocalIndex(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.KeyIndex index;
            int keySetNumber = keySet.getKeySetNumber();
            index = getLocalIndex(doc, keySetNumber, context);
            if ((index != null)) {
                Saxon.Hej.trans.KeyIndex.Status status = index.getStatus();
                if ((status == /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.UNDER_CONSTRUCTION)) {
                    if (index.isCreatedInThisThread()) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Key definition "+keySet.getKeyName().getDisplayName())+" is circular"));
                        de.setXPathContext(context);
                        de.setErrorCode("XTDE0640");
                        throw de;
                    } else {
                        index = null;
                    }
                } else {
                    if ((status == /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.FAILED)) {
                        throw new Saxon.Hej.trans.XPathException((("Construction of index for key "+keySet.getKeyName().getDisplayName())+" was unsuccessful"));
                    }
                }
            }
            if ((index == null)) {
                index = new Saxon.Hej.trans.KeyIndex(keySet.isRangeKey());
                lock (this) {
                    index.setStatus(/*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.UNDER_CONSTRUCTION);
                    Saxon.Hej.trans.KeyIndex index2 = putLocalIndex(doc, keySetNumber, index, context);
                    if ((index2.getStatus() == /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.BUILT)) {
                        return index2;
                    } else {
                        index = index2;
                    }
                }
                buildIndex(index, keySet, doc, context);
                lock (this) {
                    index.setStatus(/*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.BUILT);
                    index = putLocalIndex(doc, keySetNumber, index, context);
                }
            }
            return index;
        }
        private Saxon.Hej.trans.KeyIndex putSharedIndex(Saxon.Hej.om.TreeInfo doc, int keyFingerprint, Saxon.Hej.trans.KeyIndex index, Saxon.Hej.expr.XPathContext context) { lock(this) {
            if ((docIndexes == null)) {
                docIndexes = new Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo, Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>>>(10);
            }
            Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>> indexRef = docIndexes.get(doc);
            Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex> indexList;
            if (((indexRef == null) || (indexRef.get() == null))) {
                indexList = new Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>(10);
                Saxon.Hej.Controller controller = context.getController();
                if (controller.getDocumentPool().contains(doc)) {
                    context.getController().setUserData(doc, "saxon:key-index-list", indexList);
                } else {
                    doc.setUserData("saxon:key-index-list", indexList);
                }
                docIndexes.put(doc, new Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>>(indexList));
            } else {
                indexList = indexRef.get();
            }
            Saxon.Hej.trans.KeyIndex result = indexList.get(keyFingerprint);
            if (((result == null) || (result.getStatus() != /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.BUILT))) {
                indexList.put(keyFingerprint, index);
                result = index;
            }
            return result;
        }}
        private Saxon.Hej.trans.KeyIndex putLocalIndex(Saxon.Hej.om.TreeInfo doc, int keyFingerprint, Saxon.Hej.trans.KeyIndex index, Saxon.Hej.expr.XPathContext context) { lock(this) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.z.IntHashMap<System.Collections.Generic.IDictionary<System.Int64,Saxon.Hej.trans.KeyIndex>> masterIndex = controller.getLocalIndexes();
            System.Collections.Generic.IDictionary<System.Int64,Saxon.Hej.trans.KeyIndex> docIndexes = masterIndex.get(keyFingerprint);
            if ((docIndexes == null)) {
                docIndexes = new System.Collections.Generic.Dictionary<System.Int64, Saxon.Hej.trans.KeyIndex>();
                masterIndex.put(keyFingerprint, docIndexes);
            }
            Saxon.Hej.trans.KeyIndex result = Saxon.Impl.Helpers.MapUtils.Get(docIndexes, doc.getDocumentNumber());
            if (((result == null) || (result.getStatus() != /*explicit enum*/ Saxon.Hej.trans.KeyIndex.Status.BUILT))) {
                Saxon.Impl.Helpers.MapUtils.Put(docIndexes, doc.getDocumentNumber(), index);
                result = index;
            }
            return result;
        }}
        private Saxon.Hej.trans.KeyIndex getSharedIndex(Saxon.Hej.om.TreeInfo doc, int keyFingerprint) { lock(this) {
            if ((docIndexes == null)) {
                docIndexes = new Saxon.Ejava.util.WeakHashMap<Saxon.Hej.om.TreeInfo, Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>>>(10);
            }
            Saxon.Ejava.lang.@ref.WeakReference<Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex>> @ref = docIndexes.get(doc);
            if ((@ref == null)) {
                return null;
            }
            Saxon.Hej.z.IntHashMap<Saxon.Hej.trans.KeyIndex> indexList = @ref.get();
            if ((indexList == null)) {
                return null;
            }
            return indexList.get(keyFingerprint);
        }}
        private Saxon.Hej.trans.KeyIndex getLocalIndex(Saxon.Hej.om.TreeInfo doc, int keyFingerprint, Saxon.Hej.expr.XPathContext context) { lock(this) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.z.IntHashMap<System.Collections.Generic.IDictionary<System.Int64,Saxon.Hej.trans.KeyIndex>> masterIndex = controller.getLocalIndexes();
            System.Collections.Generic.IDictionary<System.Int64,Saxon.Hej.trans.KeyIndex> docIndexes = masterIndex.get(keyFingerprint);
            if ((docIndexes == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(docIndexes, doc.getDocumentNumber());
            }
        }}
        public void clearDocumentIndexes(Saxon.Hej.om.TreeInfo doc) { lock(this) {
            docIndexes.remove(doc);
        }}
        public System.Collections.Generic.ICollection<Saxon.Hej.trans.KeyDefinitionSet> getAllKeyDefinitionSets() {
            return keyDefinitions.Values;
        }
        public int getNumberOfKeyDefinitions() {
            return keyDefinitions.Count;
        }
        public void exportKeys(Saxon.Hej.trace.ExpressionPresenter @out, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap) {
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.trans.KeyDefinitionSet> e in keyDefinitions) {
                bool reusable = e.Value.isReusable();
                System.Collections.Generic.IList<Saxon.Hej.trans.KeyDefinition> list = e.Value.getKeyDefinitions();
                foreach (Saxon.Hej.trans.KeyDefinition kd in list) {
                    if (!(kd.getObjectName().Equals(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XS_IDREFS)))) {
                        kd.export(@out, reusable, componentIdMap);
                    }
                }
            }
        }
    
    }
}