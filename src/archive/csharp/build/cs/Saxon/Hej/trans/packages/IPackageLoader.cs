using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.packages {
    public interface IPackageLoader {
        Saxon.Hej.style.StylesheetPackage loadPackageDoc(Saxon.Hej.om.NodeInfo doc);
        Saxon.Hej.style.StylesheetPackage loadPackage(Saxon.Ejavax.xml.transform.Source source);
    
    }
}