using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    internal class CustomSetEntryChain<T>  {
        public T value;
        public Saxon.Hej.expr.sort.CustomSetEntryChain<T> next;
        public CustomSetEntryChain(T value)  {
            this.value = value;
            this.next = null;
        }
    
    }
}