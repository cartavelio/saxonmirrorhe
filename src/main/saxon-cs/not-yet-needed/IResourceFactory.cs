namespace Saxon.Api {
    /// <summary>
    /// A ResourceFactory is used for constructing a particular type of resource
    /// </summary>
    public interface IResourceFactory {


        /// <summary>
        /// Create a Resource with given content
        /// </summary>
        /// <param name="proc">the Saxon Processor</param>
        /// <returns>the resource</returns>
        IResource MakeResource(Processor proc);

    }
}