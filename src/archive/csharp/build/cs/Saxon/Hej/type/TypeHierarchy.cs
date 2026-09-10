using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class TypeHierarchy {
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.type.TypeHierarchy.ItemTypePair,Saxon.Hej.type.Affinity> map;
        protected internal Saxon.Hej.Configuration config;
        public TypeHierarchy(Saxon.Hej.Configuration config)  {
            this.config = config;
            map = new System.Collections.Concurrent.ConcurrentDictionary<Saxon.Hej.type.TypeHierarchy.ItemTypePair, Saxon.Hej.type.Affinity>();
        }
        public static Saxon.Hej.type.SchemaType getNearestNamedType(Saxon.Hej.type.SchemaType type) {
            while (type.isAnonymousType()) {
                type = type.getBaseType();
            }
            return type;
        }
        public Saxon.Hej.om.GroundedValue applyFunctionConversionRules(Saxon.Hej.om.Sequence value, Saxon.Hej.value.SequenceType requiredType, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.s9api.Location locator) {
            try {
                Saxon.Hej.om.GroundedValue groundedValue = value.materialize();
                if (requiredType.matches(groundedValue, this)) {
                    return groundedValue;
                }
                Saxon.Hej.type.ItemType suppliedItemType = Saxon.Hej.om.SequenceTool.getItemType(value, this);
                Saxon.Hej.om.SequenceIterator iterator = groundedValue.iterate();
                Saxon.Hej.type.ItemType requiredItemType = requiredType.getPrimaryType();
                if (requiredItemType.isPlainType()) {
                    if (!(suppliedItemType.isPlainType())) {
                        try {
                            iterator = Saxon.Hej.expr.Atomizer.getAtomizingIterator(iterator, false);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((("Failed to atomize the "+role.getMessage())+": ")+e.Message));
                            vf.setErrorCode("XPTY0117");
                            throw vf.makeException();
                        }
                        suppliedItemType = suppliedItemType.getAtomizedItemType();
                    }
                    if (((relationship(suppliedItemType, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT) && !(isSubType(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, requiredItemType)))) {
                        bool nsSensitive = ((Saxon.Hej.type.SimpleType)(requiredItemType)).isNamespaceSensitive();
                        Saxon.Hej.expr.ItemMappingFunction converter;
                        if (nsSensitive) {
                            converter = Saxon.Hej.expr.ItemMapper.of(item => {
                                if (((item is Saxon.Hej.value.AtomicValue) && ((Saxon.Hej.value.AtomicValue)(item)).isUntypedAtomic())) {
                                    Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((((("Failed to convert the "+role.getMessage())+": ")+"Implicit conversion of untypedAtomic value to ")+requiredItemType)+" is not allowed"));
                                    vf.setErrorCode("XPTY0117");
                                    throw vf.makeException();
                                } else {
                                    return item;
                                }
                            } );
                        } else {
                            if (((Saxon.Hej.type.SimpleType)(requiredItemType)).isUnionType()) {
                                Saxon.Hej.lib.ConversionRules rules = config.getConversionRules();
                                converter = Saxon.Hej.expr.ItemMapper.of(item => {
                                    if (((item is Saxon.Hej.value.AtomicValue) && ((Saxon.Hej.value.AtomicValue)(item)).isUntypedAtomic())) {
                                        try {
                                            return ((/*1430*/Saxon.Hej.value.AtomicValue)((Saxon.Hej.type.SimpleType)(requiredItemType)).getTypedValue(item.getUnicodeStringValue(), null, rules).head());
                                        } catch (Saxon.Hej.type.ValidationException ve) {
                                            ve.setErrorCode("XPTY0004");
                                            throw;
                                        }
                                    } else {
                                        return item;
                                    }
                                } );
                            } else {
                                converter = Saxon.Hej.expr.ItemMapper.of(item => {
                                    if (((item is Saxon.Hej.value.AtomicValue) && ((Saxon.Hej.value.AtomicValue)(item)).isUntypedAtomic())) {
                                        return Saxon.Hej.type.Converter.convert((Saxon.Hej.value.StringValue)(item), (Saxon.Hej.type.AtomicType)(requiredItemType), config.getConversionRules());
                                    } else {
                                        return item;
                                    }
                                } );
                            }
                        }
                        iterator = new Saxon.Hej.expr.ItemMappingIterator(iterator, converter, true);
                    }
                    if (requiredItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.DOUBLE)) {
                        Saxon.Hej.expr.ItemMappingFunction promoter = Saxon.Hej.expr.ItemMapper.of(item => {
                            if ((item is Saxon.Hej.value.NumericValue)) {
                                return (Saxon.Hej.value.DoubleValue)(Saxon.Hej.type.Converter.convert((Saxon.Hej.value.NumericValue)(item), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, config.getConversionRules()).asAtomic());
                            } else {
                                throw new Saxon.Hej.trans.XPathException(((("Failed to convert the "+role.getMessage())+": ")+"Cannot promote non-numeric value to xs:double"), "XPTY0004");
                            }
                        } );
                        iterator = new Saxon.Hej.expr.ItemMappingIterator(iterator, promoter, true);
                    } else {
                        if (requiredItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.FLOAT)) {
                            Saxon.Hej.expr.ItemMappingFunction promoter = Saxon.Hej.expr.ItemMapper.of(item => {
                                if ((item is Saxon.Hej.value.DoubleValue)) {
                                    throw new Saxon.Hej.trans.XPathException(((("Failed to convert the "+role.getMessage())+": ")+"Cannot promote xs:double value to xs:float"), "XPTY0004");
                                } else {
                                    if ((item is Saxon.Hej.value.NumericValue)) {
                                        return (Saxon.Hej.value.FloatValue)(Saxon.Hej.type.Converter.convert((Saxon.Hej.value.NumericValue)(item), Saxon.Hej.type.BuiltInAtomicType.FLOAT, config.getConversionRules()).asAtomic());
                                    } else {
                                        throw new Saxon.Hej.trans.XPathException(((("Failed to convert the "+role.getMessage())+": ")+"Cannot promote non-numeric value to xs:float"), "XPTY0004");
                                    }
                                }
                            } );
                            iterator = new Saxon.Hej.expr.ItemMappingIterator(iterator, promoter, true);
                        }
                    }
                    if ((requiredItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING) && (relationship(suppliedItemType, Saxon.Hej.type.BuiltInAtomicType.ANY_URI) != /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT))) {
                        Saxon.Hej.expr.ItemMappingFunction promoter = Saxon.Hej.expr.ItemMapper.of(item => {
                            if ((item is Saxon.Hej.value.AnyURIValue)) {
                                return ((Saxon.Hej.value.AnyURIValue)(item)).convertToString();
                            } else {
                                return item;
                            }
                        } );
                        iterator = new Saxon.Hej.expr.ItemMappingIterator(iterator, promoter, true);
                    }
                }
                iterator = applyFunctionCoercion(iterator, suppliedItemType, requiredItemType, locator);
                Saxon.Hej.type.Affinity relation = relationship(suppliedItemType, requiredItemType);
                if (!((((relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                    Saxon.Hej.expr.ItemTypeCheckingFunction itemChecker = new Saxon.Hej.expr.ItemTypeCheckingFunction(requiredItemType, role, locator, config);
                    iterator = new Saxon.Hej.expr.ItemMappingIterator(iterator, itemChecker, true);
                }
                if ((requiredType.getCardinality() != Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                    iterator = new Saxon.Hej.expr.CardinalityCheckingIterator(iterator, requiredType.getCardinality(), role, locator);
                }
                return Saxon.Hej.om.SequenceTool.toGroundedValue(iterator);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        protected internal Saxon.Hej.om.SequenceIterator applyFunctionCoercion(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.type.ItemType suppliedItemType, Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.s9api.Location locator) {
            if (((((requiredItemType is Saxon.Hej.type.FunctionItemType) && !(((Saxon.Hej.type.FunctionItemType)(requiredItemType)).isMapType())) && !(((Saxon.Hej.type.FunctionItemType)(requiredItemType)).isArrayType())) && !(((relationship(requiredItemType, suppliedItemType) == Saxon.Hej.type.Affinity.SUBSUMES))))) {
                if ((requiredItemType == Saxon.Hej.type.AnyFunctionType.getInstance())) {
                    return iterator;
                } else {
                    Saxon.Hej.functions.hof.FunctionSequenceCoercer.Coercer coercer = new Saxon.Hej.functions.hof.FunctionSequenceCoercer.Coercer((Saxon.Hej.type.SpecificFunctionType)(requiredItemType), config, locator);
                    return new Saxon.Hej.expr.ItemMappingIterator(iterator, coercer, true);
                }
            } else {
                return iterator;
            }
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public bool isSubType(Saxon.Hej.type.ItemType subtype, Saxon.Hej.type.ItemType supertype) {
            Saxon.Hej.type.Affinity relation = relationship(subtype, supertype);
            return ((relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY));
        }
        public Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.ItemType t1, Saxon.Hej.type.ItemType t2) {
            Saxon.Ejava.util.Objects.requireNonNull(t1);
            Saxon.Ejava.util.Objects.requireNonNull(t2);
            t1 = stabilize(t1);
            t2 = stabilize(t2);
            if (t1.Equals(t2)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
            }
            if ((t2 is Saxon.Hej.type.AnyItemType)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
            }
            if ((t1 is Saxon.Hej.type.AnyItemType)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
            }
            if (((t1 is Saxon.Hej.type.BuiltInAtomicType) && (t2 is Saxon.Hej.type.BuiltInAtomicType))) {
                if (t1.getBasicAlphaCode().StartsWith(t2.getBasicAlphaCode())) {
                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                } else {
                    if (t2.getBasicAlphaCode().StartsWith(t1.getBasicAlphaCode())) {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                    } else {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                    }
                }
            }
            if ((t1 is Saxon.Hej.type.ErrorType)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
            }
            if ((t2 is Saxon.Hej.type.ErrorType)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
            }
            Saxon.Hej.type.TypeHierarchy.ItemTypePair pair = new Saxon.Hej.type.TypeHierarchy.ItemTypePair(t1, t2);
            if (map.ContainsKey(pair)) {
                return Saxon.Impl.Helpers.MapUtils.Get(map, pair);
            }
            Saxon.Hej.type.Affinity affinity = computeRelationship(t1, t2);
            Saxon.Impl.Helpers.MapUtils.Put(map, pair, affinity);
            return affinity;
        }
        private static Saxon.Hej.type.ItemType stabilize(Saxon.Hej.type.ItemType @in) {
            if ((@in is Saxon.Hej.pattern.SameNameTest)) {
                return ((Saxon.Hej.pattern.SameNameTest)(@in)).getEquivalentNameTest();
            } else {
                return @in;
            }
        }
        private Saxon.Hej.type.Affinity computeRelationship(Saxon.Hej.type.ItemType t1, Saxon.Hej.type.ItemType t2) {
            requireTrueItemType(t1);
            requireTrueItemType(t2);
            try {
                if ((t1 == t2)) {
                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                }
                if ((t1 is Saxon.Hej.type.AnyItemType)) {
                    if ((t2 is Saxon.Hej.type.AnyItemType)) {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                    } else {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                    }
                } else {
                    if ((t2 is Saxon.Hej.type.AnyItemType)) {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if (t1.isPlainType()) {
                            if ((((t2 is Saxon.Hej.pattern.NodeTest) || (t2 is Saxon.Hej.type.FunctionItemType)) || (t2 is Saxon.Hej.type.JavaExternalObjectType))) {
                                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                            } else {
                                if (((t1 == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) && t2.isPlainType())) {
                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                } else {
                                    if ((t2 == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                    } else {
                                        if (((t1 is Saxon.Hej.type.AtomicType) && (t2 is Saxon.Hej.type.AtomicType))) {
                                            if ((((Saxon.Hej.type.AtomicType)(t1)).getFingerprint() == ((Saxon.Hej.type.AtomicType)(t2)).getFingerprint())) {
                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                            }
                                            Saxon.Hej.type.AtomicType t = (Saxon.Hej.type.AtomicType)(t2);
                                            while (true) {
                                                if ((((Saxon.Hej.type.AtomicType)(t1)).getFingerprint() == t.getFingerprint())) {
                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                }
                                                Saxon.Hej.type.SchemaType st = t.getBaseType();
                                                if ((st is Saxon.Hej.type.AtomicType)) {
                                                    t = (Saxon.Hej.type.AtomicType)(st);
                                                } else {
                                                    break ;
                                                }
                                            }
                                            t = (Saxon.Hej.type.AtomicType)(t1);
                                            while (true) {
                                                if ((t.getFingerprint() == ((Saxon.Hej.type.AtomicType)(t2)).getFingerprint())) {
                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                }
                                                Saxon.Hej.type.SchemaType st = t.getBaseType();
                                                if ((st is Saxon.Hej.type.AtomicType)) {
                                                    t = (Saxon.Hej.type.AtomicType)(st);
                                                } else {
                                                    break ;
                                                }
                                            }
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                        } else {
                                            if ((!(t1.isAtomicType()) && t2.isPlainType())) {
                                                System.Collections.Generic.ISet<Saxon.Hej.type.PlainType> s1 = toSet(((Saxon.Hej.type.PlainType)(t1)).getPlainMemberTypes());
                                                System.Collections.Generic.ISet<Saxon.Hej.type.PlainType> s2 = toSet(((Saxon.Hej.type.PlainType)(t2)).getPlainMemberTypes());
                                                if (!(unionOverlaps(s1, s2))) {
                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                                }
                                                bool gt = (s2).All(s1.Contains);
                                                bool lt = (s1).All(s2.Contains);
                                                if ((gt && lt)) {
                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                                } else {
                                                    if (gt) {
                                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                    } else {
                                                        if (lt) {
                                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                        } else {
                                                            if (unionSubsumes(s1, s2)) {
                                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                            } else {
                                                                if (unionSubsumes(s2, s1)) {
                                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                                } else {
                                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            } else {
                                                if ((t1 is Saxon.Hej.type.AtomicType)) {
                                                    Saxon.Hej.type.Affinity r = relationship(t2, t1);
                                                    return inverseRelationship(r);
                                                } else {
                                                    throw new Saxon.Ejava.lang.IllegalStateException();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } else {
                            if ((t1 is Saxon.Hej.pattern.NodeTest)) {
                                if ((t2.isPlainType() || (t2 is Saxon.Hej.type.FunctionItemType))) {
                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                } else {
                                    if ((t1 is Saxon.Hej.pattern.AnyNodeTest)) {
                                        if ((t2 is Saxon.Hej.pattern.AnyNodeTest)) {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                        } else {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                        }
                                    } else {
                                        if ((t2 is Saxon.Hej.pattern.AnyNodeTest)) {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                        } else {
                                            if ((t2 is Saxon.Hej.type.ErrorType)) {
                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                            } else {
                                                Saxon.Hej.type.Affinity nodeKindRelationship;
                                                Saxon.Hej.type.UType m1 = t1.getUType();
                                                Saxon.Hej.type.UType m2 = t2.getUType();
                                                if (!(m1.overlaps(m2))) {
                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                                } else {
                                                    if (m1.Equals(m2)) {
                                                        nodeKindRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                                    } else {
                                                        if (m2.subsumes(m1)) {
                                                            nodeKindRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                        } else {
                                                            if (m1.subsumes(m2)) {
                                                                nodeKindRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                            } else {
                                                                nodeKindRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                                                            }
                                                        }
                                                    }
                                                }
                                                Saxon.Hej.type.Affinity nodeNameRelationship;
                                                Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> on1 = ((Saxon.Hej.pattern.NodeTest)(t1)).getRequiredNodeNames();
                                                Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> on2 = ((Saxon.Hej.pattern.NodeTest)(t2)).getRequiredNodeNames();
                                                if (((t1 is Saxon.Hej.pattern.QNameTest) && (t2 is Saxon.Hej.pattern.QNameTest))) {
                                                    nodeNameRelationship = nameTestRelationship((Saxon.Hej.pattern.QNameTest)(t1), (Saxon.Hej.pattern.QNameTest)(t2));
                                                } else {
                                                    if ((on1.isPresent() && (on1.get() is Saxon.Hej.z.IntUniversalSet))) {
                                                        if ((on2.isPresent() && (on2.get() is Saxon.Hej.z.IntUniversalSet))) {
                                                            nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                                        } else {
                                                            nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                        }
                                                    } else {
                                                        if ((on2.isPresent() && (on2.get() is Saxon.Hej.z.IntUniversalSet))) {
                                                            nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                        } else {
                                                            if (!(((on1.isPresent() && on2.isPresent())))) {
                                                                nodeNameRelationship = (t1.Equals(t2) ? /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE : /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS);
                                                            } else {
                                                                Saxon.Hej.z.IntSet n1 = on1.get();
                                                                Saxon.Hej.z.IntSet n2 = on2.get();
                                                                if (n1.containsAll(n2)) {
                                                                    if ((n1.size() == n2.size())) {
                                                                        nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                                                    } else {
                                                                        nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                                    }
                                                                } else {
                                                                    if (n2.containsAll(n1)) {
                                                                        nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                                    } else {
                                                                        if (Saxon.Hej.z.IntHashSet.containsSome(n1, n2)) {
                                                                            nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                                                                        } else {
                                                                            nodeNameRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                Saxon.Hej.type.Affinity contentRelationship = computeContentRelationship(t1, t2, on1, on2);
                                                if ((((nodeKindRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) && (nodeNameRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE)) && (contentRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE))) {
                                                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                                } else {
                                                    if ((((((nodeKindRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (nodeKindRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES))) && (((nodeNameRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (nodeNameRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES)))) && (((contentRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (contentRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES))))) {
                                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                    } else {
                                                        if ((((((nodeKindRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (nodeKindRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))) && (((nodeNameRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (nodeNameRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY)))) && (((contentRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (contentRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                        } else {
                                                            if (((nodeNameRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT) || (contentRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT))) {
                                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                                            } else {
                                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } else {
                                if ((t1 is Saxon.Hej.type.AnyExternalObjectType)) {
                                    if (!(((t2 is Saxon.Hej.type.AnyExternalObjectType)))) {
                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                    }
                                    if ((t1 is Saxon.Hej.type.JavaExternalObjectType)) {
                                        if ((t2 == Saxon.Hej.type.AnyExternalObjectType.THE_INSTANCE)) {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                        } else {
                                            if ((t2 is Saxon.Hej.type.JavaExternalObjectType)) {
                                                return ((Saxon.Hej.type.JavaExternalObjectType)(t1)).getRelationship((Saxon.Hej.type.JavaExternalObjectType)(t2));
                                            } else {
                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                            }
                                        }
                                    }
                                    if ((t2 is Saxon.Hej.type.JavaExternalObjectType)) {
                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                    } else {
                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                    }
                                } else {
                                    if (((t1 is Saxon.Hej.ma.map.MapType) && (t2 is Saxon.Hej.ma.map.MapType))) {
                                        if ((t1 == Saxon.Hej.ma.map.MapType.EMPTY_MAP_TYPE)) {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                        } else {
                                            if ((t2 == Saxon.Hej.ma.map.MapType.EMPTY_MAP_TYPE)) {
                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                            }
                                        }
                                        if ((t1 == Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE)) {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                        } else {
                                            if ((t2 == Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE)) {
                                                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                            }
                                        }
                                        Saxon.Hej.type.AtomicType k1 = ((Saxon.Hej.ma.map.MapType)(t1)).getKeyType();
                                        Saxon.Hej.type.AtomicType k2 = ((Saxon.Hej.ma.map.MapType)(t2)).getKeyType();
                                        Saxon.Hej.value.SequenceType v1 = ((Saxon.Hej.ma.map.MapType)(t1)).getValueType();
                                        Saxon.Hej.value.SequenceType v2 = ((Saxon.Hej.ma.map.MapType)(t2)).getValueType();
                                        Saxon.Hej.type.Affinity keyRel = relationship(k1, k2);
                                        Saxon.Hej.type.Affinity valueRel = sequenceTypeRelationship(v1, v2);
                                        Saxon.Hej.type.Affinity rel = combineRelationships(keyRel, valueRel);
                                        if ((((rel == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES)) || (rel == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                            return rel;
                                        }
                                    }
                                    if ((t2 is Saxon.Hej.type.FunctionItemType)) {
                                        Saxon.Hej.type.Affinity signatureRelationship = ((Saxon.Hej.type.FunctionItemType)(t1)).relationship((Saxon.Hej.type.FunctionItemType)(t2), this);
                                        if ((signatureRelationship == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) {
                                            return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                        } else {
                                            Saxon.Hej.type.Affinity assertionRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                            Saxon.Hej.query.AnnotationList first = ((Saxon.Hej.type.FunctionItemType)(t1)).getAnnotationAssertions();
                                            Saxon.Hej.query.AnnotationList second = ((Saxon.Hej.type.FunctionItemType)(t2)).getAnnotationAssertions();
                                            System.Collections.Generic.ISet<string> namespaces = new System.Collections.Generic.HashSet<string>();
                                            foreach (Saxon.Hej.query.Annotation a in first) {
                                                namespaces.Add(a.getAnnotationQName().getURI());
                                            }
                                            foreach (Saxon.Hej.query.Annotation a in second) {
                                                namespaces.Add(a.getAnnotationQName().getURI());
                                            }
                                            foreach (string ns in namespaces) {
                                                Saxon.Hej.lib.FunctionAnnotationHandler handler = config.getFunctionAnnotationHandler(ns);
                                                if ((handler != null)) {
                                                    Saxon.Hej.type.Affinity localRel = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
                                                    Saxon.Hej.query.AnnotationList firstFiltered = first.filterByNamespace(ns);
                                                    Saxon.Hej.query.AnnotationList secondFiltered = second.filterByNamespace(ns);
                                                    if (firstFiltered.isEmpty()) {
                                                        if (secondFiltered.isEmpty()) {
                                                        } else {
                                                            localRel = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                                                        }
                                                    } else {
                                                        if (secondFiltered.isEmpty()) {
                                                            localRel = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                                        } else {
                                                            localRel = handler.relationship(firstFiltered, secondFiltered);
                                                        }
                                                    }
                                                    assertionRelationship = combineRelationships(assertionRelationship, localRel);
                                                }
                                            }
                                            return combineRelationships(signatureRelationship, assertionRelationship);
                                        }
                                    } else {
                                        return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
            }
        }
        private static void requireTrueItemType(Saxon.Hej.type.ItemType t) {
            Saxon.Ejava.util.Objects.requireNonNull(t);
            if (((t is Saxon.Hej.type.UnionType) && !(t.isPlainType()))) {
                throw new Saxon.Ejava.lang.AssertionError((t+" is a non-pure union type"));
            }
        }
        private static Saxon.Hej.type.Affinity nameTestRelationship(Saxon.Hej.pattern.QNameTest t1, Saxon.Hej.pattern.QNameTest t2) {
            if (t1.Equals(t2)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
            }
            if ((t2 is Saxon.Hej.pattern.NameTest)) {
                return (t1.matches(((Saxon.Hej.pattern.NameTest)(t2)).getMatchingNodeName()) ? /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES : /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT);
            }
            if ((t1 is Saxon.Hej.pattern.NameTest)) {
                return (t2.matches(((Saxon.Hej.pattern.NameTest)(t1)).getMatchingNodeName()) ? /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY : /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT);
            }
            if ((t2 is Saxon.Hej.pattern.SameNameTest)) {
                return (t1.matches(((Saxon.Hej.pattern.SameNameTest)(t2)).getMatchingNodeName()) ? /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES : /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT);
            }
            if ((t1 is Saxon.Hej.pattern.SameNameTest)) {
                return (t2.matches(((Saxon.Hej.pattern.SameNameTest)(t1)).getMatchingNodeName()) ? /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY : /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT);
            }
            if (((t1 is Saxon.Hej.pattern.NamespaceTest) && (t2 is Saxon.Hej.pattern.NamespaceTest))) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
            }
            if (((t1 is Saxon.Hej.pattern.LocalNameTest) && (t2 is Saxon.Hej.pattern.LocalNameTest))) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
            }
            return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
        }
        private static Saxon.Hej.type.Affinity combineRelationships(Saxon.Hej.type.Affinity rel1, Saxon.Hej.type.Affinity rel2) {
            if (((rel1 == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) && (rel2 == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE))) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
            } else {
                if (((((rel1 == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (rel1 == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES))) && (((rel2 == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (rel2 == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES))))) {
                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                } else {
                    if (((((rel1 == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (rel1 == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))) && (((rel2 == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (rel2 == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                        return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if (((rel1 == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT) || (rel2 == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT))) {
                            return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                        } else {
                            return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                        }
                    }
                }
            }
        }
        private static System.Collections.Generic.ISet<X> toSet<X> (System.Collections.Generic.IEnumerable<X> @in) {
            System.Collections.Generic.ISet<X> s = new System.Collections.Generic.HashSet<X>();
            foreach (X x in @in) {
                s.Add(x);
            }
            return s;
        }
        private bool unionSubsumes(System.Collections.Generic.ISet<Saxon.Hej.type.PlainType> s1, System.Collections.Generic.ISet<Saxon.Hej.type.PlainType> s2) {
            foreach (Saxon.Hej.type.PlainType t2 in s2) {
                bool t2isSubsumed = false;
                foreach (Saxon.Hej.type.PlainType t1 in s1) {
                    Saxon.Hej.type.Affinity rel = relationship(t1, t2);
                    if (((rel == /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES) || (rel == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE))) {
                        t2isSubsumed = true;
                        break ;
                    }
                }
                if (!(t2isSubsumed)) {
                    return false;
                }
            }
            return true;
        }
        private bool unionOverlaps(System.Collections.Generic.ISet<Saxon.Hej.type.PlainType> s1, System.Collections.Generic.ISet<Saxon.Hej.type.PlainType> s2) {
            foreach (Saxon.Hej.type.PlainType t2 in s2) {
                foreach (Saxon.Hej.type.PlainType t1 in s1) {
                    Saxon.Hej.type.Affinity rel = relationship(t1, t2);
                    if ((rel != /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) {
                        return true;
                    }
                }
            }
            return false;
        }
        protected internal virtual Saxon.Hej.type.Affinity computeContentRelationship(Saxon.Hej.type.ItemType t1, Saxon.Hej.type.ItemType t2, Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> n1, Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> n2) {
            Saxon.Hej.type.Affinity contentRelationship;
            if ((t1 is Saxon.Hej.pattern.DocumentNodeTest)) {
                if ((t2 is Saxon.Hej.pattern.DocumentNodeTest)) {
                    contentRelationship = relationship(((Saxon.Hej.pattern.DocumentNodeTest)(t1)).getElementTest(), ((Saxon.Hej.pattern.DocumentNodeTest)(t2)).getElementTest());
                } else {
                    contentRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                }
            } else {
                if ((t2 is Saxon.Hej.pattern.DocumentNodeTest)) {
                    contentRelationship = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                } else {
                    Saxon.Hej.type.SchemaType s1 = ((Saxon.Hej.pattern.NodeTest)(t1)).getContentType();
                    Saxon.Hej.type.SchemaType s2 = ((Saxon.Hej.pattern.NodeTest)(t2)).getContentType();
                    contentRelationship = schemaTypeRelationship(s1, s2);
                }
            }
            bool nillable1 = ((Saxon.Hej.pattern.NodeTest)(t1)).isNillable();
            bool nillable2 = ((Saxon.Hej.pattern.NodeTest)(t2)).isNillable();
            if ((nillable1 != nillable2)) {
                switch (contentRelationship) {
                  case Saxon.Hej.type.Affinity.SUBSUMES:
                    if (nillable2) {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                    }
                    break ;
                  case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                    if (nillable1) {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                    }
                    break ;
                  case Saxon.Hej.type.Affinity.SAME_TYPE:
                    if (nillable1) {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                    } else {
                        contentRelationship = /*implicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    }
                    break ;
                  default:
                    break ;
                }
            }
            return contentRelationship;
        }
        public Saxon.Hej.type.Affinity sequenceTypeRelationship(Saxon.Hej.value.SequenceType s1, Saxon.Hej.value.SequenceType s2) {
            int c1 = s1.getCardinality();
            int c2 = s2.getCardinality();
            Saxon.Hej.type.Affinity cardRel;
            if ((c1 == c2)) {
                cardRel = /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
            } else {
                if (Saxon.Hej.value.Cardinality.subsumes(c1, c2)) {
                    cardRel = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                } else {
                    if (Saxon.Hej.value.Cardinality.subsumes(c2, c1)) {
                        cardRel = /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if (((c1 == Saxon.Hej.expr.StaticProperty.EMPTY) && !(Saxon.Hej.value.Cardinality.allowsZero(c2)))) {
                            return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                        } else {
                            if (((c2 == Saxon.Hej.expr.StaticProperty.EMPTY) && !(Saxon.Hej.value.Cardinality.allowsZero(c1)))) {
                                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
                            } else {
                                cardRel = /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
                            }
                        }
                    }
                }
            }
            Saxon.Hej.type.Affinity itemRel = relationship(s1.getPrimaryType(), s2.getPrimaryType());
            if ((itemRel == /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
            }
            if (((cardRel == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE) || (cardRel == itemRel))) {
                return itemRel;
            }
            if ((itemRel == /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE)) {
                return cardRel;
            }
            return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
        }
        public Saxon.Hej.type.Affinity schemaTypeRelationship(Saxon.Hej.type.SchemaType s1, Saxon.Hej.type.SchemaType s2) {
            if (s1.isSameType(s2)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
            }
            if ((s1 is Saxon.Hej.type.AnyType)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
            }
            if ((s2 is Saxon.Hej.type.AnyType)) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
            }
            if (((s1 is Saxon.Hej.type.Untyped) && (((s2 == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || (s2 == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))))) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
            }
            if (((s2 is Saxon.Hej.type.Untyped) && (((s1 == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || (s1 == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))))) {
                return /*explicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
            }
            if (((((s1 is Saxon.Hej.type.PlainType) && ((Saxon.Hej.type.PlainType)(s1)).isPlainType()) && (s2 is Saxon.Hej.type.PlainType)) && ((Saxon.Hej.type.PlainType)(s2)).isPlainType())) {
                return relationship((Saxon.Hej.type.ItemType)(s1), (Saxon.Hej.type.ItemType)(s2));
            }
            Saxon.Hej.type.SchemaType t1 = s1;
            while (((t1 = t1.getBaseType()) != null)) {
                if (t1.isSameType(s2)) {
                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
                }
            }
            Saxon.Hej.type.SchemaType t2 = s2;
            while (((t2 = t2.getBaseType()) != null)) {
                if (t2.isSameType(s1)) {
                    return /*explicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
                }
            }
            return /*explicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
        }
        public static Saxon.Hej.type.Affinity inverseRelationship(Saxon.Hej.type.Affinity relation) {
            switch (relation) {
              case Saxon.Hej.type.Affinity.SAME_TYPE:
                return /*implicit enum*/ Saxon.Hej.type.Affinity.SAME_TYPE;
              case Saxon.Hej.type.Affinity.SUBSUMES:
                return /*implicit enum*/ Saxon.Hej.type.Affinity.SUBSUMED_BY;
              case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                return /*implicit enum*/ Saxon.Hej.type.Affinity.SUBSUMES;
              case Saxon.Hej.type.Affinity.OVERLAPS:
                return /*implicit enum*/ Saxon.Hej.type.Affinity.OVERLAPS;
              case Saxon.Hej.type.Affinity.DISJOINT:
                return /*implicit enum*/ Saxon.Hej.type.Affinity.DISJOINT;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public virtual Saxon.Hej.type.ItemType getGenericFunctionItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        private class ItemTypePair {
            internal Saxon.Hej.type.ItemType s;
            internal Saxon.Hej.type.ItemType t;
            public ItemTypePair(Saxon.Hej.type.ItemType s, Saxon.Hej.type.ItemType t)  {
                this.s = s;
                this.t = t;
            }
            public override int GetHashCode() {
                return (s.GetHashCode()^t.GetHashCode());
            }
            public override bool Equals(System.Object obj) {
                if ((obj is Saxon.Hej.type.TypeHierarchy.ItemTypePair)) {
                    Saxon.Hej.type.TypeHierarchy.ItemTypePair pair = (Saxon.Hej.type.TypeHierarchy.ItemTypePair)(obj);
                    return (s.Equals(pair.s) && t.Equals(pair.t));
                } else {
                    return false;
                }
            }
        
        }
    
    }
}