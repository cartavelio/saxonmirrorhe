using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saxon.Api;
using NUnit.Framework;
using System.Collections;
using NUnit.Framework.Internal;
using Saxon.Hej.expr;
using Saxon.Hej.om;
using Saxon.Hej.s9api;
using Saxon.Hej.type;
using Saxon.Hej.value;
using SaxonCS_NUnit;
using StaticContext = Saxon.Api.StaticContext;
using XdmNodeKind = Saxon.Api.XdmNodeKind;

namespace SaxonCS_NUnit {
    class TestIntegratedExtensionFunctionXslt {
        /**
        * Test integrated extension function in XSLT
        */
        [Test]
        public void TestIntegratedExtensionFunctionXslt1() {
            try {
                Processor proc = new();
                proc.RegisterExtensionFunction(new ZeroArgFunction());
                proc.RegisterExtensionFunction(new Sqrt());
                proc.RegisterExtensionFunction(new InscopePrefixFunction());
                XsltCompiler comp = proc.NewXsltCompiler();
                comp.ErrorReporter = err => Console.Error.WriteLine(err.Message);
                StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='http://math.com/'>" +
                    "<xsl:template name='go'><a sqrt2='{f:sqrt(2)}' insn=\"{f:insn('xsl')}\" zag=\"{f:zag()}\"/></xsl:template>" +
                    "</xsl:stylesheet>");
                XsltExecutable exec = comp.Compile(sr);
                XsltTransformer xsl = exec.Load();
                xsl.InitialTemplate = new QName("go");
                StringWriter sw = new StringWriter();
                Serializer out1 = proc.NewSerializer();
                out1.OutputWriter = sw;
                xsl.Run(out1);

                //"sqrt "
                Assert.IsTrue(sw.ToString().Contains("1.414213"));
                //"zag "
                Assert.IsTrue(sw.ToString().Contains("123.4"));
                //"inscope "
                Assert.IsTrue(sw.ToString().Contains("1999/XSL/Transform"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        /**
         * An IntegratedFunction that takes zero arguments
         */
        public class ZeroArgFunction : ExtensionFunctionDefinition {
            public override QName FunctionName => new QName("", "http://math.com/", "zag");


            /**
             * Return the minimum number of arguments required by the function
             * @return the minimum number of arguments that must be supplied in a call to this function
             */

            public override int MinimumNumberOfArguments => 0;

            /**
             * Return the maximum number of arguments allowed by the function
             * @return the maximum number of arguments that may be supplied in a call to this function
             */

            public override int MaximumNumberOfArguments => 0;

            /**
             * Get the required types for the arguments of this function, counting from zero
             * @return the required types of the argument, as defined by the function signature. Normally
             *         this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
             *         that allow a variable number of arguments, the array can be smaller than this, with the last
             *         entry in the array providing the required type for all the remaining arguments.
             */
            public override XdmSequenceType[] ArgumentTypes => new XdmSequenceType[] { };

            /**
             * Get the type of the result of the function
             * @param suppliedArgumentTypes the static types of the arguments to the function.
             *    This is provided so that a more precise result type can be returned in the common
             *    case where the type of the result depends on the type of the first argument. The value
             *    will be null if the function call has no arguments.
             * @return the return type of the function, as defined by its function signature
             */
            public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
                return new(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?');
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

            public override bool TrustResultType => true;

            /**
             * Create a call on this function. This method is called by the compiler when it identifies
             * a function call that calls this function.
             */
            public override ExtensionFunctionCall MakeFunctionCall() {
                return new ZeroArgFunctionCall();
            }
        }

        public class ZeroArgFunctionCall : ExtensionFunctionCall {
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
            public override XdmValue Call(XdmValue[] arguments, DynamicContext context) {
                return new Saxon.Api.XdmAtomicValue(123.4e0);
            }
        }

        public class Sqrt : ExtensionFunctionDefinition {
            public override QName FunctionName => new QName("http://math.com/", "sqrt");

            public override int MinimumNumberOfArguments => 1;

            public override int MaximumNumberOfArguments => 1;

            public override XdmSequenceType[] ArgumentTypes => new XdmSequenceType[] {
                new(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
            };

            public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
                return new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?');
            }

            public override bool TrustResultType => true;

            public override ExtensionFunctionCall MakeFunctionCall() {
                return new SqrtCall();
            }
        }

        public class SqrtCall : ExtensionFunctionCall {
            public override XdmValue Call(XdmValue[] arguments, DynamicContext context) {
                bool exists = arguments[0].Any();
                if (exists) {
                    XdmAtomicValue arg = (XdmAtomicValue)arguments[0][0];
                    double val = (double)arg.Value;
                    double sqrt = System.Math.Sqrt(val);
                    return new XdmAtomicValue(sqrt);
                }

                return XdmEmptySequence.Instance;
            }
        }


        /**
           * An integrated extension function whose task is to find the namespace URI corresponding to a given
           * prefix in the static namespace context
           */
        public class InscopePrefixFunction : ExtensionFunctionDefinition {
            public override QName FunctionName => new QName("", "http://math.com/", "insn");


            /**
             * Return the minimum number of arguments required by the function
             * @return the minimum number of arguments that must be supplied in a call to this function
             */

            public override int MinimumNumberOfArguments => 1;

            /**
             * Return the maximum number of arguments allowed by the function
             * @return the maximum number of arguments that may be supplied in a call to this function
             */

            public override int MaximumNumberOfArguments => 1;

            /**
             * Get the required types for the arguments of this function, counting from zero
             * @return the required types of the argument, as defined by the function signature. Normally
             *         this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
             *         that allow a variable number of arguments, the array can be smaller than this, with the last
             *         entry in the array providing the required type for all the remaining arguments.
             */
            public override XdmSequenceType[] ArgumentTypes {
                get {
                    return new XdmSequenceType[] {
                        new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), '?')
                    };
                }
            }


            /**
             * Get the type of the result of the function
             * @param suppliedArgumentTypes the static types of the arguments to the function.
             *    This is provided so that a more precise result type can be returned in the common
             *    case where the type of the result depends on the type of the first argument. The value
             *    will be null if the function call has no arguments.
             * @return the return type of the function, as defined by its function signature
             */
            public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
                return new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), '?');
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

            public override bool TrustResultType => true;

            /**
             * Create a call on this function. This method is called by the compiler when it identifies
             * a function call that calls this function.
             */
            public override ExtensionFunctionCall MakeFunctionCall() {
                return new InscopePrefixFunctionCall();
            }
        }

