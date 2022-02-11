using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMergeKey : Saxon.Hej.style.XSLSortOrMergeKey {
        protected internal override void prepareAttributes() {
            base.prepareAttributes();
            if ((stable != null)) {
                compileError("The @stable attribute is not allowed in xsl:merge-key", "XTSE0090");
            }
        }
        protected internal Saxon.Hej.type.ItemType getReturnedItemType() {
            return null;
        }
        protected internal override string getErrorCode() {
            return "XTSE3200";
        }
    
    }
}