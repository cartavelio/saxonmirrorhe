using System;
using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary>
    /// <para>Abstract superclass for user-written extension functions. An extension function may be implemented as a subclass
    /// of this class, with appropriate implementations of the defined methods.</para>
    /// <para>More precisely, a subclass of <c>ExtensionFunctionDefinition</c> identifies a family of extension functions
    /// with the same (namespace-qualified) name but potentially having different arity (number of arguments).</para>
    /// </summary>
    /// <remarks>
    /// <para>A user-defined extension function is typically implemented using a pair of classes: a class that extends 
    /// <code>ExtensionFunctionDefinition</code>, whose purpose is to define the properties of the extension function
    /// (in particular, its signature -- the types of its arguments and result); and a class that extends
    /// <code>ExtensionFunctionCall</code>, whose purpose is to perform the actual evaluation.</para> 
    /// <para>The <code>ExtensionFunctionDefinition</code> is immutable and will normally have a singleton instance
    /// for each subclass; this singleton instance is registered with the <code>Processor</code> to associate the
    /// name of the extension function with its definition.</para>
    /// <para>The <code>ExtensionFunctionCall</code> has one instance for each call on the extension function appearing
    /// in the source code of a stylesheet or query; this instance is created when Saxon calls the method <code>MakeFunctionCall</code>
    /// provided by the <code>ExtensionFunctionDefinition</code> object. The instance of <code>ExtensionFunctionCall</code>
    /// holds information about the static context of the function call, and its <code>Call</code> method is called
    /// (by Saxon) to evaluate the extension function at run-time.</para>
    /// </remarks>

    public abstract class ExtensionFunctionDefinition
    {
        /// <summary>
        /// Read-only property returning the name of the extension function, as a <c>QName</c>.
        /// </summary>
        /// <remarks>
        /// A getter for this property must be implemented in every subclass.
        /// </remarks>

        public abstract QName FunctionName { get; }

        /// <summary>
        /// Read-only property giving the minimum number of arguments in a call to this extension function.
        /// </summary>
        /// <remarks>
        /// A getter for this property must be implemented in every subclass.
        /// </remarks>

        public abstract int MinimumNumberOfArguments { get; }

        /// <summary>
        /// Read-only property giving the maximum number of arguments in a call to this extension function.
        /// </summary>
        /// <remarks>
        /// A getter for this property must be implemented in every subclass.
        /// </remarks>

        public abstract int MaximumNumberOfArguments { get; }

        /// <summary>
        /// Read-only property giving the required types of the arguments to this extension function. 
        /// If the number of items in the array is less than the maximum number of arguments, 
        /// then the last entry in the returned <c>ArgumentTypes</c> is assumed to apply to all the rest; 
        /// if the returned array is empty, then all arguments are assumed to be of type <c>item()*</c>
        /// </summary>
        /// <remarks>
        /// A getter for this property must be implemented in every subclass.
        /// </remarks>
        /// <returns>
        /// An array of <c>XdmSequenceType</c> objects representing the required types of the arguments 
        /// to the extension function.
        /// </returns>

        public abstract XdmSequenceType[] ArgumentTypes { get; }

        /// <summary>
        /// Method returning the declared type of the return value from the function. The type of the return
        /// value may be known more precisely if the types of the arguments are known (for example, some functions
        /// return a value that is the same type as the first argument). The method is therefore called supplying the
        /// static types of the actual arguments present in the call.
        /// </summary>
        /// <remarks>
        /// This method must be implemented in every subclass.
        /// </remarks>
        /// <param name="ArgumentTypes">
        /// The static types of the arguments present in the function call
        /// </param>
        /// <returns>
        /// An <c>XdmSequenceType</c> representing the declared return type of the extension function
        /// </returns>

        public abstract XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes);

        /// <summary>
        /// This property may return true for a subclass if it guarantees that the returned result of the function
        /// will always be of the declared return type: setting this to true by-passes the run-time checking of the type
        /// of the value, together with code that would otherwise perform atomization, numeric type promotion, and similar
        /// conversions. If the value is set to true and the value is not of the correct type, the effect is unpredictable
        /// and probably disastrous.
        /// </summary>
        /// <remarks>
        /// The default value of this property is <c>false</c>. A getter for this property may be implemented in a subclass
        /// to return a different value.
        /// </remarks>

        public virtual bool TrustResultType => false;

        /// <summary>
        /// This property must return true for a subclass if the evaluation of the function makes use of the context
        /// item, position, or size from the dynamic context. It should also return true (despite the property name)
        /// if the function makes use of parts of the static context that vary from one part of the query or stylesheet
        /// to another. Setting the property to true inhibits certain Saxon optimizations, such as extracting the call
        /// from a loop, or moving it into a global variable.
        /// </summary>
        /// <remarks>
        /// The default value of this property is <c>false</c>. A getter for this property may be implemented in a subclass
        /// to return a different value.
        /// </remarks>

        public virtual bool DependsOnFocus => false;

        /// <summary>
        /// This property should return true for a subclass if the evaluation of the function has side-effects.
        /// Saxon never guarantees the result of calling functions with side-effects, but if this property is set,
        /// then certain aggressive optimizations will be avoided, making it more likely that the function behaves
        /// as expected.
        /// </summary>
        /// <remarks>
        /// The default value of this property is <c>false</c>. A getter for this property may be implemented in a subclass
        /// to return a different value.
        /// </remarks>

        public virtual bool HasSideEffects => false;

        /// <summary>
        /// Factory method to create an <c>ExtensionFunctionCall</c> object, representing a specific function call in the XSLT or XQuery
        /// source code. Saxon will call this method once it has identified that a specific call relates to this extension
        /// function.
        /// </summary>
        /// <remarks>
        /// This method must be implemented in every subclass. The implementation should normally instantiate the relevant subclass
        /// of <code>ExtensionFunctionCall</code>, and return the new instance.
        /// </remarks>
        /// <returns>
        /// An instance of the appropriate implementation of <code>ExtensionFunctionCall</code>
        /// </returns>

        public abstract ExtensionFunctionCall MakeFunctionCall();
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited