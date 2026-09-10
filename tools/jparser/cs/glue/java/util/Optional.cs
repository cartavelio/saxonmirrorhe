using System;
using System.Collections.Generic;

/**
 * A C# emulation of java.util.Optional, incomplete but sufficient for use in Saxon
 */
 
namespace java.util {
    public sealed class Optional<T> {
        private readonly T _value;

        public Optional(T value) {
            _value = value;
        }
        public T get() {
            return _value;
        }
        public bool isPresent() {
            return _value != null;
        }
    }

}