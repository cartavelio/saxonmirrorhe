using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SimpleTypeComparison : Saxon.Hej.expr.sort.EqualityMatcher<Saxon.Hej.om.AtomicSequence> {
        private static readonly Saxon.Hej.expr.sort.SimpleTypeComparison INSTANCE = new Saxon.Hej.expr.sort.SimpleTypeComparison();
        public static Saxon.Hej.expr.sort.SimpleTypeComparison getInstance() {
            return INSTANCE;
        }
        public int compareSequences(Saxon.Hej.om.AtomicSequence a1, Saxon.Hej.om.AtomicSequence a2) {
            if (((a1.getLength() == 1) && (a2.getLength() == 1))) {
                return compareItems(((/*1430*/Saxon.Hej.value.AtomicValue)a1.itemAt(0)), ((/*1430*/Saxon.Hej.value.AtomicValue)a2.itemAt(0)));
            } else {
                Saxon.Hej.tree.iter.AtomicIterator iter1 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)a1.iterate());
                Saxon.Hej.tree.iter.AtomicIterator iter2 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)a2.iterate());
                while (true) {
                    Saxon.Hej.value.AtomicValue item1 = ((/*1430*/Saxon.Hej.value.AtomicValue)iter1.next());
                    Saxon.Hej.value.AtomicValue item2 = ((/*1430*/Saxon.Hej.value.AtomicValue)iter2.next());
                    if (((item1 == null) && (item2 == null))) {
                        return 0;
                    }
                    if ((item1 == null)) {
                        return -1;
                    } else {
                        if ((item2 == null)) {
                            return +(1);
                        }
                    }
                    int c = compareItems(item1, item2);
                    if ((c != 0)) {
                        return c;
                    }
                }
            }
        }
        public int compareItems(Saxon.Hej.value.AtomicValue a1, Saxon.Hej.value.AtomicValue a2) {
            char c1 = a1.getItemType().getBasicAlphaCode()[1];
            if ((c1 == 'Z')) {
                c1 = 'S';
            }
            char c2 = a2.getItemType().getBasicAlphaCode()[1];
            if ((c2 == 'Z')) {
                c2 = 'S';
            }
            if ((c1 != c2)) {
                return Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING;
            }
            switch (c1) {
              case 'S':
              case 'Z':
                return Saxon.Hej.expr.sort.CodepointCollator.getInstance().compareStrings(a1.getUnicodeStringValue(), a2.getUnicodeStringValue());
              case 'B':
                return (a1.Equals(a2) ? 0 : Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING);
              case 'R':
                Saxon.Hej.value.DurationValue dur1 = (Saxon.Hej.value.DurationValue)(a1);
                Saxon.Hej.value.DurationValue dur2 = (Saxon.Hej.value.DurationValue)(a2);
                return dur1.getSchemaComparable().compareTo(dur2.getSchemaComparable());
              case 'M':
                Saxon.Hej.value.DateTimeValue dt1 = (Saxon.Hej.value.DateTimeValue)(a1);
                Saxon.Hej.value.DateTimeValue dt2 = (Saxon.Hej.value.DateTimeValue)(a2);
                return dt1.getSchemaComparable().compareTo(dt2.getSchemaComparable());
              case 'A':
              case 'G':
              case 'H':
              case 'I':
              case 'J':
              case 'K':
                Saxon.Hej.value.GDateValue gd1 = (Saxon.Hej.value.GDateValue)(a1);
                Saxon.Hej.value.GDateValue gd2 = (Saxon.Hej.value.GDateValue)(a2);
                return gd1.getSchemaComparable().compareTo(gd2.getSchemaComparable());
              case 'T':
                Saxon.Hej.value.TimeValue t1 = (Saxon.Hej.value.TimeValue)(a1);
                Saxon.Hej.value.TimeValue t2 = (Saxon.Hej.value.TimeValue)(a2);
                return t1.getSchemaComparable().compareTo(t2.getSchemaComparable());
              case 'X':
                Saxon.Hej.value.HexBinaryValue h1 = (Saxon.Hej.value.HexBinaryValue)(a1);
                Saxon.Hej.value.HexBinaryValue h2 = (Saxon.Hej.value.HexBinaryValue)(a2);
                return (Saxon.Ejava.util.Arrays.Equals(h1.getBinaryValue(), h2.getBinaryValue()) ? 0 : Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING);
              case '2':
                Saxon.Hej.value.Base64BinaryValue x1 = (Saxon.Hej.value.Base64BinaryValue)(a1);
                Saxon.Hej.value.Base64BinaryValue x2 = (Saxon.Hej.value.Base64BinaryValue)(a2);
                return (Saxon.Ejava.util.Arrays.Equals(x1.getBinaryValue(), x2.getBinaryValue()) ? 0 : Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING);
              case 'U':
                return (a1.Equals(a2) ? 0 : Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING);
              case 'Q':
                return (a1.Equals(a2) ? 0 : Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING);
              case 'N':
                return (a1.Equals(a2) ? 0 : Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING);
              case 'D':
                if (((a1 is Saxon.Hej.value.Int64Value) && (a2 is Saxon.Hej.value.Int64Value))) {
                    return ((Saxon.Hej.value.Int64Value)(a1)).longValue().CompareTo(((Saxon.Hej.value.Int64Value)(a2)).longValue());
                } else {
                    return ((Saxon.Hej.value.DecimalValue)(a1)).getDecimalValue().CompareTo(((Saxon.Hej.value.DecimalValue)(a2)).getDecimalValue());
                }
              case 'F':
                float f1 = ((Saxon.Hej.value.FloatValue)(a1)).getFloatValue();
                float f2 = ((Saxon.Hej.value.FloatValue)(a2)).getFloatValue();
                if ((f1 == f2)) {
                    return 0;
                } else {
                    return ((f1<f2) ? -1 : 1);
                }
              case 'O':
                double d1 = ((Saxon.Hej.value.DoubleValue)(a1)).getDoubleValue();
                double d2 = ((Saxon.Hej.value.DoubleValue)(a2)).getDoubleValue();
                if ((d1 == d2)) {
                    return 0;
                } else {
                    return ((d1<d2) ? -1 : 1);
                }
              default:
                throw new Saxon.Ejava.lang.IllegalStateException();
            }
        }
        public virtual bool equal(Saxon.Hej.om.AtomicSequence a, Saxon.Hej.om.AtomicSequence b) {
            return (compareSequences(a, b) == 0);
        }
        public int hash(Saxon.Hej.om.AtomicSequence a) {
            int h = 0x56825682;
            foreach (Saxon.Hej.value.AtomicValue val in a) {
                char type = val.getItemType().getBasicAlphaCode()[1];
                switch (type) {
                  case 'S':
                  case 'Z':
                    h ^= val.getUnicodeStringValue().GetHashCode();
                    break ;
                  case 'B':
                    h ^= ((Saxon.Hej.value.BooleanValue)(val)).GetHashCode();
                    break ;
                  case 'R':
                    h ^= ((Saxon.Hej.value.DurationValue)(val)).getSchemaComparable().GetHashCode();
                    break ;
                  case 'M':
                    h ^= ((Saxon.Hej.value.DateTimeValue)(val)).getSchemaComparable().GetHashCode();
                    break ;
                  case 'A':
                  case 'G':
                  case 'H':
                  case 'I':
                  case 'J':
                  case 'K':
                    h ^= ((Saxon.Hej.value.GDateValue)(val)).getSchemaComparable().GetHashCode();
                    break ;
                  case 'T':
                    h ^= ((Saxon.Hej.value.TimeValue)(val)).getSchemaComparable().GetHashCode();
                    break ;
                  case 'X':
                    h ^= Saxon.Ejava.util.Arrays.hashCode(((Saxon.Hej.value.HexBinaryValue)(val)).getBinaryValue());
                    break ;
                  case '2':
                    h ^= Saxon.Ejava.util.Arrays.hashCode(((Saxon.Hej.value.Base64BinaryValue)(val)).getBinaryValue());
                    break ;
                  case 'U':
                    h ^= ((Saxon.Hej.value.AnyURIValue)(val)).getUnicodeStringValue().GetHashCode();
                    break ;
                  case 'Q':
                  case 'N':
                    h ^= ((Saxon.Hej.value.QualifiedNameValue)(val)).getStructuredQName().GetHashCode();
                    break ;
                  case 'D':
                  case 'F':
                  case 'O':
                    h ^= val.GetHashCode();
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException(("Alphacode "+type));
                }
                h <<= 1;
            }
            return h;
        }
        public bool equalOrIdentical(Saxon.Hej.om.AtomicSequence a1, Saxon.Hej.om.AtomicSequence a2) {
            if (((a1.getLength() == 1) && (a2.getLength() == 1))) {
                Saxon.Hej.value.AtomicValue v1 = ((/*1430*/Saxon.Hej.value.AtomicValue)a1.head());
                Saxon.Hej.value.AtomicValue v2 = ((/*1430*/Saxon.Hej.value.AtomicValue)a2.head());
                return ((compareItems(v1, v2) == 0) || ((v1.isNaN() && v2.isNaN())));
            } else {
                Saxon.Hej.tree.iter.AtomicIterator iter1 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)a1.iterate());
                Saxon.Hej.tree.iter.AtomicIterator iter2 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)a2.iterate());
                while (true) {
                    Saxon.Hej.value.AtomicValue item1 = ((/*1430*/Saxon.Hej.value.AtomicValue)iter1.next());
                    Saxon.Hej.value.AtomicValue item2 = ((/*1430*/Saxon.Hej.value.AtomicValue)iter2.next());
                    if (((item1 == null) && (item2 == null))) {
                        return true;
                    }
                    if (((item1 == null) || (item2 == null))) {
                        return false;
                    }
                    if (((compareItems(item1, item2) == 0) || ((item1.isNaN() && item2.isNaN())))) {
                        continue ;
                    }
                    return false;
                }
            }
        }
    
    }
}