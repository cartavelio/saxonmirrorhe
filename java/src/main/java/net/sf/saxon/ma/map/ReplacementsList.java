package net.sf.saxon.ma.map;

import net.sf.saxon.ma.trie.ImmutableHashTrieMap;
import net.sf.saxon.om.GroundedValue;

import java.util.Objects;
import java.util.Optional;

public class Replacements {

    private final ImmutableHashTrieMap<Integer, Optional<GroundedValue>> replacementValues;
    private final int numberReplaced;
    private final int numberRemoved;

    public Replacements() {
        replacementValues = ImmutableHashTrieMap.empty();
        numberReplaced = 0;
        numberRemoved = 0;
    }

    private Replacements(ImmutableHashTrieMap<Integer, Optional<GroundedValue>> removals, int numberReplaced, int numberRemoved) {
        this.replacementValues = removals;
        this.numberReplaced = numberReplaced;
        this.numberRemoved = numberRemoved;
    }

    public static Replacements empty = new Replacements();

    public GroundedValue get(Integer value) {
        Optional<GroundedValue> result = replacementValues.get(value);
        if (result != null && result.isPresent()) {
            return result.get();
        }
        return null;
    }

    public boolean contains(Integer value) {
        Optional<GroundedValue> result = replacementValues.get(value);
        return result != null;
    }

    public int getNumberReplaced() {
        return numberReplaced;
    }

    public int getNumberRemoved() {
        return numberRemoved;
    }

    public Replacements remove(Integer value) {
        if (replacementValues.get(value) == null) {
            return this;
        }
        return new Replacements(replacementValues.put(value, Optional.empty()), numberReplaced, numberRemoved + 1);
    }

    public Replacements replace(Integer index, GroundedValue newValue) {
        Objects.requireNonNull(newValue);
        if (replacementValues.get(index) == null) {
            return new Replacements(replacementValues.put(index, Optional.of(newValue)), numberReplaced + 1, numberRemoved);
        }
        return new Replacements(replacementValues.put(index, Optional.of(newValue)), numberReplaced, numberRemoved);
    }



}
