using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class SaxonElementFactoryEE : Saxon.Eej.xsltextn.style.SaxonElementFactory {
        public override System.Type getExtensionClass(string localname) {
            if (localname.Equals("array")) {
                return typeof(Saxon.Hej.style.XSLArray);
            }
            if (localname.Equals("array-member")) {
                return typeof(Saxon.Hej.style.XSLArrayMember);
            }
            if (localname.Equals("assign")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonAssign);
            }
            if (localname.Equals("change")) {
                return typeof(Saxon.Eej.ee.xtupdate.SaxonChange);
            }
            if (localname.Equals("deep-update")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonDeepUpdate);
            }
            if (localname.Equals("delete")) {
                return typeof(Saxon.Eej.ee.xtupdate.SaxonDelete);
            }
            if (localname.Equals("do")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonDo);
            }
            if (localname.Equals("doctype")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonDoctype);
            }
            if (localname.Equals("entity-ref")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonEntityRef);
            }
            if (localname.Equals("import-query")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonImportQuery);
            }
            if (localname.Equals("insert")) {
                return typeof(Saxon.Eej.ee.xtupdate.SaxonInsert);
            }
            if (localname.Equals("item-type")) {
                return typeof(Saxon.Hej.style.XSLItemType);
            }
            if (localname.Equals("rename")) {
                return typeof(Saxon.Eej.ee.xtupdate.SaxonRename);
            }
            if (localname.Equals("replace")) {
                return typeof(Saxon.Eej.ee.xtupdate.SaxonReplace);
            }
            if (localname.Equals("tabulate-maps")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonTabulateMaps);
            }
            if (localname.Equals("update")) {
                return typeof(Saxon.Eej.ee.xtupdate.SaxonUpdate);
            }
            if (localname.Equals("while")) {
                return typeof(Saxon.Eej.xsltextn.style.SaxonWhile);
            }
            return null;
        }
    
    }
}