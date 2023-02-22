using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using net.sf.saxon.z;
using Saxon.Impl.Helpers;
using GroundedValue = net.sf.saxon.om.GroundedValue;

namespace Saxon.Impl.DataModel {
    
    /**
     * This is a C# implementation of the Saxon AbstractArrayItem interface, implementing
     * XDM arrays underpinned by the .NET ImmutableList data structure.
     */
    public class ImmArray : net.sf.saxon.ma.arrays.AbstractArrayItem {
        
        private readonly ImmutableList<GroundedValue> backing;
        
        public ImmArray(net.sf.saxon.ma.arrays.SimpleArrayItem other) {
            this.backing = ImmutableList<GroundedValue>.Empty.AddRange(other.members());
        }

        private ImmArray(ImmutableList<GroundedValue> backingList) {
            this.backing = backingList;
        }

        public override net.sf.saxon.om.GroundedValue get(int index) {
            return backing[index];
        }
        public override net.sf.saxon.ma.arrays.ArrayItem put(int index, net.sf.saxon.om.GroundedValue newValue) {
            return new ImmArray(backing.SetItem(index, newValue));
        }
        public override net.sf.saxon.ma.arrays.ArrayItem insert(int position, net.sf.saxon.om.GroundedValue member) {
            return new ImmArray(backing.Insert(position, member));
        }
        public override int arrayLength() {
            return backing.Count;
        }
        public override bool isEmpty() {
            return backing.IsEmpty;
        }
        public override IEnumerable<net.sf.saxon.om.GroundedValue> members() {
            return backing;
        }
        public override net.sf.saxon.ma.arrays.ArrayItem subArray(int start, int end) {
            return new ImmArray(backing.GetRange(start, end - start));
        }
        public override net.sf.saxon.ma.arrays.ArrayItem concat(net.sf.saxon.ma.arrays.ArrayItem other) {
            if ((other.arrayLength()==0)) {
                return this;
            }

            if (other is ImmArray) {
                return new ImmArray(backing.AddRange(((ImmArray) other).backing));
            }
            else {
                throw new NotImplementedException();
            }
        }
        public override net.sf.saxon.ma.arrays.ArrayItem remove(int index) {
            return new ImmArray(backing.RemoveAt(index));
        }
        public override net.sf.saxon.ma.arrays.ArrayItem removeSeveral(net.sf.saxon.z.IntSet positions) {
            // TODO: optimize for the case where positions is an IntRangeSet, by calling RemoveRange once for each range
            ImmutableList<GroundedValue> vec = backing;
            // Sort the positions and start at the last so the positions of remaining items aren't affected
            int[] positionArray = new int[positions.size()];
            int p = 0;
            IntIterator iter = positions.iterator();
            while (iter.hasNext()) {
                positionArray[p++] = iter.next();
            }
            Array.Sort(positionArray);
            for (p = positionArray.Length; p >= 0; p--) {
                vec = vec.RemoveAt(p);
            }
            return new ImmArray(vec);
        }
    }
}
