using System;
using Saxon.Ejava.util.function;
using Saxon.Hej;
using Saxon.Hej.expr;
using Saxon.Hej.expr.parser;
using Saxon.Hej.om;
using Saxon.Hej.type;
using Saxon.Hej.type.coercion;
using Saxon.Hej.value;

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
        internal XdmFunctionItem(Saxon.Hej.om.FunctionItem function) : base(function) {
        }

        /// <summary>
        /// The name of the function, as a QName
        /// </summary>
        /// <returns>The name of the function. The result will be null if the function is anonymous.</returns>

        public QName FunctionName {
            get {
                StructuredQName sqName = ((FunctionItem) value).getFunctionName();
                return (sqName == null ? null : new QName(sqName));
            }
        }

        /// <summary>
        /// The arity of the function, that is, the number of arguments it expects
        /// </summary>
        /// <returns>The number of arguments that the function takes</returns>

        public int Arity => ((FunctionItem) value).getArity();

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
            try {
                TypeHierarchy th = processor.Implementation.getTypeHierarchy();
                CoercionRules coercionRules = CoercionRules.forVersion(processor.Implementation, 31);
                FunctionItem fi = (FunctionItem)value;
                FunctionItemType type = fi.getFunctionItemType();
                Sequence[] args = new Sequence[arguments.Length];
                for (int i = 0; i < arguments.Length; i++) {
                    //args[i] = arguments[i].value;
                    Hej.value.SequenceType required = type.getArgumentTypes()[i];
                    GroundedValue val = arguments[i].value;
                    if (!required.matches(val, th)) {
                        int pos = i;
                        Supplier<RoleDiagnostic> role =
                            () => new RoleDiagnostic(RoleDiagnostic.FUNCTION, "", pos);
                        val = coercionRules.coerce(val, required, SequenceType.ANY_SEQUENCE, 
                                    processor.Implementation, role, Loc.NONE).materialize();
                    }
                    args[i] = val;
                }

                Configuration config = processor.Implementation;
                Controller controller = new Controller(config);
                XPathContext context = controller.newXPathContext();
                context = fi.makeNewContext(context, controller);

                Sequence result = fi.call(context, args);
                GroundedValue groundedResult = result.materialize();
                if (!fi.isTrustedResultType()) {
                    Hej.value.SequenceType required = type.getResultType();
                    if (!required.matches(groundedResult, th)) {
                        Supplier<RoleDiagnostic> role =
                            () => new RoleDiagnostic(RoleDiagnostic.FUNCTION_RESULT, "", 0);
                        groundedResult = coercionRules.coerce(groundedResult, required, 
                            SequenceType.ANY_SEQUENCE, processor.Implementation, role, Loc.NONE).materialize();
                    }
                }
                return Wrap(groundedResult);
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited