using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SysCollGen = System.Collections.Generic;

/**
 * A C# emulation of java.util.IdentityHashMap, incomplete but sufficient for use in Saxon
 */
 
namespace java.util {
    public sealed class IdentityHashMap<K, V> : SysCollGen.Dictionary<K, V> {
        public IdentityHashMap() : base (new java.util.ReferenceEqualityComparer<K>()) {            
            
        }
    }
    
    public sealed class ReferenceEqualityComparer<K> : IEqualityComparer<K> {
        
        public bool Equals(K x, K y) => ReferenceEquals(x, y);
        public int GetHashCode(K obj) => RuntimeHelpers.GetHashCode(obj);
    }
}