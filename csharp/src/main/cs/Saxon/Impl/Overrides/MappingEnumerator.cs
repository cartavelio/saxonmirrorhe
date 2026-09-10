
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
        
        private readonly SysCollGen.IEnumerator<S> baseEnumerator;
        private readonly Func<S, T> mapper;
        private T nextItem = default(T);

        public MappingEnumerator(SysCollGen.IEnumerator<S> baseEnumerator, Func<S, T> mapper) {
            this.baseEnumerator = baseEnumerator;
            this.mapper = mapper;
            this.nextItem = default(T);
        }
        public bool MoveNext() {
            T next;
            do {
                bool baseHasNext = baseEnumerator.MoveNext();
                if (baseHasNext) {
                    next = mapper(baseEnumerator.Current);
                }
                else {
                    this.nextItem = default(T);
                    return false;
                } 
            } while (next == null);
            nextItem = next;
            return true;
        }

        public void Reset() {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public void Dispose() {
            this.baseEnumerator.Dispose();
        }

        public T Current => this.nextItem;
    } 
}

// Copyright (c) 2021 - 2025 Saxonica Limited