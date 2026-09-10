package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.Comment;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;


/**
 * Generate bytecode for the Comment Instruction
 */
public class CommentCompiler extends SimpleNodeConstructorCompiler {

    /**
     * Compile code to check the content of the node, and adjust it if necessary
     * On entry the proposed content is on the stack as a UnicodeString; on exit the revised content
     * must be on the stack as a UnicodeString.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */

    @Override
    public void checkContent(CompilerService compiler, Expression expression) {
        Comment comment = (Comment) expression;
        Generator ga = compiler.getCurrentGenerator();
        if (comment.isXSLT()) {
            ga.invokeStaticMethod(Comment.class, "checkContentXSLT", UnicodeString.class);
        } else {
            ga.invokeStaticMethod(Comment.class, "checkContentXQuery", UnicodeString.class);
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
        allocateStatic(compiler, Loc.NONE);
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "comment", UnicodeString.class, Location.class, Integer.TYPE);
    }
}

// Copyright (c) 2011-2020 Saxonica Limited