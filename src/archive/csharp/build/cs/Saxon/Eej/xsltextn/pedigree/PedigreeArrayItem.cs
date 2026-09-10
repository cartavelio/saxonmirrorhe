using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.pedigree {
    public class PedigreeArrayItem : Saxon.Eej.xsltextn.pedigree.DelegatingArrayItem, Saxon.Eej.xsltextn.pedigree.PedigreeValue {
        internal Saxon.Eej.xsltextn.pedigree.Pedigree pedigree;
        public PedigreeArrayItem(Saxon.Hej.ma.arrays.ArrayItem @base) : this (@base, Saxon.Eej.xsltextn.pedigree.Pedigree.PEDIGREE_ROOT) {
        }
        public PedigreeArrayItem(Saxon.Hej.ma.arrays.ArrayItem @base, Saxon.Eej.xsltextn.pedigree.Pedigree pedigree) : base (@base) {
            this.pedigree = pedigree;
        }
        public override Saxon.Hej.om.GroundedValue get(int index) {
            Saxon.Hej.om.GroundedValue value = base.get(index);
            Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, index);
            return ped.makePedigreeValue(value);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem remove(int index) {
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>((arrayLength()-1));
            int i = 0;
            foreach (Saxon.Hej.om.GroundedValue seq in base.remove(index).members()) {
                if ((i == index)) {
                    i++;
                }
                Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, i++);
                list.Add(ped.makePedigreeValue(seq));
            }
            return new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem subArray(int start, int end) {
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> list = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>((end-start));
            int index = start;
            foreach (Saxon.Hej.om.GroundedValue seq in base.subArray(start, end).members()) {
                Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, index++);
                list.Add(ped.makePedigreeValue(seq));
            }
            return new Saxon.Hej.ma.arrays.SimpleArrayItem(list);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.om.GroundedValue> members() {
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> memberValues = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
            int index = 0;
            foreach (Saxon.Hej.om.GroundedValue val in base.members()) {
                Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, index++);
                memberValues.Add(ped.makePedigreeValue(val));
            }
            return memberValues;
        }
        public Saxon.Eej.xsltextn.pedigree.Pedigree getPedigree() {
            return pedigree;
        }
        public override Saxon.Hej.ma.arrays.ArrayItem getBaseItem() {
            return base.getBaseItem();
        }
    
        /* Expanding default method getOrigin from interface PedigreeValue */
        public virtual Saxon.Eej.xsltextn.pedigree.PedigreeValue getOrigin() {
            Saxon.Eej.xsltextn.pedigree.Pedigree ped = getPedigree();
            if ((ped == Saxon.Eej.xsltextn.pedigree.Pedigree.PEDIGREE_ROOT)) {
                return this;
            } else {
                Saxon.Eej.xsltextn.pedigree.PedigreeValue parent = ped.getContainer();
                return parent.getOrigin();
            }
        }

    }
}