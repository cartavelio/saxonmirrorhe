using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class LineNumberMap {
        private int[] sequenceNumbers;
        private int[] lineNumbers;
        private int[] columnNumbers;
        private int allocated;
        public LineNumberMap()  {
            sequenceNumbers = new int[200];
            lineNumbers = new int[200];
            columnNumbers = new int[200];
            allocated = 0;
        }
        public void setLineAndColumn(int sequence, int line, int column) {
            if ((sequenceNumbers.Length<=(allocated+1))) {
                sequenceNumbers = Saxon.Ejava.util.Arrays.copyOf(sequenceNumbers, (allocated*2));
                lineNumbers = Saxon.Ejava.util.Arrays.copyOf(lineNumbers, (allocated*2));
                columnNumbers = Saxon.Ejava.util.Arrays.copyOf(columnNumbers, (allocated*2));
            }
            sequenceNumbers[allocated] = sequence;
            lineNumbers[allocated] = line;
            columnNumbers[allocated] = column;
            allocated++;
        }
        public int getLineNumber(int sequence) {
            if ((sequenceNumbers.Length>allocated)) {
                condense();
            }
            int index = Saxon.Ejava.util.Arrays.binarySearch(sequenceNumbers, sequence);
            if ((index<0)) {
                index = (-(index)-1);
                if ((index>(lineNumbers.Length-1))) {
                    index = (lineNumbers.Length-1);
                }
            }
            return lineNumbers[index];
        }
        public int getColumnNumber(int sequence) {
            if ((sequenceNumbers.Length>allocated)) {
                condense();
            }
            int index = Saxon.Ejava.util.Arrays.binarySearch(sequenceNumbers, sequence);
            if ((index<0)) {
                index = (-(index)-1);
                if ((index>=columnNumbers.Length)) {
                    index = (columnNumbers.Length-1);
                }
            }
            return columnNumbers[index];
        }
        private void condense() { lock(this) {
            sequenceNumbers = Saxon.Ejava.util.Arrays.copyOf(sequenceNumbers, allocated);
            lineNumbers = Saxon.Ejava.util.Arrays.copyOf(lineNumbers, allocated);
            columnNumbers = Saxon.Ejava.util.Arrays.copyOf(columnNumbers, allocated);
        }}
    
    }
}