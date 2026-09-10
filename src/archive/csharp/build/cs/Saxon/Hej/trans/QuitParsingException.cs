using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class QuitParsingException : Saxon.Hej.trans.XPathException {
        private bool notifiedByConsumer = false;
        public QuitParsingException(bool notifiedByConsumer) : base ("The input file has not been read to completion", "SXQP0001") {
            this.notifiedByConsumer = notifiedByConsumer;
        }
        public bool isNotifiedByConsumer() {
            return notifiedByConsumer;
        }
    
    }
}