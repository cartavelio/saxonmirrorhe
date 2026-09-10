using System;
using System.IO;
using Saxon.Ejava.lang;

namespace Saxon.Callbacks {
    public abstract class StandardLogger : Saxon.Hej.lib.Logger {
        private int threshold = Saxon.Hej.lib.Logger.INFO;
        
        public static Saxon.Hej.lib.Logger makeLogger() {
            return new TextWriterLogger(Console.Error);
        }
        public static Saxon.Hej.lib.Logger makeLogger(Saxon.Ejava.io.PrintStream stream) {
            return new PrintStreamLogger(stream);
        }
        public static Saxon.Hej.lib.Logger makeLogger(System.IO.TextWriter writer) {
            return new TextWriterLogger(writer);
        }
        public static Saxon.Hej.lib.Logger makeLogger(Saxon.Ejava.io.File file) {
            return new TextWriterLogger(new StreamWriter(file.getPath()));
        }

        public void setPrintStream(Saxon.Ejava.io.PrintStream output) {
            
        }

        public void setThreshold(int threshold) {
            this.threshold = threshold;
        }

        public int getThreshold() {
            return this.threshold;
        }
        


        private class PrintStreamLogger : StandardLogger {

            private readonly Saxon.Ejava.io.PrintStream stream;

            public PrintStreamLogger(Saxon.Ejava.io.PrintStream stream) {
                this.stream = stream;
            }
            
            public override void println(string message, int severity) {
                if ((severity>=threshold)) {
                    stream.Println(message);
                }
            }
            public override void close() {
                stream.close();
            }
            
            public override Saxon.Ejavax.xml.transform.stream.StreamResult asStreamResult() {
                return new Saxon.Ejavax.xml.transform.stream.StreamResult(stream.getOutputStream());
            }

        }
        
        private class TextWriterLogger : StandardLogger {

            private readonly System.IO.TextWriter writer;

            public TextWriterLogger(System.IO.TextWriter writer) {
                this.writer = writer;
            }
            
            public override void println(string message, int severity) {
                if ((severity>=threshold)) {
                    writer.Write(message+"\n");
                }
            }
            public override void close() {
                writer.Close();
            }
            
            public override Saxon.Ejavax.xml.transform.stream.StreamResult asStreamResult() {
                throw new UnsupportedOperationException();
                //return new Saxon.Ejavax.xml.transform.stream.StreamResult(writer.getOutputStream());
            }

        }
        
    
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited