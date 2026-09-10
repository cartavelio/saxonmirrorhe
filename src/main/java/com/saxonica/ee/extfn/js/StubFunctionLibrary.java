package com.saxonica.ee.extfn.js;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.IntegratedFunctionCall;
import net.sf.saxon.functions.UnparsedTextFunction;
import net.sf.saxon.lib.ExtensionFunctionCall;
import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.lib.StandardUnparsedTextResolver;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.json.ParseJsonFn;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import javax.xml.transform.stream.StreamSource;
import java.io.Reader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * This represents a library of function signatures with no actual implementation
 * in SaxonJ. The function signatures are read from a JSON file. The compiler will
 * accept calls to these functions, and generate these calls in a SEF file,
 * but any attempt to evaluate the function in SaxonJ will fail.
 *
 * <p>A specimen file is as follows:</p>
 *
 * <pre>
 * {
 *   "namespace": "http://example.com/ns/mocha",
 *   "signatures": {
 *     "base64Binary-param": {
 *       "as": "xs:boolean",
 *       "param": [
 *         "xs:base64Binary"
 *       ],
 *       "arity": [1]
 *     },
 *     "anyAtomicType-param": {
 *       "as": "xs:boolean",
 *       "param": [
 *         "xs:anyAtomicType"
 *       ],
 *       "arity": [1]
 *     }
 *   }
 * }
 * </pre>
 * <p>The JSON file can also contain a number of such maps, one per namespace, within a containing array.</p>
 */

public class StubFunctionLibrary implements FunctionLibrary {

    private final Map<SymbolicName.F, SpecificFunctionType> signatures = new HashMap<>();
    private final Configuration config;

    /**
     * Create a stub function library
     * @param config the Saxon configuration (always EE)
     */
    public StubFunctionLibrary(Configuration config) {
        this.config = config;
    }

    /**
     * Load signatures from supplied JSON file.
     * @param input the JSON function signature definitions
     * @throws XPathException if the file is invalid
     */
    public void loadSignatures(StreamSource input) throws XPathException {

        XPathContext context = config.getConversionContext();

        UnicodeString content;
        try {
            Reader reader = StandardUnparsedTextResolver.getReaderFromStreamSource(input, "utf-8", config, config.isTiming());
            content = UnparsedTextFunction.readFile((ch) -> true, reader);
        } catch (java.io.IOException ioErr) {
            throw new XPathException("Failed to read function signatures", ioErr);
        }

        Item result = ParseJsonFn.parse(content.toString(), new HashMap<>(), context);
        if (result instanceof ArrayItem) {
            for (GroundedValue member : ((ArrayItem)result).members()) {
                if (member instanceof MapItem) {
                    loadSignatureMap((MapItem)member);
                } else {
                    badInput("Top level is an array but contents are not maps");
                }
            }
        } else if (result instanceof MapItem) {
            loadSignatureMap((MapItem) result);
        } else {
            badInput("Neither an array nor a map");
        }
    }

    private final static StringValue NAMESPACE = new StringValue("namespace");
    private final static StringValue SIGNATURES = new StringValue("signatures");
    private final static StringValue AS = new StringValue("as");
    private final static StringValue PARAM = new StringValue("param");
    private final static StringValue ARITY = new StringValue("arity");

    private void loadSignatureMap(MapItem map) throws XPathException {
        String namespace = map.get(NAMESPACE).getStringValue();
        GroundedValue sigs = map.get(SIGNATURES);
        StaticContext env = new IndependentContext(config);
        if (sigs instanceof MapItem) {
            for (KeyValuePair kvp : ((MapItem)sigs).keyValuePairs()) {
                StructuredQName name = new StructuredQName("", namespace, kvp.key.toString());
                expect(kvp.value instanceof MapItem, "Value of " + name + " must be a map");
                MapItem sigValue = ((MapItem)kvp.value);
                GroundedValue asValue = sigValue.get(AS);
                expect(asValue instanceof StringValue, "In " + name + ", as must exist and be a string");
                SequenceType resultType = parseSequenceType(asValue.getStringValue(), env);
                List<SequenceType> paramTypes = new ArrayList<>();
                GroundedValue params = sigValue.get(PARAM);
                expect(params instanceof ArrayItem, "In " + name + ", param must exist and be an array");
                for (GroundedValue paramDetails : ((ArrayItem)params).members()) {
                     expect(paramDetails instanceof StringValue, "In " + name + ", param members must be strings");
                     SequenceType paramType = parseSequenceType(paramDetails.getStringValue(), env);
                     paramTypes.add(paramType);
                }
                GroundedValue arityValue = sigValue.get(ARITY);
                expect(arityValue instanceof ArrayItem, "In " + name + ", as must exist and be an array");
                for (GroundedValue arity : ((ArrayItem)arityValue).members()) {
                    expect(arity instanceof NumericValue, "In " + name + ", arity must be a number");
                    int arityInt = ((NumericValue)arity).asSubscript();
                    SymbolicName.F functionName = new SymbolicName.F(name, arityInt);
                    expect(arityInt <= paramTypes.size(), "In " + name + " arity exceeds number of params");
                    SequenceType[] st = new SequenceType[arityInt];
                    for (int i=0; i<arityInt; i++) {
                        st[i] = paramTypes.get(i);
                    }
                    SpecificFunctionType sig = new SpecificFunctionType(st, resultType);
                    signatures.put(functionName, sig);
                }

            }
        }
    }

