using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.pedigree {
    public abstract class DelegatingArrayItem : Saxon.Hej.ma.arrays.AbstractArrayItem {
        private readonly Saxon.Hej.ma.arrays.ArrayItem @base;
        public DelegatingArrayItem(Saxon.Hej.ma.arrays.ArrayItem @base)  {
            this.@base = @base;
        }
        public override Saxon.Hej.om.GroundedValue get(int index) {
            return @base.get(index);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem put(int index, Saxon.Hej.om.GroundedValue newValue) {
            return @base.put(index, newValue);
        }
        public override int arrayLength() {
            return @base.arrayLength();
        }
        public override bool isEmpty() {
            return @base.isEmpty();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.om.GroundedValue> members() {
            return @base.members();
        }
        public override Saxon.Hej.ma.arrays.ArrayItem concat(Saxon.Hej.ma.arrays.ArrayItem other) {
            return @base.concat(other);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem remove(int index) {
            return @base.remove(index);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem removeSeveral(Saxon.Hej.z.IntSet positions) {
            return @base.removeSeveral(positions);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem subArray(int start, int end) {
            return @base.subArray(start, end);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem insert(int position, Saxon.Hej.om.GroundedValue member) {
            return @base.insert(position, member);
        }
        public virtual Saxon.Hej.ma.arrays.ArrayItem getBaseItem() {
            return @base;
        }
    
    }
}