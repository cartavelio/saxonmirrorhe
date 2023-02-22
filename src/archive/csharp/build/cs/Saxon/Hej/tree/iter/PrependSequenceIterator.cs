using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class PrependSequenceIterator : Saxon.Hej.om.SequenceIterator {
        internal Saxon.Hej.om.Item start;
        internal Saxon.Hej.om.SequenceIterator @base;
        public PrependSequenceIterator(Saxon.Hej.om.Item start, Saxon.Hej.om.SequenceIterator @base)  {
            this.start = start;
            this.@base = @base;
        }
        public virtual Saxon.Hej.om.Item next() {
            if ((start != null)) {
                Saxon.Hej.om.Item temp = start;
                start = null;
                return temp;
            } else {
                return @base.next();
            }
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
    
    }
}