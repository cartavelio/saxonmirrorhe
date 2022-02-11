using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class Derivation {
    // Injecting C# code
        public const int DERIVATION_RESTRICTION = 1;
        public const int DERIVATION_EXTENSION = 2;
        public const int DERIVATION_UNION = 4;
        public const int DERIVATION_LIST = 8;
        public const int DERIVE_BY_SUBSTITUTION = 16;
    // End of injected C# code
    
    }
}