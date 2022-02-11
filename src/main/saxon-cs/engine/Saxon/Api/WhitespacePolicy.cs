using System;
using Saxon.Ejava.lang;
using Saxon.Hej.@event;
using Saxon.Hej.value;
using Saxon.Hej.om;
using Saxon.Hej.trace;
using Saxon.Hej.type;

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
            new(Whitespace.ALL, AllElementsSpaceStrippingRule.getInstance());

        /// <summary>Whitespace text nodes appearing in element-only content are stripped</summary>
        public static WhitespacePolicy StripIgnorable =
            new(Whitespace.IGNORABLE, IgnorableSpaceStrippingRule.getInstance());

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

        /// <summary>
        /// Construct the whitespace-stripping policy for a particular XSLT package,
        /// as defined by its <c>xsl:strip-space</c> and <c>xsl:preserve-space</c>
        /// declarations
        /// </summary>
        /// <param name="pack">The package containing the relevant
        /// <c>xsl:strip-space</c> and <c>xsl:preserve-space</c> declarations</param>
        
        public WhitespacePolicy(Saxon.Api.XsltPackage pack) {
            this.policy = Whitespace.XSLT;
            this.stripperRules = pack.getUnderlyingPreparedPackage().getStripperRules();
        }
        
        // For internal use: construct the whitespace policy defined by xsl:strip-space and xsl:preserve-space
        // declarations in a stylesheet
        
        internal WhitespacePolicy(Saxon.Hej.style.StylesheetPackage pack) {
            this.policy = Whitespace.XSLT;
            this.stripperRules = pack.getStripperRules();
        }

        /// <summary>
        /// Create a custom whitespace stripping policy, by supplying a predicate that indicates for any given element,
        /// whether whitespace text nodes among its children should be stripped or preserved. Note that this decision
        /// ignores any xml:space attributes that might be present, and the decision applies only to immediate children,
        /// not to descendants.
        /// </summary>
        /// <param name="elementTest">a predicate applied to element names, which should return true if whitespace-only
        /// text node children of the element are to be stripped, false if they are to be retained.</param>
        /// <returns>A <c>WhitespacePolicy</c> object</returns>
        public static WhitespacePolicy MakeCustomPolicy(System.Predicate<QName> elementTest) {
            SpaceStrippingRule rule = new CustomSpaceStrippingRule(elementTest);
            return new WhitespacePolicy(Whitespace.XSLT, rule);
        }

        internal int Ordinal() {
            return policy;
        }

        /// Escape-hatch property to get the space stripping rules that
        /// implement this policy internally.
        
		public Saxon.Hej.om.SpaceStrippingRule Implementation => stripperRules;
        
    }

    internal class CustomSpaceStrippingRule : SpaceStrippingRule {

        private readonly System.Predicate<Saxon.Api.QName> preserveTest;

        public CustomSpaceStrippingRule(System.Predicate<Saxon.Api.QName> preserveTest) {
            this.preserveTest = preserveTest;
        }
        
        public int isSpacePreserving(NodeName nodeName, SchemaType schemaType) {
            return preserveTest(new QName(nodeName.getStructuredQName()))
            ? Stripper.ALWAYS_STRIP
            : Stripper.ALWAYS_PRESERVE;
        }
        
        public ProxyReceiver makeStripper(Receiver next) {
            return new Stripper(this, next);
        }
        
        public void export(ExpressionPresenter presenter) {
            throw new UnsupportedOperationException();
        }
    }

}

// Copyright (c) 2021 Saxonica Limited