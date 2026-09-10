package net.sf.saxon.type;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.expr.parser.TypeChecker;
import net.sf.saxon.functions.hof.FunctionSequenceCoercer;
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

/**
 * Implementation of the coercion rules (3.0: function conversion rules) for dynamic
 * function calls. Converts a value supplied as an argument in a function call to
 * the required type. This is used primarily for dynamic function calls; for static calls
 * see the {@link TypeChecker} class, which converts expressions rather than values. The class
 * is also used for converting external parameters to the required type.
 *
 * <p>There are separate versions of this class for XPath 3.1 and XPath 4.0.</p>
 */
public abstract class CoercionRules {


    protected Configuration config;
    protected TypeHierarchy th;


    public static CoercionRules forVersion(Configuration config, int version) {
        return (version >= 40) ? new CoercionRules40(config) : new CoercionRules31(config);
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param value        a value to be converted
     * @param requiredType the required type
     * @param knownType    the item type of the supplied value, to the extent this is known
     * @param config       the Saxon configuration
     * @param roleSupplier identifies the value to be converted in error messages
     * @param locator      identifies the location for error messages
     * @return the converted value (possibly as a lazily-evaluated sequence)
     * @throws net.sf.saxon.trans.XPathException if the value cannot be converted to the required type
     */

    public Sequence coerce(Sequence value,
                                SequenceType requiredType,
                                SequenceType knownType,
                                Configuration config,
                                Supplier<RoleDiagnostic> roleSupplier,
                                Location locator)
            throws XPathException {

        TypeHierarchy th = config.getTypeHierarchy();
        ItemType knownItemType = knownType.getPrimaryType();
        int knownCardinality = knownType.getCardinality();

        try {
            // The aim is to do lazy evaluation. This means that the type of the supplied
            // value isn't necessarily known at this stage. If the value to be coerced is
            // supplied as a GroundedValue then we use its actual type. If not, we rely
            // on information supplied by the caller about the static type.
            if (value instanceof GroundedValue) {
                knownItemType = SequenceTool.getItemType(value, th);
                knownCardinality = SequenceTool.getCardinality(value);
                knownType = SequenceType.makeSequenceType(knownItemType, knownCardinality);
            }

            SequenceIterator iterator = value.iterate();
            final ItemType requiredItemType = requiredType.getPrimaryType();

            // Step 1: atomization. This is done separately since atomizing a node can generate
            // multiple atomic values.

            if (requiredItemType.isPlainType() && !knownItemType.isPlainType()) {

                // step 1(a): apply atomization if necessary

                try {
                    iterator = Atomizer.getAtomizingIterator(iterator, false);
                } catch (XPathException e) {
                    RoleDiagnostic role = roleSupplier.get();
                    ValidationFailure vf = new ValidationFailure(
                            "Failed to atomize the " + role.getMessage() + ": " + e.getMessage());
                    vf.setErrorCode("XPTY0117");
                    throw vf.makeException();
                }
                knownItemType = knownItemType.getAtomizedItemType();
                // TODO: modify the cardinality??

            }

            // Check if this is all we need to do

            if (th.isSubType(knownItemType, requiredItemType)) {
                if (requiredType.getCardinality() != StaticProperty.ALLOWS_ZERO_OR_MORE) {
                    iterator = new CardinalityCheckingIterator(
                            iterator, requiredType.getCardinality(), roleSupplier, locator);
                }
                return new MemoSequence(iterator);
            }

            UType knownUType = knownItemType.getUType();

            // The rest of the processing is done item-by-item. The aim is to construct an
            // ItemMappingFunction which can be applied to each item in the input sequence.

            // Step 2: promote untyped atomic values to the required type

            ItemMappingFunction mapper = getUntypedAtomicPromoter(
                    requiredItemType, knownItemType, roleSupplier);

            // Step 3: promote atomic types, for example decimal to double

            mapper = chain(mapper, getTypePromoter(requiredItemType, knownUType));

            // Step 4: (4.0 only) relabel values as members of a subtype

            mapper = chain(mapper, getRelabeller(requiredItemType, knownUType, roleSupplier, locator));

            // Step 5: if the required type is a record, apply the rules recursively to each field

            mapper = chain(mapper, getRecordTestMapper(requiredItemType, knownUType, roleSupplier, locator));

            // Step 6: if the required type is a function, apply function coercion

            mapper = chain(mapper, getFunctionCoercer(requiredItemType, knownItemType, locator));

            // Step 7: check that the final item type matches the required type

            mapper = chain(mapper, new ItemTypeCheckingFunction(
                    requiredItemType, roleSupplier, locator, config));

            // Construct an iterator to apply the mapping to each item in the sequence

            if (mapper != null) {
                iterator = new ItemMappingIterator(iterator, mapper, false);
            }

            // Check the cardinality of the result

            if (requiredType.getCardinality() != StaticProperty.ALLOWS_ZERO_OR_MORE) {
                iterator = new CardinalityCheckingIterator(iterator, requiredType.getCardinality(), roleSupplier, locator);
            }

            // Return a lazy sequence that is constructed on demand by reading this iterator

            return new MemoSequence(iterator);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }

    }

    /**
     * Construct an ItemMappingFunction by composing two supplied ItemMappingFunctions
     * @param mf1 the first ItemMappingFunction, possibly null
     * @param mf2 the second ItemMappingFunction, possibly null
     * @return the composed function
     */
    protected ItemMappingFunction chain(ItemMappingFunction mf1, ItemMappingFunction mf2) {
        if (mf1 == null) {
            return mf2;
        }
        if (mf2 == null) {
            return mf1;
        }
        return ItemMapper.of(item -> {
            Item temp = mf1.mapItem(item);
            if (temp != null) {
                return mf2.mapItem(temp);
            } else {
                return null;
            }
        });
    }

    protected abstract ItemMappingFunction getTypePromoter(ItemType requiredItemType, UType knownUType);

    private ItemMappingFunction getUntypedAtomicPromoter(
            ItemType requiredItemType, ItemType suppliedItemType, Supplier<RoleDiagnostic> roleSupplier) {

        if (requiredItemType.isPlainType() &&
                th.relationship(suppliedItemType, BuiltInAtomicType.UNTYPED_ATOMIC) != DISJOINT &&
                !th.isSubType(BuiltInAtomicType.UNTYPED_ATOMIC, requiredItemType)) {
            final boolean nsSensitive = ((PlainType) requiredItemType).isNamespaceSensitive();
            if (nsSensitive) {
                return ItemMapper.of(item -> {
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
                EnumerationType enumType = (EnumerationType) requiredItemType;
                return ItemMapper.of(item -> {
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
                return ItemMapper.of(item -> {
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
                return ItemMapper.of(item -> {
                    if (item instanceof AtomicValue && ((AtomicValue) item).isUntypedAtomic()) {
                        return Converter.convert(
                                (StringValue) item, (AtomicType) requiredItemType, config.getConversionRules());
                    } else {
                        return item;
                    }
                });
            }
        }
        return null;
    }

    protected abstract ItemMappingFunction getRelabeller(
            ItemType requiredItemType, UType knownUType, Supplier<RoleDiagnostic> roleSupplier, Location locator);

    protected abstract ItemMappingFunction getRecordTestMapper(
            ItemType requiredItemType, UType knownUType, Supplier<RoleDiagnostic> roleSupplier, Location locator);

    private ItemMappingFunction getFunctionCoercer(ItemType requiredItemType, ItemType suppliedItemType, Location locator) {
        if (requiredItemType instanceof FunctionItemType && !((FunctionItemType) requiredItemType).isMapType()
                && !((FunctionItemType) requiredItemType).isArrayType()
                && !(th.relationship(requiredItemType, suppliedItemType) == Affinity.SUBSUMES)) {

            if (requiredItemType == AnyFunctionType.getInstance()) {
                // no action (the type checking is added later)
                return null;

            } else {

                return new FunctionSequenceCoercer.Coercer(
                        (SpecificFunctionType) requiredItemType, config, locator, false, false);

            }

        } else {
            return null;
        }
    }

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
        public CoercionRules31(Configuration config) {
            this.config = config;
            this.th = config.getTypeHierarchy();
        }

        protected ItemMappingFunction getTypePromoter(ItemType requiredItemType, UType knownUType) {
            if (requiredItemType == BuiltInAtomicType.STRING && knownUType.overlaps(UType.ANY_URI)) {
                return ItemMapper.of(item -> {
                    if (((AtomicValue) item).getPrimitiveType() == BuiltInAtomicType.ANY_URI) {
                        return new StringValue(item.getUnicodeStringValue());
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.DOUBLE && knownUType.overlaps(UType.NUMERIC)) {
                return ItemMapper.of(item -> {
                    if (item instanceof NumericValue && !(item instanceof DoubleValue)) {
                        return new DoubleValue(((NumericValue) item).getDoubleValue());
                    } else {
                        return item;
                    }
                });
            } else {
                return null;
            }
        }

        protected ItemMappingFunction getRelabeller(
                ItemType requiredItemType, UType knownUType, Supplier<RoleDiagnostic>roleSupplier, Location locator) {
            return null;
        }

        @Override
        protected ItemMappingFunction getRecordTestMapper(
                ItemType requiredItemType, UType knownUType, Supplier<RoleDiagnostic> roleSupplier, Location locator) {
            return null;
        }
    }

    private static class CoercionRules40 extends CoercionRules {

        public CoercionRules40(Configuration config) {
            this.config = config;
            this.th = config.getTypeHierarchy();
        }

        protected ItemMappingFunction getTypePromoter(ItemType requiredItemType, UType knownUType) {
            if (requiredItemType == BuiltInAtomicType.STRING && knownUType.overlaps(UType.ANY_URI)) {
                return ItemMapper.of(item -> {
                    if (((AtomicValue) item).getPrimitiveType() == BuiltInAtomicType.ANY_URI) {
                        return new StringValue(item.getUnicodeStringValue());
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.ANY_URI && knownUType.overlaps(UType.STRING)) {
                return ItemMapper.of(item -> {
                    if (((AtomicValue) item).getPrimitiveType() == BuiltInAtomicType.STRING) {
                        return new StringValue(item.getUnicodeStringValue(), BuiltInAtomicType.ANY_URI);
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.DOUBLE && knownUType.overlaps(UType.NUMERIC)) {
                return ItemMapper.of(item -> {
                    if (item instanceof NumericValue && !(item instanceof DoubleValue)) {
                        return new DoubleValue(((NumericValue) item).getDoubleValue());
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.DECIMAL && knownUType.overlaps(UType.NUMERIC)) {
                return ItemMapper.of(item -> {
                    if (item instanceof NumericValue && !(item instanceof DecimalValue)) {
                        return new BigDecimalValue(((NumericValue) item).getDecimalValue());
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.FLOAT && knownUType.overlaps(UType.NUMERIC)) {
                return ItemMapper.of(item -> {
                    if (item instanceof NumericValue && !(item instanceof FloatValue)) {
                        return new FloatValue(((NumericValue) item).getFloatValue());
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.HEX_BINARY && knownUType.overlaps(UType.BASE64_BINARY)) {
                return ItemMapper.of(item -> {
                    if (((AtomicValue) item).getPrimitiveType() == BuiltInAtomicType.BASE64_BINARY) {
                        return new HexBinaryValue(((Base64BinaryValue) item).getBinaryValue());
                    } else {
                        return item;
                    }
                });
            } else if (requiredItemType == BuiltInAtomicType.BASE64_BINARY && knownUType.overlaps(UType.HEX_BINARY)) {
                return ItemMapper.of(item -> {
                    if (((AtomicValue) item).getPrimitiveType() == BuiltInAtomicType.HEX_BINARY) {
                        return new HexBinaryValue(((HexBinaryValue) item).getBinaryValue());
                    } else {
                        return item;
                    }
                });
            } else {
                return null;
            }
        }

        protected ItemMappingFunction getRelabeller(ItemType requiredItemType, UType knownUType, Supplier<RoleDiagnostic> roleSupplier, Location locator) {
            if (requiredItemType instanceof AtomicType
                    && ((AtomicType) requiredItemType).getBaseType() != BuiltInAtomicType.ANY_ATOMIC) {
                AtomicType req = (AtomicType) requiredItemType;
                AtomicType prim = (AtomicType) ((AtomicType) requiredItemType).getPrimitiveItemType();
                ConversionRules rules = config.getConversionRules();
                return ItemMapper.of(item -> {
                    if (prim.matches(item, th)
                            && !requiredItemType.matches(item, th)
                            && req.validate((AtomicValue) item, null, rules) == null) {
                        return ((AtomicValue) item).copyAsSubType(req);
                    } else {
                        return item;
                    }
                });
            }

            // step 1(b)(v): relabel as restricted atomic type within membership of union type

            if (requiredItemType instanceof UnionType && requiredItemType.isPlainType()) {
                UnionType unionType = ((UnionType) requiredItemType);
                List<? extends PlainType> memberTypes = unionType.getPlainMemberTypes();
                ConversionRules rules = config.getConversionRules();
                return ItemMapper.of(item -> {
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
            }

            return null;
        }

        @Override
        protected ItemMappingFunction getRecordTestMapper(ItemType requiredItemType, UType knownUType, Supplier<RoleDiagnostic> roleSupplier, Location locator) {
            // If R is a record test, apply the rules recursively
            if (requiredItemType instanceof RecordTest) {
                RecordTest test = (RecordTest) requiredItemType;
                return ItemMapper.of(item -> {
                    if (item instanceof MapItem) {
                        MapItem m2 = (MapItem) item;
                        for (KeyValuePair kvp : m2.keyValuePairs()) {
                            if (kvp.key instanceof StringValue) {
                                SequenceType fieldType = test.getFieldType(kvp.key.getStringValue());
                                if (!fieldType.matches(kvp.value, th)) {
                                    GroundedValue convertedValue =
                                            coerce(kvp.value, fieldType, SequenceType.ANY_SEQUENCE, config, roleSupplier, locator)
                                                    .materialize();
                                    m2 = m2.addEntry(kvp.key, convertedValue);
                                }
                            }
                        }
                        return m2;
                    } else {
                        return item;
                    }
                });
            }
            return null;
        }
    }



}

