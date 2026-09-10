using System;

namespace Saxon.Callbacks {
    public class StandardLogger : net.sf.saxon.lib.Logger {
        private int threshold = net.sf.saxon.lib.Logger.INFO;
        private bool mustClose = false;
        public StandardLogger()  {
        }
        public StandardLogger(java.io.PrintStream stream)  {
            
        }
        public StandardLogger(System.IO.TextWriter writer)  {
            
        }
        public StandardLogger(java.io.File fileName)  {
            
        }

        public void setPrintStream(java.io.PrintStream output) {
            
        }

        public void setThreshold(int threshold) {
            
        }

        public int getThreshold() {
            return 0;
        }
        
        public override javax.xml.transform.stream.StreamResult asStreamResult() {
            return new javax.xml.transform.stream.StreamResult(Console.Error);
        }
        public override void println(string message, int severity) {
            if ((severity>=threshold)) {
                Console.Error.Write((message+"\n"));
            }
        }
        public override void close() {
            if (mustClose) {
                // no action
            }
        }
    
    }
}