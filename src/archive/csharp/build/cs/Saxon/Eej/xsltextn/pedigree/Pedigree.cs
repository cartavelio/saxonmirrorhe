using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.pedigree {
    public class Pedigree {
        private readonly Saxon.Eej.xsltextn.pedigree.PedigreeValue container;
        private int index = -1;
        private Saxon.Hej.value.AtomicValue key;
        public readonly static Saxon.Eej.xsltextn.pedigree.Pedigree PEDIGREE_ROOT = new Saxon.Eej.xsltextn.pedigree.Pedigree(null, null);
        public Pedigree(Saxon.Eej.xsltextn.pedigree.PedigreeValue container, Saxon.Hej.value.AtomicValue key)  {
            this.container = container;
            this.key = key;
        }
        public Pedigree(Saxon.Eej.xsltextn.pedigree.PedigreeValue container, int index)  {
            this.container = container;
            this.index = index;
        }
        public Saxon.Eej.xsltextn.pedigree.PedigreeValue getContainer() {
            return container;
        }
        public int getIndex() {
            return index;
        }
        public Saxon.Hej.value.AtomicValue getKey() {
            return key;
        }
        public Saxon.Hej.om.GroundedValue makePedigreeValue(Saxon.Hej.om.GroundedValue value) {
            if ((value is Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem)) {
                return new Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem(((Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem)(value)).getBaseItem(), this);
            } else {
                if ((value is Saxon.Eej.xsltextn.pedigree.PedigreeMapItem)) {
                    return new Saxon.Eej.xsltextn.pedigree.PedigreeMapItem(((Saxon.Eej.xsltextn.pedigree.PedigreeMapItem)(value)).getBaseItem(), this);
                } else {
                    if ((value is Saxon.Hej.ma.arrays.ArrayItem)) {
                        return new Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem((Saxon.Hej.ma.arrays.ArrayItem)(value), this);
                    } else {
                        if ((value is Saxon.Hej.ma.map.MapItem)) {
                            return new Saxon.Eej.xsltextn.pedigree.PedigreeMapItem((Saxon.Hej.ma.map.MapItem)(value), this);
                        } else {
                            return value;
                        }
                    }
                }
            }
        }
    
    }
}