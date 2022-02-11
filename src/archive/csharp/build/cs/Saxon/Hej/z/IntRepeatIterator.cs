using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntRepeatIterator : Saxon.Hej.z.IntIterator {
        private readonly int value;
        private int count;
        public IntRepeatIterator(int value, int count)  {
            this.value = value;
            this.count = count;
        }
        public bool hasNext() {
            return (count>0);
        }
        public int next() {
            count--;
            return value;
        }
    
    }
}