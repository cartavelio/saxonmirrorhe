using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class EmptyIntIterator : Saxon.Hej.z.IntIterator {
        private static readonly Saxon.Hej.expr.sort.EmptyIntIterator THE_INSTANCE = new Saxon.Hej.expr.sort.EmptyIntIterator();
        public static Saxon.Hej.expr.sort.EmptyIntIterator getInstance() {
            return THE_INSTANCE;
        }
        private EmptyIntIterator()  {
        }
        public bool hasNext() {
            return false;
        }
        public int next() {
            return 0;
        }
    
    }
}