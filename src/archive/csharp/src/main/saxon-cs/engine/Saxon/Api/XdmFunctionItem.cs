using System;
using Saxon.Hej.expr;
using Saxon.Hej.om;

namespace Saxon.Api {
    /// <summary inherits="XdmItem">
    /// The class <c>XdmFunctionItem</c> represents an item in an XDM sequence
    /// that holds a function.
    /// </summary>
    /// <remarks>
    /// <para>Note that there is no guarantee that every <c>XdmValue</c> comprising a single
    /// function item will be an instance of this class. To force this, use the <c>Simplify</c>
    /// property of the <c>XdmValue</c>.</para>
    /// <para>There are two ways of creating an instance of this class: either evaluate
    /// an XPath or XQuery or XSLT expression that returns a function item, or use the
    /// method <c>Processor.GetSystemFunction</c> to obtain a built-in system function.</para>
    /// <para>Arrays and Maps are functions, so the types <c>XdmArray</c> and <c>XdmMap</c>
    /// extend <c>XdmFunctionItem</c>.</para>
    /// </remarks>
    [Serializable]
    public class XdmFunctionItem : XdmItem {

        // Internal constructor: create an XdmFunctionItem by wrapping an internal function object
        internal XdmFunctionItem(Saxon.Hej.om.Function function) : base(function) {
        }

        /// <summary>
        /// The name of the function, as a QName
        /// </summary>
        /// <returns>The name of the function. The result will be null if the function is anonymous.</returns>

        public QName FunctionName {
            get {
                StructuredQName sqName = ((Function) value).getFunctionName();
                return (sqName == null ? null : new QName(sqName));
            }
        }

        /// <summary>
        /// The arity of the function, that is, the number of arguments it expects
        /// </summary>
        /// <returns>The number of arguments that the function takes</returns>

        public int Arity => ((Function) value).getArity();

        /// <summary>
        /// Determine whether the item is an atomic value
        /// </summary>
        /// <returns>
        /// false (a function item is not an atomic value)
        /// </returns>
        public override bool IsAtomic() {
            return false;
        }


        /// <summary>
        /// Invoke the function
        /// </summary>
        /// <param name="arguments">The arguments to the function</param>
        /// <param name="processor">The Saxon processor, used to provide context information</param>
        /// <returns>The result of calling the function</returns>
        /// 
        public XdmValue Invoke(XdmValue[] arguments, Processor processor) {
            Sequence[] args = new Sequence[arguments.Length];
            for (int i = 0; i < arguments.Length; i++) {
                args[i] = arguments[i].value;
            }

            Function function = (Function) value;
            Saxon.Hej.Configuration config = processor.Implementation;
            Saxon.Hej.Controller controller = new Saxon.Hej.Controller(config);
            XPathContext context = controller.newXPathContext();
            context = function.makeNewContext(context, controller);

            Sequence result = function.call(context, args);
            GroundedValue groundedResult = result.materialize();
            // TODO: check the result type
            // if (!function.isTrustedResultType()) {
            //     Saxon.Hej.value.SequenceType required = type.getResultType();
            //     if (!required.matches(groundedResult, th)) {
            //         RoleDiagnostic role = new RoleDiagnostic(RoleDiagnostic.FUNCTION_RESULT, "", 0);
            //         groundedResult = th.applyFunctionConversionRules(groundedResult, required, role, Loc.NONE);
            //     }
            // }
            return Wrap(groundedResult);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited