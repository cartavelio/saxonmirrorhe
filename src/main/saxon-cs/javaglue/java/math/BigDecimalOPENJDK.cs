using System;
using System.Collections.Generic;
using System.Numerics;

namespace java.math {
    public class BigDecimalOPENJDK : java.lang.Number, java.lang.Comparable<java.math.BigDecimal> {
        private const long LONG_MASK = 0xffffffffL;
        private readonly BigInteger intVal;
        private readonly int _scale;
        private int _precision;
        private string stringCache;
        private static readonly long INFLATED = long.MinValue;
        private static readonly BigInteger INFLATED_BIGINT = (BigInteger) INFLATED;
        private readonly long intCompact;
        private const int MAX_COMPACT_DIGITS = 18;
        private const long serialVersionUID = 6108874887143696463L;

        private static readonly java.math.BigDecimal[] ZERO_THROUGH_TEN = {
            new java.math.BigDecimal(BigInteger.ZERO, 0, 0, 1), new java.math.BigDecimal(BigInteger.ONE, 1, 0, 1),
            new java.math.BigDecimal(BigInteger.TWO, 2, 0, 1), new java.math.BigDecimal((BigInteger) 3, 3, 0, 1),
            new java.math.BigDecimal((BigInteger) 4, 4, 0, 1), new java.math.BigDecimal((BigInteger) 5, 5, 0, 1),
            new java.math.BigDecimal((BigInteger) 6, 6, 0, 1), new java.math.BigDecimal((BigInteger) 7, 7, 0, 1),
            new java.math.BigDecimal((BigInteger) 8, 8, 0, 1), new java.math.BigDecimal((BigInteger) 9, 9, 0, 1),
            new java.math.BigDecimal(BigInteger.TEN, 10, 0, 2)
        };

        private static readonly java.math.BigDecimal[] ZERO_SCALED_BY = {
            ZERO_THROUGH_TEN[0], new java.math.BigDecimal(BigInteger.ZERO, 0, 1, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 2, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 3, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 4, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 5, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 6, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 7, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 8, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 9, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 10, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 11, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 12, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 13, 1),
            new java.math.BigDecimal(BigInteger.ZERO, 0, 14, 1), new java.math.BigDecimal(BigInteger.ZERO, 0, 15, 1)
        };

        private static readonly long HALF_LONG_MAX_VALUE = long.MaxValue / 2;
        private static readonly long HALF_LONG_MIN_VALUE = long.MinValue / 2;
        public static readonly java.math.BigDecimal ZERO = ZERO_THROUGH_TEN[0];
        public static readonly java.math.BigDecimal ONE = ZERO_THROUGH_TEN[1];
        public static readonly java.math.BigDecimal TEN = ZERO_THROUGH_TEN[10];
        private static readonly java.math.BigDecimal ONE_TENTH = valueOf(1L, 1);
        private static readonly java.math.BigDecimal ONE_HALF = valueOf(5L, 1);

        internal BigDecimal(BigInteger intVal, long val, int scale, int prec) {
            _scale = scale;
            _precision = prec;
            intCompact = val;
            this.intVal = intVal;
        }

        public BigDecimal(char[] @in, int offset, int len) : this(@in, offset, len, MathContext.UNLIMITED) {
        }

