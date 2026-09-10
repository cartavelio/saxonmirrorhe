using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SysCollGen = System.Collections.Generic;

/*
 * A C# emulation of java.util.IdentityHashMap, incomplete but sufficient for use in Saxon
 */
 
namespace Saxon.Ejava.util {
    public sealed class IdentityHashMap<K, V> : SysCollGen.Dictionary<K, V> {
        public IdentityHashMap() : base (new Saxon.Ejava.util.ReferenceEqualityComparer<K>()) {            
            
        }
        
        public IdentityHashMap(int allocation) : base (allocation,new Saxon.Ejava.util.ReferenceEqualityComparer<K>()) {            
            
        }
    }
    
    public sealed class ReferenceEqualityComparer<K> : IEqualityComparer<K> {
        
        public bool Equals(K x, K y) => ReferenceEquals(x, y);
        public int GetHashCode(K obj) => RuntimeHelpers.GetHashCode(obj);
    }
}