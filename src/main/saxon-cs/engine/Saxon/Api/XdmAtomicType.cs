using net.sf.saxon.om;
using net.sf.saxon.type;
using net.sf.saxon.value;


namespace Saxon.Api {
    /// <summary>
    /// An instance of class <c>XdmAtomicType</c> represents a specific atomic type, for example
    /// <c>xs:double</c>, <c>xs:integer</c>, or <c>xs:anyAtomicType</c>. This may be either a built-in
    /// atomic type or a type defined in a user-written schema.
    /// </summary>
    /// <remarks>
    /// To get an <c>XdmAtomicType</c> instance representing a built-in atomic type, use one of the predefined instances
    /// of the subclass <c>XdmBuiltInAtomicType</c>. To get an <c>XdmAtomicType</c> instance representing a user-defined
    /// atomic type (defined in a schema), use the method <c>GetAtomicType</c> defined on the <c>SchemaManager</c> class.
    /// </remarks>
    public class XdmAtomicType : XdmItemType {
        /// <summary>
        /// Instance object of the <c>XdmAtomicType</c> class
        /// </summary>
        public static readonly XdmAtomicType Instance =
            new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.ANY_ATOMIC);

        private XdmAtomicType(AtomicType type) : base (type) {
        }

        /// <summary>
        ///  ItemType representing the built-in (but non-primitive) type xs:short
        /// </summary>
        public static readonly XdmAtomicType SHORT = BuiltInAtomicType(StandardNames.XS_SHORT);


        /// <summary>
        /// ItemType representing the primitive type xs:string
        /// </summary>
        public static readonly XdmAtomicType STRING = BuiltInAtomicType(StandardNames.XS_STRING);

        /// <summary>
        /// ItemType representing the primitive type xs:boolean
        /// </summary>
        public static readonly XdmAtomicType BOOLEAN = BuiltInAtomicType(StandardNames.XS_BOOLEAN);

        /// <summary>
        /// ItemType representing the primitive type xs:duration
        /// </summary>
        public static readonly XdmAtomicType DURATION = BuiltInAtomicType(StandardNames.XS_DURATION);

        /// <summary>
        /// ItemType representing the primitive type xs:dateTime
        /// </summary>
        public static readonly XdmAtomicType DATE_TIME = BuiltInAtomicType(StandardNames.XS_DATE_TIME);

        /// <summary>
        /// ItemType representing the primitive type xs:date
        /// </summary>
        public static readonly XdmAtomicType DATE = BuiltInAtomicType(StandardNames.XS_DATE);

        /// <summary>
        /// ItemType representing the primitive type xs:time
        /// </summary>
        public static readonly XdmAtomicType TIME = BuiltInAtomicType(StandardNames.XS_TIME);

        /// <summary>
        /// ItemType representing the primitive type xs:gYearMonth
        /// </summary>
        public static readonly XdmAtomicType G_YEAR_MONTH = BuiltInAtomicType(StandardNames.XS_G_YEAR_MONTH);

        /// <summary>
        ///  ItemType representing the primitive type xs:gMonth
        /// </summary>
        public static readonly XdmAtomicType G_MONTH = BuiltInAtomicType(StandardNames.XS_G_MONTH);

        /// <summary>
        /// ItemType representing the primitive type xs:gMonthDay
        /// </summary>
        public static readonly XdmAtomicType G_MONTH_DAY = BuiltInAtomicType(StandardNames.XS_G_MONTH_DAY);

        /// <summary>
        /// ItemType representing the primitive type xs:gYear
        /// </summary>
        public static readonly XdmAtomicType G_YEAR = BuiltInAtomicType(StandardNames.XS_G_YEAR);

        /// <summary>
        /// ItemType representing the primitive type xs:gDay
        /// </summary>
        public static readonly XdmAtomicType G_DAY = BuiltInAtomicType(StandardNames.XS_G_DAY);

        /// <summary>
        /// ItemType representing the primitive type xs:hexBinary
        /// </summary>
        public static readonly XdmAtomicType HEX_BINARY = BuiltInAtomicType(StandardNames.XS_HEX_BINARY);

        /// <summary>
        ///  ItemType representing the primitive type xs:base64Binary
        /// </summary>
        public static readonly XdmAtomicType BASE64_BINARY = BuiltInAtomicType(StandardNames.XS_BASE64_BINARY);

