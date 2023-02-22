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
            return new Saxon.Impl.Overrides.LoggingWriter(this);
        }
        public virtual  /* REPLACING public */ Saxon.Ejavax.xml.transform.stream.StreamResult asStreamResult() {
            return new Saxon.Ejavax.xml.transform.stream.StreamResult(asWriter());
        }
    
    }
}