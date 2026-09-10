using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonElementFactory {
        public readonly static Saxon.Eej.xsltextn.ExtensionElementCreator FACTORY = (localname => {
            if (localname.Equals("array")) {
                return new Saxon.Hej.style.XSLArray();
            }
            if (localname.Equals("array-member")) {
                return new Saxon.Hej.style.XSLArrayMember();
            }
            if (localname.Equals("assign")) {
                return new Saxon.Eej.xsltextn.style.SaxonAssign();
            }
            if (localname.Equals("deep-update")) {
                return new Saxon.Eej.xsltextn.style.SaxonDeepUpdate();
            }
            if (localname.Equals("do")) {
                return new Saxon.Eej.xsltextn.style.SaxonDo();
            }
            if (localname.Equals("doctype")) {
                return new Saxon.Eej.xsltextn.style.SaxonDoctype();
            }
            if (localname.Equals("entity-ref")) {
                return new Saxon.Eej.xsltextn.style.SaxonEntityRef();
            }
            if (localname.Equals("import-query")) {
                return new Saxon.Eej.xsltextn.style.SaxonImportQuery();
            }
            if (localname.Equals("item-type")) {
                return new Saxon.Hej.style.XSLItemType();
            }
            if (localname.Equals("tabulate-maps")) {
                return new Saxon.Eej.xsltextn.style.SaxonTabulateMaps();
            }
            if (localname.Equals("while")) {
                return new Saxon.Eej.xsltextn.style.SaxonWhile();
            }
            return null;
        } );
    
    }
}