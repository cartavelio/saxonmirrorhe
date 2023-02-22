using System;
using net.sf.saxon.s9api;
using net.sf.saxon.value;
using net.sf.saxon.om;

namespace Saxon.Api {
    /// <summary>
    /// <c>WhitespacePolicy</c> is a class defining the possible policies for handling
    /// whitespace text nodes in a source document.
    /// </summary>
    [Serializable]
    public class WhitespacePolicy {
        private int policy;
        SpaceStrippingRule stripperRules;

        /// <summary>All whitespace text nodes are stripped</summary>
        public static WhitespacePolicy StripAll = 
            new WhitespacePolicy(Whitespace.ALL, AllElementsSpaceStrippingRule.getInstance());

        /// <summary>Whitespace text nodes appearing in element-only content are stripped</summary>
        public static WhitespacePolicy StripIgnorable =
            new WhitespacePolicy(Whitespace.IGNORABLE, IgnorableSpaceStrippingRule.getInstance());

        /// <summary>No whitespace is stripped</summary>
        public static WhitespacePolicy PreserveAll =
            new WhitespacePolicy(Whitespace.NONE, NoElementsSpaceStrippingRule.getInstance());

        /// <summary>Unspecified means that no other value has been specifically requested</summary>
        public static WhitespacePolicy Unspecified = 
            new WhitespacePolicy(Whitespace.UNSPECIFIED, null);

        private WhitespacePolicy(int policy, SpaceStrippingRule stripperRules) {
            this.policy = policy;
            this.stripperRules = stripperRules;
        }

        internal WhitespacePolicy(net.sf.saxon.s9api.XsltExecutable executable) {
            this.policy = Whitespace.XSLT;
            this.stripperRules = executable.getWhitespaceStrippingPolicy();
        }

        /// <summary>
        /// Create a custom whitespace stripping policy
        /// </summary>
        /// <param name="elementTest">a predicate applied to element names, which should return true if whitespace-only
        /// text node children of the element are to be stripped, false if they are to be retained.</param>
        /// <returns>A <c>WhitespacePolicy</c> object</returns>
        public static WhitespacePolicy MakeCustomPolicy(System.Predicate<QName> elementTest) {
            WhitespaceStrippingPolicy policy =
                WhitespaceStrippingPolicy.makeCustomPolicy(new JPredicateImpl(elementTest));
            WhitespacePolicy wsp = new WhitespacePolicy(Whitespace.XSLT, null);
            wsp.strippingPolicy = policy;
            return wsp;
        }

        internal int ordinal() {
            return policy;
        }

        /*internal net.sf.saxon.@event.FilterFactory makeStripper() {
            return new FilterFactory(strippingPolicy);
        }      */
    }
}