using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WindowClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.flwor.WindowClause windowClause;
        private readonly Saxon.Hej.expr.flwor.TuplePush destination;
        internal System.Collections.Generic.IList<Saxon.Hej.expr.flwor.WindowClause.Window> currentWindows = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.WindowClause.Window>();
        public WindowClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.WindowClause windowClause) : base (outputter) {
            this.windowClause = windowClause;
            this.destination = destination;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            currentWindows = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.WindowClause.Window>();
            bool autoclose = (windowClause.isTumblingWindow() && (windowClause.getEndCondition() == null));
            Saxon.Hej.om.Item previousPrevious = null;
            Saxon.Hej.om.Item previous = null;
            Saxon.Hej.om.Item current = null;
            Saxon.Hej.om.Item next = null;
            int position = -1;
            Saxon.Hej.om.SequenceIterator iter = windowClause.getSequence().iterate(context);
            bool finished = false;
            while (!(finished)) {
                previousPrevious = previous;
                previous = current;
                current = next;
                next = iter.next();
                if ((next == null)) {
                    finished = true;
                }
                position++;
                if ((position>0)) {
                    if (((((windowClause.isSlidingWindow() || currentWindows.Count==0) || autoclose)) && windowClause.matchesStart(previous, current, next, position, context))) {
                        if ((autoclose && !(currentWindows.Count==0))) {
                            Saxon.Hej.expr.flwor.WindowClause.Window w = currentWindows[0];
                            w.endItem = previous;
                            w.endPreviousItem = previousPrevious;
                            w.endNextItem = current;
                            w.endPosition = (position-1);
                            despatch(w, getOutputter(), context);
                            currentWindows.Clear();
                        }
                        Saxon.Hej.expr.flwor.WindowClause.Window window = new Saxon.Hej.expr.flwor.WindowClause.Window();
                        window.startPosition = position;
                        window.startItem = current;
                        window.startPreviousItem = previous;
                        window.startNextItem = next;
                        window.contents = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        currentWindows.Add(window);
                    }
                    foreach (Saxon.Hej.expr.flwor.WindowClause.Window active in currentWindows) {
                        if (!(active.isFinished())) {
                            active.contents.Add(current);
                        }
                    }
                    if ((windowClause.getEndCondition() != null)) {
                        System.Collections.Generic.IList<Saxon.Hej.expr.flwor.WindowClause.Window> removals = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.WindowClause.Window>();
                        foreach (Saxon.Hej.expr.flwor.WindowClause.Window w in currentWindows) {
                            if ((!(w.isFinished()) && windowClause.matchesEnd(w, previous, current, next, position, context))) {
                                w.endItem = current;
                                w.endPreviousItem = previous;
                                w.endNextItem = next;
                                w.endPosition = position;
                                despatch(w, getOutputter(), context);
                                if (w.isDespatched()) {
                                    removals.Add(w);
                                }
                            }
                        }
                        foreach (Saxon.Hej.expr.flwor.WindowClause.Window w in removals) {
                            Saxon.Impl.Helpers.ListUtils.Remove(currentWindows, w);
                        }
                    }
                }
            }
            if (windowClause.isIncludeUnclosedWindows()) {
                foreach (Saxon.Hej.expr.flwor.WindowClause.Window w in currentWindows) {
                    w.endItem = current;
                    w.endPreviousItem = previous;
                    w.endNextItem = null;
                    w.endPosition = position;
                    despatch(w, getOutputter(), context);
                }
            }
        }
        private void despatch(Saxon.Hej.expr.flwor.WindowClause.Window w, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            windowClause.checkWindowContents(w);
            while (true) {
                int earliestStart = System.Int32.MaxValue;
                Saxon.Hej.expr.flwor.WindowClause.Window earliestWindow = null;
                foreach (Saxon.Hej.expr.flwor.WindowClause.Window u in currentWindows) {
                    if (((u.startPosition<earliestStart) && !(u.isDespatched()))) {
                        earliestStart = u.startPosition;
                        earliestWindow = u;
                    }
                }
                if (((earliestWindow == null) || !(earliestWindow.isFinished()))) {
                    return ;
                } else {
                    Saxon.Hej.expr.flwor.WindowClause clause = windowClause;
                    Saxon.Hej.expr.flwor.LocalVariableBinding binding;
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.WINDOW_VAR);
                    context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.value.SequenceExtent.makeSequenceExtent(earliestWindow.contents));
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(earliestWindow.startItem));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM_POSITION);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(earliestWindow.startPosition));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_NEXT_ITEM);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(earliestWindow.startNextItem));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_PREVIOUS_ITEM);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(earliestWindow.startPreviousItem));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(earliestWindow.endItem));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM_POSITION);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(earliestWindow.endPosition));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_NEXT_ITEM);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(earliestWindow.endNextItem));
                    }
                    binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_PREVIOUS_ITEM);
                    if ((binding != null)) {
                        context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(earliestWindow.endPreviousItem));
                    }
                    destination.processTuple(context);
                    earliestWindow.despatched = true;
                }
            }
        }
        public override void close() {
            currentWindows = null;
            destination.close();
        }
    
    }
}