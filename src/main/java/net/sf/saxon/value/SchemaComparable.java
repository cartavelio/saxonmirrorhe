package net.sf.saxon.value;

/**
 * A Comparable value that implements the XML Schema ordering comparison semantics for this value.
 *
 * <p>In the case of data types that are partially ordered, the returned Comparable extends the standard
 * semantics of the compareTo() method by returning the value {@link net.sf.saxon.om.SequenceTool#INDETERMINATE_ORDERING} when there
 * is no defined order relationship between two given values.</p>
 *
 * <p>As well as implementing {@link Comparable#compareTo}, a <code>SchemaComparable</code>
 * must also provide compatible implementations of {@code equals()} and {@code hashCode()},
 * to allow XSD uniqueness constraints to be implemented by maintaining a hash table of
 * <code>SchemaComparable</code> objects</p>
 */

public interface SchemaComparable extends Comparable<SchemaComparable> {
}

