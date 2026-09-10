using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class CloseNotifier : Saxon.Hej.@event.ProxyReceiver {
        private readonly System.Collections.Generic.IList<Saxon.Hej.s9api.Action> actionList;
        public CloseNotifier(Saxon.Hej.@event.Receiver next, System.Collections.Generic.IList<Saxon.Hej.s9api.Action> actionList) : base (next) {
            this.actionList = actionList;
        }
        public override void close() {
            base.close();
            try {
                if ((actionList != null)) {
                    foreach (Saxon.Hej.s9api.Action action in actionList) {
                        /*Delegate.act()*/action();
                    }
                }
            } catch (Saxon.Api.SaxonApiException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
    
    }
}