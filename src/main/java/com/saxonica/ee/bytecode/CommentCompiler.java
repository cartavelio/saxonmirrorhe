package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.Comment;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;


/**
 * Generate bytecode for the Comment Instruction
 */
public class CommentCompiler extends SimpleNodeConstructorCompiler {

    /**
     * Compile code to check the content of the node, and adjust it if necessary
     * On entry the proposed content is on the stack as a CharSequence; on exit the revised content
     * must be on the stack as a CharSequence.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */

    public void checkContent(CompilerService compiler, Expression expression) {
        Comment comment = (Comment) expression;
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeInstanceMethod(Object.class, "toString");
        if (comment.isXSLT()) {
            ga.invokeStaticMethod(Comment.class, "checkContentXSLT", String.class);
        } else {
            ga.invokeStaticMethod(Comment.class, "checkContentXQuery", String.class);
        }
    }

    /**
     * Generate code to write the node to the current receiver. On entry the stack holds the
     * current receiver, the node name (for named node kinds only) and the content (for all nodes)
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */
    @Override
    protected void pushNode(/*@NotNull*/ CompilerService compiler, Expression expression) {
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, ExplicitLocation.UNKNOWN_LOCATION);
        ga.push(0);
        ga.invokeInstanceMethod(Receiver.class, "comment", CharSequence.class, Location.class, Integer.TYPE);
    }
}

//Copyright (c) 2011 Saxonica Limited.