package net.sf.saxon.type.coercion;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;

/**
 * A data package collecting together all the information needed when invoking a coercion.
 */
public class CoercionRequest {
    public Sequence value;
    public SequenceType requiredType;
    public SequenceType knownType;
    public Configuration config;
    public Supplier<RoleDiagnostic> roleSupplier;
    public Location locator;

    /**
     * Make a coercion request
     *
     * @param value        a value to be converted
     * @param requiredType the required type
     * @param knownType    the item type of the supplied value, to the extent this is known
     * @param config       the Saxon configuration
     * @param roleSupplier identifies the value to be converted, in error messages
     * @param locator      identifies the location, for error messages
     */

    public CoercionRequest(Sequence value,
                           SequenceType requiredType,
                           SequenceType knownType,
                           Configuration config,
                           Supplier<RoleDiagnostic> roleSupplier,
                           Location locator) {
        this.value = value;
        this.requiredType = requiredType;
        this.knownType = knownType;
        this.config = config;
        this.roleSupplier = roleSupplier;
        this.locator = locator;
    }
}

