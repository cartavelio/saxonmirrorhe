using System;
using java.util;
using net.sf.saxon.value;



namespace Saxon.Api.Support {
    
    ///<summary>
    ///  This is an implementation of the Saxon.Api.ExtensionFunction interface that
    ///  accepts the function implementation in the form of a lambda expression (that is,
    ///  a delegate). It allows simple extension functions to be registered with the
    ///  Saxon Processor using a call of the form <code>processor.RegisterExtensionFunction(QName,
    ///  arity, args => new XdmAtomicValue((XdmAtomicValue)args[0]).GetDoubleValue() + 1)</code>
    ///</summary>
    /// <remarks>
    /// <para>The extension function has an implicit signature of <code>(item()*, item()*, ...) => item()*</code>.
    /// This means there is no type checking performed by the system, and no implicit type conversions
    /// such as atomization.
    /// </para>
    /// </remarks>
    public class LambdaExtensionFunction : ExtensionFunction {

        private readonly QName name;
        private readonly int arity;
        private readonly Func<XdmValue[], XdmValue> implementation;
        
        public LambdaExtensionFunction(QName name, int arity, Func<XdmValue[], XdmValue> implementation) {
            this.name = name;
            this.arity = arity;
            this.implementation = implementation;
        }

        public QName GetName() {
            return name;
        }

        public XdmSequenceType GetResultType() {
            return XdmSequenceType.FromSequenceType(SequenceType.ANY_SEQUENCE);
        }

        public XdmSequenceType[] GetArgumentTypes() {
            XdmSequenceType[] result = new XdmSequenceType[arity];
            Arrays.fill(result, XdmSequenceType.FromSequenceType(SequenceType.ANY_SEQUENCE));
            return result;
        }

        public XdmValue Call(XdmValue[] arguments) {
            return implementation(arguments);
        }
    }
}