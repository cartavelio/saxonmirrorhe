package net.sf.saxon.ma.map;

import net.sf.saxon.ma.trie.ImmutableHashTrieMap;

public class RemovalsList {

    private final ImmutableHashTrieMap<Integer, Integer> removals;
    private final int numberOfItems;

    public RemovalsList() {
        removals = ImmutableHashTrieMap.empty();
        numberOfItems = 0;
    }

    private RemovalsList(ImmutableHashTrieMap<Integer, Integer> removals, int numberOfItems) {
        this.removals = removals;
        this.numberOfItems = numberOfItems;
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

    public RemovalsList remove(Integer value) {
        if(removals.get(value) == null) {
            return this;
        }
        return new RemovalsList(removals.remove(value), numberOfItems -1);
    }



}
