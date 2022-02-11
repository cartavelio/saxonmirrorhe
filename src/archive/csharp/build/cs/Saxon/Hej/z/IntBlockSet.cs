using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntBlockSet : Saxon.Hej.z.IntSet {
        private readonly int startPoint;
        private readonly int endPoint;
        private int cachedHashCode = -1;
        public IntBlockSet(int startPoint, int endPoint)  {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
        public override Saxon.Hej.z.IntSet copy() {
            return this;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return new Saxon.Hej.z.IntRangeSet(new int[]{startPoint}, new int[]{endPoint});
        }
        public override bool isMutable() {
            return false;
        }
        public override int size() {
            return (endPoint-startPoint);
        }
        public override bool isEmpty() {
            return (size() == 0);
        }
        public override bool contains(int value) {
            return ((value>=startPoint) && (value<=endPoint));
        }
        public override bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("remove");
        }
        public override void clear() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("clear");
        }
        public override bool add(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("add");
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return mutableCopy().iterator();
        }
        public override string ToString() {
            return ((startPoint+" - ")+endPoint);
        }
        public override bool Equals(System.Object other) {
            return mutableCopy().Equals(other);
        }
        public override int GetHashCode() {
            if ((cachedHashCode == -1)) {
                cachedHashCode = (0x236a89f1^((startPoint+((endPoint<<3)))));
            }
            return cachedHashCode;
        }
        public int getStartPoint() {
            return startPoint;
        }
        public int getEndPoint() {
            return endPoint;
        }
    
    }
}