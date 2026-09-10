using System;
using System.Collections;
using System.Collections.Generic;
using Saxon.Hej.expr;
using Saxon.Hej.sxpath;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XPathExecutable</c> represents the compiled form of an XPath expression. 
    /// To evaluate the expression, it must first be loaded to form an <c>XPathSelector</c>.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XPathExecutable</c> is immutable, and therefore thread-safe. It is simplest to
    /// load a new <c>XPathSelector</c> each time the expression is to be evaluated. However, the 
    /// <c>XPathSelector</c> is serially reusable within a single thread.</para>
    /// <para>An <c>XPathExecutable</c> is created by using the <c>Compile</c>
    /// method on the <c>XPathCompiler</c> class.</para>
    /// </remarks>    
    [Serializable]
    public class XPathExecutable {
        private Saxon.Hej.s9api.XPathExecutable executable;

        // internal constructor

        internal XPathExecutable(Saxon.Hej.s9api.XPathExecutable executable) {
            this.executable = executable;
        }

        /// <summary>
        /// Get a list of external variables used by the expression. This will include both variables that were explicitly
        /// declared to the <c>XPathCompiler</c>, and (if the <c>AllowUndeclaredVariables</c> option was set) variables that
        /// are referenced within the expression but not explicitly declared.
        /// </summary>
        /// <returns>
        /// An <c>IEnumerator</c> over the names of the external variables, as instances of <c>QName</c>.
        /// </returns>
        public IEnumerator EnumerateExternalVariables() {
            return executable.iterateExternalVariables();
        }

        /// <summary>
        /// Escape hatch to the <c>Saxon.Hej.s9api.XPathExecutable</c> object in the underlying Java implementation
        /// </summary>

        private Saxon.Hej.s9api.XPathExecutable Implementation => executable;

        /// <summary>
        /// Get a list of external variables used by the expression. This will include both variables that were explicitly
        /// declared to the <c>XPathCompiler</c>, and (if the <c>AllowUndeclaredVariables</c> option was set) variables that
        /// are referenced within the expression but not explicitly declared.
        /// </summary>
        /// <returns>
        /// A collection holding the names of the external variables, as instances of <c>QName</c>.
        /// </returns>

        public IEnumerable<QName> ExternalVariables {
            get {
                List<QName> names = new ();
                var iter = executable.iterateExternalVariables();
                while (iter.MoveNext()) {
                    names.Add(iter.Current);
                }
                return names;
            }
        }


        /// <summary>
        /// Get the required cardinality of a declared variable in the static context of the expression.
        /// </summary>
        /// <remarks>
        /// <para>The result is given as an occurrence indicator, one of:</para>
        /// <list>
        /// <item>'?' (zero-or-one)</item> 
        /// <item>'*' (zero-or-more)</item>
        /// <item>'+' (one-or-more)</item>
        /// <item>' ' (a single space) (exactly one)</item> 
        /// <item>'º' (masculine ordinal indicator, xBA) (exactly zero)</item>
        /// </list>
        /// <para>The type <c>empty-sequence()</c> can be represented by an occurrence indicator of 'º' with 
        /// any item type.</para>
        /// <para>If the variable was explicitly declared, this will be the occurrence indicator that was set when the
        /// variable was declared. If no item type was set, it will be "*".</para>
        /// <para>If the variable was implicitly declared by reference (which can happen only when the
        /// <c>allowUndeclaredVariables</c> option is set), the returned type will be "*".</para>
        /// <para>If no variable with the specified <c>QName</c> has been declared either explicitly or implicitly,
        /// the method returns '0'.</para>
        /// </remarks>
        /// <param name="variableName">the name of a declared variable</param>
        /// <returns>The required cardinality, in the form of an occurrence indicator.</returns>
        public char GetRequiredCardinalityForVariable(QName variableName) {
            XPathVariable var =
                ((IndependentContext) executable.getUnderlyingStaticContext()).getExternalVariable(
                    variableName.GetStructuredQName());
            if (var == null) {
                return '0';
            }
            else {
                return GetOccurrenceIndicator(var.getRequiredType().getCardinality());
            }
        }


        //internal method

        internal char GetOccurrenceIndicator(int occurrenceIndicator) {
            switch (occurrenceIndicator) {
                case StaticProperty.ALLOWS_ZERO_OR_MORE:

                    return XdmSequenceType.ZERO_OR_MORE;

                case StaticProperty.ALLOWS_ONE_OR_MORE:

                    return XdmSequenceType.ONE_OR_MORE;

                case StaticProperty.ALLOWS_ZERO_OR_ONE:

                    return XdmSequenceType.ZERO_OR_ONE;

                case StaticProperty.EXACTLY_ONE:

                    return XdmSequenceType.ONE;

                case StaticProperty.ALLOWS_ZERO:

                    return XdmSequenceType.ZERO;

                default:
                    throw new ArgumentException("Unknown occurrence indicator");
            }
        }


        /// <summary>
        /// Load the compiled XPath expression to prepare it for execution.
        /// </summary>
        /// <returns>
        /// An <c>XPathSelector</c>. The returned <c>XPathSelector</c> can be used to
        /// set up the dynamic context, and then to evaluate the expression.
        /// </returns>
        public XPathSelector Load() {
            Saxon.Hej.s9api.XPathSelector selector = executable.load();
            return new XPathSelector(selector);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited