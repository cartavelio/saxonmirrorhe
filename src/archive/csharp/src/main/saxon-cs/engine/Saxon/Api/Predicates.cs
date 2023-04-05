using System;
using System.Linq;
using System.Xml;
using Saxon.Hej.expr.instruct;
using Saxon.Hej.pattern;

namespace Saxon.Api {
    /// <summary>
    /// This non-instantiable class provides a number of useful implementations of the <c>System.Predicate</c>
    /// interface, designed for use when navigating streams of XDM items.
    /// </summary>
    public static class Predicates {

        /// <summary>
        /// A predicate to test whether all the supplied predicates are satisfied
        /// </summary>
        /// <param name="conditions">The predicates that must all be satisfied</param>
        /// <returns>A predicate that evaluates to true only if all the supplied predicates are satisfied</returns>
        public static Predicate<T> AllOf<T>(params Predicate<T>[] conditions) 
			where T : XdmItem {
            return item => conditions.All(pred => pred.Invoke(item));
        }
        
        /// <summary>
        /// A predicate to test whether at least one of the supplied predicates are satisfied
        /// </summary>
        /// <param name="conditions">The predicates, one of which must be satisfied</param>
        /// <returns>A predicate that evaluates to true only if all the supplied predicates are satisfied</returns>
        public static Predicate<T> AnyOf<T>(params Predicate<T>[] conditions) 
			where T : XdmItem {
            return item => conditions.Any(pred => pred.Invoke(item));
        }

        /// <summary>
        /// A predicate to test whether an item is an attribute node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is an attribute node.</returns>
        public static Predicate<XdmItem> IsAttribute() {
            return NodeKindPredicate(XmlNodeType.Attribute);
        }

        /// <summary>
        /// A predicate to test whether an item is a node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a node.</returns>
        public static Predicate<XdmItem> IsNode() {
            return item => item is XdmNode;
        }

        /// <summary>
        /// A predicate to test whether an item is an element node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is an element node.</returns>
        public static Predicate<XdmItem> IsElement() {
            return NodeKindPredicate(XmlNodeType.Element);
        }

        /// <summary>
        /// A predicate to test whether an item is a text node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a text node.</returns>
        public static Predicate<XdmItem> IsText() {
            return NodeKindPredicate(XmlNodeType.Text);
        }

        /// <summary>
        /// A predicate to test whether an item is a comment node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a comment node.</returns>
        public static Predicate<XdmItem> IsComment() {
            return NodeKindPredicate(XmlNodeType.Comment);
        }

        /// <summary>
        /// A predicate to test whether an item is a processing instruction node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a processing instruction node.</returns>
        public static Predicate<XdmItem> IsProcessingInstruction() {
            return NodeKindPredicate(XmlNodeType.ProcessingInstruction);
        }

        /// <summary>
        /// A predicate to test whether an item is a document node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a document node.</returns>
        public static Predicate<XdmItem> IsDocument() {
            return NodeKindPredicate(XmlNodeType.Document);
        }

        /// <summary>
        /// A predicate to test whether an item is a namespace node.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a namespace node.</returns>
        public static Predicate<XdmItem> IsNamespace() {
            return NodeKindPredicate(XmlNodeType.None);
        }

        /// <summary>
        /// A predicate to test whether an item is an atomic value.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is an atomic value.</returns>
        public static Predicate<XdmItem> IsAtomic() {
            return item => item is XdmAtomicValue;
        }

        /// <summary>
        /// A predicate to test whether an item is a function value (this includes maps and arrays).
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a function, including
        /// maps and arrays.</returns>
        public static Predicate<XdmItem> IsFunction() {
            return item => item is XdmFunctionItem;
        }


        /// <summary>
        /// A predicate to test whether an item is an XDM map.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is a map.</returns>
        public static Predicate<XdmItem> IsMap() {
            return item => item is XdmMap;
        }

        /// <summary>
        /// A predicate to test whether an item is an XDM array.
        /// </summary>
        /// <returns>A predicate that returns true if given an item that is an array.</returns>
        public static Predicate<XdmItem> IsArray() {
            return item => item is XdmArray;
        }

        /// <summary>
        /// Obtain a predicate that tests whether a supplied <c>Step</c> delivers an empty result.
        /// </summary>
        /// <param name="step">A step to be applied to the item being tested</param>
        /// <returns>A predicate that returns true if the supplied step returns an empty result.</returns>
        public static Predicate<TInput> Empty<TInput, TResult>(Step<TInput, TResult> step)
            where TInput : XdmItem
            where TResult : XdmItem {
            return item => !step.Invoke(item).Any();
        }


