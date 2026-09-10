using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonDoc : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item arg0 = arguments[0].head();
            if ((arg0 == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string href = arg0.getStringValue();
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.z.IntPredicateProxy checker = Saxon.Hej.z.IntSetPredicate.ALWAYS_TRUE;
            System.Uri absoluteURI = Saxon.Hej.functions.UnparsedTextFunction.getAbsoluteURI(href, getStaticBaseUriString(), context);
            string encoding = "UTF-8";
            System.IO.TextReader reader;
            try {
                reader = context.getController().getUnparsedTextURIResolver().resolve(absoluteURI, encoding, config);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetErrorCode("FOUT1170");
                throw;
            }
            if ((reader == null)) {
                throw new Saxon.Hej.trans.XPathException(("Unable to resolve json-doc() URI "+absoluteURI), "FOUT1170");
            }
            Saxon.Hej.str.UnicodeString content;
            try {
                content = Saxon.Hej.functions.UnparsedTextFunction.readFile(checker, reader);
            } catch (Saxon.Ejava.io.UnsupportedEncodingException encErr) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("Unknown encoding "+Saxon.Hej.trans.Err.wrap(encoding)), encErr);
                e.setErrorCode("FOUT1190");
                throw e;
            } catch (System.IO.IOException ioErr) {
                throw Saxon.Hej.functions.UnparsedTextFunction.handleIOError(absoluteURI, ioErr);
            }
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions;
            if ((getArity() == 2)) {
                Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                checkedOptions = getDetails().optionDetails.processSuppliedOptions(options, context);
            } else {
                checkedOptions = Saxon.Hej.ma.json.ParseJsonFn.OPTION_DETAILS.getDefaultOptions();
            }
            Saxon.Hej.om.Item result = Saxon.Hej.ma.json.ParseJsonFn.parse(content.ToString(), checkedOptions, context);
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(result);
        }
    
    }
}