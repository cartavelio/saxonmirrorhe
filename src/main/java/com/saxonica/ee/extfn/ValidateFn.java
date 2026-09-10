package com.saxonica.ee.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.Invalidity;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.StandardInvalidityHandler;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;


/**
 * This class implements the saxon:validate() extension function.
 * <p>There are three arguments:
 * (a) the node to be validated (element, document, or attribute)
 * (b) (optional) a map containing validation options
 * (c) (optional) a map containing validation parameters (values for saxon:param)</p>
 * <p>The result of the function is a map containing the possible entries:
 * (a) "valid: a boolean indicating whether validation succeeded
 * (b) "node": the validated node, with type annotations (a copy of the original)
 * (c) "errors": a list of error objects, each of which is a map with various
 * entries holding details of the error</p>
 */

public class ValidateFn extends SystemFunction {


    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
     *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
     *                  If the argument is always a singleton, then the single item may be obtained by calling
     *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
     *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
     *                  to completion, it is good practice to call its close() method.
     * @return the results of the function.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    /*@Nullable*/
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        final List<Invalidity> errorList = new ArrayList<>(5);
        boolean returnTreeOnFailure = false;
        NodeInfo resultNode = null;
        Configuration config = context.getConfiguration();
        //try {
        NodeInfo input = (NodeInfo) arguments[0].head();
        if (input == null) {
            return EmptySequence.getInstance();
        }

        SchemaType schemaType = null;
        int validation = Validation.STRICT;
        boolean expand = true;
        boolean buildTree = true;

        if (arguments.length >= 2) {
            MapItem options = (MapItem) arguments[1].head();
            Sequence val;

            // buildTree = true() | false() | ()
            val = options.get(StringValue.bmp("buildTree"));
            if (val != null && val.head() != null) {
                if (val instanceof BooleanValue) {
                    buildTree = ((BooleanValue) val).getBooleanValue();
                    returnTreeOnFailure = buildTree;
                } else {
                    throw new XPathException("buildTree must be true() or false() or ()");
                }
            }

            // type = QName
            val = options.get(StringValue.bmp("type"));
            if (val != null) {
                if (val instanceof QNameValue) {
                    schemaType = config.getSchemaType(((QNameValue) val).getStructuredQName());
                    if (schemaType == null) {
                        throw new XPathException("Unknown type " + val, "XPTY0004");
                    }
                } else {
                    throw new XPathException("'type' option in second argument of saxon:validate() must be a QName", "XPTY0004");
                }
            }

            // processContents = strict | lax
            val = options.get(StringValue.bmp("processContents"));
            if (val != null) {
                if (val instanceof StringValue) {
                    String s = ((StringValue) val).getStringValue();
                    switch (s) {
                        case "strict":
                            // no action
                            break;
                        case "lax":
                            validation = Validation.LAX;
                            break;
                        default:
                            throw new XPathException("processContents must be 'strict' or 'lax'");
                    }
                } else {
                    throw new XPathException("'processContents' option in second argument of saxon:validate() must be a string", "XPTY0004");
                }
            }

            // expandDefaults = true() | false()
            val = options.get(StringValue.bmp("expandDefaults"));
            if (val != null) {
                if (val instanceof BooleanValue) {
                    expand = ((BooleanValue) val).getBooleanValue();
                } else {
                    throw new XPathException("processContents must be true() or false()");
                }
            }


        } else {
            // no action
        }

        ValidationParams params = new ValidationParams();
        if (arguments.length >= 3) {
            MapItem rawParams = (MapItem) arguments[2].head();
            for (KeyValuePair pair : rawParams.keyValuePairs()) {
                AtomicValue key = pair.key;
                QNameValue qKey;
                if (key instanceof QNameValue) {
                    qKey = (QNameValue) key;
                } else if (key instanceof StringValue) {
                    qKey = new QNameValue("", NamespaceUri.NULL, key.getStringValue());
                } else {
                    throw new XPathException("key value in third argument to saxon:validate must be a string or QName");
                }
                params.put(qKey.getStructuredQName(), pair.value);
            }
        } else {
            params = new ValidationParams();
        }

