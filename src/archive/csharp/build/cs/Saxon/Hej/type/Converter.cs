using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class Converter {
        public static Saxon.Hej.value.AtomicValue convert(Saxon.Hej.value.AtomicValue value, Saxon.Hej.type.AtomicType targetType, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.Converter converter = rules.getConverter(value.getPrimitiveType(), targetType);
            if ((converter == null)) {
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Cannot convert value from "+value.getPrimitiveType())+" to ")+targetType));
                ve.setErrorCode("FORG0001");
                throw ve.makeException();
            }
            return converter.convert(value).asAtomic();
        }
        private Saxon.Hej.lib.ConversionRules conversionRules;
        protected internal Converter()  {
        }
        protected internal Converter(Saxon.Hej.lib.ConversionRules rules)  {
            setConversionRules(rules);
        }
        public abstract Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input);
        public void setConversionRules(Saxon.Hej.lib.ConversionRules rules) {
            this.conversionRules = rules;
        }
        public Saxon.Hej.lib.ConversionRules getConversionRules() {
            return conversionRules;
        }
        public virtual bool isAlwaysSuccessful() {
            return false;
        }
        public virtual Saxon.Hej.type.Converter setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            return this;
        }
        public virtual Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return null;
        }
        public abstract class UnfailingConverter : Saxon.Hej.type.Converter {
            public abstract override Saxon.Hej.value.AtomicValue convert(Saxon.Hej.value.AtomicValue input);
            public override bool isAlwaysSuccessful() {
                return true;
            }
        
        }
        public class IdentityConverter : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.IdentityConverter INSTANCE = new Saxon.Hej.type.Converter.IdentityConverter();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                return input;
            }
            public override bool isAlwaysSuccessful() {
                return true;
            }
        
        }
        public class UpCastingConverter : Saxon.Hej.type.Converter.UnfailingConverter {
            private readonly Saxon.Hej.type.AtomicType newTypeAnnotation;
            public UpCastingConverter(Saxon.Hej.type.AtomicType annotation)  {
                this.newTypeAnnotation = annotation;
            }
            public override Saxon.Hej.value.AtomicValue convert(Saxon.Hej.value.AtomicValue input) {
                return input.copyAsSubType(newTypeAnnotation);
            }
        
        }
        public class DownCastingConverter : Saxon.Hej.type.Converter {
            private readonly Saxon.Hej.type.AtomicType newType;
            public DownCastingConverter(Saxon.Hej.type.AtomicType annotation, Saxon.Hej.lib.ConversionRules rules)  {
                this.newType = annotation;
                setConversionRules(rules);
            }
            public Saxon.Hej.type.AtomicType getTargetType() {
                return newType;
            }
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                return convert(input, input.getCanonicalLexicalRepresentation());
            }
            public Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input, Saxon.Hej.str.UnicodeString lexicalForm) {
                if (Saxon.Hej.type.Type.isSubType(input.getItemType(), newType)) {
                    return input;
                }
                if ((input.getUType() != newType.getUType())) {
                    return new Saxon.Hej.type.ValidationFailure(((("Cannot convert "+input.toShortString())+" to ")+newType.getDisplayName()));
                }
                Saxon.Hej.type.ValidationFailure f = newType.validate(input, lexicalForm, getConversionRules());
                if ((f == null)) {
                    return input.copyAsSubType(newType);
                } else {
                    return f;
                }
            }
            public Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.value.AtomicValue input, Saxon.Hej.str.UnicodeString lexicalForm) {
                return newType.validate(input, lexicalForm, getConversionRules());
            }
        
        }
        public class TwoPhaseConverter : Saxon.Hej.type.Converter {
            private readonly Saxon.Hej.type.Converter phaseOne;
            private readonly Saxon.Hej.type.Converter phaseTwo;
            public TwoPhaseConverter(Saxon.Hej.type.Converter phaseOne, Saxon.Hej.type.Converter phaseTwo)  {
                this.phaseOne = phaseOne;
                this.phaseTwo = phaseTwo;
            }
            public static Saxon.Hej.type.Converter.TwoPhaseConverter makeTwoPhaseConverter(Saxon.Hej.type.AtomicType inputType, Saxon.Hej.type.AtomicType viaType, Saxon.Hej.type.AtomicType outputType, Saxon.Hej.lib.ConversionRules rules) {
                return new Saxon.Hej.type.Converter.TwoPhaseConverter(rules.getConverter(inputType, viaType), rules.getConverter(viaType, outputType));
            }
            public override Saxon.Hej.type.Converter setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
                return new Saxon.Hej.type.Converter.TwoPhaseConverter(phaseOne.setNamespaceResolver(resolver), phaseTwo.setNamespaceResolver(resolver));
            }
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.type.ConversionResult temp = phaseOne.convert(input);
                if ((temp is Saxon.Hej.type.ValidationFailure)) {
                    return temp;
                }
                Saxon.Hej.value.AtomicValue aTemp = (Saxon.Hej.value.AtomicValue)(temp);
                if ((phaseTwo is Saxon.Hej.type.Converter.DownCastingConverter)) {
                    return ((Saxon.Hej.type.Converter.DownCastingConverter)(phaseTwo)).convert(aTemp, aTemp.getCanonicalLexicalRepresentation());
                } else {
                    return phaseTwo.convert(aTemp);
                }
            }
        
        }
        public class ToUntypedAtomicConverter : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.ToUntypedAtomicConverter INSTANCE = new Saxon.Hej.type.Converter.ToUntypedAtomicConverter();
            public override Saxon.Hej.value.StringValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.UntypedAtomicValue(input.getUnicodeStringValue());
            }
        
        }
        public class ToStringConverter : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.ToStringConverter INSTANCE = new Saxon.Hej.type.Converter.ToStringConverter();
            public override Saxon.Hej.value.StringValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.StringValue(input.getUnicodeStringValue().tidy());
            }
        
        }
        public class NumericToFloat : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.NumericToFloat INSTANCE = new Saxon.Hej.type.Converter.NumericToFloat();
            public override Saxon.Hej.value.FloatValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.FloatValue(((Saxon.Hej.value.NumericValue)(input)).getFloatValue());
            }
        
        }
        public class BooleanToFloat : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.BooleanToFloat INSTANCE = new Saxon.Hej.type.Converter.BooleanToFloat();
            public override Saxon.Hej.value.FloatValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.FloatValue((((Saxon.Hej.value.BooleanValue)(input)).getBooleanValue() ? 1.0f : 0.0f));
            }
        
        }
        public class NumericToDouble : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.NumericToDouble INSTANCE = new Saxon.Hej.type.Converter.NumericToDouble();
            public override Saxon.Hej.value.DoubleValue convert(Saxon.Hej.value.AtomicValue input) {
                if ((input is Saxon.Hej.value.DoubleValue)) {
                    return (Saxon.Hej.value.DoubleValue)(input);
                } else {
                    return new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(input)).getDoubleValue());
                }
            }
        
        }
        public class BooleanToDouble : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.BooleanToDouble INSTANCE = new Saxon.Hej.type.Converter.BooleanToDouble();
            public override Saxon.Hej.value.DoubleValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.BooleanValue)(input)).getBooleanValue() ? 1.0e0 : 0.0e0));
            }
        
        }
        public class DoubleToDecimal : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.DoubleToDecimal INSTANCE = new Saxon.Hej.type.Converter.DoubleToDecimal();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                try {
                    return new Saxon.Hej.value.BigDecimalValue(((Saxon.Hej.value.DoubleValue)(input)).getDoubleValue());
                } catch (Saxon.Hej.type.ValidationException e) {
                    return e.getValidationFailure();
                }
            }
        
        }
        public class FloatToDecimal : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.FloatToDecimal INSTANCE = new Saxon.Hej.type.Converter.FloatToDecimal();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                try {
                    return new Saxon.Hej.value.BigDecimalValue(((Saxon.Hej.value.FloatValue)(input)).getFloatValue());
                } catch (Saxon.Hej.type.ValidationException e) {
                    return e.getValidationFailure();
                }
            }
        
        }
        public class IntegerToDecimal : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.IntegerToDecimal INSTANCE = new Saxon.Hej.type.Converter.IntegerToDecimal();
            public override Saxon.Hej.value.BigDecimalValue convert(Saxon.Hej.value.AtomicValue input) {
                if ((input is Saxon.Hej.value.Int64Value)) {
                    return new Saxon.Hej.value.BigDecimalValue(((Saxon.Hej.value.Int64Value)(input)).longValue());
                } else {
                    return new Saxon.Hej.value.BigDecimalValue(((Saxon.Hej.value.BigIntegerValue)(input)).asDecimal());
                }
            }
        
        }
        public class NumericToDecimal : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.NumericToDecimal INSTANCE = new Saxon.Hej.type.Converter.NumericToDecimal();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                try {
                    Singulink.Numerics.BigDecimal @decimal = ((Saxon.Hej.value.NumericValue)(input)).getDecimalValue();
                    return new Saxon.Hej.value.BigDecimalValue(@decimal);
                } catch (Saxon.Hej.type.ValidationException e) {
                    return e.getValidationFailure();
                }
            }
        
        }
        public class BooleanToDecimal : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.BooleanToDecimal INSTANCE = new Saxon.Hej.type.Converter.BooleanToDecimal();
            public override Saxon.Hej.value.BigDecimalValue convert(Saxon.Hej.value.AtomicValue input) {
                return (((Saxon.Hej.value.BooleanValue)(input)).getBooleanValue() ? Saxon.Hej.value.BigDecimalValue.ONE : Saxon.Hej.value.BigDecimalValue.ZERO);
            }
        
        }
        public class DoubleToInteger : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.DoubleToInteger INSTANCE = new Saxon.Hej.type.Converter.DoubleToInteger();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                return Saxon.Hej.value.IntegerValue.fromDouble(((Saxon.Hej.value.DoubleValue)(input)).getDoubleValue());
            }
        
        }
        public class FloatToInteger : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.FloatToInteger INSTANCE = new Saxon.Hej.type.Converter.FloatToInteger();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                return Saxon.Hej.value.IntegerValue.fromDouble(((Saxon.Hej.value.FloatValue)(input)).getDoubleValue());
            }
        
        }
        public class DecimalToInteger : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DecimalToInteger INSTANCE = new Saxon.Hej.type.Converter.DecimalToInteger();
            public override Saxon.Hej.value.IntegerValue convert(Saxon.Hej.value.AtomicValue input) {
                if ((input is Saxon.Hej.value.IntegerValue)) {
                    return (Saxon.Hej.value.IntegerValue)(input);
                }
                return Saxon.Hej.value.BigIntegerValue.makeIntegerValue(((System.Numerics.BigInteger)((Saxon.Hej.value.BigDecimalValue)(input)).getDecimalValue()));
            }
        
        }
        public class NumericToInteger : Saxon.Hej.type.Converter {
            public readonly static Saxon.Hej.type.Converter.NumericToInteger INSTANCE = new Saxon.Hej.type.Converter.NumericToInteger();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.NumericValue @in = (Saxon.Hej.value.NumericValue)(input);
                try {
                    if ((@in is Saxon.Hej.value.IntegerValue)) {
                        return @in;
                    } else {
                        if ((@in is Saxon.Hej.value.DoubleValue)) {
                            return Saxon.Hej.value.IntegerValue.fromDouble(@in.getDoubleValue());
                        } else {
                            if ((@in is Saxon.Hej.value.FloatValue)) {
                                return Saxon.Hej.value.IntegerValue.fromDouble(@in.getDoubleValue());
                            } else {
                                return Saxon.Hej.value.BigIntegerValue.makeIntegerValue(((System.Numerics.BigInteger)@in.getDecimalValue()));
                            }
                        }
                    }
                } catch (Saxon.Hej.type.ValidationException e) {
                    return e.getValidationFailure();
                }
            }
        
        }
        public class BooleanToInteger : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.BooleanToInteger INSTANCE = new Saxon.Hej.type.Converter.BooleanToInteger();
            public override Saxon.Hej.value.Int64Value convert(Saxon.Hej.value.AtomicValue input) {
                return (((Saxon.Hej.value.BooleanValue)(input)).getBooleanValue() ? Saxon.Hej.value.Int64Value.PLUS_ONE : Saxon.Hej.value.Int64Value.ZERO);
            }
        
        }
        public class DurationToDayTimeDuration : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DurationToDayTimeDuration INSTANCE = new Saxon.Hej.type.Converter.DurationToDayTimeDuration();
            public override Saxon.Hej.value.DayTimeDurationValue convert(Saxon.Hej.value.AtomicValue duration) {
                Saxon.Hej.value.DurationValue d = (Saxon.Hej.value.DurationValue)(duration);
                if ((d.signum()<0)) {
                    return new Saxon.Hej.value.DayTimeDurationValue(-(d.getDays()), -(d.getHours()), -(d.getMinutes()), -(d.getSeconds()), -(d.getNanoseconds()));
                } else {
                    return new Saxon.Hej.value.DayTimeDurationValue(d.getDays(), d.getHours(), d.getMinutes(), d.getSeconds(), d.getNanoseconds());
                }
            }
        
        }
        public class DurationToYearMonthDuration : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DurationToYearMonthDuration INSTANCE = new Saxon.Hej.type.Converter.DurationToYearMonthDuration();
            public override Saxon.Hej.value.YearMonthDurationValue convert(Saxon.Hej.value.AtomicValue input) {
                return Saxon.Hej.value.YearMonthDurationValue.fromMonths(((Saxon.Hej.value.DurationValue)(input)).getTotalMonths());
            }
        
        }
        public class DateToDateTime : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DateToDateTime INSTANCE = new Saxon.Hej.type.Converter.DateToDateTime();
            public override Saxon.Hej.value.DateTimeValue convert(Saxon.Hej.value.AtomicValue input) {
                return ((Saxon.Hej.value.DateValue)(input)).toDateTime();
            }
        
        }
        public class DateTimeToDate : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DateTimeToDate INSTANCE = new Saxon.Hej.type.Converter.DateTimeToDate();
            public override Saxon.Hej.value.DateValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.DateValue(dt.getYear(), dt.getMonth(), dt.getDay(), dt.getTimezoneInMinutes(), dt.isXsd10Rules());
            }
        
        }
        public class DateTimeToGMonth : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.DateTimeToGMonth INSTANCE = new Saxon.Hej.type.Converter.DateTimeToGMonth();
            public override Saxon.Hej.value.GMonthValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.GMonthValue(dt.getMonth(), dt.getTimezoneInMinutes());
            }
        
        }
        public class DateTimeToGYearMonth : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.DateTimeToGYearMonth INSTANCE = new Saxon.Hej.type.Converter.DateTimeToGYearMonth();
            public override Saxon.Hej.value.GYearMonthValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.GYearMonthValue(dt.getYear(), dt.getMonth(), dt.getTimezoneInMinutes(), dt.isXsd10Rules());
            }
        
        }
        public class DateTimeToGYear : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.DateTimeToGYear INSTANCE = new Saxon.Hej.type.Converter.DateTimeToGYear();
            public override Saxon.Hej.value.GYearValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.GYearValue(dt.getYear(), dt.getTimezoneInMinutes(), dt.isXsd10Rules());
            }
        
        }
        public class DateTimeToGMonthDay : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.DateTimeToGMonthDay INSTANCE = new Saxon.Hej.type.Converter.DateTimeToGMonthDay();
            public override Saxon.Hej.value.GMonthDayValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.GMonthDayValue(dt.getMonth(), dt.getDay(), dt.getTimezoneInMinutes());
            }
        
        }
        public class DateTimeToGDay : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DateTimeToGDay INSTANCE = new Saxon.Hej.type.Converter.DateTimeToGDay();
            public override Saxon.Hej.value.GDayValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.GDayValue(dt.getDay(), dt.getTimezoneInMinutes());
            }
        
        }
        public class DateTimeToTime : Saxon.Hej.type.Converter.UnfailingConverter {
            public readonly static Saxon.Hej.type.Converter.DateTimeToTime INSTANCE = new Saxon.Hej.type.Converter.DateTimeToTime();
            public override Saxon.Hej.value.TimeValue convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.value.DateTimeValue dt = (Saxon.Hej.value.DateTimeValue)(input);
                return new Saxon.Hej.value.TimeValue(dt.getHour(), dt.getMinute(), dt.getSecond(), dt.getNanosecond(), dt.getTimezoneInMinutes(), "");
            }
        
        }
        public class NumericToBoolean : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.NumericToBoolean INSTANCE = new Saxon.Hej.type.Converter.NumericToBoolean();
            public override Saxon.Hej.value.BooleanValue convert(Saxon.Hej.value.AtomicValue input) {
                return Saxon.Hej.value.BooleanValue.get(((Saxon.Hej.value.NumericValue)(input)).effectiveBooleanValue());
            }
        
        }
        public class Base64BinaryToHexBinary : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.Base64BinaryToHexBinary INSTANCE = new Saxon.Hej.type.Converter.Base64BinaryToHexBinary();
            public override Saxon.Hej.value.HexBinaryValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.HexBinaryValue(((Saxon.Hej.value.Base64BinaryValue)(input)).getBinaryValue());
            }
        
        }
        public class HexBinaryToBase64Binary : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.HexBinaryToBase64Binary INSTANCE = new Saxon.Hej.type.Converter.HexBinaryToBase64Binary();
            public override Saxon.Hej.value.Base64BinaryValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.Base64BinaryValue(((Saxon.Hej.value.HexBinaryValue)(input)).getBinaryValue());
            }
        
        }
        public class NotationToQName : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.NotationToQName INSTANCE = new Saxon.Hej.type.Converter.NotationToQName();
            public override Saxon.Hej.value.QNameValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.QNameValue(((Saxon.Hej.value.NotationValue)(input)).getStructuredQName(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
            }
        
        }
        public class QNameToNotation : Saxon.Hej.type.Converter.UnfailingConverter {
            public static readonly Saxon.Hej.type.Converter.QNameToNotation INSTANCE = new Saxon.Hej.type.Converter.QNameToNotation();
            public override Saxon.Hej.value.NotationValue convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.NotationValue(((Saxon.Hej.value.QNameValue)(input)).getStructuredQName(), Saxon.Hej.type.BuiltInAtomicType.NOTATION);
            }
        
        }
        public class PromoterToDouble : Saxon.Hej.type.Converter {
            private Saxon.Hej.type.StringConverter stringToDouble = null;
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                if ((input is Saxon.Hej.value.DoubleValue)) {
                    return input;
                } else {
                    if ((input is Saxon.Hej.value.NumericValue)) {
                        return new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(input)).getDoubleValue());
                    } else {
                        if (input.isUntypedAtomic()) {
                            if ((stringToDouble == null)) {
                                stringToDouble = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.getStringConverter(getConversionRules());
                            }
                            return stringToDouble.convert(input);
                        } else {
                            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure("Cannot promote non-numeric value to xs:double");
                            err.setErrorCode("XPTY0004");
                            return err;
                        }
                    }
                }
            }
        
        }
        public class PromoterToFloat : Saxon.Hej.type.Converter {
            private Saxon.Hej.type.StringConverter stringToFloat = null;
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                if ((input is Saxon.Hej.value.FloatValue)) {
                    return input;
                } else {
                    if ((input is Saxon.Hej.value.DoubleValue)) {
                        Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure("Cannot promote from xs:double to xs:float");
                        err.setErrorCode("XPTY0004");
                        return err;
                    } else {
                        if ((input is Saxon.Hej.value.NumericValue)) {
                            return new Saxon.Hej.value.FloatValue((float)(((Saxon.Hej.value.NumericValue)(input)).getDoubleValue()));
                        } else {
                            if (input.isUntypedAtomic()) {
                                if ((stringToFloat == null)) {
                                    stringToFloat = Saxon.Hej.type.BuiltInAtomicType.FLOAT.getStringConverter(getConversionRules());
                                }
                                return stringToFloat.convert(input);
                            } else {
                                Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure("Cannot promote non-numeric value to xs:double");
                                err.setErrorCode("XPTY0004");
                                return err;
                            }
                        }
                    }
                }
            }
        
        }
    
    }
}