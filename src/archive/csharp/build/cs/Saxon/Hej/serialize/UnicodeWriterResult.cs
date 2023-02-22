using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class UnicodeWriterResult : Saxon.Ejavax.xml.transform.Result {
        private readonly Saxon.Hej.str.UnicodeWriter unicodeWriter;
        private string systemId;
        public UnicodeWriterResult(Saxon.Hej.str.UnicodeWriter unicodeWriter, string systemId)  {
            this.systemId = systemId;
            this.unicodeWriter = unicodeWriter;
        }
        public string getSystemId() {
            return systemId;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public Saxon.Hej.str.UnicodeWriter getUnicodeWriter() {
            return unicodeWriter;
        }
        public override string ToString() {
            return base.ToString();
        }
    
    }
}