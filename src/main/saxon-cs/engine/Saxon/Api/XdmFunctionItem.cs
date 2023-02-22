using System;
using Saxon.Hej;
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
    /// static method <c>GetSystemFunction</c> to obtain a built-in system function.</para>
    /// </remarks>
    [Serializable]
    public class XdmFunctionItem : XdmItem {
        protected XdmFunctionItem() {
        }

        public XdmFunctionItem(Function function) : base(function) {
        }
        
         /// <summary>
         /// Get a system function. This can be any function defined in XPath 3.1 functions and operators,
         /// including functions in the math, map, and array namespaces. It can also be a Saxon extension
         /// function, provided a licensed Processor is used.
         /// </summary>
         /// <param name="processor">The Saxon Processor object</param>
         /// <param name="name">The function name</param>
         /// <param name="arity">The function arity</param>
         /// <return> the requested function, or null if there is no such function. Note that some functions
         /// (those with particular context dependencies) may be unsuitable for dynamic calling.
         /// </return>
         
        public static XdmFunctionItem getSystemFunction(Processor processor, QName name, int arity) {
            Configuration config = processor.Implementation;
            Function f = config.getSystemFunction(name.GetStructuredQName(), arity);
            return f==null ? null : new XdmFunctionItem(f);
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
            return XdmValue.Wrap(result);
        }
    }
}