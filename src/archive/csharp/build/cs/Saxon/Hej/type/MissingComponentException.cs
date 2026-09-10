using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class MissingComponentException : Saxon.Ejava.lang.RuntimeException {
        public MissingComponentException(string @ref) : base (@ref) {
        }
    
    }
}