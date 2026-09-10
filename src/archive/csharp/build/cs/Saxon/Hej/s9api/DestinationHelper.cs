using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class DestinationHelper {
        private readonly Saxon.Hej.s9api.Destination helpee;
        private readonly System.Collections.Generic.IList<Saxon.Hej.s9api.Action> listeners = new System.Collections.Generic.List<Saxon.Hej.s9api.Action>();
        public DestinationHelper(Saxon.Hej.s9api.Destination helpee)  {
            this.helpee = helpee;
        }
        public void onClose(Saxon.Hej.s9api.Action listener) {
            listeners.Add(listener);
        }
        public void closeAndNotify() {
            helpee.close();
            foreach (Saxon.Hej.s9api.Action action in listeners) {
                /*Delegate.act()*/action();
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.s9api.Action> getListeners() {
            return listeners;
        }
    
    }
}