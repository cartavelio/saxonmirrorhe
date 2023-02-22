using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CustomSet<T>  {
        private readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.sort.CustomSetEntryChain<T>> buckets;
        private readonly Saxon.Hej.expr.sort.EqualityMatcher<T> equalityMatcher;
        public CustomSet(Saxon.Hej.expr.sort.EqualityMatcher<T> matcher)  {
            buckets = new Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.sort.CustomSetEntryChain<T>>();
            equalityMatcher = matcher;
        }
        public bool add(T value) {
            int h = equalityMatcher.hash(value);
            Saxon.Hej.expr.sort.CustomSetEntryChain<T> bucket = buckets.get(h);
            if ((bucket == null)) {
                Saxon.Hej.expr.sort.CustomSetEntryChain<T> entry = new Saxon.Hej.expr.sort.CustomSetEntryChain<T>(value);
                buckets.put(h, entry);
                return true;
            } else {
                while (true) {
                    if (equalityMatcher.equal(bucket.value, value)) {
                        return false;
                    } else {
                        if ((bucket.next == null)) {
                            Saxon.Hej.expr.sort.CustomSetEntryChain<T> entry = new Saxon.Hej.expr.sort.CustomSetEntryChain<T>(value);
                            bucket.next = entry;
                            return true;
                        } else {
                            bucket = bucket.next;
                        }
                    }
                }
            }
        }
        public bool contains(T value) {
            int h = equalityMatcher.hash(value);
            Saxon.Hej.expr.sort.CustomSetEntryChain<T> bucket = buckets.get(h);
            if ((bucket == null)) {
                return false;
            } else {
                while (true) {
                    if (equalityMatcher.equal(bucket.value, value)) {
                        return true;
                    } else {
                        if ((bucket.next == null)) {
                            return false;
                        } else {
                            bucket = bucket.next;
                        }
                    }
                }
            }
        }
    
    }
}