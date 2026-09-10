using System;
using System.Collections.Generic;
using System.Linq;

namespace Saxon.Api {
    /// <summary>
    /// A <c>Step</c> class wraps a delegate method that can be applied to an item
    /// to return a <c>XdmValue</c> of items.
    /// </summary>
    public class Step<TInput, TResult>
        where TInput : XdmItem
        where TResult : XdmItem {
        private readonly Func<TInput, IEnumerable<TResult>> function;

        /// <summary>
        /// Constructor method to wrap a delegate method.
        /// </summary>
        /// <param name="steppingFunction">Passes a delegate as a <c>Func</c> with encapsulated type <c>XdmItem</c> and the return value <c>IEnumerable</c> of items.</param>
        public Step(Func<TInput, IEnumerable<TResult>> steppingFunction) {
            function = steppingFunction;
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the results of this <c>Step</c> using a supplied <c>Func</c> predicate.
        /// <p>For example, <c>Child.Where(Predicate.IsText())</c>
        /// returns a <c>Step</c> whose effect is to select the text node children 
        /// of a supplied element or document node.</p>
        /// </summary>
        /// <param name="predicate">The predicate is a <c>Func</c> delegate encapsulating the filter which will be applied to the results of this <c>Step</c></param>
        /// <returns>A new <c>Step</c> (that is, a wrapped delegate from one <c>Step</c> of items to another) that
        /// filters the results of this step by selecting only the items that satisfy the predicate.</returns>

        public Step<TInput, TResult> Where(System.Predicate<TResult> predicate) {
            return new Step<TInput, TResult>(item => function.Invoke(item).Where(predicate.Invoke));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that combines the results of this step with the results of another step.
        /// </summary>
        /// <param name="next">The step which will be applied to the results of this step</param>
        /// <returns>A new <c>Step</c> (that is, a function from one <c>IEnumerable</c> of items to another) that
        /// performs this step and the next step in turn. The result is equivalent to the <c>IEnumerable</c> method <c>SelectMany()</c>
        /// function or the XPath <c>!</c> operator: there is no sorting of nodes into document order, and
        /// no elimination of duplicates.</returns>
        public Step<TInput, TResult> Then(Step<TResult, TResult> next) {
            return new Step<TInput, TResult>(item => function.Invoke(item).SelectMany(next.Func));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that selects the Nth item in the results of this step.
        /// </summary>
        /// <param name="index">The zero-based index of the item to be selected</param>
        /// <returns>A new <c>Step</c> (that is, a function from one <c>IEnumerable</c> of items to another) that
        /// filters the results of this step by selecting only the items that satisfy the predicate.</returns>
        public Step<TInput, TResult> At(int index) {
            return new Step<TInput, TResult>(item => new List<TResult>(1) {
                function.Invoke(item).ElementAt(index)
            });
        }


        /// <summary>
        /// Obtain a <c>Step</c> that concatenates the results of this <c>Step</c> with the result of another
        /// <c>Step</c> applied to the same input item.
        /// </summary>
        /// <remarks><p>For example, <c>Attribute().Cat(Child())</c> returns a step whose effect is
        /// to select the attributes of a supplied element followed by its children.</p></remarks>
        /// <param name="other">The step whose results will be concatenated with the results of this step</param>
        /// <returns>A new <c>Step</c> (that is, a function from one <c>IEnumerable</c> of items to another) that
        /// concatenates the results of applying this step to the input item, followed by the
        /// results of applying the other step to the input item.</returns>
        public Step<TInput, TResult> Cat(Step<TInput, TResult> other) {
            return new Step<TInput, TResult>(item => { return function.Invoke(item).Concat(other.Invoke(item)); });
        }


        /// <summary>
        /// The <c>Func</c> property that represents the wrapped delegate method which can be invoked.
        /// </summary>
        public Func<TInput, IEnumerable<TResult>> Func => function;


        /// <summary>
        /// Invokes this function to the given argument.
        /// </summary>
        /// <param name="item">The function argument</param>
        /// <returns>The function result.</returns>
        public IEnumerable<TResult> Invoke(TInput item) {
            return function.Invoke(item);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited