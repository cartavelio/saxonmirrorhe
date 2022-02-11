using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class FocusTrackingIterator : Saxon.Hej.om.FocusIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.expr.LastPositionFinder {
        private Saxon.Hej.om.SequenceIterator @base;
        private Saxon.Hej.om.Item curr;
        private int pos = 0;
        private int last = -1;
        private Saxon.Hej.om.FocusTrackingIterator.SiblingMemory siblingMemory;
        public static Saxon.Hej.om.FocusTrackingIterator track(Saxon.Hej.om.SequenceIterator @base) {
            return new Saxon.Hej.om.FocusTrackingIterator(@base);
        }
        public FocusTrackingIterator(Saxon.Hej.om.SequenceIterator @base)  {
            this.@base = @base;
        }
        public virtual Saxon.Hej.om.SequenceIterator getUnderlyingIterator() {
            return @base;
        }
        public virtual Saxon.Hej.om.Item next() {
            curr = @base.next();
            if ((curr == null)) {
                pos = -1;
            } else {
                pos++;
            }
            return curr;
        }
        public virtual Saxon.Hej.om.Item current() {
            return curr;
        }
        public virtual int position() {
            return pos;
        }
        public virtual int getLength() {
            if ((last == -1)) {
                if (Saxon.Hej.om.SequenceTool.supportsGetLength(@base)) {
                    last = Saxon.Hej.om.SequenceTool.getLength(@base);
                }
                if ((last == -1)) {
                    Saxon.Hej.om.GroundedValue residue = Saxon.Hej.value.SequenceExtent.makeResidue(@base);
                    last = (pos+residue.getLength());
                    @base = residue.iterate();
                }
            }
            return last;
        }
        public virtual bool supportsGetLength() {
            return true;
        }
        public bool supportsHasNext() {
            return ((@base is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext());
        }
        public virtual bool hasNext() {
            System.Diagnostics.Debug.Assert((@base is Saxon.Hej.tree.iter.LookaheadIterator));
            return ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).hasNext();
        }
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(@base);
        }
        public virtual Saxon.Hej.om.GroundedValue getResidue() {
            return Saxon.Hej.value.SequenceExtent.from(this);
        }
        public virtual void close() {
            @base.close();
        }
        public bool isActuallyGrounded() {
            return (((@base is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(@base)).isActuallyGrounded()));
        }
        private class SiblingMemory {
            public Saxon.Hej.pattern.NodeTest mostRecentNodeTest = null;
            public Saxon.Hej.om.NodeInfo mostRecentNode = null;
            public int mostRecentPosition = -1;
        
        }
        public virtual int getSiblingPosition(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.NodeTest nodeTest, int max) {
            if (((node is Saxon.Hej.tree.wrapper.SiblingCountingNode) && (nodeTest is Saxon.Hej.pattern.AnyNodeTest))) {
                return ((Saxon.Hej.tree.wrapper.SiblingCountingNode)(node)).getSiblingPosition();
            }
            if ((siblingMemory == null)) {
                siblingMemory = new Saxon.Hej.om.FocusTrackingIterator.SiblingMemory();
            } else {
                if ((siblingMemory.mostRecentNodeTest.Equals(nodeTest) && node.Equals(siblingMemory.mostRecentNode))) {
                    return siblingMemory.mostRecentPosition;
                }
            }
            Saxon.Hej.om.FocusTrackingIterator.SiblingMemory s = siblingMemory;
            Saxon.Hej.tree.iter.AxisIterator prev = node.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING, nodeTest);
            Saxon.Hej.om.NodeInfo prior;
            int count = 1;
            while (((prior = ((/*1430*/Saxon.Hej.om.NodeInfo)prev.next())) != null)) {
                if ((prior.Equals(s.mostRecentNode) && nodeTest.Equals(s.mostRecentNodeTest))) {
                    int result = (count+s.mostRecentPosition);
                    s.mostRecentNode = node;
                    s.mostRecentPosition = result;
                    return result;
                }
                if ((++count>max)) {
                    return count;
                }
            }
            s.mostRecentNode = node;
            s.mostRecentPosition = count;
            s.mostRecentNodeTest = nodeTest;
            return count;
        }
    
    }
}