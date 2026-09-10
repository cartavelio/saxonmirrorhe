using System;
using System.Globalization;
using Saxon.Hej.expr.sort;
using Saxon.Hej.lib;
using Saxon.Hej.str;
using Saxon.Hej.value;

namespace Saxon.Impl.Helpers {
    
    public class DotNetComparator : SubstringMatcher {

        private readonly CompareInfo comparer;
        private readonly CompareOptions options;
        private readonly string collationURI;

        /**
         * Create a collation based on a given .NET CompareInfo and CompareOptions
         * @param uri the collation URI
         * @param comparer the CompareInfo, which determines the language-specific
         *                 collation rules to be used
         * @param options  Options to be used in performing comparisons, for example
         *                 whether they are to be case-blind and/or accent-blind
         */

        public DotNetComparator(string uri, CompareInfo comparer, CompareOptions options) {
            this.collationURI = uri;
            this.comparer = comparer;
            this.options = options;
        }

        /**
         * Get the collation URI. It must be possible to use this collation URI to reconstitute the collation
         *
         * @return a collation URI that can be used to reconstruct the collation when an XSLT package is reloaded.
         */
        public string getCollationURI() {
            return collationURI;
        }

        /**
         * Compares its two arguments for order.  Returns a negative integer,
         * zero, or a positive integer as the first argument is less than, equal
         * to, or greater than the second.
         * <p>The implementor must ensure that <tt>sgn(compare(x, y)) ==
         * -sgn(compare(y, x))</tt> for all <tt>x</tt> and <tt>y</tt>.  (This
         * implies that <tt>compare(x, y)</tt> must throw an exception if and only
         * if <tt>compare(y, x)</tt> throws an exception.)</p>
         * <p>The implementor must also ensure that the relation is transitive:
         * <tt>((compare(x, y)&gt;0) &amp;&amp; (compare(y, z)&gt;0))</tt> implies
         * <tt>compare(x, z)&gt;0</tt>.</p>
         * <p>Finally, the implementer must ensure that <tt>compare(x, y)==0</tt>
         * implies that <tt>sgn(compare(x, z))==sgn(compare(y, z))</tt> for all
         * <tt>z</tt>.</p>
         * <p>It is generally the case, but <i>not</i> strictly required that
         * <tt>(compare(x, y)==0) == (x.equals(y))</tt>.  Generally speaking,
         * any comparator that violates this condition should clearly indicate
         * this fact.  The recommended language is "Note: this comparator
         * imposes orderings that are inconsistent with equals."</p>
         *
         * @param o1 the first object to be compared.
         * @param o2 the second object to be compared.
         * @return a negative integer, zero, or a positive integer as the
         *         first argument is less than, equal to, or greater than the
         *         second.
         * @throws ClassCastException if the arguments' types prevent them from
         *                            being compared by this Comparator.
         */

        public int compareStrings(UnicodeString o1, UnicodeString o2) {
            return comparer.Compare(o1.ToString(), o2.ToString(), options);
        }

        /**
         * Test whether one string is equal to another, according to the rules
         * of the XPath compare() function. The result is true if and only if the
         * compare() method returns zero: but the implementation may be more efficient
         * than calling compare and testing the result for zero
         *
         * @param s1 the first string
         * @param s2 the second string
         * @return true iff s1 equals s2
         */
        
        public bool comparesEqual(UnicodeString s1, UnicodeString s2) {
            return comparer.Compare(s1.ToString(), s2.ToString(), options) == 0;
        }
        
        /**
         * Test if a string is empty (under this collation)
         * @param s1 the string
         * @return true if the string is empty
         */
        public bool isEqualToEmpty(UnicodeString s1) {
            return comparesEqual(s1, EmptyUnicodeString.getInstance());
        }

        /**
         * Test whether one string contains another, according to the rules
         * of the XPath contains() function
         *
         * @param s1 the containing string
         * @param s2 the contained string
         * @return true iff s1 contains s2
         */
        
        public bool contains(UnicodeString s1, UnicodeString s2) {
            return comparer.IndexOf(s1.ToString(), s2.ToString(), options) >= 0;
        }

        /**
         * Test whether one string ends with another, according to the rules
         * of the XPath ends-with() function
         *
         * @param s1 the containing string
         * @param s2 the contained string
         * @return true iff s1 ends with s2
         */
        
        public bool endsWith(UnicodeString s1, UnicodeString s2) {
            return comparer.IsSuffix(s1.ToString(), s2.ToString(), options);
        }

        /**
         * Test whether one string starts with another, according to the rules
         * of the XPath starts-with() function
         *
         * @param s1 the containing string
         * @param s2 the contained string
         * @return true iff s1 starts with s2
         */

