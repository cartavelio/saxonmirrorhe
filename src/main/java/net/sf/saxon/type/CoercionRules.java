package net.sf.saxon.type;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.expr.parser.TypeChecker;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.*;

import java.util.List;
import java.util.function.Supplier;

import static net.sf.saxon.type.Affinity.DISJOINT;
import static net.sf.saxon.type.Affinity.SAME_TYPE;
import static net.sf.saxon.type.Affinity.SUBSUMED_BY;

/**
 * Implementation of the coercion rules (3.0: function conversion rules) for dynamic
 * function calls. Converts a value supplied as an argument in a function call to
 * the required type. This is used primarily for dynamic function calls; for static calls
 * see the {@link TypeChecker} class, which converts expressions rather than values. The class
 * is also used for converting external parameters to the required type.
 *
 *
 * <p>There are separate versions of this class for XPath 3.1 and XPath 4.0.</p>
 */
public abstract class CoercionRules {

    public static CoercionRules forVersion(int version) {
        return (version >= 40) ? CoercionRules40.getInstance() : CoercionRules31.getInstance();
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param value        a value to be converted
     * @param requiredType the required type
     * @param config       the Saxon configuration
     * @param roleSupplier identifies the value to be converted in error messages
     * @param locator      identifies the location for error messages
     * @return the converted value
     * @throws net.sf.saxon.trans.XPathException if the value cannot be converted to the required type
     */

    public GroundedValue coerce(Sequence value,
                                SequenceType requiredType,
                                Configuration config,
                                Supplier<RoleDiagnostic> roleSupplier,
                                Location locator)
            throws XPathException {

        TypeHierarchy th = config.getTypeHierarchy();
        try {
            GroundedValue groundedValue = value.materialize();
            if (requiredType.matches(groundedValue, th)) {
                return groundedValue;
            }

            ItemType suppliedItemType = SequenceTool.getItemType(value, th);

            SequenceIterator iterator = groundedValue.iterate();
            final ItemType requiredItemType = requiredType.getPrimaryType();

            if (requiredItemType.isPlainType()) {

                // step 1(a): apply atomization if necessary

                if (!suppliedItemType.isPlainType()) {
                    try {
                        iterator = Atomizer.getAtomizingIterator(iterator, false);
                    } catch (XPathException e) {
                        RoleDiagnostic role = roleSupplier.get();
                        ValidationFailure vf = new ValidationFailure(
                                "Failed to atomize the " + role.getMessage() + ": " + e.getMessage());
                        vf.setErrorCode("XPTY0117");
                        throw vf.makeException();
                    }
                    suppliedItemType = suppliedItemType.getAtomizedItemType();
                }

                // step 1(b)(ii): convert untyped atomic values to target item type

                if (th.relationship(suppliedItemType, BuiltInAtomicType.UNTYPED_ATOMIC) != DISJOINT &&
                        !th.isSubType(BuiltInAtomicType.UNTYPED_ATOMIC, requiredItemType)) {
                    final boolean nsSensitive = ((PlainType) requiredItemType).isNamespaceSensitive();
                    ItemMappingFunction converter;
                    if (nsSensitive) {
                        converter = ItemMapper.of(item -> {
                            if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                                RoleDiagnostic role = roleSupplier.get();
                                ValidationFailure vf = new ValidationFailure(
                                        "Failed to convert the " + role.getMessage() + ": " +
                                                "Implicit conversion of untypedAtomic value to " + requiredItemType +
                                                " is not allowed");
                                vf.setErrorCode("XPTY0117");
                                throw vf.makeException();
                            } else {
                                return item;
                            }
                        });
                    } else if (requiredItemType instanceof EnumerationType) {
                        EnumerationType enumType = (EnumerationType)requiredItemType;
                        converter = ItemMapper.of(item -> {
                            if (enumType.matches(item, th)) {
                                return item;
                            } else if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                                if (enumType.allowsValue(item.getStringValue())) {
                                    return new StringValue(item.getStringValue());
                                } else {
                                    throw makePromotionError(roleSupplier, enumType, item);
                                }
                            } else {
                                throw makePromotionError(roleSupplier, enumType, item);
                            }
                        });
                    } else if (((SimpleType) requiredItemType).isUnionType()) {
                        final ConversionRules rules = config.getConversionRules();
                        converter = ItemMapper.of(item -> {
                            if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                                try {
                                    return ((SimpleType) requiredItemType).getTypedValue(
                                            item.getUnicodeStringValue(), null, rules).head();
                                } catch (ValidationException ve) {
                                    throw ve.withErrorCode("XPTY0004");
                                }
                            } else {
                                return item;
                            }
                        });
                    } else {
                        converter = ItemMapper.of(item -> {
                            if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                                return Converter.convert(
                                        (StringValue) item, (AtomicType) requiredItemType, config.getConversionRules());
                            } else {
                                return item;
                            }
                        });
                    }
                    iterator = new ItemMappingIterator(iterator, converter, true);
                }

