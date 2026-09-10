package net.sf.saxon.type;

/**
 * Interface representing either an XPath-defined choice type or an XSD-defined (pure) union type. The two cases
 * are slightly different. The instances of an XSD-defined union are always atomic items, and they always support
 * (for example) casting from string. But the coercion rules for both cases are very similar.
 */

public interface ChoiceType {

    /**
     * Get the alternative types available within this choice types
     * @return the alternative item types
     */

    Iterable<? extends ItemType> getAlternatives();
}

