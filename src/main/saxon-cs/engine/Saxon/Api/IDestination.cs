using Saxon.Hej.s9api;

namespace Saxon.Api {
    /// <summary>
    /// An abstract destination for the results of a query or transformation
    /// </summary>
    /// <remarks>
    /// </remarks>


    public interface IDestination
    {

        /// <summary>
        /// The underlying <c>Destination</c> object in the Saxon implementation
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned <see cref="Saxon.Hej.Configuration"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc docmentation.
        /// </para>
        /// </remarks>
        /// <returns>returns the underlying Destination object</returns>
        Destination GetUnderlyingDestination();

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        void OnClose(System.Action action);

    }
}