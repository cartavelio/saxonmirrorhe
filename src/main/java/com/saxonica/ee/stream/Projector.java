package com.saxonica.ee.stream;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.expr.parser.PathMap;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.str.CodePoints;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;

import java.io.PrintStream;
import java.util.Stack;

/**
 * This class uses a {@link net.sf.saxon.expr.parser.PathMap} to do document projection.
 * It acts as a push filter, passing through only those events that are necessary
 * to evaluate the expression that was used to generate the PathMap.
 */
public class Projector extends ProxyReceiver {

    private final Stack pathPositionStack;    // entries on this stack are instances of PathMap.PathMapNodeSet
    private final Stack isCopiedStack;        // entries are booleans to indicate whether the start tag was copied
    private int level = 0;
    private int copyAllLevel = Integer.MAX_VALUE;
    private int inputNodes = 0;
    private int outputNodes = 0;
    private boolean displayStatistics = false;
    private final NamespaceBinding[] nsStack = new NamespaceBinding[20];
    private int nsStackTop = 0;
    private final int[] nsCountStack = new int[20];

    /**
     * Create a document projector
     *
     * @param pathMapRoot defines the paths within the document that need to be retained
     * @param next        the next receiver in the pipeline
     */

    public Projector(PathMap.PathMapRoot pathMapRoot, Receiver next) {
        super(next);
        pathPositionStack = new Stack();
        isCopiedStack = new Stack();
        pathPositionStack.push(new PathMap.PathMapNodeSet(pathMapRoot));
    }


    @Override
    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        displayStatistics = pipe.getConfiguration().isTiming();
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        // TODO: handle attributes and namespaces properly
        level++;
        inputNodes++;
        boolean elementCopied = false;
        if (level > copyAllLevel) {
            super.startElement(elemName, type, attributes, namespaces, location, properties);
            elementCopied = true;
            pathPositionStack.push("dummy");
        } else {
            PathMap.PathMapNodeSet state = (PathMap.PathMapNodeSet) pathPositionStack.peek();
            PathMap.PathMapNodeSet newState = new PathMap.PathMapNodeSet();
            boolean foundMatch = false;
            for (PathMap.PathMapNode aState : state) {
                PathMap.PathMapArc[] pathMapArcs = aState.getArcs();
                for (PathMap.PathMapArc arc : pathMapArcs) {
                    NodeTest test = arc.getNodeTest();
                    if (test.matches(Type.ELEMENT, elemName, type)) {
                        foundMatch = true;
                        PathMap.PathMapNode target = arc.getTarget();
                        newState.add(target);
                        if (target.isAtomized() || target.isReturnable()) {
                            copyAllLevel = level;
                        }
                    }
                    if (arc.getAxis() == AxisInfo.DESCENDANT) {
                        newState.add(aState);
                    }
                }
            }
            pathPositionStack.push(newState);
            if (foundMatch) {
                super.startElement(elemName, type, attributes, namespaces, location, properties);
                elementCopied = true;
            }
        }
        if (elementCopied) {
            outputNodes++;
            // Add any namespaces that were declared on ancestor elements that haven't been copied through
            int pendingNamespaces = 0;
            for (int i = level - 2; i >= 0; i--) {
                if ((Boolean) isCopiedStack.get(i)) {
                    break;
                } else {
                    pendingNamespaces += nsCountStack[i];
                }
            }
            for (int i = nsStackTop - pendingNamespaces; i < nsStackTop; i++) {
                boolean duplicate = false;
                for (int j = i + 1; j < nsStackTop; j++) {
                    if (nsStack[j].getPrefix().equals(nsStack[i].getPrefix())) {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate) {
                    throw new UnsupportedOperationException("to be re-implemented");
                    //super.namespaces(nsStack[i], ReceiverOptions.NONE);
                }
            }
        }
        isCopiedStack.push(elementCopied);

    }

    @Override
    public void characters(CodePoints chars, Location locationId, int properties) throws XPathException {
        inputNodes++;
        if (level >= copyAllLevel) {
            super.characters(chars, locationId, properties);
            outputNodes++;
        } else {
            PathMap.PathMapNodeSet state = (PathMap.PathMapNodeSet) pathPositionStack.peek();
            for (PathMap.PathMapNode pathNode : state) {
                PathMap.PathMapArc[] pathMapArcs = pathNode.getArcs();
                for (PathMap.PathMapArc arc : pathMapArcs) {
                    NodeTest test = arc.getNodeTest();
                    if (test.matches(Type.TEXT, null, null)) {
                        super.characters(chars, locationId, properties);
                        outputNodes++;
                        return;
                    }
                }
            }
        }
    }

    @Override
    public void comment(CodePoints chars, Location locationId, int properties) throws XPathException {
        inputNodes++;
        if (level > copyAllLevel) {
            super.comment(chars, locationId, properties);
            outputNodes++;
        } else {
            PathMap.PathMapNodeSet state = (PathMap.PathMapNodeSet) pathPositionStack.peek();
            for (PathMap.PathMapNode aState : state) {
                PathMap.PathMapArc[] pathMapArcs = aState.getArcs();
                for (PathMap.PathMapArc arc : pathMapArcs) {
                    NodeTest test = arc.getNodeTest();
                    if (test.matches(Type.COMMENT, null, null)) {
                        super.comment(chars, locationId, properties);
                        outputNodes++;
                        return;
                    }
                }
            }
        }
    }

    @Override
    public void processingInstruction(String target, CodePoints data, Location locationId, int properties) throws XPathException {
        inputNodes++;
        if (level > copyAllLevel) {
            super.processingInstruction(target, data, locationId, properties);
            outputNodes++;
        } else {
            PathMap.PathMapNodeSet state = (PathMap.PathMapNodeSet) pathPositionStack.peek();
            for (PathMap.PathMapNode aState : state) {
                PathMap.PathMapArc[] pathMapArcs = aState.getArcs();
                for (PathMap.PathMapArc arc : pathMapArcs) {
                    NodeTest test = arc.getNodeTest();
                    if (test.matches(Type.PROCESSING_INSTRUCTION, new NoNamespaceName(target), null)) {
                        super.processingInstruction(target, data, locationId, properties);
                        outputNodes++;
                        return;
                    }
                }
            }
        }
    }

    @Override
    public void endElement() throws XPathException {
        boolean wasCopied = (Boolean) isCopiedStack.pop();
        if (wasCopied) {
            super.endElement();
        }
        pathPositionStack.pop();
        level--;
        if (level < copyAllLevel) {
            copyAllLevel = Integer.MAX_VALUE;
        }
        nsStackTop -= nsCountStack[level];
        nsCountStack[level] = 0;
    }


    @Override
    public void endDocument() throws XPathException {
        if (displayStatistics) {
            outputStatistics(System.err);
        }
        super.endDocument();
    }

    /**
     * Output statistics showing the effectiveness of filtering
     *
     * @param out the output destination
     */

    public void outputStatistics(PrintStream out) {
        out.println("Document projection for " + getSystemId());
        out.println("-- Input nodes " + inputNodes + "; output nodes " + outputNodes +
                "; reduction = " + (100 - (100 * outputNodes) / inputNodes) + "%");
    }

}


// Copyright (c) 2007-2020 Saxonica Limited
