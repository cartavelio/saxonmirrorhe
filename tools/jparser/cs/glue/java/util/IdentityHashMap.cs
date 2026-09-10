using System;
using System.Collections.Generic;

/**
 * A C# emulation of java.util.IdentityHashMap, incomplete but sufficient for use in Saxon
 */
 
namespace java.util {
    public sealed class IdentityHashMap<K, V> : java.util.HashMap<K, V> {
        public IdentityHashMap() {            
            dict = new Dictionary<K, V>(System.Collections.Generic.ReferenceEqualityComparer<K>.Instance);
        }
    }
}