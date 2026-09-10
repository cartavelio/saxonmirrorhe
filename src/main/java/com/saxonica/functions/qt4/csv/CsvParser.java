package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.TwineBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;
import net.sf.saxon.z.IntLookaheadIterator;

import java.util.Map;

/**
 * Parser for CSV files, used by functions csv-to-arrays, parse-csv, and csv-to-xml.
 */
public class CsvParser {
    /**
     * Parse an input string containing CSV data into a sequence of ArrayItems (the rows) containing StringValues (the
     * fields), controlled by the passed-in csvOptions. This form constructs a {@link CsvContentHandler} using the
     * passed-in csvOptions.
     *
     * @param input      the CSV data.
     * @param csvOptions the processed options specified by the caller.
     * @return the CSV data represented as a sequence of arrays (<code>array(xs:string)*</code>).
     * @throws XPathException if parsing fails
     */
    public static SequenceIterator parse(StringValue input, Map<String, GroundedValue> csvOptions) throws XPathException{
        return parse(input, csvOptions, new CsvContentHandler(csvOptions));
    }

    /**
     * Parse an input string containing CSV data into a sequence of ArrayItems (the rows) containing StringValues (the
     * fields), controlled by the passed-in csvOptions. This form requires an existing {@link CsvContentHandler} to be
     * used.
     *
     * @param input      the CSV data.
     * @param csvOptions the processed options specified by the caller.
     * @param handler    the content handler to receive CSV parsing events.
     * @return the CSV data represents as a sequence of arrays (<code>array(xs:string)*</code>).
     * @throws XPathException if parsing fails
     */
    public static SequenceIterator parse(StringValue input, Map<String, GroundedValue> csvOptions, CsvContentHandler handler) throws XPathException{
        if (input.isEmpty()) {
            // Special case: a zero-length string represents zero rows, not a single blank row.
            return EmptyIterator.getInstance();
        }
        CsvParser parser = new CsvParser(input, csvOptions, handler);
        return parser.parseInput();
    }

    private static final String ERR_QUOTING = "FOCV0001";

    private final CsvContentHandler handler;
    private final int fieldDelimiter;
    private final int rowDelimiter;
    private final boolean normalizeNewlines;
    private final int quoteCharacter;
    private final boolean trimWhitespace;
    private final IntLookaheadIterator codepoints;
    private int lineNumber = 1;
    private int columnNumber = 0;

