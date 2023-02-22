using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Saxon.Hej.ma.arrays;
using Saxon.Hej.om;
using Saxon.Hej.z;
using Saxon.Impl.Helpers;
using GroundedValue = Saxon.Hej.om.GroundedValue;

namespace Saxon.Impl.DataModel {
    
    /**
     * This is a C# implementation of the Saxon AbstractArrayItem interface, implementing
     * XDM arrays underpinned by the .NET ImmutableList data structure.
     */
    public class ImmArray : Saxon.Hej.ma.arrays.AbstractArrayItem {
        
        private readonly ImmutableList<GroundedValue> backing;
        
        public ImmArray(Saxon.Hej.ma.arrays.SimpleArrayItem other) {
            this.backing = ImmutableList<GroundedValue>.Empty.AddRange(other.members());
        }
        
        public ImmArray(IList<GroundedValue> members) {
            this.backing = ImmutableList<GroundedValue>.Empty.AddRange(members);
        }

        private ImmArray(ImmutableList<GroundedValue> backingList) {
            this.backing = backingList;
        }

        public static ImmArray from(SequenceIterator iter) {
            ImmutableList<GroundedValue> list = ImmutableList<GroundedValue>.Empty;
            for (Item item; (item = iter.next()) != null;) {
                list = list.Add(item);
            }

            return new ImmArray(list);
        }

        public override Saxon.Hej.om.GroundedValue get(int index) {
            return backing[index];
        }
        public override Saxon.Hej.ma.arrays.ArrayItem put(int index, Saxon.Hej.om.GroundedValue newValue) {
            return new ImmArray(backing.SetItem(index, newValue));
        }
        public override Saxon.Hej.ma.arrays.ArrayItem insert(int position, Saxon.Hej.om.GroundedValue member) {
            return new ImmArray(backing.Insert(position, member));
        }
        public override Saxon.Hej.ma.arrays.ArrayItem append(Saxon.Hej.om.GroundedValue member) {
            return new ImmArray(backing.Add(member));
        }
        public override int arrayLength() {
            return backing.Count;
        }
        public override bool isEmpty() {
            return backing.IsEmpty;
        }
        public override IEnumerable<Saxon.Hej.om.GroundedValue> members() {
            return backing;
        }
        public override Saxon.Hej.ma.arrays.ArrayItem subArray(int start, int end) {
            return new ImmArray(backing.GetRange(start, end - start));
        }
        public override Saxon.Hej.ma.arrays.ArrayItem concat(Saxon.Hej.ma.arrays.ArrayItem other) {
            if ((other.arrayLength()==0)) {
                return this;
            }
            if (other is ImmArray) {
                return new ImmArray(backing.AddRange(((ImmArray) other).backing));
            }
            else {
                Saxon.Impl.DataModel.ImmArray a2 = new Saxon.Impl.DataModel.ImmArray((SimpleArrayItem)other);
                return concat(a2);
            }
        }
        public override Saxon.Hej.ma.arrays.ArrayItem remove(int index) {
            return new ImmArray(backing.RemoveAt(index));
        }
        public override Saxon.Hej.ma.arrays.ArrayItem removeSeveral(Saxon.Hej.z.IntSet positions) {
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
            for (p = positionArray.Length-1; p >= 0; p--) {
                vec = vec.RemoveAt(positionArray[p]);
            }
            return new ImmArray(vec);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
