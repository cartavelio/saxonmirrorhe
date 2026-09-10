/**
 * <p>This package contains classes used to support the compile-time extension instructions
 * enabling the functionality of XQuery Update to be used within XSLT.</p>
 * <p>This extension defines an <code>&lt;saxon:change&gt;</code> instruction which in turn can contain
 * sub-instructions <code>&lt;saxon:delete&gt;</code>, <code>&lt;saxon:insert&gt;</code>,
 * <code>&lt;saxon:rename&gt;</code>, <code>&lt;saxon:replace&gt;</code>, and <code>&lt;saxon:update&gt;</code>.</p>
 * <p>The code
 * is compiled into an expression tree identical to that used by the XQuery Update syntax.</p>
 * <p>None of these classes will be used directly by typical user applications.</p>
 */
package com.saxonica.ee.xtupdate;

// Copyright (c) 2020-2023 Saxonica Limited.