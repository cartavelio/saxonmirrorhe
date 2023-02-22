using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ConversionRules {
        private Saxon.Hej.type.StringToDouble stringToDouble = Saxon.Hej.value.StringToDouble11.getInstance();
        private Saxon.Hej.om.NotationSet notationSet;
        private Saxon.Hej.lib.URIChecker uriChecker;
        private bool allowYearZero = true;
        private Saxon.Hej.type.TypeHierarchy typeHierarchy;
        private readonly Saxon.Hej.expr.sort.LFUCache<int,Saxon.Hej.type.Converter> converterCache = new Saxon.Hej.expr.sort.LFUCache<int, Saxon.Hej.type.Converter>(100, true);
        public readonly static Saxon.Hej.lib.ConversionRules DEFAULT = new Saxon.Hej.lib.ConversionRules();
        public ConversionRules()  {
        }
        public Saxon.Hej.lib.ConversionRules copy() {
            Saxon.Hej.lib.ConversionRules cr = new Saxon.Hej.lib.ConversionRules();
            copyTo(cr);
            return cr;
        }
        public void copyTo(Saxon.Hej.lib.ConversionRules cr) {
            cr.stringToDouble = stringToDouble;
            cr.notationSet = notationSet;
            cr.uriChecker = uriChecker;
            cr.allowYearZero = allowYearZero;
            cr.typeHierarchy = typeHierarchy;
            cr.converterCache.clear();
        }
        public void setTypeHierarchy(Saxon.Hej.type.TypeHierarchy typeHierarchy) {
            this.typeHierarchy = typeHierarchy;
        }
        public void setStringToDoubleConverter(Saxon.Hej.type.StringToDouble converter) {
            this.stringToDouble = converter;
        }
        public Saxon.Hej.type.StringToDouble getStringToDoubleConverter() {
            return stringToDouble;
        }
        public void setNotationSet(Saxon.Hej.om.NotationSet notations) {
            this.notationSet = notations;
        }
        public bool isDeclaredNotation(Saxon.Hej.om.NamespaceUri uri, string local) {
            if ((notationSet == null)) {
                return true;
            } else {
                return notationSet.isDeclaredNotation(uri, local);
            }
        }
        public void setURIChecker(Saxon.Hej.lib.URIChecker checker) {
            this.uriChecker = checker;
        }
        public bool isValidURI(string str) {
            return ((uriChecker == null) || uriChecker.isValidURI(str));
        }
        public void setAllowYearZero(bool allowed) {
            allowYearZero = allowed;
        }
        public bool isAllowYearZero() {
            return allowYearZero;
        }
        public Saxon.Hej.type.Converter getConverter(Saxon.Hej.type.AtomicType source, Saxon.Hej.type.AtomicType target) {
            if ((source == target)) {
                return Saxon.Hej.type.StringConverter.IdentityConverter.INSTANCE;
            } else {
                if (((source == Saxon.Hej.type.BuiltInAtomicType.STRING) || (source == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                    return target.getStringConverter(this);
                } else {
                    if ((target == Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                        return Saxon.Hej.type.Converter.ToStringConverter.INSTANCE;
                    } else {
                        if ((target == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                            return Saxon.Hej.type.Converter.ToUntypedAtomicConverter.INSTANCE;
                        }
                    }
                }
            }
            int key = (((source.getPrimitiveType()<<20))|target.getFingerprint());
            Saxon.Hej.type.Converter converter = converterCache.get(key);
            if ((converter == null)) {
                converter = makeConverter(source, target);
                if ((converter != null)) {
                    converterCache.put(key, converter);
                } else {
                    return null;
                }
            }
            return converter;
        }
        private Saxon.Hej.type.Converter makeConverter(Saxon.Hej.type.AtomicType sourceType, Saxon.Hej.type.AtomicType targetType) {
            if ((sourceType == targetType)) {
                return Saxon.Hej.type.StringConverter.IdentityConverter.INSTANCE;
            }
            int tt = targetType.getFingerprint();
            int tp = targetType.getPrimitiveType();
            int st = sourceType.getPrimitiveType();
            if (((((st == Saxon.Hej.om.StandardNames.XS_STRING) || (st == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC))) && (((tp == Saxon.Hej.om.StandardNames.XS_STRING) || (tp == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC))))) {
                return makeStringConverter(targetType);
            }
            if (!(targetType.isPrimitiveType())) {
                Saxon.Hej.type.AtomicType primTarget = (Saxon.Hej.type.AtomicType)(targetType.getPrimitiveItemType());
                if ((sourceType == primTarget)) {
                    return new Saxon.Hej.type.Converter.DownCastingConverter(targetType, this);
                } else {
                    if (((st == Saxon.Hej.om.StandardNames.XS_STRING) || (st == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC))) {
                        return makeStringConverter(targetType);
                    } else {
                        Saxon.Hej.type.Converter stageOne = makeConverter(sourceType, primTarget);
                        if ((stageOne == null)) {
                            return null;
                        }
                        Saxon.Hej.type.Converter stageTwo = new Saxon.Hej.type.Converter.DownCastingConverter(targetType, this);
                        return new Saxon.Hej.type.Converter.TwoPhaseConverter(stageOne, stageTwo);
                    }
                }
            }
            if ((st == tt)) {
                if (((typeHierarchy != null) && typeHierarchy.isSubType(sourceType, targetType))) {
                    return new Saxon.Hej.type.Converter.UpCastingConverter(targetType);
                }
                Saxon.Hej.type.Converter upcast = new Saxon.Hej.type.Converter.UpCastingConverter((Saxon.Hej.type.AtomicType)(sourceType.getPrimitiveItemType()));
                Saxon.Hej.type.Converter downcast = new Saxon.Hej.type.Converter.DownCastingConverter(targetType, this);
                return new Saxon.Hej.type.Converter.TwoPhaseConverter(upcast, downcast);
            }
            switch (tt) {
              case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                return Saxon.Hej.type.Converter.ToUntypedAtomicConverter.INSTANCE;
              case Saxon.Hej.om.StandardNames.XS_STRING:
                return Saxon.Hej.type.Converter.ToStringConverter.INSTANCE;
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToFloat(this);
                  case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                  case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                  case Saxon.Hej.om.StandardNames.XS_INTEGER:
                  case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                    return Saxon.Hej.type.Converter.NumericToFloat.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                    return Saxon.Hej.type.Converter.BooleanToFloat.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
              case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return stringToDouble;
                  case Saxon.Hej.om.StandardNames.XS_FLOAT:
                  case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                  case Saxon.Hej.om.StandardNames.XS_INTEGER:
                  case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                    return Saxon.Hej.type.Converter.NumericToDouble.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                    return Saxon.Hej.type.Converter.BooleanToDouble.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToDecimal.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_FLOAT:
                    return Saxon.Hej.type.Converter.FloatToDecimal.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                    return Saxon.Hej.type.Converter.DoubleToDecimal.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_INTEGER:
                    return Saxon.Hej.type.Converter.IntegerToDecimal.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                    return Saxon.Hej.type.Converter.NumericToDecimal.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                    return Saxon.Hej.type.Converter.BooleanToDecimal.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToInteger.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_FLOAT:
                    return Saxon.Hej.type.Converter.FloatToInteger.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                    return Saxon.Hej.type.Converter.DoubleToInteger.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                    return Saxon.Hej.type.Converter.DecimalToInteger.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                    return Saxon.Hej.type.Converter.NumericToInteger.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                    return Saxon.Hej.type.Converter.BooleanToInteger.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_DURATION:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToDuration.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                    return new Saxon.Hej.type.Converter.UpCastingConverter(Saxon.Hej.type.BuiltInAtomicType.DURATION);
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToYearMonthDuration.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                    return Saxon.Hej.type.Converter.DurationToYearMonthDuration.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToDayTimeDuration.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DURATION:
                  case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                    return Saxon.Hej.type.Converter.DurationToDayTimeDuration.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToDateTime(this);
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                    return Saxon.Hej.type.Converter.DateToDateTime.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_TIME:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToTime.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToTime.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_DATE:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToDate(this);
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToDate.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToGYearMonth(this);
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                    return Saxon.Hej.type.Converter.TwoPhaseConverter.makeTwoPhaseConverter(Saxon.Hej.type.BuiltInAtomicType.DATE, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH, this);
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToGYearMonth.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_G_YEAR:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToGYear(this);
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                    return Saxon.Hej.type.Converter.TwoPhaseConverter.makeTwoPhaseConverter(Saxon.Hej.type.BuiltInAtomicType.DATE, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, Saxon.Hej.type.BuiltInAtomicType.G_YEAR, this);
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToGYear.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToGMonthDay.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                    return Saxon.Hej.type.Converter.TwoPhaseConverter.makeTwoPhaseConverter(Saxon.Hej.type.BuiltInAtomicType.DATE, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY, this);
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToGMonthDay.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_G_DAY:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToGDay.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                    return Saxon.Hej.type.Converter.TwoPhaseConverter.makeTwoPhaseConverter(Saxon.Hej.type.BuiltInAtomicType.DATE, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, Saxon.Hej.type.BuiltInAtomicType.G_DAY, this);
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToGDay.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_G_MONTH:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToGMonth.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_DATE:
                    return Saxon.Hej.type.Converter.TwoPhaseConverter.makeTwoPhaseConverter(Saxon.Hej.type.BuiltInAtomicType.DATE, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, Saxon.Hej.type.BuiltInAtomicType.G_MONTH, this);
                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                    return Saxon.Hej.type.Converter.DateTimeToGMonth.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToBoolean.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_FLOAT:
                  case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                  case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                  case Saxon.Hej.om.StandardNames.XS_INTEGER:
                  case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                    return Saxon.Hej.type.Converter.NumericToBoolean.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToBase64Binary.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
                    return Saxon.Hej.type.Converter.HexBinaryToBase64Binary.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return Saxon.Hej.type.StringConverter.StringToHexBinary.INSTANCE;
                  case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
                    return Saxon.Hej.type.Converter.Base64BinaryToHexBinary.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToAnyURI(this);
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_QNAME:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToQName(this);
                  case Saxon.Hej.om.StandardNames.XS_NOTATION:
                    return Saxon.Hej.type.Converter.NotationToQName.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
                switch (st) {
                  case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                  case Saxon.Hej.om.StandardNames.XS_STRING:
                    return new Saxon.Hej.type.StringConverter.StringToNotation(this);
                  case Saxon.Hej.om.StandardNames.XS_QNAME:
                    return Saxon.Hej.type.Converter.QNameToNotation.INSTANCE;
                  default:
                    return null;
                }
              case Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE:
                return Saxon.Hej.type.StringConverter.IdentityConverter.INSTANCE;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown primitive type "+tt));
            }
        }
        public Saxon.Hej.type.StringConverter makeStringConverter(Saxon.Hej.type.AtomicType targetType) {
            int tt = targetType.getPrimitiveType();
            if (targetType.isBuiltInType()) {
                if ((tt == Saxon.Hej.om.StandardNames.XS_STRING)) {
                    switch (targetType.getFingerprint()) {
                      case Saxon.Hej.om.StandardNames.XS_STRING:
                        return Saxon.Hej.type.StringConverter.StringToString.INSTANCE;
                      case Saxon.Hej.om.StandardNames.XS_NORMALIZED_STRING:
                        return Saxon.Hej.type.StringConverter.StringToNormalizedString.INSTANCE;
                      case Saxon.Hej.om.StandardNames.XS_TOKEN:
                        return Saxon.Hej.type.StringConverter.StringToToken.INSTANCE;
                      case Saxon.Hej.om.StandardNames.XS_LANGUAGE:
                        return Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE;
                      case Saxon.Hej.om.StandardNames.XS_NAME:
                        return Saxon.Hej.type.StringConverter.StringToName.INSTANCE;
                      case Saxon.Hej.om.StandardNames.XS_NCNAME:
                        return Saxon.Hej.type.StringConverter.StringToNCName.TO_NCNAME;
                      case Saxon.Hej.om.StandardNames.XS_ID:
                        return Saxon.Hej.type.StringConverter.StringToNCName.TO_ID;
                      case Saxon.Hej.om.StandardNames.XS_IDREF:
                        return Saxon.Hej.type.StringConverter.StringToNCName.TO_IDREF;
                      case Saxon.Hej.om.StandardNames.XS_ENTITY:
                        return Saxon.Hej.type.StringConverter.StringToNCName.TO_ENTITY;
                      case Saxon.Hej.om.StandardNames.XS_NMTOKEN:
                        return Saxon.Hej.type.StringConverter.StringToNMTOKEN.INSTANCE;
                      default:
                        throw new Saxon.Ejava.lang.AssertionError("Unknown built-in subtype of xs:string");
                    }
                } else {
                    if ((tt == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                        return Saxon.Hej.type.StringConverter.StringToUntypedAtomic.INSTANCE;
                    } else {
                        if (targetType.isPrimitiveType()) {
                            Saxon.Hej.type.Converter converter = getConverter(Saxon.Hej.type.BuiltInAtomicType.STRING, targetType);
                            System.Diagnostics.Debug.Assert((converter != null));
                            return (Saxon.Hej.type.StringConverter)(converter);
                        } else {
                            if ((tt == Saxon.Hej.om.StandardNames.XS_INTEGER)) {
                                return new Saxon.Hej.type.StringConverter.StringToIntegerSubtype((Saxon.Hej.type.BuiltInAtomicType)(targetType));
                            } else {
                                switch (targetType.getFingerprint()) {
                                  case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                                    return Saxon.Hej.type.StringConverter.StringToDayTimeDuration.INSTANCE;
                                  case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                                    return Saxon.Hej.type.StringConverter.StringToYearMonthDuration.INSTANCE;
                                  case Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP:
                                    Saxon.Hej.type.StringConverter first = new Saxon.Hej.type.StringConverter.StringToDateTime(this);
                                    Saxon.Hej.type.Converter.DownCastingConverter second = new Saxon.Hej.type.Converter.DownCastingConverter(targetType, this);
                                    return new Saxon.Hej.type.StringConverter.StringToNonStringDerivedType(first, second);
                                  default:
                                    throw new Saxon.Ejava.lang.AssertionError(("Unknown built in type "+targetType));
                                }
                            }
                        }
                    }
                }
            } else {
                if ((tt == Saxon.Hej.om.StandardNames.XS_STRING)) {
                    if ((targetType.getBuiltInBaseType() == Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                        return new Saxon.Hej.type.StringConverter.StringToStringSubtype(this, targetType);
                    } else {
                        return new Saxon.Hej.type.StringConverter.StringToDerivedStringSubtype(this, targetType);
                    }
                } else {
                    Saxon.Hej.type.StringConverter first = ((Saxon.Hej.type.AtomicType)(targetType.getPrimitiveItemType())).getStringConverter(this);
                    Saxon.Hej.type.Converter.DownCastingConverter second = new Saxon.Hej.type.Converter.DownCastingConverter(targetType, this);
                    return new Saxon.Hej.type.StringConverter.StringToNonStringDerivedType(first, second);
                }
            }
        }
    
    }
}