        public class InscopePrefixFunctionCall : ExtensionFunctionCall {
            private StaticContext staticContext;

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
              * @param locationId
              * @param arguments The XPath expressions supplied in the call to this function. The method must not
              *                  modify this array, or any of the expressions contained in the array. @throws net.sf.saxon.trans.XPathException
              *          if the implementation is able to detect a static error in the way the
              *          function is being called (for example it might require that the types of the arguments are
              *          consistent with each other).
              */
            public override void SupplyStaticContext(StaticContext staticContext) {
                this.staticContext = staticContext;
                Console.WriteLine("Processor version = " + staticContext.Processor.ProductTitle);
            }

            // /**
            //  * Copy local data from one copy of the function to another. This method must be implemented
            //  * in any subclass that maintains local data retained from the static context; the job of the
            //  * method is to copy this local data to the supplied destination function.
            //  * @param destination the function to which the local data must be copied. This will always
            //  *                    be an instance of the same function class as the source function.
            //  */
            // public override void CopyLocalData(ExtensionFunctionCall destination) {
            //     ((InscopePrefixFunctionCall)destination).staticContext = staticContext;
            // }

            public override XdmValue Call(XdmValue[] arguments, DynamicContext context) {
                String val = null;
                if (arguments[0].Any()) {
                    val = arguments[0][0].StringValue;
                }

                if (val == null) {
                    return XdmEmptySequence.Instance;
                }

                string ns = staticContext.GetNamespaceForPrefix(val);
                if (ns == null) {
                    return XdmEmptySequence.Instance;
                }

                XdmAtomicValue result = new(ns);
                return result;
            }
        }

        /**
     * An integrated extension function whose task is to find the namespace URI corresponding to a given
     * prefix in the static namespace context. In fact it defines two functions: insn#0 returns the
     * default namespace for elements and types in the static context, while insn#1 returns the namespace
     * corresponding to a given prefix.
     */
        public class MultiArityFunction : ExtensionFunctionDefinition {
            private static readonly QName qName =
                new QName("", "http://math.com/", "join");

            /**
         * Get the function name, as a QName
         *
         * @return the QName of the function
         */

            public override QName FunctionName => qName;

            /**
         * Return the minimum number of arguments required by the function
         *
         * @return the minimum number of arguments that must be supplied in a call to this function
         */

            public override int MinimumNumberOfArguments => 0;

            /**
         * Return the maximum number of arguments allowed by the function
         *
         * @return the maximum number of arguments that may be supplied in a call to this function
         */

            public override int MaximumNumberOfArguments => 1;

            /**
         * Get the required types for the arguments of this function, counting from zero
         *
         * @return the required types of the argument, as defined by the function signature. Normally
         * this should be an array of size {@link #getMaximumNumberOfArguments()}; however for functions
         * that allow a variable number of arguments, the array can be smaller than this, with the last
         * entry in the array providing the required type for all the remaining arguments.
         */

            public override XdmSequenceType[] ArgumentTypes =>
                new XdmSequenceType[] { XdmAtomicType.String.One };


