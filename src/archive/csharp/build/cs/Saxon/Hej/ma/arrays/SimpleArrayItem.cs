using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    public class SimpleArrayItem : Saxon.Hej.ma.arrays.AbstractArrayItem {
        public static readonly Saxon.Hej.ma.arrays.SimpleArrayItem EMPTY_ARRAY = new Saxon.Hej.ma.arrays.SimpleArrayItem(new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>());
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> _members;
        private bool knownToBeGrounded = false;
        private Saxon.Hej.expr.elab.Pingable conversionPingable;
        public SimpleArrayItem(System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members)  {
            this._members = members;
        }
        public static Saxon.Hej.ma.arrays.SimpleArrayItem makeSimpleArrayItem(Saxon.Hej.om.SequenceIterator input) {
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
            for (Saxon.Hej.om.Item item; ((item = input.next()) != null); ) {
                members.Add(item);
            }
            Saxon.Hej.ma.arrays.SimpleArrayItem result = new Saxon.Hej.ma.arrays.SimpleArrayItem(members);
            result.knownToBeGrounded = true;
            return result;
        }
        public override Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            return new Saxon.Hej.expr.OperandRole[]{Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC};
        }
        public void requestNotification(Saxon.Hej.expr.elab.Pingable informee) {
            this.conversionPingable = informee;
        }
        public void notifyConversion() {
            if ((conversionPingable != null)) {
                conversionPingable.ping();
            }
        }
        public void makeGrounded() {
            if (!(knownToBeGrounded)) {
                lock (this) {
                    for (int i = 0; (i<_members.Count); i++) {
                        _members[i] = ((Saxon.Hej.om.Sequence)(_members[i])).materialize();
                    }
                    knownToBeGrounded = true;
                }
            }
        }
        public override Saxon.Hej.query.AnnotationList getAnnotations() {
            return Saxon.Hej.query.AnnotationList.EMPTY;
        }
        public override Saxon.Hej.om.GroundedValue get(int index) {
            return _members[index];
        }
        public override Saxon.Hej.ma.arrays.ArrayItem put(int index, Saxon.Hej.om.GroundedValue newValue) {
            notifyConversion();
            Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray(this);
            return a2.put(index, newValue);
        }
        public override int arrayLength() {
            return _members.Count;
        }
        public override bool isEmpty() {
            return _members.Count==0;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.om.GroundedValue> members() {
            return _members;
        }
        public override Saxon.Hej.om.SequenceIterator parcels() {
            return new Saxon.Hej.tree.iter.SequenceIteratorOverJavaIterator<Saxon.Hej.om.GroundedValue>(_members.GetEnumerator(), member => new Saxon.Hej.ma.Parcel(member));
        }
        public override Saxon.Hej.ma.arrays.ArrayItem removeSeveral(Saxon.Hej.z.IntSet positions) {
            notifyConversion();
            Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray(this);
            return a2.removeSeveral(positions);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem remove(int pos) {
            notifyConversion();
            Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray(this);
            return a2.remove(pos);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem subArray(int start, int end) {
            return new Saxon.Hej.ma.arrays.SimpleArrayItem(Saxon.Impl.Helpers.ListUtils.SubList(_members, start, end));
        }
        public override Saxon.Hej.ma.arrays.ArrayItem insert(int position, Saxon.Hej.om.GroundedValue member) {
            notifyConversion();
            Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray(this);
            return a2.insert(position, member);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem append(Saxon.Hej.om.GroundedValue newMember) {
            notifyConversion();
            Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray(this);
            return a2.append(newMember);
        }
        public override Saxon.Hej.ma.arrays.ArrayItem concat(Saxon.Hej.ma.arrays.ArrayItem other) {
            notifyConversion();
            Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray(this);
            return a2.concat(other);
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> getMembers() {
            return _members;
        }
        public override string toShortString() {
            int size = getLength();
            if ((size == 0)) {
                return "[]";
            } else {
                if ((size>5)) {
                    return (("[(:size "+size)+":)]");
                } else {
                    Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(256);
                    buff.append("[");
                    foreach (Saxon.Hej.om.GroundedValue entry in members()) {
                        buff.append(Saxon.Hej.trans.Err.depictSequence(entry).ToString().Trim());
                        buff.append(", ");
                    }
                    if ((size == 1)) {
                        buff.append("]");
                    } else {
                        buff.setCharAt((buff.length()-2), ']');
                    }
                    return buff.ToString().Trim();
                }
            }
        }
    
    }
}