        ParseOptions parseOptions = new ParseOptions()
                .withValidationParams(params)
                .withSchemaValidationMode(validation)
                .withTopLevelType(schemaType)
                .withExpandAttributeDefaults(expand)
                .withContinueAfterValidationErrors(true);


        final List<NodeInfo> currentErrorNode = new ArrayList<>(1);

        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
        pipe.setParseOptions(parseOptions);
        pipe.setXPathContext(context);
        //pipe.getParseOptions().setContinueAfterValidationErrors(true);

        pipe.setCopyInformee(element -> {
            currentErrorNode.clear();
            currentErrorNode.add(element);
            return null;
        });
        parseOptions = parseOptions.withInvalidityHandler(
                new LocalInvalidityHandler(config, currentErrorNode, errorList));


        int kind = input.getNodeKind();
        switch (kind) {

            case Type.DOCUMENT: {
                Receiver out = buildTree ? new SequenceCollector(pipe) : new Sink(pipe);
                Receiver val = config.getDocumentValidator(out, input.getBaseURI(), parseOptions, null);
                val.setPipelineConfiguration(out.getPipelineConfiguration());
                val.setSystemId(input.getBaseURI());
                input.copy(val, CopyOptions.ALL_NAMESPACES, Loc.NONE);
                resultNode = buildTree ? (NodeInfo) ((SequenceCollector) out).getFirstItem() : null;
                break;
            }
            case Type.ELEMENT: {
                SequenceReceiver out = buildTree ? new SequenceCollector(pipe) : new Sink(pipe);
                parseOptions = parseOptions.withTopLevelElement(NameOfNode.makeName(input).getStructuredQName());
                Receiver val = config.getElementValidator(out, parseOptions, Loc.NONE);
                val.setPipelineConfiguration(out.getPipelineConfiguration());
                val.setSystemId(input.getBaseURI());
                input.copy(val, CopyOptions.ALL_NAMESPACES, Loc.NONE);
                resultNode = buildTree ? (NodeInfo) ((SequenceCollector) out).getFirstItem() : null;
                break;
            }
            case Type.ATTRIBUTE:
                SimpleType simpleType = BuiltInAtomicType.UNTYPED_ATOMIC;
                if (schemaType instanceof SimpleType) {
                    // test whether the value actually conforms to the given type
                    simpleType = (SimpleType) schemaType;
                    ValidationFailure err = simpleType.validateContent(
                            input.getUnicodeStringValue(), DummyNamespaceResolver.getInstance(), context.getConfiguration().getConversionRules());
                    if (err != null) {
                        ValidationFailure ve = new ValidationFailure(
                                "Attribute value " + Err.wrap(input.getUnicodeStringValue(), Err.VALUE) +
                                        " does not match the required type " +
                                        schemaType.getDescription() + ". " +
                                        err.getMessage());
                        ve.setErrorCode("XTTE1540");
                        ve.setSchemaType(schemaType);
                        ve.setPath(AbsolutePath.pathToNode(input));
                        errorList.add(ve);
                    }
                } else if (validation == Validation.STRICT ||
                        validation == Validation.LAX) {
                    try {
                        simpleType = config.validateAttribute(
                                NameOfNode.makeName(input).getStructuredQName(), input.getUnicodeStringValue(), validation);

                    } catch (ValidationException e) {
                        ValidationFailure vf = e.getValidationFailure();
                        vf.setInvalidNode(input);
                        errorList.add(vf);
                    }
                } else {
                    resultNode = input;
                    break;
                }
                if (buildTree) {
                    Orphan orphan = new Orphan(config);
                    orphan.setNodeKind(Type.ATTRIBUTE);
                    orphan.setNodeName(NameOfNode.makeName(input));
                    orphan.setStringValue(input.getUnicodeStringValue());
                    orphan.setTypeAnnotation(simpleType);
                    resultNode = orphan;
                } else {
                    resultNode = null;
                }
                break;

            default:
                throw new XPathException("Input to saxon:validate must be a document, element, or attribute node", "XPTY0004");
        }

