using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class UnparsedTextLines : Saxon.Hej.functions.UnparsedTextFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue hrefVal = (Saxon.Hej.value.StringValue)(arguments[0].head());
            string encoding = ((getArity() == 2) ? arguments[1].head().getStringValue() : null);
            try {
                return Saxon.Hej.om.SequenceTool.toLazySequence(evalUnparsedTextLines(hrefVal, encoding, context));
            } catch (Saxon.Hej.trans.XPathException e) {
                if ((e.getErrorCodeQName() == null)) {
                    e.setErrorCode("FOUT1170");
                }
                if (((getArity() == 2) && "FOUT1200".Equals(e.getErrorCodeLocalPart()))) {
                    e.setErrorCode("FOUT1190");
                }
                throw;
            }
        }
        private Saxon.Hej.om.SequenceIterator evalUnparsedTextLines(Saxon.Hej.value.StringValue hrefVal, string encoding, Saxon.Hej.expr.XPathContext context) {
            if ((hrefVal == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
            }
            string href = hrefVal.getStringValue();
            bool stable = context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STABLE_UNPARSED_TEXT);
            if (stable) {
                Saxon.Hej.value.StringValue content = Saxon.Hej.functions.UnparsedText.evalUnparsedText(hrefVal, getStaticBaseUriString(), encoding, context);
                System.Diagnostics.Debug.Assert((content != null));
                System.Uri abs = Saxon.Hej.functions.UnparsedTextFunction.getAbsoluteURI(href, getStaticBaseUriString(), context);
                Saxon.Impl.Helpers.LineNumberReader reader = new Saxon.Impl.Helpers.LineNumberReader(new System.IO.StringReader(content.getStringValue()));
                return new Saxon.Hej.tree.iter.UnparsedTextIterator(reader, abs, context, encoding);
            } else {
                System.Uri absoluteURI = Saxon.Hej.functions.UnparsedTextFunction.getAbsoluteURI(href, getRetainedStaticContext().getStaticBaseUriString(), context);
                return new Saxon.Hej.tree.iter.UnparsedTextIterator(absoluteURI, context, encoding, null);
            }
        }
    
    }
}