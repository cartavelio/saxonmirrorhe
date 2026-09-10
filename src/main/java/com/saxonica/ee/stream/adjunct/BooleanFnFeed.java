package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;

/**
 * Feed implementation supporting the {@code fn:boolean()} and {@code fn:not()} functions
 */

public abstract class BooleanFnFeed extends ItemFeed {
    private WatchManager watchManager;
    private int count;
    private int level;
    private boolean foundAtomic;
    private boolean result;
    private boolean done;
    private boolean closed;

    BooleanFnFeed(WatchManager watchManager, Expression exp, ItemFeed result, XPathContext context) {
        super(exp, result, context);
        this.watchManager = watchManager;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        count = 0;
        level = 0;
        result = !isPositive();
    }


    protected abstract boolean isPositive();

    private void testNode() throws XPathException {
        if (level == 0 && !hasFailed()) {
            if (++count == 1) {
                done = true;
                getNextOutputter().append(BooleanValue.get(isPositive()));
                if (watchManager.allowsEarlyExit()) {
                    closed = true;
                    getNextOutputter().close();
                    getTerminator().terminate();
                }
            } else if (foundAtomic) {
                try {
                    ExpressionTool.ebvError("a sequence of two or more items starting with an atomic value", getExpression());
                } catch (XPathException e) {
                    dynamicError(e);
                }
            }
        }
    }

    @Override
    public void append(Item item) throws XPathException {
        if (!hasFailed()) {
            if (item instanceof NodeInfo) {
                testNode();
            } else if (++count == 1) {
                try {
                    result = ExpressionTool.effectiveBooleanValue(item) ^ !isPositive();
                } catch (XPathException e) {
                    dynamicError(e);
                    return;
                }
                foundAtomic = item instanceof AtomicValue;
            } else {
                if (foundAtomic) {
                    try {
                        ExpressionTool.ebvError("a sequence of two or more items starting with an atomic value", getExpression());
                    } catch (XPathException e) {
                        dynamicError(e);
                    }
                }
            }
        }
    }

    @Override
    public void startDocument(int properties) throws XPathException {
        testNode();
        level++;
    }

    @Override
    public void endDocument() throws XPathException {
        level--;
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        testNode();
        level++;
    }

    @Override
    public void namespace(String prefix, NamespaceUri namespaceUri, int properties) throws XPathException {
        testNode();
    }

    @Override
    public void endElement() {
        level--;
    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
        testNode();
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) throws XPathException {
        testNode();
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        testNode();
    }

    @Override
    public void close() throws XPathException {
        if (!hasFailed() && !done) {
            getNextOutputter().append(BooleanValue.get(result));
            if (!closed) {
                super.close();
            }
            done = true;
        }
    }

    /**
     * Feed implementation for the fn:boolean function
     */

    public static class Positive extends BooleanFnFeed {

        public Positive(WatchManager watchManager, Expression exp, ItemFeed result, XPathContext context) {
            super(watchManager, exp, result, context);
        }

        @Override
        protected boolean isPositive() {
            return true;
        }

    }

    /**
     * Feed implementation for the fn:not function
     */

    public static class Negative extends BooleanFnFeed {

        public Negative(WatchManager watchManager, Expression exp, ItemFeed result, XPathContext context) {
            super(watchManager, exp, result, context);
        }

        @Override
        protected boolean isPositive() {
            return false;
        }
        
    }
}

// Copyright (c) 2018-2022 Saxonica Limited