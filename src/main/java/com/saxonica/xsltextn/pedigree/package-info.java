/**
 * <p>This package contains classes associated with the implementation of the <code>saxon:deep-update</code>
 * extension instruction.</p>
 * <p>This instruction uses a special implementation of maps in which navigation through a tree of maps
 * maintains information about the route by which the leaf maps were reached. This information is referred
 * to as a <i>pedigree</i>. The data structure used is analogous to the concept of a "zipper" found in some
 * functional programming languages. By maintaining the connection between the root of a tree and the leaves,
 * it becomes possible for an update to the leaves of the tree to be propagated upwards, resulting in a new
 * version of the object at the root of the tree.</p>
 */
package com.saxonica.xsltextn.pedigree;

// Copyright (c) 2010-2023 Saxonica Limited.