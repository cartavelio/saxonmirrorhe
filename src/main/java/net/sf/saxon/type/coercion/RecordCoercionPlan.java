package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.ma.map.StringMapBuilder;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.function.Supplier;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. This class handles the case where the required
 * type is a record type.
 */
public final class RecordCoercionPlan extends CoercionPlan {

    public final static RecordCoercionPlan INSTANCE = new RecordCoercionPlan();

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param item the map to be converted
     * @param requiredType the required record type
     * @param request      the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {
        if (item instanceof MapItem) {
            MapItem suppliedMap = (MapItem) item;
            RecordTest test = (RecordTest) requiredType;
            TypeHierarchy th = request.config.getTypeHierarchy();
            // We need to reorder the entries; so start with those that are present in the record type
            StringMapBuilder builder = new StringMapBuilder(suppliedMap.size());
            for (String s : test.getFieldNames()) {
                 StringValue key = new StringValue(s);
                 GroundedValue val = suppliedMap.get(key);
                 if (val == null) {
                     if (!test.isOptionalField(s)) {
                         throw coercionError(item, requiredType, request);
                     }
                 } else {
                     SequenceType fieldType = test.getFieldType(s);
                     if (fieldType != null) {
                         CoercionPlan fieldPlan = fieldType.getPrimaryType().getCoercionPlan(40);
                         if (!fieldType.matches(val, th)) {
                             Supplier<RoleDiagnostic> fieldRoleSupplier = () ->
                                     new RoleDiagnostic(RoleDiagnostic.OPTION, s, 0);
                             CoercionRequest subRequest = new CoercionRequest(
                                     SequenceType.ANY_SEQUENCE,
                                     request.config, fieldRoleSupplier, request.locator);
                             SequenceIterator convertedValue =
                                     fieldPlan.coerceSequence(val.iterate(), fieldType, subRequest);
                             builder.put(key.getUnicodeStringValue(), SequenceTool.toGroundedValue(convertedValue));
                         } else {
                             builder.put(key.getUnicodeStringValue(), val);
                         }
                     }
                 }
            }
            // Now handle any extra entries
            MapItem resultMap = null;
            if (suppliedMap.size() > test.getSize()) {
                if (!test.isExtensible()) {
                    throw coercionError(item, requiredType, request);
                }
                for (KeyValuePair kvp : suppliedMap.keyValuePairs()) {
                    if (kvp.key.getItemType() == BuiltInAtomicType.STRING) {
                        if (test.getFieldType(kvp.key.getStringValue()) != null) {
                            // Skip this entry, we have already dealt with it
                            continue;
                        }
                        if (resultMap == null) {
                            builder.put(kvp.key.getUnicodeStringValue(), kvp.value);
                            continue;
                        }
                    }
                    if (resultMap == null) {
                        // We have found an entry whose key is not an instance of xs:string, so we need
                        // to abandon the StringMapBuilder approach
                        resultMap = builder.getCompletedMapConfidently();
                    }
                    resultMap.put(kvp.key, kvp.value);
                }
            }
            if (resultMap == null) {
                resultMap = builder.getCompletedMapConfidently();
            }
            return resultMap;
            //check(m2, test, request);  // TODO: is this needed?
            //return m2;
        } else {
            throw coercionError(item, requiredType, request);
        }
    }


}