        /// <summary>
        /// Return a <c>Predicate</c> that is the negation of a supplied <c>Predicate</c>.
        /// </summary>
        /// <typeparam name="TInput">The type of object to which the predicate is applicable</typeparam>
        /// <param name="condition">The supplied predicate</param>
        /// <returns>A predicate that matches an item if and only if the supplied predicate does not match the item.</returns>
        public static Predicate<TInput> Not<TInput>(Predicate<TInput> condition)
            where TInput : XdmItem {
            return item => !condition.Invoke(item);
        }


        /// <summary>
        /// Obtain a predicate that tests whether a supplied <c>Step</c> delivers a non-empty result.
        /// </summary>
        /// <param name="step">A step to be applied to the item being tested</param>
        /// <returns>A predicate that returns true if the step returns a non-empty result.</returns>
        public static Predicate<XdmItem> Exists(Step<XdmItem, XdmItem> step) {
            return item => step.Invoke(item).Any();
        }


        /// <summary>
        /// Obtain a predicate that tests whether an item is a node with a given namespace URI and local name.
        /// </summary>
        /// <param name="uri">The required namespace URI: supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The required local name</param>
        /// <returns>A predicate that returns true if and only if the supplied item is a node with the given namespace URI and local name.</returns>
        public static Predicate<XdmNode> HasName(string uri, string localName) {
            return item => {
                QName name = item.NodeName;
                return name != null && name.LocalName.Equals(localName) && name.Uri.Equals(uri);
            };
        }


        internal static Predicate<XdmItem> ExpandedNamePredicate(string ns, string local) {
            return item => {
                if (item is not XdmNode node) {
                    return false;
                }

                return node.NodeKind == XdmNodeKind.Element
                       && node.NodeName.LocalName.Equals(local) && node.NodeName.Uri.Equals(ns);
            };
        }

        internal static Predicate<XdmNode> LocalNamePredicate(string given) {
            if ("*".Equals(given)) {
                return IsElement();
            }

            return node => node.NodeKind == XdmNodeKind.Element
                           && node.NodeName.LocalName.Equals(given);
        }


        /// <summary>
        /// Obtain a predicate that tests whether an item is a node with a given local name, irrespective of the namespace.
        /// </summary>
        /// <param name="localName">The required local name. If the zero-length string is supplied, the predicate
        /// matches nodes with no name, such as namespace nodes for the default namespace.</param>
        /// <returns>A predicate that returns true if and only if the supplied item is a node with the given namespace URI and local name.</returns>
        public static Predicate<XdmNode> HasLocalName(string localName) {
            if (localName == "") {
                return item => item.NodeName == null;
            }
            return item => {
                QName name = item.NodeName;
                return name != null &&
                       name.LocalName.Equals(localName);
            };
        }

        /// <summary>
        /// Obtain a predicate that tests whether an item is a node with a given namespace URI.
        /// </summary>
        /// <param name="uri">The required namespace URI: supply a zero-length string to identify the null namespace</param>
        /// <returns>A predicate that returns true if and only if the supplied item is a node with the given
        /// namespace URI. If a zero-length string is supplied, the predicate will also match nodes having no name,
        /// such as text and comment nodes, and nodes having a local name only, such as namespace and processing-instruction
        /// nodes.</returns>
        public static Predicate<XdmNode> HasNamespace(string uri) {
            return item => {
                QName name = item.NodeName;
                return name != null && name.Uri.Equals(uri);
            };
        }


        /// <summary>
        /// Obtain a predicate that tests whether an item is an element node with a given attribute (whose name is in no namespace).
        /// </summary>
        /// <param name="local">The required attribute name</param>
        /// <returns>A predicate that returns true if and only if the supplied item is an element having an attribute
        /// with the given local name, in no namespace.</returns>
        public static Predicate<XdmNode> HasAttribute(string local) {
            return item => item.GetAttributeValue(local) != null;
        }

        /// <summary>
        /// Obtain a predicate that tests whether an item is an element node with a given attribute (whose
        /// name is in no namespace) whose string value is equal to a given value.
        /// </summary>
        /// <param name="local">The required attribute name</param>
        /// <param name="value">The required attribute value</param>
        /// <returns>A predicate that returns true if and only if the supplied item is an element having an attribute
        /// with the given local name, in no namespace, whose string value is equal to the given value.</returns>
        public static Predicate<XdmNode> AttributeEq(string local, string value) {
            return item => value.Equals(item.GetAttributeValue(local));
        }


        /// <summary>
        /// Obtain a predicate that tests whether an atomic value compares equal to a supplied atomic value of
        /// a comparable type.
        /// </summary>
        /// <param name="value">The atomic value to be compared with</param>
        /// <returns>A predicate which returns true when applied to a value that is equal to the supplied
        /// value under the "is-same-key" comparison rules. (These are the rules used to compare key values
        /// in an XDM map. The rules are chosen to be context-free, error-free, and transitive.)</returns>
        public static Predicate<XdmAtomicValue> Eq(XdmAtomicValue value) {
            return val => val.Equals(value);
        }

