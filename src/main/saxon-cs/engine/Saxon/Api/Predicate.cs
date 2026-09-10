using System;

namespace Saxon.Api {
    /// <summary>
    /// This class implements the <c>IPredicate</c> interface which represents a predicate (boolean-valued <c>Func</c>) of one argument.
    /// </summary>
    /// <typeparam name="T">The type of the input to the predicate, which must be of type <c>XdmItem</c> or one of its subclasses.</typeparam>
    public class Predicate<T> : IPredicate<T>
        where T : XdmItem
    {

        private Func<T, bool> function;

        /// <summary>
        /// Predicate constructor method to wrap the <c>Func</c> object.
        /// </summary>
        /// <param name="f">boolean-valued <c>Func</c> object</param>
        public Predicate(Func<T, bool> f)
        {
            function = f;
        }

        /// <summary>
        /// Evaluates this predicate on the given argument.
        /// </summary>
        /// <typeparam name="T1">The type of the input argument</typeparam>
        /// <param name="item">The input item</param>
        /// <returns><c>true</c> if the input argument matches the predicate, otherwise <c>false</c>.</returns>
        public bool Invoke<T1>(T1 item) where T1 : T
        {
            return function(item);
        }


        /// <summary>
        /// Returns a compose predicate that represents a short-circuiting logical
        /// AND of this predicate and another. When evaluating the composed predicate,
        /// if this predicate is <c>false</c>, then the other predicate is not evaluated.
        /// </summary>
        /// <param name="other">A predicate that will be logically-ANDed with this predicate</param>
        /// <returns>A composed predicate that represents the short-circuiting logical 
        /// AND of this predicate and the <c>other</c> predicate.</returns>
        /**public**/ IPredicate<T1> IPredicate<T>.And<T1>(IPredicate<T1> other)
        {
            return new Predicate<T1>(item => function(item) && other.Func(item));
        }

        /// <summary>
        /// Returns a composed predicate that represents a short-circuiting logical
        /// OR of this predicate and another. When evaluating the composed
        /// predicate, if this predicate is <c>true</c>, then the <c>other</c>
        /// predicate is not evaluated.
        /// </summary>
        /// <param name="other">A predicate that will be logically-ORed with this predicate.</param>
        /// <returns>A composed predicate that represents the short-circuiting logical
        /// OR of this predicate and the <c>other</c> predicate.</returns>
        /**public**/ IPredicate<T1> IPredicate<T>.Or<T1>(IPredicate<T1> other)
        {
            return new Predicate<T1>(item => function(item) || other.Func(item));
        }

        /// <summary>
        /// Return a predicate that represents the logical negation of this predicate.
        /// </summary>
        /// <returns>A predicate that represents the logical negation of this predicate.</returns>
        /**public**/ IPredicate<T> IPredicate<T>.Negate()
        {
            return new Predicate<T>(item => !function(item));
        }


        /// <summary>
        /// The <c>Func</c> Property represents the wrapped delegate method which can be invoked.
        /// </summary>
        public Func<T, bool> Func
        {
            get
            {
                return function;
            }
        }

    }
}