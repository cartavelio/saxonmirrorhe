package com.saxonica.serialize;

import net.sf.saxon.serialize.CharacterReferenceGenerator;

import java.io.Writer;

/**
 * A class that represents a character as a decimal character reference and writes
 * the result to a supplied Writer.
 */
public class DecimalCharacterReferenceGenerator implements CharacterReferenceGenerator {

    public final static DecimalCharacterReferenceGenerator THE_INSTANCE = new DecimalCharacterReferenceGenerator();

    private DecimalCharacterReferenceGenerator() {
    }

    public void outputCharacterReference(int charval, Writer writer) throws java.io.IOException {
        writer.write("&#");
        writer.write(Integer.toString(charval));
        writer.write(';');
    }
}

// Copyright (c) 2011 Saxonica Limited.