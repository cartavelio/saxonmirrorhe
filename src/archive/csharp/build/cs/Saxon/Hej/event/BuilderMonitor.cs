using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class BuilderMonitor : Saxon.Hej.@event.ProxyReceiver {
        public BuilderMonitor(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public abstract void markNextNode(int nodeKind);
        public abstract Saxon.Hej.om.NodeInfo getMarkedNode();
    
    }
}