using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class RawResult : Saxon.Ejavax.xml.transform.Result {
        private string systemId;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> content = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public void append(Saxon.Hej.om.Item item) {
            content.Add(item);
        }
        public Saxon.Hej.om.Sequence getResultSequence() {
            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(content);
        }
    
    }
}