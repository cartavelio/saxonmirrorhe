package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSet;

import java.util.*;

/**
 * A class that represents an XML Schema Wildcard. This is a helper class
 * used to support the two distinct but similar schema components ElementWildcard and
 * AttributeWildcard.
 * <p>A wildcard is represented by the XML elements xs:any and xs:anyAttribute</p>
 */
public class Wildcard extends SchemaStructure implements UserSchemaComponent, SerializableSchemaComponent, Term {

    /**
     * The set of permitted namespaces. The value "" represents the no-namespace, referred to as
     * "absent" in the specifications. The set may be empty, indicating that no namespaces are
     * explicitly allowed. If allowedNamespaces is non-null, then disallowedNamespaces must be null.
     */
    private Set<NamespaceUri> allowedNamespaces = null;

    /**
     * A namespace that is not permitted (supports ##other). The value "" represents the no-namespace,
     * referred to as "absent" in the specifications. Note that disallowing an explicit namespace N
     * permits all explicit namespaces other than N, but does not permit no-namespace names.
     */

    private Set<NamespaceUri> disallowedNamespaces = null;

    /**
     * the processContent of this wildcard.
     * (strict by default)
     */
    private String processContents = "strict";

    /**
     * Flag indicating that defined names are not allowed (notQName="##defined")
     */
    private boolean disallowsDefinedNames = false;

    /**
     * Flag indicating that defined siblings are not allowed (notQName="##definedSiblings")
     */
    private boolean disallowsDefinedSiblings = false;

    /**
     * The set of (fingerprints of) disallowed QNames
     */
    private IntSet disallowedQNames;

