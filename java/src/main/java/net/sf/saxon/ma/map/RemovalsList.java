package net.sf.saxon.ma.map;

import net.sf.saxon.ma.trie.ImmutableHashTrieMap;
import net.sf.saxon.transpile.CSharpReplaceBody;

public class RemovalsList {

    private final ImmutableHashTrieMap<Integer, Integer> removals;
    private final int numberOfItems;

    public RemovalsList() {
        removals = emptyMap();
        numberOfItems = 0;
    }

    private RemovalsList(ImmutableHashTrieMap<Integer, Integer> removals, int numberOfItems) {
        this.removals = removals;
        this.numberOfItems = numberOfItems;
    }

    @CSharpReplaceBody(code="return System.Collections.Immutable.ImmutableDictionary<int, int>.Empty;")
    private static ImmutableHashTrieMap<Integer, Integer> emptyMap() {
        return ImmutableHashTrieMap.empty();
    }

    public static RemovalsList empty = new RemovalsList();

    public RemovalsList add(Integer value) {
        return new RemovalsList(removals.put(value, value), numberOfItems + 1);
    }

    public boolean contains(Integer value) {
        return removals.get(value) != null;
    }

    public int size(){
        return numberOfItems;
    }

    /**
     * Remove an entry from the removals list
     * @param value the entry to be removed from the removals list
     * @return the removals list, with the supplied value reinstated
     */
    public RemovalsList remove(Integer value) {
        if (removals.get(value) == null) {
            return this;
        }
        return new RemovalsList(removals.remove(value), numberOfItems - 1);
    }



}
