using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Parse : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration config = controller.getConfiguration();
            Saxon.Hej.value.AtomicValue content = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            System.IO.StringReader sr = new System.IO.StringReader(content.getStringValue());
            Saxon.Ejavax.xml.transform.Source source = new Saxon.Ejavax.xml.transform.stream.StreamSource(sr, getStaticBaseUriString());
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions;
            Saxon.Hej.lib.ParseOptions parseOptions;
            if ((arguments.Length == 1)) {
                checkedOptions = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>();
                parseOptions = new Saxon.Hej.lib.ParseOptions();
            } else {
                checkedOptions = getDetails().optionDetails.processSuppliedOptions((Saxon.Hej.ma.map.MapItem)(arguments[1].head()), context);
                parseOptions = Saxon.Hej.functions.Doc_2.setParseOptions(getRetainedStaticContext(), checkedOptions, context);
            }
            Saxon.Hej.om.Sequence suppliedBaseUriVal = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "base-uri");
            if (((suppliedBaseUriVal == null) || (suppliedBaseUriVal.head() == null))) {
                source.setSystemId(getStaticBaseUriString());
            } else {
                source.setSystemId(suppliedBaseUriVal.head().getStringValue());
            }
            Saxon.Hej.@event.Builder b = controller.makeBuilder();
            if ((b is Saxon.Hej.tree.tiny.TinyBuilder)) {
                ((Saxon.Hej.tree.tiny.TinyBuilder)(b)).setStatistics(config.getTreeStatistics().FN_PARSE_STATISTICS);
            }
            Saxon.Hej.@event.Receiver s = b;
            Saxon.Hej.expr.PackageData pd = getRetainedStaticContext().getPackageData();
            if ((Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "strip-space") == null)) {
                if ((pd is Saxon.Hej.style.StylesheetPackage)) {
                    parseOptions = parseOptions.withSpaceStrippingRule(((Saxon.Hej.style.StylesheetPackage)(pd)).getSpaceStrippingRule());
                    if (((Saxon.Hej.style.StylesheetPackage)(pd)).isStripsTypeAnnotations()) {
                        s = config.getAnnotationStripper(s);
                    }
                } else {
                    parseOptions = parseOptions.withSpaceStrippingRule(Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance());
                }
            }
            parseOptions = parseOptions.withErrorReporter(context.getController().getErrorReporter());
            parseOptions = setXMLReader(config, parseOptions);
            s.setPipelineConfiguration(b.getPipelineConfiguration());
            Saxon.Hej.@event.Sender.send(source, s, parseOptions);
            Saxon.Hej.om.NodeInfo node = b.getCurrentRoot();
            b.reset();
            return node;
        }
        protected internal Saxon.Hej.lib.ParseOptions setXMLReader(Saxon.Hej.Configuration config, Saxon.Hej.lib.ParseOptions source) {
            return source;
        }
    
    }
}