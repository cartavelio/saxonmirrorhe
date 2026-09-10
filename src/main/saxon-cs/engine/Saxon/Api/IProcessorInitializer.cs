namespace Saxon.Api {
    /// <summary>
    /// An interface that can be implemented by the user to supply a callback for use in the -init
    /// option on the command line.
    /// </summary>


    public interface IProcessorInitializer {

        /// <summary>
        /// This method is called to allow the user-written callback implementation to initialize
        /// the configuration of a Saxon Processor
        /// </summary>
        /// <param name="processor">The Saxon Processor. The method should only attempt to set
        /// configuration properties on the processor; it should not attempt to use it for
        /// activities such as executing queries and transformations.</param>

        void Initialize(Processor processor);
 
    }
}