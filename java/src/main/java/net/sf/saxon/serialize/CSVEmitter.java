////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.serialize;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.functions.NormalizeUnicode;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.z.IntIterator;

import java.io.IOException;
import java.util.Properties;

/**
 * This class implements the back-end text generation of the CSV serialization method. It takes
 * as input a sequence of event-based calls such as startRow, writeItem, and endRow
 * and generates the lexical CSV output.
 *
 */

public class CSVEmitter extends SequenceWriter {

    private final Configuration config;
    private final UnicodeWriter writer;
    private int quoteCharacter = '"';
    private int separatorChar = ',';
    private boolean normalize;
    private String normalizationForm = "none";
    private CharacterMap characterMap;
    private Properties outputProperties;
    private boolean csvHeader;
    private Twine8 endOfLine = Twine8.NEWLINE;
    private boolean mustClose = true;
    private boolean allowTopLevelMap = true;
    private boolean allowTopLevelArray = true;



    public CSVEmitter(PipelineConfiguration pipe, UnicodeWriter writer, Properties outputProperties) throws XPathException {
        super(pipe);
        config = pipe.getConfiguration();
        setOutputProperties(outputProperties);
        this.writer = writer;

    }

    /**
     * Set output properties
     *
     * @param details the output serialization properties
     */

    public void setOutputProperties(Properties details) throws XPathException {
        this.outputProperties = details;
        if ("yes".equals(details.getProperty(SaxonOutputKeys.CSV_HEADER))) {
            csvHeader = true;
        }
        String quoteProperty = details.getProperty(SaxonOutputKeys.CSV_QUOTE_CHARACTER);
        if (quoteProperty != null) {
            quoteCharacter = StringTool.codePoints(quoteProperty).next();
        }
        String separatorProperty = details.getProperty(SaxonOutputKeys.CSV_SEPARATOR);
        if (separatorProperty != null) {
            separatorChar = StringTool.codePoints(separatorProperty).next();
        }
        String nl = details.getProperty(SaxonOutputKeys.LINE_ENDING);
        if (nl != null) {
            endOfLine = new Twine8(nl.replace("\\r", "\r").replace("\\n", "\n"));
        }
        String normalForm = details.getProperty(SaxonOutputKeys.NORMALIZATION_FORM);
        if (normalForm != null) {
            normalizationForm = normalForm;
        }

        if (separatorChar == quoteCharacter) {
            throw new XPathException("CSV separator character must differ from quotation character", "SEPM0016");
        }

    }

    /**
     * Say whether the output must be closed on completion
     *
     * @param mustClose true if the output must be closed
     */
    public void setMustClose(boolean mustClose) {
        this.mustClose = mustClose;
    }

    /**
     * Get the output properties
     *
     * @return the properties that were set using setOutputProperties
     */

    public Properties getOutputProperties() {
        return outputProperties;
    }




    /**
     * Set the CharacterMap to be used, if any
     *
     * @param map the character map
     */

    public void setCharacterMap(CharacterMap map) {
        this.characterMap = map;
    }




    /**
     * End of the document.
     *
     * @throws XPathException if any error occurs
     */

    public void close() throws XPathException {

        if (writer != null) {
            try {
                if (mustClose) {
                    writer.close();
                } else {
                    writer.flush();
                }
            } catch (IOException e) {
                throw new XPathException(e);
            }
        }
    }

    @Override
    public void write(Item item) throws XPathException {
        try {
            if (item instanceof ArrayItem a1) {
                if (!allowTopLevelArray) {
                    throw new XPathException("CSV output: top-level array items cannot follow a map item", "SERE0025");
                }
                allowTopLevelMap = false;
                processArray(a1);
            } else if (item instanceof MapItem m) {
                if (!allowTopLevelMap) {
                    throw new XPathException("CSV output: a top-level map item cannot follow another item", "SERE0025");
                }
                allowTopLevelMap = false;
                allowTopLevelArray = false;
                if (csvHeader) {
                    GroundedValue columns = m.getU(new Twine8("columns"));
                    if (columns == null) {
                        throw new XPathException("For CSV serialization with csv-header=true, the input map must have a ?columns entry", "SERE0025");
                    }
                    boolean first = true;
                    for (Item it : columns.asIterable()) {
                        if (first) {
                            first = false;
                        } else {
                            writer.writeCodePoint(separatorChar);
                        }
                        writer.write(it.getUnicodeStringValue());
                    }
                    writer.write(endOfLine);
                }
                GroundedValue rows = m.getU(new Twine8("rows"));
                if (rows == null) {
                    throw new XPathException("For CSV serialization, the input map must have a ?rows entry", "SERE0025");
                }
                for (Item it : rows.asIterable()) {
                    if (it instanceof ArrayItem a2) {
                        processArray(a2);
                    } else {
                        throw new XPathException("For CSV serialization, the ?rows entry must be a sequence of arrays", "SERE0025");
                    }
                }
            } else {
                throw new XPathException("CSV output: input must be a map or a sequence of arrays", "SERE0025");
            }
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }

    private void processArray(ArrayItem a) throws XPathException, IOException {
        boolean first = true;
        for (GroundedValue gv : a.members()) {
            if (gv.materialize() instanceof AtomicValue av) {
                if (first) {
                    first = false;
                } else {
                    writer.writeCodePoint(separatorChar);
                }
                UnicodeString content = av.getUnicodeStringValue();
                writer.write(processString(content));
            } else {
                throw new XPathException("An array to be serialized as CSV must have singleton atomic values as its members", "SERE0025");
            }
        }
        writer.write(endOfLine);
    }

    private UnicodeString processString(UnicodeString content) throws XPathException {
        if (!normalizationForm.equals("none")) {
            content = StringView.of(NormalizeUnicode.normalize(content.toString(), normalizationForm));
        }
        if (characterMap != null) {
            content = characterMap.map(content, false);
        }
        TwineBuilder builder = TwineBuilder.make(content.length32());
        IntIterator chars = content.codePoints();
        boolean needsQuoting = false;
        boolean followsCR = false;
        while (chars.hasNext()) {
            int c = chars.next();
            if (c == quoteCharacter) {
                needsQuoting = true;
                builder = builder.append(quoteCharacter).append(quoteCharacter);
            } else if (c == separatorChar) {
                needsQuoting = true;
                builder = builder.append(separatorChar);
            } else if (c == '\r') {
                needsQuoting = true;
                builder = builder.append(endOfLine);
            } else if (c == '\n') {
                needsQuoting = true;
                if (!followsCR) {
                    builder = builder.append(endOfLine);
                }
            } else {
                builder = builder.append(c);
            }
            followsCR = c == '\r';
        }
        if (needsQuoting) {
            builder = builder.append(quoteCharacter);
            return new UnicodeChar(quoteCharacter).concat(builder.toUnicodeString());
        } else {
            return builder.toUnicodeString();
        }
    }
}

