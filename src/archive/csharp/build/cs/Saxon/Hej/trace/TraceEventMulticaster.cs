using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class TraceEventMulticaster : Saxon.Hej.lib.TraceListener {
        protected internal readonly Saxon.Hej.lib.TraceListener a, b;
        protected internal TraceEventMulticaster(Saxon.Hej.lib.TraceListener a, Saxon.Hej.lib.TraceListener b)  {
            this.a = a;
            this.b = b;
        }
        public void setOutputDestination(Saxon.Hej.lib.Logger stream) {
            a.setOutputDestination(stream);
            b.setOutputDestination(stream);
        }
        protected internal Saxon.Hej.lib.TraceListener remove(Saxon.Hej.lib.TraceListener oldl) {
            if ((oldl == a)) {
                return b;
            }
            if ((oldl == b)) {
                return a;
            }
            Saxon.Hej.lib.TraceListener a2 = removeInternal(a, oldl);
            Saxon.Hej.lib.TraceListener b2 = removeInternal(b, oldl);
            if (((a2 == a) && (b2 == b))) {
                return this;
            }
            return addInternal(a2, b2);
        }
        public void open(Saxon.Hej.Controller controller) {
            a.open(controller);
            b.open(controller);
        }
        public void close() {
            a.close();
            b.close();
        }
        public void enter(Saxon.Hej.trace.Traceable element, System.Collections.Generic.IDictionary<string,System.Object> properties, Saxon.Hej.expr.XPathContext context) {
            a.enter(element, properties, context);
            b.enter(element, properties, context);
        }
        public void leave(Saxon.Hej.trace.Traceable element) {
            a.leave(element);
            b.leave(element);
        }
        public void startCurrentItem(Saxon.Hej.om.Item item) {
            a.startCurrentItem(item);
            b.startCurrentItem(item);
        }
        public void endCurrentItem(Saxon.Hej.om.Item item) {
            a.endCurrentItem(item);
            b.endCurrentItem(item);
        }
        public void startRuleSearch() {
            a.startRuleSearch();
            b.startRuleSearch();
        }
        public void endRuleSearch(System.Object rule, Saxon.Hej.trans.SimpleMode mode, Saxon.Hej.om.Item item) {
            a.endRuleSearch(rule, mode, item);
            b.endRuleSearch(rule, mode, item);
        }
        public static Saxon.Hej.lib.TraceListener add(Saxon.Hej.lib.TraceListener a, Saxon.Hej.lib.TraceListener b) {
            return addInternal(a, b);
        }
        public static Saxon.Hej.lib.TraceListener remove(Saxon.Hej.lib.TraceListener l, Saxon.Hej.lib.TraceListener oldl) {
            return removeInternal(l, oldl);
        }
        protected internal static Saxon.Hej.lib.TraceListener addInternal(Saxon.Hej.lib.TraceListener a, Saxon.Hej.lib.TraceListener b) {
            if ((a == null)) {
                return b;
            }
            if ((b == null)) {
                return a;
            }
            return new Saxon.Hej.trace.TraceEventMulticaster(a, b);
        }
        protected internal static Saxon.Hej.lib.TraceListener removeInternal(Saxon.Hej.lib.TraceListener l, Saxon.Hej.lib.TraceListener oldl) {
            if (((l == oldl) || (l == null))) {
                return null;
            } else {
                if ((l is Saxon.Hej.trace.TraceEventMulticaster)) {
                    return ((Saxon.Hej.trace.TraceEventMulticaster)(l)).remove(oldl);
                } else {
                    return l;
                }
            }
        }
    
        /* Expanding default method endRuleSearch from interface TraceListener */
        public virtual void endRuleSearch(System.Object rule, Saxon.Hej.trans.Mode mode, Saxon.Hej.om.Item item) {
        }

    }
}