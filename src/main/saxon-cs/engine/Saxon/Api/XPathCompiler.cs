using System;
using Saxon.Api.Support;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XPathCompiler</c> object allows XPath queries to be compiled.
    /// The compiler holds information that represents the static context
    /// for the expression.
    /// </summary>
    /// <remarks>
    /// <para>To construct an <c>XPathCompiler</c>, use the factory method
    /// <c>NewXPathCompiler</c> on the <see cref="Processor"/> object.</para>
    /// <para>An <c>XPathCompiler</c> may be used repeatedly to compile multiple
    /// queries. Any changes made to the <c>XPathCompiler</c> (that is, to the
    /// static context) do not affect queries that have already been compiled.
    /// An <c>XPathCompiler</c> may be used concurrently in multiple threads, but
    /// it should not then be modified once initialized.</para>
    /// <para> The <code>XPathCompiler</code> has the ability to maintain a cache of compiled
    /// expressions. This is active only if enabled by setting the <c>Caching</c> property.
    /// If caching is enabled, then the compiler will recognize an attempt to compile
    /// the same expression twice, and will avoid the cost of recompiling it. The cache
    /// is emptied by any method that changes the static context for subsequent expressions,
    /// for example, by setting the <c>BaseUri</c> property. Unless the cache is emptied,
    /// it grows indefinitely: compiled expressions are never discarded.</para>
    /// </remarks>
    [Serializable]
    public class XPathCompiler {
        private Processor processor;

        // internal constructor: the public interface is a factory method
        // on the Processor object

        internal XPathCompiler(Processor processor, Saxon.Hej.s9api.XPathCompiler compiler) {
            this.Implementation = compiler;
            this.processor = processor;
        }


        /// <summary>
        /// Declare the default collation
        /// </summary>
        /// <param name="uri">the absolute URI of the default collation. This URI must identify a known collation;
        /// either one that has been explicitly declared, or one that is recognized implicitly, such as a UCA collation</param>
        public void DeclareDefaultCollation(string uri) {
            Implementation.declareDefaultCollation(uri);
        }

        /// <summary>
        /// Declare a namespace for use by the XPath expression.
        /// </summary>
        /// <param name="prefix">The namespace prefix to be declared. Use
        /// a zero-length string to declare the default namespace (that is, the
        /// default namespace for elements and types).</param>
        /// <param name="uri">The namespace URI. It is possible to specify
        /// a zero-length string to "undeclare" a namespace.</param>
        public void DeclareNamespace(string prefix, string uri) {
            Implementation.declareNamespace(prefix, uri);
        }

        /// <summary>
        /// Set the error reporter to be used for reporting static warnings during compilation.
        /// By default, the <see cref="ErrorReporter"/> associated with the Saxon Configuration is used.
        /// Note that fatal static errors are always reported in the form
        /// of an exception thrown by the <see cref="Compile(String)"/> method, so this method only controls
        /// the handling of warnings
        /// </summary>
        /// <remarks>The property IErrorReporter to which warnings will be notified</remarks>
        public ErrorReporter WarningHandler {
            set => Implementation.setWarningHandler(new ErrorReporterWrapper(value));
        }


        /// <summary>
        /// Make available a set of functions defined in an XSLT 3.0 package. All functions
        /// defined with <c>visibility="public"</c>
        /// (or exposed as public using <c>xsl:expose</c>
        /// become part of the static context for an XPath expression created using this
        /// <c>XPathCompiler</c>. The functions are added to the search path after all existing
        /// functions, including functions added using a previous call on this method.
        /// <p>Note that if the library package includes functions that reference stylesheet parameters
        /// (or global variables that depend on the context item), then there is no way of supplying
        /// values for such parameters; calling such functions will cause a run-time error.</p>
        /// </summary>
        /// <param name="libraryPackage">the XSLT compiled library package whose functions are to be made available</param>
        public void AddXsltFunctionLibrary(XsltPackage libraryPackage) {
            Implementation.addXsltFunctionLibrary(libraryPackage.GetUnderlyingXsltPackage());
        }

        /// <summary>
        /// Get the namespace URI part of a QName provided in lexical form (<c>prefix:localName</c>)
        /// </summary>
        /// <param name="lexicalName">The lexical QName. This may either be a plain <c>NCName</c> (a local name
        /// with no prefix or colon) or a lexical name using a prefix that is bound to a namespace.</param>
        /// <param name="useDefault">Set to true if the default namespace for elements and types is to be used
        /// in the case where there is no prefix. If false, no prefix means no namespace.</param>
        /// <returns>The namespace URI associated with the prefix (or absence thereof) in the supplied
        /// lexical QName. The "null namespace" is represented by a zero length string. The method returns null
        /// if there is no known binding for the prefix used in the lexical QName.</returns>
        public string GetNamespaceURI(string lexicalName, bool useDefault) {
            string[] parts =
                Saxon.Hej.om.NameChecker.checkQNameParts(Saxon.Hej.value.Whitespace.trim(lexicalName));

            return Implementation.getUnderlyingStaticContext().getNamespaceResolver().getURIForPrefix(parts[0], useDefault);
        }

        /// <summary>
        /// The policy for matching unprefixed element names in XPath expressions
        /// </summary>

        public UnprefixedElementMatchingPolicy UnprefixedElementMatchingPolicy {
            get => Implementation.getUnprefixedElementMatchingPolicy();
            set => Implementation.setUnprefixedElementMatchingPolicy(value);
        }

        /// <summary>
        /// The <c>Processor</c> from which this <c>XPathCompiler</c> was constructed
        /// </summary>

        public Processor Processor => processor;

        /// <summary>
        /// Import schema definitions for a specified namespace. That is, add the element and attribute declarations 
        /// and type definitions contained in a given namespace to the static context for the XPath expression.
        /// </summary>
        /// <remarks>
        /// <para>This method will not cause the schema to be loaded. That must be done separately, using the
        /// <c>SchemaManager</c>. This method will not fail if the schema has not been loaded (but in that case
        /// the set of declarations and definitions made available to the XPath expression is empty). The schema
        /// document for the specified namespace may be loaded before or after this method is called.
        /// </para>
        /// <para>
        /// This method does not bind a prefix to the namespace. That must be done separately, using the
        /// <c>DeclareNamespace</c> method.
        /// </para>
        /// </remarks>
        /// <param name="uri">The namespace URI whose declarations and type definitions are to
        /// be made available for use within the XPath expression.</param>
        public void ImportSchemaNamespace(String uri) {
            Implementation.importSchemaNamespace(uri);
        }


        /// <summary>
        /// Escape hatch to the <c>Saxon.Hej.s9api.XPathCompiler</c> object in the underlying Java implementation
        /// </summary>

        private Saxon.Hej.s9api.XPathCompiler Implementation { get; }


        /// <summary>
        /// This property indicates whether the XPath expression may contain references to variables that have not been
        /// explicitly declared by calling <c>DeclareVariable</c>. The property is false by default (that is, variables
        /// must be declared).
        /// </summary>
        /// <remarks>
        /// If undeclared variables are permitted, then it is possible to determine after compiling the expression which
        /// variables it refers to by calling the method <c>EnumerateExternalVariables</c> on the <c>XPathExecutable</c> object.
        /// </remarks>

        public bool AllowUndeclaredVariables {
            get => Implementation.isAllowUndeclaredVariables();
            set => Implementation.setAllowUndeclaredVariables(value);
        }

        /// <summary>
        /// This property indicates whether XPath expressions compiled using this <c>XPathCompiler</c> are
        /// schema-aware. They will automatically be schema-aware if the method
        /// <see cref="ImportSchemaNamespace"/> is called. An XPath expression
        /// must be marked as schema-aware if it is to handle typed (validated)
        /// input documents.
        /// </summary>

        public bool SchemaAware {
            get => Implementation.isSchemaAware();
            set => Implementation.setSchemaAware(value);
        }


        private int checkSingleChar(String s) {
            if (s.Length == 1) {
                return s[0];
            }
            else if (s.Length == 2 && Char.IsSurrogatePair(s, 0)) {
                return Char.ConvertToUtf32(s, 0);
            }
            else {
                throw new ArgumentException("Attribute \"" + s + "\" should be a single character");
            }
        }

        /// <summary>
        /// Sets a property of a selected decimal format, for use by the <c>format-number()</c> function.
        /// </summary>
        /// <remarks>
        /// This method checks that the value is valid for the particular property, but it does not
        /// check that all the values for the decimal format are consistent (for example, that the
        /// decimal separator and grouping separator have different values). This consistency
        /// check is performed only when the decimal format is used.
        /// </remarks>
        /// <param name="format">The name of the decimal format whose property is to be set.
        ///  Supply null to set a property of the default (unnamed) decimal format.
        ///  This corresponds to a name used in the third argument of <c>format-number()</c>.</param>
        /// <param name="property">The name of the property to set: one of
        ///   "decimal-separator", "grouping-separator", "infinity", "NaN",
        ///   "minus-sign", "percent", "per-mille", "zero-digit", "digit",
        ///   or "pattern-separator".</param>
        /// <param name="value">The new value for the property.</param>
        public void SetDecimalFormatProperty(QName format, string property, string value) {
            Implementation.setDecimalFormatProperty(format, property, value);
        }

        /// <summary>
        /// Declare a variable for use by the XPath expression. If the expression
        /// refers to any variables, then they must be declared here, unless the
        /// <c>AllowUndeclaredVariables</c> property has been set to true.
        /// </summary>
        /// <param name="name">The name of the variable, as a <c>QName</c></param>
        public void DeclareVariable(QName name) {
            Implementation.declareVariable(name);
        }

        /// <summary>
        /// This property indicates which version of XPath language syntax is accepted. The accepted values
        /// are "2.0", "3.0", and "3.1". The default is "3.1".
        /// </summary>
        /// <remarks>
        /// <para>Requesting a value other than 3.1 restricts the XPath grammar to constructs defined
        /// in the appropriate version, and uses the appropriate subsets of the functions in the standard
        /// function library. However, the semantics of remaining constructs will generally follow the XPath 3.1
        /// rules. For example, there is a rule in XPath 2.0 that casting to
        /// <c>xs:QName</c> is only permitted if the operand is a string literal, but this is not enforced when
        /// this property is set to "2.0".</para>
        /// <para>There is no support for XPath 1.0.</para>
        /// </remarks>


        public string XPathLanguageVersion {
            get {
                try {
                    return Implementation.getLanguageVersion();
                }
                catch (Exception) {
                    throw new Saxon.Api.SaxonApiException("Unknown XPath version " +
                                                              Implementation.getLanguageVersion());
                }
            }
            set {
                try {
                    Implementation.setLanguageVersion(value);
                }
                catch (Exception) {
                    throw new Saxon.Api.SaxonApiException("Unknown XPath version " + value);
                }
            }
        }


        /// <summary>
        /// The required context item type for the expression. This is used for
        /// optimizing the expression at compile time, and to check at run-time
        /// that the value supplied for the context item is the correct type.
        /// </summary>

        public XdmItemType ContextItemType {
            get => Implementation.getRequiredContextItemType();
            set => Implementation.setRequiredContextItemType(value);
        }


        /// <summary>
        /// The base URI of the expression, which forms part of the static context
        /// of the expression. This is used for resolving any relative URIs appearing
        /// within the expression, for example in the argument to the <c>doc()</c> function.
        /// </summary>

        public Uri BaseUri {
            get => Implementation.getBaseURI();
            set => Implementation.setBaseURI(value);
        }

        /// <summary>
        /// XPath 1.0 Backwards Compatibility Mode (that is, XPath 1.0 compatibility mode). 
        /// If true, backwards compatibility mode
        /// is set. In backwards compatibility mode, more implicit type conversions are
        /// allowed in XPath expressions, for example it is possible to compare a number
        /// with a string. The default is false (backwards compatibility mode is off).
        /// </summary>
        /// <remarks>
        /// <para>Setting XPath 1.0 compatibility mode does not prevent the use of constructs
        /// defined in a later XPath version; rather, it modifies the semantics of some
        /// constructs.</para></remarks>

        public bool BackwardsCompatible {
            get => Implementation.isBackwardsCompatible();
            set => Implementation.setBackwardsCompatible(value);
        }

        /// <summary>
        /// This property controls caching of compiled XPath expressions. If caching is enabled,
        /// then compiled expressions are saved in a cache and reused if the same expression is compiled
        /// again. The cache is cleared (invalidated) if any change is made to the properties of the
        /// <c>XPathCompiler</c> that would affect the validity of cached expressions. Caching is disabled
        /// by default.
        /// </summary>

        public bool Caching {
            get => Implementation.isCaching();
            set => Implementation.setCaching(value);
        }


        /// <summary>
        /// Request fast compilation. Fast compilation will generally be achieved at the expense of run-time performance
        /// and quality of diagnostics. Fast compilation is a good trade-off if (a) the expression is known to be correct,
        /// and (b) once compiled, it is only executed once against a document of modest size.
        /// </summary>
        /// <remarks>
        /// Set to true to request fast compilation; set to false to revert to the optimization options
        /// defined in the Configuration.
        /// </remarks>

        public bool FastCompilation {
            set => Implementation.setFastCompilation(value);
            get => Implementation.isFastCompilation();
        }

        /// <summary>
        /// Compile an expression supplied as a <c>String</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// XPathExecutable q = compiler.Compile("distinct-values(//*/node-name()");
        /// </code>
        /// </example>
        /// <param name="source">A string containing the source text of the XPath expression</param>
        /// <returns>An <c>XPathExecutable</c> which represents the compiled XPath expression object.
        /// The <c>XPathExecutable</c> may be run as many times as required, in the same or a different
        /// thread. The <c>XPathExecutable</c> is not affected by any changes made to the <c>XPathCompiler</c>
        /// once it has been compiled.</returns>
        /// <exception cref="SaxonApiException">
        /// Throws a <c>Saxon.Api.SaxonApiException</c> if there is any static error in the expression
        /// (for example, a syntax error, or a reference to an undeclared function or variable).
        /// </exception>
        public XPathExecutable Compile(string source) {
            Saxon.Hej.s9api.XPathExecutable executable = Implementation.compile(source);
            return new XPathExecutable(executable);
        }

        /// <summary>
        /// Compile and execute an expression supplied as a <c>String</c>, with a given context item.
        /// </summary>
        /// <param name="expression">A string containing the source text of the XPath expression</param>
        /// <param name="contextItem">The context item to be used for evaluation of the XPath expression.
        /// May be null, in which case the expression is evaluated without any context item.</param>
        /// <returns>An <c>XdmValue</c> which is the result of evaluating the XPath expression.</returns>
        /// <exception cref="SaxonApiException">
        /// Throws a <c>Saxon.Api.SaxonApiException</c> if there is any static error in the expression
        /// (for example, a syntax error, or a reference to an undeclared function or variable),
        /// or if a dynamic error during its evaluation (for example, referring to the context item
        /// if no context item was supplied.)
        /// </exception>
        public XdmValue Evaluate(string expression, XdmItem contextItem) {
            return Implementation.evaluate(expression, contextItem);
        }

        /// <summary>
        /// Compile and execute an expression supplied as a <c>String</c>, with a given context item, where
        /// the expression is expected to return a single item as its result.
        /// </summary>
        /// <param name="expression">A string containing the source text of the XPath expression</param>
        /// <param name="contextItem">The context item to be used for evaluation of the XPath expression.
        /// May be null, in which case the expression is evaluated without any context item.</param>
        /// <returns>If the XPath expression returns a singleton, then the the <c>XdmItem</c> 
        /// which is the result of evaluating the XPath expression. If the expression returns an empty sequence,
        /// then null. If the expression returns a sequence containing more than one item, then the first
        /// item in the result.</returns>
        /// <exception cref="SaxonApiException">
        /// Throws a <c>Saxon.Api.SaxonApiException</c> if there is any static error in the expression
        /// (for example, a syntax error, or a reference to an undeclared function or variable),
        /// or if a dynamic error during its evaluation (for example, referring to the context item
        /// if no context item was supplied.)
        /// </exception>
        public XdmItem EvaluateSingle(String expression, XdmItem contextItem) {
            return Implementation.evaluateSingle(expression, contextItem);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited