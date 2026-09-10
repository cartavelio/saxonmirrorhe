package net.sf.saxon.expr.sort;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.ObjectValue;

/**
 * A record containing some item to be sorted, and a set of composite grouping keys.
 * @param thing This may be a real {@link Item}, or some other object wrapped as an item, typically as an
 *              {@link ObjectValue}
 * @param sortkeys The sort keys. There may be multiple keys, and each key may be composite. The comparator
 *                 for comparing sort keys is external to the set of actual values.
 */

public record ThingToBeSorted(Item thing, GroundedValue[] sortkeys) {}
