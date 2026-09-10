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
    internal class SequenceEnumerator<T> : IEnumerator<T>
        where T : XdmItem {
        private SequenceIterator iter;
        private Item current;

        internal SequenceEnumerator(SequenceIterator iter) {
            this.iter = iter;
            current = null;
        }


        /// <summary>Return the current item in the sequence</summary>
        /// <returns>An object which will always be an instance of <c>XdmItem</c></returns>
        /// 
        public XdmItem Current => current == null ? null : (XdmItem) XdmValue.Wrap(current);

        object IEnumerator.Current => Current;

        T IEnumerator<T>.Current => (T) Current;

        /// <summary>Move to the next item in the sequence</summary>
        /// <returns>true if there are more items in the sequence</returns>
        public bool MoveNext() {
            Saxon.Hej.om.Item nextBaseItem = iter.next();
            if (nextBaseItem == null) {
                return false;
            }
            current = nextBaseItem;
            return true;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release resources. (In the case of streamed evaluation, this may in some circumstances
        /// cause early termination of document parsing).
        /// </summary>
        public void Dispose() {
            iter.close();
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited