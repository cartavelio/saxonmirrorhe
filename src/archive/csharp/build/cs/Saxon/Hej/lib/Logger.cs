using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public abstract class Logger {
        public const int INFO = 0;
        public const int WARNING = 1;
        public const int ERROR = 2;
        public const int DISASTER = 3;
        private bool unicodeAware = false;
        public void info(string message) {
            println(message, INFO);
        }
        public void warning(string message) {
            println(message, WARNING);
        }
        public void error(string message) {
            println(message, ERROR);
        }
        public void disaster(string message) {
            println(message, DISASTER);
        }
        public abstract void println(string message, int severity);
        public virtual  /* REPLACING public */ void close() {
        }
        public bool isUnicodeAware() {
            return unicodeAware;
        }
        public void setUnicodeAware(bool aware) {
            unicodeAware = aware;
        }
        public System.IO.TextWriter asWriter() {
            return new Saxon.Hej.lib.Logger.LoggingWriter(this);
        }
        private class LoggingWriter : System.IO.TextWriter {
    // Injecting C# code
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    // End of injected C# code
            private readonly Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
            private readonly Saxon.Hej.lib.Logger logger;
            public LoggingWriter(Saxon.Hej.lib.Logger logger)  {
                this.logger = logger;
            }
            public void write(char[] cbuf, int off, int len) {
                for (int i = 0; (i<len); i++) {
                    char ch = cbuf[(off+i)];
                    if ((ch == '\n')) {
                        logger.println(builder.ToString(), INFO);
                        builder.setLength(0);
                    } else {
                        builder.append(ch);
                    }
                }
            }
            public void flush() {
                if ((builder.length()>0)) {
                    logger.println(builder.ToString(), INFO);
                    builder.setLength(0);
                }
            }
            public void close() {
                flush();
            }
        
        }
        public virtual  /* REPLACING public */ Saxon.Ejavax.xml.transform.stream.StreamResult asStreamResult() {
            return new Saxon.Ejavax.xml.transform.stream.StreamResult(asWriter());
        }
    
    }
}