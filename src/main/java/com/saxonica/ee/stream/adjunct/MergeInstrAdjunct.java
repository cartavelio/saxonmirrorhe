package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.parallel.PushToPull;
import com.saxonica.ee.parallel.Pusher;
import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.watch.StreamWatch;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.PushEvaluator;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.sort.MergeInstr;
import net.sf.saxon.functions.DocumentFn;
import net.sf.saxon.functions.SnapshotFn;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingIncident;

import java.util.ArrayList;
import java.util.List;

/**
 * Streaming adjunct for the xsl:merge instruction. This is rather different from the usual run of streaming
 * adjuncts since it is more concerned with initiating streaming than with the actual processing
 */
public class MergeInstrAdjunct extends StreamingAdjunct {

    public static void checkStreamability(MergeInstr.MergeSource mergeSource, Configuration config) throws XPathException {
        String streamabilityOption = config.getConfigurationProperty(Feature.STREAMABILITY);
        boolean actuallyStreamable = true;
        List<String> reasons = new ArrayList<>();
        if (streamabilityOption.equals("off")) {
            reasons.add("Streaming is disabled at the Saxon Configuration level");
            actuallyStreamable = false;
        } else {
            ContextItemStaticInfoEE info = new ContextItemStaticInfoEE(NodeKindTest.DOCUMENT, false, Posture.STRIDING);
            Expression rowSelect = mergeSource.getRowSelect();
            Expression snapshot;
            if (rowSelect.isCallOn(SnapshotFn.class)) {
                // Assume that we created the call on snapshot...
                snapshot = rowSelect;
                rowSelect = ((SystemFunctionCall)rowSelect).getArg(0);
            } else {
                snapshot = SystemFunction.makeCall("snapshot",
                                                          rowSelect.getRetainedStaticContext(),
                                                          mergeSource.getRowSelect());
                mergeSource.setRowSelect(snapshot);
            }
            PostureAndSweep ps = Streamability.getStreamability(rowSelect, info, reasons);
            if (ps.getPosture() != Posture.STRIDING) {
                actuallyStreamable = false;
                reasons.add("The select expression for the merge source is not striding");
            } else {
                Streamability.getStreamability(snapshot, info, reasons);
                mergeSource.invertedAction = Inversion.invertExpression(snapshot, false);
            }
        }
        if (!actuallyStreamable) {
            boolean fallback = config.getBooleanProperty(Feature.STREAMING_FALLBACK);
            if (fallback) {
                StringBuilder buffer = new StringBuilder(120);
                buffer.append("Executing without streaming. ");
                for (String s : reasons) {
                    buffer.append(s + ". ");
                }
                config.makeErrorReporter().report(new XmlProcessingIncident(buffer.toString()).asWarning());
                mergeSource.streamable = false;
            } else {
                StringBuilder buffer = new StringBuilder(120);
                buffer.append("Streaming requested for xsl:merge, but the construct is not streamable. ");
                for (String s : reasons) {
                    buffer.append(s + ". ");
                }
                XPathException err = new XPathException(buffer.toString(), "XTSE3430");
                err.setIsStaticError(true);
                throw err;
            }
        }
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        // Check that xsl:merge is allowed as an instruction within a streamable template. See bug 4275
        MergeInstr instr = (MergeInstr) getExpression();
        for (MergeInstr.MergeSource ms : instr.getMergeSources()) {
            if (ms.getForEachItem() != null && !Streamability.getStreamability(ms.getForEachItem(), contextInfo, reasons).equals(PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                reasons.add("The for-each-item expression, if present, must be grounded and motionless");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (ms.getForEachSource() != null && !Streamability.getStreamability(ms.getForEachSource(), contextInfo, reasons).equals(PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                reasons.add("The for-each-source expression, if present, must be grounded and motionless");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (ms.getForEachItem() == null && ms.getForEachSource() == null && !Streamability.getStreamability(ms.getRowSelect(), contextInfo, reasons).equals(PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                reasons.add("If neither of the attributes for-each-item or for-each-source is present, the select expression must be grounded and motionless");
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        return PostureAndSweep.GROUNDED_AND_MOTIONLESS; // Conventional response to say that it's streamable
    }

    /**
     * Given a streamable xsl:merge-source, return a set of iterators, one for each of the input
     * sequences. Each iterator returns a sequence of "ItemWithMergeKeys" objects, representing
     * an item in the streamed input sequence together with the values of its merge keys
     * @param ms The merge source
     * @param pd The containing package
     * @param c1 The dynamic XPath Context
     * @return a set of iterators, one for each of the sources selected by the xsl:merge-source
     * element, containing the items delivered by this source together with their merge keys
     * @throws XPathException if anything goes wrong
     */

    public static List<ContextMappingIterator> makeSourceIterators(
            final MergeInstr.MergeSource ms, final PackageData pd, XPathContext c1) throws XPathException {
        List<ContextMappingIterator> result = new ArrayList<>();
        SequenceIterator uriIter = ms.getForEachSource().iterate(c1);
        Item uri;
        while ((uri = uriIter.next()) != null) {
            final ParseOptions options = new ParseOptions(c1.getConfiguration().getParseOptions());
            options.setSchemaValidationMode(ms.validation);
            options.setTopLevelType(ms.schemaType);
            options.setApplicableAccumulators(ms.accumulators);

            final String href = uri.getStringValue();
            MergePusher pusher = new MergePusher(ms, pd, href, options);

            SequenceIterator iter = new PushToPull(pusher, c1).getIterator();
            XPathContext c2 = c1.newMinorContext();
            c2.trackFocus(iter);
            MergeInstr.MergeKeyMappingFunction addMergeKeys = new MergeInstr.MergeKeyMappingFunction(c2, ms);
            ContextMappingIterator iter3 =
                    new ContextMappingIterator(addMergeKeys::map, c2);
            result.add(iter3);
        }
        return result;
    }

    public static class MergePusher implements PushEvaluator {
        private final MergeInstr.MergeSource ms;
        private final PackageData pd;
        private final String href;
        private final ParseOptions options;

        public MergePusher(MergeInstr.MergeSource ms, PackageData pd, String href, ParseOptions options) {
            this.ms = ms;
            this.pd = pd;
            this.href = href;
            this.options = options;
        }

        public TailCall processLeavingTail (Outputter output, XPathContext context) throws XPathException {
            Controller controller = context.getController();
            XPathContextMajor c2 = context.newContext();
            c2.setCurrentTemplateRule(null);
            PipelineConfiguration pipe = controller.makePipelineConfiguration();
            pipe.setXPathContext(context);

            WatchManager wm = new WatchManager(pipe);
            wm.setXPathContext(context);
            wm.setOutputter(output);

            AccumulatorRegistryEE am = (AccumulatorRegistryEE) pd.getAccumulatorRegistry();

            if (am != null && ms.accumulators != null) {
                AccumulatorRegistryEE.registerSelectedAccumulators(wm, ms.accumulators);
            }

            Receiver out = wm; //new ContentDetector(stb);

            if (pd instanceof StylesheetPackage && ((StylesheetPackage) pd).isStripsTypeAnnotations()) {
                out = pd.getConfiguration().getAnnotationStripper(out);
            }

            wm.addWatch(new StreamWatch((Inversion) ms.invertedAction, wm, c2), true);

            DocumentFn.sendDoc(href, ms.baseURI, pd, context, ms.location, out, options);
            return null;
        }

    }

}

// Copyright (c) 2018-2022 Saxonica Limited