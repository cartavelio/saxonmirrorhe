using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Doc_2 : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public Doc_2()  {
        }
        public static Saxon.Hej.functions.OptionsParameter makeOptionsParameter() {
            Saxon.Hej.value.SequenceType listOfQNames = Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.type.BuiltInAtomicType.QNAME, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            Saxon.Hej.functions.OptionsParameter op = new Saxon.Hej.functions.OptionsParameter();
            op.addAllowedOption("base-uri", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("validation", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.setAllowedValues("validation", "SXZZ0001", "strict", "lax", "preserve", "strip", "skip");
            op.addAllowedOption("type", Saxon.Hej.value.SequenceType.SINGLE_QNAME);
            op.addAllowedOption("strip-space", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.setAllowedValues("strip-space", "SXZZ0001", "none", "all", "ignorable", "package-defined", "default");
            op.addAllowedOption("stable", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("dtd-validation", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("accumulators", listOfQNames);
            op.addAllowedOption("use-xsi-schema-location", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            return op;
        }
        public static Saxon.Hej.lib.ParseOptions setParseOptions(Saxon.Hej.expr.parser.RetainedStaticContext rsc, System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ParseOptions result = context.getConfiguration().getParseOptions();
            Saxon.Hej.om.Sequence value = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "validation");
            if ((value != null)) {
                string valStr = value.head().getStringValue();
                if ("skip".Equals(valStr)) {
                    valStr = "strip";
                }
                int v = Saxon.Hej.lib.Validation.getCode(valStr);
                if ((v == Saxon.Hej.lib.Validation.INVALID)) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid validation value "+valStr), "SXZZ0002");
                }
                result = result.withSchemaValidationMode(v);
            }
            value = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "type");
            if ((value != null)) {
                Saxon.Hej.value.QNameValue qval = (Saxon.Hej.value.QNameValue)(value.head());
                result = result.withTopLevelType(context.getConfiguration().getSchemaType(qval.getStructuredQName())).withSchemaValidationMode(Saxon.Hej.lib.Validation.BY_TYPE);
            }
            value = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "strip-space");
            if ((value != null)) {
                string s = value.head().getStringValue();
                switch (s) {
                  case "all":
                    result = result.withSpaceStrippingRule(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
                    break ;
                  case "none":
                    result = result.withSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
                    break ;
                  case "ignorable":
                    result = result.withSpaceStrippingRule(Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance());
                    break ;
                  case "package-defined":
                  case "default":
                    Saxon.Hej.expr.PackageData data = rsc.getPackageData();
                    if ((data is Saxon.Hej.style.StylesheetPackage)) {
                        result = result.withSpaceStrippingRule(((Saxon.Hej.style.StylesheetPackage)(data)).getSpaceStrippingRule());
                    }
                    break ;
                }
            }
            value = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "dtd-validation");
            if ((value != null)) {
                result = result.withDTDValidationMode((((Saxon.Hej.value.BooleanValue)(value.head())).getBooleanValue() ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.SKIP));
            }
            value = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "accumulators");
            if ((value != null)) {
                Saxon.Hej.expr.accum.AccumulatorRegistry reg = rsc.getPackageData().getAccumulatorRegistry();
                System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
                Saxon.Hej.om.SequenceIterator iter = value.iterate();
                Saxon.Hej.om.Item it;
                while (((it = iter.next()) != null)) {
                    Saxon.Hej.value.QNameValue name = (Saxon.Hej.value.QNameValue)(it);
                    Saxon.Hej.expr.accum.Accumulator acc = reg.getAccumulator(name.getStructuredQName());
                    accumulators.Add(acc);
                }
                result = result.withApplicableAccumulators(accumulators);
            }
            value = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "use-xsi-schema-location");
            if ((value != null)) {
                result = result.withUseXsiSchemaLocation(((Saxon.Hej.value.BooleanValue)(value.head())).getBooleanValue());
            }
            return result;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue hrefVal = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((hrefVal == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string href = hrefVal.getStringValue();
            Saxon.Hej.om.Item param = arguments[1].head();
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions = getDetails().optionDetails.processSuppliedOptions((Saxon.Hej.ma.map.MapItem)(param), context);
            Saxon.Hej.lib.ParseOptions parseOptions = setParseOptions(getRetainedStaticContext(), checkedOptions, context);
            Saxon.Hej.om.NodeInfo item = fetch(href, parseOptions, context).getRootNode();
            if ((item == null)) {
                throw new Saxon.Hej.trans.XPathException(("Failed to load document "+href), "FODC0002", context);
            }
            Saxon.Hej.Controller controller = context.getController();
            if ((controller is Saxon.Hej.trans.XsltController)) {
                ((Saxon.Hej.trans.XsltController)(controller)).getAccumulatorManager().setApplicableAccumulators(item.getTreeInfo(), parseOptions.getApplicableAccumulators());
            }
            return item;
        }
        private Saxon.Hej.om.TreeInfo fetch(string href, Saxon.Hej.lib.ParseOptions options, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.Controller controller = context.getController();
            System.Uri abs;
            try {
                abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, getStaticBaseUriString());
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(("Invalid URI supplied to saxon:doc - "+e.Message), "FODC0002");
            }
            Saxon.Ejavax.xml.transform.Source source = config.getSourceResolver().resolveSource(new Saxon.Ejavax.xml.transform.stream.StreamSource(abs.OriginalString), config);
            Saxon.Hej.om.TreeInfo newdoc;
            if (Saxon.Hej.functions.DocumentFn.sourceIsTree(source)) {
                Saxon.Hej.om.NodeInfo startNode = controller.prepareInputTree(source);
                newdoc = startNode.getTreeInfo();
            } else {
                Saxon.Hej.@event.Builder b = controller.makeBuilder();
                b.setDurability(Saxon.Hej.om.Durability.LASTING);
                if ((b is Saxon.Hej.tree.tiny.TinyBuilder)) {
                    ((Saxon.Hej.tree.tiny.TinyBuilder)(b)).setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                }
                b.setPipelineConfiguration(b.getPipelineConfiguration());
                try {
                    Saxon.Hej.@event.Sender.send(source, b, options);
                    newdoc = b.getCurrentRoot().getTreeInfo();
                    b.reset();
                } finally {
                    if (options.isPleaseCloseAfterUse()) {
                        Saxon.Hej.lib.ParseOptions.close(source);
                    }
                }
            }
            return newdoc;
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
        }
    
    }
}