    private static SequenceType parseSequenceType(String designator, StaticContext env) throws XPathException {
        XPathParser p = new XPathParser(env);
        return p.parseSequenceType(designator, env);
    }

    private static void expect(boolean condition, String description) throws XPathException {
        if (!condition) {
            throw new XPathException("Invalid function signature file: " + description);
        }
    }

    private static void badInput(String message) throws XPathException {
        throw new XPathException("Invalid content in function signatures file", message);
    }

    @Override
    public boolean isAvailable(SymbolicName.F functionName, int languageLevel) {
        return signatures.containsKey(functionName);
    }

    @Override
    public Expression bind(SymbolicName.F functionName, Expression[] staticArgs, Map<StructuredQName, Integer> keywords, StaticContext env, List<String> reasons) {
        if (signatures.containsKey(functionName)) {
            SpecificFunctionType type = signatures.get(functionName);
            ExtensionFunctionDefinition defn = new StubExtensionDefinition(functionName.getComponentName(), type);
            ExtensionFunctionCall f = defn.makeCallExpression();
            f.setDefinition(defn);
            IntegratedFunctionCall fc = new IntegratedFunctionCall(defn.getFunctionQName(), f);
            fc.setArguments(staticArgs);
            return fc;
        }
        return null;
    }

    @Override
    public FunctionLibrary copy() {
        return this;
    }

    @Override
    public FunctionItem getFunctionItem(SymbolicName.F functionName, StaticContext staticContext) throws XPathException {
        if (signatures.containsKey(functionName)) {
            SpecificFunctionType type = signatures.get(functionName);
            ExtensionFunctionDefinition defn = new StubExtensionDefinition(functionName.getComponentName(), type);
            try {
                return defn.asFunction(functionName.getArity());
            } catch (Exception err) {
                throw new XPathException("Failed to create call to extension function " + functionName.getComponentName().getDisplayName(), err);
            }
        }
        return null;
    }

    /**
     * Definition of a function corresponding to one of the signatures in the input JSON file.
     */

    private static class StubExtensionDefinition extends ExtensionFunctionDefinition {

        private final StructuredQName functionName;
        private final SpecificFunctionType functionType;

        public StubExtensionDefinition(StructuredQName name, SpecificFunctionType type) {
            this.functionName = name;
            this.functionType = type;
        }
        /**
         * Get the name of the function, as a QName.
         * <p>This method must be implemented in all subclasses</p>
         *
         * @return the function name
         */
        @Override
        public StructuredQName getFunctionQName() {
            return functionName;
        }

        /**
         * Get the required types for the arguments of this function.
         * <p>This method must be implemented in all subtypes.</p>
         *
         * @return the required types of the arguments, as defined by the function signature. Normally
         * this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
         * that allow a variable number of arguments, the array can be smaller than this, with the last
         * entry in the array providing the required type for all the remaining arguments.
         */
        @Override
        public SequenceType[] getArgumentTypes() {
            return functionType.getArgumentTypes();
        }

        /**
         * Get the type of the result of the function
         * <p>This method must be implemented in all subtypes.</p>
         *
         * @param suppliedArgumentTypes the static types of the supplied arguments to the function.
         *                              This is provided so that a more precise result type can be returned in the common
         *                              case where the type of the result depends on the types of the arguments.
         * @return the return type of the function, as defined by its function signature
         */
        @Override
        public SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
            return functionType.getResultType();
        }

        /**
         * Create a call on this function. This method is called by the compiler when it identifies
         * a function call that calls this function.
         *
         * @return an expression representing a call of this extension function
         */
        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new StubFunctionCall();
        }
    }

    /**
     * Executable code of one of the functions in the JSON signature file;
     * the code raises a dynamic error if the function is called.
     */

    private static class StubFunctionCall extends ExtensionFunctionCall {

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            throw new XPathException("Function " + getDefinition().getFunctionQName().getEQName() + " has no implementation");
        }
    }
}

// Copyright (c) 2024 Saxonica Limited
