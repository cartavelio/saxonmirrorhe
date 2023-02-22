package net.sf.saxon.expr.sort;

import net.sf.saxon.z.IntHashMap;

/**
 * A set implementation using a custom equality matcher, to avoid having to wrap
 * each entry in a wrapper object to achieve customised equality testing.
 *
 * <p>Note: this doesn't implement {@code java.util.Set<T>} to save the effort of implementing
 * methods like size() and iterator() that aren't required for our use cases. These
 * methods could easily be added without changing the basic design.
 * </p>
 */

public class CustomSet<T>  {

    private final IntHashMap<CustomSetEntryChain<T>> buckets;
    private final EqualityMatcher<T> equalityMatcher;

    public CustomSet(EqualityMatcher<T> matcher) {
        buckets = new IntHashMap<>();
        equalityMatcher = matcher;
    }

    /**
     * Add a value to the set
     * @param value the value to be added.
     * @return true if the value is newly added, that is, if it was not already present
     */
    public boolean add(T value) {
        int h = equalityMatcher.hash(value);
        CustomSetEntryChain<T> bucket = buckets.get(h);
        if (bucket == null) {
            CustomSetEntryChain<T> entry = new CustomSetEntryChain<>(value);
            buckets.put(h, entry);
            return true;
        } else {
            while (true) {
                if (equalityMatcher.equal(bucket.value, value)) {
                    return false;
                } else if (bucket.next == null) {
                    @SuppressWarnings("UnnecessaryLocalVariable")
                    CustomSetEntryChain<T> entry = new CustomSetEntryChain<>(value);
                    bucket.next = entry;
                    return true;
                } else {
                    bucket = bucket.next;
                }
            }
        }
    }

    /**
     * Ask if a value exists in the set
     * @return true if it exists
     */

    public boolean contains(T value) {
        int h = equalityMatcher.hash(value);
        CustomSetEntryChain<T> bucket = buckets.get(h);
        if (bucket == null) {
            return false;
        } else {
            while (true) {
                if (equalityMatcher.equal(bucket.value, value)) {
                    return true;
                } else if (bucket.next == null) {
                    return false;
                } else {
                    bucket = bucket.next;
                }
            }
        }
    }




}

