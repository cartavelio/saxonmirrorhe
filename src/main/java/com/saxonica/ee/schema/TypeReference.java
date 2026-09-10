package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.type.*;

import javax.xml.transform.SourceLocator;

/**
 * A reference to a simple or complex type
 */

public class TypeReference extends SchemaStructure implements ComponentReference {

    /**
     * The name of the type object referenced
     */
    private final StructuredQName qName;
    private final int fingerprint;
    private boolean dangling;

    /**
     * The corresponding SchemaType. This is null until the reference has been resolved.
     */

    private SchemaType target;

    /**
     * Creates a new Type reference
     *
     * @param fingerprint the Name Pool fingerprint of the name of the type being referenced
     * @param config      identifies the XML Schema in which this type reference appears.
     * @param locator     the SourceLocator identifying the location of the reference. May be null if the reference
     *                    is in a built-in type
     */
    public TypeReference(int fingerprint, EnterpriseConfiguration config, SourceLocator locator) {
        this.fingerprint = fingerprint;
        this.qName = config.getNamePool().getStructuredQName(fingerprint);
        if (locator != null) {
            setLocator(locator);
        }
        if (fingerprint < 1024) {
            setTarget(BuiltInType.getSchemaType(fingerprint));
        }
        setConfiguration(config);
    }

    /**
     * Determine whether this reference has been resolved
     */

    @Override
    public boolean isResolved() {
        return target != null;
    }

    public void setTarget(SchemaType target) {
        this.target = target;
    }

    public int getTargetFingerprint() {
        return fingerprint;
    }

    @Override
    public StructuredQName getTargetComponentName() {
        return qName;
    }

    /**
     * Ask whether this is known to be a dangling reference
     *
     * @return true if the target of this component reference is known to be absent, and if a warning has
     * been issued to this effect
     */
    @Override
    public boolean isDangling() {
        return dangling;
    }

    /**
     * Get the symbol space of the target of this reference
     */

    @Override
    public int getSymbolSpace() {
        return SymbolSpace.TYPE;
    }

    /**
     * Test whether this is a reference to an anonymous type
     *
     * @return true if this SchemaType is an anonymous type
     */

    public boolean isAnonymousType() {
        return getTargetFingerprint() > 1023 && getTargetComponentName().hasURI(NamespaceUri.ANONYMOUS);
    }


    /**
     * Returns the type that this type reference is a reference to
     *
     * @return the schema component if the reference has been resolved, or null otherwise
     */

    /*@NotNull*/
    @Override
    public SchemaComponent getTarget()  {
        if (target instanceof SimpleTypeDefinition) {
            UserSimpleType ust = ((SimpleTypeDefinition) target).getWorkingType();
            if (ust != null) {
                target = ust;
            }
        }
        return target;
    }

    /**
     * Try to resolve the reference if possible, relative to components present in a given schema.
     * If the reference cannot be resolved, the action depends on whether the compiler service allows
     * dangling references to absent components. If it does, we try to patch the schema up with a dummy
     * component (where possible). If not, the method throws an exception
     *
     * @param compiler The compiler being used to compile the referring component.
     * @param fatal true if a failure to resolve the reference is to be treated as fatal
     * @throws SchemaException if the reference cannot be resolved or repaired.
     */


    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) throws SchemaException {
        if (!dangling) {
            if (target == null) {
                target = compiler.getPreparedSchema().getType(qName);
            }
            if (target == null) {
                target = getConfiguration().getSuperSchema().getType(qName);
            }
            if (target == null) {
                if (compiler.isAllowDanglingReferences() && !fatal) {
                    dangling = true;
                    compiler.warning("Unknown type " + qName.getEQName() + " (substituting xs:error)", SaxonErrorCode.SXSD1008, this);
                    target = ErrorType.getInstance();
                } else {
                    dangling = true;
                    compiler.error("Unknown type " + qName.getEQName(), this);
                }
            }
        }
    }

}

// Copyright (c) 2004-2023 Saxonica Limited