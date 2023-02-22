using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class FloatingPointConverter {
        public static Saxon.Hej.value.FloatingPointConverter THE_INSTANCE = new Saxon.Hej.value.FloatingPointConverter();
        private FloatingPointConverter()  {
        }
        private const string NEGATIVE_INFINITY = "-INF";
        private const string POSITIVE_INFINITY = "INF";
        private const string NaN = "NaN";
        private static readonly char[] charForDigit = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        public const long DOUBLE_SIGN_MASK = unchecked((long)0x8000000000000000L);
        private const long doubleExpMask = 0x7ff0000000000000L;
        private const int doubleExpShift = 52;
        private const int doubleExpBias = 1023;
        private const long doubleFractMask = unchecked((long)0xfffffffffffffL);
        public const int FLOAT_SIGN_MASK = unchecked((int)0x80000000);
        private const int floatExpMask = 0x7f800000;
        private const int floatExpShift = 23;
        private const int floatExpBias = 127;
        private const int floatFractMask = 0x7fffff;
        private static readonly System.Numerics.BigInteger TEN = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(10);
        private static readonly System.Numerics.BigInteger NINE = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(9);
        public static Saxon.Hej.str.UnicodeBuilder appendInt(Saxon.Hej.str.UnicodeBuilder s, int i) {
            if ((i<0)) {
                if ((i == System.Int32.MinValue)) {
                    s.append("-2147483648");
                    return s;
                }
                s.append('-');
                i = -(i);
            }
            int c;
            if ((i<10)) {
                s.append(charForDigit[i]);
                return s;
            } else {
                if ((i<100)) {
                    s.append(charForDigit[(i/10)]);
                    s.append(charForDigit[(i%10)]);
                    return s;
                } else {
                    if ((i<1000)) {
                        s.append(charForDigit[(i/100)]);
                        s.append(charForDigit[((c = (i%100))/10)]);
                        s.append(charForDigit[(c%10)]);
                        return s;
                    } else {
                        if ((i<10000)) {
                            s.append(charForDigit[(i/1000)]);
                            s.append(charForDigit[((c = (i%1000))/100)]);
                            s.append(charForDigit[((c %= 100)/10)]);
                            s.append(charForDigit[(c%10)]);
                            return s;
                        } else {
                            if ((i<100000)) {
                                s.append(charForDigit[(i/10000)]);
                                s.append(charForDigit[((c = (i%10000))/1000)]);
                                s.append(charForDigit[((c %= 1000)/100)]);
                                s.append(charForDigit[((c %= 100)/10)]);
                                s.append(charForDigit[(c%10)]);
                                return s;
                            } else {
                                if ((i<1000000)) {
                                    s.append(charForDigit[(i/100000)]);
                                    s.append(charForDigit[((c = (i%100000))/10000)]);
                                    s.append(charForDigit[((c %= 10000)/1000)]);
                                    s.append(charForDigit[((c %= 1000)/100)]);
                                    s.append(charForDigit[((c %= 100)/10)]);
                                    s.append(charForDigit[(c%10)]);
                                    return s;
                                } else {
                                    if ((i<10000000)) {
                                        s.append(charForDigit[(i/1000000)]);
                                        s.append(charForDigit[((c = (i%1000000))/100000)]);
                                        s.append(charForDigit[((c %= 100000)/10000)]);
                                        s.append(charForDigit[((c %= 10000)/1000)]);
                                        s.append(charForDigit[((c %= 1000)/100)]);
                                        s.append(charForDigit[((c %= 100)/10)]);
                                        s.append(charForDigit[(c%10)]);
                                        return s;
                                    } else {
                                        if ((i<100000000)) {
                                            s.append(charForDigit[(i/10000000)]);
                                            s.append(charForDigit[((c = (i%10000000))/1000000)]);
                                            s.append(charForDigit[((c %= 1000000)/100000)]);
                                            s.append(charForDigit[((c %= 100000)/10000)]);
                                            s.append(charForDigit[((c %= 10000)/1000)]);
                                            s.append(charForDigit[((c %= 1000)/100)]);
                                            s.append(charForDigit[((c %= 100)/10)]);
                                            s.append(charForDigit[(c%10)]);
                                            return s;
                                        } else {
                                            if ((i<1000000000)) {
                                                s.append(charForDigit[(i/100000000)]);
                                                s.append(charForDigit[((c = (i%100000000))/10000000)]);
                                                s.append(charForDigit[((c %= 10000000)/1000000)]);
                                                s.append(charForDigit[((c %= 1000000)/100000)]);
                                                s.append(charForDigit[((c %= 100000)/10000)]);
                                                s.append(charForDigit[((c %= 10000)/1000)]);
                                                s.append(charForDigit[((c %= 1000)/100)]);
                                                s.append(charForDigit[((c %= 100)/10)]);
                                                s.append(charForDigit[(c%10)]);
                                                return s;
                                            } else {
                                                s.append(charForDigit[(i/1000000000)]);
                                                s.append(charForDigit[((c = (i%1000000000))/100000000)]);
                                                s.append(charForDigit[((c %= 100000000)/10000000)]);
                                                s.append(charForDigit[((c %= 10000000)/1000000)]);
                                                s.append(charForDigit[((c %= 1000000)/100000)]);
                                                s.append(charForDigit[((c %= 100000)/10000)]);
                                                s.append(charForDigit[((c %= 10000)/1000)]);
                                                s.append(charForDigit[((c %= 1000)/100)]);
                                                s.append(charForDigit[((c %= 100)/10)]);
                                                s.append(charForDigit[(c%10)]);
                                                return s;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void fppfpp(Saxon.Hej.str.UnicodeBuilder sb, int e, long f, int p) {
            long R = (f<<System.Math.Max((e-p), 0));
            long S = (1L<<System.Math.Max(0, -(((e-p)))));
            long Mminus = (1L<<System.Math.Max((e-p), 0));
            long Mplus = Mminus;
            bool initial = true;
            if ((f == (1L<<((p-1))))) {
                Mplus = (Mplus<<1);
                R = (R<<1);
                S = (S<<1);
            }
            int k = 0;
            while ((R<(((S+9))/10))) {
                k--;
                R = (R*10);
                Mminus = (Mminus*10);
                Mplus = (Mplus*10);
            }
            while ((((2*R)+Mplus)>=(2*S))) {
                S = (S*10);
                k++;
            }
            for (int z = k; (z<0); z++) {
                if (initial) {
                    sb.append("0.");
                }
                initial = false;
                sb.append('0');
            }
            bool low;
            bool high;
            int U;
            while (true) {
                k--;
                long R10 = (R*10);
                U = (int)(((R10/S)));
                R = (R10-((U*S)));
                Mminus = (Mminus*10);
                Mplus = (Mplus*10);
                low = ((2*R)<Mminus);
                high = ((2*R)>((2*S)-Mplus));
                if ((low || high)) {
                    break ;
                }
                if ((k == -1)) {
                    if (initial) {
                        sb.append('0');
                    }
                    sb.append('.');
                }
                sb.append(charForDigit[U]);
                initial = false;
            }
            if ((high && ((!(low) || ((2*R)>S))))) {
                U++;
            }
            if ((k == -1)) {
                if (initial) {
                    sb.append('0');
                }
                sb.append('.');
            }
            sb.append(charForDigit[U]);
            for (int z = 0; (z<k); z++) {
                sb.append('0');
            }
        }
        private static void fppfppBig(Saxon.Hej.str.UnicodeBuilder sb, int e, long f, int p) {
            System.Numerics.BigInteger R = ((Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(f)) << (System.Math.Max((e-p), 0)));
            System.Numerics.BigInteger S = ((System.Numerics.BigInteger.One) << (System.Math.Max(0, -(((e-p))))));
            System.Numerics.BigInteger Mminus = ((System.Numerics.BigInteger.One) << (System.Math.Max((e-p), 0)));
            System.Numerics.BigInteger Mplus = Mminus;
            bool initial = true;
            if ((f == (1L<<((p-1))))) {
                Mplus = ((Mplus) << (1));
                R = ((R) << (1));
                S = ((S) << (1));
            }
            int k = 0;
            while ((R.CompareTo(System.Numerics.BigInteger.Divide(System.Numerics.BigInteger.Add(S, NINE), TEN))<0)) {
                k--;
                R = System.Numerics.BigInteger.Multiply(R, TEN);
                Mminus = System.Numerics.BigInteger.Multiply(Mminus, TEN);
                Mplus = System.Numerics.BigInteger.Multiply(Mplus, TEN);
            }
            while ((System.Numerics.BigInteger.Add(((R) << (1)), Mplus).CompareTo(((S) << (1)))>=0)) {
                S = System.Numerics.BigInteger.Multiply(S, TEN);
                k++;
            }
            for (int z = k; (z<0); z++) {
                if (initial) {
                    sb.append("0.");
                }
                initial = false;
                sb.append('0');
            }
            bool low;
            bool high;
            int U;
            while (true) {
                k--;
                System.Numerics.BigInteger R10 = System.Numerics.BigInteger.Multiply(R, TEN);
                U = ((int)System.Numerics.BigInteger.Divide(R10, S));
                R = Saxon.Impl.Helpers.BigIntegerUtils.Mod(R10, S);
                Mminus = System.Numerics.BigInteger.Multiply(Mminus, TEN);
                Mplus = System.Numerics.BigInteger.Multiply(Mplus, TEN);
                System.Numerics.BigInteger R2 = ((R) << (1));
                low = (R2.CompareTo(Mminus)<0);
                high = (R2.CompareTo(System.Numerics.BigInteger.Subtract(((S) << (1)), Mplus))>0);
                if ((low || high)) {
                    break ;
                }
                if ((k == -1)) {
                    if (initial) {
                        sb.append('0');
                    }
                    sb.append('.');
                }
                sb.append(charForDigit[U]);
                initial = false;
            }
            if ((high && ((!(low) || (((R) << (1)).CompareTo(S)>0))))) {
                U++;
            }
            if ((k == -1)) {
                if (initial) {
                    sb.append('0');
                }
                sb.append('.');
            }
            sb.append(charForDigit[U]);
            for (int z = 0; (z<k); z++) {
                sb.append('0');
            }
        }
        private static void fppfppExponential(Saxon.Hej.str.UnicodeBuilder sb, int e, long f, int p) {
            System.Numerics.BigInteger R = ((Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(f)) << (System.Math.Max((e-p), 0)));
            System.Numerics.BigInteger S = ((System.Numerics.BigInteger.One) << (System.Math.Max(0, -(((e-p))))));
            System.Numerics.BigInteger Mminus = ((System.Numerics.BigInteger.One) << (System.Math.Max((e-p), 0)));
            System.Numerics.BigInteger Mplus = Mminus;
            bool initial = true;
            bool doneDot = false;
            if ((f == (1L<<((p-1))))) {
                Mplus = ((Mplus) << (1));
                R = ((R) << (1));
                S = ((S) << (1));
            }
            int k = 0;
            while ((R.CompareTo(System.Numerics.BigInteger.Divide(System.Numerics.BigInteger.Add(S, NINE), TEN))<0)) {
                k--;
                R = System.Numerics.BigInteger.Multiply(R, TEN);
                Mminus = System.Numerics.BigInteger.Multiply(Mminus, TEN);
                Mplus = System.Numerics.BigInteger.Multiply(Mplus, TEN);
            }
            while ((System.Numerics.BigInteger.Add(((R) << (1)), Mplus).CompareTo(((S) << (1)))>=0)) {
                S = System.Numerics.BigInteger.Multiply(S, TEN);
                k++;
            }
            int H = (k-1);
            bool low;
            bool high;
            int U;
            while (true) {
                k--;
                System.Numerics.BigInteger R10 = System.Numerics.BigInteger.Multiply(R, TEN);
                U = ((int)System.Numerics.BigInteger.Divide(R10, S));
                R = Saxon.Impl.Helpers.BigIntegerUtils.Mod(R10, S);
                Mminus = System.Numerics.BigInteger.Multiply(Mminus, TEN);
                Mplus = System.Numerics.BigInteger.Multiply(Mplus, TEN);
                System.Numerics.BigInteger R2 = ((R) << (1));
                low = (R2.CompareTo(Mminus)<0);
                high = (R2.CompareTo(System.Numerics.BigInteger.Subtract(((S) << (1)), Mplus))>0);
                if ((low || high)) {
                    break ;
                }
                sb.append(charForDigit[U]);
                if (initial) {
                    sb.append('.');
                    doneDot = true;
                }
                initial = false;
            }
            if ((high && ((!(low) || (((R) << (1)).CompareTo(S)>0))))) {
                U++;
            }
            sb.append(charForDigit[U]);
            if (!(doneDot)) {
                sb.append(".0");
            }
            sb.append('E');
            appendInt(sb, H);
        }
        public static Saxon.Hej.str.UnicodeString convertDouble(double d, bool forceExponential) {
            Saxon.Hej.str.UnicodeBuilder s = new Saxon.Hej.str.UnicodeBuilder(32);
            if ((d == System.Double.NegativeInfinity)) {
                s.appendLatin(NEGATIVE_INFINITY);
            } else {
                if ((d == System.Double.PositiveInfinity)) {
                    s.appendLatin(POSITIVE_INFINITY);
                } else {
                    if (System.Double.IsNaN(d)) {
                        s.appendLatin(NaN);
                    } else {
                        if ((d == 0.0)) {
                            if ((((System.BitConverter.DoubleToInt64Bits(d)&DOUBLE_SIGN_MASK)) != 0)) {
                                s.append('-');
                            }
                            s.append('0');
                            if (forceExponential) {
                                s.append(".0E0");
                            }
                        } else {
                            if ((d == System.Double.MaxValue)) {
                                s.append("1.7976931348623157E308");
                            } else {
                                if ((d == -(System.Double.MaxValue))) {
                                    s.append("-1.7976931348623157E308");
                                } else {
                                    if ((d == System.Double.MinValue)) {
                                        s.append("4.9E-324");
                                    } else {
                                        if ((d == -(System.Double.MinValue))) {
                                            s.append("-4.9E-324");
                                        } else {
                                            if ((d<0)) {
                                                s.append('-');
                                                d = -(d);
                                            }
                                            long bits = System.BitConverter.DoubleToInt64Bits(d);
                                            long fraction = (((1L<<52))|((bits&doubleFractMask)));
                                            long rawExp = (((bits&doubleExpMask))>>doubleExpShift);
                                            int exp = ((int)(rawExp)-doubleExpBias);
                                            if ((rawExp == 0)) {
                                                s.append((d).ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                return s.toUnicodeString();
                                            }
                                            if ((forceExponential || (((d>=1000000) || (d<0.000001))))) {
                                                fppfppExponential(s, exp, fraction, 52);
                                            } else {
                                                if ((d<=0.01)) {
                                                    fppfppBig(s, exp, fraction, 52);
                                                } else {
                                                    fppfpp(s, exp, fraction, 52);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return s.toUnicodeString();
        }
        public static Saxon.Hej.str.UnicodeString appendFloat(Saxon.Hej.str.UnicodeBuilder s, float f, bool forceExponential) {
            if ((f == System.Single.NegativeInfinity)) {
                s.append(NEGATIVE_INFINITY);
            } else {
                if ((f == System.Single.PositiveInfinity)) {
                    s.append(POSITIVE_INFINITY);
                } else {
                    if (System.Single.IsNaN(f)) {
                        s.append(NaN);
                    } else {
                        if ((f == 0.0)) {
                            if ((((System.BitConverter.SingleToInt32Bits(f)&FLOAT_SIGN_MASK)) != 0)) {
                                s.append('-');
                            }
                            s.append('0');
                        } else {
                            if ((f == System.Single.MaxValue)) {
                                s.appendLatin("3.4028235E38");
                            } else {
                                if ((f == -(System.Single.MaxValue))) {
                                    s.appendLatin("-3.4028235E38");
                                } else {
                                    if ((f == System.Single.MinValue)) {
                                        s.appendLatin("1.4E-45");
                                    } else {
                                        if ((f == -(System.Single.MinValue))) {
                                            s.appendLatin("-1.4E-45");
                                        } else {
                                            if ((f<0)) {
                                                s.append('-');
                                                f = -(f);
                                            }
                                            int bits = System.BitConverter.SingleToInt32Bits(f);
                                            int fraction = (((1<<23))|((bits&floatFractMask)));
                                            int rawExp = ((((bits&floatExpMask))>>floatExpShift));
                                            int exp = (rawExp-floatExpBias);
                                            int precision = 23;
                                            if ((rawExp == 0)) {
                                                s.append((f).ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                return s.toUnicodeString();
                                            }
                                            if ((forceExponential || (((f>=1000000) || (f<0.000001F))))) {
                                                fppfppExponential(s, exp, fraction, precision);
                                            } else {
                                                fppfpp(s, exp, fraction, precision);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return s.toUnicodeString();
        }
    
    }
}