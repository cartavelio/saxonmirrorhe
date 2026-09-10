package net.sf.saxon.type.coercion;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.StringValue;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. An AtomicCoercionPlan is used where the required
 * item type is atomic, and is further refined for specific atomic types where special rules
 * apply.
 */
public class AtomicCoercionPlan extends CoercionPlan {

    private final static AtomicCoercionPlan INSTANCE = new AtomicCoercionPlan();

    public static AtomicCoercionPlan getInstance() {
        return INSTANCE;
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param request the input to the coercion (note, the requiredType in the request
     *                must correspond to the item type that is handled by this particular
     *                {@code CoercionPlan}).
     * @return the converted value (possibly as a lazily-evaluated sequence)
     * @throws XPathException if the value cannot be converted to the required type
     */

    public Sequence coerce(CoercionRequest request) throws XPathException {

        TypeHierarchy th = request.config.getTypeHierarchy();
        ItemType knownItemType = request.knownType.getPrimaryType();
        int knownCardinality = request.knownType.getCardinality();

        try {
            // The aim is to do lazy evaluation. This means that the type of the supplied
            // value isn't necessarily known at this stage. If the value to be coerced is
            // supplied as a GroundedValue then we use its actual type. If not, we rely
            // on information supplied by the caller about the static type.
            if (request.value instanceof GroundedValue) {
                knownItemType = SequenceTool.getItemType(request.value, th);
                knownCardinality = SequenceTool.getCardinality(request.value);
            }

            SequenceIterator iterator = request.value.iterate();
            final ItemType requiredItemType = request.requiredType.getPrimaryType();

            // Step 1: atomization. This is done separately since atomizing a node can generate
            // multiple atomic values.

            if (!knownItemType.isPlainType()) {

                try {
                    iterator = Atomizer.getAtomizingIterator(iterator, false);
                } catch (XPathException e) {
                    RoleDiagnostic role = request.roleSupplier.get();
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
                if (request.requiredType.getCardinality() != StaticProperty.ALLOWS_ZERO_OR_MORE) {
                    iterator = new CardinalityCheckingIterator(
                            iterator, request.requiredType.getCardinality(), request.roleSupplier, request.locator);
                }
                return new MemoSequence(iterator);
            }

            UType knownUType = knownItemType.getUType();

            // The rest of the processing is done item-by-item. The aim is to construct an
            // ItemMappingFunction which can be applied to each item in the input sequence.

            // Step 2: promote untyped atomic values to the required type

            ItemMappingFunction mapper = getUntypedAtomicPromoter(request, knownUType);

            // Step 3: promote atomic types, for example decimal to double

            mapper = chain(mapper, getTypeSpecificPromoter(request, knownUType.union(requiredItemType.getUType())));

            // Step 7: check that the final item type matches the required type

            mapper = chain(mapper, new ItemTypeCheckingFunction(
                    requiredItemType, request.roleSupplier, request.locator, request.config));

            // Construct an iterator to apply the mapping to each item in the sequence

            if (mapper != null) {
                iterator = new ItemMappingIterator(iterator, mapper, false);
            }

            // Check the cardinality of the result

            int requiredCardinality = request.requiredType.getCardinality();
            if (!Cardinality.subsumes(requiredCardinality, knownCardinality)) {
                iterator = new CardinalityCheckingIterator(
                        iterator,
                        request.requiredType.getCardinality(),
                        request.roleSupplier,
                        request.locator);
            }

            // Return a lazy sequence that is constructed on demand by reading this iterator

            return new MemoSequence(iterator);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }

    }

    /**
     * This method handles untypedAtomic values in the atomized input. The default action is appropriate
     * for most atomic types; there are overrides for special cases such as handling of union types or
     * QNames.
     * @param request the coercion request
     * @param atomizedtype the type of the input after atomization, so far as this is known
     * @return a function to be applied to each item in the atomized input, or null if no
     *      * mapping is to be applied.
     */
    protected ItemMappingFunction getUntypedAtomicPromoter(CoercionRequest request, UType atomizedtype) {

        Configuration config = request.config;
        TypeHierarchy th = config.getTypeHierarchy();
        ItemType requiredItemType = request.requiredType.getPrimaryType();
        if (atomizedtype.overlaps(UType.UNTYPED_ATOMIC) &&
                !th.isSubType(BuiltInAtomicType.UNTYPED_ATOMIC, requiredItemType)) {
            if (((SimpleType) requiredItemType).isUnionType()) {
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

    /**
     * This method is implemented in subclasses that do type promotion to the target type, and
     * for other types such as Union types and Record types where special rules apply
     * @param request  the coercion request
     * @param atomizedType the type of the input (so far as it is known) after atomization
     *                     has been performed
     * @return a function to be applied to each item in the atomized input, or null if no
     * mapping is to be applied.
     */

    public ItemMappingFunction getTypeSpecificPromoter(CoercionRequest request, UType atomizedType) {
        return null;
    }

}


