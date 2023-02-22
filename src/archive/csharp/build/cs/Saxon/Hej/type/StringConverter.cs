using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class StringConverter : Saxon.Hej.type.Converter {
        protected internal StringConverter()  {
        }
        protected internal StringConverter(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
        }
        public abstract Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input);
        public virtual Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
            Saxon.Hej.type.ConversionResult result = convertString(input);
            return ((result is Saxon.Hej.type.ValidationFailure) ? (Saxon.Hej.type.ValidationFailure)(result) : null);
        }
        public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
            return convertString(input.getUnicodeStringValue());
        }
        public class StringToNonStringDerivedType : Saxon.Hej.type.StringConverter {
            private readonly Saxon.Hej.type.StringConverter phaseOne;
            private readonly Saxon.Hej.type.Converter.DownCastingConverter phaseTwo;
            public StringToNonStringDerivedType(Saxon.Hej.type.StringConverter phaseOne, Saxon.Hej.type.Converter.DownCastingConverter phaseTwo)  {
                this.phaseOne = phaseOne;
                this.phaseTwo = phaseTwo;
            }
            public override Saxon.Hej.type.StringConverter.StringToNonStringDerivedType setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
                return new Saxon.Hej.type.StringConverter.StringToNonStringDerivedType((Saxon.Hej.type.StringConverter)(phaseOne.setNamespaceResolver(resolver)), (Saxon.Hej.type.Converter.DownCastingConverter)(phaseTwo.setNamespaceResolver(resolver)));
            }
            public Saxon.Hej.type.ConversionResult convert(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString @in = input;
                try {
                    @in = phaseTwo.getTargetType().preprocess(@in);
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
                Saxon.Hej.type.ConversionResult temp = phaseOne.convertString(@in);
                if ((temp is Saxon.Hej.type.ValidationFailure)) {
                    return temp;
                }
                return phaseTwo.convert((Saxon.Hej.value.AtomicValue)(temp), @in);
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                try {
                    input = phaseTwo.getTargetType().preprocess(input);
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
                Saxon.Hej.type.ConversionResult temp = phaseOne.convertString(input);
                if ((temp is Saxon.Hej.type.ValidationFailure)) {
                    return temp;
                }
                return phaseTwo.convert((Saxon.Hej.value.AtomicValue)(temp), input);
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                try {
                    input = phaseTwo.getTargetType().preprocess(input);
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
                Saxon.Hej.type.ConversionResult temp = phaseOne.convertString(input);
                if ((temp is Saxon.Hej.type.ValidationFailure)) {
                    return (Saxon.Hej.type.ValidationFailure)(temp);
                }
                return phaseTwo.validate((Saxon.Hej.value.AtomicValue)(temp), input);
            }
        
        }
        public class StringToString : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToString INSTANCE = new Saxon.Hej.type.StringConverter.StringToString();
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                return new Saxon.Hej.value.StringValue(input.getUnicodeStringValue().tidy());
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return new Saxon.Hej.value.StringValue(input.tidy());
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                return null;
            }
            public override bool isAlwaysSuccessful() {
                return true;
            }
        
        }
        public class StringToUntypedAtomic : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToUntypedAtomic INSTANCE = new Saxon.Hej.type.StringConverter.StringToUntypedAtomic();
            public override Saxon.Hej.value.StringValue convert(Saxon.Hej.value.AtomicValue input) {
                return Saxon.Hej.value.StringValue.makeUntypedAtomic(input.getUnicodeStringValue());
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.StringValue.makeUntypedAtomic(input);
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                return null;
            }
            public override bool isAlwaysSuccessful() {
                return true;
            }
        
        }
        public class StringToNormalizedString : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToNormalizedString INSTANCE = new Saxon.Hej.type.StringConverter.StringToNormalizedString();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return new Saxon.Hej.value.StringValue(Saxon.Hej.value.Whitespace.normalizeWhitespace(input).tidy(), Saxon.Hej.type.BuiltInAtomicType.NORMALIZED_STRING);
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                return null;
            }
            public override bool isAlwaysSuccessful() {
                return true;
            }
        
        }
        public class StringToToken : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToToken INSTANCE = new Saxon.Hej.type.StringConverter.StringToToken();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return new Saxon.Hej.value.StringValue(Saxon.Hej.value.Whitespace.collapseWhitespace(input).tidy(), Saxon.Hej.type.BuiltInAtomicType.TOKEN);
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                return null;
            }
            public override bool isAlwaysSuccessful() {
                return true;
            }
        
        }
        public class StringToLanguage : Saxon.Hej.type.StringConverter {
            private readonly static Saxon.Hej.regex.ARegularExpression regex = Saxon.Hej.regex.ARegularExpression.compile("[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*", "");
            public static readonly Saxon.Hej.type.StringConverter.StringToLanguage INSTANCE = new Saxon.Hej.type.StringConverter.StringToLanguage();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(input);
                if (!(regex.matches(trimmed))) {
                    return new Saxon.Hej.type.ValidationFailure((("The value '"+input)+"' is not a valid xs:language"));
                }
                return new Saxon.Hej.value.StringValue(trimmed, Saxon.Hej.type.BuiltInAtomicType.LANGUAGE);
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                if (regex.matches(Saxon.Hej.value.Whitespace.trim(input))) {
                    return null;
                } else {
                    return new Saxon.Hej.type.ValidationFailure((("The value '"+input)+"' is not a valid xs:language"));
                }
            }
        
        }
        public class StringToNCName : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToNCName TO_ID = new Saxon.Hej.type.StringConverter.StringToNCName(Saxon.Hej.type.BuiltInAtomicType.ID);
            public static readonly Saxon.Hej.type.StringConverter.StringToNCName TO_ENTITY = new Saxon.Hej.type.StringConverter.StringToNCName(Saxon.Hej.type.BuiltInAtomicType.ENTITY);
            public static readonly Saxon.Hej.type.StringConverter.StringToNCName TO_NCNAME = new Saxon.Hej.type.StringConverter.StringToNCName(Saxon.Hej.type.BuiltInAtomicType.NCNAME);
            public static readonly Saxon.Hej.type.StringConverter.StringToNCName TO_IDREF = new Saxon.Hej.type.StringConverter.StringToNCName(Saxon.Hej.type.BuiltInAtomicType.IDREF);
            internal Saxon.Hej.type.AtomicType targetType;
            public StringToNCName(Saxon.Hej.type.AtomicType targetType)  {
                this.targetType = targetType;
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(input);
                if (Saxon.Hej.om.NameChecker.isValidNCName(trimmed.codePoints())) {
                    return new Saxon.Hej.value.StringValue(trimmed, targetType);
                } else {
                    return new Saxon.Hej.type.ValidationFailure(((("The value '"+input)+"' is not a valid ")+targetType.getDisplayName()));
                }
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                if (Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.value.Whitespace.trim(input).codePoints())) {
                    return null;
                } else {
                    return new Saxon.Hej.type.ValidationFailure(((("The value '"+input)+"' is not a valid ")+targetType.getDisplayName()));
                }
            }
        
        }
        public class StringToNMTOKEN : Saxon.Hej.type.StringConverter {
            public readonly static Saxon.Hej.type.StringConverter.StringToNMTOKEN INSTANCE = new Saxon.Hej.type.StringConverter.StringToNMTOKEN();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(input);
                if (Saxon.Hej.om.NameChecker.isValidNmtoken(trimmed)) {
                    return new Saxon.Hej.value.StringValue(trimmed, Saxon.Hej.type.BuiltInAtomicType.NMTOKEN);
                } else {
                    return new Saxon.Hej.type.ValidationFailure((("The value '"+input)+"' is not a valid xs:NMTOKEN"));
                }
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                if (Saxon.Hej.om.NameChecker.isValidNmtoken(Saxon.Hej.value.Whitespace.trim(input))) {
                    return null;
                } else {
                    return new Saxon.Hej.type.ValidationFailure((("The value '"+input)+"' is not a valid xs:NMTOKEN"));
                }
            }
        
        }
        public class StringToName : Saxon.Hej.type.StringConverter.StringToNCName {
            public static readonly Saxon.Hej.type.StringConverter.StringToName INSTANCE = new Saxon.Hej.type.StringConverter.StringToName();
            public StringToName() : base (Saxon.Hej.type.BuiltInAtomicType.NAME) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.type.ValidationFailure vf = validate(input);
                if ((vf == null)) {
                    return new Saxon.Hej.value.StringValue(Saxon.Hej.value.Whitespace.trim(input), Saxon.Hej.type.BuiltInAtomicType.NAME);
                } else {
                    return vf;
                }
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString trimmed = Saxon.Hej.value.Whitespace.trim(input);
                if (Saxon.Hej.om.NameChecker.isValidNCName(trimmed.codePoints())) {
                    return null;
                }
                if (Saxon.Hej.om.NameChecker.isValidNCName(trimmed.ToString().Replace(':', '_'))) {
                    return null;
                } else {
                    return new Saxon.Hej.type.ValidationFailure((("The value '"+trimmed)+"' is not a valid xs:Name"));
                }
            }
        
        }
        public class StringToStringSubtype : Saxon.Hej.type.StringConverter {
            internal Saxon.Hej.type.AtomicType targetType;
            internal int whitespaceAction;
            public StringToStringSubtype(Saxon.Hej.lib.ConversionRules rules, Saxon.Hej.type.AtomicType targetType) : base (rules) {
                this.targetType = targetType;
                this.whitespaceAction = targetType.getWhitespaceAction();
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString cs = Saxon.Hej.value.Whitespace.applyWhitespaceNormalization(whitespaceAction, input);
                try {
                    cs = targetType.preprocess(cs);
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
                Saxon.Hej.type.ValidationFailure f = targetType.validate(new Saxon.Hej.value.StringValue(cs), cs, getConversionRules());
                if ((f == null)) {
                    return new Saxon.Hej.value.StringValue(cs, targetType);
                } else {
                    return f;
                }
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString cs = Saxon.Hej.value.Whitespace.applyWhitespaceNormalization(whitespaceAction, input);
                try {
                    cs = targetType.preprocess(cs);
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
                return targetType.validate(new Saxon.Hej.value.StringValue(cs), cs, getConversionRules());
            }
        
        }
        public class StringToDerivedStringSubtype : Saxon.Hej.type.StringConverter {
            internal Saxon.Hej.type.AtomicType targetType;
            internal Saxon.Hej.type.StringConverter builtInValidator;
            internal int whitespaceAction;
            public StringToDerivedStringSubtype(Saxon.Hej.lib.ConversionRules rules, Saxon.Hej.type.AtomicType targetType) : base (rules) {
                this.targetType = targetType;
                this.whitespaceAction = targetType.getWhitespaceAction();
                builtInValidator = ((Saxon.Hej.type.AtomicType)(targetType.getBuiltInBaseType())).getStringConverter(rules);
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.str.UnicodeString cs = Saxon.Hej.value.Whitespace.applyWhitespaceNormalization(whitespaceAction, input);
                Saxon.Hej.type.ValidationFailure f = builtInValidator.validate(cs);
                if ((f != null)) {
                    return f;
                }
                try {
                    cs = targetType.preprocess(cs);
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
                f = targetType.validate(new Saxon.Hej.value.StringValue(cs), cs, getConversionRules());
                if ((f == null)) {
                    return new Saxon.Hej.value.StringValue(cs, targetType);
                } else {
                    return f;
                }
            }
        
        }
        public class StringToFloat : Saxon.Hej.type.StringConverter {
            public StringToFloat(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                try {
                    float flt = (float)(getConversionRules().getStringToDoubleConverter().stringToNumber(input));
                    return new Saxon.Hej.value.FloatValue(flt);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(("Cannot convert string to float: "+input));
                    ve.setErrorCode("FORG0001");
                    return ve;
                }
            }
        
        }
        public class StringToDecimal : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToDecimal INSTANCE = new Saxon.Hej.type.StringConverter.StringToDecimal();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.BigDecimalValue.makeDecimalValue(input.ToString(), true);
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                if (Saxon.Hej.value.BigDecimalValue.castableAsDecimal(input.ToString())) {
                    return null;
                } else {
                    return new Saxon.Hej.type.ValidationFailure(("Cannot convert string to decimal: "+input));
                }
            }
        
        }
        public class StringToInteger : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToInteger INSTANCE = new Saxon.Hej.type.StringConverter.StringToInteger();
            public Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.StringValue input) {
                return Saxon.Hej.value.IntegerValue.stringToInteger(input.ToString());
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.IntegerValue.stringToInteger(input.ToString());
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.IntegerValue.castableAsInteger(input);
            }
        
        }
        public class StringToIntegerSubtype : Saxon.Hej.type.StringConverter {
            internal Saxon.Hej.type.BuiltInAtomicType targetType;
            public StringToIntegerSubtype(Saxon.Hej.type.BuiltInAtomicType targetType)  {
                this.targetType = targetType;
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.type.ConversionResult iv = Saxon.Hej.value.IntegerValue.stringToInteger(input.ToString());
                if ((iv is Saxon.Hej.value.Int64Value)) {
                    bool ok = Saxon.Hej.value.IntegerValue.checkRange(((Saxon.Hej.value.Int64Value)(iv)).longValue(), targetType);
                    if (ok) {
                        return ((Saxon.Hej.value.Int64Value)(iv)).copyAsSubType(targetType);
                    } else {
                        return new Saxon.Hej.type.ValidationFailure(("Integer value is out of range for type "+targetType));
                    }
                } else {
                    if ((iv is Saxon.Hej.value.BigIntegerValue)) {
                        bool ok = Saxon.Hej.value.IntegerValue.checkBigRange(((Saxon.Hej.value.BigIntegerValue)(iv)).asBigInteger(), targetType);
                        if (ok) {
                            ((Saxon.Hej.value.BigIntegerValue)(iv)).setTypeLabel(targetType);
                            return iv;
                        } else {
                            return new Saxon.Hej.type.ValidationFailure(("Integer value is out of range for type "+targetType));
                        }
                    } else {
                        System.Diagnostics.Debug.Assert((iv is Saxon.Hej.type.ValidationFailure));
                        return iv;
                    }
                }
            }
        
        }
        public class StringToDuration : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToDuration INSTANCE = new Saxon.Hej.type.StringConverter.StringToDuration();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.DurationValue.makeDuration(input);
            }
        
        }
        public class StringToDayTimeDuration : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToDayTimeDuration INSTANCE = new Saxon.Hej.type.StringConverter.StringToDayTimeDuration();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.DayTimeDurationValue.makeDayTimeDurationValue(input);
            }
        
        }
        public class StringToYearMonthDuration : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToYearMonthDuration INSTANCE = new Saxon.Hej.type.StringConverter.StringToYearMonthDuration();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.YearMonthDurationValue.makeYearMonthDurationValue(input);
            }
        
        }
        public class StringToDateTime : Saxon.Hej.type.StringConverter {
            public StringToDateTime(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.DateTimeValue.makeDateTimeValue(input, getConversionRules());
            }
        
        }
        public class StringToDateTimeStamp : Saxon.Hej.type.StringConverter {
            public StringToDateTimeStamp(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                Saxon.Hej.type.ConversionResult val = Saxon.Hej.value.DateTimeValue.makeDateTimeValue(input, getConversionRules());
                if ((val is Saxon.Hej.value.DateTimeValue)) {
                    if (!(((Saxon.Hej.value.DateTimeValue)(val)).hasTimezone())) {
                        return new Saxon.Hej.type.ValidationFailure((("Supplied DateTimeStamp value "+input)+" has no time zone"));
                    } else {
                        ((Saxon.Hej.value.DateTimeValue)(val)).setTypeLabel(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME_STAMP);
                    }
                }
                return val;
            }
        
        }
        public class StringToDate : Saxon.Hej.type.StringConverter {
            public StringToDate(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.DateValue.makeDateValue(input, getConversionRules());
            }
        
        }
        public class StringToGMonth : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToGMonth INSTANCE = new Saxon.Hej.type.StringConverter.StringToGMonth();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.GMonthValue.makeGMonthValue(input);
            }
        
        }
        public class StringToGYearMonth : Saxon.Hej.type.StringConverter {
            public StringToGYearMonth(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.GYearMonthValue.makeGYearMonthValue(input, getConversionRules());
            }
        
        }
        public class StringToGYear : Saxon.Hej.type.StringConverter {
            public StringToGYear(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.GYearValue.makeGYearValue(input, getConversionRules());
            }
        
        }
        public class StringToGMonthDay : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToGMonthDay INSTANCE = new Saxon.Hej.type.StringConverter.StringToGMonthDay();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.GMonthDayValue.makeGMonthDayValue(input);
            }
        
        }
        public class StringToGDay : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToGDay INSTANCE = new Saxon.Hej.type.StringConverter.StringToGDay();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.GDayValue.makeGDayValue(input);
            }
        
        }
        public class StringToTime : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToTime INSTANCE = new Saxon.Hej.type.StringConverter.StringToTime();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.TimeValue.makeTimeValue(input);
            }
        
        }
        public class StringToBoolean : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToBoolean INSTANCE = new Saxon.Hej.type.StringConverter.StringToBoolean();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                return Saxon.Hej.value.BooleanValue.fromString(input);
            }
        
        }
        public class StringToHexBinary : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToHexBinary INSTANCE = new Saxon.Hej.type.StringConverter.StringToHexBinary();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                try {
                    return new Saxon.Hej.value.HexBinaryValue(input);
                } catch (Saxon.Hej.trans.XPathException e) {
                    return Saxon.Hej.type.ValidationFailure.fromException(e);
                }
            }
        
        }
        public class StringToBase64Binary : Saxon.Hej.type.StringConverter {
            public static readonly Saxon.Hej.type.StringConverter.StringToBase64Binary INSTANCE = new Saxon.Hej.type.StringConverter.StringToBase64Binary();
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                try {
                    return new Saxon.Hej.value.Base64BinaryValue(input);
                } catch (Saxon.Hej.trans.XPathException e) {
                    return Saxon.Hej.type.ValidationFailure.fromException(e);
                }
            }
        
        }
        public class StringToQName : Saxon.Hej.type.StringConverter {
            private Saxon.Hej.om.NamespaceResolver nsResolver;
            public StringToQName(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.StringConverter.StringToQName setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
                Saxon.Hej.type.StringConverter.StringToQName c = new Saxon.Hej.type.StringConverter.StringToQName(getConversionRules());
                c.nsResolver = resolver;
                return c;
            }
            public override Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
                return nsResolver;
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                if ((nsResolver == null)) {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot validate a QName without a namespace resolver");
                }
                try {
                    string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(Saxon.Hej.value.Whitespace.trim(input.ToString()));
                    string uri = nsResolver.getURIForPrefix(parts[0], true);
                    if ((uri == null)) {
                        Saxon.Hej.type.ValidationFailure failure = new Saxon.Hej.type.ValidationFailure((("Namespace prefix "+Saxon.Hej.trans.Err.wrap(parts[0]))+" has not been declared"));
                        failure.setErrorCode("FONS0004");
                        return failure;
                    }
                    return new Saxon.Hej.value.QNameValue(parts[0], uri, parts[1], Saxon.Hej.type.BuiltInAtomicType.QNAME, false);
                } catch (Saxon.Hej.om.QNameException err) {
                    return new Saxon.Hej.type.ValidationFailure(("Invalid lexical QName "+Saxon.Hej.trans.Err.wrap(input)));
                } catch (Saxon.Hej.trans.XPathException err) {
                    return new Saxon.Hej.type.ValidationFailure(err.Message);
                }
            }
        
        }
        public class StringToNotation : Saxon.Hej.type.StringConverter {
            private Saxon.Hej.om.NamespaceResolver nsResolver;
            public StringToNotation(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.StringConverter.StringToNotation setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
                Saxon.Hej.type.StringConverter.StringToNotation c = new Saxon.Hej.type.StringConverter.StringToNotation(getConversionRules());
                c.nsResolver = resolver;
                return c;
            }
            public override Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
                return nsResolver;
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                if ((getNamespaceResolver() == null)) {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot validate a NOTATION without a namespace resolver");
                }
                try {
                    string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(Saxon.Hej.value.Whitespace.trim(input.ToString()));
                    string uri = getNamespaceResolver().getURIForPrefix(parts[0], true);
                    if ((uri == null)) {
                        return new Saxon.Hej.type.ValidationFailure((("Namespace prefix "+Saxon.Hej.trans.Err.wrap(parts[0]))+" has not been declared"));
                    }
                    if (!(getConversionRules().isDeclaredNotation(uri, parts[1]))) {
                        return new Saxon.Hej.type.ValidationFailure((((("Notation {"+uri)+"}")+parts[1])+" is not declared in the schema"));
                    }
                    return new Saxon.Hej.value.NotationValue(parts[0], uri, parts[1], false);
                } catch (Saxon.Hej.om.QNameException err) {
                    return new Saxon.Hej.type.ValidationFailure(("Invalid lexical QName "+Saxon.Hej.trans.Err.wrap(input)));
                } catch (Saxon.Hej.trans.XPathException err) {
                    return new Saxon.Hej.type.ValidationFailure(err.Message);
                }
            }
        
        }
        public class StringToAnyURI : Saxon.Hej.type.StringConverter {
            public StringToAnyURI(Saxon.Hej.lib.ConversionRules rules) : base (rules) {
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                if (getConversionRules().isValidURI(input.ToString())) {
                    return new Saxon.Hej.value.AnyURIValue(input);
                } else {
                    return new Saxon.Hej.type.ValidationFailure(("Invalid URI: "+input));
                }
            }
            public override Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.str.UnicodeString input) {
                if (getConversionRules().isValidURI(input.ToString())) {
                    return null;
                } else {
                    return new Saxon.Hej.type.ValidationFailure(("Invalid URI: "+input));
                }
            }
        
        }
        public class StringToUnionConverter : Saxon.Hej.type.StringConverter {
            internal Saxon.Hej.type.PlainType targetType;
            internal Saxon.Hej.lib.ConversionRules rules;
            public StringToUnionConverter(Saxon.Hej.type.PlainType targetType, Saxon.Hej.lib.ConversionRules rules)  {
                if (!(targetType.isPlainType())) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException();
                }
                if (targetType.isNamespaceSensitive()) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException();
                }
                this.targetType = targetType;
                this.rules = rules;
            }
            public override Saxon.Hej.type.ConversionResult convertString(Saxon.Hej.str.UnicodeString input) {
                try {
                    return (Saxon.Hej.value.AtomicValue)(((/*1430*/Saxon.Hej.value.AtomicValue)((Saxon.Hej.type.UnionType)(targetType)).getTypedValue(input, null, rules).head()));
                } catch (Saxon.Hej.type.ValidationException err) {
                    return err.getValidationFailure();
                }
            }
        
        }
    
    }
}