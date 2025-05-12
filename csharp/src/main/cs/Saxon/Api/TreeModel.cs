namespace Saxon.Api {
    /// <summary>
    /// Enumeration identifying the different tree model implementations
    /// </summary>
    /// 
    public enum TreeModel
    {
        /// <summary>
        /// Saxon <c>LinkedTree</c>. This tree model is primarily useful when using XQuery Update, since it is the
        /// only standard tree model to support updates.
        /// </summary>
        LinkedTree,
        /// <summary>
        /// Saxon <c>TinyTree</c>. This is the default model and is suitable for most purposes.
        /// </summary>
        TinyTree,
        /// <summary>
        /// Saxon Condensed <c>TinyTree</c>. This is a variant of the <c>TinyTree</c> that shares storage for 
        /// duplicated text and attribute nodes. It gives a further saving in space occupied, at the cost
        /// of some increase in the time taken for tree construction.
        /// </summary>
        TinyTreeCondensed,
        /// <summary>
        /// Unspecified tree model. This value is used to indicate that there is no preference for any specific
        /// tree model, which allows the choice to fall back to other interfaces.
        /// </summary>
        Unspecified = -1
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited