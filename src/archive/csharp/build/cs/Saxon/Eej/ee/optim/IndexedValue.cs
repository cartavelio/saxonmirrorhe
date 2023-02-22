using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class IndexedValue : Saxon.Eej.ee.optim.SearchableValue {
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.Expression> indexedExpressions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(3);
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.optim.MultiIndex> indexes = new System.Collections.Generic.List<Saxon.Eej.ee.optim.MultiIndex>(3);
        private System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> distinctNodes;
        internal IndexedValue(Saxon.Hej.om.Item[] array, int end)  {
            this.value = array;
            this.end = end;
        }
        public override Saxon.Hej.om.SequenceIterator findItems(Saxon.Hej.expr.Expression use, Saxon.Hej.om.SequenceIterator values, bool convertUntypedToOther, bool firstOnly, Saxon.Hej.expr.sort.AtomicComparer comparer, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.optim.MultiIndex index = null;
            lock (this) {
                for (int i = 0; (i<indexedExpressions.Count); i++) {
                    Saxon.Hej.expr.Expression si = indexedExpressions[i];
                    if ((si == use)) {
                        index = indexes[i];
                        break ;
                    }
                }
                if ((index == null)) {
                    for (int i = 0; (i<indexedExpressions.Count); i++) {
                        Saxon.Hej.expr.Expression si = indexedExpressions[i];
                        if (si.isEqual(use)) {
                            index = indexes[i];
                            indexedExpressions.Add(use);
                            indexes.Add(index);
                            break ;
                        }
                    }
                }
                if ((index == null)) {
                    index = new Saxon.Eej.ee.optim.MultiIndex(this);
                    indexedExpressions.Add(use);
                    indexes.Add(index);
                }
            }
            return index.findItems(use, values, convertUntypedToOther, firstOnly, comparer.getCollator(), context);
        }
        public override bool containsNode(Saxon.Hej.om.NodeInfo sought) { lock(this) {
            if ((distinctNodes == null)) {
                distinctNodes = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
                Saxon.Hej.om.SequenceIterator iter = iterate();
                Saxon.Hej.om.Item it;
                while (((it = iter.next()) != null)) {
                    if ((it is Saxon.Hej.om.NodeInfo)) {
                        distinctNodes.Add((Saxon.Hej.om.NodeInfo)(it));
                    }
                }
            }
            return distinctNodes.Contains(sought);
        }}
    
    }
}