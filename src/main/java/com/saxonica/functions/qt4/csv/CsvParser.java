package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

import java.util.Map;

public class CsvParser {
    /**
     * Parse an input string containing CSV data into a sequence of ArrayItems (the rows) containing StringValues (the
     * fields), controlled by the passed-in csvOptions. This form constructs a {@link CsvContentHandler} using the
     * passed-in csvOptions.
     *
     * @param input      the CSV data.
     * @param csvOptions the processed options specified by the caller.
     * @return the CSV data represents as a sequence of arrays (<code>array(xs:string)*</code>).
     * @throws XPathException
     */
    public static Sequence parse(StringValue input, Map<String, GroundedValue> csvOptions) throws XPathException{
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
     * @throws XPathException
     */
    public static Sequence parse(StringValue input, Map<String, GroundedValue> csvOptions, CsvContentHandler handler) throws XPathException{
        CsvParser parser = new CsvParser(input, csvOptions, handler);
        return parser.parseInput();
    }

    /**
     * Primarily for handling the CRLF/CR/LF problem. The assumption in the code is that the delimiters array is ordered
     * in descending order of length.
     */
    private class RowDelimiters {
        private final RowDelimiter[] delimiters;

        public RowDelimiters(GroundedValue delimiters) {
            this.delimiters = new RowDelimiter[delimiters.getLength()];
            for (int i = 0; i < delimiters.getLength(); i++) {
                this.delimiters[i] = new RowDelimiter(delimiters.itemAt(i).toString().codePoints().toArray());
            }
        }

        public RowDelimiter[] toArray() {
            return delimiters;
        }
    }

    private class RowDelimiter {
        private final int[] delimiter;
        private final boolean singleCodepoint;

        public RowDelimiter(int[] delimiter) {
            this.delimiter = delimiter;
            singleCodepoint = delimiter.length == 1;
        }

        public int[] getDelimiter() {
            return delimiter;
        }

        public boolean isSingleCodepoint() {
            return singleCodepoint;
        }

        public boolean matchFirst(int codePoint) {
            return codePoint == delimiter[0];
        }

        public boolean matchLast(int codePoint) {
            return codePoint == delimiter[1];
        }
    }

    /**
     * We are parsing through the CSV data one codepoint at a time. This class permits backtracking when a potentially
     * multi-character token is encountered (like a quote-within-a-quoted-field). It acts mostly like an IntIterator,
     * but provides a method to access the current codepoint without incrementing the cursor's position within the
     * string.
     */
    private class CodepointsRewindableIterator {
        private final IntIterator codepoints;
        private final int bufferCapacity = 8;
        private final int[] buffer = new int[bufferCapacity];
        private final int bufferEndPosition = bufferCapacity - 1;
        private boolean bufferFilled = false;

        private int readPosition = -1;
        private int writtenPosition = -1;
        private int writePosition = 0;

        CodepointsRewindableIterator(IntIterator codepoints) {
            this.codepoints = codepoints;
        }

        /**
         * Does the iterator contain more unread data?
         * @return whether it does or not.
         */
        public boolean hasNext() {
            if (readPosition == writtenPosition) {
                return codepoints.hasNext();
            } else if (bufferFilled && writtenPosition == 0) {
                return readPosition > writtenPosition;
            }
            return readPosition < writtenPosition;
        }

        private boolean readIsBehind() {
            if (writtenPosition == 0) return readPosition > writtenPosition;
            return readPosition < writtenPosition;
        }

        private void incrementReadPosition() {
            if (readPosition == bufferEndPosition) {
                readPosition = 0;
            } else {
                readPosition++;
            }
        }

        private void incrementWrittenPositions() {
            if (writePosition == 0) {
                writePosition++;
                writtenPosition = 0;
            } else if (writePosition == bufferEndPosition) {
                if (!bufferFilled) bufferFilled = true;
                writePosition = 0;
                writtenPosition++;
            } else {
                writePosition++;
                writtenPosition++;
            }
        }

        /**
         * @return the codepoint at the current read position within the string
         */
        public int cursor() {
            return buffer[readPosition];
        }

        /**
         * Read the next character from the string, and increment the position of the cursor.
         *
         * @return the next character
         */
        public int next() {
            if (readIsBehind()) {
                incrementReadPosition();
                return buffer[readPosition];
            } else {
                int codepoint = codepoints.next();
                buffer[writePosition] = codepoint;
                incrementWrittenPositions();
                incrementReadPosition();
                return codepoint;
            }
        }

        /**
         * Move the cursor back within the string to the previous codepoint.
         */
        public void back() {
            if (readPosition == 0) {
                readPosition = bufferEndPosition;
            } else {
                readPosition--;
            }
        }
    }

    private static final String ERR_QUOTING = "FOCV0001";

    private final CsvContentHandler handler;
    private int[] fieldDelimiter;
    private RowDelimiters rowDelimiters;
    private int quoteCharacter;
    private final CodepointsRewindableIterator codepoints;
    private final StringBuilder currentField = new StringBuilder();
    private boolean needNewRow = false;
    private boolean processingField = false;
    private boolean inQuotedField = false;
    private boolean atFieldStart = true;

    /**
     * Create a new CSV parser
     * @param input  the input CSV data.
     * @param csvOptions parsing options.
     * @param handler the content handler to receive parsing events.
     */
    public CsvParser(StringValue input, Map<String, GroundedValue> csvOptions, CsvContentHandler handler) {
        this.codepoints = new CodepointsRewindableIterator(input.codePoints());
        this.handler = handler;
        this.fieldDelimiter = csvOptions.get("field-delimiter").toString().codePoints().toArray();
        this.rowDelimiters = new RowDelimiters(csvOptions.get("row-delimiter"));
        this.quoteCharacter = csvOptions.get("quote-character").toString().codePoints().toArray()[0];
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
     * @return the parsed input as <code>array(xs:string)*</code>
     * @throws XPathException
     */
    public Sequence parseInput() throws XPathException {
        while (codepoints.hasNext()) {
            codepoints.next();
            checkStartOfQuotedField();
            if (endOfField()) {
                finishField();
            } else if (endOfRow()) {
                finishRow();
            } else {
                addToCurrentField();
            }
        }
        finishUnfinishedField();
        return handler.getSequence();
    }

    private void checkStartOfQuotedField() {
        if (atFieldStart && codepoints.cursor() == quoteCharacter) {
            this.inQuotedField = true;
            codepoints.next();
        }
    }

    private void addToCurrentField() throws XPathException {
        if (!processingField) processingField = true;
        if (atFieldStart) atFieldStart = false;

        if (inQuotedField) {
            if (codepoints.cursor() == quoteCharacter) {
                codepoints.next();
                if (codepoints.cursor() != quoteCharacter) {
                    error("Illegal CSV quoting", ERR_QUOTING);
                }
            }
        }

        currentField.appendCodePoint(codepoints.cursor());
    }


    private boolean endOfField() {
        if (inQuotedField) {
            if (codepoints.cursor() == quoteCharacter && codepoints.hasNext()) {
                codepoints.next();
                if (codepoints.cursor() == fieldDelimiter[0]) return true;
                codepoints.back();
            }
            return false;
        } else {
            return codepoints.cursor() == fieldDelimiter[0];
        }
    }

    private boolean checkEndOfRow() {
        for (RowDelimiter rowDelimiter : rowDelimiters.toArray()) {
            if (rowDelimiter.matchFirst(codepoints.cursor())) {
                if (rowDelimiter.isSingleCodepoint()) return true;
                if (!codepoints.hasNext()) continue;
                codepoints.next();
                if (rowDelimiter.matchLast(codepoints.cursor())) {
                    return true;
                }
                codepoints.back();
            }
        }
        return false;
    }

    private boolean endOfRow() {
        if (inQuotedField) {
            if (codepoints.cursor() == quoteCharacter) {
                if (!codepoints.hasNext()) return true;
                codepoints.next();
                if (checkEndOfRow()) {
                    return true;
                }
                codepoints.back();
            }
            return false;
        }
        return checkEndOfRow();
    }

    private void finishField() {
        if (needNewRow) {
            handler.startNewRow();
            needNewRow = false;
        }
        handler.appendField(currentField.toString());
        currentField.delete(0, currentField.length());
        processingField = false;
        inQuotedField = false;
        atFieldStart = true;
    }

    private void finishRow() {
        finishField();
        needNewRow = true;
    }

    private void finishUnfinishedField() {
        if (processingField) finishField();
    }
}
