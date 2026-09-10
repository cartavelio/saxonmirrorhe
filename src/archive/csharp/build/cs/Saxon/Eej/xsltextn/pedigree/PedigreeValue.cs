using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.pedigree {
    public interface PedigreeValue {
        Saxon.Eej.xsltextn.pedigree.Pedigree getPedigree();
        Saxon.Eej.xsltextn.pedigree.PedigreeValue getOrigin();
    
    }
}