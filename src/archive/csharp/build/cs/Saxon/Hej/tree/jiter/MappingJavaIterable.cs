using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.jiter {
    public class MappingJavaIterable<S, T>  : System.Collections.Generic.IEnumerable<T> {
        private readonly System.Collections.Generic.IEnumerable<S> input;
        private readonly System.Func<S,T> mapper;
        public MappingJavaIterable(System.Collections.Generic.IEnumerable<S> @in, System.Func<S,T> mapper)  {
            this.input = @in;
            this.mapper = mapper;
        }
        public System.Collections.Generic.IEnumerator<T> GetEnumerator() {
            return new Saxon.Impl.Overrides.MappingEnumerator<S,T>(input.GetEnumerator(), mapper);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    
    }
}