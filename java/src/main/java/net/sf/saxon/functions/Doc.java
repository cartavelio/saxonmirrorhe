////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Whitespace;
import org.xml.sax.EntityResolver;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLReader;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParserFactory;
import java.io.IOException;
import java.util.Map;

/**
 * Implement the fn:doc() function - a simplified form of the Document function
 */

public class Doc extends SystemFunction implements Callable {

    private ParseOptions parseOptions;

    /**
     * Get the parsing options set via setParseOptions()
     *
     * @return the parsing options
     */

    public ParseOptions getParseOptions() {
        return parseOptions;
    }

    /**
     * Set options to be used for the parsing operation. Defaults to the parsing options set in the Configuration
     *
     * @param parseOptions the parsing options to be used. Currently only affects the behaviour of the sendDocument()
     *                     method (used in streamed merging)
     */

    public void setParseOptions(ParseOptions parseOptions) {
        this.parseOptions = parseOptions;
    }

    @Override
    public int getCardinality(Expression[] arguments) {
        return arguments[0].getCardinality() & ~StaticProperty.ALLOWS_MANY;
    }

    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        Expression expr = maybePreEvaluate(this, arguments);
        return expr == null ? super.makeFunctionCall(arguments) : expr;
    }

    @CSharpInnerClass(outer=false, extra={"Saxon.Hej.functions.SystemFunction sf"})
    public static Expression maybePreEvaluate(final SystemFunction sf, final Expression[] arguments) {
        if (arguments.length > 1 ||
                !sf.getRetainedStaticContext().getConfiguration().getBooleanProperty(Feature.PRE_EVALUATE_DOC_FUNCTION)) {
            sf.getDetails().properties = sf.getDetails().properties | BuiltInFunctionSet.LATE;
            return null;

        } else {
            // allow early evaluation
            return new SystemFunctionCall(sf, arguments) {
                @Override
                public Expression preEvaluate(ExpressionVisitor visitor) {
                    Configuration config = visitor.getConfiguration();
                    try {
                        GroundedValue firstArg = ((Literal) this.getArg(0)).getGroundedValue();
                        if (firstArg.getLength() == 0) {
                            return null;
                        } else if (firstArg.getLength() > 1) {
                            return this;
                        }
                        String href = firstArg.head().getStringValue();
                        if (href.indexOf('#') >= 0) {
                            return this;
                        }
                        NodeInfo item = DocumentFn.preLoadDoc(href, sf.getStaticBaseUriString(),
                                                              sf.getRetainedStaticContext().getPackageData(),
                                                              config, getLocation());
                        if (item != null) {
                            Expression constant = Literal.makeLiteral(item);
                            ExpressionTool.copyLocationInfo(this.getArg(0), constant);
                            return constant;
                        }
                    } catch (Exception err) {
                        // ignore the exception and try again at run-time
                        return this;
                    }
                    return this;
                }

                @Override
                public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
                    optimizeChildren(visitor, contextItemType);
                    if (getArg(0) instanceof StringLiteral) {
                        return preEvaluate(visitor);
                    }
                    return this;
                }

            };

        }

    }



    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        AtomicValue hrefVal = (AtomicValue) arguments[0].head();
        if (hrefVal == null) {
            return EmptySequence.getInstance();
        }
        String href = hrefVal.getStringValue();
        PackageData packageData = getRetainedStaticContext().getPackageData();

        if (getArity() >= 2) {
            // 4.0 spec provides an options parameter. For this case we create a clean
            // XML parser rather than using one from the pool. This is (a) to avoid the problems
            // of resetting it to a virgin state before returning it to the pool, and (b)
            // to ensure we get the default JDK parser implementation, rather than picking a parser
            // off the classpath.
            MapItem options = (MapItem) arguments[1].head();
            if (options != null && !options.isEmpty()) {
                Map<String, GroundedValue> checkedOptions = getDetails().optionDetails.processSuppliedOptions(options, context);

                // TODO: (a) reuse the factory, (b) avoid Java 13 dependency
                SAXParserFactory factory = SAXParserFactory.newDefaultNSInstance();
                XMLReader parser;
                try {
                    parser = factory.newSAXParser().getXMLReader();
                } catch (SAXException | ParserConfigurationException e) {
                    throw new RuntimeException(e);
                }
                parseOptions = new ParseOptions();
                parseOptions = parseOptions.withXMLReader(parser);

                GroundedValue val = checkedOptions.get("dtd-validation");
                boolean dtdValidating = val != null && val.effectiveBooleanValue();
                if (dtdValidating) {
                    try {
                        parser.setFeature("http://xml.org/sax/features/validation", true);
                    } catch (SAXException e) {
                        throw new RuntimeException(e);
                    }
                }

                val = checkedOptions.get("allow-external-entities");
                if (val != null && !val.effectiveBooleanValue()) {
                    parser.setEntityResolver(new EntityResolver() {

                        @Override
                        public InputSource resolveEntity(String publicId, String systemId) throws SAXException, IOException {
                            throw new SAXException(
                                    new XPathException("The parse-xml input references '" + systemId +
                                                               "' but external entity expansion is disallowed", "FODC0006"));
                        }
                    });
                }

                val = checkedOptions.get("entity-expansion-limit").head();
                if (val != null) {
                    try {
                        parser.setProperty("http://www.oracle.com/xml/jaxp/properties/entityExpansionLimit", Integer.parseInt(val.getStringValue()));
                    } catch (SAXException e) {
                        throw new RuntimeException(e);
                    }
                }

                val = checkedOptions.get("xsi-schema-location");
                if (val != null) {
                    parseOptions = parseOptions.withUseXsiSchemaLocation(val.effectiveBooleanValue());
                }


                val = checkedOptions.get("xsd-validation");
                boolean xsdValidating = false;
                if (val == null) {
                    // In 3.1 we defaulted from the configuration values, in 4.0 it has to be explicit
                    parseOptions = parseOptions.withSchemaValidationMode(Validation.SKIP);
                } else {
                    String validationValue = Whitespace.normalize(val.getStringValue());
                    xsdValidating = !"skip".equals(validationValue);
                    if (validationValue.equals("strict")) {
                        parseOptions = parseOptions.withSchemaValidationMode(Validation.STRICT);
                        parseOptions = parseOptions.withSchema(getRetainedStaticContext().getImportedSchema());
                    } else if (validationValue.equals("lax")) {
                        parseOptions = parseOptions.withSchemaValidationMode(Validation.LAX);
                        parseOptions = parseOptions.withSchema(getRetainedStaticContext().getImportedSchema());
                    } else if (validationValue.equals("skip")) {
                        parseOptions = parseOptions.withSchemaValidationMode(Validation.SKIP);
                    } else if (validationValue.startsWith("type ")) {
                        String eqName = validationValue.substring(5).trim();
                        if (!eqName.startsWith("Q{")) {
                            throw new XPathException("Type name in xsd-validation option of fn:parse-xml must start with 'Q{'", "FODC0008");
                        }
                        StructuredQName typeName;
                        try {
                            typeName = StructuredQName.fromLexicalQName(eqName, false, true, null);
                        } catch (XPathException e) {
                            throw e.withErrorCode("FODC0008");
                        }
                        Schema schema = getRetainedStaticContext().getImportedSchema();
                        SchemaType type = schema.getSchemaType(typeName);
                        if (type == null) {
                            throw new XPathException("Unknown type " + eqName, "FODC0008");
                        }
                        parseOptions = parseOptions
                                .withSchema(schema)
                                .withSchemaValidationMode(Validation.BY_TYPE)
                                .withTopLevelType(type);
                    } else {
                        throw new XPathException("Unknown xsd-validation option of fn:parse-xml: " + validationValue, "FODC0008");
                    }


                    val = checkedOptions.get("strip-space");
                    if (val != null) {
                        SpaceStrippingRule rule;
                        if (xsdValidating || dtdValidating) {
                            rule = IgnorableSpaceStrippingRule.getInstance();
                        } else if (val.effectiveBooleanValue()) {
                            rule = AllElementsSpaceStrippingRule.getInstance();
                        } else {
                            rule = NoElementsSpaceStrippingRule.getInstance();
                        }
                        parseOptions = parseOptions.withSpaceStrippingRule(rule);
                    }

                    val = checkedOptions.get("xinclude");
                    if (val != null && val.effectiveBooleanValue()) {

                        parseOptions = parseOptions.withXIncludeAware(true);
                        //throw new XPathException("XIncude processing in fn:parse-xml is not supported", "FODC0013");
                    }


                }
            }
        } else {
            ParseOptions parseOptions = context.getConfiguration().getParseOptions();

            PackageData pd = getRetainedStaticContext().getPackageData();
            if (pd instanceof StylesheetPackage) {
                parseOptions = parseOptions.withSpaceStrippingRule(((StylesheetPackage) pd).getSpaceStrippingRule());
            } else {
                parseOptions = parseOptions.withSpaceStrippingRule(IgnorableSpaceStrippingRule.getInstance());
            }


        }


        NodeInfo item = DocumentFn.makeDoc(href, getRetainedStaticContext().getStaticBaseUriString(), packageData, parseOptions, context, null, false);
        if (item == null) {
            // we failed to read the document
            throw new XPathException("Failed to load document " + href, "FODC0002", context);
        }
        Controller controller = context.getController();
        if (parseOptions != null && controller instanceof XsltController) {
            ((XsltController) controller).getAccumulatorManager().setApplicableAccumulators(
                    item.getTreeInfo(), parseOptions.getApplicableAccumulators()
            );
        }
        return item;
    }

    /**
     * Get the static properties of this expression (other than its type). The result is
     * bit-signficant. These properties are used for optimizations. In general, if
     * property bit is set, it is true, but if it is unset, the value is unknown.
     *
     * @param arguments the expressions supplied as arguments to the function
     */

    @Override
    public int getSpecialProperties(Expression[] arguments) {
        return StaticProperty.ORDERED_NODESET |
                StaticProperty.PEER_NODESET |
                StaticProperty.NO_NODES_NEWLY_CREATED |
                StaticProperty.SINGLE_DOCUMENT_NODESET;
        // Declaring it as a peer node-set expression avoids sorting of expressions such as
        // doc(XXX)/a/b/c
        // The doc() function might appear to be creative: but it isn't, because multiple calls
        // with the same arguments will produce identical results.
    }


}