            /**
         * Get the type of the result of the function
         *
         * @param suppliedArgumentTypes the static types of the arguments to the function.
         *                              This is provided so that a more precise result type can be returned in the common
         *                              case where the type of the result depends on the type of the first argument. The value
         *                              will be null if the function call has no arguments.
         * @return the return type of the function, as defined by its function signature
         */
            public override XdmSequenceType ResultType(XdmSequenceType[] suppliedArgumentTypes) =>
                XdmAtomicType.String.ZeroOrMore;

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


            public override bool TrustResultType => true;


            /**
         * Create a call on this function. This method is called by the compiler when it identifies
         * a function call that calls this function.
         */
            public override ExtensionFunctionCall MakeFunctionCall() {
                return new MultiArityFunctionCall();
            }
        }

        private class MultiArityFunctionCall : ExtensionFunctionCall {
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
            public override XdmValue Call(XdmValue[] arguments, DynamicContext context) {
                if (arguments.Length == 0) {
                    return new XdmAtomicValue("/");
                }
                else {
                    XdmAtomicValue val = (XdmAtomicValue)arguments[0].ItemAt(0);
                    if (val == null) {
                        return XdmEmptySequence.Instance;
                        ;
                    }

                    return new XdmAtomicValue(val.StringValue + "/" + val.StringValue);
                }
            }
        }


