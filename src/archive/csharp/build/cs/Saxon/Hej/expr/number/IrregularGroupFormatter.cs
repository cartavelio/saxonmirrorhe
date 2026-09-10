using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public class IrregularGroupFormatter : Saxon.Hej.expr.number.NumericGroupFormatter {
        private Saxon.Hej.z.IntSet groupingPositions = null;
        private System.Collections.Generic.IList<int> separators = null;
        public IrregularGroupFormatter(Saxon.Hej.z.IntSet groupingPositions, System.Collections.Generic.IList<int> sep, Saxon.Hej.str.UnicodeString adjustedPicture)  {
            this.groupingPositions = groupingPositions;
            separators = sep;
            this.adjustedPicture = adjustedPicture;
        }
        public override string format(string value) {
            Saxon.Hej.value.StringValue @in = new Saxon.Hej.value.StringValue(value);
            int l, m = 0;
            for (l = 0; (l<@in.length()); l++) {
                if (groupingPositions.contains(l)) {
                    m++;
                }
            }
            int[] @out = new int[(@in.length32()+m)];
            int j = 0;
            int k = (@out.Length-1);
            for (int i = (@in.length32()-1); (i>=0); i--) {
                @out[k--] = @in.getContent().codePointAt(i);
                if ((((i>0)) && groupingPositions.contains((@in.length32()-i)))) {
                    @out[k--] = separators[j++];
                }
            }
            return Saxon.Hej.str.StringTool.fromCodePoints(@out, @out.Length).ToString();
        }
        public override string getSeparator() {
            if ((separators.Count == 0)) {
                return null;
            } else {
                int sep = separators[(separators.Count-1)];
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(16);
                fsb.appendCodePoint(sep);
                return fsb.ToString();
            }
        }
    
    }
}