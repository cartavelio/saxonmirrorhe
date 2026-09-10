using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class KeyIndex {
        public enum Status {
            UNDER_CONSTRUCTION,
            BUILT,
            FAILED,
        }
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object> index;
        private Saxon.Hej.type.UType keyTypesPresent = Saxon.Hej.type.UType.VOID;
        private readonly Saxon.Hej.type.UType keyTypesConvertedFromUntyped = Saxon.Hej.type.UType.STRING_LIKE;
        private System.Collections.Generic.IList<Saxon.Hej.value.StringValue> untypedKeys;
        private Saxon.Hej.lib.ConversionRules rules;
        private int implicitTimezone;
        private Saxon.Hej.lib.StringCollator collation;
        private readonly long creatingThread;
        private Saxon.Hej.trans.KeyIndex.Status status;
        public KeyIndex(bool isRangeKey)  {
            if (isRangeKey) {
                index = new System.Collections.Generic.SortedDictionary<Saxon.Hej.expr.sort.AtomicMatchKey, System.Object>();
            } else {
                index = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, System.Object>(100);
            }
            creatingThread = Saxon.Ejava.lang.Thread.currentThread().getId();
            status = Saxon.Hej.trans.KeyIndex.Status.UNDER_CONSTRUCTION;
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object> getUnderlyingMap() {
            return index;
        }
        public bool isCreatedInThisThread() {
            return (creatingThread == Saxon.Ejava.lang.Thread.currentThread().getId());
        }
        public Saxon.Hej.trans.KeyIndex.Status getStatus() {
            return status;
        }
        public void setStatus(Saxon.Hej.trans.KeyIndex.Status status) {
            this.status = status;
        }
        public void buildIndex(Saxon.Hej.trans.KeyDefinitionSet keySet, Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.trans.KeyDefinition> definitions = keySet.getKeyDefinitions();
            for (int k = 0; (k<definitions.Count); k++) {
                constructIndex(doc, definitions[k], context, (k == 0));
            }
            this.rules = context.getConfiguration().getConversionRules();
            this.implicitTimezone = context.getImplicitTimezone();
            this.collation = definitions[0].getCollation();
        }
        private void constructIndex(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.trans.KeyDefinition keydef, Saxon.Hej.expr.XPathContext context, bool isFirst) {
            Saxon.Hej.pattern.Pattern match = keydef.getMatch();
            Saxon.Hej.expr.XPathContextMajor xc = context.newContext();
            xc.setOrigin(keydef);
            xc.setCurrentComponent(keydef.getDeclaringComponent());
            xc.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_KEY);
            Saxon.Hej.expr.instruct.SlotManager map = keydef.getStackFrameMap();
            if ((map != null)) {
                xc.openStackFrame(map);
            }
            Saxon.Hej.om.SequenceTool.supply(match.selectNodes(doc, xc), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((node => processNode((Saxon.Hej.om.NodeInfo)(node), keydef, xc, isFirst))));
        }
        private void processNode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.trans.KeyDefinition keydef, Saxon.Hej.expr.XPathContext xc, bool isFirst) {
            Saxon.Hej.tree.iter.ManualIterator si = new Saxon.Hej.tree.iter.ManualIterator(node);
            xc.setCurrentIterator(si);
            Saxon.Hej.lib.StringCollator collation = keydef.getCollation();
            int implicitTimezone = xc.getImplicitTimezone();
            Saxon.Hej.expr.Expression use = keydef.getUse();
            Saxon.Hej.om.SequenceIterator useval = use.iterate(xc);
            if (keydef.isComposite()) {
                System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> amks = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>(4);
                Saxon.Hej.om.SequenceTool.supply(useval, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((keyVal => amks.Add(getCollationKey((Saxon.Hej.value.AtomicValue)(keyVal), collation, implicitTimezone)))));
                addEntry(new Saxon.Hej.trans.KeyIndex.CompositeAtomicMatchKey(amks), node, isFirst);
            } else {
                Saxon.Hej.value.AtomicValue keyVal;
                while (((keyVal = (Saxon.Hej.value.AtomicValue)(useval.next())) != null)) {
                    if (keyVal.isNaN()) {
                        continue ;
                    }
                    Saxon.Hej.type.UType actualUType = keyVal.getUType();
                    if (!(keyTypesPresent.subsumes(actualUType))) {
                        keyTypesPresent = keyTypesPresent.union(actualUType);
                    }
                    Saxon.Hej.expr.sort.AtomicMatchKey amk = getCollationKey(keyVal, collation, implicitTimezone);
                    if ((actualUType.Equals(Saxon.Hej.type.UType.UNTYPED_ATOMIC) && keydef.isConvertUntypedToOther())) {
                        if ((untypedKeys == null)) {
                            untypedKeys = new System.Collections.Generic.List<Saxon.Hej.value.StringValue>(20);
                        }
                        untypedKeys.Add((Saxon.Hej.value.StringValue)(keyVal));
                    }
                    addEntry(amk, node, isFirst);
                }
            }
        }
        private void addEntry(Saxon.Hej.expr.sort.AtomicMatchKey val, Saxon.Hej.om.NodeInfo curr, bool isFirst) {
            System.Object value = Saxon.Impl.Helpers.MapUtils.Get(index, val);
            if ((value == null)) {
                Saxon.Impl.Helpers.MapUtils.Put(index, val, curr);
            } else {
                System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> nodes;
                if ((value is Saxon.Hej.om.NodeInfo)) {
                    nodes = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(4);
                    nodes.Add((Saxon.Hej.om.NodeInfo)(value));
                    Saxon.Impl.Helpers.MapUtils.Put(index, val, nodes);
                } else {
                    nodes = (System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>)(value);
                }
                if (isFirst) {
                    if ((nodes[(nodes.Count-1)] != curr)) {
                        nodes.Add(curr);
                    }
                } else {
                    Saxon.Hej.expr.sort.LocalOrderComparer comparer = Saxon.Hej.expr.sort.LocalOrderComparer.getInstance();
                    bool found = false;
                    for (int i = (nodes.Count-1); (i>=0); i--) {
                        int d = comparer.compare(curr, nodes[i]);
                        if ((d>=0)) {
                            if ((d == 0)) {
                            } else {
                                nodes.Insert((i+1), curr);
                            }
                            found = true;
                            break ;
                        }
                    }
                    if (!(found)) {
                        nodes.Insert(0, curr);
                    }
                }
            }
        }
        public void reindexUntypedValues(Saxon.Hej.type.BuiltInAtomicType type) {
            Saxon.Hej.type.UType uType = type.getUType();
            if (Saxon.Hej.type.UType.STRING_LIKE.subsumes(uType)) {
                return ;
            }
            if (Saxon.Hej.type.UType.NUMERIC.subsumes(uType)) {
                type = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
            Saxon.Hej.type.StringConverter converter = type.getStringConverter(rules);
            foreach (Saxon.Hej.value.StringValue v in untypedKeys) {
                Saxon.Hej.expr.sort.AtomicMatchKey uk = getCollationKey(v, collation, implicitTimezone);
                Saxon.Hej.value.AtomicValue convertedValue = converter.convertString(v.getUnicodeStringValue()).asAtomic();
                Saxon.Hej.expr.sort.AtomicMatchKey amk = getCollationKey(convertedValue, collation, implicitTimezone);
                System.Object value = Saxon.Impl.Helpers.MapUtils.Get(index, uk);
                if ((value is Saxon.Hej.om.NodeInfo)) {
                    addEntry(amk, ((Saxon.Hej.om.NodeInfo)(value)), false);
                } else {
                    System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> nodes = (System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>)(value);
                    foreach (Saxon.Hej.om.NodeInfo node in nodes) {
                        addEntry(amk, node, false);
                    }
                }
            }
        }
        public bool isEmpty() {
            return index.Count==0;
        }
        public Saxon.Hej.om.SequenceIterator getNodes(Saxon.Hej.value.AtomicValue soughtValue) {
            if (((untypedKeys != null) && !(keyTypesConvertedFromUntyped.subsumes(soughtValue.getUType())))) {
                reindexUntypedValues(soughtValue.getPrimitiveType());
            }
            System.Object value = Saxon.Impl.Helpers.MapUtils.Get(index, getCollationKey(soughtValue, collation, implicitTimezone));
            return entryIterator(value);
        }
        private Saxon.Hej.om.SequenceIterator entryIterator(System.Object value) {
            if ((value == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
            } else {
                if ((value is Saxon.Hej.om.NodeInfo)) {
                    return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator((Saxon.Hej.om.NodeInfo)(value));
                } else {
                    System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> nodes = (System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo>)(value);
                    return new Saxon.Hej.tree.iter.NodeListIterator(nodes);
                }
            }
        }
        public Saxon.Hej.om.SequenceIterator getComposite(Saxon.Hej.om.SequenceIterator soughtValue) {
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> amks = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>(4);
            Saxon.Hej.om.SequenceTool.supply(soughtValue, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((keyVal => amks.Add(getCollationKey((Saxon.Hej.value.AtomicValue)(keyVal), collation, implicitTimezone)))));
            System.Object value = Saxon.Impl.Helpers.MapUtils.Get(index, new Saxon.Hej.trans.KeyIndex.CompositeAtomicMatchKey(amks));
            return entryIterator(value);
        }
        private static Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.StringCollator collation, int implicitTimezone) {
            if (Saxon.Hej.type.UType.STRING_LIKE.subsumes(value.getUType())) {
                if ((collation == null)) {
                    return value.getUnicodeStringValue().tidy();
                } else {
                    return collation.getCollationKey(value.getUnicodeStringValue());
                }
            } else {
                return value.getXPathMatchKey(collation, implicitTimezone);
            }
        }
        private class CompositeAtomicMatchKey : Saxon.Hej.expr.sort.AtomicMatchKey {
            private readonly System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> keys;
            public CompositeAtomicMatchKey(System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> keys)  {
                this.keys = keys;
            }
            public Saxon.Hej.value.AtomicValue asAtomic() {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            public override bool Equals(System.Object obj) {
                if (((obj is Saxon.Hej.trans.KeyIndex.CompositeAtomicMatchKey) && (((Saxon.Hej.trans.KeyIndex.CompositeAtomicMatchKey)(obj)).keys.Count == keys.Count))) {
                    System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> keys2 = ((Saxon.Hej.trans.KeyIndex.CompositeAtomicMatchKey)(obj)).keys;
                    for (int i = 0; (i<keys.Count); i++) {
                        if (!(keys[i].Equals(keys2[i]))) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            public override int GetHashCode() {
                int h = 0x1ab27cd6;
                foreach (Saxon.Hej.expr.sort.AtomicMatchKey amk in keys) {
                    h ^= amk.GetHashCode();
                    h = (h<<1);
                }
                return h;
            }
        
        }
    
    }
}