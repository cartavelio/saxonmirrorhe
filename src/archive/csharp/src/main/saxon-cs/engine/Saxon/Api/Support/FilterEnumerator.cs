using System;
using System.Collections;
using System.Collections.Generic;
using Saxon.Hej.om;

namespace Saxon.Api.Support {
    /// <summary>
    /// This class is an implementation of <c>IEnumerator</c> that wraps
    /// a (Java) <c>SequenceIterator</c>.
    /// </summary>
    /// <remarks>
    /// Because the underlying value can be evaluated lazily, it is possible
    /// for exceptions to occur as the sequence is being read.
    /// </remarks>
    [Serializable]
    internal class FilterEnumerator<T> : IEnumerator<T> {
        private readonly IEnumerator<T> iter;
        private readonly Predicate<T> filter;
        private T current;

        internal FilterEnumerator(IEnumerator<T> iter, Predicate<T> filter) {
            this.iter = iter;
            this.filter = filter;
            current = default(T);
        }


        /// <summary>Return the current item in the sequence</summary>
        /// <returns>An object which will always be an instance of <c>XdmItem</c></returns>
        /// 
        public T Current => current;

        object IEnumerator.Current => Current;

        T IEnumerator<T>.Current => (T) Current;

        /// <summary>Move to the next item in the sequence</summary>
        /// <returns>true if there are more items in the sequence</returns>
        public bool MoveNext() {
            while (true) {
                bool more = iter.MoveNext();
                if (more) {
                    T next = iter.Current;
                    if (filter.Invoke(next)) {
                        current = next;
                        return true;
                    }
                }
                else {
                    return false;
                }
            }
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release resources. (In the case of streamed evaluation, this may in some circumstances
        /// cause early termination of document parsing).
        /// </summary>
        public void Dispose() {
            iter.Dispose();
        }
    }
}