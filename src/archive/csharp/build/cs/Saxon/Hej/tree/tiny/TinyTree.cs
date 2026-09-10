using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public sealed class TinyTree : Saxon.Hej.om.GenericTreeInfo, Saxon.Hej.tree.tiny.NodeVectorTree {
        private static readonly string[] EMPTY_STRING_ARRAY = new string[0];
        internal Saxon.Hej.str.LargeTextBuffer textBuffer;
        internal Saxon.Hej.str.UnicodeString commentBuffer = null;
        internal int numberOfNodes = 0;
        public byte[] nodeKind;
        internal short[] depth;
        internal int[] next;
        internal int[] alpha;
        internal int[] beta;
        internal int[] nameCode;
        internal int[] prior = null;
        internal Saxon.Hej.type.SchemaType[] typeArray = null;
        internal Saxon.Hej.om.AtomicSequence[] typedValueArray = null;
        internal Saxon.Hej.z.IntSet idRefElements = null;
        internal Saxon.Hej.z.IntSet idRefAttributes = null;
        internal Saxon.Hej.z.IntSet nilledElements = null;
        internal Saxon.Hej.z.IntSet defaultedAttributes = null;
        internal Saxon.Hej.z.IntSet topWithinEntity = null;
        private bool allowTypedValueCache = true;
        private System.Collections.Generic.IDictionary<string,Saxon.Hej.z.IntSet> localNameIndex = null;
        public const int TYPECODE_IDREF = (1<<29);
        internal int numberOfAttributes = 0;
        internal int[] attParent;
        internal int[] attCode;
        internal string[] attValue;
        internal Saxon.Hej.om.AtomicSequence[] attTypedValue;
        internal Saxon.Hej.type.SimpleType[] attType;
        internal int numberOfNamespaces = 0;
        internal Saxon.Hej.om.NamespaceMap[] namespaceMaps;
        private int[] lineNumbers = null;
        private int[] columnNumbers = null;
        private Saxon.Hej.tree.linked.SystemIdMap systemIdMap = null;
        internal bool usesNamespaces = false;
        internal Saxon.Hej.om.PrefixPool prefixPool = new Saxon.Hej.om.PrefixPool();
        private Saxon.Hej.tree.tiny.TinyDocumentImpl documentRoot;
        private System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NodeInfo> idTable;
        internal System.Collections.Generic.Dictionary<string,string[]> entityTable;
        private Saxon.Hej.om.NodeInfo copiedFrom;
        internal Saxon.Hej.z.IntHashMap<string> knownBaseUris;
        private string uniformBaseUri = null;
        public TinyTree(Saxon.Hej.Configuration config, Saxon.Hej.tree.tiny.Statistics statistics) : base (config) {
            int nodes = ((int)(statistics.getAverageNodes())+1);
            int attributes = ((int)(statistics.getAverageAttributes())+1);
            int namespaces = ((int)(statistics.getAverageNamespaces())+1);
            int characters = ((int)(statistics.getAverageCharacters())+1);
            nodeKind = new byte[nodes];
            depth = new short[nodes];
            next = new int[nodes];
            alpha = new int[nodes];
            beta = new int[nodes];
            nameCode = new int[nodes];
            numberOfAttributes = 0;
            attParent = new int[attributes];
            attCode = new int[attributes];
            attValue = new string[attributes];
            numberOfNamespaces = 0;
            namespaceMaps = new Saxon.Hej.om.NamespaceMap[namespaces];
            textBuffer = new Saxon.Hej.str.LargeTextBuffer();
            setConfiguration(config);
        }
        public override void setConfiguration(Saxon.Hej.Configuration config) {
            base.setConfiguration(config);
            allowTypedValueCache = (config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION) && config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.USE_TYPED_VALUE_CACHE));
        }
        private void ensureNodeCapacity(short kind, int needed) {
            if ((nodeKind.Length<(numberOfNodes+needed))) {
                int k = ((kind == Saxon.Hej.type.Type.STOPPER) ? (numberOfNodes+1) : System.Math.Max((numberOfNodes*2), (numberOfNodes+needed)));
                nodeKind = Saxon.Ejava.util.Arrays.copyOf(nodeKind, k);
                next = Saxon.Ejava.util.Arrays.copyOf(next, k);
                depth = Saxon.Ejava.util.Arrays.copyOf(depth, k);
                alpha = Saxon.Ejava.util.Arrays.copyOf(alpha, k);
                beta = Saxon.Ejava.util.Arrays.copyOf(beta, k);
                nameCode = Saxon.Ejava.util.Arrays.copyOf(nameCode, k);
                if ((typeArray != null)) {
                    typeArray = Saxon.Ejava.util.Arrays.copyOf(typeArray, k);
                }
                if ((typedValueArray != null)) {
                    typedValueArray = Saxon.Ejava.util.Arrays.copyOf(typedValueArray, k);
                }
                if ((lineNumbers != null)) {
                    lineNumbers = Saxon.Ejava.util.Arrays.copyOf(lineNumbers, k);
                    columnNumbers = Saxon.Ejava.util.Arrays.copyOf(columnNumbers, k);
                }
            }
        }
        private void ensureAttributeCapacity(int needed) {
            if ((attParent.Length<(numberOfAttributes+needed))) {
                int k = System.Math.Max((numberOfAttributes+needed), (numberOfAttributes*2));
                if ((k == 0)) {
                    k = (10+needed);
                }
                attParent = Saxon.Ejava.util.Arrays.copyOf(attParent, k);
                attCode = Saxon.Ejava.util.Arrays.copyOf(attCode, k);
                attValue = Saxon.Ejava.util.Arrays.copyOf(attValue, k);
                if ((attType != null)) {
                    attType = Saxon.Ejava.util.Arrays.copyOf(attType, k);
                }
                if ((attTypedValue != null)) {
                    attTypedValue = Saxon.Ejava.util.Arrays.copyOf(attTypedValue, k);
                }
            }
        }
        private void ensureNamespaceCapacity(int needed) {
            if ((namespaceMaps.Length<(numberOfNamespaces+needed))) {
                int k = System.Math.Max((numberOfNamespaces*2), (numberOfNamespaces+needed));
                if ((k == 0)) {
                    k = 10;
                }
                namespaceMaps = Saxon.Ejava.util.Arrays.copyOf(namespaceMaps, k);
            }
        }
        public Saxon.Hej.om.PrefixPool getPrefixPool() {
            return prefixPool;
        }
        public void setCopiedFrom(Saxon.Hej.om.NodeInfo copiedFrom) {
            this.copiedFrom = copiedFrom;
        }
        public Saxon.Hej.om.NodeInfo getCopiedFrom() {
            return this.copiedFrom;
        }
        internal int addDocumentNode(Saxon.Hej.tree.tiny.TinyDocumentImpl doc) {
            setRootNode(doc);
            return addNode(Saxon.Hej.type.Type.DOCUMENT, 0, 0, 0, -1);
        }
        internal int addNode(short kind, int depth, int alpha, int beta, int nameCode) {
            ensureNodeCapacity(kind, 1);
            nodeKind[numberOfNodes] = (byte)(kind);
            this.depth[numberOfNodes] = (short)(depth);
            this.alpha[numberOfNodes] = alpha;
            this.beta[numberOfNodes] = beta;
            this.nameCode[numberOfNodes] = nameCode;
            next[numberOfNodes] = -1;
            if ((typeArray != null)) {
                typeArray[numberOfNodes] = Saxon.Hej.type.Untyped.getInstance();
            }
            if ((numberOfNodes == 0)) {
                setDocumentNumber(getConfiguration().getDocumentNumberAllocator().allocateDocumentNumber());
            }
            return numberOfNodes++;
        }
        internal void appendChars(Saxon.Hej.str.UnicodeString chars) {
            textBuffer.appendUnicodeString(chars);
        }
        public int addTextNodeCopy(int depth, int existingNodeNr) {
            return addNode(Saxon.Hej.type.Type.TEXT, depth, alpha[existingNodeNr], beta[existingNodeNr], -1);
        }
        internal void condense(Saxon.Hej.tree.tiny.Statistics statistics) {
            if ((((numberOfNodes*3)<nodeKind.Length) || (((nodeKind.Length-numberOfNodes)>20000)))) {
                nodeKind = Saxon.Ejava.util.Arrays.copyOf(nodeKind, numberOfNodes);
                next = Saxon.Ejava.util.Arrays.copyOf(next, numberOfNodes);
                depth = Saxon.Ejava.util.Arrays.copyOf(depth, numberOfNodes);
                alpha = Saxon.Ejava.util.Arrays.copyOf(alpha, numberOfNodes);
                beta = Saxon.Ejava.util.Arrays.copyOf(beta, numberOfNodes);
                nameCode = Saxon.Ejava.util.Arrays.copyOf(nameCode, numberOfNodes);
                if ((typeArray != null)) {
                    typeArray = Saxon.Ejava.util.Arrays.copyOf(typeArray, numberOfNodes);
                }
                if ((lineNumbers != null)) {
                    lineNumbers = Saxon.Ejava.util.Arrays.copyOf(lineNumbers, numberOfNodes);
                    columnNumbers = Saxon.Ejava.util.Arrays.copyOf(columnNumbers, numberOfNodes);
                }
            }
            if (((((numberOfAttributes*3)<attParent.Length)) || (((attParent.Length-numberOfAttributes)>1000)))) {
                int k = numberOfAttributes;
                if ((k == 0)) {
                    attParent = Saxon.Hej.z.IntArraySet.EMPTY_INT_ARRAY;
                    attCode = Saxon.Hej.z.IntArraySet.EMPTY_INT_ARRAY;
                    attValue = EMPTY_STRING_ARRAY;
                    attType = null;
                } else {
                    attParent = Saxon.Ejava.util.Arrays.copyOf(attParent, numberOfAttributes);
                    attCode = Saxon.Ejava.util.Arrays.copyOf(attCode, numberOfAttributes);
                    attValue = Saxon.Ejava.util.Arrays.copyOf(attValue, numberOfAttributes);
                }
                if ((attType != null)) {
                    attType = Saxon.Ejava.util.Arrays.copyOf(attType, numberOfAttributes);
                }
            }
            if (((numberOfNamespaces*3)<namespaceMaps.Length)) {
                namespaceMaps = Saxon.Ejava.util.Arrays.copyOf(namespaceMaps, numberOfNamespaces);
            }
            prefixPool.condense();
            statistics.updateStatistics(numberOfNodes, numberOfAttributes, numberOfNamespaces, textBuffer.length());
        }
        internal void setElementAnnotation(int nodeNr, Saxon.Hej.type.SchemaType type) {
            if (!(type.Equals(Saxon.Hej.type.Untyped.getInstance()))) {
                if ((typeArray == null)) {
                    typeArray = new Saxon.Hej.type.SchemaType[nodeKind.Length];
                    Saxon.Ejava.util.Arrays.fill(typeArray, 0, nodeKind.Length, Saxon.Hej.type.Untyped.getInstance());
                }
                System.Diagnostics.Debug.Assert((typeArray != null));
                typeArray[nodeNr] = type;
            }
        }
        public int getTypeAnnotation(int nodeNr) {
            if ((typeArray == null)) {
                return Saxon.Hej.om.StandardNames.XS_UNTYPED;
            }
            return typeArray[nodeNr].getFingerprint();
        }
        public Saxon.Hej.type.SchemaType getSchemaType(int nodeNr) {
            if ((typeArray == null)) {
                return Saxon.Hej.type.Untyped.getInstance();
            }
            return typeArray[nodeNr];
        }
        public Saxon.Hej.om.AtomicSequence getTypedValueOfElement(Saxon.Hej.tree.tiny.TinyElementImpl element) {
            int nodeNr = element.nodeNr;
            if (((typedValueArray == null) || (typedValueArray[nodeNr] == null))) {
                Saxon.Hej.type.SchemaType stype = getSchemaType(nodeNr);
                int annotation = stype.getFingerprint();
                if ((((annotation == Saxon.Hej.om.StandardNames.XS_UNTYPED) || (annotation == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (annotation == Saxon.Hej.om.StandardNames.XS_ANY_TYPE))) {
                    Saxon.Hej.str.UnicodeString stringValue = Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr);
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(stringValue);
                } else {
                    if ((annotation == Saxon.Hej.om.StandardNames.XS_STRING)) {
                        Saxon.Hej.str.UnicodeString stringValue = Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr);
                        return new Saxon.Hej.value.StringValue(stringValue);
                    } else {
                        if ((annotation == Saxon.Hej.om.StandardNames.XS_ANY_URI)) {
                            Saxon.Hej.str.UnicodeString stringValue = Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr);
                            return new Saxon.Hej.value.AnyURIValue(stringValue);
                        } else {
                            Saxon.Hej.om.AtomicSequence value = stype.atomize(element);
                            if (allowTypedValueCache) {
                                if ((typedValueArray == null)) {
                                    typedValueArray = new Saxon.Hej.om.AtomicSequence[nodeKind.Length];
                                }
                                typedValueArray[nodeNr] = value;
                            }
                            return value;
                        }
                    }
                }
            } else {
                return typedValueArray[nodeNr];
            }
        }
        public Saxon.Hej.om.AtomicSequence getTypedValueOfElement(int nodeNr) {
            if (((typedValueArray == null) || (typedValueArray[nodeNr] == null))) {
                Saxon.Hej.type.SchemaType stype = getSchemaType(nodeNr);
                int annotation = stype.getFingerprint();
                if (((annotation == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC) || (annotation == Saxon.Hej.om.StandardNames.XS_UNTYPED))) {
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr));
                } else {
                    if ((annotation == Saxon.Hej.om.StandardNames.XS_STRING)) {
                        return new Saxon.Hej.value.StringValue(Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr).tidy());
                    } else {
                        if ((annotation == Saxon.Hej.om.StandardNames.XS_ANY_URI)) {
                            return new Saxon.Hej.value.AnyURIValue(Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr));
                        } else {
                            if ((annotation == Saxon.Hej.om.StandardNames.XS_ID)) {
                                return new Saxon.Hej.value.StringValue(Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr).tidy(), Saxon.Hej.type.BuiltInAtomicType.ID);
                            } else {
                                Saxon.Hej.tree.tiny.TinyNodeImpl element = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getNode(nodeNr));
                                Saxon.Hej.om.AtomicSequence value = stype.atomize(element);
                                if (allowTypedValueCache) {
                                    if ((typedValueArray == null)) {
                                        typedValueArray = new Saxon.Hej.om.AtomicSequence[nodeKind.Length];
                                    }
                                    typedValueArray[nodeNr] = value;
                                }
                                return value;
                            }
                        }
                    }
                }
            } else {
                return typedValueArray[nodeNr];
            }
        }
        public Saxon.Hej.om.AtomicSequence getTypedValueOfAttribute(Saxon.Hej.tree.tiny.TinyAttributeImpl att, int nodeNr) {
            if ((attType == null)) {
                return new Saxon.Hej.value.StringValue(attValue[nodeNr], Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
            }
            if (((attTypedValue == null) || (attTypedValue[nodeNr] == null))) {
                Saxon.Hej.type.SimpleType type = getAttributeType(nodeNr);
                if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                    return new Saxon.Hej.value.StringValue(attValue[nodeNr], Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                } else {
                    if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                        return new Saxon.Hej.value.StringValue(attValue[nodeNr]);
                    } else {
                        if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_URI)) {
                            return new Saxon.Hej.value.AnyURIValue((attValue[nodeNr].ToString()));
                        } else {
                            if ((att == null)) {
                                att = new Saxon.Hej.tree.tiny.TinyAttributeImpl(this, nodeNr);
                            }
                            Saxon.Hej.om.AtomicSequence value = type.atomize(att);
                            if (allowTypedValueCache) {
                                if ((attTypedValue == null)) {
                                    attTypedValue = new Saxon.Hej.om.AtomicSequence[attParent.Length];
                                }
                                attTypedValue[nodeNr] = value;
                            }
                            return value;
                        }
                    }
                }
            } else {
                return attTypedValue[nodeNr];
            }
        }
        public int getNodeKind(int nodeNr) {
            int kind = nodeKind[nodeNr];
            return ((kind == Saxon.Hej.type.Type.WHITESPACE_TEXT) ? Saxon.Hej.type.Type.TEXT : kind);
        }
        public int getNameCode(int nodeNr) {
            return nameCode[nodeNr];
        }
        public int getFingerprint(int nodeNr) {
            int nc = nameCode[nodeNr];
            return ((nc == -1) ? -1 : (nc&Saxon.Hej.om.NamePool.FP_MASK));
        }
        public string getPrefix(int nodeNr) {
            int code = (nameCode[nodeNr]>>20);
            if ((code<=0)) {
                return ((code == 0) ? "" : null);
            }
            return prefixPool.getPrefix(code);
        }
        internal void ensurePriorIndex() {
            if (((prior == null) || (prior.Length<numberOfNodes))) {
                makePriorIndex();
            }
        }
        private void makePriorIndex() { lock(this) {
            int[] p = new int[numberOfNodes];
            Saxon.Ejava.util.Arrays.fill(p, 0, numberOfNodes, -1);
            for (int i = 0; (i<numberOfNodes); i++) {
                int nextNode = next[i];
                if ((nextNode>i)) {
                    p[nextNode] = i;
                }
            }
            prior = p;
        }}
        internal void addAttribute(Saxon.Hej.om.NodeInfo root, int parent, int nameCode, Saxon.Hej.type.SimpleType type, string attValue, int properties) {
            ensureAttributeCapacity(1);
            attParent[numberOfAttributes] = parent;
            attCode[numberOfAttributes] = nameCode;
            this.attValue[numberOfAttributes] = attValue.ToString();
            if (!(type.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                initializeAttributeTypeCodes();
            }
            if ((attType != null)) {
                attType[numberOfAttributes] = type;
            }
            if ((alpha[parent] == -1)) {
                alpha[parent] = numberOfAttributes;
            }
            if ((root is Saxon.Hej.tree.tiny.TinyDocumentImpl)) {
                bool isID = false;
                try {
                    if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.IS_ID)) {
                        isID = true;
                    } else {
                        if ((((nameCode&Saxon.Hej.om.NamePool.FP_MASK)) == Saxon.Hej.om.StandardNames.XML_ID)) {
                            isID = true;
                        } else {
                            if (type.isIdType()) {
                                isID = true;
                            }
                        }
                    }
                } catch (Saxon.Hej.type.MissingComponentException e) {
                }
                if (isID) {
                    string id = Saxon.Hej.value.Whitespace.trim(attValue);
                    this.attValue[numberOfAttributes] = id;
                    if (Saxon.Hej.om.NameChecker.isValidNCName(id)) {
                        Saxon.Hej.om.NodeInfo e = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getNode(parent));
                        registerID(e, id);
                    } else {
                        if ((attType != null)) {
                            attType[numberOfAttributes] = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                        }
                    }
                }
                bool isIDREF = false;
                try {
                    if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.IS_IDREF)) {
                        isIDREF = true;
                    } else {
                        if (((type == Saxon.Hej.type.BuiltInAtomicType.IDREF) || (type == Saxon.Hej.type.BuiltInListType.IDREFS))) {
                            isIDREF = true;
                        } else {
                            if (type.isIdRefType()) {
                                try {
                                    Saxon.Hej.om.AtomicSequence @as = type.getTypedValue(Saxon.Hej.str.StringView.of(attValue).tidy(), null, getConfiguration().getConversionRules());
                                    foreach (Saxon.Hej.value.AtomicValue v in @as) {
                                        if (v.getItemType().isIdRefType()) {
                                            isIDREF = true;
                                            break ;
                                        }
                                    }
                                } catch (Saxon.Hej.type.ValidationException ve) {
                                }
                            }
                        }
                    }
                } catch (Saxon.Hej.type.MissingComponentException e) {
                }
                if (isIDREF) {
                    if ((idRefAttributes == null)) {
                        idRefAttributes = new Saxon.Hej.z.IntHashSet();
                    }
                    idRefAttributes.add(numberOfAttributes);
                }
            }
            numberOfAttributes++;
        }
        private void initializeAttributeTypeCodes() {
            if ((attType == null)) {
                attType = new Saxon.Hej.type.SimpleType[attParent.Length];
                Saxon.Ejava.util.Arrays.fill(attType, 0, numberOfAttributes, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
            }
        }
        public void markDefaultedAttribute(int attNr) {
            if ((defaultedAttributes == null)) {
                defaultedAttributes = new Saxon.Hej.z.IntHashSet();
            }
            defaultedAttributes.add(attNr);
        }
        public bool isDefaultedAttribute(int attNr) {
            return ((defaultedAttributes != null) && defaultedAttributes.contains(attNr));
        }
        public void indexIDElement(Saxon.Hej.om.NodeInfo root, int nodeNr) {
            string id = Saxon.Hej.value.Whitespace.trim(Saxon.Hej.tree.tiny.TinyParentNodeImpl.getStringValue(this, nodeNr).tidy()).ToString();
            if (((root.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT) && Saxon.Hej.om.NameChecker.isValidNCName(id))) {
                Saxon.Hej.om.NodeInfo e = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getNode(nodeNr));
                registerID(e, id);
            }
        }
        public bool hasXmlSpacePreserveAttribute() {
            for (int i = 0; (i<numberOfAttributes); i++) {
                if (((((attCode[i]&Saxon.Hej.om.NamePool.FP_MASK)) == Saxon.Hej.om.StandardNames.XML_SPACE) && "preserve".Equals(attValue[i].ToString()))) {
                    return true;
                }
            }
            return false;
        }
        internal void addNamespaces(int parent, Saxon.Hej.om.NamespaceMap nsMap) {
            usesNamespaces = true;
            for (int i = 0; (i<numberOfNamespaces); i++) {
                if (namespaceMaps[i].Equals(nsMap)) {
                    beta[parent] = i;
                    return ;
                }
            }
            ensureNamespaceCapacity(1);
            namespaceMaps[numberOfNamespaces] = nsMap;
            beta[parent] = numberOfNamespaces;
            numberOfNamespaces++;
        }
        public Saxon.Hej.om.NodeInfo getNode(int nr) {
            switch ((short)(nodeKind[nr])) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return (Saxon.Hej.tree.tiny.TinyDocumentImpl)(getRootNode());
              case Saxon.Hej.type.Type.ELEMENT:
                return new Saxon.Hej.tree.tiny.TinyElementImpl(this, nr);
              case Saxon.Hej.type.Type.TEXTUAL_ELEMENT:
                return new Saxon.Hej.tree.tiny.TinyTextualElement(this, nr);
              case Saxon.Hej.type.Type.TEXT:
                return new Saxon.Hej.tree.tiny.TinyTextImpl(this, nr);
              case Saxon.Hej.type.Type.WHITESPACE_TEXT:
                return new Saxon.Hej.tree.tiny.WhitespaceTextImpl(this, nr);
              case Saxon.Hej.type.Type.COMMENT:
                return new Saxon.Hej.tree.tiny.TinyCommentImpl(this, nr);
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return new Saxon.Hej.tree.tiny.TinyProcInstImpl(this, nr);
              case Saxon.Hej.type.Type.PARENT_POINTER:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Attempting to treat a parent pointer as a node");
              case Saxon.Hej.type.Type.STOPPER:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Attempting to treat a stopper entry as a node");
              default:
                throw new Saxon.Ejava.lang.IllegalStateException(("Unknown node kind "+nodeKind[nr]));
            }
        }
        internal Saxon.Hej.value.AtomicValue getAtomizedValueOfUntypedNode(int nodeNr) {
            switch ((short)(nodeKind[nodeNr])) {
              case Saxon.Hej.type.Type.ELEMENT:
              case Saxon.Hej.type.Type.DOCUMENT:
                int level = depth[nodeNr];
                int next = (nodeNr+1);
                if ((depth[next]<=level)) {
                    return Saxon.Hej.value.StringValue.ZERO_LENGTH_UNTYPED;
                } else {
                    if (((nodeKind[next] == Saxon.Hej.type.Type.TEXT) && (depth[(next+1)]<=level))) {
                        int length = beta[next];
                        int start = alpha[next];
                        return Saxon.Hej.value.StringValue.makeUntypedAtomic(textBuffer.substring(start, (start+length)));
                    } else {
                        if (((nodeKind[next] == Saxon.Hej.type.Type.WHITESPACE_TEXT) && (depth[(next+1)]<=level))) {
                            long compressedValue = ((((long)(alpha[next])<<32))|(((long)(beta[next])&unchecked((long)0xffffffffL))));
                            return Saxon.Hej.value.StringValue.makeUntypedAtomic(new Saxon.Hej.str.CompressedWhitespace(compressedValue));
                        }
                    }
                }
                Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
                while (((next<numberOfNodes) && (depth[next]>level))) {
                    if ((nodeKind[next] == Saxon.Hej.type.Type.TEXT)) {
                        sb.accept(Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(this, next));
                    } else {
                        if ((nodeKind[next] == Saxon.Hej.type.Type.WHITESPACE_TEXT)) {
                            Saxon.Hej.tree.tiny.WhitespaceTextImpl.appendStringValue(this, next, sb);
                        }
                    }
                    next++;
                }
                return sb.toStringItem(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
              case Saxon.Hej.type.Type.TEXT:
                return new Saxon.Hej.value.StringValue(Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(this, nodeNr));
              case Saxon.Hej.type.Type.WHITESPACE_TEXT:
                {
                    long compressedValue = ((((long)(alpha[nodeNr])<<32))|(((long)(beta[nodeNr])&unchecked((long)0xffffffffL))));
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(new Saxon.Hej.str.CompressedWhitespace(compressedValue));
                }
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                {
                    int start2 = alpha[nodeNr];
                    int len2 = beta[nodeNr];
                    if ((len2 == 0)) {
                        return Saxon.Hej.value.StringValue.ZERO_LENGTH_UNTYPED;
                    }
                    char[] dest = new char[len2];
                    System.Diagnostics.Debug.Assert((commentBuffer != null));
                    return new Saxon.Hej.value.StringValue(commentBuffer.substring(start2, (start2+len2)));
                }
              default:
                throw new Saxon.Ejava.lang.IllegalStateException("Unknown node kind");
            }
        }
        internal Saxon.Hej.tree.tiny.TinyAttributeImpl getAttributeNode(int nr) {
            return new Saxon.Hej.tree.tiny.TinyAttributeImpl(this, nr);
        }
        internal int getAttributeAnnotation(int nr) {
            if ((attType == null)) {
                return Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC;
            } else {
                return attType[nr].getFingerprint();
            }
        }
        internal Saxon.Hej.type.SimpleType getAttributeType(int nr) {
            if ((attType == null)) {
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            } else {
                return attType[nr];
            }
        }
        public bool isIdAttribute(int nr) {
            try {
                return ((attType != null) && getAttributeType(nr).isIdType());
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
        }
        public bool isIdrefAttribute(int nr) {
            return ((idRefAttributes != null) && idRefAttributes.contains(nr));
        }
        public bool isIdElement(int nr) {
            try {
                return (getSchemaType(nr).isIdType() && (getTypedValueOfElement(nr).getLength() == 1));
            } catch (Saxon.Hej.trans.XPathException e) {
                return false;
            }
        }
        public bool isIdrefElement(int nr) {
            Saxon.Hej.type.SchemaType type = getSchemaType(nr);
            try {
                if (type.isIdRefType()) {
                    if (((type == Saxon.Hej.type.BuiltInAtomicType.IDREF) || (type == Saxon.Hej.type.BuiltInListType.IDREFS))) {
                        return true;
                    }
                    try {
                        foreach (Saxon.Hej.value.AtomicValue av in getTypedValueOfElement(nr)) {
                            if (av.getItemType().isIdRefType()) {
                                return true;
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException err) {
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
            return false;
        }
        internal void setSystemId(int seq, string uri) {
            if ((uri == null)) {
                uri = "";
            }
            if ((systemIdMap == null)) {
                systemIdMap = new Saxon.Hej.tree.linked.SystemIdMap();
            }
            systemIdMap.setSystemId(seq, uri);
        }
        internal void setUniformBaseUri(string @base) {
            this.uniformBaseUri = @base;
        }
        internal string getUniformBaseUri() {
            return this.uniformBaseUri;
        }
        public string getSystemId(int seq) {
            if ((systemIdMap == null)) {
                return null;
            }
            return systemIdMap.getSystemId(seq);
        }
        public override Saxon.Hej.om.NodeInfo getRootNode() {
            if ((getNodeKind(0) == Saxon.Hej.type.Type.DOCUMENT)) {
                if ((documentRoot != null)) {
                    return documentRoot;
                } else {
                    documentRoot = new Saxon.Hej.tree.tiny.TinyDocumentImpl(this);
                    return documentRoot;
                }
            } else {
                return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)getNode(0));
            }
        }
        public void setLineNumbering() {
            lineNumbers = new int[nodeKind.Length];
            Saxon.Ejava.util.Arrays.fill(lineNumbers, -1);
            columnNumbers = new int[nodeKind.Length];
            Saxon.Ejava.util.Arrays.fill(columnNumbers, -1);
        }
        internal void setLineNumber(int sequence, int line, int column) {
            if ((lineNumbers != null)) {
                System.Diagnostics.Debug.Assert((columnNumbers != null));
                lineNumbers[sequence] = line;
                columnNumbers[sequence] = column;
            }
        }
        public int getLineNumber(int sequence) {
            if ((lineNumbers != null)) {
                for (int i = sequence; (i>=0); i--) {
                    int c = lineNumbers[i];
                    if ((c>0)) {
                        return c;
                    }
                }
            }
            return -1;
        }
        public int getColumnNumber(int sequence) {
            if ((columnNumbers != null)) {
                for (int i = sequence; (i>=0); i--) {
                    int c = columnNumbers[i];
                    if ((c>0)) {
                        return c;
                    }
                }
            }
            return -1;
        }
        public void setNilled(int nodeNr) {
            if ((nilledElements == null)) {
                nilledElements = new Saxon.Hej.z.IntHashSet();
            }
            nilledElements.add(nodeNr);
        }
        public bool isNilled(int nodeNr) {
            return ((nilledElements != null) && nilledElements.contains(nodeNr));
        }
        internal void registerID(Saxon.Hej.om.NodeInfo e, string id) {
            if ((idTable == null)) {
                idTable = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.NodeInfo>(256);
            }
            Saxon.Impl.Helpers.MapUtils.PutIfAbsent(idTable, id, e);
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            if ((idTable == null)) {
                return null;
            }
            Saxon.Hej.om.NodeInfo node = Saxon.Impl.Helpers.MapUtils.Get(idTable, id);
            if (((((node != null) && getParent) && node.isId()) && node.getStringValue().Equals(id))) {
                node = node.getParent();
            }
            return node;
        }
        internal void setUnparsedEntity(string name, string uri, string publicId) {
            if ((entityTable == null)) {
                entityTable = new System.Collections.Generic.Dictionary<string, string[]>(20);
            }
            string[] ids = new string[2];
            ids[0] = uri;
            ids[1] = publicId;
            Saxon.Impl.Helpers.MapUtils.Put(entityTable, name, ids);
        }
        public override System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            if ((entityTable == null)) {
                return new System.Collections.Generic.List<string>().GetEnumerator();;
            } else {
                return Saxon.Impl.Helpers.MapUtils.KeySet(entityTable).GetEnumerator();
            }
        }
        public override string[] getUnparsedEntity(string name) {
            if ((entityTable == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(entityTable, name);
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return getConfiguration().getNamePool();
        }
        public void markTopWithinEntity(int nodeNr) {
            if ((topWithinEntity == null)) {
                topWithinEntity = new Saxon.Hej.z.IntHashSet();
            }
            topWithinEntity.add(nodeNr);
        }
        public bool isTopWithinEntity(int nodeNr) {
            return ((topWithinEntity != null) && topWithinEntity.contains(nodeNr));
        }
        public void diagnosticDump() {
            Saxon.Hej.om.NamePool pool = getNamePool();
            System.Console.Error.WriteLine("    node    kind   depth    next   alpha    beta    name    type");
            for (int i = 0; (i<numberOfNodes); i++) {
                string eqName = "";
                if ((nameCode[i] != -1)) {
                    try {
                        eqName = pool.getEQName(nameCode[i]);
                    } catch (System.Exception err) {
                        eqName = ("#"+nameCode[1]);
                    }
                }
                System.Console.Error.WriteLine((((((((((n8(i)+n8(nodeKind[i]))+n8(depth[i]))+n8(next[i]))+n8(alpha[i]))+n8(beta[i]))+n8(nameCode[i]))+n8(getTypeAnnotation(i)))+" ")+eqName));
            }
            System.Console.Error.WriteLine("    attr  parent    name    value");
            for (int i = 0; (i<numberOfAttributes); i++) {
                System.Console.Error.WriteLine(((((n8(i)+n8(attParent[i]))+n8(attCode[i]))+"    ")+attValue[i]));
            }
            System.Console.Error.WriteLine("      ns  parent  prefix     uri");
            for (int i = 0; (i<numberOfNamespaces); i++) {
                System.Console.Error.WriteLine(((n8(i)+"  ")+namespaceMaps[i]));
            }
        }
        public static void diagnosticDump(Saxon.Hej.om.NodeInfo node) { lock(typeof(TinyTree)) {
            if ((node is Saxon.Hej.tree.tiny.TinyNodeImpl)) {
                Saxon.Hej.tree.tiny.TinyTree tree = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).tree;
                System.Console.Error.WriteLine(("Tree containing node "+((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).nodeNr));
                tree.diagnosticDump();
            } else {
                System.Console.Error.WriteLine("Node is not in a TinyTree");
            }
        }}
        private static string n8(int val) {
            string s = ("        "+val);
            return s.Substring((s.Length-8));
        }
        public void showSize() {
            System.Console.Error.WriteLine((((((("Tree size: "+numberOfNodes)+" nodes, ")+textBuffer.length())+" characters, ")+numberOfAttributes)+" attributes"));
        }
        public override  /* REPLACING public */ bool isTyped() {
            return (typeArray != null);
        }
        public int getNumberOfNodes() {
            return numberOfNodes;
        }
        public int getNumberOfAttributes() {
            return numberOfAttributes;
        }
        public int getNumberOfNamespaces() {
            return numberOfNamespaces;
        }
        public byte[] getNodeKindArray() {
            return nodeKind;
        }
        public short[] getNodeDepthArray() {
            return depth;
        }
        public int[] getNameCodeArray() {
            return nameCode;
        }
        public Saxon.Hej.type.SchemaType[] getTypeArray() {
            return typeArray;
        }
        public int[] getNextPointerArray() {
            return next;
        }
        public int[] getAlphaArray() {
            return alpha;
        }
        public int[] getBetaArray() {
            return beta;
        }
        public Saxon.Hej.str.LargeTextBuffer getCharacterBuffer() {
            return textBuffer;
        }
        public Saxon.Hej.str.UnicodeString getCommentBuffer() {
            return commentBuffer;
        }
        public int[] getAttributeNameCodeArray() {
            return attCode;
        }
        public Saxon.Hej.type.SimpleType[] getAttributeTypeArray() {
            return attType;
        }
        public int[] getAttributeParentArray() {
            return attParent;
        }
        public string[] getAttributeValueArray() {
            return attValue;
        }
        public Saxon.Hej.om.NamespaceBinding[] getNamespaceBindings() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public Saxon.Hej.om.NamespaceMap[] getNamespaceMaps() {
            return namespaceMaps;
        }
        public int[] getNamespaceParentArray() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public bool isUsesNamespaces() {
            return usesNamespaces;
        }
        public System.Collections.Generic.IDictionary<string,Saxon.Hej.z.IntSet> getLocalNameIndex() { lock(this) {
            if ((localNameIndex == null)) {
                localNameIndex = new System.Collections.Generic.Dictionary<string, Saxon.Hej.z.IntSet>();
                Saxon.Hej.z.IntHashSet indexed = new Saxon.Hej.z.IntHashSet();
                for (int i = 0; (i<numberOfNodes); i++) {
                    if ((((nodeKind[i]&unchecked((int)0xf))) == Saxon.Hej.type.Type.ELEMENT)) {
                        int fp = (nameCode[i]&Saxon.Hej.om.NamePool.FP_MASK);
                        if (!(indexed.contains(fp))) {
                            string local = getNamePool().getLocalName(fp);
                            indexed.add(fp);
                            Saxon.Hej.z.IntSet existing = Saxon.Impl.Helpers.MapUtils.Get(localNameIndex, local);
                            if ((existing == null)) {
                                Saxon.Impl.Helpers.MapUtils.Put(localNameIndex, local, new Saxon.Hej.z.IntSingletonSet(fp));
                            } else {
                                Saxon.Hej.z.IntSet copy = (existing.isMutable() ? existing : existing.mutableCopy());
                                copy.add(fp);
                                Saxon.Impl.Helpers.MapUtils.Put(localNameIndex, local, copy);
                            }
                        }
                    }
                }
            }
            return localNameIndex;
        }}
    
    }
}