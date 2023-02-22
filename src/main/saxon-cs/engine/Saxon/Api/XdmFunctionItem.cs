using System;
using net.sf.saxon.expr;
using net.sf.saxon.om;

namespace Saxon.Api {
    /// <summary inherits="XdmItem">
    /// The class <c>XdmFunctionItem</c> represents an item in an XDM sequence
    /// that holds a function.
    /// </summary>
    /// <remarks>
    /// <para>Note that there is no guarantee that every <c>XdmValue</c> comprising a single
    /// function item will be an instance of this class. To force this, use the <c>Simplify</c>
    /// property of the <c>XdmValue</c>.</para>
    /// <para>At present the only way of creating an instance of this class is as the result of
    /// an XPath or XQuery expression that returns a function item.</para>
    /// </remarks>
    [Serializable]
    public class XdmFunctionItem : XdmItem {
        protected XdmFunctionItem() {
        }

        public XdmFunctionItem(Function function) : base(function) {
        }

        /// <summary>
        /// The name of the function, as a QName
        /// </summary>
        /// <returns>The name of the function. The result will be null if the function is anonymous.</returns>

        public QName FunctionName {
            get {
                StructuredQName sqName = ((Function) value).getFunctionName();
                return (sqName == null ? null : QName.FromStructuredQName(sqName));
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
            net.sf.saxon.Configuration config = processor.Implementation;
            net.sf.saxon.Controller controller = new net.sf.saxon.Controller(config);
            XPathContext context = controller.newXPathContext();
            context = function.makeNewContext(context, controller);

            Sequence result = function.call(context, args);
            GroundedValue groundedResult = result.materialize();
            // TODO: check the result type
            // if (!function.isTrustedResultType()) {
            //     net.sf.saxon.value.SequenceType required = type.getResultType();
            //     if (!required.matches(groundedResult, th)) {
            //         RoleDiagnostic role = new RoleDiagnostic(RoleDiagnostic.FUNCTION_RESULT, "", 0);
            //         groundedResult = th.applyFunctionConversionRules(groundedResult, required, role, Loc.NONE);
            //     }
            // }
            return XdmValue.Wrap(result);
        }
    }
}