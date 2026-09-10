using System;
using net.sf.saxon;
using net.sf.saxon.lib;
using net.sf.saxon.om;
using net.sf.saxon.type;
using net.sf.saxon.value;
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

        public XdmAtomicValue(AtomicValue atomicValue) : base(atomicValue) {
            
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
        public XdmAtomicValue(String str) : base(new StringValue(str)) {
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
        public XdmAtomicValue(DateTime dt) {
            if (dt.Kind == DateTimeKind.Local) {
                int offsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;
                this.value = new net.sf.saxon.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month),
                    Convert.ToByte(dt.Day), Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute),
                    Convert.ToByte(dt.Second), Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)),
                    offsetMinutes);
            }
            else if (dt.Kind == DateTimeKind.Utc) {
                this.value = new net.sf.saxon.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month),
                    Convert.ToByte(dt.Day), Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute),
                    Convert.ToByte(dt.Second), Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)), 0);
            }
            else if (dt.Kind == DateTimeKind.Unspecified) {
                int offsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;
                this.value = new net.sf.saxon.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month),
                    Convert.ToByte(dt.Day), Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute),
                    Convert.ToByte(dt.Second), Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)),
                    net.sf.saxon.value.CalendarValue.NO_TIMEZONE);
            }
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:dateTime</c> with a specific timezone offset from a DateTimeOffset object.
        /// </summary>
        /// <param name="offset">The DateTimeOffset value</param>
        public XdmAtomicValue(DateTimeOffset offset) {
            int offsetMinutes = offset.Offset.Minutes;
            DateTime dt = offset.DateTime;
            this.value = new net.sf.saxon.value.DateTimeValue(dt.Year, Convert.ToByte(dt.Month), Convert.ToByte(dt.Day),
                Convert.ToByte(dt.Hour), Convert.ToByte(dt.Minute), Convert.ToByte(dt.Second),
                Convert.ToByte(dt.Ticks * (1000000000 / TimeSpan.TicksPerSecond)), offsetMinutes);
        }


        /// <summary>
        /// Construct an atomic value of type <c>xs:double</c>
        /// </summary>
        /// <param name="d">The double value</param>
        public XdmAtomicValue(double d) {
            this.value = new DoubleValue(d);
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:boolean</c>
        /// </summary>
        /// <param name="b">The boolean value</param>
        public XdmAtomicValue(bool b) {
            this.value = BooleanValue.get(b);
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:anyURI</c>
        /// </summary>
        /// <param name="u">The uri value</param>
        public XdmAtomicValue(Uri u) {
            this.value = new AnyURIValue(u.ToString());
        }

        /// <summary>
        /// Construct an atomic value of type <c>xs:QName</c>
        /// </summary>
        /// <param name="q">The QName value</param>                
        public XdmAtomicValue(QName q) {
            this.value = new QNameValue(
                q.Prefix, q.Uri, q.LocalName);
        }

        /// <summary>
        /// Construct an atomic value of a given type
        /// </summary>
        /// <param name="lexicalForm">The string representation of the value (any value that is acceptable
        /// in the lexical space, as defined by XML Schema Part 2). Whitespace normalization as defined by
        /// the target type will be applied to the value.</param>
        /// <param name="type">The type given as an <c>XdmAtomicType</c></param>
        public XdmAtomicValue(String lexicalForm, XdmAtomicType type) {
            net.sf.saxon.type.ItemType it = type.Unwrap();
            if (!it.isPlainType()) {
                throw new StaticError(new net.sf.saxon.s9api.SaxonApiException("Requested type is not atomic"));
            }

            if (((AtomicType) it).isAbstract()) {
                throw new StaticError(
                    new net.sf.saxon.s9api.SaxonApiException("Requested type is not namespace-sensitive"));
            }

            try {
                net.sf.saxon.str.UnicodeString us = net.sf.saxon.str.StringView.of(lexicalForm).tidy();
                net.sf.saxon.type.StringConverter Converter =
                    ((AtomicType) it).getStringConverter(ConversionRules.DEFAULT);
                this.value = Converter.convertString(us).asAtomic();
            }
            catch (Exception ex) {
                throw new DynamicError(ex.Message);
            }
        }

        /// <summary>
        /// Construct an atomic value of a given built-in or user-defined type
        /// </summary>
        /// <example>
        ///   <code>XdmAtomicValue("abcd", QName.XDT_UNTYPED_ATOMIC, processor)</code>
        ///   <para>creates an untyped atomic value containing the string "abcd"</para>
        /// </example>
        /// <param name="lexicalForm">The string representation of the value (any value that is acceptable
        /// in the lexical space, as defined by XML Schema Part 2). Whitespace normalization as defined by
        /// the target type will be applied to the value.</param>
        /// <param name="type">The QName giving the name of the target type. This must be an atomic
        /// type, and it must not be a type that is namespace-sensitive (QName, NOTATION, or types derived
        /// from these). If the type is a user-defined type then its definition must be present
        /// in the schema cache maintained by the <c>SchemaManager</c>.</param> 
        /// <param name="processor">The <c>Processor</c> object. This is needed for looking up user-defined
        /// types, and also because some conversions are context-sensitive, for example they depend on the
        /// implicit timezone or the choice of XML 1.0 versus XML 1.1 for validating names.</param>
        /// <exception cref="ArgumentException">Thrown if the type is unknown or unsuitable, or if the supplied string is not
        /// a valid lexical representation of a value of the given type.</exception>
        public XdmAtomicValue(String lexicalForm, QName type, Processor processor) {
            Configuration jconfig = processor.Implementation;
            int fp = jconfig.getNamePool().getFingerprint(type.Uri, type.LocalName);
            if (fp == -1) {
                throw new ArgumentException("Unknown name " + type);
            }

            SchemaType st = jconfig.getSchemaType(new StructuredQName("", type.Uri.ToString(), type.LocalName));
            if (st == null) {
                throw new ArgumentException("Unknown type " + type);
            }

            if (!(st is AtomicType)) {
                throw new ArgumentException("Specified type " + type + " is not atomic");
            }

            if (((net.sf.saxon.type.SimpleType) st).isNamespaceSensitive()) {
                throw new ArgumentException("Specified type " + type + " is namespace-sensitive");
            }

            net.sf.saxon.str.UnicodeString us = net.sf.saxon.str.StringView.of(lexicalForm).tidy();
            ConversionResult result = ((AtomicType) st).getStringConverter(jconfig.getConversionRules())
                .convertString(us);

            if (result is net.sf.saxon.type.ValidationFailure) {
                throw new ArgumentException(((net.sf.saxon.type.ValidationFailure) result).getMessage());
            }

            this.value = (AtomicValue) result;
        }


        /// <summary>
        /// Create an atomic value of a type appropriate to the supplied value. 
        /// </summary>
        /// <remarks>
        /// The supplied value must be one of the following:
        /// <list>
        /// <item>An instance of the Saxon Java class <c>net.sf.saxon.value.AtomicValue</c></item>
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
        public static XdmAtomicValue MakeAtomicValue(object value) {
            if (value is AtomicValue) {
                return (XdmAtomicValue) XdmValue.Wrap((AtomicValue) value);
            }
            else if (value is Boolean) {
                return new XdmAtomicValue((Boolean) value);
            }
            else if (value is int) {
                return new XdmAtomicValue((int) value);
            }
            else if (value is long) {
                return new XdmAtomicValue((long) value);
            }
            else if (value is short) {
                return new XdmAtomicValue((short) value);
            }
            else if (value is Char) {
                return new XdmAtomicValue((long) value);
            }
            else if (value is Byte) {
                return new XdmAtomicValue((Byte) value);
            }
            else if (value is String) {
                return new XdmAtomicValue((String) value);
            }
            else if (value is Double) {
                return new XdmAtomicValue((Double) value);
            }
            else if (value is float) {
                return new XdmAtomicValue((float) value);
            }
            else if (value is decimal) {
                return new XdmAtomicValue((decimal) value);
            }
            else if (value is Uri) {
                return new XdmAtomicValue((Uri) value);
            }
            else if (value is QName) {
                return new XdmAtomicValue((QName) value);
            }

            if (value is XdmAtomicValue) {
                return (XdmAtomicValue) value;
            }
            else {
                throw new ArgumentException(value.ToString());
            }
        }


        /// <summary>
        /// Get the value converted to a boolean using the XPath casting rules.
        /// </summary>
        /// <returns>The result of converting to a boolean (Note: this is not the same as the
        /// effective boolean value).</returns> 
        public bool GetBooleanValue() {
            AtomicValue av = (AtomicValue) this.value;
            if (av is BooleanValue) {
                return ((BooleanValue) av).getBooleanValue();
            }
            else if (av is NumericValue) {
                return !av.isNaN() && ((NumericValue) av).signum() != 0;
            }
            else if (av is StringValue) {
                String s = av.getStringValue().Trim();
                return "1".Equals(s) || "true".Equals(s);
            }
            else {
                throw new ArgumentException("Cannot cast item to a boolean");
            }
        }


        /// <summary>
        /// Get the value converted to a long using the XPath casting rules.
        /// </summary>
        /// <returns>The result of converting to a long</returns>
        public long GetLongValue() {
            AtomicValue av = (AtomicValue) this.value;
            if (av is BooleanValue) {
                return ((BooleanValue) av).getBooleanValue() ? 0L : 1L;
            }
            else if (av is NumericValue) {
                try {
                    return ((NumericValue) av).longValue();
                }
                catch (Exception) {
                    throw new ArgumentException("Cannot cast item to an integer");
                }
            }
            else if (av is StringValue) {
                StringToDouble converter = StringToDouble.getInstance();
                return (long) converter.stringToNumber(av.getUnicodeStringValue());
            }
            else {
                throw new ArgumentException("Cannot cast item to an integer");
            }
        }


        /// <summary>
        /// Get the value converted to a double using the XPath casting rules.
        /// </summary>
        /// <remarks>If the value is a string, the XSD 1.1 rules are used, which means that the string
        /// "+INF" is recognised.</remarks>
        /// <returns>The result of converting to a double</returns>
        public double GetDoubleValue() {
            AtomicValue av = (AtomicValue) this.value;
            if (av is BooleanValue) {
                return ((BooleanValue) av).getBooleanValue() ? 0.0 : 1.0;
            }
            else if (av is NumericValue) {
                return ((NumericValue) av).getDoubleValue();
            }
            else if (av is StringValue) {
                try {
                    StringToDouble converter = StringToDouble11.getInstance();
                    return converter.stringToNumber(av.getUnicodeStringValue());
                }
                catch (Exception e) {
                    throw new ArgumentException(e.Message);
                }
            }
            else {
                throw new ArgumentException("Cannot cast item to a double");
            }
        }


        /// <summary>
        /// Get the value converted to a decimal using the XPath casting rules.
        /// </summary>
        /// <returns>The result of converting to a decimal</returns>
        public Decimal GetDecimalValue() {
            AtomicValue av = (AtomicValue) this.value;
            if (av is BooleanValue) {
                return ((BooleanValue) av).getBooleanValue() ? 0 : 1;
            }
            else if (av is NumericValue) {
                try {
                    return Convert.ToDecimal(((NumericValue) av).getDecimalValue().ToString());
                }
                catch (Exception) {
                    throw new ArgumentException("Cannot cast item to a decimal");
                }
            }
            else if (av is StringValue) {
                return Convert.ToDecimal(av.getUnicodeStringValue().ToString());
            }
            else {
                throw new ArgumentException("Cannot cast item to a decimal");
            }
        }


        /// <summary>
        /// Convert the atomic value to a string
        /// </summary>
        /// <returns>The value converted to a string, according to the rules
        /// of the XPath cast expression</returns>        
        public override String ToString() {
            return ((AtomicValue) value).getStringValue();
        }

        /// <summary>
        /// Compare two atomic values for equality
        /// </summary>
        /// <param name="other">The object to be compared</param>
        /// <returns>The result of the equality comparison, using the rules of the
        /// <c>op:is-same-key()</c> comparison used for comparing key values in maps.</returns>
        public override Boolean Equals(object other) {
            if (other is XdmAtomicValue) {
                return ((AtomicValue) value).asMapKey()
                    .Equals(((AtomicValue) ((XdmAtomicValue) other).value).asMapKey());
            }
            else {
                return false;
            }
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
        public QName GetTypeName() {
            StructuredQName sqname = ((AtomicValue) value).getItemType().getStructuredQName();
            return new QName(sqname.getPrefix(),
                sqname.getURI(),
                sqname.getLocalPart());
        }

        /// <summary>
        /// Get the name of the value's XDM type
        /// </summary>
        /// <param name="processor">The <code>Processor</code> object. 
        /// This parameter is no longer used, but is accepted for backwards compatibility.</param>
        /// <returns>The type of the value, as a QName.</returns>
        public QName GetTypeName(Processor processor) {
            return GetTypeName();
        }

        /// <summary>
        /// Get the name of the primitive type of the value
        /// </summary>
        /// <returns>The primitive type of the value, as a QName. This will be the name of
        /// one of the primitive types defined in XML Schema Part 2, or the XPath-defined
        /// type <c>xs:untypedAtomic</c>. For the purposes of this method, <c>xs:integer</c> is considered
        /// to be a primitive type.
        /// </returns>
        public QName GetPrimitiveTypeName() {
            int fp = ((AtomicValue) value).getItemType().getPrimitiveType();
            return new QName(StandardNames.getPrefix(fp),
                StandardNames.getURI(fp),
                StandardNames.getLocalName(fp));
        }

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

        public Object Value {
            get {
                if (value is IntegerValue) {
                    return ((IntegerValue) value).longValue();
                }
                else if (value is DoubleValue) {
                    return ((DoubleValue) value).getDoubleValue();
                }
                else if (value is FloatValue) {
                    return ((FloatValue) value).getFloatValue();
                }
                else if (value is DecimalValue) {
                    return Decimal.Parse(((DecimalValue) value).getStringValue());
                }
                else if (value is BooleanValue) {
                    return ((BooleanValue) value).getBooleanValue();
                }
                else if (value is AnyURIValue) {
                    return new Uri(((AnyURIValue) value).getStringValue());
                }
                else if (value is QNameValue) {
                    return new QName((QNameValue) value);
                }
                // else if (value is JDotNetObjectValue) // TODO: can't happen?
                // {
                //     return ((JDotNetObjectValue) value).getObject();
                // }
                else {
                    return ((AtomicValue) value).getStringValue();
                }
            }
        }
    }
}