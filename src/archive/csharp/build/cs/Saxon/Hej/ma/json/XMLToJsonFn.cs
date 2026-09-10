using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class XMLToJsonFn : Saxon.Hej.functions.SystemFunction, Saxon.Hej.functions.PushableFunction {
        private readonly static Saxon.Hej.type.FunctionItemType formatterFunctionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.SINGLE_STRING);
        public static Saxon.Hej.functions.OptionsParameter makeOptionsParameter() {
            Saxon.Hej.functions.OptionsParameter xmlToJsonOptions = new Saxon.Hej.functions.OptionsParameter();
            xmlToJsonOptions.addAllowedOption("indent", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            xmlToJsonOptions.addAllowedOption("number-formatter", Saxon.Hej.value.SequenceType.makeSequenceType(formatterFunctionType, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE), Saxon.Hej.value.EmptySequence.getInstance());
            return xmlToJsonOptions;
        }
        private class Options {
            public bool indent;
            public Saxon.Hej.om.FunctionItem numberFormatter;
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo xml = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((xml == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.ma.json.XMLToJsonFn.Options options = getOptions(context, arguments);
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            pipe.setXPathContext(context);
            Saxon.Hej.str.UnicodeBuilder uniBuffer = new Saxon.Hej.str.UnicodeBuilder();
            convertToJson(xml, uniBuffer, options, context);
            return new Saxon.Hej.value.StringValue(uniBuffer.toUnicodeString());
        }
        private Saxon.Hej.ma.json.XMLToJsonFn.Options getOptions(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            if ((getArity()>1)) {
                Saxon.Hej.ma.map.MapItem suppliedOptions = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options = getDetails().optionDetails.processSuppliedOptions(suppliedOptions, context);
                Saxon.Hej.ma.json.XMLToJsonFn.Options o = new Saxon.Hej.ma.json.XMLToJsonFn.Options();
                o.indent = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "indent").head())).getBooleanValue();
                Saxon.Hej.om.Sequence format = Saxon.Impl.Helpers.MapUtils.Get(options, "number-formatter");
                if ((format != null)) {
                    o.numberFormatter = (Saxon.Hej.om.FunctionItem)(format.head());
                }
                return o;
            } else {
                return new Saxon.Hej.ma.json.XMLToJsonFn.Options();
            }
        }
        public void process(Saxon.Hej.@event.Outputter destination, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo xml = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((xml != null)) {
                Saxon.Hej.ma.json.XMLToJsonFn.Options options = getOptions(context, arguments);
                Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                pipe.setXPathContext(context);
                convertToJson(xml, destination.getStringReceiver(false, Saxon.Hej.expr.parser.Loc.NONE), options, context);
            }
        }
        private void convertToJson(Saxon.Hej.om.NodeInfo xml, Saxon.Hej.str.UniStringConsumer output, Saxon.Hej.ma.json.XMLToJsonFn.Options options, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            pipe.setXPathContext(context);
            Saxon.Hej.ma.json.JsonReceiver receiver = new Saxon.Hej.ma.json.JsonReceiver(pipe, context, output);
            receiver.setIndenting(options.indent);
            if ((options.numberFormatter != null)) {
                receiver.setNumberFormatter(options.numberFormatter);
            }
            Saxon.Hej.@event.Receiver r = receiver;
            if ((xml.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                r = new Saxon.Hej.@event.DocumentValidator(r, "FOJS0006");
            }
            r.open();
            xml.copy(r, 0, Saxon.Hej.expr.parser.Loc.NONE);
            r.close();
        }
        public override string getStreamerName() {
            return "XmlToJsonFn";
        }
    
    }
}