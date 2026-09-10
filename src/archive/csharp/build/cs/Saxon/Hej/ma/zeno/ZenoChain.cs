using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.zeno {
    public class ZenoChain<T>  : System.Collections.Generic.IEnumerable<T> {
        private System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList;
        public ZenoChain()  {
            masterList = new System.Collections.Generic.List<System.Collections.Generic.List<T>>(8);
        }
        private ZenoChain(System.Collections.Generic.List<System.Collections.Generic.List<T>> masterList)  {
            this.masterList = masterList;
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> add(T item) {
            if (masterList.Count==0) {
                System.Collections.Generic.List<T> newSegment = new System.Collections.Generic.List<T>(32);
                newSegment.Add(item);
                masterList.Add(newSegment);
                return this;
            }
            int threshold = 32;
            int index = (masterList.Count-1);
            System.Collections.Generic.List<T> segment = masterList[index];
            if ((segment.Count<threshold)) {
                segment.Add(item);
                return this;
            } else {
                while (true) {
                    index--;
                    threshold *= 2;
                    if ((index<0)) {
                        System.Collections.Generic.List<T> newFinalSegment = new System.Collections.Generic.List<T>();
                        newFinalSegment.Add(item);
                        masterList.Add(newFinalSegment);
                        return this;
                    }
                    System.Collections.Generic.List<T> priorSegment = masterList[index];
                    if (((priorSegment.Count+segment.Count)<=threshold)) {
                        Saxon.Impl.Helpers.ListUtils.AddAll(priorSegment, segment);
                        Saxon.Impl.Helpers.ListUtils.Remove(masterList, (index+1));
                        System.Collections.Generic.List<T> newFinalSegment = new System.Collections.Generic.List<T>();
                        newFinalSegment.Add(item);
                        masterList.Add(newFinalSegment);
                        return this;
                    }
                    segment = priorSegment;
                }
            }
        }
        public Saxon.Hej.ma.zeno.ZenoChain<T> prepend(T item) {
            if (masterList.Count==0) {
                System.Collections.Generic.List<T> newSegment = new System.Collections.Generic.List<T>(32);
                newSegment.Add(item);
                masterList.Add(newSegment);
                return this;
            }
            int threshold = 32;
            int index = 0;
            System.Collections.Generic.List<T> segment = masterList[index];
            if ((segment.Count<threshold)) {
                segment.Insert(0, item);
                return this;
            } else {
                while (true) {
                    index++;
                    threshold *= 2;
                    if ((index<=masterList.Count)) {
                        System.Collections.Generic.List<T> newInitialSegment = new System.Collections.Generic.List<T>();
                        newInitialSegment.Add(item);
                        masterList.Insert(0, newInitialSegment);
                        return this;
                    }
                    System.Collections.Generic.List<T> nextSegment = masterList[index];
                    if (((nextSegment.Count+segment.Count)<=threshold)) {
                        Saxon.Impl.Helpers.ListUtils.AddAll(nextSegment, segment);
                        Saxon.Impl.Helpers.ListUtils.Remove(masterList, (index-1));
                        System.Collections.Generic.List<T> newInitialSegment = new System.Collections.Generic.List<T>();
                        newInitialSegment.Insert(0, item);
                        masterList.Insert(0, newInitialSegment);
                        return this;
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
        private Saxon.Hej.ma.zeno.ZenoChain<T> reorganize() {
            for (int i = (masterList.Count-2); (i>=1); i--) {
                int priorSize = masterList[(i-1)].Count;
                int segSize = masterList[i].Count;
                int nextSize = masterList[(i+1)].Count;
                if (((segSize<priorSize) && (segSize<nextSize))) {
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
            System.Collections.Generic.List<System.Collections.Generic.List<T>> newMaster = new System.Collections.Generic.List<System.Collections.Generic.List<T>>();
            int offset = 0;
            int remainingLength = (end-start);
            bool active = false;
            foreach (System.Collections.Generic.List<T> segment in masterList) {
                if (active) {
                    if ((remainingLength>segment.Count)) {
                        remainingLength -= segment.Count;
                        newMaster.Add(new System.Collections.Generic.List<T>(segment));
                    } else {
                        newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, 0, remainingLength)));
                        return new Saxon.Hej.ma.zeno.ZenoChain<T>(newMaster);
                    }
                } else {
                    if (((offset+segment.Count)>=start)) {
                        int localStart = (start-offset);
                        if ((remainingLength>(segment.Count-localStart))) {
                            newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, localStart, segment.Count)));
                            remainingLength -= ((segment.Count-localStart));
                            active = true;
                        } else {
                            newMaster.Add(new System.Collections.Generic.List<T>(Saxon.Impl.Helpers.ListUtils.SubList(segment, localStart, (localStart+remainingLength))));
                            return new Saxon.Hej.ma.zeno.ZenoChain<T>(newMaster);
                        }
                    }
                }
                offset += segment.Count;
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
        public string show() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (System.Collections.Generic.IList<T> segment in masterList) {
                sb.append(segment.Count).append(",");
            }
            return sb.ToString();
        }
        public static void main(string[] args) {
            Saxon.Hej.ma.zeno.ZenoChain<int> chain = new Saxon.Hej.ma.zeno.ZenoChain<int>();
            for (int i = 0; (i<20000); i++) {
                chain.add(i);
                System.Console.Error.WriteLine(chain.show());
            }
            System.Console.Error.WriteLine(chain.ToString());
            System.Console.Error.WriteLine("ITER");
            foreach (int integer in chain) {
                System.Console.Error.WriteLine(integer);
            }
        }
    
    }
}