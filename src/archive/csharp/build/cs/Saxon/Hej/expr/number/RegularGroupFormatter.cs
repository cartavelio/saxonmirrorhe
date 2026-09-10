using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public class RegularGroupFormatter : Saxon.Hej.expr.number.NumericGroupFormatter {
        private readonly int groupSize;
        private readonly string groupSeparator;
        public RegularGroupFormatter(int grpSize, string grpSep, Saxon.Hej.str.UnicodeString adjustedPicture)  {
            groupSize = grpSize;
            groupSeparator = grpSep;
            this.adjustedPicture = adjustedPicture;
        }
        public override string format(string value) {
            if (((groupSize>0) && (groupSeparator.Length>0))) {
                Saxon.Hej.str.UnicodeString valueEx = Saxon.Hej.str.StringView.tidy(value);
                Saxon.Ejava.lang.StringBuilder temp = new Saxon.Ejava.lang.StringBuilder(16);
                for (int i = (valueEx.length32()-1), j = 0; (i>=0); i--, j++) {
                    if (((j != 0) && (((j%groupSize)) == 0))) {
                        temp.insert(0, groupSeparator);
                    }
                    Saxon.Hej.str.StringTool.prependWideChar(temp, valueEx.codePointAt(i));
                }
                return temp.ToString();
            } else {
                return value.ToString();
            }
        }
        public override string getSeparator() {
            return groupSeparator;
        }
    
    }
}