/**
 * <p>The package <code>com.saxonica.ee.stream.om</code> contains internal code to support
 * executing of streaming templates in XSLT.</p>
 * <p>These classes will never be used by user-written applications.</p>
 * <p>The concept of a "fleeting node" is a node that is only transiently available during
 * a streaming pass over a document. It implements the <code>NodeInfo</code> interface
 * to a sufficient extent to enable the operations that are allowed for pattern matching
 * and template execution in a streaming mode.</p>
 */
package com.saxonica.ee.stream.om;

// Copyright (c) 2010-2023 Saxonica Limited.