        DictionaryMap result = new DictionaryMap();
        if (errorList.isEmpty()) {
            if (buildTree) {
                result.initialPut("node", resultNode);
            }
            result.initialPut("valid", BooleanValue.TRUE);
            return result;

        } else {

            result.initialPut("valid", BooleanValue.FALSE);
            if (returnTreeOnFailure) {
                result.initialPut("node", resultNode);
            }
            List<Item> errors = new ArrayList<>(errorList.size());
            for (Invalidity te : errorList) {

                ValidationFailure v = (ValidationFailure) te;
                DictionaryMap im = new DictionaryMap();
                if (v.getErrorCodeQName() != null) {
                    im.initialPut("code", new QNameValue(v.getErrorCodeQName(), BuiltInAtomicType.QNAME));
                }
                im.initialPut("message", new StringValue(v.getMessage()));

                if (v.getSchemaPart() != -1) {
                    im.initialPut("specPart", new Int64Value(v.getSchemaPart()));
                }
                if (v.getConstraintName() != null) {
                    im.initialPut("constraint", new StringValue(v.getConstraintName()));
                }
                if (v.getConstraintClauseNumber() != null) {
                    im.initialPut("clause", new StringValue(v.getConstraintClauseNumber()));
                }
                if (v.getLineNumber() != -1) {
                    im.initialPut("line", new Int64Value(v.getLineNumber()));
                }
                if (v.getColumnNumber() != -1) {
                    im.initialPut("colummn", new Int64Value(v.getColumnNumber()));
                }
                if (v.getSystemId() != null) {
                    im.initialPut("systemId", new StringValue(v.getSystemId()));
                }
                if (v.getPublicId() != null) {
                    im.initialPut("publicId", new StringValue(v.getPublicId()));
                }
                if (v.getInvalidNode() != null) {
                    im.initialPut("node", v.getInvalidNode());
                }
                if (v.getPath() != null) {
                    im.initialPut("path", new StringValue(v.getPath().getPathUsingUris()));
                }

                errors.add(im);
            }
            result.initialPut("errors", SequenceExtent.makeSequenceExtent(errors));
            return result;


        }


    }

    private static class LocalInvalidityHandler extends StandardInvalidityHandler {

        List<NodeInfo> currentErrorNode;
        List<Invalidity> errorList;

        /**
         * Create the Invalidity Handler
         *
         * @param config the Saxon configuration
         */
        public LocalInvalidityHandler(Configuration config, List<NodeInfo> currentErrorNode, List<Invalidity> errorList) {
            super(config);
            this.errorList = errorList;
            this.currentErrorNode = currentErrorNode;
        }

        /**
         * At the start of a validation episode, initialize the handler
         *
         * @param systemId optional; may be used to represent the destination of any report produced
         */
        @Override
        public void startReporting(String systemId) {
            // no action
        }

        /**
         * Report a validation error found during validation of an instance document
         * against a schema
         *
         * @param failure details of the validation error
         */
        @Override
        public void reportInvalidity(Invalidity failure) {
            if (!currentErrorNode.isEmpty()) {
                ((ValidationFailure) failure).setSourceLocator(currentErrorNode.get(0));
                ((ValidationFailure) failure).setPath(AbsolutePath.pathToNode(currentErrorNode.get(0)));
                ((ValidationFailure) failure).setInvalidNode(currentErrorNode.get(0));
            }
            errorList.add(failure);
        }

        /**
         * Get the value to be associated with a validation exception. May return null.
         * In the case of the InvalidityReportGenerator, this returns the XML document
         * containing the validation report
         *
         * @return a value (or null). This will be the value returned as the value of
         * the variable $err:value during try/catch processing
         */
        @Override
        public Sequence endReporting() {
            return null;
        }

    }


}


// Copyright (c) 2009-2022 Saxonica Limited