        /// <summary>
        /// ItemType representing the primitive type xs:anyURI
        /// </summary>
        public static readonly XdmAtomicType ANY_URI = BuiltInAtomicType(StandardNames.XS_ANY_URI);

        /// <summary>
        /// ItemType representing the primitive type xs:QName
        /// </summary>
        public static readonly XdmAtomicType QNAME = BuiltInAtomicType(StandardNames.XS_QNAME);

        /// <summary>
        /// ItemType representing the primitive type xs:NOTATION
        /// </summary>
        public static readonly XdmAtomicType NOTATION = BuiltInAtomicType(StandardNames.XS_NOTATION);

        /// <summary>
        ///  ItemType representing the XPath-defined type xs:untypedAtomic
        /// </summary>
        public static readonly XdmAtomicType UNTYPED_ATOMIC = BuiltInAtomicType(StandardNames.XS_UNTYPED_ATOMIC);

        /// <summary>
        /// ItemType representing the primitive type xs:decimal
        /// </summary>
        public static readonly XdmAtomicType DECIMAL = BuiltInAtomicType(StandardNames.XS_DECIMAL);

        /// <summary>
        /// ItemType representing the primitive type xs:float
        /// </summary>
        public static readonly XdmAtomicType FLOAT = BuiltInAtomicType(StandardNames.XS_FLOAT);

        /// <summary>
        /// ItemType representing the primitive type xs:double
        /// </summary>
        public static readonly XdmAtomicType DOUBLE = BuiltInAtomicType(StandardNames.XS_DOUBLE);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:integer
        /// </summary>
        public static readonly XdmAtomicType INTEGER = BuiltInAtomicType(StandardNames.XS_INTEGER);

        /// <summary>
        /// 
        /// </summary>
        public static readonly XdmAtomicType NON_POSITIVE_INTEGER = BuiltInAtomicType(StandardNames.XS_NON_POSITIVE_INTEGER);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:negativeInteger
        /// </summary>
        public static readonly XdmAtomicType NEGATIVE_INTEGER = BuiltInAtomicType(StandardNames.XS_NEGATIVE_INTEGER);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:long
        /// </summary>
        public static readonly XdmAtomicType LONG = BuiltInAtomicType(StandardNames.XS_LONG);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:int
        /// </summary>
        public static readonly XdmAtomicType INT = BuiltInAtomicType(StandardNames.XS_INT);


        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:byte
        /// </summary>
        public static readonly XdmAtomicType BYTE = BuiltInAtomicType(StandardNames.XS_BYTE);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:nonNegativeInteger
        /// </summary>
        public static readonly XdmAtomicType NON_NEGATIVE_INTEGER = BuiltInAtomicType(StandardNames.XS_NON_NEGATIVE_INTEGER);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:positiveInteger
        /// </summary>
        public static readonly XdmAtomicType POSITIVE_INTEGER = BuiltInAtomicType(StandardNames.XS_POSITIVE_INTEGER);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:unsignedLong
        /// </summary>
        public static readonly XdmAtomicType UNSIGNED_LONG = BuiltInAtomicType(StandardNames.XS_UNSIGNED_LONG);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:unsignedInt
        /// </summary>
        public static readonly XdmAtomicType UNSIGNED_INT = BuiltInAtomicType(StandardNames.XS_UNSIGNED_INT);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:unsignedShort
        /// </summary>
        public static readonly XdmAtomicType UNSIGNED_SHORT = BuiltInAtomicType(StandardNames.XS_UNSIGNED_SHORT);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:unsignedByte
        /// </summary>
        public static readonly XdmAtomicType UNSIGNED_BYTE = BuiltInAtomicType(StandardNames.XS_UNSIGNED_BYTE);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:yearMonthDuration
        /// </summary>
        public static readonly XdmAtomicType YEAR_MONTH_DURATION = BuiltInAtomicType(StandardNames.XS_YEAR_MONTH_DURATION);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:dayTimeDuration
        /// </summary>
        public static readonly XdmAtomicType DAY_TIME_DURATION = BuiltInAtomicType(StandardNames.XS_DAY_TIME_DURATION);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:normalizedString
        /// </summary>
        public static readonly XdmAtomicType NORMALIZED_STRING = BuiltInAtomicType(StandardNames.XS_NORMALIZED_STRING);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:token
        /// </summary>
        public static readonly XdmAtomicType TOKEN = BuiltInAtomicType(StandardNames.XS_TOKEN);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:language
        /// </summary>
        public static readonly XdmAtomicType LANGUAGE = BuiltInAtomicType(StandardNames.XS_LANGUAGE);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:Name
        /// </summary>
        public static readonly XdmAtomicType NAME = BuiltInAtomicType(StandardNames.XS_NAME);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:NMTOKEN
        /// </summary>
        public static readonly XdmAtomicType NMTOKEN = BuiltInAtomicType(StandardNames.XS_NMTOKEN);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:NCName
        /// </summary>
        public static readonly XdmAtomicType NCNAME = BuiltInAtomicType(StandardNames.XS_NCNAME);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:ID
        /// </summary>
        public static readonly XdmAtomicType ID = BuiltInAtomicType(StandardNames.XS_ID);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:IDREF
        /// </summary>
        public static readonly XdmAtomicType IDREF = BuiltInAtomicType(StandardNames.XS_IDREF);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:ENTITY
        /// </summary>
        public static readonly XdmAtomicType ENTITY = BuiltInAtomicType(StandardNames.XS_ENTITY);

