using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WindowClausePull : Saxon.Hej.expr.flwor.TuplePull {
        private readonly Saxon.Hej.expr.flwor.WindowClause windowClause;
        private readonly Saxon.Hej.expr.flwor.TuplePull source;
        private Saxon.Hej.om.SequenceIterator baseIterator;
        private bool finished = false;
        private Saxon.Hej.om.Item previous = null;
        private Saxon.Hej.om.Item current = null;
        private Saxon.Hej.om.Item next = null;
        private int position = -1;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.flwor.WindowClause.Window> currentWindows = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.WindowClause.Window>();
        internal WindowClausePull(Saxon.Hej.expr.flwor.TuplePull source, Saxon.Hej.expr.flwor.WindowClause windowClause, Saxon.Hej.expr.XPathContext context)  {
            this.windowClause = windowClause;
            this.source = source;
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.WindowClause.Window earliest = lookForEarliest();
            if ((earliest != null)) {
                processWindow(earliest, context);
                return true;
            }
            if ((finished || (baseIterator == null))) {
                if (source.nextTuple(context)) {
                    baseIterator = windowClause.getSequence().iterate(context);
                    finished = false;
                    previous = null;
                    position = -1;
                    current = null;
                    next = null;
                }
            }
            while (!(finished)) {
                bool autoclose = (windowClause.isTumblingWindow() && (windowClause.getEndCondition() == null));
                Saxon.Hej.om.Item oldPrevious = previous;
                previous = current;
                current = next;
                next = baseIterator.next();
                if ((next == null)) {
                    finished = true;
                }
                position++;
                if ((position>0)) {
                    if (((((windowClause.isSlidingWindow() || currentWindows.Count==0) || autoclose)) && windowClause.matchesStart(previous, current, next, position, context))) {
                        if ((autoclose && !(currentWindows.Count==0))) {
                            Saxon.Hej.expr.flwor.WindowClause.Window w = currentWindows[0];
                            w.endItem = previous;
                            w.endPreviousItem = oldPrevious;
                            w.endNextItem = current;
                            w.endPosition = (position-1);
                            earliest = despatch(w, context);
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
                    bool explicitEndCondition = (windowClause.getEndCondition() != null);
                    bool implicitEndCondition = (finished && windowClause.isIncludeUnclosedWindows());
                    if ((explicitEndCondition || implicitEndCondition)) {
                        System.Collections.Generic.IList<Saxon.Hej.expr.flwor.WindowClause.Window> removals = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.WindowClause.Window>();
                        foreach (Saxon.Hej.expr.flwor.WindowClause.Window w in currentWindows) {
                            if ((!(w.isFinished()) && ((implicitEndCondition || windowClause.matchesEnd(w, previous, current, next, position, context))))) {
                                w.endItem = current;
                                w.endPreviousItem = previous;
                                w.endNextItem = next;
                                w.endPosition = position;
                                if ((earliest == null)) {
                                    earliest = despatch(w, context);
                                    if (w.isDespatched()) {
                                        removals.Add(w);
                                    }
                                }
                            }
                        }
                        foreach (Saxon.Hej.expr.flwor.WindowClause.Window w in removals) {
                            Saxon.Impl.Helpers.ListUtils.Remove(currentWindows, w);
                        }
                    }
                    if ((earliest != null)) {
                        processWindow(earliest, context);
                        return true;
                    }
                }
            }
            return false;
        }
        private Saxon.Hej.expr.flwor.WindowClause.Window despatch(Saxon.Hej.expr.flwor.WindowClause.Window w, Saxon.Hej.expr.XPathContext context) {
            windowClause.checkWindowContents(w);
            return lookForEarliest();
        }
        private Saxon.Hej.expr.flwor.WindowClause.Window lookForEarliest() {
            int earliestStart = System.Int32.MaxValue;
            Saxon.Hej.expr.flwor.WindowClause.Window earliestWindow = null;
            foreach (Saxon.Hej.expr.flwor.WindowClause.Window u in currentWindows) {
                if (((u.startPosition<earliestStart) && !(u.isDespatched()))) {
                    earliestStart = u.startPosition;
                    earliestWindow = u;
                }
            }
            if (((earliestWindow == null) || !(earliestWindow.isFinished()))) {
                return null;
            } else {
                earliestWindow.despatched = true;
                return earliestWindow;
            }
        }
        private void processWindow(Saxon.Hej.expr.flwor.WindowClause.Window w, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.WindowClause clause = windowClause;
            Saxon.Hej.expr.flwor.LocalVariableBinding binding;
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.WINDOW_VAR);
            context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.value.SequenceExtent.makeSequenceExtent(w.contents));
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(w.startItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM_POSITION);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(w.startPosition));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_NEXT_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(w.startNextItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_PREVIOUS_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(w.startPreviousItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(w.endItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM_POSITION);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(w.endPosition));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_NEXT_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(w.endNextItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_PREVIOUS_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), Saxon.Hej.expr.flwor.WindowClause.makeValue(w.endPreviousItem));
            }
            w.despatched = true;
        }
        public override void close() {
            baseIterator.close();
            source.close();
        }
    
    }
}