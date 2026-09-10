////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.trace;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.event.PushToReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.instruct.TemplateRule;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.push.Document;
import net.sf.saxon.s9api.push.Element;
import net.sf.saxon.s9api.push.Push;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.RuleTarget;

import java.net.URL;
import java.util.IdentityHashMap;
import java.util.Map;

/**
 * A trace listener that records coverage data (actually execution counts) for instructions in
 * an XSLT stylesheet, outputting the histogram at the end as XML.
 */

public class CoverageTraceListener implements TraceListener {

    private URL stylesheet = null;
    private PreparedStylesheet reportingStylesheet;
    // The transformStack allows for the fact that multiple transformations can be initiated using fn:transform(),
    // and all are reported to the same TimingTraceListener
    //private final Stack<TimingRun> transformStack = new Stack<>();
    private Configuration config;

    private IdentityHashMap<Traceable, Integer> histogram;



    private Logger out = new StandardLogger();

    /**
     * Set the PrintStream to which the output will be written.
     *
     * @param stream the PrintStream to be used for output. By default, the output is written
     *               to System.err.
     */

    @Override
    public void setOutputDestination(Logger stream) {
        out = stream;
    }

    /**
     * Set the URI of the stylesheet to be used for formatting the results
     * @param stylesheet the URI of the stylesheet
     */

    public void setStylesheet(URL stylesheet) {
        this.stylesheet = stylesheet;
    }

    /**
     * Called at start
     */

    @Override
    public void open(/*@NotNull*/ Controller controller) {
        config = controller.getConfiguration();
        histogram = getAllTracePoints(controller);
//        int depth = transformStack.size();
//        transformStack.push(new TimingRun(controller, depth));
    }

    /**
     * Called at end. This method builds the XML out and analyzed html output
     */

    @Override
    public void close() {
        try {
            SerializationProperties props = new SerializationProperties();
            props.setProperty("method", "xml");
            props.setProperty("indent", "yes");

            Receiver result = config.getSerializerFactory().getReceiver(
                    out.asStreamResult(), props, config.makePipelineConfiguration());
            
            Push push = new PushToReceiver(result);
            Document doc = push.document(true);
            Element root = doc.element("coverage");
            for (Map.Entry<Traceable, Integer> entry : histogram.entrySet()) {
                Traceable traceable = entry.getKey();
                Location loc = traceable.getLocation();
                int count = entry.getValue();
                Element point = root.element(new QName("T"))
                        .attribute(new QName("type"), XSLTTraceListener.tagName(traceable))
                        .attribute(new QName("module"), loc.getSystemId())
                        .attribute(new QName("line"), ""+loc.getLineNumber())
                        .attribute(new QName("column"), ""+loc.getColumnNumber())
                        .attribute(new QName("count"), ""+count);
                if (traceable.getObjectName() != null) {
                    point.attribute(new QName("name"), traceable.getObjectName().getDisplayName());
                }
                traceable.gatherProperties((key, value) -> {
                    try {
                        point.attribute(new QName(key), value.toString());
                    } catch (SaxonApiException e) {
                        // discard any failing properties
                    }
                });
            }
            doc.close();


//            if (reportingStylesheet == null) {
//                reportingStylesheet = getStyleSheet();
//            }
//            TimingRun run = transformStack.pop();
//            run.complete(reportingStylesheet, out);
        } catch (SaxonApiException | XPathException e) {
            // no action
        }

    }

    /**
     * Called when an instruction in the stylesheet gets processed
     */

    @Override
    public void enter(Traceable instruction, Map<String, Object> properties, XPathContext context) {
        Integer counter = histogram.get(instruction);
        if (counter == null) {
            System.err.println("*** Non-registered trace point " + instruction);
            counter = 0;
        }
        int incr = counter+1;
        histogram.put(instruction, incr);
    }

    /**
     * Called after an instruction of the stylesheet got processed
     * @param instruction the instruction or other construct that has now finished execution
     */

    @Override
    public void leave(/*@NotNull*/ Traceable instruction) {
        // no action
    }

    /**
     * Called when an item becomes current
     */

    @Override
    public void startCurrentItem(Item item) {
    }

    /**
     * Called after a node of the source tree got processed
     */

    @Override
    public void endCurrentItem(Item item) {
    }

//    /**
//     * Prepare Stylesheet to render the analyzed XML data out.
//     * This method can be overridden in a subclass to produce the output in a different format.
//     */
//    /*@NotNull*/
//    protected PreparedStylesheet getStyleSheet() throws XPathException {
//        InputStream in = getStylesheetInputStream();
//        StreamSource ss = new StreamSource(in, "profile.xsl");
//
//        CompilerInfo info = config.getDefaultXsltCompilerInfo();
//        PreparedStylesheet pss = Compilation.compileSingletonPackage(config, info, ss);
//        try {
//            in.close();
//        } catch (IOException err) {
//            out.warning("Preparation of profiling stylesheet failed: " + err.getMessage());
//        }
//        return pss;
//    }


//    /**
//     * Get an input stream containing the stylesheet used for formatting results.
//     * The method is protected so that a user-written stylesheet can be supplied in a subclass.
//     * @return the input stream containing the stylesheet for processing the results.
//     */
//
//    protected InputStream getStylesheetInputStream() {
//        if (stylesheet == null) {
//            return Version.platform.locateResource("profile.xsl", new ArrayList<>());
//        } else {
//            try {
//                return stylesheet.openConnection().getInputStream();
//            } catch (IOException e) {
//                System.err.println("Unable to read " + stylesheet + "; using default stylesheet for -TP output");
//                return Version.platform.locateResource("profile.xsl", new ArrayList<>());
//            }
//        }
//    }



    private static IdentityHashMap<Traceable, Integer> getAllTracePoints(Controller controller) {
        Executable exec = controller.getExecutable();
        IdentityHashMap<Traceable, Integer> histogram = new IdentityHashMap<>();
        for (PackageData pack : exec.getPackages()) {
            StylesheetPackage ssPack = ((StylesheetPackage) pack);
            for (Component component : ssPack.getComponentIndex().values()) {
                if (component.getActor() instanceof Mode) {
                    try {
                        ((Mode)component.getActor()).processRules(rule -> {
                            RuleTarget t = rule.getAction();
                            if (t instanceof TemplateRule) {
                                histogram.put((TemplateRule)t, 0);
                                Expression templateBody = ((TemplateRule)t).getBody();
                                gatherTracePoints(templateBody, histogram);
                            }
                        });
                    } catch (XPathException e) {
                        System.err.println("*** " + e.getMessage());
                        // ignore failures
                    }
                } else if (component.getActor() instanceof Traceable) {
                    histogram.put(((Traceable)component.getActor()), 0);
                    Expression body = component.getActor().getBody();
                    gatherTracePoints(body, histogram);
                }

            }
        }
        return histogram;
    }

    private static void gatherTracePoints(Expression expr, IdentityHashMap<Traceable, Integer> histogram) {
        if (expr != null) {
            if (expr instanceof TraceExpression) {
                histogram.put(((TraceExpression) expr).getBody(), 0);
            }
            for (Operand o : expr.operands()) {
                gatherTracePoints(o.getChildExpression(), histogram);
            }
        }
    }

}

