package net.sf.saxon.ma.map;

import net.sf.saxon.ma.trie.ImmutableHashTrieMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.transpile.CSharpReplaceBody;

public class ReplacementsList {

    private final ImmutableHashTrieMap<Integer, GroundedValue> replacementValues;

    public ReplacementsList() {
        replacementValues = emptyMap();
    }

    private ReplacementsList(ImmutableHashTrieMap<Integer, GroundedValue> replacements) {
        this.replacementValues = replacements;
    }

    @CSharpReplaceBody(code = "return System.Collections.Immutable.ImmutableDictionary<int, Saxon.Hej.om.GroundedValue>.Empty;")
    private ImmutableHashTrieMap<Integer, GroundedValue> emptyMap() {
        return ImmutableHashTrieMap.empty();
    }

    public static ReplacementsList empty = new ReplacementsList();

    public GroundedValue get(Integer value) {
        return replacementValues.get(value);
    }

    public ReplacementsList remove(Integer value) {
        if (replacementValues.get(value) == null) {
            return this;
        }
        return new ReplacementsList(replacementValues.remove(value));
    }

    public ReplacementsList put(Integer index, GroundedValue newValue) {
        return new ReplacementsList(replacementValues.put(index, newValue));
    }



}
