using System.Globalization;

namespace Saxon.Api.Support {
using System;
using System.Collections.Generic;

    public class ComparingCollation : net.sf.saxon.lib.SubstringMatcher {
        private readonly CompareInfo comparer;
        private readonly CompareOptions options;
        private readonly string collationURI;
        public ComparingCollation(string uri, CompareInfo comparer, CompareOptions options)  {
            this.collationURI = uri;
            this.comparer = comparer;
            this.options = options;
        }
        public string getCollationURI() {
            return collationURI;
        }
        public int compareStrings(net.sf.saxon.str.UnicodeString o1, net.sf.saxon.str.UnicodeString o2) {
            return comparer.Compare(o1.ToString(), o2.ToString(), options);
        }
        public bool comparesEqual(net.sf.saxon.str.UnicodeString s1, net.sf.saxon.str.UnicodeString s2) {
            return (comparer.Compare(s1.ToString(), s2.ToString(), options)==0);
        }
        public bool contains(net.sf.saxon.str.UnicodeString s1, net.sf.saxon.str.UnicodeString s2) {
            return (comparer.IndexOf(s1.ToString(), s2.ToString(), options)>=0);
        }
        public bool endsWith(net.sf.saxon.str.UnicodeString s1, net.sf.saxon.str.UnicodeString s2) {
            return comparer.IsSuffix(s1.ToString(), s2.ToString(), options);
        }
        public bool startsWith(net.sf.saxon.str.UnicodeString s1, net.sf.saxon.str.UnicodeString s2) {
            return comparer.IsPrefix(s1.ToString(), s2.ToString(), options);
        }
        public net.sf.saxon.str.UnicodeString substringAfter(net.sf.saxon.str.UnicodeString s1, net.sf.saxon.str.UnicodeString s2) {
            string g1 = s1.ToString();
            string g2 = s2.ToString();
            int i = comparer.IndexOf(g1, g2, options);
            if ((i<0)) {
                return net.sf.saxon.str.EmptyUnicodeString.getInstance();
            }
            int pivot = (i+s2.length32());
            if ((comparer.Compare(g1.Substring(i, pivot), g2, options)==0)) {
                return s1.substring(pivot);
            }
            int z = 0;
            bool reachedStart;
            bool reachedEnd;
            while (true) {
                z++;
                reachedStart = ((pivot-z)<i);
                reachedEnd = ((pivot+z)>=s1.length());
                if ((!(reachedEnd)&&(comparer.Compare(g1.Substring(i, (pivot+z)), g2, options)==0))) {
                    return s1.substring((pivot+z));
                }
                if ((!(reachedStart)&&(comparer.Compare(g1.Substring(i, (pivot-z)), g2, options)==0))) {
                    return s1.substring((pivot-z));
                }
                if ((reachedStart&&reachedEnd)) {
                    return net.sf.saxon.str.EmptyUnicodeString.getInstance();
                }
            }
        }
        private net.sf.saxon.value.StringValue stringValueFrom(string s, int from) {
            return new net.sf.saxon.value.StringValue(s.Substring(from));
        }
        public net.sf.saxon.str.UnicodeString substringBefore(net.sf.saxon.str.UnicodeString s1, net.sf.saxon.str.UnicodeString s2) {
            string g1 = s1.ToString();
            string g2 = s2.ToString();
            int j = comparer.IndexOf(g1, g2, options);
            if ((j<0)) {
                return net.sf.saxon.str.EmptyUnicodeString.getInstance();
            }
            return s1.substring(0, j);
        }
        public net.sf.saxon.expr.sort.AtomicMatchKey getCollationKey(net.sf.saxon.str.UnicodeString value) {
            SortKey sortKey = comparer.GetSortKey(value.ToString(), options);
            return new ComparingCollation.SortKeyWrapper(sortKey);
        }
        public net.sf.saxon.expr.sort.AtomicMatchKey getCollationKey(string value) {
            SortKey sortKey = comparer.GetSortKey(value, options);
            return new ComparingCollation.SortKeyWrapper(sortKey);
        }
        private class SortKeyWrapper : net.sf.saxon.expr.sort.AtomicMatchKey {
            private readonly SortKey key;
            public SortKeyWrapper(SortKey key)  {
                this.key = key;
            }
            public int compareTo(System.Object other) {
                return SortKey.Compare(key, (SortKey)other);
            }
            public int hashCode() {
                byte[] data = key.KeyData;
                int h = 0x7d6a8521;
                for (int i = 0; (i<data.Length); i++) {
                    h ^= (data[i]<<((i&31)));
                }
                return h;
            }
            public bool equals(System.Object obj) {
                if (!(((obj is ComparingCollation.SortKeyWrapper)))) {
                    return false;
                }
                byte[] data1 = key.KeyData;
                byte[] data2 = ((ComparingCollation.SortKeyWrapper)obj).key.KeyData;
                if ((data1.Length!=data2.Length)) {
                    return false;
                }
                for (int i = (data1.Length-1); (i>=0); i--) {
                    if ((data1[i]!=data2[i])) {
                        return false;
                    }
                }
                return true;
            }
            public net.sf.saxon.value.AtomicValue asAtomic() {
                return new net.sf.saxon.value.Base64BinaryValue(key.KeyData);
            }
        }
    }
}