        [Test]
        public void testVariableArityFunctionXPath() {
            try {
                Processor proc = new Processor();
                proc.RegisterExtensionFunction(new TestIntegratedExtensionFunctionXslt.MultiArityFunction());
                XPathCompiler comp = proc.NewXPathCompiler();
                comp.DeclareNamespace("if", "http://math.com/");
                comp.DeclareNamespace("", "http://example.com/ns");
                comp.DeclareVariable(new QName("arg"));
                XPathExecutable exp = comp.Compile("if:join() || '~' || if:join($arg)");
                XPathSelector ev = exp.Load();
                ev.SetVariable(new QName("arg"), new XdmAtomicValue("if"));
                XdmValue val = ev.Evaluate();
                String result = val.ToString();
                Assert.AreEqual("/~if/if", result, "result ");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestVariableArityFunctionXPathDynamic() {
            try {
                Processor proc = new Processor();
                proc.RegisterExtensionFunction(new TestIntegratedExtensionFunctionXslt.MultiArityFunction());
                XPathCompiler comp = proc.NewXPathCompiler();
                comp.DeclareNamespace("if", "http://math.com/");
                comp.DeclareNamespace("", "http://example.com/ns");
                comp.DeclareVariable(new QName("arg"));
                XPathExecutable exp = comp.Compile("if:join#0() || '~' || if:join#1($arg)");
                XPathSelector ev = exp.Load();
                ev.SetVariable(new QName("arg"), new XdmAtomicValue("if"));
                XdmValue val = ev.Evaluate();
                String result = val.ToString();
                Assert.AreEqual("/~if/if", result, "result ");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestVariableArityFunctionXQueryLookup() {
            try {
                Processor proc = new Processor();
                proc.RegisterExtensionFunction(new TestIntegratedExtensionFunctionXslt.MultiArityFunction());
                XQueryCompiler comp = proc.NewXQueryCompiler();
                comp.DeclareNamespace("if", "http://math.com/");
                comp.DeclareNamespace("", "http://example.com/ns");
                XQueryExecutable exp = comp.Compile(
                    "declare variable $arg external;" +
                    "let $ns := 'http://math.com/', "
                    + "$f0 := function-lookup(QName($ns, 'join'), 0),"
                    + "$f1 := function-lookup(QName($ns, 'join'), 1)"
                    + "return $f0() || '~' || $f1($arg)");
                XQueryEvaluator ev = exp.Load();
                ev.SetExternalVariable(new QName("arg"), new XdmAtomicValue("if"));
                XdmValue val = ev.Evaluate();
                String result = val.ToString();
                Assert.AreEqual("/~if/if", result, "result ");
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void testExternalObjectParam() {
            string xslt =
                @"<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                @" xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:ext='http://www.ctk.cz/mrs/extensions/'>" +
                @"<xsl:output method='text'/>" +
                @"<xsl:param name='pgconn' required='yes'/>" +
                @"<xsl:template name = 'xsl:initial-template'>" +
                @"<xsl:sequence select='ext:testFunc($pgconn)'/>" +
                @"</xsl:template>" +
                @"</xsl:transform>";

            Processor processor = new Processor(true);
            processor.RegisterExtensionFunction(new TestFunc());
            XsltCompiler comp = processor.NewXsltCompiler();
            Xslt30Transformer transformer = comp.Compile(new StringReader(xslt)).Load30();
            var p = new Dictionary<QName, XdmValue>();
            p.Add(new QName("pgconn"), new XdmExternalObject(Encoding.UTF8));
            transformer.SetStylesheetParameters(p);
            Serializer serializer = processor.NewSerializer();
            serializer.OutputWriter = Console.Out;
            serializer.SetOutputProperty(Serializer.INDENT, "yes");
            transformer.CallTemplate(null, serializer);
        }

        
        [Test] // Bug 5658
        public void TestUseOfProcessorContext() {

            Processor proc = new();
            proc.RegisterExtensionFunction(new ExtensionFunctionUsingProcessor());
            var xqueryCompiler = proc.NewXQueryCompiler();
            var xqueryCode = @"
declare namespace mf = 'http://example.com/mf';
let $doc1 := mf:parse-xml('<root><item>a</item><item>b</item><item>1</item></root>')
return 
  <robot>{ $doc1//item[matches(., '\p{L}+')] }</robot>
";
            var xquery = xqueryCompiler.Compile(xqueryCode).Load();
            var result = xquery.EvaluateSingle();
            Assert.IsInstanceOf<XdmNode>(result);
            Assert.AreEqual(XdmNodeKind.Element, ((XdmNode)result).NodeKind);
            Assert.AreEqual("robot", ((XdmNode)result).NodeName.LocalName);

            var xpath = proc.NewXPathCompiler().Compile("Q{http://example.com/mf}parse-xml('<item/>')");
            var xpathResult = xpath.Load().EvaluateSingle();
            Assert.IsInstanceOf<XdmNode>(xpathResult);
            Assert.AreEqual(XdmNodeKind.Document, ((XdmNode)xpathResult).NodeKind);

            var xslt = proc.NewXsltCompiler().Compile(new StringReader(
                "<out xsl:version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                "<xsl:copy-of select='Q{http://example.com/mf}parse-xml(\"&lt;item/>\")'/>" + 
                "</out>"));
            var xsltTransform = xslt.Load30();

            var xsltResult = xsltTransform.ApplyTemplates(
                proc.NewDocumentBuilder().Build(new StringReader("<dummy/>")));
            Assert.IsInstanceOf<XdmNode>(xsltResult);
            Assert.AreEqual(XdmNodeKind.Element, ((XdmNode)xsltResult).NodeKind);
            Assert.AreEqual("out", ((XdmNode)xsltResult).NodeName.LocalName);
        }
    }

    class TestFunc : ExtensionFunctionDefinition {
        public override QName FunctionName => new QName("http://www.ctk.cz/mrs/extensions/", "testFunc");
        public override int MinimumNumberOfArguments => 1;
        public override int MaximumNumberOfArguments => 1;

        public override XdmSequenceType[] ArgumentTypes =>
            new XdmSequenceType[] { new XdmSequenceType(XdmAnyItemType.Instance, ' ') };

        public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) =>
            new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' ');

        public override bool TrustResultType => true;

        public override ExtensionFunctionCall MakeFunctionCall() {
            return new TestFuncCall();
        }
    }

    class TestFuncCall : ExtensionFunctionCall {
        public override XdmValue Call(XdmValue[] arguments, DynamicContext context) {
            var arg = (XdmExternalObject)arguments[0][0];
            Encoding pgconn = (Encoding)arg.GetObject();
            Assert.That(ReferenceEquals(pgconn, Encoding.UTF8));
            return new XdmAtomicValue("OK");
        }
    }

    class ExtensionFunctionUsingProcessor : ExtensionFunctionDefinition {
        public override QName FunctionName => new QName("http://example.com/mf", "parse-xml");

        public override int MinimumNumberOfArguments => 1;

        public override int MaximumNumberOfArguments => 1;

        public override XdmSequenceType[] ArgumentTypes => new XdmSequenceType[] {
            new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), XdmSequenceType.ONE)
        };

        public override ExtensionFunctionCall MakeFunctionCall()
        {
            return new ExtensionFunctionUsingProcessorCall();
        }

        public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes)
        {
            return new XdmSequenceType(XdmAnyNodeType.Instance, XdmSequenceType.ONE);
        }
        
        public class ExtensionFunctionUsingProcessorCall : ExtensionFunctionCall
        {

            Processor processor;
            public override XdmValue Call(XdmValue[] arguments, DynamicContext context)
            {
                string inputString = arguments[0][0].StringValue;
                Assert.NotNull(processor);
                Assert.AreSame(processor, context.Processor);
                return processor.NewDocumentBuilder().Build(new StringReader(inputString));
            }

            // public override void CopyLocalData(ExtensionFunctionCall destination)
            // {
            //     base.CopyLocalData(destination);
            //     var castedDestination = (ExtensionFunctionUsingProcessorCall)destination;
            //     castedDestination.processor = processor;
            // }

            public override void SupplyStaticContext(StaticContext context)
            {
                base.SupplyStaticContext(context);
                processor = context.Processor;
            }
        }
    
    }
    
    
}