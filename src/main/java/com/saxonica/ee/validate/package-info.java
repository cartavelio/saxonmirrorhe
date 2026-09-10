/**
 * <p>The validation package contains classes used to validate document instances against a schema.</p>
 * <p>This validation is all performed "on-the-fly", by a collection of classes that
 * implement Saxon's (SAX-like) Receiver interface, which is notified of start/end element
 * events, attributes, and text nodes, as they occur. A pipeline of such classes is
 * used both while processing input documents and while writing output documents, and in
 * both cases, validation classes can be added to the pipeline.</p>
 * <p>Local validation of elements is controlled using the ValidationStack class, which
 * manages a stack of validators for each element that is currently open. The most
 * interesting of these is the ComplexContentValidator, which makes use of a finite
 * state machine created by compiling a ComplexType in the schema. The finite state
 * machine is represented by the AutomatonState object corresponding to the initial
 * (entry) state of the machine.</p>
 * <p>Global validation of uniqueness and referential constraints is controlled using the
 * ConstraintChecker class. The maintains a set of Watch classes that are interested in
 * elements matching particular (restricted) XPath expressions. The restricted XPath expression
 * used in XML Schema (which is similar to, but not identical to, an XPath pattern) is
 * represented by a Selection object. Implementations of the Watch class are used to
 * handle the different kinds of constraint that a schema can specify.</p>
 * <p>None of the classes in this package will normally be used directly by user
 * applications.</p>
 */
package com.saxonica.ee.validate;

// Copyright (c) 2004-2023 Saxonica Limited.