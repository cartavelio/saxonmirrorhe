using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ParameterSet {
        private Saxon.Hej.om.StructuredQName[] keys;
        private Saxon.Hej.om.Sequence[] values;
        private bool[] typeChecked;
        private int used = 0;
        public static Saxon.Hej.expr.instruct.ParameterSet EMPTY_PARAMETER_SET = new Saxon.Hej.expr.instruct.ParameterSet(0);
        public ParameterSet() : this (10) {
        }
        public ParameterSet(int capacity)  {
            keys = new Saxon.Hej.om.StructuredQName[capacity];
            values = new Saxon.Hej.om.Sequence[capacity];
            typeChecked = new bool[capacity];
        }
        public ParameterSet(System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> map) : this (map.Count) {
            int i = 0;
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> entry in map) {
                keys[i] = entry.Key;
                values[i] = entry.Value;
                typeChecked[i++] = false;
            }
            used = i;
        }
        public ParameterSet(Saxon.Hej.expr.instruct.ParameterSet existing, int extra) : this ((existing.used+extra)) {
            for (int i = 0; (i<existing.used); i++) {
                put(existing.keys[i], existing.values[i], existing.typeChecked[i]);
            }
        }
        public int size() {
            return used;
        }
        public void put(Saxon.Hej.om.StructuredQName id, Saxon.Hej.om.Sequence value, bool @checked) {
            for (int i = 0; (i<used); i++) {
                if (keys[i].Equals(id)) {
                    values[i] = value;
                    typeChecked[i] = @checked;
                    return ;
                }
            }
            if (((used+1)>keys.Length)) {
                int newLength = ((used<=5) ? 10 : (used*2));
                values = Saxon.Ejava.util.Arrays.copyOf(values, newLength);
                keys = Saxon.Ejava.util.Arrays.copyOf(keys, newLength);
                typeChecked = Saxon.Ejava.util.Arrays.copyOf(typeChecked, newLength);
            }
            keys[used] = id;
            typeChecked[used] = @checked;
            values[used++] = value;
        }
        public Saxon.Hej.om.StructuredQName[] getParameterNames() {
            return keys;
        }
        public int getIndex(Saxon.Hej.om.StructuredQName id) {
            for (int i = 0; (i<used); i++) {
                if (keys[i].Equals(id)) {
                    return i;
                }
            }
            return -1;
        }
        public Saxon.Hej.om.Sequence getValue(int index) {
            return values[index];
        }
        public bool isTypeChecked(int index) {
            return typeChecked[index];
        }
        public void clear() {
            used = 0;
        }
        public void materializeValues() {
            for (int i = 0; (i<used); i++) {
                if ((values[i] is Saxon.Hej.value.Closure)) {
                    values[i] = ((Saxon.Hej.value.Closure)(values[i])).reduce();
                }
            }
        }
        public const int NOT_SUPPLIED = 0;
        public const int SUPPLIED = 1;
        public const int SUPPLIED_AND_CHECKED = 2;
    
    }
}