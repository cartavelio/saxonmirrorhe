package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ExtensionFunctionCall;
import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.FileOutputStream;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *  JUnit tests for integrated extension functions in XSLT, XQuery, and XPath
 */
public class TestIntegratedFunctions {

    /**
     * Test integrated extension function in XSLT
     */

    @Test
    public void testIntegratedExtensionFunctionXslt() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new ZeroArgFunction());
            proc.registerExtensionFunction(new SqrtFunction());
            proc.registerExtensionFunction(new InscopePrefixFunction());
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='http://math.com/'>" +
                    "<xsl:template name='go'><a sqrt2='{f:sqrt(2)}' insn=\"{f:insn('xsl')}\" zag=\"{f:zag()}\"/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            xsl.setInitialTemplate(new QName("go"));
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer();
            out.setOutputWriter(sw);
            xsl.setDestination(out);
            xsl.transform();
            assertTrue(sw.toString().contains("1.414213"), "sqrt ");
            assertTrue(sw.toString().contains("123.4"), "zag ");
            assertTrue(sw.toString().contains("1999/XSL/Transform"), "inscope ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * Test overriding of integrated extension function using stylesheet functions in XSLT
     */

    @Test
    public void testOverrideFunctionXsltOverriding() {
        testOverrideFunctionXslt(xslWithOverridingFunction1, true);
        testOverrideFunctionXslt(xslWithOverridingFunction2, true);
    }

    @Test
    public void testOverrideFunctionXsltUnderriding() {
        testOverrideFunctionXslt(xslWithOverridingFunction1, false);
        testOverrideFunctionXslt(xslWithOverridingFunction2, false);
    }

    private static final String xslWithOverridingFunction1 =
            "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='http://math.com/'>" +
                    "<xsl:template name='go'><a sqrt2='{f:sqrt(2)}' />" +
                    "</xsl:template>" +
                    "<xsl:param name='O' as='xs:boolean' static='true'/>" +
                    "<xsl:function name='f:sqrt' as='xs:string' _override-extension-function='{$O}'>" +
                    "<xsl:param name='num'/>" +
                    "<xsl:sequence select=\"'local'\" />" +
                    "</xsl:function>" +
                    "</xsl:stylesheet>";

    private static final String xslWithOverridingFunction2 =
            "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='http://math.com/'>" +
                    "<xsl:template name='go'><a sqrt2='{f:sqrt(2)}' />" +
                    "</xsl:template>" +
                    "<xsl:param name='O' as='xs:boolean' static='true'/>" +
                    "<xsl:function name='f:sqrt' as='xs:string' _override='{$O}'>" +
                    "<xsl:param name='num'/>" +
                    "<xsl:sequence select=\"'local'\" />" +
                    "</xsl:function>" +
                    "</xsl:stylesheet>";

    private static void testOverrideFunctionXslt(String xsl, boolean override) {
        // Bug #4111
        try {
            ConfigTest ctest = new ConfigTest();
            Processor proc = new Processor(true);
            proc.registerExtensionFunction(new SqrtFunction());
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            StringReader sr = new StringReader(xsl);
            comp.setParameter(new QName("O"), new XdmAtomicValue(override));
            XsltExecutable exec1 = comp.compile(new StreamSource(sr));
            List<XsltExecutable> execs = new ArrayList<>();
            execs.add(exec1);
            exec1.export(new FileOutputStream(ctest.getDataDir() + "sandpit/export.sef"));
            XsltExecutable exec2 = comp.loadExecutablePackage(new File(ctest.getDataDir() + "sandpit/export.sef").toURI());
            execs.add(exec2);
            for (XsltExecutable exec : execs) {
                XsltTransformer trans = exec.load();
                trans.setInitialTemplate(new QName("go"));
                StringWriter sw = new StringWriter();
                Serializer out = proc.newSerializer();
                out.setOutputWriter(sw);
                trans.setDestination(out);
                trans.transform();
                assertEquals(override, sw.toString().contains("local"), "sqrt ");
            }

        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testIntegratedExtensionFunctionXQuery() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new SqrtFunction());
            proc.registerExtensionFunction(new InscopePrefixFunction());
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryExecutable exp = comp.compile("<a xmlns:f='http://math.com/' s='{f:sqrt(2.0)}' i=\"{f:insn('local')}\"/>");
            XQueryEvaluator ev = exp.load();
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertTrue(result.contains("1.414213"), "sqrt ");
            assertTrue(result.contains("2005/xquery-local-functions"), "inscope ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testIntegratedExtensionFunctionXPath() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new SqrtFunction());
            proc.registerExtensionFunction(new InscopePrefixFunction());
            XPathCompiler comp = proc.newXPathCompiler();
            comp.declareNamespace("if", "http://math.com/");
            comp.declareVariable(new QName("arg"));
            XPathExecutable exp = comp.compile("if:sqrt($arg)");
            XPathSelector ev = exp.load();
            ev.setVariable(new QName("arg"), new XdmAtomicValue(2.0));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertTrue(result.contains("1.414213"), "sqrt ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testIntegratedExtensionFunctionConfigFile() {
        try {
            String config =
                        "<configuration xmlns='http://saxon.sf.net/ns/configuration'>\n" +
                            "<resources>\n" +
                                "<extensionFunction>s9apitest.TestIntegratedFunctions$SqrtFunction</extensionFunction>\n" +
                            "</resources>\n" +
                        "</configuration>";
            Processor proc = new Processor(new StreamSource(new StringReader(config)));
            XPathCompiler comp = proc.newXPathCompiler();
            comp.declareNamespace("if", "http://math.com/");
            comp.declareVariable(new QName("arg"));
            XPathExecutable exp = comp.compile("if:sqrt($arg)");
            XPathSelector ev = exp.load();
            ev.setVariable(new QName("arg"), new XdmAtomicValue(2.0));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertTrue(result.contains("1.414213"), "sqrt ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    /**
     * Test that nodes returned by an integrated extension function are checked. Bug 4185.
     */

    @Test
    public void testIntegratedExtensionFunctionReturningNodesFromWrongConfiguration() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new ExtensionFunction() {
                @Override
                public QName getName() {
                    return new QName("my", "http://www.my.com/", "function");
                }

                @Override
                public SequenceType getResultType() {
                    return SequenceType.makeSequenceType(ItemType.ANY_NODE, OccurrenceIndicator.ONE);
                }

                @Override
                public SequenceType[] getArgumentTypes() {
                    return new SequenceType[0];
                }

                @Override
                public XdmValue call(XdmValue[] arguments) throws SaxonApiException {
                    Processor processor = new Processor(false);
                    DocumentBuilder builder = processor.newDocumentBuilder();
                    return builder.build(new StreamSource(new StringReader("<a/>")));
                }
            });
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='http://www.my.com/'>" +
                            "<xsl:template name='go'><a name='{my:function()/a}'/></xsl:template>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            xsl.setInitialTemplate(new QName("go"));
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer();
            out.setOutputWriter(sw);
            xsl.setDestination(out);
            xsl.transform();
            fail("Unexpected success");
        } catch (Exception e) {
            assertTrue(e.getMessage().contains("incompatible"));
        }
    }


    /**
     * An IntegratedFunction whose task is to compute a square root
     */


    public static class SqrtFunction extends ExtensionFunctionDefinition {


        private static final StructuredQName qName =
                new StructuredQName("", "http://math.com/", "sqrt");

        /**
         * Get the function name, as a QName
         * @return the QName of the function
         */

        @Override
        public StructuredQName getFunctionQName() {
            return qName;
        }

        /**
         * Return the minimum number of arguments required by the function
         * @return the minimum number of arguments that must be supplied in a call to this function
         */

        @Override
        public int getMinimumNumberOfArguments() {
            return 1;
        }

        /**
         * Return the maximum number of arguments allowed by the function
         * @return the maximum number of arguments that may be supplied in a call to this function
         */

        @Override
        public int getMaximumNumberOfArguments() {
            return 1;
        }

        /**
         * Get the required types for the arguments of this function, counting from zero
         * @return the required types of the argument, as defined by the function signature. Normally
         *         this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
         *         that allow a variable number of arguments, the array can be smaller than this, with the last
         *         entry in the array providing the required type for all the remaining arguments.
         */

        @Override
        public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.value.SequenceType[]{net.sf.saxon.value.SequenceType.OPTIONAL_DOUBLE};
        }

        @Override
        public net.sf.saxon.value.SequenceType getResultType(net.sf.saxon.value.SequenceType[] suppliedArgumentTypes) {
            return net.sf.saxon.value.SequenceType.OPTIONAL_DOUBLE;
        }

        /**
         * Get the type of the result of the function
         * @param suppliedArgumentTypes the static types of the arguments to the function.
         *    This is provided so that a more precise result type can be returned in the common
         *    case where the type of the result depends on the type of the first argument. The value
         *    will be null if the function call has no arguments.
         * @return the return type of the function, as defined by its function signature
         */

        public net.sf.saxon.value.SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
            return net.sf.saxon.value.SequenceType.OPTIONAL_DOUBLE;
        }

        /**
         * Ask whether the result actually returned by the function should be checked against
         * the declared type.
         * @return true if the function implementation warrants that the value it returns will
         *         be an instance of the declared result type. The default value is false, in which case
         *         the result will be checked at run-time to ensure that it conforms to the declared type.
         *         If the value true is returned, but the function returns a value of the wrong type, the
         *         consequences are unpredictable.
         */

        @Override
        public boolean trustResultType() {
            return true;
        }

        /**
     * Create a call on this function. This method is called by the compiler when it identifies
         * a function call that calls this function.
         */

        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new SqrtFunctionCall();
        }
    }

    private static class SqrtFunctionCall extends ExtensionFunctionCall {

        /**
         * Optimize the function call. This method is called at compile time if expression optimization
         * is enabled. It gives the implementation an opportunity to replace itself with an optimized implementation
         * that returns the same result. It is the responsibility of this method to ensure that the returned
         * expression is equivalent; part of this responsibility is to ensure that it always returns a value
         * of the correct type. The system will not check this.
         * @param context   The static context in which the function call appears. The method must not modify
         *                  the static context.
         * @param arguments The XPath expressions supplied in the call to this function. The method must not
         *                  modify this array, or any of the expressions contained in the array.
         * @return an expression to be evaluated at run-time in place of this function call. Return null
         *         if no optimization is possible and the function call should be used unchanged. Return a
         *         {@link net.sf.saxon.expr.Literal} representing the result of the function call if the function call
         *         can be precomputed at compile time
         * @throws net.sf.saxon.trans.XPathException
         *          if the implementation is able to detect a static error in the way the
         *          function is being called (for example it might require that the types of the arguments are
         *          consistent with each other).
         */

        @Override
        public Expression rewrite(StaticContext context, Expression[] arguments) throws XPathException {
            if (arguments[0] instanceof Literal) {
                DoubleValue val = (DoubleValue)((Literal)arguments[0]).getGroundedValue();
                if (val == null) {
                    return Literal.makeEmptySequence();
                }
                double sqrt = Math.sqrt(val.getDoubleValue());
                return Literal.makeLiteral(new DoubleValue(sqrt));
            } else {
                return null;
            }
        }

        /**
         * Evaluate this function call at run-time
         *
         *
         * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call. Each argument value (which is in general
         *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
         *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
         *                  If the argument is always a singleton, then the single item may be obtained by calling
         *                  <code>arguments[i].next()</code>.
         * @return an iterator over the results of the function. If the result is a single item, it can be
         *         returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}.
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs during evaluation of the function.
         */

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            DoubleValue val = (DoubleValue)arguments[0].head();
            if (val == null) {
                return EmptySequence.getInstance();
            }
            double sqrt = Math.sqrt(val.getDoubleValue());
            return new DoubleValue(sqrt);
        }
    }

    /**
     * An IntegratedFunction that takes zero arguments
     */


    public static class ZeroArgFunction extends ExtensionFunctionDefinition {


        private static final StructuredQName qName =
                new StructuredQName("", "http://math.com/", "zag");

        /**
         * Get the function name, as a QName
         * @return the QName of the function
         */

        @Override
        public StructuredQName getFunctionQName() {
            return qName;
        }

        /**
         * Return the minimum number of arguments required by the function
         * @return the minimum number of arguments that must be supplied in a call to this function
         */

        @Override
        public int getMinimumNumberOfArguments() {
            return 0;
        }

        /**
         * Return the maximum number of arguments allowed by the function
         * @return the maximum number of arguments that may be supplied in a call to this function
         */

        @Override
        public int getMaximumNumberOfArguments() {
            return 0;
        }

        /**
         * Get the required types for the arguments of this function, counting from zero
         * @return the required types of the argument, as defined by the function signature. Normally
         *         this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
         *         that allow a variable number of arguments, the array can be smaller than this, with the last
         *         entry in the array providing the required type for all the remaining arguments.
         */

        @Override
        public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.value.SequenceType[0];
        }

        /**
         * Get the type of the result of the function
         * @param suppliedArgumentTypes the static types of the arguments to the function.
         *    This is provided so that a more precise result type can be returned in the common
         *    case where the type of the result depends on the type of the first argument. The value
         *    will be null if the function call has no arguments.
         * @return the return type of the function, as defined by its function signature
         */

        @Override
        public net.sf.saxon.value.SequenceType getResultType(net.sf.saxon.value.SequenceType[] suppliedArgumentTypes) {
            return net.sf.saxon.value.SequenceType.OPTIONAL_DOUBLE;
        }

        /**
         * Ask whether the result actually returned by the function should be checked against
         * the declared type.
         * @return true if the function implementation warrants that the value it returns will
         *         be an instance of the declared result type. The default value is false, in which case
         *         the result will be checked at run-time to ensure that it conforms to the declared type.
         *         If the value true is returned, but the function returns a value of the wrong type, the
         *         consequences are unpredictable.
         */

        @Override
        public boolean trustResultType() {
            return true;
        }

        /**
     * Create a call on this function. This method is called by the compiler when it identifies
         * a function call that calls this function.
         */

        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new ZeroArgFunctionCall();
        }
    }

    private static class ZeroArgFunctionCall extends ExtensionFunctionCall {

        /**
         * Evaluate this function call at run-time
         *
         *
 * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call. Each argument value (which is in general
         *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
         *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
         *                  If the argument is always a singleton, then the single item may be obtained by calling
         *                  <code>arguments[i].next()</code>.
         * @return an iterator over the results of the function. If the result is a single item, it can be
         *         returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}.
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs during evaluation of the function.
         */

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new DoubleValue(123.4e0);
        }
    }


    /**
     * An integrated extension function whose task is to find the namespace URI corresponding to a given
     * prefix in the static namespace context. In fact it defines two functions: insn#0 returns the
     * default namespace for elements and types in the static context, while insn#1 returns the namespace
     * corresponding to a given prefix.
     */

    public static class InscopePrefixFunction extends ExtensionFunctionDefinition {


        private static final StructuredQName qName =
                new StructuredQName("", "http://math.com/", "insn");

        /**
         * Get the function name, as a QName
         * @return the QName of the function
         */

        @Override
        public StructuredQName getFunctionQName() {
            return qName;
        }

        /**
         * Return the minimum number of arguments required by the function
         * @return the minimum number of arguments that must be supplied in a call to this function
         */

        @Override
        public int getMinimumNumberOfArguments() {
            return 0;
        }

        /**
         * Return the maximum number of arguments allowed by the function
         * @return the maximum number of arguments that may be supplied in a call to this function
         */

        @Override
        public int getMaximumNumberOfArguments() {
            return 1;
        }

        /**
         * Get the required types for the arguments of this function, counting from zero
         * @return the required types of the argument, as defined by the function signature. Normally
         * this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
         * that allow a variable number of arguments, the array can be smaller than this, with the last
         * entry in the array providing the required type for all the remaining arguments.
         */

        @Override
        public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.value.SequenceType[]{net.sf.saxon.value.SequenceType.SINGLE_STRING};
        }

        /**
         * Get the type of the result of the function
         * @param suppliedArgumentTypes the static types of the arguments to the function.
         *    This is provided so that a more precise result type can be returned in the common
         *    case where the type of the result depends on the type of the first argument. The value
         *    will be null if the function call has no arguments.
         * @return the return type of the function, as defined by its function signature
         */

        @Override
        public net.sf.saxon.value.SequenceType getResultType(net.sf.saxon.value.SequenceType[] suppliedArgumentTypes) {
            return net.sf.saxon.value.SequenceType.OPTIONAL_STRING;
        }

        /**
         * Ask whether the result actually returned by the function should be checked against
         * the declared type.
         * @return true if the function implementation warrants that the value it returns will
         *         be an instance of the declared result type. The default value is false, in which case
         *         the result will be checked at run-time to ensure that it conforms to the declared type.
         *         If the value true is returned, but the function returns a value of the wrong type, the
         *         consequences are unpredictable.
         */

        @Override
        public boolean trustResultType() {
            return true;
        }

        /**
         * Create a call on this function. This method is called by the compiler when it identifies
         * a function call that calls this function.
         */

        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new InscopePrefixFunctionCall();
        }
    }

    private static class InscopePrefixFunctionCall extends ExtensionFunctionCall {

        private NamespaceResolver nsContext;
        private NamespaceUri defaultElementNamespace;

        /**
          * Supply static context information.
          * <p>This method is called during compilation to provide information about the static context in which
          * the function call appears. If the implementation of the function needs information from the static context,
          * then it should save it now, as it will not be available later at run-time.</p>
          * <p>The implementation also has the opportunity to examine the expressions that appear in the
          * arguments to the function call at this stage. These might already have been modified from the original
          * expressions as written by the user. The implementation should not modify any of these expressions.</p>
          * <p>The default implementation of this method does nothing.</p>
          * @param context   The static context in which the function call appears. The method must not modify
          *                  the static context.
          * @param locationId the location of the node in the source, or of the instruction that created it
          *@param arguments The XPath expressions supplied in the call to this function. The method must not
          *                  modify this array, or any of the expressions contained in the array. @throws net.sf.saxon.trans.XPathException
          *          if the implementation is able to detect a static error in the way the
          *          function is being called (for example it might require that the types of the arguments are
          *          consistent with each other).
          */

         @Override
         public void supplyStaticContext(StaticContext context, int locationId, Expression[] arguments) throws XPathException {
             nsContext = context.getNamespaceResolver();
             defaultElementNamespace = context.getDefaultElementNamespace();
         }



        /**
         * Copy local data from one copy of the function to another. This method must be implemented
         * in any subclass that maintains local data retained from the static context; the job of the
         * method is to copy this local data to the supplied destination function.
         * @param destination the function to which the local data must be copied. This will always
         *                    be an instance of the same function class as the source function.
         */

//        @Override
//        public void copyLocalData(ExtensionFunctionCall destination) {
//            ((InscopePrefixFunctionCall)destination).nsContext = nsContext;
//        }

        /**
         * Evaluate this function call at run-time
         *
         * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call. Each argument value (which is in general
         *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
         *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
         *                  If the argument is always a singleton, then the single item may be obtained by calling
         *                  <code>arguments[i].next()</code>.
         * @return an iterator over the results of the function. If the result is a single item, it can be
         * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}.
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function.
         */

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NamespaceUri ns;
            if (arguments.length == 0) {
                ns = defaultElementNamespace;
            } else {
                StringValue val = (StringValue) arguments[0].head();
                if (val == null) {
                    return EmptySequence.getInstance();
                }
                ns = nsContext.getURIForPrefix(val.getStringValue(), false);
            }
            if (ns == null) {
                return EmptySequence.getInstance();
            }
            return new StringValue(ns.toUnicodeString());
        }
    }

    /**
     * An integrated extension function whose task is to find the namespace URI corresponding to a given
     * prefix in the static namespace context. In fact it defines two functions: insn#0 returns the
     * default namespace for elements and types in the static context, while insn#1 returns the namespace
     * corresponding to a given prefix.
     */

    public static class MultiArityFunction extends ExtensionFunctionDefinition {


        private static final StructuredQName qName =
                new StructuredQName("", "http://math.com/", "join");

        /**
         * Get the function name, as a QName
         *
         * @return the QName of the function
         */

        @Override
        public StructuredQName getFunctionQName() {
            return qName;
        }

        /**
         * Return the minimum number of arguments required by the function
         *
         * @return the minimum number of arguments that must be supplied in a call to this function
         */

        @Override
        public int getMinimumNumberOfArguments() {
            return 0;
        }

        /**
         * Return the maximum number of arguments allowed by the function
         *
         * @return the maximum number of arguments that may be supplied in a call to this function
         */

        @Override
        public int getMaximumNumberOfArguments() {
            return 1;
        }

        /**
         * Get the required types for the arguments of this function, counting from zero
         *
         * @return the required types of the argument, as defined by the function signature. Normally
         * this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
         * that allow a variable number of arguments, the array can be smaller than this, with the last
         * entry in the array providing the required type for all the remaining arguments.
         */

        @Override
        public net.sf.saxon.value.SequenceType[] getArgumentTypes() {
            return new net.sf.saxon.value.SequenceType[]{net.sf.saxon.value.SequenceType.SINGLE_STRING};
        }

        /**
         * Get the type of the result of the function
         *
         * @param suppliedArgumentTypes the static types of the arguments to the function.
         *                              This is provided so that a more precise result type can be returned in the common
         *                              case where the type of the result depends on the type of the first argument. The value
         *                              will be null if the function call has no arguments.
         * @return the return type of the function, as defined by its function signature
         */

        @Override
        public net.sf.saxon.value.SequenceType getResultType(net.sf.saxon.value.SequenceType[] suppliedArgumentTypes) {
            return net.sf.saxon.value.SequenceType.OPTIONAL_STRING;
        }

        /**
         * Ask whether the result actually returned by the function should be checked against
         * the declared type.
         *
         * @return true if the function implementation warrants that the value it returns will
         * be an instance of the declared result type. The default value is false, in which case
         * the result will be checked at run-time to ensure that it conforms to the declared type.
         * If the value true is returned, but the function returns a value of the wrong type, the
         * consequences are unpredictable.
         */

        @Override
        public boolean trustResultType() {
            return true;
        }

        /**
         * Create a call on this function. This method is called by the compiler when it identifies
         * a function call that calls this function.
         */

        @Override
        public ExtensionFunctionCall makeCallExpression() {
            return new MultiArityFunctionCall();
        }
    }

    private static class MultiArityFunctionCall extends ExtensionFunctionCall {

        /**
         * Evaluate this function call at run-time
         *
         * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call. Each argument value (which is in general
         *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
         *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
         *                  If the argument is always a singleton, then the single item may be obtained by calling
         *                  <code>arguments[i].next()</code>.
         * @return an iterator over the results of the function. If the result is a single item, it can be
         * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}.
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function.
         */

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NamespaceUri ns;
            if (arguments.length == 0) {
                return new StringValue("/");
            } else {
                StringValue val = (StringValue) arguments[0].head();
                if (val == null) {
                    return EmptySequence.getInstance();
                }
                return new StringValue(val.getStringValue() + "/" + val.getStringValue());
            }
        }
    }

    @Test
    public void testSimpleExternalFunctionXPath() {
        try {
            Processor proc = new Processor(false);
            ExtensionFunction sqrt = new ExtensionFunction() {
                @Override
                public QName getName() {
                    return new QName("http://math.com/", "sqrt");
                }

                @Override
                public net.sf.saxon.s9api.SequenceType getResultType() {
                    return net.sf.saxon.s9api.SequenceType.makeSequenceType(
                        ItemType.DOUBLE, OccurrenceIndicator.ONE
                    );
                }

                @Override
                public net.sf.saxon.s9api.SequenceType[] getArgumentTypes() {
                    return new net.sf.saxon.s9api.SequenceType[]{
                        net.sf.saxon.s9api.SequenceType.makeSequenceType(
                            ItemType.DOUBLE, OccurrenceIndicator.ONE)};
                }

                @Override
                public XdmValue call(XdmValue[] arguments) throws SaxonApiException {
                    double arg = ((XdmAtomicValue)arguments[0].itemAt(0)).getDoubleValue();
                    double result = Math.sqrt(arg);
                    return new XdmAtomicValue(result);
                }
            };

            proc.registerExtensionFunction(sqrt);
            XPathCompiler comp = proc.newXPathCompiler();
            comp.declareNamespace("if", "http://math.com/");
            comp.declareVariable(new QName("arg"));
            XPathExecutable exp = comp.compile("if:sqrt($arg)");
            XPathSelector ev = exp.load();
            ev.setVariable(new QName("arg"), new XdmAtomicValue(2.0));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertTrue(result.contains("1.414213"), "sqrt ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testVariableArityFunctionXPath() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new InscopePrefixFunction());
            XPathCompiler comp = proc.newXPathCompiler();
            comp.declareNamespace("if", "http://math.com/");
            comp.declareNamespace("", "http://example.com/ns");
            comp.declareVariable(new QName("arg"));
            XPathExecutable exp = comp.compile("if:insn() || '~' || if:insn($arg)");
            XPathSelector ev = exp.load();
            ev.setVariable(new QName("arg"), new XdmAtomicValue("if"));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertTrue(result.contains("http://example.com/ns~http://math.com/"), "result ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testVariableArityFunctionXPathDynamic() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new MultiArityFunction());
            XPathCompiler comp = proc.newXPathCompiler();
            comp.declareNamespace("if", "http://math.com/");
            comp.declareNamespace("", "http://example.com/ns");
            comp.declareVariable(new QName("arg"));
            XPathExecutable exp = comp.compile("if:join#0() || '~' || if:join#1($arg)");
            XPathSelector ev = exp.load();
            ev.setVariable(new QName("arg"), new XdmAtomicValue("if"));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertEquals("/~if/if", result, "result ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testVariableArityFunctionXQueryLookup() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new MultiArityFunction());
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.declareNamespace("if", "http://math.com/");
            comp.declareNamespace("", "http://example.com/ns");
            XQueryExecutable exp = comp.compile(
                    "declare variable $arg external;" +
                    "let $ns := 'http://math.com/', "
                            + "$f0 := function-lookup(QName($ns, 'join'), 0),"
                            + "$f1 := function-lookup(QName($ns, 'join'), 1)"
                            + "return $f0() || '~' || $f1($arg)");
            XQueryEvaluator ev = exp.load();
            ev.setExternalVariable(new QName("arg"), new XdmAtomicValue("if"));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            assertEquals("/~if/if", result, "result ");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testIntegratedExtensionFunctionXslt3() {
        try {
            Processor proc = new Processor(false);
            proc.registerExtensionFunction(new ConstructDocument(proc));
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='https://w3id.org/atomgraph/client#' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='go'><a name=\"{f:construct-doc(xs:anyURI('http://a/'), xs:anyURI('http://b/'), xs:anyURI('http://c/'))/*/name()}\"/></xsl:template>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            XsltTransformer xsl = exec.load();
            xsl.setInitialTemplate(new QName("go"));
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer();
            out.setOutputWriter(sw);
            xsl.setDestination(out);
            xsl.transform();
            assertTrue(sw.toString().contains("doc52"), "out");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testBug5937 () {
        Processor processor = new Processor(false);

        processor.registerExtensionFunction(new ExtensionFunction() {
            @Override
            public QName getName() {
                return new QName("http://example.math.co.uk/demo", "sqrtSimple");
            }

            @Override
            public SequenceType[] getArgumentTypes() {
                return new SequenceType[]
                        {
                                SequenceType.makeSequenceType(ItemType.DOUBLE, OccurrenceIndicator.ZERO_OR_ONE)
                        };
            }

            @Override
            public SequenceType getResultType() {
                return SequenceType.makeSequenceType(ItemType.DOUBLE, OccurrenceIndicator.ZERO_OR_ONE);
            }

            @Override
            public XdmValue call(XdmValue[] xdmValues) throws SaxonApiException {
                return xdmValues[0].isEmpty() ? XdmEmptySequence.getInstance() : new XdmAtomicValue(Math.sqrt(((XdmAtomicValue) xdmValues[0]).getDoubleValue()));
            }
        });

        String xslt = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "    xmlns:math='http://example.math.co.uk/demo'> " +
                " <xsl:template name='xsl:initial-template'> " +
                "   <out sqrt2='{math:sqrtSimple(2.0e0)}' " +
                "        sqrtEmpty='{math:sqrtSimple(())}'/> " +
                " </xsl:template>" +
                " </xsl:transform>";

        try {
            Xslt30Transformer xslt30Transformer = processor.newXsltCompiler().compile(new StreamSource(new StringReader(xslt))).load30();
            XdmDestination out = new XdmDestination();
            xslt30Transformer.callTemplate(null, out);
            assertTrue(out.getXdmNode().getOutermostElement().getAttributeValue(new QName("", "sqrt2")).startsWith("1.4142"));
            assertEquals(out.getXdmNode().getOutermostElement().getAttributeValue(new QName("", "sqrtEmpty")), "");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }

    }


    public static class ConstructDocument implements ExtensionFunction {

        private final Processor processor;

        public ConstructDocument(Processor processor) {
            this.processor = processor;
        }

        @Override
        public QName getName() {
            return new QName("https://w3id.org/atomgraph/client#", "construct-doc");
        }

        @Override
        public SequenceType getResultType() {
            return SequenceType.makeSequenceType(ItemType.DOCUMENT_NODE,
                                                 OccurrenceIndicator.ZERO_OR_MORE);
        }

        @Override
        public SequenceType[] getArgumentTypes() {
            return new SequenceType[]
                    {
                            SequenceType.makeSequenceType(ItemType.ANY_URI,
                                                          OccurrenceIndicator.ONE),
                            SequenceType.makeSequenceType(ItemType.ANY_URI,
                                                          OccurrenceIndicator.ZERO_OR_MORE),
                            SequenceType.makeSequenceType(ItemType.ANY_URI,
                                                          OccurrenceIndicator.ONE),
                    };
        }

        @Override
        public XdmValue call(XdmValue[] arguments) throws SaxonApiException {
            return processor.newDocumentBuilder().build(new StreamSource(new StringReader("<doc52/>")));
        }

        public Processor getProcessor() {
            return processor;
        }

    }

    @Test
    public void testCardinalityCheck() throws SaxonApiException {
        // Function returns a sequence of two strings, but declares its result type as xs:string
        Processor processor = new Processor(true);
        processor.registerExtensionFunction(new ExtensionFunction() {
            @Override
            public QName getName() {
                return new QName(NamespaceUri.of("https://example.com"), "f");
            }

            @Override
            public SequenceType[] getArgumentTypes() {
                return new SequenceType[0];
            }

            @Override
            public SequenceType getResultType() {
                return ItemType.STRING.with(OccurrenceIndicator.ONE);
            }

            @Override
            public XdmValue call(final XdmValue[] xdmValues) {
                return XdmAtomicValue.makeValue("a")
                        .append(XdmAtomicValue.makeValue("b"));
            }
        });
        XPathCompiler xPathCompiler = processor.newXPathCompiler();
        xPathCompiler.declareNamespace("eg", "https://example.com");
        XPathSelector selector = xPathCompiler.compile("eg:f() eq 'a'").load();
        try {
            selector.evaluate();
            fail("No error thrown");
        } catch (SaxonApiException e) {
            assertEquals("XPTY0004", e.getErrorCode().getLocalName());
        }
    }




}