    public Wildcard(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Determine whether this wildcard represents an "inexpressible" intersection or union
     *
     * @return true if the intersection or union is inexpressible, as defined in XML Schema. This
     *         is essentially an error condition, so the wildcard should not be used if this property is
     *         true.
     */

    public boolean isInexpressible() {
        return false;
    }

    /**
     * Determine whether this wildcard allows any namespace (##any)
     *
     * @return true if this wildcard allows any namespace
     */

    public final boolean allowsAny() {
        return allowedNamespaces == null && disallowedNamespaces == null;
    }

    /**
     * Initialize the set of permitted namespaces to an empty set. This
     * is needed in case the namespace attribute is present but empty;
     * it indicates that no namespaces are allowed.
     */

    public void setNoNamespacesAllowed() {
        allowedNamespaces = new HashSet<>(3);
    }

    /**
     * Add a permitted namespace. This must be an actual namespace, not one of
     * the special tokens such as ##other; the non-namespace ("absent") is represented
     * by the zero-length string. Validation of the namespace is the
     * responsibility of the caller.
     *
     * @param namespace the namespace to add
     */
    public void addAllowedNamespace(NamespaceUri namespace) {
        if (allowedNamespaces == null) {
            allowedNamespaces = new HashSet<>(3);
        }
        allowedNamespaces.add(namespace);
    }

    /**
     * Add a disallowed namespace. This corresponds to the option ##other.
     *
     * @param namespace the disallowed namespace to be added
     */

    public void addDisallowedNamespace(NamespaceUri namespace) {
        if (disallowedNamespaces == null) {
            disallowedNamespaces = new HashSet<>(3);
        }
        disallowedNamespaces.add(namespace);
    }

    /**
     * Get the disallowed namespaces. Returns null if there is no disallowed namespace.
     *
     * @return the set of disallowed namespaces; may be null to represent the empty set
     */

    /*@Nullable*/
    public Set<NamespaceUri> getDisallowedNamespaces() {
        return disallowedNamespaces;
    }

    /**
     * Determine whether a particular namespace is allowed by this wildcard. This method ignores
     * the QNames that have been explicitly disallowed (using notQNames)
     *
     * @param uri the namespace to be tested. Supply "" for the no-namespace.
     * @return true if the wildcard allows this namespace, false if not.
     */

    public boolean matches(NamespaceUri uri) {
        if (allowedNamespaces != null) {
            return allowedNamespaces.contains(uri);
        } else {
            return disallowedNamespaces == null || !disallowedNamespaces.contains(uri);
        }
    }

    /**
     * Determine whether a name is allowed by this wildcard. This method ignores
     * the restrictions imposed by ##definedSiblings
     *
     * @param name        the name to be tested
     * @param isElement   true if looking for an element, false if looking for an attribute
     * @param schema      the schema against which we are validating
     * @param parentType  the governing type definition of the parent element, used to assess whether
     *                    the ##definedSibling constraint prevents an element from matching. May be null, in which case
     *                    ##definedSibling is ignored.
     * @return true if the wildcard allows this name, false if not.
     */

    public boolean matches(StructuredQName name, boolean isElement, Schema schema, UserComplexType parentType) {
        PreparedSchema ps = (PreparedSchema)schema;
        NodeName nn = new FingerprintedQName(name, ps.getNamePool());
        return matches(nn, isElement, ps, parentType);
    }

    public boolean matches(int fp, boolean isElement, Schema schema, UserComplexType parentType) {
        PreparedSchema ps = (PreparedSchema) schema;
        NodeName nn = new CodedName(fp, "", ps.getNamePool());
        return matches(nn, isElement, ps, parentType);
    }

    /**
     * Determine whether a name is allowed by this wildcard. This method ignores
     * the restrictions imposed by ##definedSiblings
     *
     * @param name        the name to be tested
     * @param isElement   true if looking for an element, false if looking for an attribute
     * @param schema      the schema against which we are validating
     * @param parentType  the governing type definition of the parent element, used to assess whether
     *                    the ##definedSibling constraint prevents an element from matching. May be null, in which case
     *                    ##definedSibling is ignored.
     * @return true if the wildcard allows this name, false if not.
     */

    public boolean matches(NodeName name, boolean isElement, Schema schema, /*@Nullable*/ UserComplexType parentType) {
        int fingerprint = -1;
        PreparedSchema ps = (PreparedSchema) schema;
        NamePool namePool = ps.getNamePool();
        if (disallowedQNames != null && disallowedQNames.contains(fingerprint)) {
            return false;
        }
        if (disallowedQNames != null) {
            fingerprint = name.obtainFingerprint(namePool);
            if (disallowedQNames.contains(fingerprint)) {
                return false;
            }
        }
        if (disallowsDefinedNames) {
            if (fingerprint == -1) {
                fingerprint = name.obtainFingerprint(namePool);
            }
            if ((isElement ?
                         schema.getElementDecl(fingerprint) :
                         schema.getAttributeDecl(fingerprint)) != null) {
                return false;
            }
        }
        if (parentType != null && disallowsDefinedSiblings) {
            IntHashSet siblings = new IntHashSet();
            try {
                parentType.gatherAllPermittedChildren(schema, siblings, true);
            } catch (SchemaException e) {
                throw new IllegalStateException(e);
            }
            if (siblings.contains(name.getFingerprint())) {
                return false;
            }
        }
        if (allowedNamespaces != null) {
            return allowedNamespaces.contains(name.getNamespaceUri());
        } else {
            return disallowedNamespaces == null || !disallowedNamespaces.contains(name.getNamespaceUri());
        }
    }

    /**
     * If matches() returns false, return a string explaining the reason for the non-match
     *
     * @param nn          the name of the node that does not match the wildcard
     * @param isElement   true if looking for an element, false if looking for an attribute
     * @param schema      the schema we are validating against
     * @param parentType  the governing type definition of the parent element, used to assess whether
     *                    the ##definedSibling constraint prevents an element from matching. May be null, in which case
     *                    ##definedSibling is ignored.
     * @return null if the name matches, or a string explaining the reason if not. The string will be in the form
     *         of one or more complete English sentences terminated by a full stop and trailing space.
     */

    public String reasonForNonMatch(NodeName nn, boolean isElement, PreparedSchema schema, /*@Nullable*/ UserComplexType parentType) {
        int fingerprint = nn.obtainFingerprint(schema.getNamePool());
        if (disallowedQNames != null && disallowedQNames.contains(fingerprint)) {
            return "The name is in the list of disallowed QNames for the wildcard. ";
        }
        if (disallowsDefinedNames &&
                (isElement ?
                         schema.getElementDecl(fingerprint) :
                         schema.getAttributeDecl(fingerprint)) != null) {
            return "There is a global " + (isElement ? "element" : "attribute") +
                    " declaration for the name, and the wildcard specifies notQName='##defined'. ";
        }
        if (parentType != null && disallowsDefinedSiblings) {
            IntHashSet siblings = new IntHashSet();
            try {
                parentType.gatherAllPermittedChildren(schema, siblings, true);
            } catch (SchemaException e) {
                throw new IllegalStateException(e);
            }
            if (siblings.contains(nn.getFingerprint())) {
                return "The wildcard disallows defined siblings, and the name is a permitted sibling. ";
            }
        }
        NamespaceUri uri = nn.getNamespaceUri();
        if (allowedNamespaces != null) {
            if (allowedNamespaces.contains(uri)) {
                return null;
            } else {
                return "The name is not in one of the permitted namespaces for the wildcard. ";
            }
        } else {
            if (disallowedNamespaces == null || !disallowedNamespaces.contains(uri)) {
                return null;
            } else {
                return "The name is in " +
                        (disallowedNamespaces.size() == 1 ?
                                "the disallowed namespace" :
                                "one of the disallowed namespaces") + " for the wildcard. ";
            }
        }
    }

    /**
     * Test whether a name is in the set of disallowed QNames
     *
     * @param fingerprint the fingerprint of the name to be tested
     * @return true if the name is in the set of explicitly disallowed QNames
     */

    public boolean disallowsQName(int fingerprint) {
        return disallowedQNames != null && disallowedQNames.contains(fingerprint);
    }

    /**
     * Returns the set of permitted namespaces of this wildcard
     *
     * @return the set of permitted namespaces of this wildcard, or null if no namespaces are
     *         explicitly permitted
     */
    public Set<NamespaceUri> getAllowedNamespaces() {
        return allowedNamespaces;
    }

    /**
     * Returns the processContents attribute of this wildcard
     *
     * @return the processContents attribute of this wildcard, as a string
     *         ("strict", "lax", or "skip")
     */
    public String getProcessContents() {
        return processContents;
    }

    /**
     * Sets the processContents attribute of the wildCard. For internal use only.
     *
     * @param process the processContents value to set (e.g. "strict", "lax").
     *                Validation of the value is the responsibility of the caller.
     */
    public void setProcessContents(String process) {
        processContents = process;
    }

    /**
     * Say that defined names are not allowed (notQName = "##defined")
     *
     * @param disallow true if defined names are disallowed
     */
    public void setDisallowDefinedNames(boolean disallow) {
        disallowsDefinedNames = disallow;
    }

    /**
     * Ask whether globally-defined names are disallowed
     *
     * @return true if globally-defined names are disallowed (notQName = "##defined")
     */

    public boolean isDisallowDefinedNames() {
        return disallowsDefinedNames;
    }

    /**
     * Say that defined siblings are not allowed (notQName = "##definedSibling")
     *
     * @param disallow true if defined siblings are disallowed
     */
    public void setDisallowDefinedSiblings(boolean disallow) {
        disallowsDefinedSiblings = disallow;
    }

    /**
     * Ask whether defined siblings are disallowed
     *
     * @return true if the names of defined siblings are disallowed (notQName = "##definedSibling")
     */

    public boolean isDisallowDefinedSiblings() {
        return disallowsDefinedSiblings;
    }

    /**
     * Mark a specific QName as being disallowed
     *
     * @param fingerprint the NamePool fingerprint of the disallowed name
     */

    public void addDisallowedQName(int fingerprint) {
        if (disallowedQNames == null) {
            disallowedQNames = new IntHashSet(10);
        }
        disallowedQNames.add(fingerprint);
    }

    /**
     * Ask whether a specific QName is disallowed
     *
     * @param fingerprint the NamePool fingerprint of the QName in question
     * @return true if the QName is in the list of QNames specifically disallowed by this wildcard
     */

    public boolean isDisallowedQName(int fingerprint) {
        return disallowedQNames != null && disallowedQNames.contains(fingerprint);
    }

    /**
     * Perform validation on this wildcard (this implementation of the method is a no-op)
     * @param compiler the schema compiler
     */
    @Override
    public boolean validate(SchemaCompiler compiler)  {
        //only do the validation on the namespace
        return true;
    }

    /**
     * Get a string representation of this wildcard, used for diagnostics
     */

    public String toString() {
        StringBuilder fsb = new StringBuilder(64);
        if (disallowedNamespaces != null) {
            fsb.append("not{");
            appendNamespaces(disallowedNamespaces, fsb);
            fsb.append("}");
        } else if (allowedNamespaces != null) {
            fsb.append("anyOf{");
            appendNamespaces(allowedNamespaces, fsb);
            fsb.append("}");
        } else {
            fsb.append("any");
        }
        if (disallowsDefinedSiblings && disallowedQNames != null) {
            fsb.append(" except defined siblings and other specific names");
        }
        if (disallowsDefinedSiblings) {
            fsb.append(" except defined siblings");
        }
        if (disallowedQNames != null) {
            int nr = disallowedQNames.size();
            if (nr == 1) {
                fsb.append(" except one specific name");
            } else {
                fsb.append(" except " + nr + " specific names");
            }
        }
        return fsb.toString();
    }

    private void appendNamespaces(/*@Nullable*/ Set<NamespaceUri> namespaces, StringBuilder sb) {
        if (namespaces == null) {
            return;
        }
        Iterator<NamespaceUri> iter = namespaces.iterator();
        boolean first = true;
        while (iter.hasNext()) {
            if (first) {
                first = false;
            } else {
                sb.append(' ');
            }
            NamespaceUri s = iter.next();
            sb.append(s.isEmpty() ? "##local" : s);
        }
    }

    private String listOfNamespaces(/*@Nullable*/ Set<NamespaceUri> namespaces) {
        if (namespaces == null) {
            return "";
        }
        StringBuilder sb = new StringBuilder(64);
        appendNamespaces(namespaces, sb);
        return sb.toString();
    }

    /**
     * Determine whether this wildcard is a subset of another wildcard.
     * See Schema Part 1 Schema Component Constraint: Wildcard Subset (section 3.10.6.2)
     *
     * @param sup  the other wildcard
     * @param pool the NamePool
     * @return true if this wildcard is a subset of the other wildcard, as defined in the XSD specification
     */

    public boolean isSubset(Wildcard sup, NamePool pool) {

        // Rules 1-4
        if (!isNamespaceSubset(sup)) {
            return false;
        }

        // Rule 5: each QName member of super.{disallowedNames} is not allowed by sub
        if (sup.disallowedQNames != null) {
            for (IntIterator it = sup.disallowedQNames.iterator(); it.hasNext(); ) {
                int fp = it.next();
                NamespaceUri uri = pool.getURI(fp);
                if (this.matches(uri) && !this.disallowsQName(fp)) {
                    return false;
                }
            }
        }

        // Rule 6: if super contains ##defined, then sub must contain ##defined
        //noinspection RedundantIfStatement
        if (sup.disallowsDefinedNames && !this.disallowsDefinedNames) {
            return false;
        }
        return true;
    }

    /**
     * Determine if the namespaces allowed by this wildcard are a subset of the namespaces
     * allowed by a second wildcard
     *
     * @param sup the second wildcard
     * @return true if every namespace allowed by sup is also allowed by this wildcard
     */

    private boolean isNamespaceSubset(Wildcard sup) {
        // Rule 1: if super is "any" return true
        if (sup.allowedNamespaces == null && sup.disallowedNamespaces == null) {
            return true;
        }
        // Rule 2:
        if (sup.allowedNamespaces != null && this.allowedNamespaces != null) {
            return sup.allowedNamespaces.containsAll(this.allowedNamespaces);
        }
        // Rule 3:
        if (this.allowedNamespaces != null && sup.disallowedNamespaces != null) {
            return Collections.disjoint(this.allowedNamespaces, sup.disallowedNamespaces);
        }
        // Rule 4:
        //noinspection SimplifiableIfStatement
        if (sup.disallowedNamespaces != null && this.disallowedNamespaces != null) {
            return this.disallowedNamespaces.containsAll(sup.disallowedNamespaces);
        }
        return false;
    }


    /**
     * Determine whether this wildcard overlaps another wildcard, that is, whether there is
     * an element that could match either. Note, this is no longer a defined algorithm in
     * Schema 1.1, but it is still needed to assess UPA.
     *
     * @param other the other wildcard
     * @return true if the two wildcards overlap, that is, if there are possible elements that could
     *         match either of the wildcards
     */

    public boolean overlaps(Wildcard other) {
        // Rule 1a: if this is "any" return true
        if (this.allowedNamespaces == null && this.disallowedNamespaces == null) {
            return true;
        }

        // Rule 1b: if other is "any" return true
        if (other.allowedNamespaces == null && other.disallowedNamespaces == null) {
            return true;
        }

        // Rule 2: if this is (not, X) and other is (not, Y) return true
        if (this.disallowedNamespaces != null && other.disallowedNamespaces != null) {
            return true;
        }

        // Rule 3: if this is (not, X) and other is (enum, Y) return false if X is a superset of Y
        if (this.disallowedNamespaces != null && other.allowedNamespaces != null) {
            return !this.disallowedNamespaces.containsAll(other.allowedNamespaces);
        }

        // Rule 4: inverse of rule 3
        if (other.disallowedNamespaces != null && this.allowedNamespaces != null) {
            return !other.disallowedNamespaces.containsAll(this.allowedNamespaces);
        }

        // Rule 5: return true if both are sets of allowed namespaces and they overlap
        //noinspection SimplifiableIfStatement
        if (this.allowedNamespaces != null && other.allowedNamespaces != null) {
            return !Collections.disjoint(this.allowedNamespaces, other.allowedNamespaces);
        }

        return true;
    }


    /**
     * Test whether one processContents value is &lt;= the strength of another,
     * where skip &lt; lax &lt; strict
     *
     * @param str1 the first processContents value, as a string
     * @param str2 the second processContents value, as a string
     * @return true if the pair is one of (strict, strict), (lax, lax), (skip, skip),
     *         (skip, strict), (lax, strict), (skip, lax)
     */


    public static boolean isNotStronger(String str1, String str2) {
        String ranking = "skip lax strict";
        int r1 = ranking.indexOf(str1);
        int r2 = ranking.indexOf(str2);
        return r1 <= r2;
    }

    public int compareStrength(Wildcard other) {
        String ranking = "skip lax strict";
        int r1 = ranking.indexOf(processContents);
        int r2 = ranking.indexOf(other.processContents);
        return Integer.compare(r1, r2);
    }

    /**
     * Create an AttributeWildcard as the intersection of two AttributeWildcards.
     * For internal use only.
     * <p>In XML Schema 1.1, the intersection of two wildcards is always expressible.
     * The resulting wildcard might not be expressible using XML Schema 1.0, however.</p>
     *
     * @param wat1 the first operand: the processContents is taken from this operand
     * @param wat2 the second operand
     * @param pool the NamePool
     * @return a new AttributeWildcard representing the intersection, as defined in
     *         Schema Component Constraint: Attribute Wildcard Intersection; if the
     *         intersection "is not expressible" this will be flagged as such, for later
     *         rejection by the validator.
     */

    public static Wildcard makeIntersection(Wildcard wat1, Wildcard wat2, NamePool pool) {
        if (wat1.getConfiguration() != wat2.getConfiguration()) {
            throw new IllegalArgumentException("Cannot combine wildcards from different configurations");
        }
        Set<NamespaceUri> allowed1 = wat1.getAllowedNamespaces();
        Set<NamespaceUri> allowed2 = wat2.getAllowedNamespaces();
        Set<NamespaceUri> disallowed1 = wat1.getDisallowedNamespaces();
        Set<NamespaceUri> disallowed2 = wat2.getDisallowedNamespaces();
        Wildcard result = new Wildcard(wat1.getConfiguration());
        result.setProcessContents(wat1.getProcessContents());
        result.setLocator(wat1);

        if (allowed1 != null && allowed2 != null) {
            result.allowedNamespaces = intersection(allowed1, allowed2);
        } else if (disallowed1 != null && disallowed2 != null) {
            result.disallowedNamespaces = union(disallowed1, disallowed2);
        } else if (allowed1 == null && disallowed1 == null) {
            result.allowedNamespaces = allowed2;
            result.disallowedNamespaces = disallowed2;
        } else if (allowed2 == null && disallowed2 == null) {
            result.allowedNamespaces = allowed1;
            result.disallowedNamespaces = disallowed1;
        } else if (allowed1 != null && disallowed2 != null) {
            result.allowedNamespaces = difference(allowed1, disallowed2);
        } else if (allowed2 != null && disallowed1 != null) {
            result.allowedNamespaces = difference(allowed2, disallowed1);
        } else {
            throw new IllegalStateException("Invalid wildcard");
        }
//        if (result.allowedNamespaces != null && result.allowedNamespaces.isEmpty()) {
//            result.allowedNamespaces = null;
//        }
        if (result.disallowedNamespaces != null && result.disallowedNamespaces.isEmpty()) {
            result.disallowedNamespaces = null;
        }
        if (wat1.disallowedQNames != null) {
            for (IntIterator it = wat1.disallowedQNames.iterator(); it.hasNext(); ) {
                int fp = it.next();
                NamespaceUri uri = pool.getURI(fp);
                if (wat2.matches(uri)) {
                    result.addDisallowedQName(fp);
                }
            }
        }
        if (wat2.disallowedQNames != null) {
            for (IntIterator it = wat2.disallowedQNames.iterator(); it.hasNext(); ) {
                int fp = it.next();
                NamespaceUri uri = pool.getURI(fp);
                if (wat1.matches(uri)) {
                    result.addDisallowedQName(fp);
                }
            }
        }
        if (wat1.disallowsDefinedNames || wat2.disallowsDefinedNames) {
            result.disallowsDefinedNames = true;
        }
        return result;
    }

    /**
     * Create a AttributeWildcard as the union of two AttributeWildcards.
     * For internal use only.
     *
     * @param wat1 the first operand: the processContents is taken from this operand
     * @param wat2 the second operand
     * @param pool the Name Pool
     * @return a new AttributeWildcard representing the union, as defined in
     *         Schema Component Constraint: Attribute Wildcard Union; if the
     *         union "is not expressible" then the result will be marked as such, for later
     *         use by the validator.
     */

    public static Wildcard makeUnion(Wildcard wat1, Wildcard wat2, NamePool pool) {
        if (wat1.getConfiguration() != wat2.getConfiguration()) {
            throw new IllegalArgumentException("Cannot combine wildcards from different configurations");
        }
        Set<NamespaceUri> allowed1 = wat1.getAllowedNamespaces();
        Set<NamespaceUri> allowed2 = wat2.getAllowedNamespaces();
        Set<NamespaceUri> disallowed1 = wat1.getDisallowedNamespaces();
        Set<NamespaceUri> disallowed2 = wat2.getDisallowedNamespaces();
        Wildcard result = new Wildcard(wat1.getConfiguration());
        result.setConfiguration(wat1.getConfiguration());
        result.setProcessContents(wat1.getProcessContents());
        result.setLocator(wat1);

        if (allowed1 != null && allowed2 != null) {
            result.allowedNamespaces = union(allowed1, allowed2);
        } else if (disallowed1 != null && disallowed2 != null) {
            result.disallowedNamespaces = intersection(disallowed1, disallowed2);
        } else if (allowed1 == null && disallowed1 == null) {
            result.allowedNamespaces = null;
            result.disallowedNamespaces = null;
        } else if (allowed2 == null && disallowed2 == null) {
            result.allowedNamespaces = null;
            result.disallowedNamespaces = null;
        } else if (allowed1 != null && disallowed2 != null) {
            result.disallowedNamespaces = difference(disallowed2, allowed1);
        } else if (allowed2 != null && disallowed1 != null) {
            result.disallowedNamespaces = difference(disallowed1, allowed2);
        } else {
            throw new IllegalStateException("Invalid wildcard");
        }
        if (result.allowedNamespaces != null && result.allowedNamespaces.isEmpty()) {
            result.allowedNamespaces = null;
        }
        if (result.disallowedNamespaces != null && result.disallowedNamespaces.isEmpty()) {
            result.disallowedNamespaces = null;
        }
        if (wat1.disallowedQNames != null) {
            for (IntIterator it = wat1.disallowedQNames.iterator(); it.hasNext(); ) {
                int fp = it.next();
                NamespaceUri uri = pool.getURI(fp);
                if (!wat2.matches(uri) || wat2.disallowsQName(fp)) {
                    result.addDisallowedQName(fp);
                }
            }
        }
        if (wat2.disallowedQNames != null) {
            for (IntIterator it = wat2.disallowedQNames.iterator(); it.hasNext(); ) {
                int fp = it.next();
                NamespaceUri uri = pool.getURI(fp);
                if (!wat1.matches(uri) || wat1.disallowsQName(fp)) {
                    result.addDisallowedQName(fp);
                }
            }
        }
        // if both have notQName="##defined", so does the result
        if (wat1.disallowsDefinedNames && wat2.disallowsDefinedNames) {
            result.disallowsDefinedNames = true;
        }

        return result;
    }

    /**
     * Test whether one wildcard allows namespaces that are not allowed by the other wildcard
     *
     * @param w1 the first wildcard
     * @param w2 the second wildcard
     * @return true if there exists a namespace that is allowed by w1 and not by w2
     */

//    private static boolean allowsMoreNamespacesThan(Wildcard w1, Wildcard w2) {
//        return w1.allowsAny() && !w2.allowsAny() ||
//                w1.allowedNamespaces != null && w2.allowedNamespaces != null &&
//                        !difference(w1.allowedNamespaces, w2.allowedNamespaces).isEmpty() ||
//                w1.allowedNamespaces != null && w2.disallowedNamespaces != null &&
//                        !difference(w2.disallowedNamespaces, w1.allowedNamespaces).isEmpty() ||
//                w1.disallowedNamespaces != null && w2.allowedNamespaces != null ||
//                w1.disallowedNamespaces != null && w2.disallowedNamespaces != null &&
//                        !difference(w2.disallowedNamespaces, w1.disallowedNamespaces).isEmpty();
//    }

    /**
     * Form the union of two sets
     *
     * @param a the first set
     * @param b the second set
     * @return the union of the two sets
     */

    private static Set<NamespaceUri> union(Set<NamespaceUri> a, Set<NamespaceUri> b) {
        Set<NamespaceUri> result = new HashSet<>(a.size() + b.size());
        result.addAll(a);
        result.addAll(b);
        return result;
    }

    /**
     * Form the intersection of two sets
     *
     * @param a the first set
     * @param b the second set
     * @return the intersection of the two sets
     */

    private static Set<NamespaceUri> intersection(Set<NamespaceUri> a, Set<NamespaceUri> b) {
        Set<NamespaceUri> result = new HashSet<>();
        for (NamespaceUri s : a) {
            if (b.contains(s)) {
                result.add(s);
            }
        }
        return result;
    }

    /**
     * Form the difference of two sets
     *
     * @param a the first set
     * @param b the second set
     * @return the difference of the two sets (first minus second)
     */

    private static Set<NamespaceUri> difference(Set<NamespaceUri> a, Set<NamespaceUri> b) {
        Set<NamespaceUri> result = new HashSet<>();
        for (NamespaceUri s : a) {
            if (!b.contains(s)) {
                result.add(s);
            }
        }
        return result;
    }

    /**
     * Serialize this schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("wildcard");
        serializer.emitAttribute("id", serializer.getId(this, true));
        serializer.emitAttribute("processContents", getProcessContents());
        if (allowedNamespaces == null) {
            if (disallowedNamespaces == null) {
                serializer.emitAttribute("constraint", "any");
            } else {
                serializer.emitAttribute("constraint", "not");
                serializer.emitAttribute("namespaces", listOfNamespaces(disallowedNamespaces));
            }
        } else {
            serializer.emitAttribute("constraint", "enumeration");
            serializer.emitAttribute("namespaces", listOfNamespaces(allowedNamespaces));
        }
        StringBuilder disallowedNames = new StringBuilder(100);
        if (disallowsDefinedNames) {
            disallowedNames.append("##defined ");
        }
        if (disallowsDefinedSiblings) {
            disallowedNames.append("##definedSibling ");
        }
        if (disallowedQNames != null && !disallowedQNames.isEmpty()) {
            NamePool pool = getConfiguration().getNamePool();
            for (IntIterator intit = disallowedQNames.iterator(); intit.hasNext(); ) {
                int nc = intit.next();
                String name = pool.getClarkName(nc);
                disallowedNames.append(name);
                disallowedNames.append(' ');
            }
        }
        if (disallowedNames.length() > 0) {
            disallowedNames.setLength(disallowedNames.length() - 1);
            serializer.emitAttribute("notQName", disallowedNames.toString());
        }
        serializer.endElement();
    }

    @Override
    public FunctionItem getComponentAsFunction() {
        Wildcard thisWildcard = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Wildcard");
                case "implementation":
                    return new ObjectValue<>(thisWildcard);
                case "namespace constraint":
                    CallableDelegate constraintFunction = new CallableDelegate((context1, arguments1) -> {
                        String key1 = arguments[0].head().getStringValue();
                        switch (key1) {
                            case "class":
                                return StringValue.bmp("Namespace Constraint");
                            case "variety":
                                if (allowsAny()) {
                                    return StringValue.bmp("any");
                                } else if (allowedNamespaces != null) {
                                    return StringValue.bmp("enumeration");
                                } else {
                                    return StringValue.bmp("not");
                                }
                            case "namespaces":
                                if (allowsAny()) {
                                    return EmptySequence.getInstance();
                                } else {
                                    Set<NamespaceUri> namespaces = allowedNamespaces != null ? allowedNamespaces : disallowedNamespaces;
                                    List<AtomicValue> uriList = new ArrayList<>(namespaces.size());
                                    for (NamespaceUri ns : namespaces) {
                                        uriList.add(new AnyURIValue(ns.toUnicodeString()));
                                    }
                                    return new AtomicArray(uriList);
                                }
                            case "disallowed names":
                                List<AtomicValue> disallowedList = new ArrayList<>();
                                if (disallowsDefinedNames) {
                                    disallowedList.add(StringValue.bmp("defined"));
                                }
                                if (disallowsDefinedSiblings) {
                                    disallowedList.add(StringValue.bmp("definedSiblings"));
                                }
                                NamePool pool = getConfiguration().getNamePool();
                                IntIterator iter = disallowedQNames.iterator();
                                while (iter.hasNext()) {
                                    int fp = iter.next();
                                    NamespaceUri uri = pool.getURI(fp);
                                    String local = pool.getLocalName(fp);
                                    QNameValue value = new QNameValue("", uri, local);
                                    disallowedList.add(value);
                                }
                                return new AtomicArray(disallowedList);
                            default:
                                return EmptySequence.getInstance();
                        }

                    });
                    return new CallableFunction(1, constraintFunction, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                case "process contents":
                    return new StringValue(getProcessContents());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }
}

// Copyright (c) 2004-2023 Saxonica Limited