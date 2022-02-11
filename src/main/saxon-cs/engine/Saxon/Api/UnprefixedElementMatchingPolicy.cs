using System;

namespace Saxon.Api {
    /// <summary>
    /// An enumeration defining possible strategies for resolving unprefixed element names appearing
    /// as name tests in the steps of a path expression or XSLT match pattern
    /// </summary>
    [Serializable]
    public enum UnprefixedElementMatchingPolicy {
        
        /// <summary>
        /// The standard W3C policy, whereby element names are implicitly qualified by the
        /// default namespace for elements and types, as defined in the XPath static context.
        /// In XSLT this can be set using the <code>[xsl:]xpath-default-namespace</code>
        /// attribute, or programmatically by binding the namespace prefix "".
        /// </summary>
        
        DefaultNamespace,
    
        /// <summary>
        /// Under this policy, unprefixed element names match on the local part only; an element
        /// with this local name is matched regardless of its namespace (that is, it can have any
        /// namespace, or none)
        /// </summary>

        AnyNamespace,
        
        /// <summary>
        /// Under this policy, unprefixed element names match provided that (a) the local part of
        /// the name matches, and (b) the namespace part of the name is either equal to the default
        /// namespace for elements and types, or is absent.
        /// </summary>
        /// <remarks>
        /// This policy is provided primarily for use with HTML, where it can be unpredictable
        /// whether HTML elements are in the XHTML namespace or in no namespace. It is also useful
        /// with other vocabularies where instances are sometimes in a namespace and sometimes not.
        /// The policy approximates to the special rules defined in the HTML5 specification, which states
        /// that unprefixed names are treated as matching names in the XHTML namespace if the context
        /// item for a step is "a node in an HTML DOM", and as matching no-namespace names otherwise;
        /// since in the XDM data model it is not possible to make a distinction between different kinds
        /// of DOM, this policy allows use of unprefixed names both when matching elements in the XHTML
        /// namespace and when matching no-namespace elements
        /// </remarks>
        
        DefaultNamespaceOrNone
    }
}

// Copyright (c) 2021 Saxonica Limited