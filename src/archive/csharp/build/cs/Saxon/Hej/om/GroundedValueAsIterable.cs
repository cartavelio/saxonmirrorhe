using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class GroundedValueAsIterable : System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> {
        private readonly Saxon.Hej.om.GroundedValue value;
        public GroundedValueAsIterable(Saxon.Hej.om.GroundedValue value)  {
            this.value = value;
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.om.Item> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.WrappingJavaIterator<Saxon.Hej.om.Item>(value.iterate());
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    
    }
}