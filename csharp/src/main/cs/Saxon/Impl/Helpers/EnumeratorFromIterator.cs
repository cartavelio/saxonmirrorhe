using System;
using System.Collections;
using System.Collections.Generic;

 
namespace Saxon.Impl.Helpers {
    
 /**
 * A C# wrapper class that converts a Java Iterator to a C# Enumerator
 */


    public class EnumeratorFromIterator<T> : IEnumerator<T>
    {
        private Saxon.Ejava.util.Iterator<T> iter;
        internal T _Current = default(T);
        public EnumeratorFromIterator(Saxon.Ejava.util.Iterator<T> iter) {
            this.iter = iter;
        } 
        public bool MoveNext() {
            if (iter.hasNext()) {
                _Current = iter.next();
                return true;
            } else {
                _Current = default(T);
                return false;
            }
        }

        public T Current {
            get {
                return _Current;
            }
        }

        object IEnumerator.Current => Current;

        public void Reset() {
            throw new NotSupportedException();
        }

        public void Dispose() {
            // no action
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
