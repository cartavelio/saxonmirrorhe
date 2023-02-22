
using System;
using System.Collections;
using Saxon.Hej.expr;
using SysCollGen = System.Collections.Generic;
namespace Saxon.Impl.Overrides {
    
    /// <summary>
    /// An IEnumerator that applies a mapping function to every item returned by a supplied input IEnumerator
    /// </summary>
    /// <typeparam name="S">The type of items returned by the input IEnumerator</typeparam>
    /// <typeparam name="T">The type of items returned by the output IEnumerator</typeparam>
    public class MappingEnumerator<S, T> : SysCollGen.IEnumerator<T> {
        
        // TODO: unlike the Java class that this emulates, it doesn't skip items when the mapper returns null

        private readonly SysCollGen.IEnumerator<S> baseEnumerator;
        private readonly Func<S, T> mapper;

        public MappingEnumerator(SysCollGen.IEnumerator<S> baseEnumerator, Func<S, T> mapper) {
            this.baseEnumerator = baseEnumerator;
            this.mapper = mapper;
        }
        public bool MoveNext() {
            return baseEnumerator.MoveNext();
        }

        public void Reset() {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public void Dispose() {
            this.baseEnumerator.Dispose();
        }

        public T Current => mapper(baseEnumerator.Current);

    }
}

// Copyright (c) 2021 Saxonica Limited