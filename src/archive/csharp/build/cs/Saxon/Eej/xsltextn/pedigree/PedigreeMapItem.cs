using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.pedigree {
    public class PedigreeMapItem : Saxon.Eej.xsltextn.pedigree.DelegatingMapItem, Saxon.Eej.xsltextn.pedigree.PedigreeValue {
        private readonly Saxon.Eej.xsltextn.pedigree.Pedigree pedigree;
        public PedigreeMapItem(Saxon.Hej.ma.map.MapItem @base) : this (@base, Saxon.Eej.xsltextn.pedigree.Pedigree.PEDIGREE_ROOT) {
        }
        public PedigreeMapItem(Saxon.Hej.ma.map.MapItem @base, Saxon.Eej.xsltextn.pedigree.Pedigree pedigree) : base (@base) {
            this.pedigree = pedigree;
        }
        public override Saxon.Hej.om.GroundedValue get(Saxon.Hej.value.AtomicValue key) {
            Saxon.Hej.om.GroundedValue value = base.get(key);
            if ((value is Saxon.Hej.ma.arrays.ArrayItem)) {
                Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, key);
                return new Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem((Saxon.Hej.ma.arrays.ArrayItem)(value), ped);
            } else {
                if ((value is Saxon.Hej.ma.map.MapItem)) {
                    Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, key);
                    return new Saxon.Eej.xsltextn.pedigree.PedigreeMapItem((Saxon.Hej.ma.map.MapItem)(value), ped);
                } else {
                    return value;
                }
            }
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.ma.map.KeyValuePair> keyValuePairs() {
            System.Collections.Generic.IList<Saxon.Hej.ma.map.KeyValuePair> pairs = new System.Collections.Generic.List<Saxon.Hej.ma.map.KeyValuePair>();
            foreach (Saxon.Hej.ma.map.KeyValuePair val in base.keyValuePairs()) {
                Saxon.Eej.xsltextn.pedigree.Pedigree ped = new Saxon.Eej.xsltextn.pedigree.Pedigree(this, val.key);
                Saxon.Hej.om.GroundedValue newValue = ped.makePedigreeValue(val.value);
                pairs.Add(new Saxon.Hej.ma.map.KeyValuePair(val.key, newValue));
            }
            return pairs;
        }
        public Saxon.Eej.xsltextn.pedigree.Pedigree getPedigree() {
            return pedigree;
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