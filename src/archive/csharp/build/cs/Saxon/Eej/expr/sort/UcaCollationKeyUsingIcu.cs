using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr.sort {
    public class UcaCollationKeyUsingIcu : Saxon.Ejava.text.CollationKey {
        private readonly ICU4N.Text.CollationKey ucaKey;
        internal UcaCollationKeyUsingIcu(string source, byte[] key) : base (source) {
            this.ucaKey = new ICU4N.Text.CollationKey(source, key);
        }
        public ICU4N.Text.CollationKey getKey() {
            return ucaKey;
        }
        public override  /* REPLACING public */ int compareTo(Saxon.Ejava.text.CollationKey target) {
            return ucaKey.CompareTo(((Saxon.Eej.expr.sort.UcaCollationKeyUsingIcu)(target)).getKey());
        }
        public override  /* REPLACING public */ byte[] toByteArray() {
            return ucaKey.ToByteArray();
        }
        public override int GetHashCode() {
            return ucaKey.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Eej.expr.sort.UcaCollationKeyUsingIcu) && ucaKey.Equals(((Saxon.Eej.expr.sort.UcaCollationKeyUsingIcu)(obj)).ucaKey));
        }
    
    }
}