        /// <summary>
        /// Obtain a predicate that tests whether the result of applying the XPath <c>string()</c> function to an item
        /// is equal to a given string.
        /// </summary>
        /// <param name="value">The string being tested</param>
        /// <returns>A predicate which returns true if the string value of the item being tested
        /// is equal to the given string under Java comparison rules for comparing strings.</returns>
        public static Predicate<XdmItem> Eq(string value) {
            return item => item.StringValue.Equals(value);
        }

        /// <summary>
        /// Obtain a predicate that tests whether there is some item in the result of applying a step,
        /// whose string value is equal to a given string. For example, <c>Eq(attribute("id"), "foo")</c>
        /// matches an element if it has an "id" attribute whose value is "foo".
        /// </summary>
        /// <typeparam name="TInput">The type of the input object to the <c>Step</c> function</typeparam>
        /// <typeparam name="TResult">The result type after invoking the function</typeparam>
        /// <param name="step">The step to be evaluated</param>
        /// <param name="value">The string to be compared against the items returned by the step</param>
        /// <returns>A predicate which returns true if some item selected by the step has a string value
        /// equal to the given string.</returns>
        public static Predicate<TInput> Eq<TInput, TResult>(Step<TInput, TResult> step, string value)
            where TInput : XdmItem
            where TResult : XdmItem {
            return Some(step, Eq(value));
        }

        /// <summary>
        /// Obtain a predicate that tests whether the result of applying the XPath <c>string()</c> function to an item
        /// matches a given regular expression.
        /// </summary>
        /// <param name="regex">The regular expression (this is a Java regular expression, not an XPath regular expression)</param>
        /// <returns>A predicate which returns true if the string value of the item being tested
        /// contains a substring that matches the given regular expression. To test the string in its entirety,
        /// use anchors "^" and "$" in the regular expression.</returns>
        /// 
        public static Predicate<XdmItem> MatchesRegex(string regex) {
            return item1 => System.Text.RegularExpressions.Regex.Match(item1.StringValue, regex).Success;
        }

        /// <summary>
        /// Obtain a predicate that tests whether there is some item in the result of applying a step that
        /// satisfies the supplied condition.
        /// </summary>
        /// <remarks><para>For example, <c>some(CHILD, exists(attribute("foo"))</c> matches an element if it has a child
        /// element with an attribute whose local name is "foo".</para>
        /// <para>If the step returns an empty sequence the result will always be false.</para></remarks>
        /// <typeparam name="TInput">The type of the input object to the <c>Step</c> function</typeparam>
        /// <typeparam name="TResult">The expected result type after invoking the function. The predicate must also be of this type.</typeparam>
        /// <param name="step">The step to be evaluated</param>
        /// <param name="condition">The predicate to be applied to the items returned by the step</param>
        /// <returns>A predicate which returns true if some item selected by the step satisfies the supplied condition.</returns>
        public static Predicate<TInput> Some<TInput, TResult>(Step<TInput, TResult> step, Predicate<TResult> condition)
            where TInput : XdmItem
            where TResult : XdmItem {
            return item => step.Invoke(item).Any(condition.Invoke);
        }


        /// <summary>
        /// Obtain a predicate that tests whether every item in the result of applying a step
        /// satisfies the supplied condition.
        /// </summary>
        /// <remarks> <para>For example, <c>every(CHILD, exists(attribute("foo"))</c> matches an element if each of its child
        /// elements has an attribute whose local name is "foo".</para>
        /// <para>If the step returns an empty sequence the result will always be true.</para></remarks>
        /// <typeparam name="TInput">the type of the input object to the <c>Step</c> function</typeparam>
        /// <typeparam name="TResult">The expected result type after invoking the function. The predicate must also be of this type.</typeparam>
        /// <param name="step">The step to be evaluated</param>
        /// <param name="condition">The predicate to be applied to the items returned by the step</param>
        /// <returns>A predicate which returns true if every item selected by the step satisfies the supplied condition.</returns>
        public static Predicate<TInput> Every<TInput, TResult>(Step<TInput, TResult> step, Predicate<TResult> condition)
            where TInput : XdmItem
            where TResult : XdmItem {
            return item => step.Invoke(item).All(condition.Invoke);
        }


        internal static Predicate<XdmItem> NodeKindPredicate(XmlNodeType kind) {
            return node => node.IsNode() && ((XdmNode) node).NodeType == kind;
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited