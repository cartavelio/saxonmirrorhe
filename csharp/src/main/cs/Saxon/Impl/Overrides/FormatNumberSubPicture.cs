using Singulink.Numerics;

namespace Saxon.Impl.Overrides {
    public class FormatNumberSubPicture : Saxon.Hej.functions.FormatNumber.SubPicture {
        public FormatNumberSubPicture(Saxon.Hej.str.UnicodeString pic, Saxon.Hej.trans.DecimalSymbols dfs) : base(pic, dfs) {
        }
        // Override the Java handling of formatDecimal().
        protected override void formatDecimal(BigDecimal dval, Saxon.Ejava.lang.StringBuilder fsb) {
            int exponent = 0;
            if (minExponentSize != 0) {
                if (dval.IsZero) {
                    exponent = 0;
                } else if (dval.DecimalPlaces > 0) {
                    exponent = dval.Precision - dval.DecimalPlaces - scalingFactor;
                } else {
                    exponent = dval.ToString().Length - scalingFactor;
                }
                dval = dval * Singulink.Numerics.BigDecimal.Pow10(-exponent);
            }

            if (dval.DecimalPlaces > maxFractionPartSize) {
                dval = Singulink.Numerics.BigDecimal.Round(dval, maxFractionPartSize);
            }

            Saxon.Hej.value.BigDecimalValue.decimalToString(dval, fsb);
            if (dval.DecimalPlaces < minFractionPartSize) {
                if (dval.DecimalPlaces == 0) {
                    fsb.append('.');
                }

                Saxon.Hej.str.StringTool.appendRepeated(fsb, '0', minFractionPartSize - dval.DecimalPlaces);
            }

            int point = fsb.indexOf(".");
            int intDigits;
            if ((point >= 0)) {
                int zz = (maxFractionPartSize - minFractionPartSize);
                while ((zz > 0)) {
                    if ((fsb.charAt((fsb.length() - 1)) == '0')) {
                        fsb.setLength((fsb.length() - 1));
                        zz--;
                    }
                    else {
                        break;
                    }
                }

                intDigits = point;
                if ((fsb.charAt((fsb.length() - 1)) == '.')) {
                    if (minFractionPartSize > 0) {
                        fsb.append('0');
                    }
                    else {
                        fsb.setLength((fsb.length() - 1));
                    }
                }
            }
            else {
                intDigits = fsb.length();
                if ((minFractionPartSize > 0)) {
                    fsb.append('.');
                    for (int i = 0; (i < minFractionPartSize); i++) {
                        fsb.append('0');
                    }
                }
            }

            if (minWholePartSize == 0 && intDigits == 1 && fsb.charAt(0) == '0') {
                fsb.deleteCharAt(0);
            }
            else {
                if ((minWholePartSize > intDigits)) {
                    Saxon.Hej.str.StringTool.prependRepeated(fsb, '0', (minWholePartSize - intDigits));
                }
            }

            if ((minExponentSize != 0)) {
                fsb.append('e');
                Saxon.Hej.value.IntegerValue exp =
                    (Saxon.Hej.value.IntegerValue) (Saxon.Hej.value.IntegerValue.fromDouble(exponent));
                string expStr = exp.getUnicodeStringValue().ToString();
                char first = expStr[0];
                if ((first == '-')) {
                    fsb.append('-');
                    expStr = expStr.Substring(1);
                }

                int length = expStr.Length;
                if ((length < minExponentSize)) {
                    int zz = (minExponentSize - length);
                    for (int i = 0; (i < zz); i++) {
                        fsb.append('0');
                    }
                }

                fsb.append(expStr);
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited