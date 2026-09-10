using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class MemoFunction : Saxon.Hej.expr.instruct.UserFunction {
        private bool lookForNodes = false;
        public override void setParameterDefinitions(Saxon.Hej.expr.instruct.UserFunctionParameter[] @params) {
            base.setParameterDefinitions(@params);
            foreach (Saxon.Hej.expr.instruct.UserFunctionParameter param in @params) {
                if (param.getRequiredType().getPrimaryType().getUType().overlaps(Saxon.Hej.type.UType.ANY_NODE)) {
                    lookForNodes = true;
                }
            }
        }
        public override void computeEvaluationMode() {
            bodyEvaluator = getBody().makeElaborator().eagerly();
        }
        public override bool isMemoFunction() {
            return true;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] actualArgs) {
            for (int i = 0; (i<actualArgs.Length); i++) {
                actualArgs[i] = actualArgs[i].materialize();
            }
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.expr.instruct.MemoFunction.MemoFunctionCache cache = (Saxon.Hej.expr.instruct.MemoFunction.MemoFunctionCache)(controller.getUserData(this, "memo-function-cache"));
            if ((cache == null)) {
                cache = new Saxon.Hej.expr.instruct.MemoFunction.MemoFunctionCache(lookForNodes);
                controller.setUserData(this, "memo-function-cache", cache);
            }
            Saxon.Hej.om.Sequence[] savedArgs = ((isTailRecursive() || containsTailCalls()) ? Saxon.Ejava.util.Arrays.copyOf(actualArgs, actualArgs.Length) : actualArgs);
            int hash = cache.hash(actualArgs);
            Saxon.Hej.om.GroundedValue value = cache.get(hash, actualArgs);
            if ((value != null)) {
                return value;
            }
            value = base.call(context, actualArgs).materialize();
            cache.put(hash, savedArgs, value);
            return value;
        }
        private class MemoFunctionCache {
            private readonly bool lookForNodes;
            private readonly Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue>> cacheMap = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue>>();
            public MemoFunctionCache(bool lookForNodes)  {
                this.lookForNodes = lookForNodes;
            }
            public int hash(Saxon.Hej.om.Sequence[] args) {
                int h = 0x389247ab;
                foreach (Saxon.Hej.om.Sequence arg in args) {
                    Saxon.Hej.om.GroundedValue val = (Saxon.Hej.om.GroundedValue)(arg);
                    if ((val is Saxon.Hej.om.Item)) {
                        h ^= (hashItem((Saxon.Hej.om.Item)(val))+1);
                    } else {
                        foreach (Saxon.Hej.om.Item it in val.asIterable()) {
                            h ^= (hashItem(it)+1);
                        }
                    }
                }
                return h;
            }
            private int hashItem(Saxon.Hej.om.Item it) {
                return it.GetHashCode();
            }
            public Saxon.Hej.om.GroundedValue get(int hash, Saxon.Hej.om.Sequence[] args) {
                int arity = args.Length;
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> bucket = cacheMap.get(hash);
                if ((bucket == null)) {
                    return null;
                }
                for (int i = 0; (i<bucket.Count); i += ((arity+1))) {
                    bool found = true;
                    for (int j = 0; (j<arity); j++) {
                        if (!(sameValue((Saxon.Hej.om.GroundedValue)(args[j]), bucket[(i+j)]))) {
                            found = false;
                            break ;
                        }
                    }
                    if (found) {
                        return bucket[(i+arity)];
                    }
                }
                return null;
            }
            private bool sameValue(Saxon.Hej.om.GroundedValue v0, Saxon.Hej.om.GroundedValue v1) {
                if ((v0.getLength() != v1.getLength())) {
                    return false;
                }
                for (int i = 0; (i<v0.getLength()); i++) {
                    Saxon.Hej.om.Item it0 = v0.itemAt(i);
                    Saxon.Hej.om.Item it1 = v1.itemAt(i);
                    if ((it0 == it1)) {
                        continue ;
                    }
                    Saxon.Hej.om.Genre g0 = it0.getGenre();
                    if ((g0 == Saxon.Hej.om.Genre.NODE)) {
                        if ((it1 is Saxon.Hej.expr.instruct.MemoFunction.NodeSurrogate)) {
                            return ((Saxon.Hej.expr.instruct.MemoFunction.NodeSurrogate)(it1)).getObject()((Saxon.Hej.om.NodeInfo)(it0));
                        } else {
                            return false;
                        }
                    }
                    if ((g0 != it1.getGenre())) {
                        return false;
                    }
                    if ((it0.getGenre() == Saxon.Hej.om.Genre.ATOMIC)) {
                        Saxon.Hej.value.AtomicValue av0 = (Saxon.Hej.value.AtomicValue)(it0);
                        Saxon.Hej.value.AtomicValue av1 = (Saxon.Hej.value.AtomicValue)(it1);
                        if (!(av0.getItemType().Equals(av1.getItemType()))) {
                            return false;
                        }
                        if (!(av0.isIdentical(av1))) {
                            return false;
                        }
                        if (((av0 is Saxon.Hej.value.NumericValue) && ((((Saxon.Hej.value.NumericValue)(av0)).isNegativeZero() != ((Saxon.Hej.value.NumericValue)(av1)).isNegativeZero())))) {
                            return false;
                        }
                    } else {
                        if (!(it0.Equals(it1))) {
                            return false;
                        }
                    }
                }
                return true;
            }
            public void put(int hash, Saxon.Hej.om.Sequence[] args, Saxon.Hej.om.GroundedValue result) {
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> bucket = cacheMap.get(hash);
                if ((bucket == null)) {
                    bucket = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>((args.Length+1));
                    cacheMap.put(hash, bucket);
                }
                int initialSize = bucket.Count;
                foreach (Saxon.Hej.om.Sequence val in args) {
                    Saxon.Hej.om.GroundedValue gVal = (Saxon.Hej.om.GroundedValue)(val);
                    if ((((gVal is Saxon.Hej.value.AtomicValue) || (gVal is Saxon.Hej.om.FunctionItem)) || (gVal is Saxon.Hej.value.EmptySequence))) {
                        bucket.Add(gVal);
                    } else {
                        if ((gVal is Saxon.Hej.om.NodeInfo)) {
                            Saxon.Hej.om.Item subs = substitute((Saxon.Hej.om.NodeInfo)(gVal));
                            if ((subs == null)) {
                                return ;
                            }
                            bucket.Add(subs);
                        } else {
                            if (lookForNodes) {
                                Saxon.Hej.om.SequenceIterator iter = gVal.iterate();
                                Saxon.Hej.om.Item it;
                                System.Collections.Generic.IList<Saxon.Hej.om.Item> newSeq = new System.Collections.Generic.List<Saxon.Hej.om.Item>(gVal.getLength());
                                while (((it = iter.next()) != null)) {
                                    if ((it is Saxon.Hej.om.NodeInfo)) {
                                        Saxon.Hej.om.Item subs = substitute((Saxon.Hej.om.NodeInfo)(it));
                                        if ((subs == null)) {
                                            while ((bucket.Count>initialSize)) {
                                                Saxon.Impl.Helpers.ListUtils.Remove(bucket, (bucket.Count-1));
                                            }
                                            return ;
                                        }
                                        newSeq.Add(subs);
                                    } else {
                                        newSeq.Add(it);
                                    }
                                }
                                bucket.Add(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(newSeq));
                            } else {
                                bucket.Add(gVal);
                            }
                        }
                    }
                }
                bucket.Add(result);
            }
        
        }
        private static Saxon.Hej.om.Item substitute(Saxon.Hej.om.NodeInfo node) {
            switch (node.getTreeInfo().getDurability()) {
              case Saxon.Hej.om.Durability.LASTING:
                return node;
              case Saxon.Hej.om.Durability.TEMPORARY:
                return new Saxon.Hej.expr.instruct.MemoFunction.NodeSurrogate(node);
              default:
                return null;
            }
        }
        public class NodeSurrogate : Saxon.Hej.value.ObjectValue<System.Func<Saxon.Hej.om.NodeInfo,bool>> {
            protected internal NodeSurrogate(Saxon.Hej.om.NodeInfo node) : base (matcher(node)) {
            }
            private static System.Func<Saxon.Hej.om.NodeInfo,bool> matcher(Saxon.Hej.om.NodeInfo node) {
                if ((node is Saxon.Hej.tree.tiny.TinyAttributeImpl)) {
                    long docNr = node.getTreeInfo().getDocumentNumber();
                    int nodeNr = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).getNodeNumber();
                    return node1 => (((node1 is Saxon.Hej.tree.tiny.TinyAttributeImpl) && (docNr == node1.getTreeInfo().getDocumentNumber())) && (nodeNr == ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node1)).getNodeNumber()));
                } else {
                    if ((node is Saxon.Hej.tree.tiny.TinyNodeImpl)) {
                        long docNr = node.getTreeInfo().getDocumentNumber();
                        int nodeNr = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).getNodeNumber();
                        return node1 => (((node1 is Saxon.Hej.tree.tiny.TinyNodeImpl) && (docNr == node1.getTreeInfo().getDocumentNumber())) && (nodeNr == ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node1)).getNodeNumber()));
                    } else {
                        string generatedId = generateId(node);
                        return node1 => generatedId.Equals(generateId(node1));
                    }
                }
            }
            private static string generateId(Saxon.Hej.om.NodeInfo node) {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
                node.generateId(sb);
                return sb.ToString();
            }
        
        }
    
    }
}