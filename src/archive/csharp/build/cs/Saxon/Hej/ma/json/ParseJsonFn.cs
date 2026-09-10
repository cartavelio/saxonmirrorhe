using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class ParseJsonFn : Saxon.Hej.ma.json.JsonToXMLFn {

        static ParseJsonFn() {
            Saxon.Hej.type.SpecificFunctionType fallbackType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.SINGLE_STRING);
            Saxon.Hej.type.SpecificFunctionType parserType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
            Saxon.Hej.functions.OptionsParameter parseJsonOptions = new Saxon.Hej.functions.OptionsParameter();
            parseJsonOptions.addAllowedOption("liberal", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            parseJsonOptions.addAllowedOption("duplicates", Saxon.Hej.value.SequenceType.SINGLE_STRING, Saxon.Hej.value.StringValue.bmp("use-first"));
            parseJsonOptions.setAllowedValues("duplicates", "FOJS0005", "reject", "use-first", "use-last");
            parseJsonOptions.addAllowedOption("escape", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            parseJsonOptions.addAllowedOption("fallback", Saxon.Hej.value.SequenceType.makeSequenceType(fallbackType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), null);
            parseJsonOptions.addAllowedOption("number-parser", Saxon.Hej.value.SequenceType.makeSequenceType(parserType, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), null);
            OPTION_DETAILS = parseJsonOptions;
        }
        public static new Saxon.Hej.functions.OptionsParameter OPTION_DETAILS;
        protected internal override Saxon.Hej.om.Item eval(string input, Saxon.Hej.ma.map.MapItem options, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions = null;
            if ((options != null)) {
                checkedOptions = getDetails().optionDetails.processSuppliedOptions(options, context);
            }
            return parse(input, checkedOptions, context);
        }
        public static Saxon.Hej.om.Item parse(string input, System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.json.JsonParser parser = new Saxon.Hej.ma.json.JsonParser();
            int flags = 0;
            if ((options != null)) {
                flags = Saxon.Hej.ma.json.JsonParser.getFlags(options, false, false);
            }
            Saxon.Hej.ma.json.JsonHandlerMap handler = new Saxon.Hej.ma.json.JsonHandlerMap(context, flags);
            if ((((flags&Saxon.Hej.ma.json.JsonParser.DUPLICATES_RETAINED)) != 0)) {
                throw new Saxon.Hej.trans.XPathException("parse-json: duplicates=retain is not allowed", "FOJS0005");
            }
            if ((((flags&Saxon.Hej.ma.json.JsonParser.DUPLICATES_SPECIFIED)) == 0)) {
                flags |= Saxon.Hej.ma.json.JsonParser.DUPLICATES_FIRST;
            }
            if ((options != null)) {
                handler.setFallbackFunction(options, context);
                parser.setNumberParser(options, context);
            }
            parser.parse(input, flags, handler, context);
            return handler.getResult().head();
        }
    
    }
}