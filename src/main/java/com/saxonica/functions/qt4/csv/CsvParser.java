package com.saxonica.functions.qt4.csv;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

public class CsvParser {
    public static ParsedCSV parse(StringValue input) throws XPathException{
        CsvOptions csvOptions = CsvOptions.DEFAULT;
        return parse(input, csvOptions);
    }

    public static ParsedCSV parse(StringValue input, CsvOptions csvOptions) throws XPathException{
        return parse(input, csvOptions, new CsvContentHandler(csvOptions));
    }

    public static ParsedCSV parse(StringValue input, CsvOptions csvOptions, CsvContentHandler handler) throws XPathException{
        CsvParser parser = new CsvParser(input, csvOptions, handler);
        return parser.parseInput();
    }

    /**
     * Primarily for handling the CRLF/CR/LF problem. The assumption in the code is that the delimiters array is
     * ordered in descending order of length.
     */
    private class RowDelimiters {
        private final RowDelimiter[] delimiters;
        public RowDelimiters(String[] delimiters) {
            this.delimiters = new RowDelimiter[delimiters.length];
            for (int i = 0; i < delimiters.length; i++) {
                this.delimiters[i] = new RowDelimiter(delimiters[i].codePoints().toArray());
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

        public boolean hasNext() {
            if (readPosition == writtenPosition ) return codepoints.hasNext();
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

        public int cursor() {
            return buffer[readPosition];
        }

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

        public void back() {
            if (readPosition == 0) {
                readPosition = bufferEndPosition;
            } else {
                readPosition--;
            }
        }
    }

    private static final String ERR_QUOTING = "FOCV0001";

    private final CsvOptions options;
    private final CsvContentHandler handler;
    private int[] fieldDelimiter;
    private RowDelimiters rowDelimiters;
    private int quoteCharacter;
    private final CodepointsRewindableIterator codepoints;
    private final StringBuilder currentField = new StringBuilder();
    private int currentCodePoint;
    private boolean needNewRow = false;
    private boolean processingField = false;
    private boolean inQuotedField = false;
    private boolean atFieldStart = true;

    public CsvParser(StringValue input, CsvOptions csvOptions, CsvContentHandler handler) {
        this.codepoints = new CodepointsRewindableIterator(input.codePoints());
        this.options = csvOptions;
        this.handler = handler;
        this.fieldDelimiter = options.getFieldDelimiter().codePoints().toArray();
        this.rowDelimiters = new RowDelimiters(options.getRowDelimiters());
        this.quoteCharacter = options.getQuoteCharacter().codePoints().toArray()[0];
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

    public ParsedCSV parseInput() throws XPathException {
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
        return handler.parsedCsv();
    }

    private void checkStartOfQuotedField() {
        if (atFieldStart && codepoints.cursor() == quoteCharacter) {
            this.inQuotedField = true;
            codepoints.next();
        }
    }
    private void addToCurrentField() throws XPathException{
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
