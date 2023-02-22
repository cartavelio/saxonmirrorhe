using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class UnionIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private class Intake {
            public Saxon.Hej.om.SequenceIterator iter;
            public Saxon.Hej.om.NodeInfo nextNode;
            public Intake(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.om.NodeInfo nextNode)  {
                this.iter = iter;
                this.nextNode = nextNode;
            }
        
        }
        private class IntakeComparer : System.Collections.Generic.Comparer<Saxon.Hej.expr.UnionIterator.Intake> {
            private readonly System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> itemOrderComparer;
            public IntakeComparer(System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> itemOrderComparer)  {
                this.itemOrderComparer = itemOrderComparer;
            }
            public int compare(Saxon.Hej.expr.UnionIterator.Intake o1, Saxon.Hej.expr.UnionIterator.Intake o2) {
                return itemOrderComparer.Compare(o1.nextNode, o2.nextNode);
            }
            // Extra method to make the class a C# Comparer
            public override int Compare(Saxon.Hej.expr.UnionIterator.Intake a, Saxon.Hej.expr.UnionIterator.Intake b) {
                return compare(a, b);
            }
        
        }
        private readonly System.Collections.Generic.SortedSet<Saxon.Hej.expr.UnionIterator.Intake> intakes;
        public UnionIterator(System.Collections.Generic.IList<Saxon.Hej.om.SequenceIterator> inputs, System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer)  {
            System.Collections.Generic.Comparer<Saxon.Hej.expr.UnionIterator.Intake> comp = new Saxon.Hej.expr.UnionIterator.IntakeComparer(comparer);
            intakes = new System.Collections.Generic.SortedSet<Saxon.Hej.expr.UnionIterator.Intake>(comp);
            foreach (Saxon.Hej.om.SequenceIterator seq in inputs) {
                Saxon.Hej.om.NodeInfo next = (Saxon.Hej.om.NodeInfo)(seq.next());
                while ((next != null)) {
                    bool added = intakes.Add(new Saxon.Hej.expr.UnionIterator.Intake(seq, next));
                    if (added) {
                        break ;
                    } else {
                        next = (Saxon.Hej.om.NodeInfo)(seq.next());
                    }
                }
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return !(intakes.Count==0);
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.expr.UnionIterator.Intake nextIntake = Saxon.Impl.Helpers.SetUtils.PollFirst(intakes);
            if ((nextIntake != null)) {
                Saxon.Hej.om.SequenceIterator iter = nextIntake.iter;
                Saxon.Hej.om.NodeInfo nextNode = (Saxon.Hej.om.NodeInfo)(iter.next());
                while ((nextNode != null)) {
                    bool added = false;
                    if (!(nextNode.isSameNodeInfo(nextIntake.nextNode))) {
                        Saxon.Hej.expr.UnionIterator.Intake replacement = new Saxon.Hej.expr.UnionIterator.Intake(iter, nextNode);
                        added = intakes.Add(replacement);
                    }
                    if (added) {
                        break ;
                    } else {
                        nextNode = (Saxon.Hej.om.NodeInfo)(iter.next());
                    }
                }
                return nextIntake.nextNode;
            }
            return null;
        }
        public void close() {
            foreach (Saxon.Hej.expr.UnionIterator.Intake intake in intakes) {
                intake.iter.close();
            }
        }
    
    }
}