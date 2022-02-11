using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Trace : Saxon.Hej.functions.SystemFunction, Saxon.Hej.trace.Traceable {
        internal Saxon.Hej.s9api.Location location = Saxon.Hej.expr.parser.Loc.NONE;
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getSpecialProperties();
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getCardinality();
        }
        public void notifyListener(string label, Saxon.Hej.om.Sequence val, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IDictionary<string,System.Object> info = new System.Collections.Generic.Dictionary<string, System.Object>();
            Saxon.Impl.Helpers.MapUtils.Put(info, "label", label);
            Saxon.Impl.Helpers.MapUtils.Put(info, "value", val);
            Saxon.Hej.lib.TraceListener listener = context.getController().getTraceListener();
            listener.enter(this, info, context);
            listener.leave(this);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression e = base.makeFunctionCall(arguments);
            location = e.getLocation();
            return e;
        }
        public static void traceItem(Saxon.Hej.om.Item val, string label, Saxon.Hej.lib.Logger @out) {
            if ((val == null)) {
                @out.info(label);
            } else {
                if ((val is Saxon.Hej.om.NodeInfo)) {
                    @out.info(((((label+": ")+Saxon.Hej.type.Type.displayTypeName(val))+": ")+Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(val))));
                } else {
                    if ((val is Saxon.Hej.value.AtomicValue)) {
                        @out.info(((((label+": ")+Saxon.Hej.type.Type.displayTypeName(val))+": ")+val.getUnicodeStringValue()));
                    } else {
                        if (((val is Saxon.Hej.ma.arrays.ArrayItem) || (val is Saxon.Hej.ma.map.MapItem))) {
                            @out.info(((label+": ")+val.toShortString()));
                        } else {
                            if ((val is Saxon.Hej.om.Function)) {
                                Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.om.Function)(val)).getFunctionName();
                                @out.info(((((label+": function ")+(((name == null) ? "(anon)" : name.getDisplayName())))+"#")+((Saxon.Hej.om.Function)(val)).getArity()));
                            } else {
                                if ((val.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                                    System.Object obj = ((Saxon.Hej.value.ObjectValue<object>)(val)).getObject();
                                    @out.info(((((label+": ")+obj.GetType().Name)+" = ")+Saxon.Hej.trans.Err.truncate30(Saxon.Hej.str.StringView.tidy(obj.ToString()))));
                                } else {
                                    @out.info(((label+": ")+val.toShortString()));
                                }
                            }
                        }
                    }
                }
            }
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return location;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            string label = ((arguments.Length == 1) ? "*" : arguments[1].head().getStringValue());
            if (controller.isTracing()) {
                Saxon.Hej.om.Sequence value = Saxon.Hej.om.SequenceTool.toGroundedValue(arguments[0].iterate());
                notifyListener(label, value, context);
                return value;
            } else {
                Saxon.Hej.lib.Logger @out = controller.getTraceFunctionDestination();
                if ((@out == null)) {
                    return arguments[0];
                } else {
                    return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Hej.functions.Trace.TracingIterator(arguments[0].iterate(), label, @out));
                }
            }
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return null;
        }
        private class TracingIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly string label;
            private readonly Saxon.Hej.lib.Logger @out;
            private bool empty = true;
            private int position = 0;
            public TracingIterator(Saxon.Hej.om.SequenceIterator @base, string label, Saxon.Hej.lib.Logger @out)  {
                this.@base = @base;
                this.label = label;
                this.@out = @out;
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.Item n = @base.next();
                position++;
                if ((n == null)) {
                    if (empty) {
                        traceItem(null, (label+": empty sequence"), @out);
                    }
                } else {
                    traceItem(n, (((label+" [")+position)+']'), @out);
                    empty = false;
                }
                return n;
            }
            public void close() {
                @base.close();
            }
            public void discharge() {
                @base.discharge();
            }
        
        }
        public override string getStreamerName() {
            return "Trace";
        }
    
        /* Expanding default method gatherProperties from interface Traceable */
        public virtual void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
        }

    }
}