        /// <summary>
        /// ItemType representing the built-in (but non-primitive) type xs:dateTimeStamp
        /// (introduced in XSD 1.1)
        /// </summary>
        public static readonly XdmAtomicType DATE_TIME_STAMP = BuiltInAtomicType(StandardNames.XS_DATE_TIME_STAMP);


        /// <summary>
        /// Get an <c>XdmAtomicType</c> object representing a built-in atomic type with a given name.
        /// </summary>
        /// <param name="name">The name of the required built-in atomic type</param>
        /// <returns>An <c>XdmAtomicType</c> object representing the built-in atomic type with the supplied name.
        /// Returns null if there is no built-in atomic type with this name.
        /// It is undefined whether two requests for the same built-in type will return the same object.</returns>
        public static XdmAtomicType BuiltInAtomicType(QName name) {
            int fingerprint = StandardNames.getFingerprint(name.Uri, name.LocalName);
            return BuiltInAtomicType(fingerprint);
        }

        private static XdmAtomicType BuiltInAtomicType(int fingerprint) {
            if (fingerprint == -1) {
                return null;
            }

            switch (fingerprint) {
                case StandardNames.XS_ANY_ATOMIC_TYPE:
                    return XdmAtomicType.Instance;

                case StandardNames.XS_STRING:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.STRING);

                case StandardNames.XS_BOOLEAN:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.BOOLEAN);

