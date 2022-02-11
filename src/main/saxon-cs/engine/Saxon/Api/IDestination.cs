
namespace Saxon.Api {
    /// <summary>
    /// An abstract destination for the results of a query or transformation
    /// </summary>
    /// <remarks>
    /// </remarks>


    public interface IDestination
    {

        /// <summary>
        /// The underlying <c>Saxon.Hej.s9api.Destination</c> object in the Saxon implementation
        /// </summary>
        /// <remarks>
        /// <para>This method must be implemented in any class that implements this interface,
        /// that is, a class that provides a custom destination not based on those supplied
        /// in the Saxon-CS product.</para> 
        /// <para>Implementing this method requires familiarity with Saxon internals (which may
        /// change from one release to another.</para>
        /// <para>Information about the returned <see cref="Saxon.Hej.s9api.Destination"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc documentation.
        /// </para>
        /// </remarks>
        /// <returns>returns the underlying Destination object</returns>
		/**public**/ Saxon.Hej.s9api.Destination GetUnderlyingDestination();

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
		/**public**/ void OnClose(System.Action action);

    }
}

// Copyright (c) 2021 Saxonica Limited