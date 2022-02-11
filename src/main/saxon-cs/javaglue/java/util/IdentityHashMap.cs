using System;
using System.Collections.Generic;
using SysCollGen = System.Collections.Generic;

/**
 * A C# emulation of java.util.IdentityHashMap, incomplete but sufficient for use in Saxon
 */
 
namespace java.util {
    public sealed class IdentityHashMap<K, V> : SysCollGen.Dictionary<K, V> {
        public IdentityHashMap() : base (System.Collections.Generic.ReferenceEqualityComparer<K>.Instance) {            
            
        }
    }
}