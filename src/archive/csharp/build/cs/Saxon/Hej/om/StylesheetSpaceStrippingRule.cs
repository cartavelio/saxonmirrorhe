using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class StylesheetSpaceStrippingRule : Saxon.Hej.om.SpaceStrippingRule {
        private static readonly int[] specials = {Saxon.Hej.om.StandardNames.XSL_ANALYZE_STRING, Saxon.Hej.om.StandardNames.XSL_APPLY_IMPORTS, Saxon.Hej.om.StandardNames.XSL_APPLY_TEMPLATES, Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, Saxon.Hej.om.StandardNames.XSL_CALL_TEMPLATE, Saxon.Hej.om.StandardNames.XSL_CHARACTER_MAP, Saxon.Hej.om.StandardNames.XSL_CHOOSE, Saxon.Hej.om.StandardNames.XSL_EVALUATE, Saxon.Hej.om.StandardNames.XSL_MERGE, Saxon.Hej.om.StandardNames.XSL_MERGE_SOURCE, Saxon.Hej.om.StandardNames.XSL_NEXT_ITERATION, Saxon.Hej.om.StandardNames.XSL_NEXT_MATCH, Saxon.Hej.om.StandardNames.XSL_STYLESHEET, Saxon.Hej.om.StandardNames.XSL_TRANSFORM};
        private readonly Saxon.Hej.om.NamePool namePool;
        public StylesheetSpaceStrippingRule(Saxon.Hej.om.NamePool pool)  {
            this.namePool = pool;
        }
        public int isSpacePreserving(Saxon.Hej.om.NodeName elementName, Saxon.Hej.type.SchemaType schemaType) {
            int fingerprint = elementName.obtainFingerprint(namePool);
            if ((fingerprint == ((Saxon.Hej.om.StandardNames.XSL_TEXT&Saxon.Hej.om.NamePool.FP_MASK)))) {
                return Saxon.Hej.@event.Stripper.ALWAYS_PRESERVE;
            }
            if ((Saxon.Ejava.util.Arrays.binarySearch(specials, fingerprint)>=0)) {
                return Saxon.Hej.@event.Stripper.ALWAYS_STRIP;
            }
            return Saxon.Hej.@event.Stripper.STRIP_DEFAULT;
        }
        public Saxon.Hej.@event.ProxyReceiver makeStripper(Saxon.Hej.@event.Receiver next) {
            return new Saxon.Hej.@event.Stripper(this, next);
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
        }
    
    }
}