                // step 1(b)(iii): apply promotion rules

                iterator = applyPromotionRules(config, iterator, suppliedItemType, requiredItemType, roleSupplier);

                // step 1(b)(iv): relabel as restricted atomic type

                if (requiredItemType instanceof AtomicType && ((AtomicType)requiredItemType).getBaseType() != BuiltInAtomicType.ANY_ATOMIC) {
                    AtomicType req = (AtomicType)requiredItemType;
                    AtomicType prim = (AtomicType)((AtomicType)requiredItemType).getPrimitiveItemType();
                    ConversionRules rules = config.getConversionRules();
                    ItemMappingFunction relabeller = ItemMapper.of(item -> {
                        if (prim.matches(item, th)
                                && !requiredItemType.matches(item, th)
                                && req.validate((AtomicValue)item, null, rules) == null) {
                            return ((AtomicValue)item).copyAsSubType(req);
                        } else {
                            throw makePromotionError(roleSupplier,req, item);
                        }
                    });
                    iterator = new ItemMappingIterator(iterator, relabeller, true);
                }

                // step 1(b)(v): relabel as restricted atomic type within membership of union type

                if (requiredItemType instanceof UnionType && requiredItemType.isPlainType()) {
                    UnionType unionType = ((UnionType) requiredItemType);
                    List<? extends PlainType> memberTypes = unionType.getPlainMemberTypes();
                    ConversionRules rules = config.getConversionRules();
                    ItemMappingFunction relabeller = ItemMapper.of(item -> {
                        for (PlainType memberType : memberTypes) {
                            if (memberType.isAtomicType()) {
                                AtomicType prim = memberType.getPrimitiveItemType();
                                if (prim.matches(item, th)
                                        && ((AtomicType) memberType).validate((AtomicValue) item, null, rules) == null) {
                                    return ((AtomicValue) item).copyAsSubType((AtomicType) memberType);
                                }
                            }
                        }
                        throw makePromotionError(roleSupplier, unionType, item);
                    });
                    iterator = new ItemMappingIterator(iterator, relabeller, true);
                }
            }

            // 2: If R is a record test, apply the rules recursively

            if (requiredItemType instanceof RecordTest) {
                RecordTest test = (RecordTest)requiredItemType;
                ItemMappingFunction mapMapper = ItemMapper.of(item -> {
                    if (item instanceof MapItem) {
                        MapItem m2 = (MapItem)item;
                        for (KeyValuePair kvp : m2.keyValuePairs()) {
                            if (kvp.key instanceof StringValue) {
                                SequenceType fieldType = test.getFieldType(kvp.key.getStringValue());
                                if (!fieldType.matches(kvp.value, th)) {
                                    GroundedValue convertedValue = coerce(kvp.value, fieldType, config, roleSupplier, locator);
                                    m2 = m2.addEntry(kvp.key, convertedValue);
                                }
                            }
                        }
                        return m2;
                    } else {
                        return item;
                    }
                });
                iterator = new ItemMappingIterator(iterator, mapMapper);

            }

            // step 5: apply function coercion

            iterator = th.applyFunctionCoercion(iterator, suppliedItemType, requiredItemType, locator);

            // Add a check that the values conform to the required type

            Affinity relation = th.relationship(suppliedItemType, requiredItemType);
