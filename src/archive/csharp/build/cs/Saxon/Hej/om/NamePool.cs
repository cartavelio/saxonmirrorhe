using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public sealed class NamePool {
        public const int FP_MASK = unchecked((int)0xfffff);
        public const int USER_DEFINED_MASK = unchecked((int)0xffc00);
        private const int MAX_FINGERPRINT = FP_MASK;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Saxon.Hej.om.StructuredQName,int> qNameToInteger = new /* java.util.concurrent.ConcurrentHashMap */ System.Collections.Concurrent.ConcurrentDictionary<Saxon.Hej.om.StructuredQName, int>(16, 1000);
        private readonly System.Collections.Concurrent.ConcurrentDictionary<int,Saxon.Hej.om.StructuredQName> integerToQName = new /* java.util.concurrent.ConcurrentHashMap */ System.Collections.Concurrent.ConcurrentDictionary<int, Saxon.Hej.om.StructuredQName>(16, 1000);
        private readonly Saxon.Hej.om.AtomicCounter unique = new Saxon.Hej.om.AtomicCounter(1024);
        private readonly System.Collections.Concurrent.ConcurrentDictionary<Saxon.Hej.om.NamespaceUri,string> suggestedPrefixes = new System.Collections.Concurrent.ConcurrentDictionary<Saxon.Hej.om.NamespaceUri, string>();
        public NamePool()  {
        }
        public void suggestPrefix(string prefix, Saxon.Hej.om.NamespaceUri uri) {
            Saxon.Impl.Helpers.MapUtils.Put(suggestedPrefixes, uri, prefix);
        }
        public Saxon.Hej.om.StructuredQName getUnprefixedQName(int nameCode) {
            int fp = (nameCode&FP_MASK);
            if ((((fp&USER_DEFINED_MASK)) == 0)) {
                return Saxon.Hej.om.StandardNames.getUnprefixedQName(fp);
            }
            return Saxon.Impl.Helpers.MapUtils.Get(integerToQName, fp);
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName(int fingerprint) {
            return getUnprefixedQName(fingerprint);
        }
        public static bool isPrefixed(int nameCode) {
            return (((nameCode&0x3ff00000)) != 0);
        }
        public string suggestPrefixForURI(Saxon.Hej.om.NamespaceUri uri) {
            if (uri.Equals(Saxon.Hej.om.NamespaceUri.XML)) {
                return "xml";
            }
            return Saxon.Impl.Helpers.MapUtils.Get(suggestedPrefixes, uri);
        }
        public int allocateFingerprint(Saxon.Hej.om.NamespaceUri uri, string local) { lock(this) {
            if ((Saxon.Hej.om.NamespaceUri.isReserved(uri) || Saxon.Hej.om.NamespaceUri.SAXON.Equals(uri))) {
                int fp = Saxon.Hej.om.StandardNames.getFingerprint(uri, local);
                if ((fp != -1)) {
                    return fp;
                }
            }
            Saxon.Hej.om.StructuredQName qName = new Saxon.Hej.om.StructuredQName("", uri, local);
            int existing = Saxon.Impl.Helpers.MapUtils.GetOrDefault(qNameToInteger, qName, -1);
            if ((existing>=0)) {
                return existing;
            }
            long nextUnique = unique.getAndIncrement();
            if ((nextUnique>MAX_FINGERPRINT)) {
                throw new Saxon.Hej.om.NamePool.NamePoolLimitException("Too many distinct names in NamePool");
            }
            int next = (int)(nextUnique);
            int existing2 = Saxon.Impl.Helpers.MapUtils.PutIfAbsent(qNameToInteger, qName, next);
            if (keyWasAbsent(existing2)) {
                Saxon.Impl.Helpers.MapUtils.Put(integerToQName, next, qName);
                return next;
            } else {
                return existing;
            }
        }}
        private static bool keyWasAbsent(int result) {
            return result == default(System.Int32);
        }
        public Saxon.Hej.om.NamespaceUri getURI(int nameCode) {
            int fp = (nameCode&FP_MASK);
            if ((((fp&USER_DEFINED_MASK)) == 0)) {
                return Saxon.Hej.om.StandardNames.getURI(fp);
            }
            return getUnprefixedQName(fp).getNamespaceUri();
        }
        public string getLocalName(int nameCode) {
            return getUnprefixedQName(nameCode).getLocalPart();
        }
        public string getDisplayName(int nameCode) {
            return getStructuredQName(nameCode).getDisplayName();
        }
        public string getClarkName(int nameCode) {
            return getUnprefixedQName(nameCode).getClarkName();
        }
        public string getEQName(int nameCode) {
            return getUnprefixedQName(nameCode).getEQName();
        }
        public int allocateClarkName(string expandedName) {
            Saxon.Hej.om.NamespaceUri @namespace;
            string localName;
            if ((expandedName[0] == '{')) {
                int closeBrace = expandedName.IndexOf('}');
                if ((closeBrace<0)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("No closing '}' in Clark name");
                }
                @namespace = Saxon.Hej.om.NamespaceUri.of(expandedName.Substring(1, closeBrace - 1));
                if ((closeBrace == expandedName.Length)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Missing local part in Clark name");
                }
                localName = expandedName.Substring((closeBrace+1));
            } else {
                @namespace = Saxon.Hej.om.NamespaceUri.NULL;
                localName = expandedName;
            }
            return allocateFingerprint(@namespace, localName);
        }
        public int getFingerprint(Saxon.Hej.om.NamespaceUri uri, string localName) {
            if ((Saxon.Hej.om.NamespaceUri.isReserved(uri) || uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON))) {
                int fp = Saxon.Hej.om.StandardNames.getFingerprint(uri, localName);
                if ((fp != -1)) {
                    return fp;
                }
            }
            return Saxon.Impl.Helpers.MapUtils.GetOrDefault(qNameToInteger, new Saxon.Hej.om.StructuredQName("", uri, localName), -1);
        }
        public class NamePoolLimitException : Saxon.Ejava.lang.RuntimeException {
            public NamePoolLimitException(string message) : base (message) {
            }
        
        }
    
    }
}