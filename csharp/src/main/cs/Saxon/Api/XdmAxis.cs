namespace Saxon.Api {
    /// <summary>
    /// Enumeration identifying the thirteen XPath axes
    /// </summary>

    public enum XdmAxis
    {
        /// <summary>The XPath ancestor axis</summary> 
        Ancestor,
        /// <summary>The XPath ancestor-or-self axis</summary> 
        AncestorOrSelf,
        /// <summary>The XPath attribute axis</summary> 
        Attribute,
        /// <summary>The XPath child axis</summary> 
        Child,
        /// <summary>The XPath descendant axis</summary> 
        Descendant,
        /// <summary>The XPath descandant-or-self axis</summary> 
        DescendantOrSelf,
        /// <summary>The XPath following axis</summary> 
        Following,
        /// <summary>The XPath following-sibling axis</summary> 
        FollowingSibling,
        /// <summary>The XPath namespace axis</summary> 
        Namespace,
        /// <summary>The XPath parent axis</summary> 
        Parent,
        /// <summary>The XPath preceding axis</summary> 
        Preceding,
        /// <summary>The XPath preceding-sibling axis</summary> 
        PrecedingSibling,
        /// <summary>The XPath self axis</summary> 
        Self
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited