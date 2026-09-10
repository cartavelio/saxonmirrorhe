using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public class DocumentNumberAllocator {
        private long nextDocumentNumber = 0;
        private long nextStreamedDocumentNumber = -2;
        public long allocateDocumentNumber() { lock(this) {
            return nextDocumentNumber++;
        }}
        public long allocateStreamedDocumentNumber() { lock(this) {
            return nextStreamedDocumentNumber--;
        }}
    
    }
}