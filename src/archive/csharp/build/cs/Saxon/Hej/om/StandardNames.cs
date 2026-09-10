using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public abstract class StandardNames {

        static StandardNames() {
            bindXSLTName(XSL_ACCEPT, "accept");
            bindXSLTName(XSL_ACCUMULATOR, "accumulator");
            bindXSLTName(XSL_ACCUMULATOR_RULE, "accumulator-rule");
            bindXSLTName(XSL_ANALYZE_STRING, "analyze-string");
            bindXSLTName(XSL_APPLY_IMPORTS, "apply-imports");
            bindXSLTName(XSL_APPLY_TEMPLATES, "apply-templates");
            bindXSLTName(XSL_ACCEPT, "accept");
            bindXSLTName(XSL_ARRAY, "array");
            bindXSLTName(XSL_ARRAY_MEMBER, "array-member");
            bindXSLTName(XSL_ASSERT, "assert");
            bindXSLTName(XSL_ATTRIBUTE, "attribute");
            bindXSLTName(XSL_ATTRIBUTE_SET, "attribute-set");
            bindXSLTName(XSL_BREAK, "break");
            bindXSLTName(XSL_CALL_TEMPLATE, "call-template");
            bindXSLTName(XSL_CATCH, "catch");
            bindXSLTName(XSL_CHARACTER_MAP, "character-map");
            bindXSLTName(XSL_CHOOSE, "choose");
            bindXSLTName(XSL_COMMENT, "comment");
            bindXSLTName(XSL_CONTEXT_ITEM, "context-item");
            bindXSLTName(XSL_COPY, "copy");
            bindXSLTName(XSL_COPY_OF, "copy-of");
            bindXSLTName(XSL_DECIMAL_FORMAT, "decimal-format");
            bindXSLTName(XSL_DOCUMENT, "document");
            bindXSLTName(XSL_ELEMENT, "element");
            bindXSLTName(XSL_EVALUATE, "evaluate");
            bindXSLTName(XSL_EXPOSE, "expose");
            bindXSLTName(XSL_FALLBACK, "fallback");
            bindXSLTName(XSL_FOR_EACH, "for-each");
            bindXSLTName(XSL_FOR_EACH_GROUP, "for-each-group");
            bindXSLTName(XSL_FORK, "fork");
            bindXSLTName(XSL_FUNCTION, "function");
            bindXSLTName(XSL_GLOBAL_CONTEXT_ITEM, "global-context-item");
            bindXSLTName(XSL_IF, "if");
            bindXSLTName(XSL_IMPORT, "import");
            bindXSLTName(XSL_IMPORT_SCHEMA, "import-schema");
            bindXSLTName(XSL_INCLUDE, "include");
            bindXSLTName(XSL_ITEM_TYPE, "item-type");
            bindXSLTName(XSL_ITERATE, "iterate");
            bindXSLTName(XSL_KEY, "key");
            bindXSLTName(XSL_MAP, "map");
            bindXSLTName(XSL_MAP_ENTRY, "map-entry");
            bindXSLTName(XSL_MATCH, "match");
            bindXSLTName(XSL_MATCHING_SUBSTRING, "matching-substring");
            bindXSLTName(XSL_MERGE, "merge");
            bindXSLTName(XSL_MERGE_SOURCE, "merge-source");
            bindXSLTName(XSL_MERGE_ACTION, "merge-action");
            bindXSLTName(XSL_MERGE_KEY, "merge-key");
            bindXSLTName(XSL_MESSAGE, "message");
            bindXSLTName(XSL_MODE, "mode");
            bindXSLTName(XSL_NEXT_MATCH, "next-match");
            bindXSLTName(XSL_NUMBER, "number");
            bindXSLTName(XSL_NAMESPACE, "namespace");
            bindXSLTName(XSL_NAMESPACE_ALIAS, "namespace-alias");
            bindXSLTName(XSL_NEXT_ITERATION, "next-iteration");
            bindXSLTName(XSL_NON_MATCHING_SUBSTRING, "non-matching-substring");
            bindXSLTName(XSL_ON_COMPLETION, "on-completion");
            bindXSLTName(XSL_ON_EMPTY, "on-empty");
            bindXSLTName(XSL_ON_NON_EMPTY, "on-non-empty");
            bindXSLTName(XSL_OTHERWISE, "otherwise");
            bindXSLTName(XSL_OUTPUT, "output");
            bindXSLTName(XSL_OUTPUT_CHARACTER, "output-character");
            bindXSLTName(XSL_OVERRIDE, "override");
            bindXSLTName(XSL_PACKAGE, "package");
            bindXSLTName(XSL_PARAM, "param");
            bindXSLTName(XSL_PERFORM_SORT, "perform-sort");
            bindXSLTName(XSL_PRESERVE_SPACE, "preserve-space");
            bindXSLTName(XSL_PROCESSING_INSTRUCTION, "processing-instruction");
            bindXSLTName(XSL_RESULT_DOCUMENT, "result-document");
            bindXSLTName(XSL_SEQUENCE, "sequence");
            bindXSLTName(XSL_SORT, "sort");
            bindXSLTName(XSL_SOURCE_DOCUMENT, "source-document");
            bindXSLTName(XSL_STRIP_SPACE, "strip-space");
            bindXSLTName(XSL_STYLESHEET, "stylesheet");
            bindXSLTName(XSL_SWITCH, "switch");
            bindXSLTName(XSL_TEMPLATE, "template");
            bindXSLTName(XSL_TEXT, "text");
            bindXSLTName(XSL_TRANSFORM, "transform");
            bindXSLTName(XSL_TRY, "try");
            bindXSLTName(XSL_USE_PACKAGE, "use-package");
            bindXSLTName(XSL_VALUE_OF, "value-of");
            bindXSLTName(XSL_VARIABLE, "variable");
            bindXSLTName(XSL_WITH_PARAM, "with-param");
            bindXSLTName(XSL_WHEN, "when");
            bindXSLTName(XSL_WHERE_POPULATED, "where-populated");
            bindXSLTName(XSL_DEFAULT_COLLATION, "default-collation");
            bindXSLTName(XSL_DEFAULT_MODE, "default-mode");
            bindXSLTName(XSL_DEFAULT_VALIDATION, "default-validation");
            bindXSLTName(XSL_EXPAND_TEXT, "expand-text");
            bindXSLTName(XSL_EXCLUDE_RESULT_PREFIXES, "exclude-result-prefixes");
            bindXSLTName(XSL_EXTENSION_ELEMENT_PREFIXES, "extension-element-prefixes");
            bindXSLTName(XSL_INHERIT_NAMESPACES, "inherit-namespaces");
            bindXSLTName(XSL_TYPE, "type");
            bindXSLTName(XSL_USE_ATTRIBUTE_SETS, "use-attribute-sets");
            bindXSLTName(XSL_USE_WHEN, "use-when");
            bindXSLTName(XSL_VALIDATION, "validation");
            bindXSLTName(XSL_VERSION, "version");
            bindXSLTName(XSL_XPATH_DEFAULT_NAMESPACE, "xpath-default-namespace");
            bindSaxonName(SAXON_ASSIGN, "assign");
            bindSaxonName(SAXON_DEEP_UPDATE, "deep-update");
            bindSaxonName(SAXON_DISTINCT, "distinct");
            bindSaxonName(SAXON_DO, "do");
            bindSaxonName(SAXON_DOCTYPE, "doctype");
            bindSaxonName(SAXON_ENTITY_REF, "entity-ref");
            bindSaxonName(SAXON_ORDER, "order");
            bindSaxonName(SAXON_WHILE, "while");
            bindSaxonName(SAXON_PARAM, "param");
            bindSaxonName(SAXON_PREPROCESS, "preprocess");
            bindXMLName(XML_BASE, "base");
            bindXMLName(XML_SPACE, "space");
            bindXMLName(XML_LANG, "lang");
            bindXMLName(XML_ID, "id");
            bindXMLName(XML_LANG_TYPE, "_langType");
            bindXMLName(XML_SPACE_TYPE, "_spaceType");
            bindXSName(XS_STRING, "string");
            bindXSName(XS_BOOLEAN, "boolean");
            bindXSName(XS_DECIMAL, "decimal");
            bindXSName(XS_FLOAT, "float");
            bindXSName(XS_DOUBLE, "double");
            bindXSName(XS_DURATION, "duration");
            bindXSName(XS_DATE_TIME, "dateTime");
            bindXSName(XS_TIME, "time");
            bindXSName(XS_DATE, "date");
            bindXSName(XS_G_YEAR_MONTH, "gYearMonth");
            bindXSName(XS_G_YEAR, "gYear");
            bindXSName(XS_G_MONTH_DAY, "gMonthDay");
            bindXSName(XS_G_DAY, "gDay");
            bindXSName(XS_G_MONTH, "gMonth");
            bindXSName(XS_HEX_BINARY, "hexBinary");
            bindXSName(XS_BASE64_BINARY, "base64Binary");
            bindXSName(XS_ANY_URI, "anyURI");
            bindXSName(XS_QNAME, "QName");
            bindXSName(XS_NOTATION, "NOTATION");
            bindXSName(XS_NUMERIC, "numeric");
            bindXSName(XS_INTEGER, "integer");
            bindXSName(XS_NON_POSITIVE_INTEGER, "nonPositiveInteger");
            bindXSName(XS_NEGATIVE_INTEGER, "negativeInteger");
            bindXSName(XS_LONG, "long");
            bindXSName(XS_INT, "int");
            bindXSName(XS_SHORT, "short");
            bindXSName(XS_BYTE, "byte");
            bindXSName(XS_NON_NEGATIVE_INTEGER, "nonNegativeInteger");
            bindXSName(XS_POSITIVE_INTEGER, "positiveInteger");
            bindXSName(XS_UNSIGNED_LONG, "unsignedLong");
            bindXSName(XS_UNSIGNED_INT, "unsignedInt");
            bindXSName(XS_UNSIGNED_SHORT, "unsignedShort");
            bindXSName(XS_UNSIGNED_BYTE, "unsignedByte");
            bindXSName(XS_NORMALIZED_STRING, "normalizedString");
            bindXSName(XS_TOKEN, "token");
            bindXSName(XS_LANGUAGE, "language");
            bindXSName(XS_NMTOKEN, "NMTOKEN");
            bindXSName(XS_NMTOKENS, "NMTOKENS");
            bindXSName(XS_NAME, "Name");
            bindXSName(XS_NCNAME, "NCName");
            bindXSName(XS_ID, "ID");
            bindXSName(XS_IDREF, "IDREF");
            bindXSName(XS_IDREFS, "IDREFS");
            bindXSName(XS_ENTITY, "ENTITY");
            bindXSName(XS_ENTITIES, "ENTITIES");
            bindXSName(XS_DATE_TIME_STAMP, "dateTimeStamp");
            bindXSName(XS_ANY_TYPE, "anyType");
            bindXSName(XS_ANY_SIMPLE_TYPE, "anySimpleType");
            bindXSName(XS_ERROR, "error");
            bindXSName(XS_ALL, "all");
            bindXSName(XS_ALTERNATIVE, "alternative");
            bindXSName(XS_ANNOTATION, "annotation");
            bindXSName(XS_ANY, "any");
            bindXSName(XS_ANY_ATTRIBUTE, "anyAttribute");
            bindXSName(XS_APPINFO, "appinfo");
            bindXSName(XS_ASSERT, "assert");
            bindXSName(XS_ASSERTION, "assertion");
            bindXSName(XS_ATTRIBUTE, "attribute");
            bindXSName(XS_ATTRIBUTE_GROUP, "attributeGroup");
            bindXSName(XS_CHOICE, "choice");
            bindXSName(XS_COMPLEX_CONTENT, "complexContent");
            bindXSName(XS_COMPLEX_TYPE, "complexType");
            bindXSName(XS_DEFAULT_OPEN_CONTENT, "defaultOpenContent");
            bindXSName(XS_DOCUMENTATION, "documentation");
            bindXSName(XS_ELEMENT, "element");
            bindXSName(XS_ENUMERATION, "enumeration");
            bindXSName(XS_EXPLICIT_TIMEZONE, "explicitTimezone");
            bindXSName(XS_EXTENSION, "extension");
            bindXSName(XS_FIELD, "field");
            bindXSName(XS_FRACTION_DIGITS, "fractionDigits");
            bindXSName(XS_GROUP, "group");
            bindXSName(XS_IMPORT, "import");
            bindXSName(XS_INCLUDE, "include");
            bindXSName(XS_KEY, "key");
            bindXSName(XS_KEYREF, "keyref");
            bindXSName(XS_LENGTH, "length");
            bindXSName(XS_LIST, "list");
            bindXSName(XS_MAX_EXCLUSIVE, "maxExclusive");
            bindXSName(XS_MAX_INCLUSIVE, "maxInclusive");
            bindXSName(XS_MAX_LENGTH, "maxLength");
            bindXSName(XS_MAX_SCALE, "maxScale");
            bindXSName(XS_MIN_EXCLUSIVE, "minExclusive");
            bindXSName(XS_MIN_INCLUSIVE, "minInclusive");
            bindXSName(XS_MIN_LENGTH, "minLength");
            bindXSName(XS_MIN_SCALE, "minScale");
            bindXSName(XS_notation, "notation");
            bindXSName(XS_OPEN_CONTENT, "openContent");
            bindXSName(XS_OVERRIDE, "override");
            bindXSName(XS_PATTERN, "pattern");
            bindXSName(XS_REDEFINE, "redefine");
            bindXSName(XS_RESTRICTION, "restriction");
            bindXSName(XS_SCHEMA, "schema");
            bindXSName(XS_SELECTOR, "selector");
            bindXSName(XS_SEQUENCE, "sequence");
            bindXSName(XS_SIMPLE_CONTENT, "simpleContent");
            bindXSName(XS_SIMPLE_TYPE, "simpleType");
            bindXSName(XS_TOTAL_DIGITS, "totalDigits");
            bindXSName(XS_UNION, "union");
            bindXSName(XS_UNIQUE, "unique");
            bindXSName(XS_WHITE_SPACE, "whiteSpace");
            bindXSName(XS_UNTYPED, "untyped");
            bindXSName(XS_UNTYPED_ATOMIC, "untypedAtomic");
            bindXSName(XS_ANY_ATOMIC_TYPE, "anyAtomicType");
            bindXSName(XS_YEAR_MONTH_DURATION, "yearMonthDuration");
            bindXSName(XS_DAY_TIME_DURATION, "dayTimeDuration");
            bindXSIName(XSI_TYPE, "type");
            bindXSIName(XSI_NIL, "nil");
            bindXSIName(XSI_SCHEMA_LOCATION, "schemaLocation");
            bindXSIName(XSI_NO_NAMESPACE_SCHEMA_LOCATION, "noNamespaceSchemaLocation");
            bindXSIName(XSI_SCHEMA_LOCATION_TYPE, "anonymous_schemaLocationType");
        }
        private const int DFLT_NS = 0;
        private const int XSL_NS = 1;
        private const int SAXON_NS = 2;
        private const int XML_NS = 3;
        private const int XS_NS = 4;
        private const int XSI_NS = 5;
        public const int DFLT = 0;
        public const int XSL = 128;
        public const int SAXON = (128*2);
        public const int XML = (128*3);
        public const int XS = (128*4);
        public const int XSI = (128*5);
        public const int XSL_ACCEPT = XSL;
        public const int XSL_ACCUMULATOR = (XSL+1);
        public const int XSL_ACCUMULATOR_RULE = (XSL+2);
        public const int XSL_ANALYZE_STRING = (XSL+3);
        public const int XSL_APPLY_IMPORTS = (XSL+4);
        public const int XSL_APPLY_TEMPLATES = (XSL+5);
        public const int XSL_ARRAY = (XSL+6);
        public const int XSL_ARRAY_MEMBER = (XSL+7);
        public const int XSL_ASSERT = (XSL+8);
        public const int XSL_ATTRIBUTE = (XSL+9);
        public const int XSL_ATTRIBUTE_SET = (XSL+10);
        public const int XSL_BREAK = (XSL+11);
        public const int XSL_CALL_TEMPLATE = (XSL+12);
        public const int XSL_CATCH = (XSL+13);
        public const int XSL_CHARACTER_MAP = (XSL+14);
        public const int XSL_CHOOSE = (XSL+15);
        public const int XSL_COMMENT = (XSL+16);
        public const int XSL_CONTEXT_ITEM = (XSL+17);
        public const int XSL_COPY = (XSL+18);
        public const int XSL_COPY_OF = (XSL+19);
        public const int XSL_DECIMAL_FORMAT = (XSL+20);
        public const int XSL_DOCUMENT = (XSL+22);
        public const int XSL_ELEMENT = (XSL+23);
        public const int XSL_EXPOSE = (XSL+24);
        public const int XSL_EVALUATE = (XSL+25);
        public const int XSL_FALLBACK = (XSL+26);
        public const int XSL_FOR_EACH = (XSL+27);
        public const int XSL_FOR_EACH_GROUP = (XSL+28);
        public const int XSL_FORK = (XSL+31);
        public const int XSL_FUNCTION = (XSL+32);
        public const int XSL_GLOBAL_CONTEXT_ITEM = (XSL+33);
        public const int XSL_IF = (XSL+34);
        public const int XSL_IMPORT = (XSL+35);
        public const int XSL_IMPORT_SCHEMA = (XSL+36);
        public const int XSL_INCLUDE = (XSL+37);
        public const int XSL_ITEM_TYPE = (XSL+38);
        public const int XSL_ITERATE = (XSL+40);
        public const int XSL_KEY = (XSL+41);
        public const int XSL_MAP = (XSL+42);
        public const int XSL_MAP_ENTRY = (XSL+43);
        public const int XSL_MATCH = (XSL+44);
        public const int XSL_MATCHING_SUBSTRING = (XSL+45);
        public const int XSL_MERGE = (XSL+46);
        public const int XSL_MERGE_ACTION = (XSL+47);
        public const int XSL_MERGE_KEY = (XSL+48);
        public const int XSL_MERGE_SOURCE = (XSL+50);
        public const int XSL_MESSAGE = (XSL+51);
        public const int XSL_MODE = (XSL+52);
        public const int XSL_NAMESPACE = (XSL+53);
        public const int XSL_NAMESPACE_ALIAS = (XSL+54);
        public const int XSL_NEXT_ITERATION = (XSL+55);
        public const int XSL_NEXT_MATCH = (XSL+56);
        public const int XSL_NON_MATCHING_SUBSTRING = (XSL+57);
        public const int XSL_NUMBER = (XSL+58);
        public const int XSL_OTHERWISE = (XSL+59);
        public const int XSL_ON_COMPLETION = (XSL+60);
        public const int XSL_ON_EMPTY = (XSL+61);
        public const int XSL_ON_NON_EMPTY = (XSL+62);
        public const int XSL_OUTPUT = (XSL+63);
        public const int XSL_OVERRIDE = (XSL+64);
        public const int XSL_OUTPUT_CHARACTER = (XSL+65);
        public const int XSL_PACKAGE = (XSL+66);
        public const int XSL_PARAM = (XSL+67);
        public const int XSL_PERFORM_SORT = (XSL+70);
        public const int XSL_PRESERVE_SPACE = (XSL+71);
        public const int XSL_PROCESSING_INSTRUCTION = (XSL+72);
        public const int XSL_RESULT_DOCUMENT = (XSL+73);
        public const int XSL_SEQUENCE = (XSL+74);
        public const int XSL_SORT = (XSL+75);
        public const int XSL_SOURCE_DOCUMENT = (XSL+76);
        public const int XSL_STRIP_SPACE = (XSL+77);
        public const int XSL_STYLESHEET = (XSL+80);
        public const int XSL_SWITCH = (XSL+81);
        public const int XSL_TEMPLATE = (XSL+82);
        public const int XSL_TEXT = (XSL+83);
        public const int XSL_TRANSFORM = (XSL+84);
        public const int XSL_TRY = (XSL+85);
        public const int XSL_USE_PACKAGE = (XSL+86);
        public const int XSL_VALUE_OF = (XSL+87);
        public const int XSL_VARIABLE = (XSL+90);
        public const int XSL_WHEN = (XSL+91);
        public const int XSL_WHERE_POPULATED = (XSL+92);
        public const int XSL_WITH_PARAM = (XSL+93);
        public const int XSL_DEFAULT_COLLATION = (XSL+100);
        public const int XSL_DEFAULT_MODE = (XSL+101);
        public const int XSL_DEFAULT_VALIDATION = (XSL+102);
        public const int XSL_EXCLUDE_RESULT_PREFIXES = (XSL+103);
        public const int XSL_EXPAND_TEXT = (XSL+104);
        public const int XSL_EXTENSION_ELEMENT_PREFIXES = (XSL+105);
        public const int XSL_INHERIT_NAMESPACES = (XSL+106);
        public const int XSL_TYPE = (XSL+107);
        public const int XSL_USE_ATTRIBUTE_SETS = (XSL+108);
        public const int XSL_USE_WHEN = (XSL+109);
        public const int XSL_VALIDATION = (XSL+110);
        public const int XSL_VERSION = (XSL+111);
        public const int XSL_XPATH_DEFAULT_NAMESPACE = (XSL+112);
        public const int SAXON_ASSIGN = (SAXON+1);
        public const int SAXON_DEEP_UPDATE = (SAXON+3);
        public const int SAXON_DO = (SAXON+6);
        public const int SAXON_DOCTYPE = (SAXON+7);
        public const int SAXON_ENTITY_REF = (SAXON+8);
        public const int SAXON_TABULATE_MAPS = (SAXON+9);
        public const int SAXON_WHILE = (SAXON+15);
        public const int SAXON_PARAM = (SAXON+20);
        public const int SAXON_PREPROCESS = (SAXON+21);
        public const int SAXON_DISTINCT = (SAXON+22);
        public const int SAXON_ORDER = (SAXON+23);
        private static readonly string SAXON_B = (('{'+Saxon.Hej.lib.NamespaceConstant.SAXON)+'}');
        public static readonly string SAXON_ASYCHRONOUS = (SAXON_B+"asynchronous");
        public static readonly string SAXON_EXPLAIN = (SAXON_B+"explain");
        public const int XML_BASE = (XML+1);
        public const int XML_SPACE = (XML+2);
        public const int XML_LANG = (XML+3);
        public const int XML_ID = (XML+4);
        public const int XML_LANG_TYPE = (XML+5);
        public const int XML_SPACE_TYPE = 6;
        public static readonly Saxon.Hej.om.NodeName XML_ID_NAME = new Saxon.Hej.om.FingerprintedQName("xml", Saxon.Hej.lib.NamespaceConstant.XML, "id", XML_ID);
        public const int XS_STRING = (XS+1);
        public const int XS_BOOLEAN = (XS+2);
        public const int XS_DECIMAL = (XS+3);
        public const int XS_FLOAT = (XS+4);
        public const int XS_DOUBLE = (XS+5);
        public const int XS_DURATION = (XS+6);
        public const int XS_DATE_TIME = (XS+7);
        public const int XS_TIME = (XS+8);
        public const int XS_DATE = (XS+9);
        public const int XS_G_YEAR_MONTH = (XS+10);
        public const int XS_G_YEAR = (XS+11);
        public const int XS_G_MONTH_DAY = (XS+12);
        public const int XS_G_DAY = (XS+13);
        public const int XS_G_MONTH = (XS+14);
        public const int XS_HEX_BINARY = (XS+15);
        public const int XS_BASE64_BINARY = (XS+16);
        public const int XS_ANY_URI = (XS+17);
        public const int XS_QNAME = (XS+18);
        public const int XS_NOTATION = (XS+19);
        public const int XS_INTEGER = (XS+21);
        public const int XS_NON_POSITIVE_INTEGER = (XS+22);
        public const int XS_NEGATIVE_INTEGER = (XS+23);
        public const int XS_LONG = (XS+24);
        public const int XS_INT = (XS+25);
        public const int XS_SHORT = (XS+26);
        public const int XS_BYTE = (XS+27);
        public const int XS_NON_NEGATIVE_INTEGER = (XS+28);
        public const int XS_POSITIVE_INTEGER = (XS+29);
        public const int XS_UNSIGNED_LONG = (XS+30);
        public const int XS_UNSIGNED_INT = (XS+31);
        public const int XS_UNSIGNED_SHORT = (XS+32);
        public const int XS_UNSIGNED_BYTE = (XS+33);
        public const int XS_NORMALIZED_STRING = (XS+41);
        public const int XS_TOKEN = (XS+42);
        public const int XS_LANGUAGE = (XS+43);
        public const int XS_NMTOKEN = (XS+44);
        public const int XS_NMTOKENS = (XS+45);
        public const int XS_NAME = (XS+46);
        public const int XS_NCNAME = (XS+47);
        public const int XS_ID = (XS+48);
        public const int XS_IDREF = (XS+49);
        public const int XS_IDREFS = (XS+50);
        public const int XS_ENTITY = (XS+51);
        public const int XS_ENTITIES = (XS+52);
        public const int XS_DATE_TIME_STAMP = (XS+53);
        public const int XS_ANY_TYPE = (XS+60);
        public const int XS_ANY_SIMPLE_TYPE = (XS+61);
        public const int XS_ERROR = (XS+63);
        public const int XS_ALL = (XS+64);
        public const int XS_ALTERNATIVE = (XS+65);
        public const int XS_ANNOTATION = (XS+66);
        public const int XS_ANY = (XS+67);
        public const int XS_ANY_ATTRIBUTE = (XS+68);
        public const int XS_APPINFO = (XS+69);
        public const int XS_ASSERT = (XS+70);
        public const int XS_ASSERTION = (XS+71);
        public const int XS_ATTRIBUTE = (XS+72);
        public const int XS_ATTRIBUTE_GROUP = (XS+73);
        public const int XS_CHOICE = (XS+74);
        public const int XS_COMPLEX_CONTENT = (XS+75);
        public const int XS_COMPLEX_TYPE = (XS+76);
        public const int XS_DEFAULT_OPEN_CONTENT = (XS+77);
        public const int XS_DOCUMENTATION = (XS+78);
        public const int XS_ELEMENT = (XS+79);
        public const int XS_ENUMERATION = (XS+80);
        public const int XS_EXTENSION = (XS+81);
        public const int XS_FIELD = (XS+82);
        public const int XS_FRACTION_DIGITS = (XS+83);
        public const int XS_GROUP = (XS+84);
        public const int XS_IMPORT = (XS+85);
        public const int XS_INCLUDE = (XS+86);
        public const int XS_KEY = (XS+87);
        public const int XS_KEYREF = (XS+88);
        public const int XS_LENGTH = (XS+89);
        public const int XS_LIST = (XS+90);
        public const int XS_MAX_EXCLUSIVE = (XS+91);
        public const int XS_MAX_INCLUSIVE = (XS+92);
        public const int XS_MAX_LENGTH = (XS+93);
        public const int XS_MAX_SCALE = (XS+94);
        public const int XS_MIN_EXCLUSIVE = (XS+95);
        public const int XS_MIN_INCLUSIVE = (XS+96);
        public const int XS_MIN_LENGTH = (XS+97);
        public const int XS_MIN_SCALE = (XS+98);
        public const int XS_notation = (XS+99);
        public const int XS_OPEN_CONTENT = (XS+100);
        public const int XS_OVERRIDE = (XS+101);
        public const int XS_PATTERN = (XS+102);
        public const int XS_REDEFINE = (XS+103);
        public const int XS_RESTRICTION = (XS+104);
        public const int XS_SCHEMA = (XS+105);
        public const int XS_SELECTOR = (XS+106);
        public const int XS_SEQUENCE = (XS+107);
        public const int XS_SIMPLE_CONTENT = (XS+108);
        public const int XS_SIMPLE_TYPE = (XS+109);
        public const int XS_EXPLICIT_TIMEZONE = (XS+110);
        public const int XS_TOTAL_DIGITS = (XS+111);
        public const int XS_UNION = (XS+112);
        public const int XS_UNIQUE = (XS+113);
        public const int XS_WHITE_SPACE = (XS+114);
        public const int XS_UNTYPED = (XS+118);
        public const int XS_UNTYPED_ATOMIC = (XS+119);
        public const int XS_ANY_ATOMIC_TYPE = (XS+120);
        public const int XS_YEAR_MONTH_DURATION = (XS+121);
        public const int XS_DAY_TIME_DURATION = (XS+122);
        public const int XS_NUMERIC = (XS+123);
        public const int XSI_TYPE = (XSI+1);
        public const int XSI_NIL = (XSI+2);
        public const int XSI_SCHEMA_LOCATION = (XSI+3);
        public const int XSI_NO_NAMESPACE_SCHEMA_LOCATION = (XSI+4);
        public const int XSI_SCHEMA_LOCATION_TYPE = (XSI+5);
        private static readonly string[] localNames = new string[1023];
        private static readonly System.Collections.Generic.Dictionary<string,int> lookup = new System.Collections.Generic.Dictionary<string, int>(1023);
        public static Saxon.Hej.om.StructuredQName[] errorVariables = {new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "code"), new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "description"), new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "value"), new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "module"), new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "line-number"), new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "column-number"), new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "additional")};
        private StandardNames()  {
        }
        private static void bindXSLTName(int constant, string localName) {
            localNames[constant] = localName;
            Saxon.Impl.Helpers.MapUtils.Put(lookup, ((('{'+Saxon.Hej.lib.NamespaceConstant.XSLT)+'}')+localName), constant);
        }
        private static void bindSaxonName(int constant, string localName) {
            localNames[constant] = localName;
            Saxon.Impl.Helpers.MapUtils.Put(lookup, ((('{'+Saxon.Hej.lib.NamespaceConstant.SAXON)+'}')+localName), constant);
        }
        private static void bindXMLName(int constant, string localName) {
            localNames[constant] = localName;
            Saxon.Impl.Helpers.MapUtils.Put(lookup, ((('{'+Saxon.Hej.lib.NamespaceConstant.XML)+'}')+localName), constant);
        }
        private static void bindXSName(int constant, string localName) {
            localNames[constant] = localName;
            Saxon.Impl.Helpers.MapUtils.Put(lookup, ((('{'+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+'}')+localName), constant);
        }
        private static void bindXSIName(int constant, string localName) {
            localNames[constant] = localName;
            Saxon.Impl.Helpers.MapUtils.Put(lookup, ((('{'+Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE)+'}')+localName), constant);
        }
        public static int getFingerprint(string uri, string localName) {
            return Saxon.Impl.Helpers.MapUtils.GetOrDefault(lookup, ((('{'+uri)+'}')+localName), -1);
        }
        public static string getLocalName(int fingerprint) {
            return localNames[fingerprint];
        }
        public static string getURI(int fingerprint) {
            int c = (fingerprint>>7);
            switch (c) {
              case DFLT_NS:
                return "";
              case XSL_NS:
                return Saxon.Hej.lib.NamespaceConstant.XSLT;
              case SAXON_NS:
                return Saxon.Hej.lib.NamespaceConstant.SAXON;
              case XML_NS:
                return Saxon.Hej.lib.NamespaceConstant.XML;
              case XS_NS:
                return Saxon.Hej.lib.NamespaceConstant.SCHEMA;
              case XSI_NS:
                return Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown system fingerprint "+fingerprint));
            }
        }
        public static string getClarkName(int fingerprint) {
            string uri = getURI(fingerprint);
            if (uri.Length==0) {
                return getLocalName(fingerprint);
            } else {
                return ((('{'+uri)+'}')+getLocalName(fingerprint));
            }
        }
        public static string getPrefix(int fingerprint) {
            int c = (fingerprint>>7);
            switch (c) {
              case DFLT_NS:
                return "";
              case XSL_NS:
                return "xsl";
              case SAXON_NS:
                return "saxon";
              case XML_NS:
                return "xml";
              case XS_NS:
                return "xs";
              case XSI_NS:
                return "xsi";
              default:
                return null;
            }
        }
        public static string getDisplayName(int fingerprint) {
            if ((fingerprint == -1)) {
                return "(anonymous type)";
            }
            if ((fingerprint>1023)) {
                return (("("+fingerprint)+')');
            }
            if ((((fingerprint>>7)) == DFLT)) {
                return getLocalName(fingerprint);
            }
            return ((getPrefix(fingerprint)+':')+getLocalName(fingerprint));
        }
        public static Saxon.Hej.om.StructuredQName getStructuredQName(int fingerprint) {
            return new Saxon.Hej.om.StructuredQName(getPrefix(fingerprint), getURI(fingerprint), getLocalName(fingerprint));
        }
        public static Saxon.Hej.om.StructuredQName getUnprefixedQName(int fingerprint) {
            return new Saxon.Hej.om.StructuredQName("", getURI(fingerprint), getLocalName(fingerprint));
        }
        public readonly static Saxon.Hej.om.StructuredQName SQ_XS_INVALID_NAME = new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, "invalid-name");
    
    }
}