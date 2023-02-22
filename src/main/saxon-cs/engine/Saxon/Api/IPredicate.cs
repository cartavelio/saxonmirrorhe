using System;

namespace Saxon.Api {
    /// <summary>
    /// Interface that represents a predicate (boolean-valued <c>Func</c>) of one argument.
    /// </summary>
    /// <remarks>This is a functional interface whose functional method is <c>Invoke(object)</c>.</remarks>
    /// <typeparam name="XdmItem">The type of the input to the predicate.</typeparam>
    public interface IPredicate<in XdmItem>
    {

        /// <summary>
        /// Returns a composed predicate that represents a short-circuiting logical
        /// OR of this predicate and another. When evaluating the composed
        /// predicate, if this predicate is <c>true</c>, then the <c>other</c>
        /// predicate is not evaluated.
        /// </summary>
        /// <typeparam name="T">The type of the input argument</typeparam>
        /// <param name="other">A predicate that will be logically-ORed with this predicate</param>
        /// <returns>A composed predicate that represents the short-circuiting logical OR of this predicate and the <c>other</c>
        /// predicate.</returns>
        /**public**/ IPredicate<T> Or<T>(IPredicate<T> other) where T : XdmItem;

        /// <summary>
        /// Returns a composed predicate that represents a short-circuiting logical
        /// AND of this predicate and another. When evaluating the composed
        /// predicate, if this predicate is <c>false</c>, then the <c>other</c>
        /// predicate is not evaluated.
        /// </summary>
        /// <typeparam name="T">The type of the input argument</typeparam>
        /// <param name="other">A predicate that will be logically-ANDed with this predicate</param>
        /// <returns>A composed predicate that represents the short-circuiting logical AND of this predicate the <c>other</c>
        /// predicate.</returns>
        /**public**/ IPredicate<T> And<T>(IPredicate<T> other) where T : XdmItem;

        /// <summary>
        /// Returns a predicate that represents the logical negation of this predicate.
        /// </summary>
        /// <returns>A predicate that represents the logical negation of this predicate.</returns>
        /**public**/ IPredicate<XdmItem> Negate();

        /// <summary>
        /// Evaluates this predicate on the given argument.
        /// </summary>
        /// <typeparam name="T">The type of the input argument</typeparam>
        /// <param name="item">The input item</param>
        /// <returns><c>true</c> if the input argument matches the predicate, otherwise <c>false</c>.</returns>
        /**public**/ bool Invoke<T>(T item) where T : XdmItem;

        /// <summary>
        /// Unwrapped Func property which evaluates to boolean.
        /// </summary>
        /**public**/ Func<XdmItem, bool> Func { get; }
    }
}