        public BigDecimal(char[] @in, int offset, int len, MathContext mc) {
            try {
                java.util.Objects.checkFromIndexSize(offset, len, @in.Length);
            }
            catch (System.IndexOutOfRangeException e) {
                throw new java.lang.NumberFormatException("Bad offset or len arguments for char[] input.");
            }

            var prec = 0;
            var scl = 0;
            long rs = 0;
            BigInteger rb = default(BigInteger);
            try {
                var isneg = false;
                if (@in[offset] == '-') {
                    isneg = true;
                    offset++;
                    len--;
                }
                else {
                    if (@in[offset] == '+') {
                        offset++;
                        len--;
                    }
                }

                var dot = false;
                long exp = 0;
                char c;
                var isCompact = len <= MAX_COMPACT_DIGITS;
                var idx = 0;
                if (isCompact) {
                    for (; len > 0; offset++, len--) {
                        c = @in[offset];
                        if (c == '0') {
                            if (prec == 0) {
                                prec = 1;
                            }
                            else {
                                if (rs != 0) {
                                    rs *= 10;
                                    ++prec;
                                }
                            }

                            if (dot) ++scl;
                        }
                        else {
                            if (c >= '1' && c <= '9') {
                                var digit = c - '0';
                                if (prec != 1 || rs != 0) ++prec;

                                rs = rs * 10 + digit;
                                if (dot) ++scl;
                            }
                            else {
                                if (c == '.') {
                                    if (dot)
                                        throw new java.lang.NumberFormatException("Character array" +
                                            " contains more than one decimal point.");

                                    dot = true;
                                }
                                else {
                                    if (java.lang.Character.isDigit(c)) {
                                        var digit = java.lang.Character.digit(c, 10);
                                        if (digit == 0) {
                                            if (prec == 0) {
                                                prec = 1;
                                            }
                                            else {
                                                if (rs != 0) {
                                                    rs *= 10;
                                                    ++prec;
                                                }
                                            }
                                        }
                                        else {
                                            if (prec != 1 || rs != 0) ++prec;

                                            rs = rs * 10 + digit;
                                        }

                                        if (dot) ++scl;
                                    }
                                    else {
                                        if (c == 'e' || c == 'E') {
                                            exp = parseExp(@in, offset, len);
                                            if ((int) exp != exp)
                                                throw new java.lang.NumberFormatException("Exponent overflow.");

                                            break;
                                        }
                                        else {
                                            throw new java.lang.NumberFormatException(
                                                "Character " + c +
                                                " is neither a decimal digit number, decimal point, nor" +
                                                " \"e\" notation exponential mark.");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (prec == 0) throw new java.lang.NumberFormatException("No digits found.");

                    if (exp != 0) scl = adjustScale(scl, exp);

                    rs = isneg ? -rs : rs;
                    var mcp = mc.precision;
                    var drop = prec - mcp;
                    if (mcp > 0 && drop > 0)
                        while (drop > 0) {
                            scl = checkScaleNonZero((long) scl - drop);
                            rs = divideAndRound(rs, LONG_TEN_POWERS_TABLE[drop], oldRoundingMode(mc.roundingMode));
                            prec = longDigitLength(rs);
                            drop = prec - mcp;
                        }
                }
                else {
                    var coeff = new char[len];
                    for (; len > 0; offset++, len--) {
                        c = @in[offset];
                        if (c >= '0' && c <= '9' || java.lang.Character.isDigit(c)) {
                            if (c == '0' || java.lang.Character.digit(c, 10) == 0) {
                                if (prec == 0) {
                                    coeff[idx] = c;
                                    prec = 1;
                                }
                                else {
                                    if (idx != 0) {
                                        coeff[idx++] = c;
                                        ++prec;
                                    }
                                }
                            }
                            else {
                                if (prec != 1 || idx != 0) ++prec;

                                coeff[idx++] = c;
                            }

                            if (dot) ++scl;

                            continue;
                        }

                        if (c == '.') {
                            if (dot)
                                throw new java.lang.NumberFormatException("Character array" +
                                                                          " contains more than one decimal point.");

                            dot = true;
                            continue;
                        }

                        if (c != 'e' && c != 'E')
                            throw new java.lang.NumberFormatException("Character array" +
                                                                      " is missing \"e\" notation exponential mark.");

                        exp = parseExp(@in, offset, len);
                        if ((int) exp != exp) throw new java.lang.NumberFormatException("Exponent overflow.");

                        break;
                    }

                    if (prec == 0) throw new java.lang.NumberFormatException("No digits found.");

                    if (exp != 0) scl = adjustScale(scl, exp);

                    rb = new BigInteger(coeff, isneg ? -1 : 1, prec);
                    rs = compactValFor(rb);
                    var mcp = mc.precision;
                    if (mcp > 0 && prec > mcp) {
                        if (rs == INFLATED) {
                            var drop = prec - mcp;
                            while (drop > 0) {
                                scl = checkScaleNonZero((long) scl - drop);
                                rb = divideAndRoundByTenPow(rb, drop, oldRoundingMode(mc.roundingMode));
                                rs = compactValFor(rb);
                                if (rs != INFLATED) {
                                    prec = longDigitLength(rs);
                                    break;
                                }

                                prec = bigDigitLength(rb);
                                drop = prec - mcp;
                            }
                        }

                        if (rs != INFLATED) {
                            var drop = prec - mcp;
                            while (drop > 0) {
                                scl = checkScaleNonZero((long) scl - drop);
                                rs = divideAndRound(rs, LONG_TEN_POWERS_TABLE[drop], oldRoundingMode(mc.roundingMode));
                                prec = longDigitLength(rs);
                                drop = prec - mcp;
                            }

                            rb = default(BigInteger);
                        }
                    }
                }
            }
            catch (Exception e) when (e is System.IndexOutOfRangeException ||
                                      e is java.lang.NegativeArraySizeException) {
                var nfe = new java.lang.NumberFormatException();
                nfe.initCause(e);
                throw nfe;
            }

            _scale = scl;
            _precision = prec;
            intCompact = rs;
            intVal = rb;
        }

        private int adjustScale(int scl, long exp) {
            var adjustedScale = scl - exp;
            if (adjustedScale > int.MaxValue || adjustedScale < int.MinValue)
                throw new java.lang.NumberFormatException("_scale out of range.");

            scl = (int) adjustedScale;
            return scl;
        }

        private static int compareBigIntegerMagnitude(BigInteger b1, BigInteger b2) {
            // compare the values ignoring sign
            return BigInteger.Abs(b1).CompareTo(BigInteger.Abs(b2));
        }

        private static long parseExp(char[] @in, int offset, int len) {
            long exp = 0;
            offset++;
            var c = @in[offset];
            len--;
            var negexp = c == '-';
            if (negexp || c == '+') {
                offset++;
                c = @in[offset];
                len--;
            }

            if (len <= 0) throw new java.lang.NumberFormatException("No exponent digits.");

            while (len > 10 && (c == '0' || java.lang.Character.digit(c, 10) == 0)) {
                offset++;
                c = @in[offset];
                len--;
            }

            if (len > 10) throw new java.lang.NumberFormatException("Too many nonzero exponent digits.");

            for (;; len--) {
                int v;
                if (c >= '0' && c <= '9') {
                    v = c - '0';
                }
                else {
                    v = java.lang.Character.digit(c, 10);
                    if (v < 0) throw new java.lang.NumberFormatException("Not a digit.");
                }

                exp = exp * 10 + v;
                if (len == 1) break;

                offset++;
                c = @in[offset];
            }

            if (negexp) exp = -exp;

            return exp;
        }

        public BigDecimal(char[] @in) : this(@in, 0, @in.Length) {
        }

        public BigDecimal(char[] @in, MathContext mc) : this(@in, 0, @in.Length, mc) {
        }

        public BigDecimal(string val) : this(val.ToCharArray(), 0, val.Length) {
        }

        public BigDecimal(string val, MathContext mc) : this(val.ToCharArray(), 0, val.Length, mc) {
        }

        public BigDecimal(double val) : this(val, MathContext.UNLIMITED) {
        }

        public BigDecimal(double val, MathContext mc) {
            if (java.lang.Double.isInfinite(val) || java.lang.Double.isNaN(val))
                throw new java.lang.NumberFormatException("Infinite or NaN");

            var valBits = java.lang.Double.doubleToLongBits(val);
            var sign = valBits >> 63 == 0 ? 1 : -1;
            var exponent = (int) ((valBits >> 52) & 0x7ffL);
            var significand = exponent == 0
                ? (valBits & ((1L << 52) - 1)) << 1
                : (valBits & ((1L << 52) - 1)) | (1L << 52);
            exponent -= 1075;
            if (significand == 0) {
                intVal = (BigInteger) 0;
                _scale = 0;
                intCompact = 0;
                _precision = 1;
                return;
            }

            while ((significand & 1) == 0) {
                significand >>= 1;
                exponent++;
            }

            var scl = 0;
            BigInteger rb;
            var compactVal = sign * significand;
            if (exponent == 0) {
                rb = compactVal == INFLATED ? INFLATED_BIGINT : default;
            }
            else {
                if (exponent < 0) {
                    rb = BigInteger.Pow(5, -exponent) * (compactVal);
                    scl = -exponent;
                }
                else {
                    rb = BigInteger.Pow(2, exponent) * (compactVal);
                }

                compactVal = compactValFor(rb);
            }

            var prec = 0;
            var mcp = mc.precision;
            if (mcp > 0) {
                var mode = oldRoundingMode(mc.roundingMode);
                int drop;
                if (compactVal == INFLATED) {
                    prec = bigDigitLength(rb);
                    drop = prec - mcp;
                    while (drop > 0) {
                        scl = checkScaleNonZero((long) scl - drop);
                        rb = divideAndRoundByTenPow(rb, drop, mode);
                        compactVal = compactValFor(rb);
                        if (compactVal != INFLATED) break;

                        prec = bigDigitLength(rb);
                        drop = prec - mcp;
                    }
                }

                if (compactVal != INFLATED) {
                    prec = longDigitLength(compactVal);
                    drop = prec - mcp;
                    while (drop > 0) {
                        scl = checkScaleNonZero((long) scl - drop);
                        compactVal = divideAndRound(compactVal, LONG_TEN_POWERS_TABLE[drop],
                            oldRoundingMode(mc.roundingMode));
                        prec = longDigitLength(compactVal);
                        drop = prec - mcp;
                    }

                    rb = default;
                }
            }

            intVal = rb;
            intCompact = compactVal;
            _scale = scl;
            _precision = prec;
        }

        public BigDecimal(BigInteger val) {
            _scale = 0;
            intVal = val;
            intCompact = compactValFor(val);
        }

        public BigDecimal(BigInteger val, MathContext mc) : this(val, 0, mc) {
        }

        public BigDecimal(BigInteger unscaledVal, int _scale) {
            intVal = unscaledVal;
            intCompact = compactValFor(unscaledVal);
            this._scale = _scale;
        }

        public BigDecimal(BigInteger unscaledVal, int _scale, MathContext mc) {
            var compactVal = compactValFor(unscaledVal);
            var mcp = mc.precision;
            var prec = 0;
            if (mcp > 0) {
                var mode = oldRoundingMode(mc.roundingMode);
                if (compactVal == INFLATED) {
                    prec = bigDigitLength(unscaledVal);
                    var drop = prec - mcp;
                    while (drop > 0) {
                        _scale = checkScaleNonZero((long) _scale - drop);
                        unscaledVal = divideAndRoundByTenPow(unscaledVal, drop, mode);
                        compactVal = compactValFor(unscaledVal);
                        if (compactVal != INFLATED) break;

                        prec = bigDigitLength(unscaledVal);
                        drop = prec - mcp;
                    }
                }

                if (compactVal != INFLATED) {
                    prec = longDigitLength(compactVal);
                    var drop = prec - mcp;
                    while (drop > 0) {
                        _scale = checkScaleNonZero((long) _scale - drop);
                        compactVal = divideAndRound(compactVal, LONG_TEN_POWERS_TABLE[drop], mode);
                        prec = longDigitLength(compactVal);
                        drop = prec - mcp;
                    }

                    unscaledVal = default;
                }
            }

            intVal = unscaledVal;
            intCompact = compactVal;
            this._scale = _scale;
            _precision = prec;
        }

        public BigDecimal(int val) {
            intCompact = val;
            _scale = 0;
            intVal = default;
        }

        public BigDecimal(int val, MathContext mc) {
            var mcp = mc.precision;
            long compactVal = val;
            var scl = 0;
            var prec = 0;
            if (mcp > 0) {
                prec = longDigitLength(compactVal);
                var drop = prec - mcp;
                while (drop > 0) {
                    scl = checkScaleNonZero((long) scl - drop);
                    compactVal = divideAndRound(compactVal, LONG_TEN_POWERS_TABLE[drop],
                        oldRoundingMode(mc.roundingMode));
                    prec = longDigitLength(compactVal);
                    drop = prec - mcp;
                }
            }

            intVal = default;
            intCompact = compactVal;
            _scale = scl;
            _precision = prec;
        }

        public BigDecimal(long val) {
            intCompact = val;
            intVal = val == INFLATED ? INFLATED_BIGINT : default;
            _scale = 0;
        }

        public BigDecimal(long val, MathContext mc) {
            var mcp = mc.precision;
            var mode = oldRoundingMode(mc.roundingMode);
            var prec = 0;
            var scl = 0;
            var rb = val == INFLATED ? INFLATED_BIGINT : default;
            if (mcp > 0) {
                if (val == INFLATED) {
                    prec = 19;
                    var drop = prec - mcp;
                    while (drop > 0) {
                        scl = checkScaleNonZero((long) scl - drop);
                        rb = divideAndRoundByTenPow(rb, drop, mode);
                        val = compactValFor(rb);
                        if (val != INFLATED) break;

                        prec = bigDigitLength(rb);
                        drop = prec - mcp;
                    }
                }

                if (val != INFLATED) {
                    prec = longDigitLength(val);
                    var drop = prec - mcp;
                    while (drop > 0) {
                        scl = checkScaleNonZero((long) scl - drop);
                        val = divideAndRound(val, LONG_TEN_POWERS_TABLE[drop], oldRoundingMode(mc.roundingMode));
                        prec = longDigitLength(val);
                        drop = prec - mcp;
                    }

                    rb = default;
                }
            }

            intVal = rb;
            intCompact = val;
            _scale = scl;
            _precision = prec;
        }

        public static java.math.BigDecimal valueOf(long unscaledVal, int _scale) {
            if (_scale == 0) {
                return valueOf(unscaledVal);
            }
            else {
                if (unscaledVal == 0) return zeroValueOf(_scale);
            }

            return new java.math.BigDecimal(unscaledVal == INFLATED ? INFLATED_BIGINT : default,
                unscaledVal, _scale, 0);
        }

        public static java.math.BigDecimal valueOf(long val) {
            if (val >= 0 && val < ZERO_THROUGH_TEN.Length) {
                return ZERO_THROUGH_TEN[(int) val];
            }
            else {
                if (val != INFLATED) return new java.math.BigDecimal(default, val, 0, 0);
            }

            return new java.math.BigDecimal(INFLATED_BIGINT, val, 0, 0);
        }

        private static java.math.BigDecimal valueOf(long unscaledVal, int _scale, int prec) {
            if (_scale == 0 && unscaledVal >= 0 && unscaledVal < ZERO_THROUGH_TEN.Length) {
                return ZERO_THROUGH_TEN[(int) unscaledVal];
            }
            else {
                if (unscaledVal == 0) return zeroValueOf(_scale);
            }

            return new java.math.BigDecimal(unscaledVal == INFLATED ? INFLATED_BIGINT : default,
                unscaledVal, _scale, prec);
        }

        private static java.math.BigDecimal valueOf(BigInteger intVal, int _scale, int prec) {
            var val = compactValFor(intVal);
            if (val == 0) {
                return zeroValueOf(_scale);
            }
            else {
                if (_scale == 0 && val >= 0 && val < ZERO_THROUGH_TEN.Length) return ZERO_THROUGH_TEN[(int) val];
            }

            return new java.math.BigDecimal(intVal, val, _scale, prec);
        }

        private static java.math.BigDecimal zeroValueOf(int _scale) {
            if (_scale >= 0 && _scale < ZERO_SCALED_BY.Length)
                return ZERO_SCALED_BY[_scale];
            else
                return new java.math.BigDecimal((BigInteger) 0, 0, _scale, 1);
        }

        public static java.math.BigDecimal valueOf(double val) {
            return new java.math.BigDecimal(val.ToString());
        }

        public java.math.BigDecimal add(java.math.BigDecimal augend) {
            if (intCompact != INFLATED) {
                if (augend.intCompact != INFLATED)
                    return add(intCompact, _scale, augend.intCompact, augend._scale);
                else
                    return add(intCompact, _scale, augend.intVal, augend._scale);
            }
            else {
                if (augend.intCompact != INFLATED)
                    return add(augend.intCompact, augend._scale, intVal, _scale);
                else
                    return add(intVal, _scale, augend.intVal, augend._scale);
            }
        }

        public java.math.BigDecimal add(java.math.BigDecimal augend, MathContext mc) {
            if (mc.precision == 0) return add(augend);

            var lhs = this;
            {
                var lhsIsZero = lhs.signum() == 0;
                var augendIsZero = augend.signum() == 0;
                if (lhsIsZero || augendIsZero) {
                    var preferredScale = System.Math.Max(lhs.scale(), augend.scale());
                    java.math.BigDecimal result;
                    if (lhsIsZero && augendIsZero) return zeroValueOf(preferredScale);

                    result = lhsIsZero ? doRound(augend, mc) : doRound(lhs, mc);
                    if (result.scale() == preferredScale) {
                        return result;
                    }
                    else {
                        if (result.scale() > preferredScale) {
                            return stripZerosToMatchScale(result.intVal, result.intCompact, result._scale,
                                preferredScale);
                        }
                        else {
                            var precisionDiff = mc.precision - result.precision();
                            var scaleDiff = preferredScale - result.scale();
                            if (precisionDiff >= scaleDiff)
                                return result.setScale(preferredScale);
                            else
                                return result.setScale(result.scale() + precisionDiff);
                        }
                    }
                }
            }
            var padding = (long) lhs._scale - augend._scale;
            if (padding != 0) {
                var arg = preAlign(lhs, augend, padding, mc);
                matchScale(arg);
                lhs = arg[0];
                augend = arg[1];
            }

            return doRound(lhs.inflated() + augend.inflated(), lhs._scale, mc);
        }

        private java.math.BigDecimal[] preAlign(java.math.BigDecimal lhs, java.math.BigDecimal augend, long padding,
            MathContext mc) {
            System.Diagnostics.Debug.Assert(padding != 0);
            java.math.BigDecimal big;
            java.math.BigDecimal small;
            if (padding < 0) {
                big = lhs;
                small = augend;
            }
            else {
                big = augend;
                small = lhs;
            }

            var estResultUlpScale = (long) big._scale - big.precision() + mc.precision;
            var smallHighDigitPos = (long) small._scale - small.precision() + 1;
            if (smallHighDigitPos > big._scale + 2 && smallHighDigitPos > estResultUlpScale + 2)
                small = valueOf(small.signum(),
                    checkScale(System.Math.Max(big._scale, estResultUlpScale) + 3));

            java.math.BigDecimal[] result = {big, small};
            return result;
        }

        public java.math.BigDecimal subtract(java.math.BigDecimal subtrahend) {
            if (intCompact != INFLATED) {
                if (subtrahend.intCompact != INFLATED)
                    return add(intCompact, _scale, -subtrahend.intCompact, subtrahend._scale);
                else
                    return add(intCompact, _scale, -subtrahend.intVal, subtrahend._scale);
            }
            else {
                if (subtrahend.intCompact != INFLATED)
                    return add(-subtrahend.intCompact, subtrahend._scale, intVal, _scale);
                else
                    return add(intVal, _scale, -subtrahend.intVal, subtrahend._scale);
            }
        }

        public java.math.BigDecimal subtract(java.math.BigDecimal subtrahend, MathContext mc) {
            if (mc.precision == 0) return subtract(subtrahend);

            return add(subtrahend.negate(), mc);
        }

        public java.math.BigDecimal multiply(java.math.BigDecimal multiplicand) {
            var productScale = checkScale((long) _scale + multiplicand._scale);
            if (intCompact != INFLATED) {
                if (multiplicand.intCompact != INFLATED)
                    return multiply(intCompact, multiplicand.intCompact, productScale);
                else
                    return multiply(intCompact, multiplicand.intVal, productScale);
            }
            else {
                if (multiplicand.intCompact != INFLATED)
                    return multiply(multiplicand.intCompact, intVal, productScale);
                else
                    return multiply(intVal, multiplicand.intVal, productScale);
            }
        }

        public java.math.BigDecimal multiply(java.math.BigDecimal multiplicand, MathContext mc) {
            if (mc.precision == 0) return multiply(multiplicand);

            var productScale = checkScale((long) _scale + multiplicand._scale);
            if (intCompact != INFLATED) {
                if (multiplicand.intCompact != INFLATED)
                    return multiplyAndRound(intCompact, multiplicand.intCompact, productScale, mc);
                else
                    return multiplyAndRound(intCompact, multiplicand.intVal, productScale, mc);
            }
            else {
                if (multiplicand.intCompact != INFLATED)
                    return multiplyAndRound(multiplicand.intCompact, intVal, productScale, mc);
                else
                    return multiplyAndRound(intVal, multiplicand.intVal, productScale, mc);
            }
        }

        public java.math.BigDecimal divide(java.math.BigDecimal divisor, int _scale, int roundingMode) {
            if (roundingMode < ROUND_UP || roundingMode > ROUND_UNNECESSARY)
                throw new java.lang.IllegalArgumentException("Invalid rounding mode");

            if (intCompact != INFLATED) {
                if (divisor.intCompact != INFLATED)
                    return divide(intCompact, this._scale, divisor.intCompact, divisor._scale, _scale,
                        roundingMode);
                else
                    return divide(intCompact, this._scale, divisor.intVal, divisor._scale, _scale, roundingMode);
            }
            else {
                if (divisor.intCompact != INFLATED)
                    return divide(intVal, this._scale, divisor.intCompact, divisor._scale, _scale, roundingMode);
                else
                    return divide(intVal, this._scale, divisor.intVal, divisor._scale, _scale, roundingMode);
            }
        }

        public java.math.BigDecimal divide(java.math.BigDecimal divisor, int _scale, RoundingMode roundingMode) {
            return divide(divisor, _scale, oldRoundingMode(roundingMode));
        }

        public java.math.BigDecimal divide(java.math.BigDecimal divisor, int roundingMode) {
            return divide(divisor, _scale, roundingMode);
        }

        public java.math.BigDecimal divide(java.math.BigDecimal divisor, RoundingMode roundingMode) {
            return divide(divisor, _scale, oldRoundingMode(roundingMode));
        }

        public java.math.BigDecimal divide(java.math.BigDecimal divisor) {
            if (divisor.signum() == 0) {
                if (signum() == 0) throw new System.ArithmeticException("Division undefined");

                throw new System.ArithmeticException("Division by zero");
            }

            var preferredScale = saturateLong((long) _scale - divisor._scale);
            if (signum() == 0) {
                return zeroValueOf(preferredScale);
            }
            else {
                var mc =
                    new MathContext(
                        (int) System.Math.Min(
                            precision() + (long) System.Math.Ceil(10.0 * divisor.precision() / 3.0),
                            int.MaxValue), RoundingMode.UNNECESSARY);
                java.math.BigDecimal quotient;
                try {
                    quotient = divide(divisor, mc);
                }
                catch (System.ArithmeticException e) {
                    throw new System.ArithmeticException("Non-terminating decimal expansion; " +
                                                         "no exact representable decimal result.");
                }

                var quotientScale = quotient.scale();
                if (preferredScale > quotientScale) return quotient.setScale(preferredScale, ROUND_UNNECESSARY);

                return quotient;
            }
        }

        public java.math.BigDecimal divide(java.math.BigDecimal divisor, MathContext mc) {
            var mcp = mc.precision;
            if (mcp == 0) return divide(divisor);

            var dividend = this;
            var preferredScale = (long) dividend._scale - divisor._scale;
            if (divisor.signum() == 0) {
                if (dividend.signum() == 0) throw new System.ArithmeticException("Division undefined");

                throw new System.ArithmeticException("Division by zero");
            }

            if (dividend.signum() == 0) return zeroValueOf(saturateLong(preferredScale));

            var xscale = dividend.precision();
            var yscale = divisor.precision();
            if (dividend.intCompact != INFLATED) {
                if (divisor.intCompact != INFLATED)
                    return divide(dividend.intCompact, xscale, divisor.intCompact, yscale, preferredScale, mc);
                else
                    return divide(dividend.intCompact, xscale, divisor.intVal, yscale, preferredScale, mc);
            }
            else {
                if (divisor.intCompact != INFLATED)
                    return divide(dividend.intVal, xscale, divisor.intCompact, yscale, preferredScale, mc);
                else
                    return divide(dividend.intVal, xscale, divisor.intVal, yscale, preferredScale, mc);
            }
        }

        public java.math.BigDecimal divideToIntegralValue(java.math.BigDecimal divisor) {
            var preferredScale = saturateLong((long) _scale - divisor._scale);
            if (compareMagnitude(divisor) < 0) return zeroValueOf(preferredScale);

            if (signum() == 0 && divisor.signum() != 0) return setScale(preferredScale, ROUND_UNNECESSARY);

            var maxDigits =
                (int) System.Math.Min(
                    precision() + (long) System.Math.Ceil(10.0 * divisor.precision() / 3.0) +
                    System.Math.Abs((long) scale() - divisor.scale()) + 2, int.MaxValue);
            var quotient = divide(divisor, new MathContext(maxDigits, RoundingMode.DOWN));
            if (quotient._scale > 0) {
                quotient = quotient.setScale(0, RoundingMode.DOWN);
                quotient = stripZerosToMatchScale(quotient.intVal, quotient.intCompact, quotient._scale,
                    preferredScale);
            }

            if (quotient._scale < preferredScale) quotient = quotient.setScale(preferredScale, ROUND_UNNECESSARY);

            return quotient;
        }

        public java.math.BigDecimal divideToIntegralValue(java.math.BigDecimal divisor, MathContext mc) {
            if (mc.precision == 0 || compareMagnitude(divisor) < 0) return divideToIntegralValue(divisor);

            var preferredScale = saturateLong((long) _scale - divisor._scale);
            var result = divide(divisor, new MathContext(mc.precision, RoundingMode.DOWN));
            if (result.scale() < 0) {
                var product = result.multiply(divisor);
                if (subtract(product).compareMagnitude(divisor) >= 0)
                    throw new System.ArithmeticException("Division impossible");
            }
            else {
                if (result.scale() > 0) result = result.setScale(0, RoundingMode.DOWN);
            }

            int precisionDiff;
            if (preferredScale > result.scale() && (precisionDiff = mc.precision - result.precision()) > 0)
                return result.setScale(result.scale() +
                                       System.Math.Min(precisionDiff, preferredScale - result._scale));
            else
                return stripZerosToMatchScale(result.intVal, result.intCompact, result._scale, preferredScale);
        }

        public java.math.BigDecimal remainder(java.math.BigDecimal divisor) {
            var divrem = divideAndRemainder(divisor);
            return divrem[1];
        }

        public java.math.BigDecimal remainder(java.math.BigDecimal divisor, MathContext mc) {
            var divrem = divideAndRemainder(divisor, mc);
            return divrem[1];
        }

        public java.math.BigDecimal[] divideAndRemainder(java.math.BigDecimal divisor) {
            var result = new java.math.BigDecimal[2];
            result[0] = divideToIntegralValue(divisor);
            result[1] = subtract(result[0].multiply(divisor));
            return result;
        }

        public java.math.BigDecimal[] divideAndRemainder(java.math.BigDecimal divisor, MathContext mc) {
            if (mc.precision == 0) return divideAndRemainder(divisor);

            var result = new java.math.BigDecimal[2];
            var lhs = this;
            result[0] = lhs.divideToIntegralValue(divisor, mc);
            result[1] = lhs.subtract(result[0].multiply(divisor));
            return result;
        }

        public java.math.BigDecimal sqrt(MathContext mc) {
            var _signum = signum();
            if (_signum == 1) {
                var preferredScale = scale() / 2;
                var zeroWithFinalPreferredScale = valueOf(0L, preferredScale);
                var stripped = stripTrailingZeros();
                var strippedScale = stripped.scale();
                if (stripped.isPowerOfTen() && strippedScale % 2 == 0) {
                    var result = valueOf(1L, strippedScale / 2);
                    if (result.scale() != preferredScale) result = result.add(zeroWithFinalPreferredScale, mc);

                    return result;
                }

                var scaleAdjust = 0;
                var _scale = stripped.scale() - stripped.precision() + 1;
                if (_scale % 2 == 0)
                    scaleAdjust = _scale;
                else
                    scaleAdjust = _scale - 1;

                var working = stripped.scaleByPowerOfTen(scaleAdjust);
                System.Diagnostics.Debug.Assert(ONE_TENTH.compareTo(working) <= 0 && working.compareTo(TEN) < 0);
                var guess = new java.math.BigDecimal(System.Math.Sqrt(working.doubleValue()));
                var guessPrecision = 15;
                var originalPrecision = mc.getPrecision();
                int targetPrecision;
                if (originalPrecision == 0)
                    targetPrecision = stripped.precision() / 2 + 1;
                else
                    switch (mc.getRoundingMode()) {
                        case RoundingMode.HALF_UP:
                        case RoundingMode.HALF_DOWN:
                        case RoundingMode.HALF_EVEN:
                            targetPrecision = 2 * originalPrecision;
                            if (targetPrecision < 0) targetPrecision = int.MaxValue - 2;

                            break;
                        default:
                            targetPrecision = originalPrecision;
                            break;
                    }

                var approx = guess;
                var workingPrecision = working.precision();
                do {
                    var tmpPrecision = System.Math.Max(System.Math.Max(guessPrecision, targetPrecision + 2),
                        workingPrecision);
                    var mcTmp = new MathContext(tmpPrecision, RoundingMode.HALF_EVEN);
                    approx = ONE_HALF.multiply(approx.add(working.divide(approx, mcTmp), mcTmp));
                    guessPrecision *= 2;
                } while (guessPrecision < targetPrecision + 2);

                java.math.BigDecimal _result;
                var targetRm = mc.getRoundingMode();
                if (targetRm == RoundingMode.UNNECESSARY || originalPrecision == 0) {
                    var tmpRm = targetRm == RoundingMode.UNNECESSARY ? RoundingMode.DOWN : targetRm;
                    var mcTmp = new MathContext(targetPrecision, tmpRm);
                    _result = approx.scaleByPowerOfTen(-scaleAdjust / 2).round(mcTmp);
                    if (subtract(_result.square()).compareTo(ZERO) != 0)
                        throw new System.ArithmeticException("Computed square root not exact.");
                }
                else {
                    _result = approx.scaleByPowerOfTen(-scaleAdjust / 2).round(mc);
                    switch (targetRm) {
                        case RoundingMode.DOWN:
                        case RoundingMode.FLOOR:
                            if (_result.square().compareTo(this) > 0) {
                                var ulp = _result.ulp();
                                if (approx.compareTo(ONE) == 0) ulp = ulp.multiply(ONE_TENTH);

                                _result = _result.subtract(ulp);
                            }

                            break;
                        case RoundingMode.UP:
                        case RoundingMode.CEILING:
                            if (_result.square().compareTo(this) < 0) _result = _result.add(_result.ulp());

                            break;
                        default:
                            break;
                    }
                }

                System.Diagnostics.Debug.Assert(squareRootResultAssertions(_result, mc));
                if (_result.scale() != preferredScale)
                    _result = _result.stripTrailingZeros().add(zeroWithFinalPreferredScale,
                        new MathContext(originalPrecision, RoundingMode.UNNECESSARY));

                return _result;
            }
            else {
                java.math.BigDecimal _result = null;
                switch (_signum) {
                    case -1:
                        throw new System.ArithmeticException("Attempted square root " + "of negative BigDecimal");
                    case 0:
                        _result = valueOf(0L, scale() / 2);
                        System.Diagnostics.Debug.Assert(squareRootResultAssertions(_result, mc));
                        return _result;
                    default:
                        throw new java.lang.AssertionError("Bad value from signum");
                }
            }
        }

        private java.math.BigDecimal square() {
            return multiply(this);
        }

        private bool isPowerOfTen() {
            return ((BigInteger) 1).Equals(unscaledValue());
        }

        private bool squareRootResultAssertions(java.math.BigDecimal result, MathContext mc) {
            if (result.signum() == 0) {
                return squareRootZeroResultAssertions(result, mc);
            }
            else {
                var rm = mc.getRoundingMode();
                var ulp = result.ulp();
                var neighborUp = result.add(ulp);
                if (result.isPowerOfTen()) ulp = ulp.divide(TEN);

                var neighborDown = result.subtract(ulp);
                //System.Diagnostics.Debug.Assert((((result.signum()==1)&&(this.signum()==1)))"Bad signum of this and/or its sqrt.");
                switch (rm) {
                    case RoundingMode.DOWN:
                    case RoundingMode.FLOOR:
                        //System.Diagnostics.Debug.Assert(((result.square().compareTo(this)<=0)&&(neighborUp.square().compareTo(this)>0))("Square of result out for bounds rounding "+rm));
                        return true;
                    case RoundingMode.UP:
                    case RoundingMode.CEILING:
                        //System.Diagnostics.Debug.Assert(((result.square().compareTo(this)>=0)&&(neighborDown.square().compareTo(this)<0))("Square of result out for bounds rounding "+rm));
                        return true;
                    case RoundingMode.HALF_DOWN:
                    case RoundingMode.HALF_EVEN:
                    case RoundingMode.HALF_UP:
                        var err = result.square().subtract(this).abs();
                        var errUp = neighborUp.square().subtract(this);
                        var errDown = subtract(neighborDown.square());
                        var err_comp_errUp = err.compareTo(errUp);
                        var err_comp_errDown = err.compareTo(errDown);
                        //System.Diagnostics.Debug.Assert(((errUp.signum()==1)&&(errDown.signum()==1))"Errors of neighbors squared don't have correct signs");
                        //System.Diagnostics.Debug.Assert(((err_comp_errUp<=0)||(err_comp_errDown<=0))("Computed square root has larger error than neighbors for "+rm));
                        //System.Diagnostics.Debug.Assert((((((err_comp_errUp==0)) ? (err_comp_errDown<0) : true))&&((((err_comp_errDown==0)) ? (err_comp_errUp<0) : true)))"Incorrect error relationships");
                        return true;
                    default:
                        return true;
                }
            }
        }

        private bool squareRootZeroResultAssertions(java.math.BigDecimal result, MathContext mc) {
            return compareTo(ZERO) == 0;
        }

        public java.math.BigDecimal pow(int n) {
            if (n < 0 || n > 999999999) throw new System.ArithmeticException("Invalid operation");

            var newScale = checkScale((long) _scale * n);
            return new java.math.BigDecimal(inflated().pow(n), newScale);
        }

        public java.math.BigDecimal pow(int n, MathContext mc) {
            if (mc.precision == 0) return pow(n);

            if (n < -999999999 || n > 999999999) throw new System.ArithmeticException("Invalid operation");

            if (n == 0) return ONE;

            var lhs = this;
            var workmc = mc;
            var mag = System.Math.Abs(n);
            if (mc.precision > 0) {
                var elength = longDigitLength(mag);
                if (elength > mc.precision) throw new System.ArithmeticException("Invalid operation");

                workmc = new MathContext(mc.precision + elength + 1, mc.roundingMode);
            }

            var acc = ONE;
            var seenbit = false;
            for (var i = 1;; i++) {
                mag += mag;
                if (mag < 0) {
                    seenbit = true;
                    acc = acc.multiply(lhs, workmc);
                }

                if (i == 31) break;

                if (seenbit) acc = acc.multiply(acc, workmc);
            }

            if (n < 0) acc = ONE.divide(acc, workmc);

            return doRound(acc, mc);
        }

        public java.math.BigDecimal abs() {
            return signum() < 0 ? negate() : this;
        }

        public java.math.BigDecimal abs(MathContext mc) {
            return signum() < 0 ? negate(mc) : plus(mc);
        }

        public java.math.BigDecimal negate() {
            if (intCompact == INFLATED)
                return new java.math.BigDecimal(-intVal, INFLATED, _scale, _precision);
            else
                return valueOf(-intCompact, _scale, _precision);
        }

        public java.math.BigDecimal negate(MathContext mc) {
            return negate().plus(mc);
        }

        public java.math.BigDecimal plus() {
            return this;
        }

        public java.math.BigDecimal plus(MathContext mc) {
            if (mc.precision == 0) return this;

            return doRound(this, mc);
        }

        public int signum() {
            return intCompact != INFLATED ? java.lang.Long.signum(intCompact) : intVal.Sign;
        }

        public int scale() {
            return _scale;
        }

        public int precision() {
            var result = _precision;
            if (result == 0) {
                var s = intCompact;
                if (s != INFLATED)
                    result = longDigitLength(s);
                else
                    result = bigDigitLength(intVal);

                _precision = result;
            }

            return result;
        }

        public BigInteger unscaledValue() {
            return inflated();
        }

        public const int ROUND_UP = 0;
        public const int ROUND_DOWN = 1;
        public const int ROUND_CEILING = 2;
        public const int ROUND_FLOOR = 3;
        public const int ROUND_HALF_UP = 4;
        public const int ROUND_HALF_DOWN = 5;
        public const int ROUND_HALF_EVEN = 6;
        public const int ROUND_UNNECESSARY = 7;

        private static int oldRoundingMode(RoundingMode rounding) {
            switch (rounding) {
                case RoundingMode.UP:
                    return ROUND_UP;
                case RoundingMode.DOWN:
                    return ROUND_DOWN;
                case RoundingMode.FLOOR:
                    return ROUND_FLOOR;
                case RoundingMode.HALF_UP:
                    return ROUND_HALF_UP;
                case RoundingMode.CEILING:
                    return ROUND_CEILING;
                case RoundingMode.HALF_DOWN:
                    return ROUND_HALF_DOWN;
                case RoundingMode.HALF_EVEN:
                    return ROUND_HALF_EVEN;
                case RoundingMode.UNNECESSARY:
                default:
                    return ROUND_UNNECESSARY;
            }
        }

        public java.math.BigDecimal round(MathContext mc) {
            return plus(mc);
        }

        public java.math.BigDecimal setScale(int newScale, RoundingMode roundingMode) {
            return setScale(newScale, oldRoundingMode(roundingMode));
        }

        public java.math.BigDecimal setScale(int newScale, int roundingMode) {
            if (roundingMode < ROUND_UP || roundingMode > ROUND_UNNECESSARY)
                throw new java.lang.IllegalArgumentException("Invalid rounding mode");

            var oldScale = _scale;
            if (newScale == oldScale) return this;

            if (signum() == 0) return zeroValueOf(newScale);

            if (intCompact != INFLATED) {
                var rs = intCompact;
                if (newScale > oldScale) {
                    var raise = checkScale((long) newScale - oldScale);
                    if ((rs = longMultiplyPowerTen(rs, raise)) != INFLATED) return valueOf(rs, newScale);

                    var rb = bigMultiplyPowerTen(raise);
                    return new java.math.BigDecimal(rb, INFLATED, newScale,
                        _precision > 0 ? _precision + raise : 0);
                }
                else {
                    var drop = checkScale((long) oldScale - newScale);
                    if (drop < LONG_TEN_POWERS_TABLE.Length)
                        return divideAndRound(rs, LONG_TEN_POWERS_TABLE[drop], newScale, roundingMode, newScale);
                    else
                        return divideAndRound(inflated(), bigTenToThe(drop), newScale, roundingMode, newScale);
                }
            }
            else {
                if (newScale > oldScale) {
                    var raise = checkScale((long) newScale - oldScale);
                    var rb = bigMultiplyPowerTen(intVal, raise);
                    return new java.math.BigDecimal(rb, INFLATED, newScale,
                        _precision > 0 ? _precision + raise : 0);
                }
                else {
                    var drop = checkScale((long) oldScale - newScale);
                    if (drop < LONG_TEN_POWERS_TABLE.Length)
                        return divideAndRound(intVal, LONG_TEN_POWERS_TABLE[drop], newScale, roundingMode,
                            newScale);
                    else
                        return divideAndRound(intVal, bigTenToThe(drop), newScale, roundingMode, newScale);
                }
            }
        }

        public java.math.BigDecimal setScale(int newScale) {
            return setScale(newScale, ROUND_UNNECESSARY);
        }

        public java.math.BigDecimal movePointLeft(int n) {
            if (n == 0) return this;

            var newScale = checkScale((long) _scale + n);
            var num = new java.math.BigDecimal(intVal, intCompact, newScale, 0);
            return num._scale < 0 ? num.setScale(0, ROUND_UNNECESSARY) : num;
        }

        public java.math.BigDecimal movePointRight(int n) {
            if (n == 0) return this;

            var newScale = checkScale((long) _scale - n);
            var num = new java.math.BigDecimal(intVal, intCompact, newScale, 0);
            return num._scale < 0 ? num.setScale(0, ROUND_UNNECESSARY) : num;
        }

        public java.math.BigDecimal scaleByPowerOfTen(int n) {
            return new java.math.BigDecimal(intVal, intCompact, checkScale((long) _scale - n), _precision);
        }

        public java.math.BigDecimal stripTrailingZeros() {
            if (intCompact == 0 || intVal != null && intVal.Sign == 0) {
                return ZERO;
            }
            else {
                if (intCompact != INFLATED)
                    return createAndStripZerosToMatchScale(intCompact, _scale, long.MinValue);
                else
                    return createAndStripZerosToMatchScale(intVal, _scale, long.MinValue);
            }
        }

        public int compareTo(java.math.BigDecimal val) {
            if (_scale == val._scale) {
                var xs = intCompact;
                var ys = val.intCompact;
                if (xs != INFLATED && ys != INFLATED) return xs != ys ? xs > ys ? 1 : -1 : 0;
            }

            var xsign = signum();
            var ysign = val.signum();
            if (xsign != ysign) return xsign > ysign ? 1 : -1;

            if (xsign == 0) return 0;

            var cmp = compareMagnitude(val);
            return xsign > 0 ? cmp : -cmp;
        }

        private int compareMagnitude(java.math.BigDecimal val) {
            var ys = val.intCompact;
            var xs = intCompact;
            if (xs == 0) return ys == 0 ? 0 : -1;

            if (ys == 0) return 1;

            var sdiff = (long) _scale - val._scale;
            if (sdiff != 0) {
                var xae = (long) precision() - _scale;
                var yae = (long) val.precision() - val._scale;
                if (xae < yae) return -1;

                if (xae > yae) return 1;

                if (sdiff < 0) {
                    if (sdiff > int.MinValue &&
                        (xs == INFLATED || (xs = longMultiplyPowerTen(xs, (int) -sdiff)) == INFLATED) &&
                        ys == INFLATED) {
                        var rb = bigMultiplyPowerTen((int) -sdiff);
                        return compareBigIntegerMagnitude(rb, val.intVal);
                    }
                }
                else {
                    if (sdiff <= int.MaxValue &&
                        (ys == INFLATED || (ys = longMultiplyPowerTen(ys, (int) sdiff)) == INFLATED) &&
                        xs == INFLATED) {
                        var rb = val.bigMultiplyPowerTen((int) sdiff);
                        return compareBigIntegerMagnitude(intVal, rb);
                    }
                }
            }

            if (xs != INFLATED) {
                return ys != INFLATED ? longCompareMagnitude(xs, ys) : -1;
            }
            else {
                if (ys != INFLATED)
                    return 1;
                else
                    return compareBigIntegerMagnitude(intVal, val.intVal);
            }
        }

        public bool equals(object x) {
            if (!(x is java.math.BigDecimal)) return false;

            var xDec = (java.math.BigDecimal) x;
            if (x == this) return true;

            if (_scale != xDec._scale) return false;

            var s = intCompact;
            var xs = xDec.intCompact;
            if (s != INFLATED) {
                if (xs == INFLATED) xs = compactValFor(xDec.intVal);

                return xs == s;
            }
            else {
                if (xs != INFLATED) return xs == compactValFor(intVal);
            }

            return inflated().Equals(xDec.inflated());
        }

        public java.math.BigDecimal min(java.math.BigDecimal val) {
            return compareTo(val) <= 0 ? this : val;
        }

        public java.math.BigDecimal max(java.math.BigDecimal val) {
            return compareTo(val) >= 0 ? this : val;
        }

        public int hashCode() {
            if (intCompact != INFLATED) {
                var val2 = intCompact < 0 ? -intCompact : intCompact;
                var temp = (int) ((int) com.saxonica.glue.Utils.unsignedRightShift(val2, 32) * 31 +
                                  (val2 & LONG_MASK));
                return 31 * (intCompact < 0 ? -temp : temp) + _scale;
            }
            else {
                return 31 * intVal.GetHashCode() + _scale;
            }
        }

        public string toString() {
            var sc = stringCache;
            if (sc == null) stringCache = sc = layoutChars(true);

            return sc;
        }

        public string toEngineeringString() {
            return layoutChars(false);
        }

        public string toPlainString() {
            if (_scale == 0) {
                if (intCompact != INFLATED)
                    return java.lang.Long.ToString(intCompact);
                else
                    return intVal.ToString();
            }

            if (_scale < 0) {
                if (signum() == 0) return "0";

                var trailingZeros = checkScaleNonZero(-(long) _scale);
                java.lang.StringBuilder buf;
                if (intCompact != INFLATED) {
                    buf = new java.lang.StringBuilder(20 + trailingZeros);
                    buf.append(intCompact);
                }
                else {
                    var strVal = intVal.ToString();
                    buf = new java.lang.StringBuilder(strVal.Length + trailingZeros);
                    buf.append(strVal);
                }

                for (var i = 0; i < trailingZeros; i++) buf.append('0');

                return buf.ToString();
            }

            string str;
            if (intCompact != INFLATED)
                str = java.lang.Long.ToString(System.Math.Abs(intCompact));
            else
                str = BigInteger.Abs(intVal).ToString();

            return getValueString(signum(), str, _scale);
        }

        private string getValueString(int signum, string intString, int _scale) {
            java.lang.StringBuilder buf;
            var insertionPoint = intString.Length - _scale;
            if (insertionPoint == 0) {
                return (signum < 0 ? "-0." : "0.") + intString;
            }
            else {
                if (insertionPoint > 0) {
                    buf = new java.lang.StringBuilder(intString);
                    buf.insert(insertionPoint, '.');
                    if (signum < 0) buf.insert(0, '-');
                }
                else {
                    buf = new java.lang.StringBuilder(3 - insertionPoint + intString.Length);
                    buf.append(signum < 0 ? "-0." : "0.");
                    for (var i = 0; i < -insertionPoint; i++) buf.append('0');

                    buf.append(intString);
                }
            }

            return buf.ToString();
        }

        public BigInteger toBigInteger() {
            return setScale(0, ROUND_DOWN).inflated();
        }

        public BigInteger toBigIntegerExact() {
            return setScale(0, ROUND_UNNECESSARY).inflated();
        }

        public override long longValue() {
            if (intCompact != INFLATED && _scale == 0) {
                return intCompact;
            }
            else {
                if (signum() == 0 || fractionOnly() || _scale <= -64)
                    return 0;
                else
                    return (long) toBigInteger();
            }
        }

        private bool fractionOnly() {
            System.Diagnostics.Debug.Assert(signum() != 0);
            return precision() - _scale <= 0;
        }

        public long longValueExact() {
            if (intCompact != INFLATED && _scale == 0) return intCompact;

            if (signum() == 0) return 0;

            if (fractionOnly()) throw new System.ArithmeticException("Rounding necessary");

            if (precision() - _scale > 19) throw new System.ArithmeticException("Overflow");

            var num = setScale(0, ROUND_UNNECESSARY);
            if (num.precision() >= 19) java.math.BigDecimal.LongOverflow.check(num);

            return (long) num.inflated();
        }

        private class LongOverflow {
            private static readonly BigInteger LONGMIN = (BigInteger) long.MinValue;
            private static readonly BigInteger LONGMAX = (BigInteger) long.MaxValue;

            public static void check(java.math.BigDecimal num) {
                var intVal = num.inflated();
                if (intVal.CompareTo(LONGMIN) < 0 || intVal.CompareTo(LONGMAX) > 0)
                    throw new System.ArithmeticException("Overflow");
            }
        }

        public override int intValue() {
            return intCompact != INFLATED && _scale == 0 ? (int) intCompact : (int) longValue();
        }

        public int intValueExact() {
            long num;
            num = longValueExact();
            if ((int) num != num) throw new System.ArithmeticException("Overflow");

            return (int) num;
        }

        public short shortValueExact() {
            long num;
            num = longValueExact();
            if ((short) num != num) throw new System.ArithmeticException("Overflow");

            return (short) num;
        }

        public byte byteValueExact() {
            long num;
            num = longValueExact();
            if ((byte) num != num) throw new System.ArithmeticException("Overflow");

            return (byte) num;
        }

        public override float floatValue() {
            if (intCompact != INFLATED) {
                if (_scale == 0) {
                    return (float) intCompact;
                }
                else {
                    if (System.Math.Abs(intCompact) < 1L << 22) {
                        if (_scale > 0 && _scale < FLOAT_10_POW.Length) {
                            return (float) intCompact / FLOAT_10_POW[_scale];
                        }
                        else {
                            if (_scale < 0 && _scale > -FLOAT_10_POW.Length)
                                return (float) intCompact * FLOAT_10_POW[-_scale];
                        }
                    }
                }
            }

            return java.lang.Float.parseFloat(ToString());
        }

        public override double doubleValue() {
            if (intCompact != INFLATED) {
                if (_scale == 0) {
                    return (double) intCompact;
                }
                else {
                    if (System.Math.Abs(intCompact) < 1L << 52) {
                        if (_scale > 0 && _scale < DOUBLE_10_POW.Length) {
                            return (double) intCompact / DOUBLE_10_POW[_scale];
                        }
                        else {
                            if (_scale < 0 && _scale > -DOUBLE_10_POW.Length)
                                return (double) intCompact * DOUBLE_10_POW[-_scale];
                        }
                    }
                }
            }

            return java.lang.Double.parseDouble(ToString());
        }

        private static readonly double[] DOUBLE_10_POW = {
            1.0e0, 1.0e1, 1.0e2, 1.0e3, 1.0e4, 1.0e5, 1.0e6, 1.0e7, 1.0e8, 1.0e9, 1.0e10, 1.0e11, 1.0e12, 1.0e13,
            1.0e14, 1.0e15, 1.0e16, 1.0e17, 1.0e18, 1.0e19, 1.0e20, 1.0e21, 1.0e22
        };

        private static readonly float[] FLOAT_10_POW =
            {1.0e0f, 1.0e1f, 1.0e2f, 1.0e3f, 1.0e4f, 1.0e5f, 1.0e6f, 1.0e7f, 1.0e8f, 1.0e9f, 1.0e10f};

        public java.math.BigDecimal ulp() {
            return valueOf(1, scale(), 1);
        }

        private class StringBuilderHelper {
            private readonly java.lang.StringBuilder sb;
            private readonly char[] cmpCharArray;

            internal StringBuilderHelper() {
                sb = new java.lang.StringBuilder(32);
                cmpCharArray = new char[19];
            }

            internal java.lang.StringBuilder getStringBuilder() {
                sb.setLength(0);
                return sb;
            }

            internal char[] getCompactCharArray() {
                return cmpCharArray;
            }

            internal int putIntCompact(long intCompact) {
                System.Diagnostics.Debug.Assert(intCompact >= 0);
                long q;
                int r;
                var charPos = cmpCharArray.Length;
                while (intCompact > int.MaxValue) {
                    q = intCompact / 100;
                    r = (int) (intCompact - q * 100);
                    intCompact = q;
                    cmpCharArray[--charPos] = DIGIT_ONES[r];
                    cmpCharArray[--charPos] = DIGIT_TENS[r];
                }

                int q2;
                var i2 = (int) intCompact;
                while (i2 >= 100) {
                    q2 = i2 / 100;
                    r = i2 - q2 * 100;
                    i2 = q2;
                    cmpCharArray[--charPos] = DIGIT_ONES[r];
                    cmpCharArray[--charPos] = DIGIT_TENS[r];
                }

                cmpCharArray[--charPos] = DIGIT_ONES[i2];
                if (i2 >= 10) cmpCharArray[--charPos] = DIGIT_TENS[i2];

                return charPos;
            }

            private static readonly char[] DIGIT_TENS = {
                '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '2',
                '2', '2', '2', '2', '2', '2', '2', '2', '2', '3', '3', '3', '3', '3', '3', '3', '3', '3', '3', '4', '4',
                '4', '4', '4', '4', '4', '4', '4', '4', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '6', '6', '6',
                '6', '6', '6', '6', '6', '6', '6', '7', '7', '7', '7', '7', '7', '7', '7', '7', '7', '8', '8', '8', '8',
                '8', '8', '8', '8', '8', '8', '9', '9', '9', '9', '9', '9', '9', '9', '9', '9'
            };

            private static readonly char[] DIGIT_ONES = {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1',
                '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2',
                '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3',
                '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
            };
        }

        private string layoutChars(bool sci) {
            if (_scale == 0) return intCompact != INFLATED ? java.lang.Long.ToString(intCompact) : intVal.ToString();

            if (_scale == 2 && intCompact >= 0 && intCompact < int.MaxValue) {
                var lowInt = (int) intCompact % 100;
                var highInt = (int) intCompact / 100;
                return java.lang.Integer.ToString(highInt) + '.' +
                       java.math.BigDecimal.StringBuilderHelper.DIGIT_TENS[lowInt] +
                       java.math.BigDecimal.StringBuilderHelper.DIGIT_ONES[lowInt];
            }

            var sbHelper = new java.math.BigDecimal.StringBuilderHelper();
            char[] coeff;
            int offset;
            if (intCompact != INFLATED) {
                offset = sbHelper.putIntCompact(System.Math.Abs(intCompact));
                coeff = sbHelper.getCompactCharArray();
            }
            else {
                offset = 0;
                coeff = BigInteger.Abs(intVal).ToString().toCharArray();
            }

            var buf = sbHelper.getStringBuilder();
            if (signum() < 0) buf.append('-');

            var coeffLen = coeff.Length - offset;
            var adjusted = -(long) _scale + (coeffLen - 1);
            if (_scale >= 0 && adjusted >= -6) {
                var pad = _scale - coeffLen;
                if (pad >= 0) {
                    buf.append('0');
                    buf.append('.');
                    for (; pad > 0; pad--) buf.append('0');

                    buf.append(coeff, offset, coeffLen);
                }
                else {
                    buf.append(coeff, offset, -pad);
                    buf.append('.');
                    buf.append(coeff, -pad + offset, _scale);
                }
            }
            else {
                if (sci) {
                    buf.append(coeff[offset]);
                    if (coeffLen > 1) {
                        buf.append('.');
                        buf.append(coeff, offset + 1, coeffLen - 1);
                    }
                }
                else {
                    var sig = (int) (adjusted % 3);
                    if (sig < 0) sig += 3;

                    adjusted -= sig;
                    sig++;
                    if (signum() == 0) {
                        switch (sig) {
                            case 1:
                                buf.append('0');
                                break;
                            case 2:
                                buf.append("0.00");
                                adjusted += 3;
                                break;
                            case 3:
                                buf.append("0.0");
                                adjusted += 3;
                                break;
                            default:
                                throw new java.lang.AssertionError("Unexpected sig value " + sig);
                        }
                    }
                    else {
                        if (sig >= coeffLen) {
                            buf.append(coeff, offset, coeffLen);
                            for (var i = sig - coeffLen; i > 0; i--) buf.append('0');
                        }
                        else {
                            buf.append(coeff, offset, sig);
                            buf.append('.');
                            buf.append(coeff, offset + sig, coeffLen - sig);
                        }
                    }
                }

                if (adjusted != 0) {
                    buf.append('E');
                    if (adjusted > 0) buf.append('+');

                    buf.append(adjusted);
                }
            }

            return buf.ToString();
        }

        private static BigInteger bigTenToThe(int n) {
            if (n < 0) return (BigInteger) 0;

            if (n < BIG_TEN_POWERS_TABLE_MAX) {
                var pows = BIG_TEN_POWERS_TABLE;
                if (n < pows.Length)
                    return pows[n];
                else
                    return expandBigIntegerTenPowers(n);
            }

            return BigInteger.Pow(10, n);
        }

        private static BigInteger expandBigIntegerTenPowers(int n) {
            lock (typeof(java.math.BigDecimal)) {
                var pows = BIG_TEN_POWERS_TABLE;
                var curLen = pows.Length;
                if (curLen <= n) {
                    var newLen = curLen << 1;
                    while (newLen <= n) newLen <<= 1;

                    pows = java.util.Arrays.copyOf(pows, newLen);
                    for (var i = curLen; i < newLen; i++) pows[i] = pows[i - 1] * (BigInteger) 10;

                    BIG_TEN_POWERS_TABLE = pows;
                }

                return pows[n];
            }
        }

        private static readonly long[] LONG_TEN_POWERS_TABLE = {
            1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000, 10000000000L, 100000000000L,
            1000000000000L, 10000000000000L, 100000000000000L, 1000000000000000L, 10000000000000000L,
            100000000000000000L, 1000000000000000000L
        };

        private static volatile BigInteger[] BIG_TEN_POWERS_TABLE = {
            (BigInteger) 1, (BigInteger) 10, (BigInteger) 100, (BigInteger) 1000, (BigInteger) 10000,
            (BigInteger) 100000, (BigInteger) 1000000, (BigInteger) 10000000, (BigInteger) 100000000,
            (BigInteger) 1000000000, (BigInteger) 10000000000L, (BigInteger) 100000000000L,
            (BigInteger) 1000000000000L, (BigInteger) 10000000000000L, (BigInteger) 100000000000000L,
            (BigInteger) 1000000000000000L, (BigInteger) 10000000000000000L, (BigInteger) 100000000000000000L,
            (BigInteger) 1000000000000000000L
        };

        private static readonly int BIG_TEN_POWERS_TABLE_INITLEN = BIG_TEN_POWERS_TABLE.Length;
        private static readonly int BIG_TEN_POWERS_TABLE_MAX = 16 * BIG_TEN_POWERS_TABLE_INITLEN;

        private static readonly long[] THRESHOLDS_TABLE = {
            long.MaxValue, long.MaxValue / 10L, long.MaxValue / 100L,
            long.MaxValue / 1000L, long.MaxValue / 10000L, long.MaxValue / 100000L,
            long.MaxValue / 1000000L, long.MaxValue / 10000000L,
            long.MaxValue / 100000000L, long.MaxValue / 1000000000L,
            long.MaxValue / 10000000000L, long.MaxValue / 100000000000L,
            long.MaxValue / 1000000000000L, long.MaxValue / 10000000000000L,
            long.MaxValue / 100000000000000L, long.MaxValue / 1000000000000000L,
            long.MaxValue / 10000000000000000L, long.MaxValue / 100000000000000000L,
            long.MaxValue / 1000000000000000000L
        };

        private static long longMultiplyPowerTen(long val, int n) {
            if (val == 0 || n <= 0) return val;

            var tab = LONG_TEN_POWERS_TABLE;
            var bounds = THRESHOLDS_TABLE;
            if (n < tab.Length && n < bounds.Length) {
                var tenpower = tab[n];
                if (val == 1) return tenpower;

                if (System.Math.Abs(val) <= bounds[n]) return val * tenpower;
            }

            return INFLATED;
        }

        private BigInteger bigMultiplyPowerTen(int n) {
            if (n <= 0) return inflated();

            if (intCompact != INFLATED)
                return bigTenToThe(n) * intCompact;
            else
                return intVal * bigTenToThe(n);
        }

        private BigInteger inflated() {
            if (intVal == null) return (BigInteger) intCompact;

            return intVal;
        }

        private static void matchScale(java.math.BigDecimal[] val) {
            if (val[0]._scale < val[1]._scale) {
                val[0] = val[0].setScale(val[1]._scale, ROUND_UNNECESSARY);
            }
            else {
                if (val[1]._scale < val[0]._scale) val[1] = val[1].setScale(val[0]._scale, ROUND_UNNECESSARY);
            }
        }

        // private class UnsafeHolder {
        //     private static readonly Unsafe @unsafe = jdk.@internal.misc.Unsafe.getUnsafe();
        //     private static readonly long intCompactOffset = @unsafe.objectFieldOffset(typeof(java.math.BigDecimal), "intCompact");
        //     private static readonly long intValOffset = @unsafe.objectFieldOffset(typeof(java.math.BigDecimal), "intVal");
        //     static void setIntCompact(java.math.BigDecimal bd, long val) {
        //         @unsafe.putLong(bd, intCompactOffset, val);
        //     }
        //     static void setIntValVolatile(java.math.BigDecimal bd, BigInteger val) {
        //         @unsafe.putReferenceVolatile(bd, intValOffset, val);
        //     }
        // }
        // private void readObject(java.io.ObjectInputStream s) {
        //     s.defaultReadObject();
        //     if ((intVal==null)) {
        //         string message = "BigDecimal: null intVal in stream";
        //         throw new java.io.StreamCorruptedException(message);
        //     }
        //     java.math.BigDecimal.UnsafeHolder.setIntCompact(this, compactValFor(intVal));
        // }
        // private void writeObject(java.io.ObjectOutputStream s) {
        //     if ((this.intVal==null)) {
        //         java.math.BigDecimal.UnsafeHolder.setIntValVolatile(this, (BigInteger)(this.intCompact));
        //     }
        //     s.defaultWriteObject();
        // }
        private static int longDigitLength(long x) {
            //System.Diagnostics.Debug.Assert((x!=java.math.BigDecimal.INFLATED));
            if (x < 0) x = -x;

            if (x < 10) return 1;

            var r = com.saxonica.glue.Utils.unsignedRightShift(
                (64 - java.lang.Long.numberOfLeadingZeros(x) + 1) * 1233, 12);
            var tab = LONG_TEN_POWERS_TABLE;
            return r >= tab.Length || x < tab[r] ? r : r + 1;
        }

        private static int bigDigitLength(BigInteger b) {
            if (b.Sign == 0) return 1;

            var r = (int) com.saxonica.glue.Utils.unsignedRightShift(((long) b.bitLength() + 1) * 646456993,
                31);
            return compareBigIntegerMagnitude(b, bigTenToThe(r)) < 0 ? r : r + 1;
        }

        private int checkScale(long val) {
            var asInt = (int) val;
            if (asInt != val) {
                asInt = val > int.MaxValue ? int.MaxValue : int.MinValue;
                BigInteger b;
                if (intCompact != 0 && ((b = intVal) == null || b.Sign != 0))
                    throw new System.ArithmeticException(asInt > 0 ? "Underflow" : "Overflow");
            }

            return asInt;
        }

        private static long compactValFor(BigInteger b) {
            int[] m = b.mag;
            var len = m.Length;
            if (len == 0) return 0;

            var d = m[0];
            if (len > 2 || len == 2 && d < 0) return INFLATED;

            var u =
                len == 2 ? ((long) m[1] & LONG_MASK) + ((long) d << 32) : (long) d & LONG_MASK;
            return b.Sign < 0 ? -u : u;
        }

        private static int longCompareMagnitude(long x, long y) {
            if (x < 0) x = -x;

            if (y < 0) y = -y;

            return x < y ? -1 : x == y ? 0 : 1;
        }

        private static int saturateLong(long s) {
            var i = (int) s;
            return s == i ? i : s < 0 ? int.MinValue : int.MaxValue;
        }

        private static void print(string name, java.math.BigDecimal bd) {
            System.Console.Error.format("%s:\tintCompact %d\tintVal %d\t_scale %d\t_precision %d%n", name,
                bd.intCompact,
                bd.intVal, bd._scale, bd._precision);
        }

        private java.math.BigDecimal audit() {
            if (intCompact == INFLATED) {
                if (intVal == null) {
                    print("audit", this);
                    throw new java.lang.AssertionError("null intVal");
                }

                if (_precision > 0 && _precision != bigDigitLength(intVal)) {
                    print("audit", this);
                    throw new java.lang.AssertionError("_precision mismatch");
                }
            }
            else {
                if (intVal != null) {
                    var val = (long) intVal;
                    if (val != intCompact) {
                        print("audit", this);
                        throw new java.lang.AssertionError(
                            "Inconsistent state, intCompact=" + intCompact + "\t intVal=" + val);
                    }
                }

                if (_precision > 0 && _precision != longDigitLength(intCompact)) {
                    print("audit", this);
                    throw new java.lang.AssertionError("_precision mismatch");
                }
            }

            return this;
        }

        private static int checkScaleNonZero(long val) {
            var asInt = (int) val;
            if (asInt != val) throw new System.ArithmeticException(asInt > 0 ? "Underflow" : "Overflow");

            return asInt;
        }

        private static int checkScale(long intCompact, long val) {
            var asInt = (int) val;
            if (asInt != val) {
                asInt = val > int.MaxValue ? int.MaxValue : int.MinValue;
                if (intCompact != 0) throw new System.ArithmeticException(asInt > 0 ? "Underflow" : "Overflow");
            }

            return asInt;
        }

        private static int checkScale(BigInteger intVal, long val) {
            var asInt = (int) val;
            if (asInt != val) {
                asInt = val > int.MaxValue ? int.MaxValue : int.MinValue;
                if (intVal.Sign != 0) throw new System.ArithmeticException(asInt > 0 ? "Underflow" : "Overflow");
            }

            return asInt;
        }

        private static java.math.BigDecimal doRound(java.math.BigDecimal val, MathContext mc) {
            var mcp = mc.precision;
            var wasDivided = false;
            if (mcp > 0) {
                var intVal = val.intVal;
                var compactVal = val.intCompact;
                var _scale = val._scale;
                var prec = val.precision();
                var mode = oldRoundingMode(mc.roundingMode);
                int drop;
                if (compactVal == INFLATED) {
                    drop = prec - mcp;
                    while (drop > 0) {
                        _scale = checkScaleNonZero((long) _scale - drop);
                        intVal = divideAndRoundByTenPow(intVal, drop, mode);
                        wasDivided = true;
                        compactVal = compactValFor(intVal);
                        if (compactVal != INFLATED) {
                            prec = longDigitLength(compactVal);
                            break;
                        }

                        prec = bigDigitLength(intVal);
                        drop = prec - mcp;
                    }
                }

                if (compactVal != INFLATED) {
                    drop = prec - mcp;
                    while (drop > 0) {
                        _scale = checkScaleNonZero((long) _scale - drop);
                        compactVal = divideAndRound(compactVal, LONG_TEN_POWERS_TABLE[drop],
                            oldRoundingMode(mc.roundingMode));
                        wasDivided = true;
                        prec = longDigitLength(compactVal);
                        drop = prec - mcp;
                        intVal = default(BigInteger);
                    }
                }

                return wasDivided ? new java.math.BigDecimal(intVal, compactVal, _scale, prec) : val;
            }

            return val;
        }

        private static java.math.BigDecimal doRound(long compactVal, int _scale, MathContext mc) {
            var mcp = mc.precision;
            if (mcp > 0 && mcp < 19) {
                var prec = longDigitLength(compactVal);
                var drop = prec - mcp;
                while (drop > 0) {
                    _scale = checkScaleNonZero((long) _scale - drop);
                    compactVal = divideAndRound(compactVal, LONG_TEN_POWERS_TABLE[drop],
                        oldRoundingMode(mc.roundingMode));
                    prec = longDigitLength(compactVal);
                    drop = prec - mcp;
                }

                return valueOf(compactVal, _scale, prec);
            }

            return valueOf(compactVal, _scale);
        }

        private static java.math.BigDecimal doRound(BigInteger intVal, int _scale, MathContext mc) {
            var mcp = mc.precision;
            var prec = 0;
            if (mcp > 0) {
                var compactVal = compactValFor(intVal);
                var mode = oldRoundingMode(mc.roundingMode);
                int drop;
                if (compactVal == INFLATED) {
                    prec = bigDigitLength(intVal);
                    drop = prec - mcp;
                    while (drop > 0) {
                        _scale = checkScaleNonZero((long) _scale - drop);
                        intVal = divideAndRoundByTenPow(intVal, drop, mode);
                        compactVal = compactValFor(intVal);
                        if (compactVal != INFLATED) break;

                        prec = bigDigitLength(intVal);
                        drop = prec - mcp;
                    }
                }

                if (compactVal != INFLATED) {
                    prec = longDigitLength(compactVal);
                    drop = prec - mcp;
                    while (drop > 0) {
                        _scale = checkScaleNonZero((long) _scale - drop);
                        compactVal = divideAndRound(compactVal, LONG_TEN_POWERS_TABLE[drop],
                            oldRoundingMode(mc.roundingMode));
                        prec = longDigitLength(compactVal);
                        drop = prec - mcp;
                    }

                    return valueOf(compactVal, _scale, prec);
                }
            }

            return new java.math.BigDecimal(intVal, INFLATED, _scale, prec);
        }

        private static BigInteger divideAndRoundByTenPow(BigInteger intVal, int tenPow, int roundingMode) {
            if (tenPow < LONG_TEN_POWERS_TABLE.Length)
                intVal = divideAndRound(intVal, LONG_TEN_POWERS_TABLE[tenPow], roundingMode);
            else
                intVal = divideAndRound(intVal, bigTenToThe(tenPow), roundingMode);

            return intVal;
        }

        private static java.math.BigDecimal divideAndRound(long ldividend, long ldivisor, int _scale, int roundingMode,
            int preferredScale) {
            int qsign;
            var q = ldividend / ldivisor;
            if (roundingMode == ROUND_DOWN && _scale == preferredScale) return valueOf(q, _scale);

            var r = ldividend % ldivisor;
            qsign = ldividend < 0 == ldivisor < 0 ? 1 : -1;
            if (r != 0) {
                var increment = needIncrement(ldivisor, roundingMode, qsign, q, r);
                return valueOf(increment ? q + qsign : q, _scale);
            }
            else {
                if (preferredScale != _scale)
                    return createAndStripZerosToMatchScale(q, _scale, preferredScale);
                else
                    return valueOf(q, _scale);
            }
        }

        private static long divideAndRound(long ldividend, long ldivisor, int roundingMode) {
            int qsign;
            var q = ldividend / ldivisor;
            if (roundingMode == ROUND_DOWN) return q;

            var r = ldividend % ldivisor;
            qsign = ldividend < 0 == ldivisor < 0 ? 1 : -1;
            if (r != 0) {
                var increment = needIncrement(ldivisor, roundingMode, qsign, q, r);
                return increment ? q + qsign : q;
            }
            else {
                return q;
            }
        }

        private static bool commonNeedIncrement(int roundingMode, int qsign, int cmpFracHalf, bool oddQuot) {
            switch (roundingMode) {
                case ROUND_UNNECESSARY:
                    throw new System.ArithmeticException("Rounding necessary");
                case ROUND_UP:
                    return true;
                case ROUND_DOWN:
                    return false;
                case ROUND_CEILING:
                    return qsign > 0;
                case ROUND_FLOOR:
                    return qsign < 0;
                default:
                    // System.Diagnostics.Debug.Assert(
                    //     (roundingMode >= ROUND_HALF_UP && roundingMode <= ROUND_HALF_EVEN)(
                    //         "Unexpected rounding mode" + RoundingMode.valueOf(roundingMode)));
                    if (cmpFracHalf < 0) {
                        return false;
                    }
                    else {
                        if (cmpFracHalf > 0) {
                            return true;
                        }
                        else {
                            System.Diagnostics.Debug.Assert(cmpFracHalf == 0);
                            switch (roundingMode) {
                                case ROUND_HALF_DOWN:
                                    return false;
                                case ROUND_HALF_UP:
                                    return true;
                                case ROUND_HALF_EVEN:
                                    return oddQuot;
                                default:
                                    throw new java.lang.AssertionError("Unexpected rounding mode" + roundingMode);
                            }
                        }
                    }
            }
        }

        private static bool needIncrement(long ldivisor, int roundingMode, int qsign, long q, long r) {
            System.Diagnostics.Debug.Assert(r != 0L);
            int cmpFracHalf;
            if (r <= HALF_LONG_MIN_VALUE || r > HALF_LONG_MAX_VALUE)
                cmpFracHalf = 1;
            else
                cmpFracHalf = longCompareMagnitude(2 * r, ldivisor);

            return commonNeedIncrement(roundingMode, qsign, cmpFracHalf, (q & 1L) != 0L);
        }

        private static BigInteger divideAndRound(BigInteger bdividend, long ldivisor, int roundingMode) {
            MutableBigInteger mdividend = new MutableBigInteger(bdividend.mag);
            MutableBigInteger mq = new MutableBigInteger();
            long r = mdividend.divide(ldivisor, mq);
            var isRemainderZero = r == 0;
            int qsign = ldivisor < 0 ? -bdividend.Sign : bdividend.Sign;
            if (!isRemainderZero)
                if (needIncrement(ldivisor, roundingMode, qsign, mq, r))
                    mq.add(MutableBigInteger.ONE);

            return mq.toBigInteger(qsign);
        }

        private static java.math.BigDecimal divideAndRound(BigInteger bdividend, long ldivisor, int _scale,
            int roundingMode, int preferredScale) {
            MutableBigInteger mdividend = new MutableBigInteger(bdividend.mag);
            MutableBigInteger mq = new MutableBigInteger();
            long r = mdividend.divide(ldivisor, mq);
            var isRemainderZero = r == 0;
            int qsign = ldivisor < 0 ? -bdividend.Sign : bdividend.Sign;
            if (!isRemainderZero) {
                if (needIncrement(ldivisor, roundingMode, qsign, mq, r)) mq.add(MutableBigInteger.ONE);

                return mq.toBigDecimal(qsign, _scale);
            }
            else {
                if (preferredScale != _scale) {
                    long compactVal = mq.toCompactValue(qsign);
                    if (compactVal != INFLATED)
                        return createAndStripZerosToMatchScale(compactVal, _scale, preferredScale);

                    BigInteger intVal = mq.toBigInteger(qsign);
                    return createAndStripZerosToMatchScale(intVal, _scale, preferredScale);
                }
                else {
                    return mq.toBigDecimal(qsign, _scale);
                }
            }
        }

        private static bool needIncrement(long ldivisor, int roundingMode, int qsign, MutableBigInteger mq, long r) {
            System.Diagnostics.Debug.Assert(r != 0L);
            int cmpFracHalf;
            if (r <= HALF_LONG_MIN_VALUE || r > HALF_LONG_MAX_VALUE)
                cmpFracHalf = 1;
            else
                cmpFracHalf = longCompareMagnitude(2 * r, ldivisor);

            return commonNeedIncrement(roundingMode, qsign, cmpFracHalf, mq.isOdd());
        }

        private static BigInteger divideAndRound(BigInteger bdividend, BigInteger bdivisor, int roundingMode) {
            bool isRemainderZero;
            int qsign;
            MutableBigInteger mdividend = new MutableBigInteger(bdividend.mag);
            MutableBigInteger mq = new MutableBigInteger();
            MutableBigInteger mdivisor = new MutableBigInteger(bdivisor.mag);
            MutableBigInteger mr = mdividend.divide(mdivisor, mq);
            isRemainderZero = mr.isZero();
            qsign = bdividend.Sign != bdivisor.Sign ? -1 : 1;
            if (!isRemainderZero)
                if (needIncrement(mdivisor, roundingMode, qsign, mq, mr))
                    mq.add(MutableBigInteger.ONE);

            return mq.toBigInteger(qsign);
        }

        private static java.math.BigDecimal divideAndRound(BigInteger bdividend, BigInteger bdivisor, int _scale,
            int roundingMode, int preferredScale) {
            bool isRemainderZero;
            int qsign;
            MutableBigInteger mdividend = new MutableBigInteger(bdividend.mag);
            MutableBigInteger mq = new MutableBigInteger();
            MutableBigInteger mdivisor = new MutableBigInteger(bdivisor.mag);
            MutableBigInteger mr = mdividend.divide(mdivisor, mq);
            isRemainderZero = mr.isZero();
            qsign = bdividend.Sign != bdivisor.Sign ? -1 : 1;
            if (!isRemainderZero) {
                if (needIncrement(mdivisor, roundingMode, qsign, mq, mr)) mq.add(MutableBigInteger.ONE);

                return mq.toBigDecimal(qsign, _scale);
            }
            else {
                if (preferredScale != _scale) {
                    long compactVal = mq.toCompactValue(qsign);
                    if (compactVal != INFLATED)
                        return createAndStripZerosToMatchScale(compactVal, _scale, preferredScale);

                    BigInteger intVal = mq.toBigInteger(qsign);
                    return createAndStripZerosToMatchScale(intVal, _scale, preferredScale);
                }
                else {
                    return mq.toBigDecimal(qsign, _scale);
                }
            }
        }

        private static bool needIncrement(MutableBigInteger mdivisor, int roundingMode, int qsign, MutableBigInteger mq,
            MutableBigInteger mr) {
            System.Diagnostics.Debug.Assert(!mr.isZero());
            int cmpFracHalf = mr.compareHalf(mdivisor);
            return commonNeedIncrement(roundingMode, qsign, cmpFracHalf, mq.isOdd());
        }

        private static java.math.BigDecimal createAndStripZerosToMatchScale(BigInteger intVal, int _scale,
            long preferredScale) {
            BigInteger[] qr;
            while (intVal.compareMagnitude((BigInteger) 10) >= 0 && _scale > preferredScale)) {
                if (intVal.testBit(0)) break;

                qr = intVal.divideAndRemainder((BigInteger) 10);
                if (qr[1].Sign != 0) break;

                intVal = qr[0];
                _scale = checkScale(intVal, (long) _scale - 1);
            }
            return valueOf(intVal, _scale, 0);
        }

        private static java.math.BigDecimal createAndStripZerosToMatchScale(long compactVal, int _scale,
            long preferredScale) {
            while (System.Math.Abs(compactVal) >= 10L && _scale > preferredScale) {
                if ((compactVal & 1L) != 0L) break;

                var r = compactVal % 10L;
                if (r != 0L) break;

                compactVal /= 10;
                _scale = checkScale(compactVal, (long) _scale - 1);
            }

            return valueOf(compactVal, _scale);
        }

        private static java.math.BigDecimal stripZerosToMatchScale(BigInteger intVal, long intCompact, int _scale,
            int preferredScale) {
            if (intCompact != INFLATED)
                return createAndStripZerosToMatchScale(intCompact, _scale, preferredScale);
            else
                return createAndStripZerosToMatchScale(intVal == null ? INFLATED_BIGINT : intVal, _scale,
                    preferredScale);
        }

        private static long add(long xs, long ys) {
            var sum = xs + ys;
            if (((sum ^ xs) & (sum ^ ys)) >= 0L) return sum;

            return INFLATED;
        }

        private static java.math.BigDecimal add(long xs, long ys, int _scale) {
            var sum = add(xs, ys);
            if (sum != INFLATED) return valueOf(sum, _scale);

            return new java.math.BigDecimal((BigInteger) xs+ys, _scale);
        }

        private static java.math.BigDecimal add(long xs, int scale1, long ys, int scale2) {
            var sdiff = (long) scale1 - scale2;
            if (sdiff == 0) {
                return add(xs, ys, scale1);
            }
            else {
                if (sdiff < 0) {
                    var raise = checkScale(xs, -sdiff);
                    var scaledX = longMultiplyPowerTen(xs, raise);
                    if (scaledX != INFLATED) {
                        return add(scaledX, ys, scale2);
                    }
                    else {
                        var bigsum = bigMultiplyPowerTen(xs, raise) + ys;
                        return (xs ^ ys) >= 0
                            ? new java.math.BigDecimal(bigsum, INFLATED, scale2, 0)
                            : valueOf(bigsum, scale2, 0);
                    }
                }
                else {
                    var raise = checkScale(ys, sdiff);
                    var scaledY = longMultiplyPowerTen(ys, raise);
                    if (scaledY != INFLATED) {
                        return add(xs, scaledY, scale1);
                    }
                    else {
                        var bigsum = bigMultiplyPowerTen(ys, raise) + xs;
                        return (xs ^ ys) >= 0
                            ? new java.math.BigDecimal(bigsum, INFLATED, scale1, 0)
                            : valueOf(bigsum, scale1, 0);
                    }
                }
            }
        }

        private static java.math.BigDecimal add(long xs, int scale1, BigInteger snd, int scale2) {
            var rscale = scale1;
            var sdiff = (long) rscale - scale2;
            var sameSigns = java.lang.Long.signum(xs) == snd.Sign;
            BigInteger sum;
            if (sdiff < 0) {
                var raise = checkScale(xs, -sdiff);
                rscale = scale2;
                var scaledX = longMultiplyPowerTen(xs, raise);
                if (scaledX == INFLATED)
                    sum = snd + bigMultiplyPowerTen(xs, raise);
                else
                    sum = snd + scaledX;
            }
            else {
                var raise = checkScale(snd, sdiff);
                snd = bigMultiplyPowerTen(snd, raise);
                sum = snd + xs;
            }

            return sameSigns ? new java.math.BigDecimal(sum, INFLATED, rscale, 0) : valueOf(sum, rscale, 0);
        }

        private static java.math.BigDecimal add(BigInteger fst, int scale1, BigInteger snd, int scale2) {
            var rscale = scale1;
            var sdiff = (long) rscale - scale2;
            if (sdiff != 0) {
                if (sdiff < 0) {
                    var raise = checkScale(fst, -sdiff);
                    rscale = scale2;
                    fst = bigMultiplyPowerTen(fst, raise);
                }
                else {
                    var raise = checkScale(snd, sdiff);
                    snd = bigMultiplyPowerTen(snd, raise);
                }
            }

            var sum = fst + snd;
            return fst.Sign == snd.Sign
                ? new java.math.BigDecimal(sum, INFLATED, rscale, 0)
                : valueOf(sum, rscale, 0);
        }

        private static BigInteger bigMultiplyPowerTen(long value, int n) {
            if (n <= 0) return (BigInteger) value;

            return bigTenToThe(n) * value;
        }

        private static BigInteger bigMultiplyPowerTen(BigInteger value, int n) {
            if (n <= 0) return value;

            if (n < LONG_TEN_POWERS_TABLE.Length) return value * LONG_TEN_POWERS_TABLE[n];

            return value * bigTenToThe(n);
        }

        private static java.math.BigDecimal divideSmallFastPath(long xs, int xscale, long ys, int yscale,
            long preferredScale, MathContext mc) {
            var mcp = mc.precision;
            var roundingMode = oldRoundingMode(mc.roundingMode);
            System.Diagnostics.Debug.Assert(xscale <= yscale && yscale < 18 && mcp < 18);
            var xraise = yscale - xscale;
            var scaledX = xraise == 0 ? xs : longMultiplyPowerTen(xs, xraise);
            java.math.BigDecimal quotient;
            var cmp = longCompareMagnitude(scaledX, ys);
            if (cmp > 0) {
                yscale -= 1;
                var scl = checkScaleNonZero(preferredScale + yscale - xscale + mcp);
                if (checkScaleNonZero((long) mcp + yscale - xscale) > 0) {
                    var raise = checkScaleNonZero((long) mcp + yscale - xscale);
                    long scaledXs;
                    if ((scaledXs = longMultiplyPowerTen(xs, raise)) == INFLATED) {
                        quotient = null;
                        if (mcp - 1 >= 0 && mcp - 1 < LONG_TEN_POWERS_TABLE.Length)
                            quotient = multiplyDivideAndRound(LONG_TEN_POWERS_TABLE[mcp - 1], scaledX, ys, scl,
                                roundingMode, checkScaleNonZero(preferredScale));

                        if (quotient == null) {
                            var rb = bigMultiplyPowerTen(scaledX, mcp - 1);
                            quotient = divideAndRound(rb, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                        }
                    }
                    else {
                        quotient = divideAndRound(scaledXs, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                    }
                }
                else {
                    var newScale = checkScaleNonZero((long) xscale - mcp);
                    if (newScale == yscale) {
                        quotient = divideAndRound(xs, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                    }
                    else {
                        var raise = checkScaleNonZero((long) newScale - yscale);
                        long scaledYs;
                        if ((scaledYs = longMultiplyPowerTen(ys, raise)) == INFLATED) {
                            var rb = bigMultiplyPowerTen(ys, raise);
                            quotient = divideAndRound((BigInteger) xs, rb, scl, roundingMode,
                                checkScaleNonZero(preferredScale));
                        }
                        else {
                            quotient = divideAndRound(xs, scaledYs, scl, roundingMode,
                                checkScaleNonZero(preferredScale));
                        }
                    }
                }
            }
            else {
                var scl = checkScaleNonZero(preferredScale + yscale - xscale + mcp);
                if (cmp == 0) {
                    quotient = roundedTenPower(scaledX < 0 == ys < 0 ? 1 : -1, mcp, scl,
                        checkScaleNonZero(preferredScale));
                }
                else {
                    long scaledXs;
                    if ((scaledXs = longMultiplyPowerTen(scaledX, mcp)) == INFLATED) {
                        quotient = null;
                        if (mcp < LONG_TEN_POWERS_TABLE.Length)
                            quotient = multiplyDivideAndRound(LONG_TEN_POWERS_TABLE[mcp], scaledX, ys, scl,
                                roundingMode, checkScaleNonZero(preferredScale));

                        if (quotient == null) {
                            var rb = bigMultiplyPowerTen(scaledX, mcp);
                            quotient = divideAndRound(rb, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                        }
                    }
                    else {
                        quotient = divideAndRound(scaledXs, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                    }
                }
            }

            return doRound(quotient, mc);
        }

        private static java.math.BigDecimal divide(long xs, int xscale, long ys, int yscale, long preferredScale,
            MathContext mc) {
            var mcp = mc.precision;
            if (xscale <= yscale && yscale < 18 && mcp < 18)
                return divideSmallFastPath(xs, xscale, ys, yscale, preferredScale, mc);

            if (compareMagnitudeNormalized(xs, xscale, ys, yscale) > 0) yscale -= 1;

            var roundingMode = oldRoundingMode(mc.roundingMode);
            var scl = checkScaleNonZero(preferredScale + yscale - xscale + mcp);
            java.math.BigDecimal quotient;
            if (checkScaleNonZero((long) mcp + yscale - xscale) > 0) {
                var raise = checkScaleNonZero((long) mcp + yscale - xscale);
                long scaledXs;
                if ((scaledXs = longMultiplyPowerTen(xs, raise)) == INFLATED) {
                    var rb = bigMultiplyPowerTen(xs, raise);
                    quotient = divideAndRound(rb, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                }
                else {
                    quotient = divideAndRound(scaledXs, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                }
            }
            else {
                var newScale = checkScaleNonZero((long) xscale - mcp);
                if (newScale == yscale) {
                    quotient = divideAndRound(xs, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                }
                else {
                    var raise = checkScaleNonZero((long) newScale - yscale);
                    long scaledYs;
                    if ((scaledYs = longMultiplyPowerTen(ys, raise)) == INFLATED) {
                        var rb = bigMultiplyPowerTen(ys, raise);
                        quotient = divideAndRound((BigInteger) xs, rb, scl, roundingMode,
                            checkScaleNonZero(preferredScale));
                    }
                    else {
                        quotient = divideAndRound(xs, scaledYs, scl, roundingMode, checkScaleNonZero(preferredScale));
                    }
                }
            }

            return doRound(quotient, mc);
        }

        private static java.math.BigDecimal divide(BigInteger xs, int xscale, long ys, int yscale, long preferredScale,
            MathContext mc) {
            if (-compareMagnitudeNormalized(ys, yscale, xs, xscale) > 0) yscale -= 1;

            var mcp = mc.precision;
            var roundingMode = oldRoundingMode(mc.roundingMode);
            java.math.BigDecimal quotient;
            var scl = checkScaleNonZero(preferredScale + yscale - xscale + mcp);
            if (checkScaleNonZero((long) mcp + yscale - xscale) > 0) {
                var raise = checkScaleNonZero((long) mcp + yscale - xscale);
                var rb = bigMultiplyPowerTen(xs, raise);
                quotient = divideAndRound(rb, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
            }
            else {
                var newScale = checkScaleNonZero((long) xscale - mcp);
                if (newScale == yscale) {
                    quotient = divideAndRound(xs, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
                }
                else {
                    var raise = checkScaleNonZero((long) newScale - yscale);
                    long scaledYs;
                    if ((scaledYs = longMultiplyPowerTen(ys, raise)) == INFLATED) {
                        var rb = bigMultiplyPowerTen(ys, raise);
                        quotient = divideAndRound(xs, rb, scl, roundingMode, checkScaleNonZero(preferredScale));
                    }
                    else {
                        quotient = divideAndRound(xs, scaledYs, scl, roundingMode, checkScaleNonZero(preferredScale));
                    }
                }
            }

            return doRound(quotient, mc);
        }

        private static java.math.BigDecimal divide(long xs, int xscale, BigInteger ys, int yscale, long preferredScale,
            MathContext mc) {
            if (compareMagnitudeNormalized(xs, xscale, ys, yscale) > 0) yscale -= 1;

            var mcp = mc.precision;
            var roundingMode = oldRoundingMode(mc.roundingMode);
            java.math.BigDecimal quotient;
            var scl = checkScaleNonZero(preferredScale + yscale - xscale + mcp);
            if (checkScaleNonZero((long) mcp + yscale - xscale) > 0) {
                var raise = checkScaleNonZero((long) mcp + yscale - xscale);
                var rb = bigMultiplyPowerTen(xs, raise);
                quotient = divideAndRound(rb, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
            }
            else {
                var newScale = checkScaleNonZero((long) xscale - mcp);
                var raise = checkScaleNonZero((long) newScale - yscale);
                var rb = bigMultiplyPowerTen(ys, raise);
                quotient = divideAndRound((BigInteger) xs, rb, scl, roundingMode, checkScaleNonZero(preferredScale));
            }

            return doRound(quotient, mc);
        }

        private static java.math.BigDecimal divide(BigInteger xs, int xscale, BigInteger ys, int yscale,
            long preferredScale, MathContext mc) {
            if (compareMagnitudeNormalized(xs, xscale, ys, yscale) > 0) yscale -= 1;

            var mcp = mc.precision;
            var roundingMode = oldRoundingMode(mc.roundingMode);
            java.math.BigDecimal quotient;
            var scl = checkScaleNonZero(preferredScale + yscale - xscale + mcp);
            if (checkScaleNonZero((long) mcp + yscale - xscale) > 0) {
                var raise = checkScaleNonZero((long) mcp + yscale - xscale);
                var rb = bigMultiplyPowerTen(xs, raise);
                quotient = divideAndRound(rb, ys, scl, roundingMode, checkScaleNonZero(preferredScale));
            }
            else {
                var newScale = checkScaleNonZero((long) xscale - mcp);
                var raise = checkScaleNonZero((long) newScale - yscale);
                var rb = bigMultiplyPowerTen(ys, raise);
                quotient = divideAndRound(xs, rb, scl, roundingMode, checkScaleNonZero(preferredScale));
            }

            return doRound(quotient, mc);
        }

        private static java.math.BigDecimal multiplyDivideAndRound(long dividend0, long dividend1, long divisor,
            int _scale, int roundingMode, int preferredScale) {
            var qsign = java.lang.Long.signum(dividend0) * java.lang.Long.signum(dividend1) *
                        java.lang.Long.signum(divisor);
            dividend0 = System.Math.Abs(dividend0);
            dividend1 = System.Math.Abs(dividend1);
            divisor = System.Math.Abs(divisor);
            var d0_hi = com.saxonica.glue.Utils.unsignedRightShift(dividend0, 32);
            var d0_lo = dividend0 & LONG_MASK;
            var d1_hi = com.saxonica.glue.Utils.unsignedRightShift(dividend1, 32);
            var d1_lo = dividend1 & LONG_MASK;
            var product = d0_lo * d1_lo;
            var d0 = product & LONG_MASK;
            var d1 = com.saxonica.glue.Utils.unsignedRightShift(product, 32);
            product = d0_hi * d1_lo + d1;
            d1 = product & LONG_MASK;
            var d2 = com.saxonica.glue.Utils.unsignedRightShift(product, 32);
            product = d0_lo * d1_hi + d1;
            d1 = product & LONG_MASK;
            d2 += com.saxonica.glue.Utils.unsignedRightShift(product, 32);
            var d3 = com.saxonica.glue.Utils.unsignedRightShift(d2, 32);
            d2 &= LONG_MASK;
            product = d0_hi * d1_hi + d2;
            d2 = product & LONG_MASK;
            d3 = (com.saxonica.glue.Utils.unsignedRightShift(product, 32) + d3) & LONG_MASK;
            var dividendHi = make64(d3, d2);
            var dividendLo = make64(d1, d0);
            return divideAndRound128(dividendHi, dividendLo, divisor, qsign, _scale, roundingMode, preferredScale);
        }

        private static readonly long DIV_NUM_BASE = 1L << 32;

        private static java.math.BigDecimal divideAndRound128(long dividendHi, long dividendLo, long divisor, int sign,
            int _scale, int roundingMode, int preferredScale) {
            if (dividendHi >= divisor) return null;

            int shift = java.lang.Long.numberOfLeadingZeros(divisor);
            divisor <<= shift;
            var v1 = com.saxonica.glue.Utils.unsignedRightShift(divisor, 32);
            var v0 = divisor & LONG_MASK;
            var tmp = dividendLo << shift;
            var u1 = com.saxonica.glue.Utils.unsignedRightShift(tmp, 32);
            var u0 = tmp & LONG_MASK;
            tmp = (dividendHi << shift) | com.saxonica.glue.Utils.unsignedRightShift(dividendLo, 64 - shift);
            var u2 = tmp & LONG_MASK;
            long q1, r_tmp;
            if (v1 == 1) {
                q1 = tmp;
                r_tmp = 0;
            }
            else {
                if (tmp >= 0) {
                    q1 = tmp / v1;
                    r_tmp = tmp - q1 * v1;
                }
                else {
                    var rq = divRemNegativeLong(tmp, v1);
                    q1 = rq[1];
                    r_tmp = rq[0];
                }
            }

            while (q1 >= DIV_NUM_BASE || unsignedLongCompare(q1 * v0, make64(r_tmp, u1))) {
                q1--;
                r_tmp += v1;
                if (r_tmp >= DIV_NUM_BASE) break;
            }

            tmp = mulsub(u2, u1, v1, v0, q1);
            u1 = tmp & LONG_MASK;
            long q0;
            if (v1 == 1) {
                q0 = tmp;
                r_tmp = 0;
            }
            else {
                if (tmp >= 0) {
                    q0 = tmp / v1;
                    r_tmp = tmp - q0 * v1;
                }
                else {
                    var rq = divRemNegativeLong(tmp, v1);
                    q0 = rq[1];
                    r_tmp = rq[0];
                }
            }

            while (q0 >= DIV_NUM_BASE || unsignedLongCompare(q0 * v0, make64(r_tmp, u0))) {
                q0--;
                r_tmp += v1;
                if (r_tmp >= DIV_NUM_BASE) break;
            }

            if ((int) q1 < 0) {
                MutableBigInteger mq = new MutableBigInteger(new int[] {(int) q1, (int) q0});
                if (roundingMode == ROUND_DOWN && _scale == preferredScale) return mq.toBigDecimal(sign, _scale);

                var r = com.saxonica.glue.Utils.unsignedRightShift(mulsub(u1, u0, v1, v0, q0), shift);
                if (r != 0) {
                    if (needIncrement(com.saxonica.glue.Utils.unsignedRightShift(divisor, shift), roundingMode, sign,
                        mq, r))
                        mq.add(MutableBigInteger.ONE);

                    return mq.toBigDecimal(sign, _scale);
                }
                else {
                    if (preferredScale != _scale) {
                        BigInteger intVal = mq.toBigInteger(sign);
                        return createAndStripZerosToMatchScale(intVal, _scale, preferredScale);
                    }
                    else {
                        return mq.toBigDecimal(sign, _scale);
                    }
                }
            }

            var q = make64(q1, q0);
            q *= sign;
            if (roundingMode == ROUND_DOWN && _scale == preferredScale) return valueOf(q, _scale);

            var r = com.saxonica.glue.Utils.unsignedRightShift(mulsub(u1, u0, v1, v0, q0), shift);
            if (r != 0) {
                var increment = needIncrement(com.saxonica.glue.Utils.unsignedRightShift(divisor, shift), roundingMode,
                    sign, q, r);
                return valueOf(increment ? q + sign : q, _scale);
            }
            else {
                if (preferredScale != _scale)
                    return createAndStripZerosToMatchScale(q, _scale, preferredScale);
                else
                    return valueOf(q, _scale);
            }
        }

        private static java.math.BigDecimal roundedTenPower(int qsign, int raise, int _scale, int preferredScale) {
            if (_scale > preferredScale) {
                var diff = _scale - preferredScale;
                if (diff < raise)
                    return scaledTenPow(raise - diff, qsign, preferredScale);
                else
                    return valueOf(qsign, _scale - raise);
            }
            else {
                return scaledTenPow(raise, qsign, _scale);
            }
        }

        private static java.math.BigDecimal scaledTenPow(int n, int sign, int _scale) {
            if (n < LONG_TEN_POWERS_TABLE.Length) {
                return valueOf(sign * LONG_TEN_POWERS_TABLE[n], _scale);
            }
            else {
                var unscaledVal = bigTenToThe(n);
                if (sign == -1) unscaledVal = -unscaledVal;

                return new java.math.BigDecimal(unscaledVal, INFLATED, _scale, n + 1);
            }
        }

        private static long[] divRemNegativeLong(long n, long d) {
            // System.Diagnostics.Debug.Assert((n < 0)("Non-negative numerator " + n));
            // System.Diagnostics.Debug.Assert(d != 1"Unity denominator");
            var q = com.saxonica.glue.Utils.unsignedRightShift(n, 1) /
                    com.saxonica.glue.Utils.unsignedRightShift(d, 1);
            var r = n - q * d;
            while (r < 0) {
                r += d;
                q--;
            }

            while (r >= d) {
                r -= d;
                q++;
            }

            return new long[] {r, q};
        }

        private static long make64(long hi, long lo) {
            return (hi << 32) | lo;
        }

        private static long mulsub(long u1, long u0, long v1, long v0, long q0) {
            var tmp = u0 - q0 * v0;
            return make64(u1 + com.saxonica.glue.Utils.unsignedRightShift(tmp, 32) - q0 * v1,
                tmp & LONG_MASK);
        }

        private static bool unsignedLongCompare(long one, long two) {
            return one + long.MinValue > two + long.MinValue;
        }

        private static bool unsignedLongCompareEq(long one, long two) {
            return one + long.MinValue >= two + long.MinValue;
        }

        private static int compareMagnitudeNormalized(long xs, int xscale, long ys, int yscale) {
            var sdiff = xscale - yscale;
            if (sdiff != 0) {
                if (sdiff < 0)
                    xs = longMultiplyPowerTen(xs, -sdiff);
                else
                    ys = longMultiplyPowerTen(ys, sdiff);
            }

            if (xs != INFLATED)
                return ys != INFLATED ? longCompareMagnitude(xs, ys) : -1;
            else
                return 1;
        }

        private static int compareMagnitudeNormalized(long xs, int xscale, BigInteger ys, int yscale) {
            if (xs == 0) return -1;

            var sdiff = xscale - yscale;
            if (sdiff < 0)
                if (longMultiplyPowerTen(xs, -sdiff) == INFLATED)
                    return compareBigIntegerMagnitude(bigMultiplyPowerTen(xs, -sdiff), ys);

            return -1;
        }

        private static int compareMagnitudeNormalized(BigInteger xs, int xscale, BigInteger ys, int yscale) {
            var sdiff = xscale - yscale;
            if (sdiff < 0)
                return compareBigIntegerMagnitude(bigMultiplyPowerTen(xs, -sdiff), ys);
            else
                return compareBigIntegerMagnitude(xs, bigMultiplyPowerTen(ys, sdiff));
        }

        private static long multiply(long x, long y) {
            var product = x * y;
            var ax = System.Math.Abs(x);
            var ay = System.Math.Abs(y);
            if (com.saxonica.glue.Utils.unsignedRightShift(ax | ay, 31) == 0 || y == 0 ||
                product / y == x)
                return product;

            return INFLATED;
        }

        private static java.math.BigDecimal multiply(long x, long y, int _scale) {
            var product = multiply(x, y);
            if (product != INFLATED) return valueOf(product, _scale);

            return new java.math.BigDecimal((BigInteger) x*y, INFLATED, _scale, 0);
        }

        private static java.math.BigDecimal multiply(long x, BigInteger y, int _scale) {
            if (x == 0) return zeroValueOf(_scale);

            return new java.math.BigDecimal(y * x, INFLATED, _scale, 0);
        }

        private static java.math.BigDecimal multiply(BigInteger x, BigInteger y, int _scale) {
            return new java.math.BigDecimal(x * y, INFLATED, _scale, 0);
        }

        private static java.math.BigDecimal multiplyAndRound(long x, long y, int _scale, MathContext mc) {
            var product = multiply(x, y);
            if (product != INFLATED) return doRound(product, _scale, mc);

            var rsign = 1;
            if (x < 0) {
                x = -x;
                rsign = -1;
            }

            if (y < 0) {
                y = -y;
                rsign *= -1;
            }

            var m0_hi = com.saxonica.glue.Utils.unsignedRightShift(x, 32);
            var m0_lo = x & LONG_MASK;
            var m1_hi = com.saxonica.glue.Utils.unsignedRightShift(y, 32);
            var m1_lo = y & LONG_MASK;
            product = m0_lo * m1_lo;
            var m0 = product & LONG_MASK;
            var m1 = com.saxonica.glue.Utils.unsignedRightShift(product, 32);
            product = m0_hi * m1_lo + m1;
            m1 = product & LONG_MASK;
            var m2 = com.saxonica.glue.Utils.unsignedRightShift(product, 32);
            product = m0_lo * m1_hi + m1;
            m1 = product & LONG_MASK;
            m2 += com.saxonica.glue.Utils.unsignedRightShift(product, 32);
            var m3 = com.saxonica.glue.Utils.unsignedRightShift(m2, 32);
            m2 &= LONG_MASK;
            product = m0_hi * m1_hi + m2;
            m2 = product & LONG_MASK;
            m3 = (com.saxonica.glue.Utils.unsignedRightShift(product, 32) + m3) & LONG_MASK;
            var mHi = make64(m3, m2);
            var mLo = make64(m1, m0);
            var res = doRound128(mHi, mLo, rsign, _scale, mc);
            if (res != null) return res;

            res = new java.math.BigDecimal((BigInteger) x*(y * rsign), INFLATED, _scale, 0);
            return doRound(res, mc);
        }

        private static java.math.BigDecimal multiplyAndRound(long x, BigInteger y, int _scale, MathContext mc) {
            if (x == 0) return zeroValueOf(_scale);

            return doRound(y * x, _scale, mc);
        }

        private static java.math.BigDecimal multiplyAndRound(BigInteger x, BigInteger y, int _scale, MathContext mc) {
            return doRound(x * y, _scale, mc);
        }

        private static java.math.BigDecimal doRound128(long hi, long lo, int sign, int _scale, MathContext mc) {
            var mcp = mc.precision;
            int drop;
            java.math.BigDecimal res = null;
            if ((drop = precision(hi, lo) - mcp) > 0 && drop < LONG_TEN_POWERS_TABLE.Length) {
                _scale = checkScaleNonZero((long) _scale - drop);
                res = divideAndRound128(hi, lo, LONG_TEN_POWERS_TABLE[drop], sign, _scale,
                    oldRoundingMode(mc.roundingMode), _scale);
            }

            if (res != null) return doRound(res, mc);

            return null;
        }

        private static readonly ulong[][] LONGLONG_TEN_POWERS_TABLE = {
            new ulong[]{0L, 0x8AC7230489E80000L}, 
            new ulong[]{0x5L, 0x6bc75e2d63100000L}, 
            new ulong[]{0x36L, 0x35c9adc5dea00000L},
            new ulong[]{0x21eL, 0x19e0c9bab2400000L}, 
            new ulong[]{0x152dL, 0x02c7e14af6800000L}, 
            new ulong[]{0xd3c2L, 0x1bcecceda1000000L},
            new ulong[]{0x84595L, 0x161401484a000000L}, 
            new ulong[]{0x52b7d2L, 0xdcc80cd2e4000000L}, 
            new ulong[]{0x33b2e3cL, 0x9fd0803ce8000000L},
            new ulong[]{0x204fce5eL, 0x3e25026110000000L}, 
            new ulong[]{0x1431e0faeL, 0x6d7217caa0000000L},
            new ulong[]{0xc9f2c9cd0L, 0x4674edea40000000L}, 
            new ulong[]{0x7e37be2022L, 0xc0914b2680000000L},
            new ulong[]{0x4ee2d6d415bL, 0x85acef8100000000L}, 
            new ulong[]{0x314dc6448d93L, 0x38c15b0a00000000L},
            new ulong[]{0x1ed09bead87c0L, 0x378d8e6400000000L}, 
            new ulong[]{0x13426172c74d82L, 0x2b878fe800000000L},
            new ulong[]{0xc097ce7bc90715L, 0xb34b9f1000000000L}, 
            new ulong[]{0x785ee10d5da46d9L, 0x00f436a000000000L},
            new ulong[]{0x4b3b4ca85a86c47aL, 0x098a224000000000L}
        };

        private static int precision(long hi, long lo) {
            if (hi == 0) {
                if (lo >= 0) return longDigitLength(lo);

                return unsignedLongCompareEq(lo, LONGLONG_TEN_POWERS_TABLE[0][1]) ? 20 : 19;
            }

            var r = com.saxonica.glue.Utils.unsignedRightShift(
                (128 - java.lang.Long.numberOfLeadingZeros(hi) + 1) * 1233, 12);
            var idx = r - 19;
            return idx >= LONGLONG_TEN_POWERS_TABLE.Length || longLongCompareMagnitude(hi, lo,
                LONGLONG_TEN_POWERS_TABLE[idx][0], LONGLONG_TEN_POWERS_TABLE[idx][1])
                ? r
                : r + 1;
        }

        private static bool longLongCompareMagnitude(long hi0, long lo0, long hi1, long lo1) {
            if (hi0 != hi1) return hi0 < hi1;

            return lo0 + long.MinValue < lo1 + long.MinValue;
        }

        private static java.math.BigDecimal divide(long dividend, int dividendScale, long divisor, int divisorScale,
            int _scale, int roundingMode) {
            if (checkScale(dividend, (long) _scale + divisorScale) > dividendScale) {
                var newScale = _scale + divisorScale;
                var raise = newScale - dividendScale;
                if (raise < LONG_TEN_POWERS_TABLE.Length) {
                    var xs = dividend;
                    if ((xs = longMultiplyPowerTen(xs, raise)) != INFLATED)
                        return divideAndRound(xs, divisor, _scale, roundingMode, _scale);

                    var q = multiplyDivideAndRound(LONG_TEN_POWERS_TABLE[raise], dividend, divisor,
                        _scale, roundingMode, _scale);
                    if (q != null) return q;
                }

                var scaledDividend = bigMultiplyPowerTen(dividend, raise);
                return divideAndRound(scaledDividend, divisor, _scale, roundingMode, _scale);
            }
            else {
                var newScale = checkScale(divisor, (long) dividendScale - _scale);
                var raise = newScale - divisorScale;
                if (raise < LONG_TEN_POWERS_TABLE.Length) {
                    var ys = divisor;
                    if ((ys = longMultiplyPowerTen(ys, raise)) != INFLATED)
                        return divideAndRound(dividend, ys, _scale, roundingMode, _scale);
                }

                var scaledDivisor = bigMultiplyPowerTen(divisor, raise);
                return divideAndRound((BigInteger) dividend, scaledDivisor, _scale, roundingMode, _scale);
            }
        }

        private static java.math.BigDecimal divide(BigInteger dividend, int dividendScale, long divisor,
            int divisorScale, int _scale, int roundingMode) {
            if (checkScale(dividend, (long) _scale + divisorScale) > dividendScale) {
                var newScale = _scale + divisorScale;
                var raise = newScale - dividendScale;
                var scaledDividend = bigMultiplyPowerTen(dividend, raise);
                return divideAndRound(scaledDividend, divisor, _scale, roundingMode, _scale);
            }
            else {
                var newScale = checkScale(divisor, (long) dividendScale - _scale);
                var raise = newScale - divisorScale;
                if (raise < LONG_TEN_POWERS_TABLE.Length) {
                    var ys = divisor;
                    if ((ys = longMultiplyPowerTen(ys, raise)) != INFLATED)
                        return divideAndRound(dividend, ys, _scale, roundingMode, _scale);
                }

                var scaledDivisor = bigMultiplyPowerTen(divisor, raise);
                return divideAndRound(dividend, scaledDivisor, _scale, roundingMode, _scale);
            }
        }

        private static java.math.BigDecimal divide(long dividend, int dividendScale, BigInteger divisor,
            int divisorScale, int _scale, int roundingMode) {
            if (checkScale(dividend, (long) _scale + divisorScale) > dividendScale) {
                var newScale = _scale + divisorScale;
                var raise = newScale - dividendScale;
                var scaledDividend = bigMultiplyPowerTen(dividend, raise);
                return divideAndRound(scaledDividend, divisor, _scale, roundingMode, _scale);
            }
            else {
                var newScale = checkScale(divisor, (long) dividendScale - _scale);
                var raise = newScale - divisorScale;
                var scaledDivisor = bigMultiplyPowerTen(divisor, raise);
                return divideAndRound((BigInteger) dividend, scaledDivisor, _scale, roundingMode, _scale);
            }
        }

        private static java.math.BigDecimal divide(BigInteger dividend, int dividendScale, BigInteger divisor,
            int divisorScale, int _scale, int roundingMode) {
            if (checkScale(dividend, (long) _scale + divisorScale) > dividendScale) {
                var newScale = _scale + divisorScale;
                var raise = newScale - dividendScale;
                var scaledDividend = bigMultiplyPowerTen(dividend, raise);
                return divideAndRound(scaledDividend, divisor, _scale, roundingMode, _scale);
            }
            else {
                var newScale = checkScale(divisor, (long) dividendScale - _scale);
                var raise = newScale - divisorScale;
                var scaledDivisor = bigMultiplyPowerTen(divisor, raise);
                return divideAndRound(dividend, scaledDivisor, _scale, roundingMode, _scale);
            }
        }
    }
}