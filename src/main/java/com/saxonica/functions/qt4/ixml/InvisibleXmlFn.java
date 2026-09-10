package com.saxonica.functions.qt4.ixml;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceCopier;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.AbstractFunction;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.serialize.UnicodeWriterResult;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;
import org.nineml.coffeefilter.InvisibleXml;
import org.nineml.coffeefilter.InvisibleXmlDocument;
import org.nineml.coffeefilter.InvisibleXmlParser;
import org.nineml.coffeegrinder.trees.Arborist;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;

import javax.xml.transform.sax.SAXSource;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.util.Map;
import java.util.Properties;

public class InvisibleXmlFn extends SystemFunction {

    public final static SpecificFunctionType RETURN_TYPE = new SpecificFunctionType(
            new SequenceType[]{SequenceType.SINGLE_STRING}, NodeKindTest.DOCUMENT.one());


    public static OptionsParameter getOptionDetails() {
        OptionsParameter details = new OptionsParameter(40);
        details.addAllowedOption("fail-on-error", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        details.addAllowedOption(sx("choose-alternative"), SequenceType.SINGLE_FUNCTION, null);
        return details;
    }

    private static String sx(String s) {
        return "Q{" + NamespaceConstant.SAXON + "}" + s;
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Object grammar = arguments[0].head();

        MapItem options = null;
        if (arguments.length > 1) {
            options = (MapItem)arguments[1].head();
        }
        if (options == null) {
            options = new DictionaryMap();
        }
        Map<String, GroundedValue> checkedOptions =
                getDetails().optionDetails.processSuppliedOptions(options, context);

        InvisibleXml invisibleXml = new InvisibleXml();
        final InvisibleXmlParser parser;
        if (grammar == null) {
            parser = invisibleXml.getParser();
        } else {
            if (grammar instanceof NodeInfo) {
                // This is a terrible hack. The NineML parser API should (will!) be updated to make this
                // easier. But for the time being, needs must.
                NodeInfo nodeInfo = (NodeInfo)grammar;
                UnicodeBuilder builder = new UnicodeBuilder();
                UnicodeWriterResult result = new UnicodeWriterResult(builder, null);
                SerializationProperties params = new SerializationProperties(new Properties());

                SerializerFactory sf = context.getConfiguration().getSerializerFactory();
                PipelineConfiguration pipe = context.getConfiguration().makePipelineConfiguration();
                PackageData pack = getRetainedStaticContext().getPackageData();
                pipe.setHostLanguage(pack.getHostLanguage(), pack.getHostLanguageVersion());
                Receiver out = sf.getReceiver(result, params, pipe);
                SequenceCopier.copySequence(nodeInfo.iterate(), out);

                ByteArrayInputStream bais = new ByteArrayInputStream(builder.toString().getBytes(StandardCharsets.UTF_8));
                try {
                    parser = invisibleXml.getParserFromVxml(bais, nodeInfo.getSystemId());
                } catch (IOException ex) {
                    throw new XPathException(ex);
                }
            } else {
                parser = invisibleXml.getParserFromIxml(((AtomicValue) grammar).getStringValue());
            }
        }

        return functionFromParser(context, parser, checkedOptions);
    }

    private Sequence functionFromParser(XPathContext context,
                                        InvisibleXmlParser parser,
                                        Map<String, GroundedValue> options) throws XPathException {
        if (parser.constructed()) {

            AbstractFunction chooseAlternative = null;
            if (options.containsKey(sx("choose-alternative"))) {
                chooseAlternative = (AbstractFunction) options.get(sx("choose-alternative"));
            }

            if (chooseAlternative != null) {
                // T.B.D. This is an extension that lets the user supply a function to resolve ambiguity.
                FunctionItemType ctype = chooseAlternative.getFunctionItemType();
                if (ctype.getResultType().getPrimaryType() != MapType.ANY_MAP_TYPE) {
                    throw new XPathException("Invalid choose-alternative function item type", "FOXX0001");
                }
                if (ctype.getArgumentTypes().length != 2) {
                    throw new XPathException("Invalid choose-alternative arity", "FOXX0001");
                }

                // The primary type for function arguments under Saxon 11 (for a curried function, anyway) appears to be item()*
                // instead of the more specific type in the underlying declaration (element(), for example).
                //
                // As a workaround, if the specific type check fails, accept item()*. This type check is really to help developers,
                // by failing early if the declared type of the function is wrong. If you declare it as item()*, it'll pass this test,
                // but it'll fail at runtime, so I'm not going to fuss too much about checking for Saxon 11 or what have you.

                if (ctype.getArgumentTypes()[0].getPrimaryType() != NodeKindTest.ELEMENT) {
                    if (!(ctype.getArgumentTypes()[0].getPrimaryType() instanceof net.sf.saxon.type.AnyItemType)) {
                        throw new XPathException("Invalid choose-alternative function, first argument must be an element", "FOXX0001");
                    }
                }
                if (ctype.getArgumentTypes()[1].getPrimaryType() != MapType.ANY_MAP_TYPE) {
                    if (!(ctype.getArgumentTypes()[0].getPrimaryType() instanceof net.sf.saxon.type.AnyItemType)) {
                        throw new XPathException("Invalid choose-alternative function, second argument must be a map", "FOXX0001");
                    }
                }
            }

            return new CallableFunction(1,
                                        new InvisibleXmlParserFunction(parser, chooseAlternative, options),
                                        RETURN_TYPE);
        } else {
            InvisibleXmlDocument failed = parser.getFailedParse();
            if (failed.getResult().succeeded()) {
                // we never even tried to parse apparently
                throw new XPathException("Failed to parse grammar", "FOIX0001");
            } else {
                if (failed.getLineNumber() > 0) {
                    String loc = "line " + failed.getLineNumber();
                    if (failed.getColumnNumber() > 0) {
                        loc += ", column " + failed.getColumnNumber();
                    }
                    throw new XPathException("Failed to parse grammar, error at (approximately) " + loc, "FOIX0001");
                }
                throw new XPathException("Failed to parse grammar", "FOIX0001");
            }
        }
    }

    private static class InvisibleXmlParserFunction implements Callable {
        private final InvisibleXmlParser parser;
        private final AbstractFunction chooseAlternative;
        private final Map<String,GroundedValue> options;
        private final QName _json = new QName("", "json");

        public InvisibleXmlParserFunction(InvisibleXmlParser parser, AbstractFunction chooseAlternative, Map<String, GroundedValue> options) {
            this.parser = parser;
            this.chooseAlternative = chooseAlternative;
            this.options = options;
        }

        @Override
        public Sequence call(XPathContext context, Sequence[] sequences) throws XPathException {
            String input = sequences[0].head().getStringValue();

            try {
                Processor processor = (Processor) context.getConfiguration().getProcessor();
                InvisibleXmlDocument document = parser.parse(input);

                if (!document.succeeded()) {
                    boolean failOnError = options.get("fail-on-error").effectiveBooleanValue();
                    if (failOnError) {
                        if (document.getLineNumber() > 0) {
                            String loc = "line " + document.getLineNumber();
                            if (document.getColumnNumber() > 0) {
                                loc += ", column " + document.getColumnNumber();
                            }
                            throw new XPathException("Failed to parse input, error at (approximately) " + loc, "FOIX0002");
                        }
                        throw new XPathException("Failed to parse input", "FOIX0002");
                    }
                    ByteArrayInputStream bais = new ByteArrayInputStream(document.getTree().getBytes(StandardCharsets.UTF_8));
                    SAXSource source = new SAXSource(new InputSource(bais));
                    processor = (Processor) context.getConfiguration().getProcessor();
                    DocumentBuilder builder = processor.newDocumentBuilder();
                    XdmNode errdoc = builder.build(source);
                    return errdoc.getUnderlyingNode();
                }

                final XmlForest forest;
                try {
                    forest = new XmlForest(processor, document);
                } catch (SaxonApiException | SAXException ex) {
                    throw new XPathException(ex);
                }

                XPathAxe axe = new XPathAxe(processor, parser, forest, input);
                axe.setChooseFunction(context, chooseAlternative);

                Arborist arborist = document.getResult().getArborist(axe);

                DocumentBuilder docBuilder = processor.newDocumentBuilder();
                BuildingContentHandler handler = docBuilder.newBuildingContentHandler();
                arborist.getTree(document.getAdapter(handler));
                return handler.getDocumentNode().getUnderlyingNode();
            } catch (XPathException ex) {
                throw ex;
            } catch (Exception ex) {
                throw new XPathException(ex);
            }
        }
    }
}
