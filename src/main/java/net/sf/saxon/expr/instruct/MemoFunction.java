////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.instruct;

import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Genre;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.z.IntHashMap;

import java.util.ArrayList;
import java.util.List;

/**
 * A user-defined function that is declared as a memo function, meaning that it remembers results
 * of previous calls.
 */

public class MemoFunction extends UserFunction {

    /**
     * Determine the preferred evaluation mode for this function
     */

    @Override
    public void computeEvaluationMode() {
        evaluator = ExpressionTool.eagerEvaluator(getBody());
    }

    /**
     * Ask whether this function is a memo function
     *
     * @return true if this function is marked as a memo function
     */

    @Override
    public boolean isMemoFunction() {
        return true;
    }

    /**
     * Call this function to return a value.
     *
     * @param actualArgs the arguments supplied to the function. These must have the correct
     *                   types required by the function signature (it is the caller's responsibility to check this).
     *                   It is acceptable to supply a {@link net.sf.saxon.value.Closure} to represent a value whose
     *                   evaluation will be delayed until it is needed. The array must be the correct size to match
     *                   the number of arguments: again, it is the caller's responsibility to check this.
     * @param context    This provides the run-time context for evaluating the function. It is the caller's
     *                   responsibility to allocate a "clean" context for the function to use; the context that is provided
     *                   will be overwritten by the function.
     * @return a Value representing the result of the function.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] actualArgs) throws XPathException {

        // Ensure the arguments are all grounded
        for (int i=0; i<actualArgs.length; i++) {
            actualArgs[i] = actualArgs[i].materialize();
        }

        Controller controller = context.getController();
        MemoFunctionCache cache = (MemoFunctionCache) controller.getUserData(this, "memo-function-cache");
        if (cache == null) {
            cache = new MemoFunctionCache();
            controller.setUserData(this, "memo-function-cache", cache);
        }
        int hash = cache.hash(actualArgs);
        GroundedValue value = cache.get(hash, actualArgs);
        if (value != null) {
            return value;
        }

        value = super.call(context, actualArgs).materialize();

        // Save the result in the cache
        cache.put(hash, actualArgs, value);
        return value;
    }

    private static class MemoFunctionCache {

        /*
         * The internal data structure is chosen to minimize the need to allocate new objects, and for efficiency
         * in the common case where there is only one argument and it is a singleton item. The cacheMap contains
         * a set of buckets, indexed by the computed hash code of the argument values; each bucket holds a sequence
         * of groups of length (arity + 1), where the first (arity) values in the group are the sequence of arguments
         * to the function, and the final value in the group is the corresponding function result.
         */

        private final IntHashMap<List<GroundedValue>> cacheMap = new IntHashMap<>();

        /**
         * Compute a hash key for a set of argument values
         * @param args the set of argument values. Although supplied as type {@code Sequence[]},
         *             the members of the array must all be {@link GroundedValue} instances.
         * @return the corresponding hash code
         */
        public int hash(Sequence[] args) {
            int h = 0x389247ab;
            for (Sequence arg : args) {
                GroundedValue val = (GroundedValue) arg;
                if (val instanceof Item) {
                    h ^= hashItem((Item) val) + 1;
                } else {
                    for (Item it : val.asIterable()) {
                        h ^= hashItem(it) + 1;
                    }
                }
            }
            return h;
        }

        private int hashItem(Item it) {
            return it.hashCode(); // may need to refine this
        }

        /**
         * Get the cached result of the function for a given set of argument values, if available
         * @param hash the computed hash code for this set of argument values
         * @param args the argument values (which must all be instances of {@code GroundedValue})
         * @return the cached function result, if available, or null otherwise
         */
        public GroundedValue get(int hash, Sequence[] args) {
            int arity = args.length;
            List<GroundedValue> bucket = cacheMap.get(hash);
            if (bucket == null) {
                return null;
            }
            for (int i=0; i<bucket.size(); i += (arity+1)) {
                boolean found = true;
                for (int j=0; j<arity; j++) {
                    if (!sameValue((GroundedValue)args[j], bucket.get(i+j))) {
                        found = false;
                        break;
                    }
                }
                if (found) {
                    return bucket.get(i+arity);
                }
            }
            return null;
        }

        /**
         * Test if two values are the same for the purposes of caching. This is a stronger test than
         * normal XPath (or even XSD) equality or identity testing: the values must not just be equal,
         * they must be indistinguishable. This means for example that they must have exactly the same
         * type annotation.
         * @param v0 the first value
         * @param v1 the second value
         * @return true if the values are equivalent for caching purposes
         */
        private static boolean sameValue(GroundedValue v0, GroundedValue v1) {
            if (v0.getLength() != v1.getLength()) {
                return false;
            }
            for (int i=0; i<v0.getLength(); i++) {
                Item it0 = v0.itemAt(i);
                Item it1 = v1.itemAt(i);
                if (it0 == it1) {
                    continue;
                }
                if (it0.getGenre() != it1.getGenre()) {
                    return false;
                }
                if (it0.getGenre() == Genre.ATOMIC) {
                    AtomicValue av0 = (AtomicValue) it0;
                    AtomicValue av1 = (AtomicValue) it1;
                    if (!av0.getItemType().equals(av1.getItemType())) {
                        return false;
                    }
                    if (!av0.isIdentical(av1)) {
                        return false;
                    }
                    if (av0 instanceof NumericValue &&
                            (((NumericValue)av0).isNegativeZero() != ((NumericValue) av1).isNegativeZero())) {
                        return false; // The isIdentical() test treats positive and negative zero as identical
                    }
                } else {
                    if (!it0.equals(it1)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /**
         * Put a new entry in the cache
         * @param hash the hash code of the argument values
         * @param args the argument values
         * @param result the corresponding function result
         */
        public void put(int hash, Sequence[] args, GroundedValue result) {
            List<GroundedValue> bucket = cacheMap.get(hash);
            if (bucket == null) {
                bucket = new ArrayList<>(args.length + 1);
                cacheMap.put(hash, bucket);
            }
            for (Sequence val : args) {
                bucket.add((GroundedValue)val);
            }
            bucket.add(result);
        }
    }


    
}