        public bool startsWith(UnicodeString s1, UnicodeString s2) {
            return comparer.IsPrefix(s1.ToString(), s2.ToString(), options);
        }

        /**
         * Return the part of a string after a given substring, according to the rules
         * of the XPath substring-after() function
         *
         * @param s1 the containing string
         * @param s2 the contained string
         * @return the part of s1 that follows the first occurrence of s2
         */

        public UnicodeString substringAfter(UnicodeString s1, UnicodeString s2) {
            String g1 = s1.ToString();
            String g2 = s2.ToString();
            int i = comparer.IndexOf(g1, g2, options);
            if (i < 0) {
                return EmptyUnicodeString.getInstance();
            }
            // We need to know where the matched string ends. Start with a guess, that the matched string
            // is the same length as s2. If it's not, try shorter and longer strings until we find one that's
            // equal. The search strategy is designed on the assumption that the matched string is of similar
            // length to the input string.
            int pivot = i + s2.length32();
            if (comparer.Compare(g1.Substring(i, pivot), g2, options) == 0) {
                return s1.substring(pivot);
            }

            int z = 0;
            bool reachedStart;
            bool reachedEnd;
            while (true) {
                z++;
                reachedStart = pivot - z < i;
                reachedEnd = pivot + z >= s1.length();
                if (!reachedEnd && comparer.Compare(g1.Substring(i, pivot + z), g2, options) == 0) {
                    return s1.substring(pivot + z);
                }
                if (!reachedStart && comparer.Compare(g1.Substring(i, pivot - z), g2, options) == 0) {
                    return s1.substring(pivot - z);
                }
                if (reachedStart && reachedEnd) {
                    // shouldn't happen
                    return EmptyUnicodeString.getInstance();
                }
            }
        }

        private StringValue stringValueFrom(String s, int from) {
            return new StringValue(s.Substring(from));
        }

        /**
         * Return the part of a string before a given substring, according to the rules
         * of the XPath substring-before() function
         *
         * @param s1 the containing string
         * @param s2 the contained string
         * @return the part of s1 that precedes the first occurrence of s2
         */
        
        public UnicodeString substringBefore(UnicodeString s1, UnicodeString s2) {
            String g1 = s1.ToString();
            String g2 = s2.ToString();
            int j = comparer.IndexOf(g1, g2, options);
            if (j < 0) {
                return EmptyUnicodeString.getInstance();
            }
            return s1.substring(0, j);
        }

        /**
         * Return a collation key. This is a wrapper around the System.Globalization.Sortkey object,
         * where the wrapper implements the required comparison methods.
         * @param value
         */
        
        public AtomicMatchKey getCollationKey(UnicodeString value) {
            SortKey sortKey = comparer.GetSortKey(value.ToString(), options);
            return new SortKeyWrapper(sortKey);
        }

        public AtomicMatchKey getCollationKey(String value) {
            SortKey sortKey = comparer.GetSortKey(value, options);
            return new SortKeyWrapper(sortKey);
        }

        private class SortKeyWrapper : AtomicMatchKey {

            private readonly SortKey key;

            public SortKeyWrapper(SortKey key) {
                this.key = key;
            }

            public int compareTo(Object other) {
                return SortKey.Compare(key, (SortKey) other);
            }

            /**
             * Returns a hash code value for the object.
             */
            public override int GetHashCode() {
                byte[] data = key.KeyData;
                int h = 0x7d6a8521;
                for (int i = 0; i < data.Length; i++) {
                    h ^= data[i] << (i & 31);
                }
                //Console.Error.WriteLine("Hashcode for " + key + " = " + h);
                return h;
            }
            
            /**
             * Returns a 64-bit hash code value for the object.
             */
            public long longHashCode() {
                byte[] data = key.KeyData;
                long h = 0x7d6a8521_59a0133c;
                for (int i = 0; i < data.Length; i++) {
                    h ^= data[i] << (i & 31);
                }
                return h;
            }

            /**
             * Indicates whether some other SortKeyWrapper is "equal to" this one (that is, it represents
             * the sortkey of a value that is equal to this value).
             */
            public override bool Equals(object obj) {
                if (obj is not SortKeyWrapper wrapper) {
                    return false;
                }
                byte[] data1 = key.KeyData;
                byte[] data2 = wrapper.key.KeyData;
                if (data1.Length != data2.Length) {
                    return false;
                }
                for (int i = data1.Length - 1; i >= 0; i--) {
                    if (data1[i] != data2[i]) {
                        return false;
                    }
                }
                return true;
            }

            /**
             * Get an atomic value that encapsulates this match key. Needed to support the collation-key() function.
             *
             * @return an atomic value that encapsulates this match key
             */

            public AtomicValue asAtomic() {
                return new Base64BinaryValue(key.KeyData);
            }
        }
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited