using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.parallel {
    public class MultithreadedFocusTrackingIterator : Saxon.Hej.om.FocusTrackingIterator {
        public MultithreadedFocusTrackingIterator(Saxon.Hej.om.SequenceIterator @base) : base (@base) {
        }
        public override Saxon.Hej.om.SequenceIterator getUnderlyingIterator() { lock(this) {
            return base.getUnderlyingIterator();
        }}
        public override Saxon.Hej.om.Item next() { lock(this) {
            return base.next();
        }}
        public override Saxon.Hej.om.Item current() { lock(this) {
            return base.current();
        }}
        public override int position() { lock(this) {
            return base.position();
        }}
        public override bool supportsGetLength() {
            return true;
        }
        public override int getLength() { lock(this) {
            return base.getLength();
        }}
        public override bool hasNext() { lock(this) {
            return base.hasNext();
        }}
        public override Saxon.Hej.om.GroundedValue materialize() { lock(this) {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(this);
        }}
        public override Saxon.Hej.om.GroundedValue getResidue() { lock(this) {
            return base.getResidue();
        }}
        public override void close() { lock(this) {
            base.close();
        }}
        public override int getSiblingPosition(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.NodeTest nodeTest, int max) { lock(this) {
            return base.getSiblingPosition(node, nodeTest, max);
        }}
    
    }
}