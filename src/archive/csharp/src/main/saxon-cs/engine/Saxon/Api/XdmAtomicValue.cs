using System;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Hej.om;
using Saxon.Hej.type;
using Saxon.Hej.value;
using Singulink.Numerics;

namespace Saxon.Api {
    /// <summary inherits="XdmItem">
    /// The class <c>XdmAtomicValue</c> represents an item in an XDM sequence
    /// that is an atomic value. The value may belong to any of the 19 primitive types
    /// defined in XML Schema, or to a type derived from these primitive types, or to 
    /// the XDM-specific type <c>xs:untypedAtomic</c>.
    /// </summary>
    /// <remarks>
    /// Note that there is no guarantee that every <c>XdmValue</c> comprising a single
    /// atomic value will be an instance of this class. To force this, use the <c>Simplify</c>
    /// property of the <c>XdmValue</c>.
    /// </remarks>
    [Serializable]
    public class XdmAtomicValue : XdmItem {
        // Internal constructor

        internal XdmAtomicValue() {
        }

        /// <summary>
        /// Construct an XdmNode as a wrapper around an instance of the internal class <c>Saxon.Hej.value.AtomicValue</c>
        /// </summary>
        /// <remarks>
        /// Applications using the class <c>Saxon.Hej.value.AtomicValue</c> should be aware that it is subject to occasional
        /// changes from one release to the next. For documentation on this class, see the Saxon-EE Javadoc documentation
        /// for the very similar class <c>net.sf.saxon.value.AtomicValue</c>.
        /// </remarks>
        /// <param name="atomicValue">The object to be wrapped</param>
		public XdmAtomicValue(Saxon.Hej.value.AtomicValue atomicValue) : base(atomicValue) {
            
        }

        /// <summary>
        /// Determine whether the item is an atomic value
        /// </summary>
        /// <returns>
        /// True (the item is an atomic value).
        /// </returns>
        public override bool IsAtomic() {
            return true;
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:string</c>
        /// </summary>
        /// <param name="str">The string value</param>
        public XdmAtomicValue(string str) : base(new StringValue(str)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:integer</c> from a supplied <c>long</c>
        /// </summary>
        /// <param name="i">The integer value</param>
        public XdmAtomicValue(long i) : base(Int64Value.makeDerived(i, BuiltInAtomicType.LONG)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:integer</c> from a supplied <c>long</c>
        /// </summary>
        /// <param name="i">The integer value</param>
        public XdmAtomicValue(int i) : base(Int64Value.makeDerived(i, BuiltInAtomicType.INT)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:integer</c> from a supplied <c>byte</c>
        /// </summary>
        /// <param name="i">The integer value, in the range -128 to +127</param>
        public XdmAtomicValue(byte i) : base(Int64Value.makeDerived(i, BuiltInAtomicType.BYTE)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:decimal</c>
        /// </summary>
        /// <param name="d">The decimal value</param>
        public XdmAtomicValue(decimal d) : base (
            new BigDecimalValue(BigDecimal.Parse(d.ToString(System.Globalization.CultureInfo.InvariantCulture)))) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:float</c>
        /// </summary>
        /// <param name="f">The float value</param>        
        public XdmAtomicValue(float f) : base(new FloatValue(f)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:dateTime</c> from a .NET DateTime object
        /// Here we probe the object for timezone offset information to create the resulting value. 
        /// </summary>
        /// <param name="dt">The DateTime object value</param>
        public XdmAtomicValue(DateTime dt) : base(_init(dt)) {
        }
        
        private static DateTimeValue _init(DateTime dt) {
            switch (dt) {
                case {Kind: DateTimeKind.Local}: {
                    int offsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;
                    return new Saxon.Hej.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month),
                        Convert.ToByte(dt.Day), Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute),
                        Convert.ToByte(dt.Second), Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)),
                        offsetMinutes);
                }
                case {Kind: DateTimeKind.Utc}: {
                    return new Saxon.Hej.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month),
                        Convert.ToByte(dt.Day), Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute),
                        Convert.ToByte(dt.Second), Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)),
                        0);
                }
                case {Kind: DateTimeKind.Unspecified}: {
                    return new Saxon.Hej.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month),
                        Convert.ToByte(dt.Day), Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute),
                        Convert.ToByte(dt.Second), Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)),
                        Saxon.Hej.value.CalendarValue.NO_TIMEZONE);
                }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:dateTime</c> with a specific timezone offset from a DateTimeOffset object.
        /// </summary>
        /// <param name="offset">The DateTimeOffset value</param>
        public XdmAtomicValue(DateTimeOffset offset) : base (_initOffset(offset)) {
        }

        private static DateTimeValue _initOffset(DateTimeOffset offset) {
            DateTime dt = offset.DateTime;
            return new Saxon.Hej.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month), Convert.ToByte(dt.Day),
                Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute), Convert.ToByte(dt.Second),
                Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)), offset.Offset.Minutes);

        }


        /// <summary>
        /// Construct an atomic value of type <c>xs:double</c>
        /// </summary>
        /// <param name="d">The double value</param>
        public XdmAtomicValue(double d) : base (new DoubleValue(d)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:boolean</c>
        /// </summary>
        /// <param name="b">The boolean value</param>
        public XdmAtomicValue(bool b) : base (BooleanValue.get(b)) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:anyURI</c>
        /// </summary>
        /// <param name="u">The uri value</param>
        public XdmAtomicValue(Uri u) : base (new AnyURIValue(u.ToString())) {
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:QName</c>
        /// </summary>
        /// <param name="q">The QName value</param>                
        public XdmAtomicValue(QName q) : base(new QNameValue( q.Prefix, q.Uri, q.LocalName)) {
        }

        /// <summary>
        /// Construct an atomic value of a given type
        /// </summary>
        /// <param name="lexicalForm">The string representation of the value (any value that is acceptable
        /// in the lexical space, as defined by XML Schema Part 2). Whitespace normalization as defined by
        /// the target type will be applied to the value.</param>
        /// <param name="type">The type given as an <c>XdmAtomicType</c></param>
        public XdmAtomicValue(string lexicalForm, XdmAtomicType type) : base(_initGeneric(lexicalForm, type)) {
        }
        
        private static AtomicValue _initGeneric(string lexicalForm, XdmAtomicType type) {

            Saxon.Hej.type.ItemType it = type.Unwrap();
            if (!it.isPlainType()) {
                throw new SaxonApiException("Requested type is not atomic");
            }

            if (((AtomicType) it).isAbstract()) {
                throw new SaxonApiException("Requested type is not namespace-sensitive");
            }

            try {
                Saxon.Hej.str.UnicodeString us = Saxon.Hej.str.StringView.of(lexicalForm).tidy();
                Saxon.Hej.type.StringConverter Converter =
                    ((AtomicType) it).getStringConverter(ConversionRules.DEFAULT);
                return Converter.convertString(us).asAtomic();
            }
            catch (Exception ex) {
                throw new SaxonApiException(ex);
            }
        }
        

        /// <summary>
        /// Create an atomic value of a type appropriate to the supplied value. 
        /// </summary>
        /// <remarks>
        /// The supplied value must be one of the following:
        /// <list>
        /// <item>An instance of the Saxon Java class <c>Saxon.Hej.value.AtomicValue</c></item>
        /// <item>A <c>Boolean</c> - returns an instance of <c>xs:boolean</c></item>
        /// <item>A (signed) <c>int</c>, <c>long</c>, <c>short</c>, or <c>byte</c> - returns an instance of <c>xs:integer</c></item>
        /// <item>A <c>Char</c> - TODO ???????</item>
        /// <item>A <c>String</c> - returns an instance of <c>xs:string</c></item>
        /// <item>A <c>Double</c> - returns an instance of <c>xs:double</c></item>
        /// <item>A <c>Float</c> - returns an instance of <c>xs:float</c></item>
        /// <item>A <c>decimal</c> - returns an instance of <c>xs:decimal</c></item>
        /// <item>A <c>URI</c> - returns an instance of <c>xs:anyURI</c></item>
        /// <item>A <c>QName</c> - returns an instance of <c>xs:QName</c></item>
        /// </list>
        /// </remarks>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The converted value</returns>
        /// <exception cref="ArgumentException">If the supplied value is not one of the recognized types</exception>
        public static XdmAtomicValue MakeAtomicValue(object value) {
            return value switch {
                XdmAtomicValue xdm => xdm,
                AtomicValue atomicValue => (XdmAtomicValue) XdmValue.Wrap(atomicValue),
                bool boolValue => new XdmAtomicValue(boolValue),
                int intValue => new XdmAtomicValue(intValue),
                long longValue => new XdmAtomicValue(longValue),
                short shortValue => new XdmAtomicValue(shortValue),
                char charValue => new XdmAtomicValue((long) charValue),
                byte byteValue => new XdmAtomicValue(byteValue),
                string stringValue => new XdmAtomicValue(stringValue),
                double doubleValue => new XdmAtomicValue(doubleValue),
                float floatValue => new XdmAtomicValue(floatValue),
                decimal decimalValue => new XdmAtomicValue(decimalValue),
                Uri uriValue => new XdmAtomicValue(uriValue),
                QName qNameValue => new XdmAtomicValue(qNameValue),
                _ => throw new ArgumentException(value.ToString())
            };
        }


        /// <summary>
        /// Get the value converted to a boolean using the XPath casting rules.
        /// </summary>
        /// <returns>The result of converting to a boolean (Note: this is not the same as the
        /// effective boolean value).</returns>
        /// <exception cref="InvalidCastException">If the value cannot be cast to an xs:boolean</exception> 
        public bool AsBoolean() {
            AtomicValue av = (AtomicValue) value;
            switch (av) {
                case BooleanValue booleanValue:
                    return booleanValue.getBooleanValue();
                case NumericValue numericValue:
                    return !numericValue.isNaN() && numericValue.signum() != 0;
                case StringValue stringValue: {
                    string s = stringValue.getStringValue().Trim();
                    return "1".Equals(s) || "true".Equals(s);
                }
                default:
                    throw new InvalidCastException("Cannot cast " + TypeName + " to boolean");
            }
        }


        /// <summary>
        /// Get the value converted to a long using the XPath casting rules.
        /// </summary>
        /// <returns>The result of converting to a long</returns>
        /// <exception cref="InvalidCastException">If the value cannot be cast to an xs:long</exception> 
        public long AsLong() {
            AtomicValue av = (AtomicValue) value;
            switch (av) {
                case BooleanValue booleanValue:
                    return booleanValue.getBooleanValue() ? 0L : 1L;
                case NumericValue numericValue:
                    try {
                        return numericValue.longValue();
                    }
                    catch (System.Exception) {
                        throw new InvalidCastException("Cannot cast " + TypeName + " to an integer");
                    }

                case StringValue stringValue: {
                    StringToDouble converter = StringToDouble.getInstance();
                    return (long) converter.stringToNumber(av.getUnicodeStringValue());
                }
                default:
                    throw new InvalidCastException("Cannot cast " + TypeName + " to an integer");
            }
        }


        /// <summary>
        /// Get the value converted to a double using the XPath casting rules.
        /// </summary>
        /// <remarks>If the value is a string, the XSD 1.1 rules are used, which means that the string
        /// "+INF" is recognised.</remarks>
        /// <returns>The result of converting to a double</returns>
        /// <exception cref="InvalidCastException">If the value cannot be cast to an xs:double</exception> 
        public double AsDouble() {
            AtomicValue av = (AtomicValue) this.value;
            switch (av) {
                case BooleanValue booleanValue:
                    return booleanValue.getBooleanValue() ? 0.0 : 1.0;
                case NumericValue numericValue:
                    return numericValue.getDoubleValue();
                case StringValue stringValue:
                    try {
                        return StringToDouble11.getInstance().stringToNumber(av.getUnicodeStringValue());
                    }
                    catch (Exception e) {
                        throw new InvalidCastException(e.Message);
                    }

                default:
                    throw new InvalidCastException("Cannot cast " + TypeName + " to a double");
            }
        }


        /// <summary>
        /// Get the value converted to a decimal using the XPath casting rules.
        /// </summary>
        /// <returns>The result of converting to a decimal</returns>
        /// <exception cref="InvalidCastException">If the value cannot be cast to an xs:decimal, or converted
        /// to a .NET primitive decimal.</exception> 
        public decimal AsDecimal() {
            AtomicValue av = (AtomicValue) this.value;
            switch (av) {
                case BooleanValue booleanValue:
                    return booleanValue.getBooleanValue() ? 0 : 1;
                case NumericValue numericValue:
                    try {
                        return Convert.ToDecimal(numericValue.getDecimalValue().ToString());
                    }
                    catch (Exception) {
                        throw new InvalidCastException("Cannot cast " + TypeName + " to a decimal");
                    }

                case StringValue stringValue:
                    return Convert.ToDecimal(av.getUnicodeStringValue().ToString());
                default:
                    throw new InvalidCastException("Cannot cast " + TypeName + " to a decimal");
            }
        }
        
        /// <summary>
        /// Get the value converted to a QName. The value must be of type xs:QName or xs:NOTATION.
        /// </summary>
        /// <returns>The result of converting to a QName</returns>
        /// <exception cref="InvalidCastException">If the value cannot be cast to an xs:QName</exception> 
        public QName AsQName() {
            AtomicValue av = (AtomicValue) this.value;
            if (av is QualifiedNameValue q)
                return new QName(q.qName);
            else
                throw new InvalidCastException("Cannot cast " + TypeName + " to a QName");
        }


        /// <summary>
        /// Convert the atomic value to a string
        /// </summary>
        /// <returns>The value converted to a string, according to the rules
        /// of the XPath cast expression. The result is the same as the <c>StringValue</c> property.</returns>        
        public override string ToString() {
            return ((AtomicValue) value).getStringValue();
        }

        /// <summary>
        /// Compare two atomic values for equality
        /// </summary>
        /// <param name="other">The object to be compared</param>
        /// <returns>The result of the equality comparison, using the rules of the
        /// <c>op:is-same-key()</c> comparison used for comparing key values in maps.</returns>
        public override bool Equals(object other) {
            if (other is XdmAtomicValue otherValue) {
                return ((AtomicValue) value).asMapKey()
                    .Equals(((AtomicValue) otherValue.value).asMapKey());
            }

            return false;
        }

        /// <summary>
        /// Get a hash code to support equality comparison
        /// </summary>
        /// <returns>A suitable hash code</returns>
        public override int GetHashCode() {
            return ((AtomicValue) value).asMapKey().GetHashCode();
        }

        /// <summary>
        /// Get the name of the value's XDM type
        /// </summary>
        /// <returns>The type of the value, as a QName.</returns>
        public QName TypeName => new QName(((AtomicValue) value).getItemType().getStructuredQName());

        /// <summary>
        /// Get the name of the primitive type of the value
        /// </summary>
        /// <returns>The primitive type of the value, as a QName. This will be the name of
        /// one of the primitive types defined in XML Schema Part 2, or the XPath-defined
        /// type <c>xs:untypedAtomic</c>. For the purposes of this method, <c>xs:integer</c> is considered
        /// to be a primitive type.
        /// </returns>
        public QName PrimitiveTypeName => 
            new QName(StandardNames.getStructuredQName(((AtomicValue) value).getItemType().getPrimitiveType()));

        /// <summary>Get the value as a CLI object of the nearest equivalent type.</summary>
        /// <remarks>
        /// <para>The return type is as follows:</para>
        /// <list>
        /// <item><c>xs:string</c> - String</item>
        /// <item><c>xs:integer</c> - Long</item>
        /// <item><c>xs:decimal</c> - Decimal</item>
        /// <item><c>xs:double</c> - Double</item>
        /// <item><c>xs:float</c> - Float</item>
        /// <item><c>xs:boolean</c> - Bool</item>
        /// <item><c>xs:QName</c> - QName</item>
        /// <item><c>xs:anyURI</c> - Uri</item>
        /// <item><c>xs:untypedAtomic</c> - String</item>
        /// <item>wrapped external object - the original external object</item>
        /// <item>Other types - currently String, but this may change in the future</item>
        /// </list>
        /// </remarks>
        /// <returns>The value converted to the most appropriate CLI type</returns>

        public object Value =>
            value switch {
                IntegerValue integerValue => integerValue.longValue(),
                DoubleValue doubleValue => doubleValue.getDoubleValue(),
                FloatValue floatValue => floatValue.getFloatValue(),
                DecimalValue decimalValue => decimal.Parse(decimalValue.getStringValue()),
                BooleanValue booleanValue => booleanValue.getBooleanValue(),
                AnyURIValue uriValue => new Uri(uriValue.getStringValue()),
                QNameValue nameValue => new QName(nameValue),
                _ => ((AtomicValue) value).getStringValue()
            };
    }
}

// Copyright (c) 2021 Saxonica Limited