//
//            if (relation == DISJOINT) {
//                RoleDiagnostic role = roleSupplier.get();
//                throw new XPathException(
//                        "Failed to convert the " + role.getMessage() + ": " +
//                        "Cannot convert from " + suppliedItemType + " to "
//                                + requiredType, "XPTY0004");
//            }
            if (!(relation == SAME_TYPE || relation == SUBSUMED_BY)) {
                ItemTypeCheckingFunction itemChecker =
                        new ItemTypeCheckingFunction(requiredItemType, roleSupplier, locator, config);
                iterator = new ItemMappingIterator(iterator, itemChecker, true);
            }

            if (requiredType.getCardinality() != StaticProperty.ALLOWS_ZERO_OR_MORE) {
                iterator = new CardinalityCheckingIterator(iterator, requiredType.getCardinality(), roleSupplier, locator);
            }

            return SequenceTool.toGroundedValue(iterator);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }

    }

    protected abstract SequenceIterator applyPromotionRules(
            Configuration config,
            SequenceIterator iterator,
            ItemType suppliedItemType,
            ItemType requiredItemType,
            Supplier<RoleDiagnostic> roleSupplier);

    protected XPathException makePromotionError(
            Supplier<RoleDiagnostic> roleSupplier,
            ItemType requiredType,
            Item value) {
        RoleDiagnostic role = roleSupplier.get();
        return new XPathException(
                "Failed to convert the " + role.getMessage() + ": " +
                        "Cannot promote " + Err.depict(value) + " to "
                        + requiredType, "XPTY0004");
    }

    private static class CoercionRules31 extends CoercionRules {
        public static CoercionRules31 getInstance() {
            return new CoercionRules31();
        }

        protected SequenceIterator applyPromotionRules(
                Configuration config,
                SequenceIterator iterator,
                ItemType suppliedItemType,
                ItemType requiredItemType,
                Supplier<RoleDiagnostic> roleSupplier) {
            TypeHierarchy th = config.getTypeHierarchy();
            if (requiredItemType.equals(BuiltInAtomicType.DOUBLE)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof NumericValue) {
                        return Converter.convert(
                                (NumericValue) item, BuiltInAtomicType.DOUBLE, config.getConversionRules()).asAtomic();
                    } else {
                        throw makePromotionError(roleSupplier, requiredItemType, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.FLOAT)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof DoubleValue) {
                        RoleDiagnostic role = roleSupplier.get();
                        throw new XPathException(
                                "Failed to convert the " + role.getMessage() + ": " +
                                        "Cannot promote xs:double value to xs:float", "XPTY0004");
                    } else if (item instanceof NumericValue) {
                        return (FloatValue) Converter.convert(
                                (NumericValue) item, BuiltInAtomicType.FLOAT, config.getConversionRules()).asAtomic();
                    } else {
                        RoleDiagnostic role = roleSupplier.get();
                        throw new XPathException(
                                "Failed to convert the " + role.getMessage() + ": " +
                                        "Cannot promote non-numeric value to xs:float", "XPTY0004");
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            }

            // step 4: apply URI-to-string promotion

            if (requiredItemType.equals(BuiltInAtomicType.STRING) &&
                    th.relationship(suppliedItemType, BuiltInAtomicType.ANY_URI) != DISJOINT) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof AnyURIValue) {
                        return ((AnyURIValue) item).convertToString();
                    } else {
                        return item;
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            }
            return iterator;
        }
    }

    private static class CoercionRules40 extends CoercionRules {
        public static CoercionRules40 getInstance() {
            return new CoercionRules40();
        }

        protected SequenceIterator applyPromotionRules(
                Configuration config,
                SequenceIterator iterator,
                ItemType suppliedItemType,
                ItemType requiredItemType,
                Supplier<RoleDiagnostic> roleSupplier) {
            TypeHierarchy th = config.getTypeHierarchy();
            if (suppliedItemType.equals(requiredItemType)) {
                return iterator;
            }
            if (requiredItemType.equals(BuiltInAtomicType.DOUBLE)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof DoubleValue) {
                        return item;
                    }
                    if (item instanceof NumericValue) {
                        return Converter.convert(
                                        (NumericValue) item, (BuiltInAtomicType) requiredItemType, config.getConversionRules())
                                .asAtomic();
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.DOUBLE, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.FLOAT)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof FloatValue) {
                        return item;
                    }
                    if (item instanceof NumericValue) {
                        return Converter.convert(
                                        (NumericValue) item, (BuiltInAtomicType) requiredItemType, config.getConversionRules())
                                .asAtomic();
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.FLOAT, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.DECIMAL)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof DecimalValue) {
                        return item;
                    }
                    if (item instanceof NumericValue) {
                        return Converter.convert(
                                        (NumericValue) item, (BuiltInAtomicType) requiredItemType, config.getConversionRules())
                                .asAtomic();
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.DECIMAL, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.HEX_BINARY)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof HexBinaryValue) {
                        return item;
                    } else if (item instanceof Base64BinaryValue) {
                        return new HexBinaryValue(((Base64BinaryValue)item).getBinaryValue());
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.HEX_BINARY, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.BASE64_BINARY)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof Base64BinaryValue) {
                        return item;
                    } else if (item instanceof HexBinaryValue) {
                        return new Base64BinaryValue(((HexBinaryValue) item).getBinaryValue());
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.BASE64_BINARY, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.STRING)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof AnyURIValue) {
                        return new StringValue(((AnyURIValue)item).getContent());
                    } else if (item instanceof StringValue) {
                        return item;
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.STRING, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            } else if (requiredItemType.equals(BuiltInAtomicType.ANY_URI)) {
                ItemMappingFunction promoter = ItemMapper.of(item -> {
                    if (item instanceof AnyURIValue) {
                        return item;
                    } else if (item instanceof StringValue) {
                        return new AnyURIValue(((StringValue) item).getContent());
                    } else {
                        throw makePromotionError(roleSupplier, BuiltInAtomicType.ANY_URI, item);
                    }
                });
                iterator = new ItemMappingIterator(iterator, promoter, true);
            }
            return iterator;
        }
    }

}

