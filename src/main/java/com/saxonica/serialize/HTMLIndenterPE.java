package com.saxonica.serialize;

import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.serialize.HTMLIndenter;

import java.util.Properties;

/**
 * HTMLIndenter with extra functionality used by Saxon-PE and Saxon-EE
 */

public class HTMLIndenterPE extends HTMLIndenter {

    private int indentSpaces = 3;
    private int lineLength = 80;

    public HTMLIndenterPE(Receiver next, String method) {
        super(next, method);
    }

    /**
     * Set the properties for this indenter
     *
     * @param props the serialization properties
     */

    @Override
    public void setOutputProperties(Properties props) {
        super.setOutputProperties(props);
        String s = props.getProperty(SaxonOutputKeys.INDENT_SPACES);
        if (s == null) {
            indentSpaces = 3;
        } else {
            try {
                indentSpaces = Integer.parseInt(s);
            } catch (NumberFormatException err) {
                indentSpaces = 3;
            }
        }

        s = props.getProperty(SaxonOutputKeys.LINE_LENGTH);
        if (s == null) {
            lineLength = 80;
        } else {
            try {
                lineLength = Integer.parseInt(s);
            } catch (NumberFormatException err) {
                lineLength = 80;
            }
        }

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

// Copyright (c) 2011-2023 Saxonica Limited