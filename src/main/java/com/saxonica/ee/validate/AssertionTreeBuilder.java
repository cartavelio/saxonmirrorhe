package com.saxonica.ee.validate;

import net.sf.saxon.event.*;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.tiny.TinyNodeImpl;
import net.sf.saxon.tree.tiny.TinyTree;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.SchemaType;

/**
 * This class sits in the validation pipeline, ready (when requested) to build subtrees of the validated
 * document for use in testing assertions against complex types. If no assertions are active, the class
 * passes input events through unchanged. If one or more assertions is active, it builds a tree corresponding
 * to the outermost active assertion. The nodes added to the tree will be typed. In the case of an element
 * node, the type annotation is added during the endElement call, after testing assertions against that element,
 * to satisfy the rule that when an assertion is evaluated, the target element has type AnyType but its attributes
 * and descendants are typed according to the appropriate schema declarations.
 */
public class AssertionTreeBuilder extends TeeOutputter {

    private final Sink sink;
    private final boolean retainComments;
    private TinyBuilder builder;

    public AssertionTreeBuilder(Receiver next, boolean retainComments) {
        super(next, new Sink(next.getPipelineConfiguration()));
        sink = (Sink) getSecondDestination();
        this.retainComments = retainComments;
    }

    public void activate(TinyBuilder builder) {
        this.builder = builder;
        setSecondDestination(retainComments ? builder : new CommentStripper(builder));
    }

    public void deactivate() {
        this.builder = null;
        setSecondDestination(sink);
    }

    /**
     *  Mark all ancestor elements of the current node as untyped (actually, as xs:anyType). This is
     *  invoked when a validation failure is encountered, because when a subtree contains invalid
     *  nodes, type annotations on ancestor elements are invalidated.
     */

    void markAncestorNodesUntyped() {
        if (builder != null) {
            TinyTree tree = builder.getTree();
            TinyNodeImpl last = tree.getNode(tree.getNumberOfNodes() - 1);
            SchemaType[] typeArray = tree.getTypeArray();
            if (typeArray != null) {
                while (last != null) {
                    typeArray[last.getNodeNumber()] = AnyType.getInstance();
                    last = last.getParent();
                }
            }
        }
    }


}

// Copyright (c) 2010-2022 Saxonica Limited