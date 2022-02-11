using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SortKeyDefinitionList : Saxon.Hej.expr.PseudoExpression, System.Collections.Generic.IEnumerable<Saxon.Hej.expr.sort.SortKeyDefinition> {
        private readonly Saxon.Hej.expr.sort.SortKeyDefinition[] sortKeyDefinitions;
        public SortKeyDefinitionList(Saxon.Hej.expr.sort.SortKeyDefinition[] sortKeyDefinitions)  {
            this.sortKeyDefinitions = sortKeyDefinitions;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(size());
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in sortKeyDefinitions) {
                list.Add(new Saxon.Hej.expr.Operand(this, skd, Saxon.Hej.expr.OperandRole.INSPECT));
            }
            return list;
        }
        public override bool isLiftable(bool forStreaming) {
            return false;
        }
        public int size() {
            return sortKeyDefinitions.Length;
        }
        public Saxon.Hej.expr.sort.SortKeyDefinition getSortKeyDefinition(int i) {
            return sortKeyDefinitions[i];
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.expr.sort.SortKeyDefinition> GetEnumerator() {
            return Saxon.Ejava.util.Arrays.asList(sortKeyDefinitions).GetEnumerator();
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public override Saxon.Hej.expr.sort.SortKeyDefinitionList copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.sort.SortKeyDefinition[] s2 = new Saxon.Hej.expr.sort.SortKeyDefinition[sortKeyDefinitions.Length];
            for (int i = 0; (i<sortKeyDefinitions.Length); i++) {
                s2[i] = sortKeyDefinitions[i].copy(rebindings);
            }
            return new Saxon.Hej.expr.sort.SortKeyDefinitionList(s2);
        }
        public override int getImplementationMethod() {
            return 0;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in sortKeyDefinitions) {
                skd.export(@out);
            }
        }
    
    }
}