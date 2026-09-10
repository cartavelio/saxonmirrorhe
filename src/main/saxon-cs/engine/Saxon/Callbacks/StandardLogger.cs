using System;

namespace Saxon.Callbacks {
    public class StandardLogger : Saxon.Hej.lib.Logger {
        private int threshold = Saxon.Hej.lib.Logger.INFO;
        private bool mustClose = false;
        public StandardLogger()  {
        }
        public StandardLogger(Saxon.Ejava.io.PrintStream stream)  {
            
        }
        public StandardLogger(System.IO.TextWriter writer)  {
            
        }
        public StandardLogger(Saxon.Ejava.io.File fileName)  {
            
        }

        public void setPrintStream(Saxon.Ejava.io.PrintStream output) {
            
        }

        public void setThreshold(int threshold) {
            
        }

        public int getThreshold() {
            return 0;
        }
        
        public override Saxon.Ejavax.xml.transform.stream.StreamResult asStreamResult() {
            return new Saxon.Ejavax.xml.transform.stream.StreamResult(Console.Error);
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

// Copyright (c) 2021 Saxonica Limited