using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class EventSource : Saxon.Hej.lib.ActiveSource {
        private string systemId;
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public abstract void deliver(Saxon.Hej.@event.Receiver @out, Saxon.Hej.lib.ParseOptions options);
    
    }
}