using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.zeno {
    public class ZenoChain<T>  : System.Collections.Generic.IEnumerable<T> {
        private readonly System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList;
        public ZenoChain()  {
            masterList = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(8);
        }
        private ZenoChain(System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList)  {
            this.masterList = masterList;
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> add(T item) {
            System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList2 = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(masterList);
            if (masterList2.Count==0) {
                System.Collections.Generic.List<T> newSegment = new System.Collections.Generic.List<T>(32);
                newSegment.Add(item);
                masterList2.Add(newSegment);
                return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
            }
            int threshold = 32;
            int index = (masterList2.Count-1);
            System.Collections.Generic.List<T> segment = masterList2[index];
            if ((segment.Count<threshold)) {
                System.Collections.Generic.List<T> segment2 = new System.Collections.Generic.List<T>(32);
                Saxon.Impl.Helpers.ListUtils.AddAll(segment2, segment);
                segment2.Add(item);
                masterList2[index] = segment2;
                return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
            } else {
                while (true) {
                    index--;
                    threshold *= 2;
                    if ((index<0)) {
                        System.Collections.Generic.List<T> newFinalSegment = new System.Collections.Generic.List<T>();
                        newFinalSegment.Add(item);
                        masterList2.Add(newFinalSegment);
                        return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
                    }
                    System.Collections.Generic.List<T> priorSegment = masterList2[index];
                    if (((priorSegment.Count+segment.Count)<=threshold)) {
                        System.Collections.Generic.List<T> combinedSegment = new System.Collections.Generic.List<T>((priorSegment.Count+segment.Count));
                        Saxon.Impl.Helpers.ListUtils.AddAll(combinedSegment, priorSegment);
                        Saxon.Impl.Helpers.ListUtils.AddAll(combinedSegment, segment);
                        masterList2[index] = combinedSegment;
                        Saxon.Impl.Helpers.ListUtils.Remove(masterList2, (index+1));
                        System.Collections.Generic.List<T> newFinalSegment = new System.Collections.Generic.List<T>();
                        newFinalSegment.Add(item);
                        masterList2.Add(newFinalSegment);
                        return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
                    }
                    segment = priorSegment;
                }
            }
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> prepend(T item) {
            System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList2 = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(masterList);
            if (masterList2.Count==0) {
                return add(item);
            }
            int threshold = 32;
            int index = 0;
            System.Collections.Generic.List<T> segment = masterList2[index];
            if ((segment.Count<threshold)) {
                System.Collections.Generic.List<T> segment2 = new System.Collections.Generic.List<T>(32);
                segment2.Add(item);
                Saxon.Impl.Helpers.ListUtils.AddAll(segment2, segment);
                masterList2[index] = segment2;
                return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
            } else {
                while (true) {
                    index++;
                    threshold *= 2;
                    if ((index>=masterList2.Count)) {
                        System.Collections.Generic.List<T> newInitialSegment = new System.Collections.Generic.List<T>();
                        newInitialSegment.Add(item);
                        masterList2.Insert(0, newInitialSegment);
                        return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
                    }
                    System.Collections.Generic.List<T> nextSegment = masterList2[index];
                    if (((nextSegment.Count+segment.Count)<=threshold)) {
                        System.Collections.Generic.List<T> combinedSegment = new System.Collections.Generic.List<T>();
                        Saxon.Impl.Helpers.ListUtils.AddAll(combinedSegment, segment);
                        Saxon.Impl.Helpers.ListUtils.AddAll(combinedSegment, nextSegment);
                        masterList2[index] = combinedSegment;
                        Saxon.Impl.Helpers.ListUtils.Remove(masterList2, (index-1));
                        System.Collections.Generic.List<T> newInitialSegment = new System.Collections.Generic.List<T>();
                        newInitialSegment.Add(item);
                        masterList2.Insert(0, newInitialSegment);
                        return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
                    }
                    segment = nextSegment;
                }
            }
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> addAll(System.Collections.Generic.IEnumerable<T> items) {
            Saxon.Hej.ma.zeno.ZenoChain<T> result = this;
            foreach (T item in items) {
                result = result.add(item);
            }
            return result;
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> concat(Saxon.Hej.ma.zeno.ZenoChain<T> other) {
            System.Collections.Generic.List<System.Collections.Generic.List<T>> newMaster = new System.Collections.Generic.List<System.Collections.Generic.List<T>>((masterList.Count+other.masterList.Count));
            Saxon.Impl.Helpers.ListUtils.AddAll(newMaster, masterList);
            Saxon.Impl.Helpers.ListUtils.AddAll(newMaster, other.masterList);
            return new Saxon.Hej.ma.zeno.ZenoChain<T>(newMaster).reorganize();
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> replace(int n, T value) {
            if ((n<0)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is negative"));
            }
            int offset = 0;
            System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList2 = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(masterList.Count);
            bool done = false;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                if ((((offset+segment.Count)>n) && !(done))) {
                    System.Collections.Generic.List<T> replacementSegment = new System.Collections.Generic.List<T>(segment.Count);
                    Saxon.Impl.Helpers.ListUtils.AddAll(replacementSegment, Saxon.Impl.Helpers.ListUtils.SubList(segment, 0, (n-offset)));
                    replacementSegment.Add(value);
                    Saxon.Impl.Helpers.ListUtils.AddAll(replacementSegment, Saxon.Impl.Helpers.ListUtils.SubList(segment, ((n-offset)+1), segment.Count));
                    masterList2.Add(replacementSegment);
                    done = true;
                } else {
                    masterList2.Add(segment);
                }
                offset += segment.Count;
            }
            if (!(done)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is too large"));
            }
            return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> remove(int n) {
            if ((n<0)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is negative"));
            }
            int offset = 0;
            System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList2 = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(masterList.Count);
            bool done = false;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                if ((((offset+segment.Count)>n) && !(done))) {
                    if ((segment.Count>1)) {
                        System.Collections.Generic.List<T> replacementSegment = new System.Collections.Generic.List<T>((segment.Count-1));
                        Saxon.Impl.Helpers.ListUtils.AddAll(replacementSegment, Saxon.Impl.Helpers.ListUtils.SubList(segment, 0, (n-offset)));
                        Saxon.Impl.Helpers.ListUtils.AddAll(replacementSegment, Saxon.Impl.Helpers.ListUtils.SubList(segment, ((n-offset)+1), segment.Count));
                        masterList2.Add(replacementSegment);
                    }
                    done = true;
                } else {
                    masterList2.Add(segment);
                }
                offset += segment.Count;
            }
            if (!(done)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is too large"));
            }
            return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> insert(int n, T value) {
            if ((n<0)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is negative"));
            }
            if ((n == 0)) {
                return prepend(value);
            }
            int length = size();
            if ((n == length)) {
                return add(value);
            }
            if ((n>length)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is too large"));
            }
            int offset = 0;
            System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList2 = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(masterList.Count);
            bool done = false;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                if ((((offset+segment.Count)>n) && !(done))) {
                    System.Collections.Generic.List<T> replacementSegment = new System.Collections.Generic.List<T>((segment.Count+1));
                    Saxon.Impl.Helpers.ListUtils.AddAll(replacementSegment, Saxon.Impl.Helpers.ListUtils.SubList(segment, 0, (n-offset)));
                    replacementSegment.Add(value);
                    Saxon.Impl.Helpers.ListUtils.AddAll(replacementSegment, Saxon.Impl.Helpers.ListUtils.SubList(segment, (n-offset), segment.Count));
                    masterList2.Add(replacementSegment);
                    done = true;
                } else {
                    masterList2.Add(segment);
                }
                offset += segment.Count;
            }
            if (!(done)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is too large"));
            }
            return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList2);
        }
        private Saxon.Hej.ma.zeno.ZenoChain<T> reorganize() {
            for (int i = (masterList.Count-2); (i>=1); i--) {
                int priorSize = masterList[(i-1)].Count;
                int segSize = masterList[i].Count;
                int nextSize = masterList[(i+1)].Count;
                if (((segSize<=priorSize) && (segSize<=nextSize))) {
                    System.Collections.Generic.List<T> combinedSegment = new System.Collections.Generic.List<T>((priorSize+segSize));
                    Saxon.Impl.Helpers.ListUtils.AddAll(combinedSegment, masterList[(i-1)]);
                    Saxon.Impl.Helpers.ListUtils.AddAll(combinedSegment, masterList[i]);
                    masterList[(i-1)] = combinedSegment;
                    Saxon.Impl.Helpers.ListUtils.Remove(masterList, i);
                }
            }
            return new Saxon.Hej.ma.zeno.ZenoChain<T>(masterList);
        }
        public T get(int n) {
            if ((n<0)) {
                throw new System.IndexOutOfRangeException((("Index "+n)+" is negative"));
            }
            int offset = 0;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                if (((offset+segment.Count)>n)) {
                    return segment[(n-offset)];
                }
                offset += segment.Count;
            }
            throw new System.IndexOutOfRangeException((("Index "+n)+" is too large"));
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> subList(int start, int end) {
            if (((start<0) || (start>end))) {
                throw new System.IndexOutOfRangeException("start position for subList is out of range");
            }
            System.Collections.Generic.List<System.Collections.Generic.List<T>> newMaster = new System.Collections.Generic.List<System.Collections.Generic.List<T>>();
            int offset = 0;
            int remainingLength = (end-start);
            bool active = false;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                if (active) {
                    if ((remainingLength>segment.Count)) {
                        remainingLength -= segment.Count;
                        newMaster.Add(segment);
                    } else {
                        newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, 0, remainingLength)));
                        return new Saxon.Hej.ma.zeno.ZenoChain<T>(newMaster);
                    }
                } else {
                    if (((offset+segment.Count)>start)) {
                        int localStart = (start-offset);
                        if ((remainingLength>(segment.Count-localStart))) {
                            if (((start == 1) && (segment.Count>128))) {
                                newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, localStart, (localStart+64))));
                                newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, (localStart+64), segment.Count)));
                            } else {
                                newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, localStart, segment.Count)));
                            }
                            remainingLength -= ((segment.Count-localStart));
                            active = true;
                        } else {
                            newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, localStart, (localStart+remainingLength))));
                            return new Saxon.Hej.ma.zeno.ZenoChain<T>(newMaster);
                        }
                    } else {
                        if ((remainingLength == 0)) {
                            break ;
                        }
                    }
                }
                offset += segment.Count;
            }
            if ((remainingLength>0)) {
                throw new System.IndexOutOfRangeException("end position for subList is out of range");
            }
            return new Saxon.Hej.ma.zeno.ZenoChain<T>(newMaster);
        }
        public int size() {
            int total = 0;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                total += segment.Count;
            }
            return total;
        }
        public bool isEmpty() {
            return (masterList.Count==0 || (((masterList.Count == 1) && masterList[0].Count==0)));
        }
        public bool isSingleton() {
            return ((masterList.Count == 1) && (masterList[0].Count == 1));
        }
        public System.Collections.Generic.IEnumerator<T> GetEnumerator() {
            return new Saxon.Hej.ma.zeno.ZenoChainIterator<T>(masterList);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (System.Collections.Generic.IList<T> segment in masterList) {
                sb.append("(");
                foreach (T item in segment) {
                    sb.append(item).append(",");
                }
                sb.setCharAt((sb.length()-1), ')');
            }
            return sb.ToString();
        }
        public string showMetrics() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            sb.append('(');
            foreach (System.Collections.Generic.IList<T> segment in masterList) {
                sb.append(segment.Count).append(",");
            }
            sb.setCharAt((sb.length()-1), ')');
            return sb.ToString();
        }
    
    }
}