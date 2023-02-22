using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonToXMLFn : Saxon.Hej.functions.SystemFunction {

        static JsonToXMLFn() {
            Saxon.Hej.type.SpecificFunctionType fallbackType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Hej.functions.OptionsParameter jsonToXmlOptions = new Saxon.Hej.functions.OptionsParameter();
            jsonToXmlOptions.addAllowedOption("liberal", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            jsonToXmlOptions.addAllowedOption("duplicates", Saxon.Hej.value.SequenceType.SINGLE_STRING, null);
            jsonToXmlOptions.setAllowedValues("duplicates", "FOJS0005", "reject", "use-first", "retain");
            jsonToXmlOptions.addAllowedOption("validate", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            jsonToXmlOptions.addAllowedOption("escape", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            jsonToXmlOptions.addAllowedOption("fallback", Saxon.Hej.value.SequenceType.makeSequenceType(fallbackType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), null);
            OPTION_DETAILS = jsonToXmlOptions;
        }
        public static Saxon.Hej.functions.OptionsParameter OPTION_DETAILS;
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item arg0 = arguments[0].head();
            if ((arg0 == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string input = arg0.getStringValue();
            Saxon.Hej.ma.map.MapItem options = null;
            if ((getArity() == 2)) {
                options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
            }
            Saxon.Hej.om.Item result = eval(input, options, context);
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(result);
        }
        protected internal virtual Saxon.Hej.om.Item eval(string input, Saxon.Hej.ma.map.MapItem options, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.json.JsonParser parser = new Saxon.Hej.ma.json.JsonParser();
            int flags = 0;
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions = null;
            if ((options != null)) {
                checkedOptions = getDetails().optionDetails.processSuppliedOptions(options, context);
                flags = Saxon.Hej.ma.json.JsonParser.getFlags(checkedOptions, context, true);
                if ((((flags&Saxon.Hej.ma.json.JsonParser.DUPLICATES_LAST)) != 0)) {
                    throw new Saxon.Hej.trans.XPathException("json-to-xml: duplicates=use-last is not allowed", "FOJS0005");
                }
                if ((((flags&Saxon.Hej.ma.json.JsonParser.DUPLICATES_SPECIFIED)) == 0)) {
                    if ((((flags&Saxon.Hej.ma.json.JsonParser.VALIDATE)) != 0)) {
                        flags |= Saxon.Hej.ma.json.JsonParser.DUPLICATES_REJECTED;
                    } else {
                        flags |= Saxon.Hej.ma.json.JsonParser.DUPLICATES_RETAINED;
                    }
                }
            } else {
                flags = Saxon.Hej.ma.json.JsonParser.DUPLICATES_RETAINED;
            }
            Saxon.Hej.ma.json.JsonHandlerXML handler = new Saxon.Hej.ma.json.JsonHandlerXML(context, getStaticBaseUriString(), flags);
            if ((options != null)) {
                handler.setFallbackFunction(checkedOptions, context);
            }
            parser.parse(input, flags, handler, context);
            return handler.getResult();
        }
    
    }
}