                case StandardNames.XS_DURATION:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DURATION);

                case StandardNames.XS_DATE_TIME:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DATE_TIME);

                case StandardNames.XS_DATE:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DATE);

                case StandardNames.XS_TIME:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.TIME);

                case StandardNames.XS_G_YEAR_MONTH:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.G_YEAR_MONTH);

                case StandardNames.XS_G_MONTH:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.G_MONTH);

                case StandardNames.XS_G_MONTH_DAY:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.G_MONTH_DAY);

                case StandardNames.XS_G_YEAR:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.G_YEAR);

                case StandardNames.XS_G_DAY:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.G_DAY);

                case StandardNames.XS_HEX_BINARY:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.HEX_BINARY);

                case StandardNames.XS_BASE64_BINARY:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.BASE64_BINARY);

                case StandardNames.XS_ANY_URI:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.ANY_URI);

                case StandardNames.XS_QNAME:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.QNAME);

                case StandardNames.XS_NOTATION:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NOTATION);

                case StandardNames.XS_UNTYPED_ATOMIC:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.UNTYPED_ATOMIC);

                case StandardNames.XS_DECIMAL:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DECIMAL);

                case StandardNames.XS_FLOAT:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.FLOAT);

                case StandardNames.XS_DOUBLE:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DOUBLE);

                case StandardNames.XS_INTEGER:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.INTEGER);

                case StandardNames.XS_NON_POSITIVE_INTEGER:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NON_POSITIVE_INTEGER);

                case StandardNames.XS_NEGATIVE_INTEGER:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NEGATIVE_INTEGER);

                case StandardNames.XS_LONG:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.LONG);

                case StandardNames.XS_INT:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.INT);

                case StandardNames.XS_SHORT:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.SHORT);

                case StandardNames.XS_BYTE:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.BYTE);

                case StandardNames.XS_NON_NEGATIVE_INTEGER:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NON_NEGATIVE_INTEGER);

                case StandardNames.XS_POSITIVE_INTEGER:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.POSITIVE_INTEGER);

                case StandardNames.XS_UNSIGNED_LONG:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.UNSIGNED_LONG);

                case StandardNames.XS_UNSIGNED_INT:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.UNSIGNED_INT);

                case StandardNames.XS_UNSIGNED_SHORT:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.UNSIGNED_SHORT);

                case StandardNames.XS_UNSIGNED_BYTE:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.UNSIGNED_BYTE);

                case StandardNames.XS_YEAR_MONTH_DURATION:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.YEAR_MONTH_DURATION);

                case StandardNames.XS_DAY_TIME_DURATION:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DAY_TIME_DURATION);

                case StandardNames.XS_NORMALIZED_STRING:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NORMALIZED_STRING);

                case StandardNames.XS_TOKEN:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.TOKEN);

                case StandardNames.XS_LANGUAGE:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.LANGUAGE);

                case StandardNames.XS_NAME:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NAME);

                case StandardNames.XS_NMTOKEN:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NMTOKEN);

                case StandardNames.XS_NCNAME:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.NCNAME);

                case StandardNames.XS_ID:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.ID);

                case StandardNames.XS_IDREF:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.IDREF);

                case StandardNames.XS_ENTITY:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.ENTITY);

                case StandardNames.XS_DATE_TIME_STAMP:
                    return new XdmAtomicType(net.sf.saxon.type.BuiltInAtomicType.DATE_TIME_STAMP);


                case StandardNames.XS_ERROR:
                    return null;

                default:
                    throw new SaxonApiException(
                        "Unknown atomic type " + StandardNames.getClarkName(fingerprint));
            }
        }


        /// <summary>
        /// The name of the atomic type, or null if the type is anonymous.
        /// </summary>

        public QName Name {
            get {
                StructuredQName jQName = ((AtomicType) underlyingType).getTypeName();
                if (jQName == null) {
                    return null;
                }

                return new QName(jQName.getPrefix(), jQName.getURI(), jQName.getLocalPart());
            }
        }

        /// <summary>
        /// Determine whether this item type matches a given item
        /// </summary>
        /// <param name="item">the item to be tested against this item type</param>
        /// <returns>true if the item matches this item type, false if it does not match</returns>
        public override bool Matches(XdmItem item) {
            Item theItem = (Item) item.value;
            return theItem is AtomicValue 
                   && ((AtomicType)underlyingType).matches((AtomicValue)theItem, null);
        }

        public override string ToString() {
            return Name.EQName;
        }

        /// <summary>
        /// Determine whether this ItemType subsumes another ItemType. Specifically
        /// <code>A.Sumsumes(B)</code> is true if every value that matches the ItemType B also
        /// matches the ItemType A.
        /// </summary>
        /// <param name="other">the other ItemType</param>
        /// <returns>true if this ItemType subsumes the other ItemType. This includes the case where A and B
        /// represent the same ItemType.</returns>
        public override bool Subsumes(XdmItemType other) {
            net.sf.saxon.type.ItemType otherType = other.Unwrap();
            if (!otherType.isPlainType()) {
                return false;
            }
            AtomicType type = (AtomicType) otherType;
            return subsumesUnderlyingType(type);
        }

        private bool subsumesUnderlyingType(AtomicType otherType) {
            SchemaType thisType = (AtomicType)underlyingType;
            BuiltInAtomicType builtIn =
                otherType is BuiltInAtomicType ? (BuiltInAtomicType) otherType : (BuiltInAtomicType) otherType.getBuiltInBaseType();
            while (true) {
                if (builtIn.isSameType(thisType)) {
                    return true;
                }
                SchemaType baseType = builtIn.getBaseType();
                if (!(baseType is BuiltInAtomicType)) {
                    return false;
                }
                builtIn = (BuiltInAtomicType) baseType;
            }
        }
        
    }
}