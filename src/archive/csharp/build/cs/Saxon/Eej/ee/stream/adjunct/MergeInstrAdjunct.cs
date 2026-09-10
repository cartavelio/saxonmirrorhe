using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class MergeInstrAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public static void checkStreamability(Saxon.Hej.expr.sort.MergeInstr.MergeSource mergeSource, Saxon.Hej.Configuration config) {
            string streamabilityOption = config.getConfigurationProperty(Saxon.Hej.lib.Feature<string>.STREAMABILITY);
            bool actuallyStreamable = true;
            System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
            if (streamabilityOption.Equals("off")) {
                reasons.Add("Streaming is disabled at the Saxon Configuration level");
                actuallyStreamable = false;
            } else {
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE info = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false, Saxon.Eej.ee.stream.Posture.STRIDING);
                Saxon.Hej.expr.Expression rowSelect = mergeSource.getRowSelect();
                Saxon.Hej.expr.Expression snapshot;
                if (rowSelect.isCallOn(typeof(Saxon.Hej.functions.SnapshotFn))) {
                    snapshot = rowSelect;
                    rowSelect = ((Saxon.Hej.expr.SystemFunctionCall)(rowSelect)).getArg(0);
                } else {
                    snapshot = Saxon.Hej.functions.SystemFunction.makeCall("snapshot", rowSelect.getRetainedStaticContext(), mergeSource.getRowSelect());
                    mergeSource.setRowSelect(snapshot);
                }
                Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(rowSelect, info, reasons);
                if ((ps.getPosture() != Saxon.Eej.ee.stream.Posture.STRIDING)) {
                    actuallyStreamable = false;
                    reasons.Add("The select expression for the merge source is not striding");
                } else {
                    /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(snapshot, info, reasons);
                    mergeSource.invertedAction = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(snapshot, false);
                }
            }
            if (!(actuallyStreamable)) {
                bool fallback = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK);
                if (fallback) {
                    Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(120);
                    buffer.append("Executing without streaming. ");
                    foreach (string s in reasons) {
                        buffer.append((s+". "));
                    }
                    config.makeErrorReporter().report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(buffer.ToString()).asWarning()));
                    mergeSource.streamable = false;
                } else {
                    Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(120);
                    buffer.append("Streaming requested for xsl:merge, but the construct is not streamable. ");
                    foreach (string s in reasons) {
                        buffer.append((s+". "));
                    }
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(buffer.ToString(), "XTSE3430");
                    err.setIsStaticError(true);
                    throw err;
                }
            }
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.sort.MergeInstr instr = (Saxon.Hej.expr.sort.MergeInstr)(getExpression());
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource ms in instr.getMergeSources()) {
                if (((ms.getForEachItem() != null) && !(/*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(ms.getForEachItem(), contextInfo, reasons).Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)))) {
                    reasons.Add("The for-each-item expression, if present, must be grounded and motionless");
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
                if (((ms.getForEachSource() != null) && !(/*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(ms.getForEachSource(), contextInfo, reasons).Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)))) {
                    reasons.Add("The for-each-source expression, if present, must be grounded and motionless");
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
                if ((((ms.getForEachItem() == null) && (ms.getForEachSource() == null)) && !(/*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(ms.getRowSelect(), contextInfo, reasons).Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)))) {
                    reasons.Add("If neither of the attributes for-each-item or for-each-source is present, the select expression must be grounded and motionless");
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            }
            return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
        public static System.Collections.Generic.IList<Saxon.Hej.expr.ContextMappingIterator> makeSourceIterators(Saxon.Hej.expr.sort.MergeInstr.MergeSource ms, Saxon.Hej.expr.PackageData pd, Saxon.Hej.expr.XPathContext c1) {
            System.Collections.Generic.IList<Saxon.Hej.expr.ContextMappingIterator> result = new System.Collections.Generic.List<Saxon.Hej.expr.ContextMappingIterator>();
            Saxon.Hej.om.SequenceIterator uriIter = ms.getForEachSource().iterate(c1);
            Saxon.Hej.om.Item uri;
            while (((uri = uriIter.next()) != null)) {
                Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(c1.getConfiguration().getParseOptions());
                options.setSchemaValidationMode(ms.validation);
                options.setTopLevelType(ms.schemaType);
                options.setApplicableAccumulators(ms.accumulators);
                string href = uri.getStringValue();
                Saxon.Eej.ee.parallel.Pusher pusher = new Saxon.Eej.ee.stream.adjunct.MergeInstrAdjunct.MergePusher(ms, pd, href, options);
                Saxon.Hej.om.SequenceIterator iter = new Saxon.Impl.Parallel.PushToPull(pusher, c1).getIterator();
                Saxon.Hej.expr.XPathContext c2 = c1.newMinorContext();
                c2.trackFocus(iter);
                Saxon.Hej.expr.sort.MergeInstr.MergeKeyMappingFunction addMergeKeys = new Saxon.Hej.expr.sort.MergeInstr.MergeKeyMappingFunction(c2, ms);
                Saxon.Hej.expr.ContextMappingIterator iter3 = new Saxon.Hej.expr.ContextMappingIterator(it => addMergeKeys.map(it) /* converted from instance method reference */, c2);
                result.Add(iter3);
            }
            return result;
        }
        public class MergePusher : Saxon.Eej.ee.parallel.Pusher {
            private readonly Saxon.Hej.expr.sort.MergeInstr.MergeSource ms;
            private readonly Saxon.Hej.expr.PackageData pd;
            private readonly string href;
            private readonly Saxon.Hej.lib.ParseOptions options;
            public MergePusher(Saxon.Hej.expr.sort.MergeInstr.MergeSource ms, Saxon.Hej.expr.PackageData pd, string href, Saxon.Hej.lib.ParseOptions options)  {
                this.ms = ms;
                this.pd = pd;
                this.href = href;
                this.options = options;
            }
            public void push(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.Controller controller = context.getController();
                Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                c2.setCurrentTemplateRule(null);
                Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                pipe.setXPathContext(context);
                Saxon.Eej.ee.stream.watch.WatchManager wm = new Saxon.Eej.ee.stream.watch.WatchManager(pipe);
                wm.setXPathContext(context);
                wm.setOutputter(output);
                Saxon.Eej.ee.stream.AccumulatorRegistryEE am = (Saxon.Eej.ee.stream.AccumulatorRegistryEE)(pd.getAccumulatorRegistry());
                if (((am != null) && (ms.accumulators != null))) {
                    /*134*/Saxon.Eej.ee.stream.AccumulatorRegistryEE.registerSelectedAccumulators(wm, ms.accumulators);
                }
                Saxon.Hej.@event.Receiver @out = wm;
                if (((pd is Saxon.Hej.style.StylesheetPackage) && ((Saxon.Hej.style.StylesheetPackage)(pd)).isStripsTypeAnnotations())) {
                    @out = pd.getConfiguration().getAnnotationStripper(@out);
                }
                wm.addWatch(new Saxon.Eej.ee.stream.watch.StreamWatch((Saxon.Eej.ee.stream.Inversion)(ms.invertedAction), wm, c2), true);
                Saxon.Hej.functions.DocumentFn.sendDoc(href, ms.baseURI, pd, context, ms.location, @out, options);
            }
        
        }
    
    }
}