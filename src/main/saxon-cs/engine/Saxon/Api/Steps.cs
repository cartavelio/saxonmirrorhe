using System;
using System.Collections.Generic;
using System.Linq;
using Saxon.Hej.ma.arrays;
using Saxon.Hej.om;
using Saxon.Hej.value;
using Saxon.Api.Support;

namespace Saxon.Api {
    /// <summary>
    /// This non-instantiable class provides a number of useful implementations of the <c>Step</c>
    /// class which wraps a <c>Func</c> object, used to navigate XDM trees, typically getting the <c>Func</c> property 
    /// and used as an argument to <c>XdmValue#Select</c> and <c>XdmValue#SelectMany</c>.
    /// </summary>
    public class Steps {
        /// <summary>
        /// Obtain a <c>Step</c> that selects the root node of the containing document (which may or may not
        /// be a document node). If not a node a wrapped empty sequence is returned.
        /// </summary>
        /// <returns>A <c>Step</c> that selects the root node of the containing document.</returns>
        public static Step<XdmNode, XdmNode> Root() {
            return new Step<XdmNode, XdmNode>(origin => {
                IList<XdmNode> list = new List<XdmNode>(1);
                list.Add(origin.Root);
                return list;
            });
        }

        internal static Step<XdmNode, XdmNode> AxisStep(XdmAxis axis) {
            return new Step<XdmNode, XdmNode>(item => item.EnumerableOverAxis(axis));
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its ancestors, in reverse document
        /// order (that is, nearest ancestor first, root node last).
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the ancestor axis.</returns>
        public static Step<XdmNode, XdmNode> Ancestor() {
            return AxisStep(XdmAxis.Ancestor);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to its ancestor elements having a specified
        /// local name, irrespective of the namespace. The nodes are returned in reverse document
        /// order (that is, nearest ancestor first, root node last).
        /// </summary>
        /// <param name="localName">The local name of the ancestors to be selected by the <c>Step</c>,
        /// or "*" to select all ancestors that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the ancestors of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> Ancestor(String localName) {
            return Ancestor().Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Return a <c>Step</c> that navigates from a node to its ancestors having a specified
        /// namespace URI and local name, in reverse document order (that is, nearest ancestor first,
        /// root node last).
        /// </summary>
        /// <param name="uri">The namespace URI of the ancestors to be selected by the <c>Step</c></param>
        /// <param name="localName">The local name of the ancestors to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the ancestors (at most one) of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Ancestor(String uri, String localName) {
            return Ancestor().Where(Predicates.HasName(uri, localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the ancestor axis using a supplied <c>Predicate</c>.
        /// Nodes are returned in reverse document order (that is, nearest ancestor first, root node last).
        /// </summary>
        /// <remarks>The function call <c>Ancestor(predicate)</c> is equivalent to <c>Ancestor.Where(predicate)</c>.</remarks>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the ancestor-or-self axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Ancestor(Predicate<XdmItem> filter) {
            return Ancestor().Where(filter);
        }

        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its ancestors, in reverse document
        /// order, with the node itself returned at the start of the sequence (that is, origin node first,
        /// root node last).
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the ancestor-or-self axis.</returns>
        public static Step<XdmNode, XdmNode> AncestorOrSelf() {
            return AxisStep(XdmAxis.AncestorOrSelf);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to its ancestor elements having a specified
        /// local name, irrespective of the namespace. The nodes are returned in reverse document
        /// order (that is, nearest ancestor first, root node last), and include the node itself.
        /// </summary>
        /// <param name="localName">The local name of the ancestors to be selected by the <c>Step</c>,
        /// or "*" to select all ancestor-or-self nodes that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the ancestors-or-self of a supplied node that have 
        /// the required local name.</returns>
        public static Step<XdmNode, XdmNode> AncestorOrSelf(String localName) {
            return AncestorOrSelf().Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to its ancestors-or-self having a specified
        /// namespace URI and local name, in reverse document order (that is, nearest ancestor first, root node last).
        /// </summary>
        /// <param name="uri">The namespace URI of the ancestors to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the ancestors to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the ancestors-or-self of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> AncestorOrSelf(String uri, String localName) {
            return Ancestor().Where(Predicates.HasName(uri, localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the ancestor-or-self axis using a supplied <c>Predicate</c>.
        /// Nodes are returned in reverse document order (that is, origin node first, root node last).
        /// </summary>
        /// <remarks>The function call <c>AncestorOrSelf(predicate)</c> is equivalent to <c>AncestorOrSelf.Where(predicate)</c>.</remarks>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the ancestor-or-self axis using a supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> AncestorOrSelf(Predicate<XdmItem> filter) {
            return AncestorOrSelf().Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its attributes.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all attribute nodes.</returns>
        public static Step<XdmNode, XdmNode> Attribute() {
            return AxisStep(XdmAxis.Attribute);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to its attributes having a specified
        /// local name, irrespective of the namespace.
        /// </summary>
        /// <param name="localName">The local name of the attributes to be selected by the <c>Step</c>, or
        /// "*" to select all attributes</param>
        /// <returns>A <c>Step</c>, which selects the attributes of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> Attribute(String localName) {
            if (localName.Equals("*")) {
                return AxisStep(XdmAxis.Attribute);
            }
            else {
                return AxisStep(XdmAxis.Attribute).Where(Predicates.HasLocalName(localName));
            }
        }


        /// <summary>
        /// Return a <c>Step</c> that navigates from a node to its attribute having a specified
        /// namespace URI and local name.
        /// </summary>
        /// <param name="uri">The namespace URI of the attributes to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the attributes to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the attributes (at most one) of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Attribute(String uri, String localName) {
            return AxisStep(XdmAxis.Attribute).Where(Predicates.HasName(uri, localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the attribute axis using a supplied <c>Predicate</c>.
        /// The function call <c>Attribute(predicate)</c> is equivalent to <c>Attribute.Where(predicate)</c>.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the attribute axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Attribute(Predicate<XdmNode> filter) {
            return AxisStep(XdmAxis.Attribute).Where(filter.Invoke);
        }

        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its children
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the child axis</returns>
        public static Step<XdmNode, XdmNode> Child() {
            return AxisStep(XdmAxis.Child);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the element children having a specified
        /// local name, irrespective of the namespace.
        /// </summary>
        /// <param name="localName">The local name of the child elements to be selected by the <c>Step</c>,
        /// or "*" to select all children that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the element children of a supplied node that have the required local name.</returns>
        public static Step<XdmNode, XdmNode> Child(String localName) {
            return AxisStep(XdmAxis.Child).Where(Predicates.LocalNamePredicate(localName));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the element children having a specified
        /// namespace URI and local name.
        /// </summary>
        /// <param name="uri">The namespace URI of the child elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the child elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the element children of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Child(String uri, String localName) {
            return AxisStep(XdmAxis.Child).Where(Predicates.ExpandedNamePredicate(uri, localName));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the child axis using a supplied <c>Predicate</c>.
        /// The function call <c>Child(predicate)</c> is equivalent to <c>Child.Where(predicate)</c>.
        /// For example, <c>Child(IsElement())</c> returns a <c>Step</c> that selects the element node children
        /// of a given node.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the child axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Child(Predicate<XdmNode> filter) {
            return AxisStep(XdmAxis.Child).Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its descendants, which are returned in document order.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the descendant axis.</returns>
        public static Step<XdmNode, XdmNode> Descendant() {
            return AxisStep(XdmAxis.Descendant);
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the descendant elements having a specified
        /// local name, irrespective of the namespace. These are returned in document order.
        /// </summary>
        /// <param name="localname">The local name of the descendant elements to be selected by the <c>Step</c>,
        /// or "*" to select all descendants that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the element descendants of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> Descendant(String localname) {
            return Descendant().Where(Predicates.HasLocalName(localname));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the descendant elements having a specified
        /// namespace URI and local name.
        /// </summary>
        /// <param name="uri">The namespace URI of the elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the element descendants of a supplied node that have the required
        /// local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Descendant(String uri, String localName) {
            return Descendant().Where(Predicates.HasName(uri, localName));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the descendant axis using a supplied <c>Predicate</c>.
        /// The function call <c>Descendant(predicate)</c> is equivalent to <c>Descendant.Where(predicate)</c>.
        /// For example, <c>Steps.Descendant(Predicates.IsElement())</c>
        /// returns a <c>Step</c> that selects the element node descendants
        /// of a given node, while <c>Descendant(Predicate.Exists(attribute("id")))</c> selects those that have an attribute
        /// named "id". These are returned in document order.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the descendant axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Descendant(Predicate<XdmNode> filter) {
            return Descendant().Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its descendants, which are returned in document order,
        /// preceded by the origin node itself.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the descendant-or-self axis.</returns>
        public static Step<XdmNode, XdmNode> DescendantOrSelf() {
            return AxisStep(XdmAxis.DescendantOrSelf);
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the descendant-or-self elements having a specified
        /// local name, irrespective of the namespace. These are returned in document order.
        /// </summary>
        /// <param name="localname">The local name of the descendant-or-self elements to be selected by the <c>Step</c>,
        /// or "*" to select all descendant-or-self that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the descendant-or-self elements of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> DescendantOrSelf(String localname) {
            return DescendantOrSelf().Where(Predicates.HasLocalName(localname));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the descendant-or-self elements having a specified
        /// namespace URI and local name.
        /// </summary>
        /// <param name="uri">The namespace URI of the elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the descendant-or-self elements of a supplied node that have the required
        /// local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> DescendantOrSelf(String uri, String localName) {
            return DescendantOrSelf().Where(Predicates.HasName(uri, localName));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the descendant-or-self axis using a supplied <c>Predicate</c>.
        /// The function call <c>DescendantOrSelf(predicate)</c> is equivalent to <c>DescendantOrSelf.Where(predicate)</c>.
        /// For example, <c>Steps.DescendantOrSelf(Predicates.IsElement())</c>
        /// returns a <c>Step</c> that selects the descendant-or-self element nodes
        /// of a given node, while <c>DescendantOrSelf(Predicate.Exists(attribute("id")))</c> selects those that have an attribute
        /// named "id". These are returned in document order.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the descendant-or-self axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> DescendantOrSelf(Predicate<XdmNode> filter) {
            return DescendantOrSelf().Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its following nodes
        /// (excluding descendants), which are returned in document order.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the following axis.</returns>
        public static Step<XdmNode, XdmNode> Following() {
            return AxisStep(XdmAxis.Following);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the following elements having a specified
        /// local name, irrespective of the namespace. These are returned in document order.
        /// </summary>
        /// <param name="localName">The local name of the following elements to be selected by the <c>Step</c>,
        /// or "*" to select all following nodes that are elements</param>
        /// <returns>A <c>Step</c>, which selects the following elements of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> Following(String localName) {
            return AxisStep(XdmAxis.Following).Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the following elements having a specified
        /// namespace URI and local name. These are returned in document order.
        /// </summary>
        /// <param name="uri">The namespace URI of the following elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the following elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the following elements of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Following(String uri, String localName) {
            return AxisStep(XdmAxis.Following).Where(Predicates.HasName(uri, localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the following axis using a supplied <c>Predicate</c>.
        /// The function call <c>Following(predicate)</c> is equivalent to <c>Following().Where(predicate)</c>.
        /// For example, <c>Following(IsElement())</c> returns a <c>Step</c> that selects the following elements
        /// of a given node, while <c>Following(Exists(Attribute("id")))</c> selects those that have an attribute
        /// named "id". These are returned in document order.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the following axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Following(Predicate<XdmItem> filter) {
            return AxisStep(XdmAxis.Following).Where(filter);
        }

        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its following siblings, which are returned in document order.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the following-sibling axis.</returns>
        public static Step<XdmNode, XdmNode> FollowingSibling() {
            return AxisStep(XdmAxis.FollowingSibling);
        }

        /// <summary>
        ///  Obtain a <c>Step</c> that navigates from a node to the following sibling elements having a specified
        ///  local name, irrespective of the namespace. These are returned in document order.
        /// </summary>
        /// <param name="localName">The local name of the following sibling elements to be selected by the <c>Step</c>,
        /// or "*" to select all following siblings that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the following sibling elements of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> FollowingSibling(String localName) {
            return AxisStep(XdmAxis.FollowingSibling).Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the following sibling elements having a specified
        /// namespace URI and local name. These are returned in document order.
        /// </summary>
        /// <param name="uri">The namespace URI of the following sibling elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the following sibling elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the following sibling elements of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> FollowingSibling(String uri, String localName) {
            return AxisStep(XdmAxis.FollowingSibling).Where(Predicates.HasName(uri, localName));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the following sibling axis using a supplied <c>Predicate</c>.
        /// The function call <c>FollowingSibling(predicate)</c> is equivalent to <c>FollowingSibling.Where(predicate)</c>.
        /// For example, <c>FollowingSibling(IsElement())</c> returns a <c>Step</c> that selects the following sibling 
        /// elements of a given node, while <c>FollowingSibling(Exists(Attribute("id")))</c> 
        /// selects those that have an attribute named "id". These are returned in document order.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the following sibling axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> FollowingSibling(Predicate<XdmItem> filter) {
            return AxisStep(XdmAxis.FollowingSibling).Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its namespace nodes.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the namespace axis.</returns>
        public static Step<XdmNode, XdmNode> Namespace() {
            return AxisStep(XdmAxis.Namespace);
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to its namespaces having a specified
        /// local name. The local name of a namespace node corresponds to the prefix used in the
        /// namespace binding.
        /// </summary>
        /// <param name="localName">The local name (representing the namespace prefix) of the namespace nodes
        /// to be selected by the <c>Step</c>, or "*" to select all namespaces</param>
        /// <returns>A <c>Step</c>, which selects the namespaces of a supplied node that have a
        /// given local name (prefix).</returns>
        public static Step<XdmNode, XdmNode> Namespace(String localName) {
            if (localName.Equals("*")) {
                return AxisStep(XdmAxis.Namespace);
            }
            else {
                return AxisStep(XdmAxis.Namespace).Where(Predicates.HasLocalName(localName));
            }
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the namespace axis using a supplied <c>Predicate</c>.
        /// The function call <c>Namespace(predicate)</c> is equivalent to <c>Namespace().Where(predicate)</c>.
        /// For example, <c>Namespace(Eq("http://www.w3.org/1999/XSL/Transform")</c>
        /// selects a namespace node that binds a prefix to the XSLT namespace.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the namespace axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Namespace(Predicate<XdmItem> filter) {
            return AxisStep(XdmAxis.Namespace).Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its parent.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the parent axis (of which there is at most one).</returns>
        public static Step<XdmNode, XdmNode> Parent() {
            return AxisStep(XdmAxis.Parent);
        }

        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the parent element provided it has a specified
        /// local name, irrespective of the namespace.
        /// </summary>
        /// <param name="localName">The local name of the parent element to be selected by the <c>Step</c>,
        ///  or "*" to select the parent node provided it is an element</param>
        /// <returns>A <c>Step</c>, which selects the parent of a supplied node provided it is an element with the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> Parent(String localName) {
            return AxisStep(XdmAxis.Parent).Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the parent element provided it has a specified
        /// namespace URI and local name.
        /// </summary>
        /// <param name="uri">The namespace URI of the parent element to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the parent element to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the parent element of a supplied node provided it is an 
        /// element with the required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Parent(String uri, String localName) {
            return AxisStep(XdmAxis.Parent).Where(Predicates.HasName(uri, localName));
        }

        /// <summary>
        /// Obtain a <c>Step</c> that filters the node found on the parent axis using a supplied <c>Predicate</c>.
        /// The function call <c>Parent(filter)</c> is equivalent to <c>Parent(filter).where(Predicate)</c>.
        /// For example, <c>Parent(Predicate.IsElement()).GetFunc</c> returns a <c>Step</c> that selects the parent node provided it is an element
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the parent axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Parent(Predicate<XdmItem> filter) {
            return AxisStep(XdmAxis.Parent).Where(filter);
        }

        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its preceding siblings, which are returned
        /// in reverse document order.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the preceding-sibling axis.</returns>
        public static Step<XdmNode, XdmNode> PrecedingSibling() {
            return AxisStep(XdmAxis.PrecedingSibling);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the preceding sibling elements having a specified
        /// local name. These are returned in reverse document order.
        /// </summary>
        /// <param name="localName">The local name of the preceding sibling elements to be selected by the <c>Step</c>,
        /// or "*" to select all descendants that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the preceding sibling elements of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> PrecedingSibling(String localName) {
            return PrecedingSibling().Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the preceding sibling elements having a specified
        /// namespace URI and local name. These are returned in reverse document order.
        /// </summary>
        /// <param name="uri">The namespace URI of the preceding sibling elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the preceding sibling elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the preceding sibling elements of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> PrecedingSibling(String uri, String localName) {
            return PrecedingSibling().Where(Predicates.ExpandedNamePredicate(uri, localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the nodes found on the preceding sibling axis using a supplied <c>Predicate</c>.
        /// The function call <c>PrecedingSibling(predicate)</c> is equivalent to <c>PrecedingSibling().Where(predicate)</c>.
        /// For example, <c>PrecedingSibling(isElement())</c> returns a <c>Step</c> that selects the preceding sibling elements 
        /// of a given node, while <c>PrecedingSibling(Predicate.Exists(attribute("id")))</c> selects those that have an attribute named "id". 
        /// These are returned in reverse document order.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the following sibling axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> PrecedingSibling(Predicate<XdmItem> filter) {
            return PrecedingSibling().Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to its preceding nodes (excluding ancestors),
        /// which are returned in reverse document order.
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the preceding axis.</returns>
        public static Step<XdmNode, XdmNode> Preceding() {
            return AxisStep(XdmAxis.Preceding);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the preceding elements having a specified
        /// local name. These are returned in reverse document order.
        /// </summary>
        /// <param name="localName">The local name of the preceding elements to be selected by the <c>Step</c>,
        /// or "*" to select all descendants that are element nodes</param>
        /// <returns>A <c>Step</c>, which selects the preceding elements of a supplied node that have the
        /// required local name.</returns>
        public static Step<XdmNode, XdmNode> Preceding(String localName) {
            return Preceding().Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to the preceding elements having a specified
        /// namespace URI and local name. These are returned in reverse document order.
        /// </summary>
        /// <param name="uri">The namespace URI of the preceding elements to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the preceding elements to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the preceding elements of a supplied node that have the
        /// required local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Preceding(String uri, String localName) {
            return Preceding().Where(Predicates.ExpandedNamePredicate(uri, localName));
        }


        /// <summary>
        /// Obtain a Step that filters the nodes found on the preceding axis using a supplied <c>Predicate</c> .
        /// The function call <c>Preceding(predicate)</c> is equivalent to <c>Preceding().Where(predicate)</c>.
        /// For example, <c>Preceding(isElement())</c> returns a <c>Step</c> that selects the preceding elements 
        /// of a given node, while <c>Preceding(Predicate.Exists(attribute("id")))</c> selects those that have an attribute named "id". 
        /// These are returned in reverse document order.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the following axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Preceding(Predicate<XdmItem> filter) {
            return Preceding().Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> to navigate from a node to itself (useful only if applying a predicate).
        /// </summary>
        /// <returns>A <c>Step</c> that selects all nodes on the self axis (that is, the node itself).</returns>
        public static Step<XdmNode, XdmNode> Self() {
            return AxisStep(XdmAxis.Self);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to itself provided it is an element with a specified
        /// local name, irrespective of the namespace.
        /// </summary>
        /// <param name="localName">The local name of the element to be selected by the <c>Step</c>,
        /// or "*" to select the node provided that it is an element node</param>
        /// <returns>A <c>Step</c>, which selects the supplied node provided it has a given local name.</returns>
        public static Step<XdmNode, XdmNode> Self(String localName) {
            return AxisStep(XdmAxis.Self).Where(Predicates.HasLocalName(localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that navigates from a node to itself provided it has a specified
        /// namespace URI and local name.
        /// </summary>
        /// <param name="uri">The namespace URI of the element to be selected by the <c>Step</c>:
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">The local name of the element to be selected by the <c>Step</c></param>
        /// <returns>A <c>Step</c>, which selects the supplied node provided it is an element with a given local name and namespace URI.</returns>
        public static Step<XdmNode, XdmNode> Self(String uri, String localName) {
            return AxisStep(XdmAxis.Self).Where(Predicates.HasName(uri, localName));
        }


        /// <summary>
        /// Obtain a <c>Step</c> that filters the node found on the self axis using a supplied <c>Predicate</c> filter.
        /// The function call <c>Self(predicate)</c> is equivalent to <c>Self().Where(predicate)</c>.
        /// For example, <c>self(isElement())</c> returns a <c>Step</c> that selects the supplied node provided it is an element.
        /// </summary>
        /// <param name="filter">The predicate to be applied</param>
        /// <returns>A <c>Step</c> that filters the nodes found on the self axis using the supplied predicate.</returns>
        public static Step<XdmNode, XdmNode> Self(Predicate<XdmNode> filter) {
            return AxisStep(XdmAxis.Self).Where(filter);
        }


        /// <summary>
        /// Obtain a <c>Step</c> that atomizes an item to produce a <c>XdmValue</c> of atomic values. Atomizing a node will
        /// usually produce a single atomic value, but in the case of schema-typed nodes using a list type, there may
        /// be more than one atomic value. Atomizing an array also returns multiple atomic values.
        /// </summary>
        public static Step<XdmItem, XdmAtomicValue> Atomize() {
            return new Step<XdmItem, XdmAtomicValue>(
                item => {
                    IList<XdmAtomicValue> list = new List<XdmAtomicValue>();

                    if (item is XdmAtomicValue atomicValue) {
                        list.Add(atomicValue);
                    }
                    else if (item is XdmNode node) {
                        try {
                            var value = node.value;
                            list.Add((XdmAtomicValue) XdmValue.Wrap(((NodeInfo) value).atomize())[0]);
                        }
                        catch (Exception) {
                            throw new Exception("Cannot atomize supplied value");
                        }
                    }
                    else if (item is XdmArray array) {
                        list.Add((XdmAtomicValue) XdmValue.Wrap(((ArrayItem) array.value).atomize()));
                    }
                    else {
                        throw new Exception("Cannot atomize supplied value");
                    }

                    return list;
                });
        }


        /// <summary>
        /// Obtain a <c>Step</c> that returns text nodes found on the child axis.
        /// The function call <c>Text()</c> is equivalent to <c>Child().Where(Predicate.IsText)</c>.
        /// </summary>
        /// <returns>A <c>Step</c> that returns the text nodes found on the child axis.</returns>
        public static Step<XdmNode, XdmNode> Text() {
            return Child().Where(Predicates.IsText());
        }


        /// <summary>
        /// Obtain a <c>Step</c> whose effect is to tokenize the supplied item on whitespace
        /// boundaries, returning a sequence of strings as <c>XdmAtomicValue</c> instances.
        /// </summary>
        /// <remarks><p>Note: the tokenize step, when applied to a string with leading and trailing whitespace,
        /// has the effect of removing this whitespace. In addition to its primary role, the function
        /// can therefore be useful for trimming the content of a single string.</p></remarks>
        /// <returns>A <c>Step</c> whose effect is to take a supplied item and split its string
        /// value into a sequence of <c>xs:string</c> instances</returns>
        public static Step<XdmAtomicValue, XdmAtomicValue> Tokenize() {
            return new Step<XdmAtomicValue, XdmAtomicValue>(item => {
                {
                    return new SequenceEnumerable<XdmAtomicValue>(
                        () => new Saxon.Hej.value.Whitespace.Tokenizer(item.StringValue));
                }
            });
        }

        /// <summary>
        /// Obtain a Step whose effect is to interpret the supplied item as an <c>xs:ID</c> value
        /// and return the nodes (in a given document) that have that string as their ID.
        /// </summary>
        /// <param name="doc">The root node (document node) of the document within which the ID
        /// value should be sought</param>
        /// <returns>A <c>Step</c> whose effect is to return the nodes that have the given string as their ID.</returns>
        public static Step<XdmNode, XdmNode> id(XdmNode doc) {
            return new Step<XdmNode, XdmNode>(item => {
                IList<XdmNode> list = new List<XdmNode>();
                NodeInfo target = ((NodeInfo) doc.value).getTreeInfo()
                    .selectID(doc.StringValue, true);

                if (target != null) {
                    list.Add(new XdmNode(target));
                }

                return list;
            });
        }


        /// <summary>
        /// Construct a path as a composite <c>Step</c> from a sequence of steps composed together.
        /// </summary>
        /// <param name="steps">The constituent steps in the path</param>
        /// <returns>A composite step.</returns>
        public static Step<XdmNode, XdmNode> Path(params string[] steps) {
            IList<Step<XdmNode, XdmNode>> pathSteps = new List<Step<XdmNode, XdmNode>>();

            foreach (string step in steps) {
                switch (step) {
                    case "/":
                        pathSteps.Add(Steps.Root().Where(Predicates.IsDocument()));
                        break;
                    case "..":
                        pathSteps.Add(Steps.Parent());
                        break;
                    case "*":
                        pathSteps.Add(Steps.Child(Predicates.IsElement()));
                        break;
                    case "//":
                        pathSteps.Add(Steps.DescendantOrSelf());
                        break;
                    default: {
                        if (step.StartsWith("@")) {
                            string name = step[1..];
                            if (!Saxon.Hej.om.NameChecker.isValidNCName(name)) {
                                throw new System.ArgumentException("Invalid attribute name " + name);
                            }

                            pathSteps.Add(Steps.Attribute(name));
                        }
                        else {
                            if (!Saxon.Hej.om.NameChecker.isValidNCName(step)) {
                                throw new System.ArgumentException("Invalid element name " + step);
                            }

                            pathSteps.Add(Steps.Child(step));
                        }

                        break;
                    }
                }
            }

            return PathFromEnumerable(pathSteps);
        }


        internal static Step<XdmNode, XdmNode> PathFromEnumerable(IEnumerable<Step<XdmNode, XdmNode>> steps) {
            List<Step<XdmNode, XdmNode>> list = steps.ToList();
            return PathFromList(list);
        }


        /// <summary>
        /// Construct a path as a composite <c>Step</c> from a list of steps composed together.
        /// </summary>
        /// <param name="steps">The constituent steps in the path</param>
        /// <returns>A composite step.</returns>
        public static Step<XdmNode, XdmNode> PathFromList(List<Step<XdmNode, XdmNode>> steps) {
            return steps.Count switch {
                0 => new Step<XdmNode, XdmNode>(item => new List<XdmNode>()),
                1 => steps.ElementAt(0),
                _ => steps.ElementAt(0).Then(PathFromList(steps.GetRange(1, steps.Count - 1)))
            };
        }
    }
}

// Copyright (c) 2021 Saxonica Limited