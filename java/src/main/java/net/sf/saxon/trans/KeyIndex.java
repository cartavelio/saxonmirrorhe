////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.trans;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.sort.*;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.transpile.CSharpSimpleEnum;
import net.sf.saxon.transpile.CSharpUsing;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.tree.jiter.IterableUsingIteratorSupplier;
import net.sf.saxon.tree.jiter.MappingJavaIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.util.*;

/**
 * A key index is an index maintained to support xsl:key key definitions, including both user-defined
 * keys and keys added by the optimizer. Each key index supports one key definition (a set of xsl:key
 * declarations with the same name) applied to one document tree.
 *
 * <p>The index can support a mixture of atomic keys of different types; there is no error caused by comparing
 * values of different types. This relies on the fact that XPathComparable values are identical for comparable
 * values (e.g. integers and doubles), and distinct otherwise.</p>
 *
 * <p>With the XSLT xsl:key construct, untypedAtomic values are treated as strings. However, this structure is
 * also used to support internally-generated keys with general comparison semantics, where untyped values
 * are converted to the type of the other operand. To enable this to work, we maintain a list of all
 * untypedAtomic keys present in the index; and if a search is made for some type like xs:date, we then go
 * through this list converting each untypedAtomic value to a date and indexing it as such. In principle this
 * can happen for an arbitrary number of data types, though it is unlikely in practice because not many
 * types have overlapping lexical spaces.</p>
 */
@CSharpUsing(code="Saxon.Hej.type")
public class KeyIndex {

    @CSharpSimpleEnum
    public enum Status {UNDER_CONSTRUCTION, BUILT, FAILED}

    // The entry in an index is either a NodeInfo or a List<NodeInfo>
    private final Map <AtomicMatchKey, Object> index;
    private UType keyTypesPresent = UType.VOID;
    private UType keyTypesConvertedFromUntyped = UType.STRING_LIKE;
    private Set<UnicodeString> untypedKeys;
    private ConversionRules rules;
    private int implicitTimezone;
    private StringCollator collation;
    private final long creatingThread;
    private Status status;
    private AtomicMatchKeyMaker matchKeyMaker;

    public KeyIndex(boolean isRangeKey) {
        if (isRangeKey) {
            index = new TreeMap<>();     // TODO: should be using an XPathComparable here, for sorting purposes?
        } else {
            index = new HashMap<>(100);
        }
        creatingThread = Thread.currentThread().getId();
        status = Status.UNDER_CONSTRUCTION;
        //Instrumentation.count("Building KeyIndex");
    }

    /**
     * Get the underlying map
     * @return the underlying map. The "Object" in the map entry is either a {@code NodeInfo}
     * or a {@code List<NodeInfo>}
     */

    public Map <AtomicMatchKey, Object> getUnderlyingMap() {
        return index;
    }

    /**
     * Get the contents of the index as a collection of key-value pairs, to support
     * the XSLT 4.0 map-for-key() function
     * @return an iterable set of key value pairs
     */
    @CSharpReplaceBody(code="return keyValuePairList();")
    public Iterable<KeyValuePair> keyValuePairs() {
        Set<Map.Entry<AtomicMatchKey, Object>> entries = index.entrySet();
        return new IterableUsingIteratorSupplier<>(() -> {
            Iterator<Map.Entry<AtomicMatchKey, Object>> baseIterator = entries.iterator();
            return new MappingJavaIterator<>(baseIterator, e -> makeKVP(e.getKey(), e.getValue()));
        });
    }

    private static KeyValuePair makeKVP(AtomicMatchKey matchKey, Object rawValue) {
        GroundedValue value;
        if (rawValue instanceof NodeInfo node) {
            value = node;
        } else {
            value = SequenceExtent.makeSequenceExtent((List<? extends Item>) rawValue);
        }
        AtomicValue key;
        if (matchKey instanceof UnicodeString str) {
            key = new StringValue(str);
        } else {
            key = matchKey.asAtomic();
        }
        return new KeyValuePair(key, value);
    }

    // simpler implementation for transpiling to C#
    public List<KeyValuePair> keyValuePairList() {
        List<KeyValuePair> result = new ArrayList<>();
        for (AtomicMatchKey key : index.keySet()) {
            result.add(makeKVP(key, index.get(key)));
        }
        return result;
    }

    /**
     * Ask if the index was created in the current thread
     * @return true if this index was created in this thread
     */

    public boolean isCreatedInThisThread() {
        return creatingThread == Thread.currentThread().getId();
    }

    /**
     * Ask if the index is under construction
     * @return true if the index is still under construction
     */

    public Status getStatus() {
        return status;
    }

    /**
     * Say whether the index is under construction
     * @param status the status of the index, for example {@link Status#UNDER_CONSTRUCTION}
     */

    public void setStatus(Status status) {
        this.status = status;
    }

    /**
     * Build the index for a particular document for a named key
     *
     *
     * @param keySet         The set of key definitions with this name
     * @param doc            The source document in question
     * @param context        The dynamic context
     * @throws XPathException if a dynamic error is encountered
     */




    public void buildIndex(KeyDefinitionSet keySet,
                           TreeInfo doc,
                           XPathContext context) throws XPathException {

        List<KeyDefinition> definitions = keySet.getKeyDefinitions();

        // There may be multiple xsl:key definitions with the same name. Index them all.
        for (int k = 0; k < definitions.size(); k++) {
            constructIndex(doc, definitions.get(k), context, k == 0);
        }
        this.rules = context.getConfiguration().getConversionRules();
        this.implicitTimezone = context.getImplicitTimezone();
        this.collation = definitions.get(0).getCollation();
        this.matchKeyMaker = keySet.getMatchKeyMaker(context.getImplicitTimezone());
    }

    /**
     * Process one key definition to add entries to the index
     *
     *
     * @param doc            the document to be indexed
     * @param keydef         the key definition used to build the index
     * @param context        the XPath dynamic evaluation context
     * @param isFirst        true if this is the first index to be built for this key
     * @throws XPathException if a dynamic error is encountered
     */

    private void constructIndex(TreeInfo doc,
                                KeyDefinition keydef,
                                XPathContext context,
                                boolean isFirst) throws XPathException {
        //System.err.println("build index for doc " + doc.getDocumentNumber());
        Pattern match = keydef.getMatch();

        //NodeInfo curr;
        XPathContextMajor xc = context.newContext();
        xc.setOrigin(keydef);
        xc.setCurrentComponent(keydef.getDeclaringComponent());
        xc.setTemporaryOutputState(StandardNames.XSL_KEY);

        // The use expression (or sequence constructor) may contain local variables.
        SlotManager map = keydef.getStackFrameMap();
        if (map != null) {
            xc.openStackFrame(map);
        }

        int version = keydef.getXPathVersion();
        StringCollator collator = keydef.getCollation();
        AtomicMatchKeyMaker keyMaker = keydef.getMatchKeyMaker(context.getImplicitTimezone());

        SequenceTool.supply(match.selectNodes(doc, xc),
                            (ItemConsumer<? super Item>) node ->
                                    processNode((NodeInfo) node, keydef, keyMaker, xc, isFirst));

    }

    /**
     * Process one matching node, adding entries to the index if appropriate
     *
     *
     * @param node           the node being processed
     * @param keydef         the key definition
     * @param keyMaker       function for computing atomic match keys
     * @param xc             the context for evaluating expressions
     * @param isFirst        indicates whether this is the first key definition with a given key name (which means
     *                       no sort of the resulting key entries is required)
     * @throws XPathException if a dynamic error is encountered
     */

    private void processNode(NodeInfo node,
                             KeyDefinition keydef,
                             AtomicMatchKeyMaker keyMaker,
                             XPathContext xc,
                             boolean isFirst) throws XPathException {


        // Make the node we are testing the context node,
        // with context position and context size set to 1

        ManualIterator si = new ManualIterator(node);
        xc.setCurrentIterator(si);

        // Evaluate the "use" expression against this context node

        PullEvaluator use = keydef.obtainUseEvaluator();
        SequenceIterator useval = use.iterate(xc);
        if (keydef.isComposite()) {
            List<AtomicMatchKey> amks = new ArrayList<>(4);
            SequenceTool.supply(useval, (ItemConsumer<? super Item>) keyVal ->
                    amks.add(keyMaker.makeMatchKey((AtomicValue) keyVal)));
            addEntry(new CompositeAtomicMatchKey(amks), node, isFirst);
        } else {
            AtomicValue keyVal;
            while ((keyVal = (AtomicValue) useval.next()) != null) {
                if (keyVal.isNaN()) {
                    continue;
                }
                UType actualUType = keyVal.getUType();
                if (!keyTypesPresent.subsumes(actualUType)) {
                    keyTypesPresent = keyTypesPresent.union(actualUType);
                }
                AtomicMatchKey amk = keyMaker.makeMatchKey(keyVal);
                if (actualUType.equals(UType.UNTYPED_ATOMIC) && keydef.isConvertUntypedToOther()) {
                    if (untypedKeys == null) {
                        untypedKeys = new HashSet<>(20);
                    }
                    untypedKeys.add(keyVal.getUnicodeStringValue());
                }
                addEntry(amk, node, isFirst);
            }
        }

    }

    private void addEntry(AtomicMatchKey val, NodeInfo curr, boolean isFirst) {
        Object value = index.get(val);
        if (value == null) {
            // this is the first node with this key value; we store the entry as a singleton
            // node to avoid the overhead of creating a list
            index.put(val, curr);
        } else {
            List<NodeInfo> nodes;
            if (value instanceof NodeInfo) {
                // replace the singleton key entry with a list-valued key entry
                nodes = new ArrayList<>(4);
                nodes.add((NodeInfo)value);
                index.put(val, nodes);
            } else {
                nodes = (List<NodeInfo>)value;
            }
            // this is not the first node with this key value.
            // add the node to the list of nodes for this key,
            // unless it's already there
            if (isFirst) {
                // if this is the first index definition that we're processing,
                // then this node must be after all existing nodes in document
                // order, or the same node as the last existing node
                if (nodes.get(nodes.size() - 1) != curr) {
                    nodes.add(curr);
                }
            } else {
                // otherwise, we need to insert the node at the correct
                // position in document order. This code does an insertion sort:
                // not ideal for performance, but it's very unusual to have more than
                // one key definition for a key. We start looking at the end because
                // it's most likely that the new node will come after all the others.
                // See bug 2092 in saxonica.plan.io
                LocalOrderComparer comparer = LocalOrderComparer.getInstance();
                boolean found = false;
                for (int i=nodes.size()-1; i>=0; i--) {
                    int d = comparer.compare(curr, nodes.get(i));
                    if (d>=0) {
                        if (d==0) {
                            // node already in list; do nothing
                        } else {
                            // add the node at this position
                            nodes.add(i+1, curr);
                        }
                        found = true;
                        break;
                    }
                    // else continue round the loop
                }
                // if we're still here, add the new node at the start
                if (!found) {
                    nodes.add(0, curr);
                }
            }
        }
    }

    /**
     * Re-index untyped atomic values after conversion to a specific type. This
     * happens when the "convertUntypedToOther" option is set (typically because this
     * index is used to support a general comparison), and the sought value is of a type
     * other that string or untyped atomic. We go through the index finding all untyped
     * atomic values, converting each one to the sought type, and adding it to the index under
     * this type.
     * @param type the type to which untyped atomic values should be converted
     * @throws XPathException if conversion of any untyped atomic value to the requested key type fails
     */

    public void reindexUntypedValues(BuiltInAtomicType type) throws XPathException {
        UType uType = type.getUType();
        if (UType.STRING_LIKE.subsumes(uType)) {
            return;
        }
        if (UType.NUMERIC.subsumes(uType)) {
            type = BuiltInAtomicType.DOUBLE;
        }
        StringConverter converter = type.getStringConverter(rules);
        for (UnicodeString v : untypedKeys) {
            AtomicMatchKey uk = matchKeyMaker.makeMatchKey(new StringValue(v));
            AtomicValue convertedValue = converter.convertString(v).asAtomic();
            AtomicMatchKey amk = matchKeyMaker.makeMatchKey(convertedValue);
            Object value = index.get(uk);
            if (value instanceof NodeInfo) {
                addEntry(amk, ((NodeInfo)value), false);
            } else {
                List<NodeInfo> nodes = (List<NodeInfo>)value;
                for (NodeInfo node : nodes) {
                    addEntry(amk, node, false);
                }
            }
        }

    }

    /**
     * Ask whether the index is empty
     * @return true if the index is empty
     */

    public boolean isEmpty() {
        return index.isEmpty();
    }

    /**
     * Get the nodes with a given key value
     *
     * @param soughtValue The required key value
     * @return an iterator over the selected nodes, always in document order with no duplicates
     * @throws XPathException if a dynamic error is encountered
     */

    public SequenceIterator getNodes(AtomicValue soughtValue) throws XPathException {
        if (untypedKeys != null && !keyTypesConvertedFromUntyped.subsumes(soughtValue.getUType())) {
            reindexUntypedValues(soughtValue.getPrimitiveType());
            keyTypesConvertedFromUntyped = keyTypesConvertedFromUntyped.union(soughtValue.getPrimitiveType().getUType());
        }
        if (soughtValue.isUntypedAtomic()) {
            List<NodeInfo> resultNodes = new ArrayList<>();
            int counter = 0;
            for (PrimitiveUType type : keyTypesPresent.decompose()) {
                AtomicType targetType = (AtomicType) UType.toItemType(type);
                AtomicValue converted = Converter.convert(soughtValue, targetType, rules);
                Object value = index.get(matchKeyMaker.makeMatchKey(converted));
                if (value != null) {
                    counter++;
                    if (value instanceof NodeInfo) {
                        resultNodes.add(((NodeInfo) value));
                    } else {
                        resultNodes.addAll((List<NodeInfo>) value);
                    }
                }
            }
            SequenceIterator result = new ListIterator.Of<>(resultNodes);
            if (counter > 1) {
                result = new DocumentOrderIterator(result, GlobalOrderComparer.getInstance());
            }
            return result;
        } else {
            Object value = index.get(matchKeyMaker.makeMatchKey(soughtValue));
            return entryIterator(value);
        }
    }

    private SequenceIterator entryIterator(Object value) {
        if (value == null) {
            return EmptyIterator.INSTANCE;
        } else if (value instanceof NodeInfo) {
            return SingletonIterator.makeIterator((NodeInfo) value);
        } else {
            List<NodeInfo> nodes = (List<NodeInfo>) value;
            return new ListIterator.Of<>(nodes);
        }
    }

    /**
     * Get the nodes with a given composite key value
     *
     * @param soughtValue The required composite key value
     * @return a list of the selected nodes, always in document order with no duplicates, or null
     * to represent an empty list
     * @throws XPathException if a dynamic error is encountered
     */

    public SequenceIterator getComposite(SequenceIterator soughtValue) throws XPathException {
        List<AtomicMatchKey> amks = new ArrayList<>(4);
        SequenceTool.supply(soughtValue, (ItemConsumer<? super Item>) keyVal -> amks.add(matchKeyMaker.makeMatchKey((AtomicValue) keyVal)));
        Object value = index.get(new CompositeAtomicMatchKey(amks));
        return entryIterator(value);
    }


    private static class CompositeAtomicMatchKey implements AtomicMatchKey {

        private final List<AtomicMatchKey> keys;

        public CompositeAtomicMatchKey(List<AtomicMatchKey> keys) {
            this.keys = keys;
        }

        /**
         * Get an atomic value that encapsulates this match key. Needed to support the collation-key() function.
         *
         * @return an atomic value that encapsulates this match key
         */
        @Override
        public AtomicValue asAtomic() {
            throw new UnsupportedOperationException();
        }

        @Override
        public boolean equals(Object obj) {
            if (obj instanceof CompositeAtomicMatchKey &&
                    ((CompositeAtomicMatchKey)obj).keys.size() == keys.size()) {
                List<AtomicMatchKey> keys2 = ((CompositeAtomicMatchKey)obj).keys;
                for (int i=0; i<keys.size(); i++) {
                    if (!keys.get(i).equals(keys2.get(i))) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        @Override
        public int hashCode() {
            int h = 0x1ab27cd6;
            for (AtomicMatchKey amk : keys) {
                h ^= amk.hashCode();
                h = h << 1;
            }
            return h;
        }
    }
}