    /**
     * Create a new CSV parser
     * @param input  the input CSV data.
     * @param csvOptions parsing options.
     * @param handler the content handler to receive parsing events.
     */
    public CsvParser(StringValue input, Map<String, GroundedValue> csvOptions, CsvContentHandler handler) {
        this.codepoints = new IntLookaheadIterator(input.codePoints());
        this.handler = handler;
        this.fieldDelimiter = ((StringValue)csvOptions.get("field-delimiter")).getUnicodeStringValue().codePointAt(0);
        this.rowDelimiter = ((StringValue) csvOptions.get("row-delimiter")).getUnicodeStringValue().codePointAt(0);
        this.quoteCharacter = ((StringValue)csvOptions.get("quote-character")).getUnicodeStringValue().codePointAt(0);
        this.trimWhitespace = ((BooleanValue)csvOptions.get("trim-whitespace")).getBooleanValue();
        this.normalizeNewlines = ((BooleanValue) csvOptions.get("normalize-newlines")).getBooleanValue();
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws net.sf.saxon.trans.XPathException always
     */
    private static void error(String message, String code) throws XPathException {
        throw new XPathException(message, code);
    }

    /**
     * Parse the input into a sequence of arrays and return them
     * @return the parsed input as an <code>array(xs:string)*</code>
     * @throws XPathException if parsing fails
     */
    public SequenceIterator parseInput() throws XPathException {
        while (true) {
            int cp = codepoints.hasNext() ? codepoints.next() : -1;
            if (cp == '\r' && normalizeNewlines) {
                int cp2 = codepoints.peekAhead();
                if (cp2 == '\n') {
                    codepoints.next();
                }
                cp = '\n';
            }
            if (cp == '\n') {
                lineNumber++;
                columnNumber = 0;
            } else {
                columnNumber++;
            }
            if ((cp == rowDelimiter || cp == -1) && handler.getCurrentRowSize() == 0) {
                handler.blankRow();
                if (cp == rowDelimiter && codepoints.peekAhead() != -1) {
                    continue;
                } else {
                    return handler.getSequenceOfRows();
                }
            }
            if (cp == quoteCharacter) {
                readQuotedField();
                cp = codepoints.current();
            } else {
                cp = readUnquotedField(cp);
            }
            if (cp == rowDelimiter || cp == -1) {
                handler.finishRow(cp);
                if (cp == rowDelimiter) {
                    while (codepoints.peekAhead() == rowDelimiter) {
                        cp = codepoints.next();
                        handler.blankRow();
                    }
                }
            }
            if (cp == -1 || (cp == rowDelimiter && codepoints.peekAhead() == -1)) {
                return handler.getSequenceOfRows();
            }
        }
    }

    /**
     * Read a field delimited by quotation marks. On completion the field will have been
     * added to the {@link CsvContentHandler}, and the input stream is positioned at the
     * first character after the closing quotes.
     * @throws XPathException if mismatched quotes are found
     */
    private void readQuotedField() throws XPathException {
        TwineBuilder field = TwineBuilder.make(128);
        int startLine = lineNumber;
        int startColumn = columnNumber;
        int cp = codepoints.next(); // the first character after the quote
        while (true) {
            if (cp == '\n') {
                lineNumber++;
                columnNumber = 0;
            } else {
                columnNumber++;
            }
            if (cp == quoteCharacter) {
                int nextCp = codepoints.peekAhead();
                if (nextCp == quoteCharacter) {
                    // doubled quotes, just skip one of them
                    cp = codepoints.next();
                } else {
                    // end of quoted field
                    if (!(nextCp == fieldDelimiter || nextCp == rowDelimiter || nextCp == -1)) {
                        String message = String.format(
                                "Invalid CSV data. Quote character on line %1$d column %2$d is not at end of field",
                                startLine, startColumn);
                        error(message, ERR_QUOTING);
                    }
                    handler.appendField(field.toUnicodeString());
                    cp = codepoints.next();
                    break;
                }
            }
            if (cp == -1) {
                String message = String.format(
                        "Invalid CSV data. Quoted field starting on line %1$d column %2$d is not closed",
                        startLine, startColumn);
                error(message, ERR_QUOTING);
                break;
            }
            field = field.append(cp);
            cp = codepoints.next();
        }
    }

    /**
     * Read a field having no quotation marks
     * @param cp The first character of the field, that is the character immediately
     *           after a comma, newline, etc. Note that in the case of an empty field
     *           this may be a delimiter. On completion the field will have been
     *           added to the {@link CsvContentHandler}.
     * @return the delimiter that caused the end of the field to be recognized.
     * @throws XPathException if the field contains a quotation mark
     */
    private int readUnquotedField(int cp) throws XPathException {
        TwineBuilder field = TwineBuilder.make(16);
        boolean started = false;
        while (true) {
            if (cp == '\r' && normalizeNewlines) {
                int cp2 = codepoints.peekAhead();
                if (cp2 == '\n') {
                    codepoints.next();
                }
                cp = '\n';
            }
            if (cp == '\n') {
                lineNumber++;
                columnNumber = 0;
            } else {
                columnNumber++;
            }

            if (cp == quoteCharacter) {
                String message = String.format("Invalid CSV data: quote character appears in unquoted field at line %1$d column %2$d", lineNumber, columnNumber);
                error(message, ERR_QUOTING);
            }
            if (cp == fieldDelimiter || cp == rowDelimiter || cp == -1) {
                handler.appendField(field.toUnicodeString());
                break;
            } else {
                if (trimWhitespace && !started && Whitespace.isWhite(cp)) {
                    cp = codepoints.next();
                    continue;
                }
                started = true;
                field = field.append(cp);
                cp = codepoints.next();
            }
        }
        return cp;

    }


}
