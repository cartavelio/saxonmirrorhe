package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.TwineBuilder;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;
import net.sf.saxon.z.PositiveIntIterator;

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
    private final int quoteCharacter;
    private final boolean trimWhitespace;
    private final NormalizingIterator codepoints;


    /**
     * Create a new CSV parser
     * @param input  the input CSV data.
     * @param csvOptions parsing options.
     * @param handler the content handler to receive parsing events.
     */
    public CsvParser(StringValue input, Map<String, GroundedValue> csvOptions, CsvContentHandler handler) {
        this.codepoints = new NormalizingIterator(input.getUnicodeStringValue().iterate());
        this.handler = handler;
        this.fieldDelimiter = ((StringValue)csvOptions.get("field-delimiter")).getUnicodeStringValue().codePointAt(0);
        this.rowDelimiter = ((StringValue) csvOptions.get("row-delimiter")).getUnicodeStringValue().codePointAt(0);
        this.quoteCharacter = ((StringValue)csvOptions.get("quote-character")).getUnicodeStringValue().codePointAt(0);
        this.trimWhitespace = ((BooleanValue)csvOptions.get("trim-whitespace")).getBooleanValue();
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
        int cp;
        while (true) {
            cp = codepoints.next();
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
        int startLine = codepoints.getLineNumber();
        int startColumn = codepoints.getColumnNumber();
        int cp = codepoints.next(); // the first character after the quote
        while (true) {
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
            if (cp == quoteCharacter) {
                String message = String.format("Invalid CSV data: quote character appears in unquoted field at line %1$d column %2$d",
                                               codepoints.getLineNumber(), codepoints.getColumnNumber());
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

    /**
     * Codepoint iterator that provides the following services: (a) reading ahead, making the current
     * and next codepoints available to the caller; (b) normalizing CR and CRLF line endings to a single LF:;
     * (c) maintaining line number and column number.
     */

    public static class NormalizingIterator implements PositiveIntIterator {

        private final PositiveIntIterator baseIterator;
        private int pendingChar;
        private int currentChar = -1;
        private int lineNumber = 1;
        private int columnNumber = 1;

        /**
         * Create an iterator that provides a single-item lookahead relative to a supplied iterator
         *
         * @param baseIterator the supplied iterator. A lookahead value of -1 indicates
         *                     the end of the stream, so the base iterator should not return
         *                     -1 as an ordinary result.
         */
        public NormalizingIterator(PositiveIntIterator baseIterator) {
            this.baseIterator = baseIterator;
            this.pendingChar = baseIterator.next();
        }

        /**
         * Return the next integer in the sequence.
         *
         * @return the next integer in the sequence
         */
        @Override
        public int next() {
            currentChar = pendingChar;
            if (currentChar != -1) {
                pendingChar = baseIterator.next();
            }
            if (currentChar == '\r') {
                if (pendingChar == '\n') {
                    return next();
                } else {
                    currentChar = '\n';
                }
            }
            if (currentChar == '\n') {
                lineNumber++;
                columnNumber = 1;
            } else {
                columnNumber++;
            }
            return currentChar;
        }

        /**
         * Get the integer that has been most recently read
         *
         * @return the integer most recently returned by the next() method
         */

        public int current() {
            return currentChar;
        }

        /**
         * Return the next integer that will be read, without advancing the position
         *
         * @return the next integer in the stream, or -1 if there are no more to come.
         */
        public int peekAhead() {
            return pendingChar;
        }

        /**
         * Get the current line number
         * @return the line number (starting at 1 for characters before the first newline)
         */

        public int getLineNumber() {
            return lineNumber;
        }

        /**
         * Get the current column number
         * @return the column number (starting at 1 for the first character on a line)
         */

        public int getColumnNumber() {
            return columnNumber;
        }

    }


}
