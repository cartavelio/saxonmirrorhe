package com.saxonica.serialize;

import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.serialize.XMLEmitter;
import net.sf.saxon.serialize.XMLIndenter;
import net.sf.saxon.transpile.CSharpReplaceException;
import net.sf.saxon.value.Whitespace;

import java.util.*;

/**
 * XMLIndenter with extra functionality for Saxon-PE and Saxon-EE
 */

public class XMLIndenterPE extends XMLIndenter {

    private int indentSpaces = 3;
    private int lineLength = 80;
    private Set<NodeName> doubleSpacedElements = null;

    public XMLIndenterPE(XMLEmitter next) {
        super(next);
    }

    /**
     * Set the properties for this indenter
     *
     * @param props the serialization properties
     */

    @Override
    @CSharpReplaceException(from = "java.lang.NumberFormatException", to = "System.FormatException")
    public void setOutputProperties(Properties props) {
        super.setOutputProperties(props);
        String s = props.getProperty(SaxonOutputKeys.INDENT_SPACES);
        if (s == null) {
            indentSpaces = 3;
        } else {
            try {
                indentSpaces = Integer.parseInt(Whitespace.trim(s));
            } catch (NumberFormatException err) {
                indentSpaces = 3;
            }
        }

        s = props.getProperty(SaxonOutputKeys.LINE_LENGTH);
        if (s != null) {
            try {
                lineLength = Integer.parseInt(Whitespace.trim(s));
            } catch (NumberFormatException err) {
                lineLength = 80;
            }
        }

        s = props.getProperty(SaxonOutputKeys.DOUBLE_SPACE);
        if (s != null) {
            doubleSpacedElements = new HashSet<>(8);
            StringTokenizer st = new StringTokenizer(s, " \t\r\n");
            while (st.hasMoreTokens()) {
                String eqName = st.nextToken();
                doubleSpacedElements.add(FingerprintedQName.fromEQName(eqName));
            }
        }

    }

    @Override
    protected boolean isDoubleSpaced(NodeName name) {
        return doubleSpacedElements != null && doubleSpacedElements.contains(name);
    }

    @Override
    protected int getIndentation() {
        return indentSpaces;
    }

    @Override
    protected int getLineLength() {
        return lineLength;
    }
}

// Copyright (c) 2011-2020 Saxonica Limited