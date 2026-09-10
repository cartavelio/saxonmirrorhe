using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AlphaCode {
        private interface ParserCallBack<T>  {
            T makeContainer();
            void setStringProperty(T container, string key, string value);
            void setMultiStringProperty(T container, string key, System.Collections.Generic.IList<string> value);
            void setTypeProperty(T container, string key, T value);
            void setMultiTypeProperty(T container, string key, System.Collections.Generic.IList<T> value);
        
        }
        private class MapItemCallBack : Saxon.Hej.type.AlphaCode.ParserCallBack<Saxon.Hej.ma.map.DictionaryMap> {
            public Saxon.Hej.ma.map.DictionaryMap makeContainer() {
                return new Saxon.Hej.ma.map.DictionaryMap();
            }
            public void setStringProperty(Saxon.Hej.ma.map.DictionaryMap container, string key, string value) {
                container.initialPut(key, new Saxon.Hej.value.StringValue(value));
            }
            public void setMultiStringProperty(Saxon.Hej.ma.map.DictionaryMap container, string key, System.Collections.Generic.IList<string> value) {
                System.Collections.Generic.IList<Saxon.Hej.om.Item> xdmValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                foreach (string v in value) {
                    xdmValue.Add(new Saxon.Hej.value.StringValue(v));
                }
                container.initialPut(key, new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(xdmValue));
            }
            public void setTypeProperty(Saxon.Hej.ma.map.DictionaryMap container, string key, Saxon.Hej.ma.map.DictionaryMap value) {
                container.initialPut(key, value);
            }
            public void setMultiTypeProperty(Saxon.Hej.ma.map.DictionaryMap container, string key, System.Collections.Generic.IList<Saxon.Hej.ma.map.DictionaryMap> value) {
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> contents = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                foreach (Saxon.Hej.ma.map.DictionaryMap map in value) {
                    contents.Add(map);
                }
                container.initialPut(key, new Saxon.Hej.ma.arrays.SimpleArrayItem(contents));
            }
        
        }
        private class TreeCallBack : Saxon.Hej.type.AlphaCode.ParserCallBack<Saxon.Hej.type.AlphaCode.AlphaCodeTree> {
            public Saxon.Hej.type.AlphaCode.AlphaCodeTree makeContainer() {
                return new Saxon.Hej.type.AlphaCode.AlphaCodeTree();
            }
            public void setStringProperty(Saxon.Hej.type.AlphaCode.AlphaCodeTree tree, string key, string value) {
                switch (key) {
                  case "o":
                    tree.cardinality = value;
                    break ;
                  case "p":
                    tree.principal = value;
                    break ;
                  case "n":
                    tree.name = value;
                    break ;
                  case "c":
                    tree.content = value;
                    break ;
                  case "z":
                    tree.nillable = true;
                    break ;
                  case "x":
                    tree.extensibleTupleType = true;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Bad alphacode component "+key));
                }
            }
            public void setMultiStringProperty(Saxon.Hej.type.AlphaCode.AlphaCodeTree tree, string key, System.Collections.Generic.IList<string> value) {
                if (key.Equals("f")) {
                    tree.fieldNames = value;
                } else {
                    if (key.Equals("optionalFields")) {
                        tree.optionalFieldNames = new System.Collections.Generic.HashSet<string>();
                        Saxon.Impl.Helpers.SetUtils.AddAll(tree.optionalFieldNames, value);
                    } else {
                        throw new Saxon.Ejava.lang.IllegalArgumentException(("Bad alphacode component "+key));
                    }
                }
            }
            public void setTypeProperty(Saxon.Hej.type.AlphaCode.AlphaCodeTree tree, string key, Saxon.Hej.type.AlphaCode.AlphaCodeTree value) {
                switch (key) {
                  case "k":
                    tree.keyType = value;
                    break ;
                  case "v":
                    tree.valueType = value;
                    break ;
                  case "r":
                    tree.resultType = value;
                    break ;
                  case "e":
                    tree.elementType = value;
                    break ;
                  case "selfReference":
                    tree.selfReference = value;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Bad alphacode component "+key));
                }
            }
            public void setMultiTypeProperty(Saxon.Hej.type.AlphaCode.AlphaCodeTree tree, string key, System.Collections.Generic.IList<Saxon.Hej.type.AlphaCode.AlphaCodeTree> value) {
                switch (key) {
                  case "a":
                    tree.argTypes = value;
                    break ;
                  case "m":
                    tree.members = value;
                    break ;
                  case "i":
                    tree.vennOperands = value.ToArray();
                    tree.vennOperator = Saxon.Hej.expr.parser.Token.INTERSECT;
                    break ;
                  case "u":
                    tree.vennOperands = value.ToArray();
                    tree.vennOperator = Saxon.Hej.expr.parser.Token.UNION;
                    break ;
                  case "d":
                    tree.vennOperands = value.ToArray();
                    tree.vennOperator = Saxon.Hej.expr.parser.Token.EXCEPT;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Bad alphacode component "+key));
                }
            }
        
        }
        private class AlphaCodeParser<T>  {
            private readonly string input;
            private int position = 0;
            private readonly Saxon.Hej.type.AlphaCode.ParserCallBack<T> callBack;
            public AlphaCodeParser(string input, Saxon.Hej.type.AlphaCode.ParserCallBack<T> callBack)  {
                this.input = input;
                this.callBack = callBack;
            }
            private int nextChar() {
                if ((position>=input.Length)) {
                    return -1;
                }
                return input[position++];
            }
            private string nextToken() {
                int inBraces = 0;
                int start = position;
                while ((position<input.Length)) {
                    char ch = input[position++];
                    switch (ch) {
                      case '{':
                        inBraces++;
                        break ;
                      case '}':
                        inBraces--;
                        break ;
                      case ']':
                      case ',':
                        if ((inBraces == 0)) {
                            return input.Substring(start, --position - start);
                        }
                        break ;
                      case ' ':
                        if ((inBraces == 0)) {
                            return input.Substring(start, (position-1) - start);
                        }
                        break ;
                      default:
                        break;
                    }
                }
                return input.Substring(start, position - start);
            }
            private void expect(char c) {
                int d = nextChar();
                if ((d != c)) {
                    throw new Saxon.Ejava.lang.IllegalStateException((((("Expected '"+c)+"', found '")+(((d == -1) ? "<eof>" : ((""+(char)(d))))))+"'"));
                }
            }
            internal T parseType(T parent) {
                T container = callBack.makeContainer();
                int indicator = nextChar();
                if ((indicator<0)) {
                    callBack.setStringProperty(container, "o", "1");
                } else {
                    if ((("*+1?0\u00B0".IndexOf((char)(indicator))>=0))) {
                        if ((indicator == 0xB0)) {
                            indicator = '0';
                        }
                        callBack.setStringProperty(container, "o", ((""+(char)(indicator))));
                    } else {
                        callBack.setStringProperty(container, "o", "1");
                        position--;
                    }
                }
                string primary = nextToken();
                callBack.setStringProperty(container, "p", primary);
                if (primary.Equals("..")) {
                    callBack.setTypeProperty(container, "selfReference", parent);
                }
                while ((position<input.Length)) {
                    char c = input[position];
                    switch (c) {
                      case ']':
                      case ',':
                        return container;
                      case ' ':
                        position++;
                        break ;
                      case 'n':
                      case 'c':
                        position++;
                        string token = nextToken();
                        if (token.StartsWith("~")) {
                            token = ((("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}")+token.Substring(1));
                        }
                        if (((c == 'c') && token.EndsWith("?"))) {
                            callBack.setStringProperty(container, "z", "1");
                            token = token.Substring(0, (token.Length-1) - 0);
                        }
                        callBack.setStringProperty(container, (""+c), token);
                        break ;
                      case 'k':
                      case 'r':
                      case 'v':
                      case 'e':
                        position++;
                        expect('[');
                        T nestedType = parseType(parent);
                        expect(']');
                        callBack.setTypeProperty(container, (""+c), nestedType);
                        break ;
                      case 'a':
                      case 'm':
                      case 'i':
                      case 'u':
                      case 'd':
                        position++;
                        expect('[');
                        System.Collections.Generic.IList<T> nestedTypes = new System.Collections.Generic.List<T>();
                        if ((input[position] == ']')) {
                            position++;
                            callBack.setMultiTypeProperty(container, (""+c), nestedTypes);
                        } else {
                            while (true) {
                                nestedTypes.Add(parseType(container));
                                if ((input[position] == ',')) {
                                    position++;
                                } else {
                                    expect(']');
                                    callBack.setMultiTypeProperty(container, (""+c), nestedTypes);
                                    break ;
                                }
                            }
                        }
                        break ;
                      case 'f':
                      case 'F':
                        if ((c == 'F')) {
                            callBack.setStringProperty(container, "x", "1");
                        }
                        position++;
                        expect('[');
                        System.Collections.Generic.IList<string> fieldNames = new System.Collections.Generic.List<string>();
                        System.Collections.Generic.IList<string> optionalFieldNames = new System.Collections.Generic.List<string>();
                        Saxon.Ejava.lang.StringBuilder currName = new Saxon.Ejava.lang.StringBuilder();
                        bool escaped = false;
                        while (true) {
                            char ch = input[position++];
                            if (((ch == '\\') && !(escaped))) {
                                escaped = true;
                            } else {
                                if (((ch == '?') && !(escaped))) {
                                    optionalFieldNames.Add(currName.ToString());
                                } else {
                                    if (((ch == ',') && !(escaped))) {
                                        fieldNames.Add(currName.ToString());
                                        currName.setLength(0);
                                        escaped = false;
                                    } else {
                                        if (((ch == ']') && !(escaped))) {
                                            fieldNames.Add(currName.ToString());
                                            currName.setLength(0);
                                            callBack.setMultiStringProperty(container, "f", fieldNames);
                                            callBack.setMultiStringProperty(container, "optionalFields", optionalFieldNames);
                                            break ;
                                        } else {
                                            currName.append(ch);
                                            escaped = false;
                                        }
                                    }
                                }
                            }
                        }
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalStateException((("Expected one of n|c|t|k|r|v|a|u, found '"+c)+"'"));
                    }
                }
                return container;
            }
        
        }
        public static Saxon.Hej.ma.map.MapItem toXdmMap(string input) {
            Saxon.Hej.type.AlphaCode.MapItemCallBack callBack = new Saxon.Hej.type.AlphaCode.MapItemCallBack();
            Saxon.Hej.type.AlphaCode.AlphaCodeParser<Saxon.Hej.ma.map.DictionaryMap> parser = new Saxon.Hej.type.AlphaCode.AlphaCodeParser<Saxon.Hej.ma.map.DictionaryMap>(input, callBack);
            return parser.parseType(null);
        }
        public static string fromXdmMap(Saxon.Hej.ma.map.MapItem map) {
            Saxon.Ejava.lang.StringBuilder @out = new Saxon.Ejava.lang.StringBuilder();
            Saxon.Hej.value.StringValue indicator = (Saxon.Hej.value.StringValue)(map.get(Saxon.Hej.value.StringValue.bmp("o")));
            @out.append(((indicator == null) ? "1" : indicator.getStringValue()));
            Saxon.Hej.value.StringValue alphaCode = (Saxon.Hej.value.StringValue)(map.get(Saxon.Hej.value.StringValue.bmp("p")));
            @out.append(((alphaCode == null) ? "" : alphaCode.getStringValue()));
            @out.append(" ");
            foreach (Saxon.Hej.ma.map.KeyValuePair kvp in map.keyValuePairs()) {
                string key = kvp.key.getStringValue();
                switch (key) {
                  case "o":
                  case "p":
                    break ;
                  case "n":
                  case "c":
                  case "t":
                    @out.append(key);
                    @out.append(((Saxon.Hej.value.StringValue)(kvp.value)).getStringValue());
                    @out.append(" ");
                    break ;
                  case "k":
                  case "r":
                  case "v":
                  case "e":
                    @out.append(key);
                    @out.append('[');
                    @out.append(fromXdmMap((Saxon.Hej.ma.map.MapItem)(kvp.value)));
                    @out.append(']');
                    @out.append(" ");
                    break ;
                  case "a":
                  case "u":
                    @out.append(key);
                    @out.append('[');
                    Saxon.Hej.ma.arrays.ArrayItem types = (Saxon.Hej.ma.arrays.ArrayItem)(kvp.value);
                    bool first = true;
                    foreach (Saxon.Hej.om.GroundedValue t in types.members()) {
                        if (first) {
                            first = false;
                        } else {
                            @out.append(",");
                        }
                        @out.append(fromXdmMap((Saxon.Hej.ma.map.MapItem)(t)));
                    }
                    @out.append(']');
                    @out.append(" ");
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException((("Unexpected key '"+key)+"'"));
                }
            }
            return @out.ToString();
        }
        private class AlphaCodeTree {
            internal string cardinality;
            internal string principal;
            internal string name;
            internal string content;
            internal bool nillable;
            internal System.Collections.Generic.IList<Saxon.Hej.type.AlphaCode.AlphaCodeTree> members;
            internal Saxon.Hej.type.AlphaCode.AlphaCodeTree keyType;
            internal Saxon.Hej.type.AlphaCode.AlphaCodeTree valueType;
            internal Saxon.Hej.type.AlphaCode.AlphaCodeTree resultType;
            internal System.Collections.Generic.IList<Saxon.Hej.type.AlphaCode.AlphaCodeTree> argTypes;
            internal Saxon.Hej.type.AlphaCode.AlphaCodeTree elementType;
            internal int vennOperator;
            internal Saxon.Hej.type.AlphaCode.AlphaCodeTree[] vennOperands;
            internal System.Collections.Generic.IList<string> fieldNames;
            internal System.Collections.Generic.ISet<string> optionalFieldNames;
            internal bool extensibleTupleType;
            internal Saxon.Hej.type.AlphaCode.AlphaCodeTree selfReference;
        
        }
        public static Saxon.Hej.value.SequenceType toSequenceType(string input, Saxon.Hej.Configuration config) {
            Saxon.Hej.type.AlphaCode.TreeCallBack callBack = new Saxon.Hej.type.AlphaCode.TreeCallBack();
            Saxon.Hej.type.AlphaCode.AlphaCodeParser<Saxon.Hej.type.AlphaCode.AlphaCodeTree> parser = new Saxon.Hej.type.AlphaCode.AlphaCodeParser<Saxon.Hej.type.AlphaCode.AlphaCodeTree>(input, callBack);
            Saxon.Hej.type.AlphaCode.AlphaCodeTree tree = parser.parseType(null);
            return sequenceTypeFromTree(tree, config);
        }
        public static Saxon.Hej.type.ItemType toItemType(string input, Saxon.Hej.Configuration config) {
            Saxon.Hej.value.SequenceType st = toSequenceType(input, config);
            if ((st.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Supplied alphacode has a cardinality other than 1");
            }
            return st.getPrimaryType();
        }
        private static Saxon.Hej.value.SequenceType sequenceTypeFromTree(Saxon.Hej.type.AlphaCode.AlphaCodeTree tree, Saxon.Hej.Configuration config) {
            string principal = tree.principal;
            Saxon.Hej.type.ItemType itemType = null;
            if (principal.Length==0) {
                itemType = Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                if (principal.StartsWith("A")) {
                    Saxon.Hej.type.BuiltInAtomicType builtIn = Saxon.Hej.type.BuiltInAtomicType.fromAlphaCode(principal);
                    if ((builtIn == null)) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown type "+principal));
                    }
                    itemType = builtIn;
                    if ((tree.name != null)) {
                        Saxon.Hej.type.SchemaType type = config.getSchemaType(Saxon.Hej.om.StructuredQName.fromEQName(tree.name));
                        if (!(((type is Saxon.Hej.type.PlainType)))) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException((("Schema type "+tree.name)+" is not known"));
                        }
                        itemType = (Saxon.Hej.type.PlainType)(type);
                    } else {
                        if (((builtIn == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) && (tree.members != null))) {
                            System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> members = new System.Collections.Generic.List<Saxon.Hej.type.AtomicType>();
                            foreach (Saxon.Hej.type.AlphaCode.AlphaCodeTree m in tree.members) {
                                Saxon.Hej.value.SequenceType st = sequenceTypeFromTree(m, config);
                                if (st.getPrimaryType().isAtomicType()) {
                                    Saxon.Hej.type.AtomicType primaryType = (Saxon.Hej.type.AtomicType)(st.getPrimaryType());
                                    members.Add(primaryType);
                                }
                            }
                            itemType = new Saxon.Hej.type.LocalUnionType(members);
                        }
                    }
                } else {
                    if (principal.StartsWith("N")) {
                        string contentName = tree.content;
                        Saxon.Hej.om.StructuredQName contentQName;
                        Saxon.Hej.pattern.ContentTypeTest contentTest = null;
                        bool nillable = tree.nillable;
                        if ((contentName != null)) {
                            contentQName = Saxon.Hej.om.StructuredQName.fromEQName(contentName);
                            Saxon.Hej.type.SchemaType contentType = config.getSchemaType(contentQName);
                            if ((contentType == null)) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown type "+contentName));
                            }
                            contentTest = new Saxon.Hej.pattern.ContentTypeTest((principal.Equals("NE") ? Saxon.Hej.type.Type.ELEMENT : Saxon.Hej.type.Type.ATTRIBUTE), contentType, config, nillable);
                        }
                        if ((tree.vennOperands != null)) {
                            if ((tree.vennOperands.Length == 2)) {
                                Saxon.Hej.pattern.NodeTest nt0 = (Saxon.Hej.pattern.NodeTest)(sequenceTypeFromTree(tree.vennOperands[0], config).getPrimaryType());
                                Saxon.Hej.pattern.NodeTest nt1 = (Saxon.Hej.pattern.NodeTest)(sequenceTypeFromTree(tree.vennOperands[1], config).getPrimaryType());
                                itemType = new Saxon.Hej.pattern.CombinedNodeTest(nt0, tree.vennOperator, nt1);
                            } else {
                                System.Diagnostics.Debug.Assert((tree.vennOperator == Saxon.Hej.expr.parser.Token.UNION));
                                Saxon.Hej.type.UType u = Saxon.Hej.type.UType.VOID;
                                for (int i = 0; (i<tree.vennOperands.Length); i++) {
                                    Saxon.Hej.type.ItemType it = sequenceTypeFromTree(tree.vennOperands[i], config).getPrimaryType();
                                    System.Diagnostics.Debug.Assert((it is Saxon.Hej.pattern.NodeKindTest));
                                    u = u.union(it.getUType());
                                }
                                itemType = new Saxon.Hej.pattern.MultipleNodeKindTest(u);
                            }
                        } else {
                            int kind = Saxon.Hej.type.Type.NODE;
                            if ((principal.Length>=2)) {
                                switch (principal.Substring(0, 2 - 0)) {
                                  case "NT":
                                    kind = Saxon.Hej.type.Type.TEXT;
                                    break ;
                                  case "NC":
                                    kind = Saxon.Hej.type.Type.COMMENT;
                                    break ;
                                  case "NN":
                                    kind = Saxon.Hej.type.Type.NAMESPACE;
                                    break ;
                                  case "NP":
                                    kind = Saxon.Hej.type.Type.PROCESSING_INSTRUCTION;
                                    break ;
                                  case "ND":
                                    kind = Saxon.Hej.type.Type.DOCUMENT;
                                    break ;
                                  case "NE":
                                    kind = Saxon.Hej.type.Type.ELEMENT;
                                    break ;
                                  case "NA":
                                    kind = Saxon.Hej.type.Type.ATTRIBUTE;
                                    break ;
                                }
                            }
                            string name = tree.name;
                            Saxon.Hej.pattern.QNameTest partialNameTest = null;
                            if (((name != null) && name.Contains("*"))) {
                                if (name.StartsWith("*:")) {
                                    partialNameTest = new Saxon.Hej.pattern.LocalNameTest(config.getNamePool(), kind, name.Substring(2));
                                } else {
                                    if (name.EndsWith("}*")) {
                                        string uri = name.Substring(2, (name.Length-2) - 2);
                                        partialNameTest = new Saxon.Hej.pattern.NamespaceTest(config.getNamePool(), kind, Saxon.Hej.om.NamespaceUri.of(uri));
                                    }
                                }
                            }
                            if ((partialNameTest != null)) {
                                itemType = (Saxon.Hej.pattern.NodeTest)(partialNameTest);
                            } else {
                                Saxon.Hej.om.StructuredQName qName = ((name == null) ? null : Saxon.Hej.om.StructuredQName.fromEQName(name));
                                switch (principal) {
                                  case "N":
                                    itemType = Saxon.Hej.pattern.AnyNodeTest.getInstance();
                                    break ;
                                  case "NT":
                                    itemType = Saxon.Hej.pattern.NodeKindTest.TEXT;
                                    break ;
                                  case "NC":
                                    itemType = Saxon.Hej.pattern.NodeKindTest.COMMENT;
                                    break ;
                                  case "NN":
                                    if ((name == null)) {
                                        itemType = Saxon.Hej.pattern.NodeKindTest.NAMESPACE;
                                    } else {
                                        itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.NAMESPACE, Saxon.Hej.om.NamespaceUri.NULL, qName.getLocalPart(), config.getNamePool());
                                    }
                                    break ;
                                  case "NP":
                                    if ((name == null)) {
                                        itemType = Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION;
                                    } else {
                                        itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION, Saxon.Hej.om.NamespaceUri.NULL, qName.getLocalPart(), config.getNamePool());
                                    }
                                    break ;
                                  case "ND":
                                    Saxon.Hej.type.AlphaCode.AlphaCodeTree elementType = tree.elementType;
                                    if ((elementType == null)) {
                                        itemType = Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
                                    } else {
                                        Saxon.Hej.type.ItemType e = sequenceTypeFromTree(elementType, config).getPrimaryType();
                                        itemType = new Saxon.Hej.pattern.DocumentNodeTest((Saxon.Hej.pattern.NodeTest)(e));
                                    }
                                    break ;
                                  case "NE":
                                    if ((qName == null)) {
                                        if ((contentTest == null)) {
                                            itemType = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                                        } else {
                                            itemType = contentTest;
                                        }
                                    } else {
                                        itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, qName.getNamespaceUri(), qName.getLocalPart(), config.getNamePool());
                                        if ((contentTest != null)) {
                                            itemType = new Saxon.Hej.pattern.CombinedNodeTest((Saxon.Hej.pattern.NodeTest)(itemType), Saxon.Hej.expr.parser.Token.INTERSECT, contentTest);
                                        }
                                    }
                                    break ;
                                  case "NA":
                                    if ((qName == null)) {
                                        if ((contentTest == null)) {
                                            itemType = Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
                                        } else {
                                            itemType = contentTest;
                                        }
                                    } else {
                                        itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, qName.getNamespaceUri(), qName.getLocalPart(), config.getNamePool());
                                        if ((contentTest != null)) {
                                            itemType = new Saxon.Hej.pattern.CombinedNodeTest((Saxon.Hej.pattern.NodeTest)(itemType), Saxon.Hej.expr.parser.Token.INTERSECT, contentTest);
                                        }
                                    }
                                    break ;
                                  case "NES":
                                    {
                                        System.Diagnostics.Debug.Assert((qName != null));
                                        Saxon.Hej.type.SchemaDeclaration decl = config.getElementDeclaration(qName);
                                        if ((decl != null)) {
                                            try {
                                                itemType = decl.makeSchemaNodeTest();
                                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                            }
                                        }
                                        if ((itemType == null)) {
                                            itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, qName.getNamespaceUri(), qName.getLocalPart(), config.getNamePool());
                                        }
                                        break ;
                                    }
                                  case "NAS":
                                    {
                                        System.Diagnostics.Debug.Assert((qName != null));
                                        Saxon.Hej.type.SchemaDeclaration decl = config.getAttributeDeclaration(qName);
                                        if ((decl != null)) {
                                            try {
                                                itemType = decl.makeSchemaNodeTest();
                                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                            }
                                        }
                                        if ((itemType == null)) {
                                            itemType = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, qName.getNamespaceUri(), qName.getLocalPart(), config.getNamePool());
                                        }
                                        break ;
                                    }
                                  default:
                                    itemType = Saxon.Hej.pattern.AnyNodeTest.getInstance();
                                    break ;
                                }
                            }
                        }
                    } else {
                        if (principal.StartsWith("F")) {
                            if (principal.Equals("FA")) {
                                Saxon.Hej.type.AlphaCode.AlphaCodeTree valueType = tree.valueType;
                                if ((valueType == null)) {
                                    itemType = Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE;
                                } else {
                                    itemType = new Saxon.Hej.ma.arrays.ArrayItemType(sequenceTypeFromTree(valueType, config));
                                }
                            } else {
                                if (principal.Equals("FM")) {
                                    if ((tree.fieldNames == null)) {
                                        Saxon.Hej.type.AlphaCode.AlphaCodeTree keyType = tree.keyType;
                                        Saxon.Hej.type.AlphaCode.AlphaCodeTree valueType = tree.valueType;
                                        if (((keyType != null) && (valueType != null))) {
                                            Saxon.Hej.type.AtomicType a = (Saxon.Hej.type.AtomicType)(sequenceTypeFromTree(keyType, config).getPrimaryType());
                                            Saxon.Hej.value.SequenceType v = sequenceTypeFromTree(valueType, config);
                                            itemType = new Saxon.Hej.ma.map.MapType(a, v);
                                        } else {
                                            itemType = Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE;
                                        }
                                    } else {
                                        System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> fieldTypes = new System.Collections.Generic.List<Saxon.Hej.value.SequenceType>(tree.argTypes.Count);
                                        Saxon.Hej.ma.map.RecordTest recordTest = new Saxon.Hej.ma.map.RecordTest();
                                        foreach (Saxon.Hej.type.AlphaCode.AlphaCodeTree t in tree.argTypes) {
                                            if ((t.selfReference != null)) {
                                                Saxon.Hej.ma.map.SelfReferenceRecordTest selfie = new Saxon.Hej.ma.map.SelfReferenceRecordTest(recordTest);
                                                int xcardinality = Saxon.Hej.value.Cardinality.fromOccurrenceIndicator(tree.cardinality);
                                                fieldTypes.Add(Saxon.Hej.value.SequenceType.makeSequenceType(selfie, xcardinality));
                                            } else {
                                                fieldTypes.Add(sequenceTypeFromTree(t, config));
                                            }
                                        }
                                        recordTest.setDetails(tree.fieldNames, fieldTypes, tree.optionalFieldNames, tree.extensibleTupleType);
                                        itemType = recordTest;
                                    }
                                } else {
                                    Saxon.Hej.type.AlphaCode.AlphaCodeTree returnType = tree.resultType;
                                    System.Collections.Generic.IList<Saxon.Hej.type.AlphaCode.AlphaCodeTree> argTypes = tree.argTypes;
                                    if ((argTypes == null)) {
                                        itemType = Saxon.Hej.type.AnyFunctionType.getInstance();
                                    } else {
                                        Saxon.Hej.value.SequenceType r;
                                        if ((returnType == null)) {
                                            r = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                                        } else {
                                            r = sequenceTypeFromTree(returnType, config);
                                        }
                                        Saxon.Hej.value.SequenceType[] a = new Saxon.Hej.value.SequenceType[argTypes.Count];
                                        for (int i = 0; (i<a.Length); i++) {
                                            a[i] = sequenceTypeFromTree(argTypes[i], config);
                                        }
                                        itemType = new Saxon.Hej.type.SpecificFunctionType(a, r);
                                    }
                                }
                            }
                        } else {
                            if (principal.StartsWith("X")) {
                                System.Type theClass = typeof(System.Object);
                                itemType = Saxon.Hej.type.JavaExternalObjectType.of(theClass);
                            }
                        }
                    }
                }
            }
            string indicator = tree.cardinality;
            int cardinality = Saxon.Hej.value.Cardinality.fromOccurrenceIndicator(indicator);
            return Saxon.Hej.value.SequenceType.makeSequenceType(itemType, cardinality);
        }
        private static Saxon.Hej.type.AlphaCode.AlphaCodeTree makeTree(Saxon.Hej.value.SequenceType sequenceType) {
            Saxon.Hej.type.AlphaCode.AlphaCodeTree tree = makeTree(sequenceType.getPrimaryType());
            if ((sequenceType.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                tree.cardinality = Saxon.Hej.value.Cardinality.getOccurrenceIndicator(sequenceType.getCardinality());
            }
            return tree;
        }
        private static Saxon.Hej.type.AlphaCode.AlphaCodeTree makeTree(Saxon.Hej.type.ItemType primary) {
            Saxon.Hej.type.AlphaCode.AlphaCodeTree result = new Saxon.Hej.type.AlphaCode.AlphaCodeTree();
            result.principal = primary.getBasicAlphaCode();
            result.cardinality = "1";
            if (((primary is Saxon.Hej.type.AtomicType) && !(((Saxon.Hej.type.AtomicType)(primary)).isBuiltInType()))) {
                result.name = ((Saxon.Hej.type.AtomicType)(primary)).getEQName();
            } else {
                if ((primary is Saxon.Hej.type.UnionType)) {
                    Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.type.UnionType)(primary)).getTypeName();
                    if (name.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA)) {
                        result.name = ("~"+name.getLocalPart());
                    } else {
                        if (name.hasURI(Saxon.Hej.om.NamespaceUri.ANONYMOUS)) {
                            try {
                                System.Collections.Generic.IList<Saxon.Hej.type.AlphaCode.AlphaCodeTree> memberMaps = new System.Collections.Generic.List<Saxon.Hej.type.AlphaCode.AlphaCodeTree>();
                                foreach (Saxon.Hej.type.PlainType pt in ((Saxon.Hej.type.UnionType)(primary)).getPlainMemberTypes()) {
                                    memberMaps.Add(makeTree(pt));
                                }
                                result.members = memberMaps;
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                            }
                        } else {
                            result.name = name.getEQName();
                        }
                    }
                } else {
                    if ((primary is Saxon.Hej.pattern.NameTest)) {
                        Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.pattern.NameTest)(primary)).getMatchingNodeName();
                        result.name = name.getEQName();
                    } else {
                        if ((primary is Saxon.Hej.pattern.SchemaNodeTest)) {
                            Saxon.Hej.om.StructuredQName name = ((Saxon.Hej.pattern.SchemaNodeTest)(primary)).getNodeName();
                            result.name = name.getEQName();
                        } else {
                            if ((primary is Saxon.Hej.pattern.LocalNameTest)) {
                                result.name = ("*:"+((Saxon.Hej.pattern.LocalNameTest)(primary)).getLocalName());
                            } else {
                                if ((primary is Saxon.Hej.pattern.NamespaceTest)) {
                                    result.name = (("Q{"+((Saxon.Hej.pattern.NamespaceTest)(primary)).getNamespaceURI())+"}*");
                                } else {
                                    if ((primary is Saxon.Hej.pattern.CombinedNodeTest)) {
                                        Saxon.Hej.pattern.CombinedNodeTest combi = (Saxon.Hej.pattern.CombinedNodeTest)(primary);
                                        string c = combi.getContentTypeForAlphaCode();
                                        if ((c != null)) {
                                            if (!(c.StartsWith(("Q{"+Saxon.Hej.lib.NamespaceConstant.ANONYMOUS)))) {
                                                result.content = c;
                                            }
                                            result.name = combi.getMatchingNodeName().getEQName();
                                            result.nillable = combi.isNillable();
                                        } else {
                                            result.vennOperator = combi.getOperator();
                                            result.vennOperands = new Saxon.Hej.type.AlphaCode.AlphaCodeTree[2];
                                            result.vennOperands[0] = makeTree(combi.getOperand(0));
                                            result.vennOperands[1] = makeTree(combi.getOperand(1));
                                        }
                                    } else {
                                        if ((primary is Saxon.Hej.pattern.MultipleNodeKindTest)) {
                                            result.vennOperator = Saxon.Hej.expr.parser.Token.UNION;
                                            System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> types = primary.getUType().decompose();
                                            result.vennOperands = new Saxon.Hej.type.AlphaCode.AlphaCodeTree[types.Count];
                                            int i = 0;
                                            foreach (Saxon.Hej.type.PrimitiveUType type in types) {
                                                result.vennOperands[i++] = makeTree(type.toItemType());
                                            }
                                        } else {
                                            if ((primary is Saxon.Hej.pattern.ContentTypeTest)) {
                                                result.content = ((Saxon.Hej.pattern.ContentTypeTest)(primary)).getContentType().getEQName();
                                            } else {
                                                if ((primary is Saxon.Hej.pattern.DocumentNodeTest)) {
                                                    Saxon.Hej.type.ItemType content = ((Saxon.Hej.pattern.DocumentNodeTest)(primary)).getElementTest();
                                                    result.elementType = makeTree(content);
                                                } else {
                                                    if ((primary is Saxon.Hej.type.FunctionItemType)) {
                                                        if ((primary is Saxon.Hej.ma.arrays.ArrayItemType)) {
                                                            Saxon.Hej.value.SequenceType memberType = ((Saxon.Hej.ma.arrays.ArrayItemType)(primary)).getMemberType();
                                                            if ((memberType != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                                                                result.valueType = makeTree(memberType);
                                                            }
                                                        } else {
                                                            if ((primary is Saxon.Hej.ma.map.RecordTest)) {
                                                                result.extensibleTupleType = ((Saxon.Hej.ma.map.RecordTest)(primary)).isExtensible();
                                                                result.optionalFieldNames = new System.Collections.Generic.HashSet<string>();
                                                                result.fieldNames = new System.Collections.Generic.List<string>();
                                                                result.argTypes = new System.Collections.Generic.List<Saxon.Hej.type.AlphaCode.AlphaCodeTree>();
                                                                foreach (string s in ((Saxon.Hej.ma.map.RecordTest)(primary)).getFieldNames()) {
                                                                    result.fieldNames.Add(s);
                                                                    Saxon.Hej.value.SequenceType fieldType = ((Saxon.Hej.ma.map.RecordTest)(primary)).getFieldType(s);
                                                                    if ((fieldType.getPrimaryType() is Saxon.Hej.ma.map.SelfReferenceRecordTest)) {
                                                                        Saxon.Hej.type.AlphaCode.AlphaCodeTree selfRef = new Saxon.Hej.type.AlphaCode.AlphaCodeTree();
                                                                        selfRef.selfReference = result;
                                                                        selfRef.cardinality = ((fieldType.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) ? "1" : Saxon.Hej.value.Cardinality.getOccurrenceIndicator(fieldType.getCardinality()));
                                                                        result.argTypes.Add(selfRef);
                                                                    } else {
                                                                        result.argTypes.Add(makeTree(((Saxon.Hej.ma.map.RecordTest)(primary)).getFieldType(s)));
                                                                    }
                                                                    if (((Saxon.Hej.ma.map.RecordTest)(primary)).isOptionalField(s)) {
                                                                        result.optionalFieldNames.Add(s);
                                                                    }
                                                                }
                                                            } else {
                                                                if ((primary is Saxon.Hej.ma.map.MapType)) {
                                                                    Saxon.Hej.type.AtomicType keyType = ((Saxon.Hej.ma.map.MapType)(primary)).getKeyType();
                                                                    if ((keyType != Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                                                                        result.keyType = makeTree(keyType);
                                                                    }
                                                                    Saxon.Hej.value.SequenceType valueType = ((Saxon.Hej.ma.map.MapType)(primary)).getValueType();
                                                                    if ((valueType != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                                                                        result.valueType = makeTree(valueType);
                                                                    }
                                                                } else {
                                                                    Saxon.Hej.value.SequenceType resultType = ((Saxon.Hej.type.FunctionItemType)(primary)).getResultType();
                                                                    if ((resultType != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                                                                        result.resultType = makeTree(resultType);
                                                                    }
                                                                    Saxon.Hej.value.SequenceType[] argTypes = ((Saxon.Hej.type.FunctionItemType)(primary)).getArgumentTypes();
                                                                    if ((argTypes != null)) {
                                                                        System.Collections.Generic.IList<Saxon.Hej.type.AlphaCode.AlphaCodeTree> argMaps = new System.Collections.Generic.List<Saxon.Hej.type.AlphaCode.AlphaCodeTree>();
                                                                        foreach (Saxon.Hej.value.SequenceType at in argTypes) {
                                                                            argMaps.Add(makeTree(at));
                                                                        }
                                                                        result.argTypes = argMaps;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    } else {
                                                        if ((primary is Saxon.Hej.type.ExternalObjectType)) {
                                                            result.name = ((Saxon.Hej.type.ExternalObjectType)(primary)).getName();
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
                }
            }
            return result;
        }
        private static string abbreviateEQName(string @in) {
            if (@in.StartsWith((("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}"))) {
                return ("~"+@in.Substring(((("Q{"+Saxon.Hej.lib.NamespaceConstant.SCHEMA)+"}")).Length));
            } else {
                return @in;
            }
        }
        private static void alphaCodeFromTree(Saxon.Hej.type.AlphaCode.AlphaCodeTree tree, bool withCardinality, Saxon.Ejava.lang.StringBuilder sb) {
            if (withCardinality) {
                sb.append(tree.cardinality);
            }
            if ((tree.selfReference != null)) {
                sb.append("..");
                return ;
            }
            sb.append(tree.principal);
            if ((tree.name != null)) {
                sb.append(" n").append(abbreviateEQName(tree.name));
            }
            if ((tree.content != null)) {
                sb.append(" c").append(abbreviateEQName(tree.content));
                if (tree.nillable) {
                    sb.append("?");
                }
            }
            if ((tree.keyType != null)) {
                sb.append(" k[");
                alphaCodeFromTree(tree.keyType, false, sb);
                sb.append("]");
            }
            if ((tree.valueType != null)) {
                sb.append(" v[");
                alphaCodeFromTree(tree.valueType, true, sb);
                sb.append("]");
            }
            if ((tree.resultType != null)) {
                sb.append(" r[");
                alphaCodeFromTree(tree.resultType, true, sb);
                sb.append("]");
            }
            if ((tree.argTypes != null)) {
                sb.append(" a[");
                bool first = true;
                foreach (Saxon.Hej.type.AlphaCode.AlphaCodeTree a in tree.argTypes) {
                    if (first) {
                        first = false;
                    } else {
                        sb.append(",");
                    }
                    alphaCodeFromTree(a, true, sb);
                }
                sb.append("]");
            }
            if ((tree.members != null)) {
                sb.append(" m[");
                bool first = true;
                foreach (Saxon.Hej.type.AlphaCode.AlphaCodeTree a in tree.members) {
                    if (first) {
                        first = false;
                    } else {
                        sb.append(",");
                    }
                    alphaCodeFromTree(a, false, sb);
                }
                sb.append("]");
            }
            if ((tree.elementType != null)) {
                sb.append(" e[");
                alphaCodeFromTree(tree.elementType, false, sb);
                sb.append("]");
            }
            if ((tree.vennOperands != null)) {
                string @operator = ((tree.vennOperator == Saxon.Hej.expr.parser.Token.INTERSECT) ? "i" : ((tree.vennOperator == Saxon.Hej.expr.parser.Token.UNION) ? "u" : "d"));
                sb.append(" ").append(@operator).append("[");
                for (int i = 0; (i<tree.vennOperands.Length); i++) {
                    if ((i != 0)) {
                        sb.append(",");
                    }
                    alphaCodeFromTree(tree.vennOperands[i], false, sb);
                }
                sb.append("]");
            }
            if ((tree.fieldNames != null)) {
                sb.append((tree.extensibleTupleType ? " F[" : " f["));
                bool first = true;
                foreach (string s in tree.fieldNames) {
                    if (!(first)) {
                        sb.append(",");
                    } else {
                        first = false;
                    }
                    sb.append(escapeNCName(s));
                    if (tree.optionalFieldNames.Contains(s)) {
                        sb.append('?');
                    }
                }
                sb.append("]");
            }
        }
        private static string escapeNCName(string ncName) {
            if (Saxon.Hej.om.NameChecker.isValidNCName(ncName)) {
                return ncName;
            } else {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
                for (int i = 0; (i<ncName.Length); i++) {
                    char c = ncName[i];
                    if (((c>=128) || Saxon.Hej.om.NameChecker.isNCNameChar(c))) {
                        sb.append(c);
                    } else {
                        sb.append("\\").append(c);
                    }
                }
                return sb.ToString();
            }
        }
        public static string fromItemType(Saxon.Hej.type.ItemType type) {
            Saxon.Hej.type.AlphaCode.AlphaCodeTree tree = makeTree(type);
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            alphaCodeFromTree(tree, false, sb);
            return sb.ToString().Trim();
        }
        public static string fromSequenceType(Saxon.Hej.value.SequenceType type) {
            if ((type == Saxon.Hej.value.SequenceType.EMPTY_SEQUENCE)) {
                return "0";
            }
            string s = fromItemType(type.getPrimaryType());
            if ((type.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                return ("1"+s);
            } else {
                return (Saxon.Hej.value.Cardinality.getOccurrenceIndicator(type.getCardinality())+s);
            }
        }
    
    }
}