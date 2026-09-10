package com.saxonica.serialize;

import net.sf.saxon.serialize.CharacterReferenceGenerator;
import net.sf.saxon.str.UnicodeWriter;

/**
 * A class that represents a character as a decimal character reference and writes
 * the result to a supplied Writer.
 */
public class DecimalCharacterReferenceGenerator implements CharacterReferenceGenerator {

    public final static DecimalCharacterReferenceGenerator THE_INSTANCE = new DecimalCharacterReferenceGenerator();

    private DecimalCharacterReferenceGenerator() {
    }

    @Override
    public void outputCharacterReference(int charval, UnicodeWriter writer) throws java.io.IOException {
        writer.writeCodePoint('&');
        writer.writeCodePoint('#');
        writer.write(Integer.toString(charval));
        writer.writeCodePoint(';');
    }
}

// Copyright (c) 2011-2023 Saxonica Limited