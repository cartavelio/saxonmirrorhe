package net.sf.saxon.pattern;

import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * A pattern of the form map(key: pattern, ... [, *]), introduced in 4.0
 */

public class MapPattern extends Pattern {

    public record MapPatternEntry (AtomicValue key, SequenceMatcher sequenceMatcher){};

    private final List<MapPatternEntry> entries;
    private final boolean extensible;
    private final Set<AtomicMatchKey> allowedKeys;


    public MapPattern(List<MapPatternEntry> entries, boolean extensible) {
        this.entries = entries;
        this.extensible = extensible;
        allowedKeys = extensible ? null : new HashSet<AtomicMatchKey>();
        if (!extensible) {
            for (MapPatternEntry entry : entries) {
                allowedKeys.add(entry.key.asMapKey(40));
            }
        }
    }

    @Override
    public Iterable<Operand> operands() {
        List<Operand> result = new ArrayList<>();
        for (MapPatternEntry entry : entries) {
            result.add(new Operand(this, entry.sequenceMatcher.getItemPattern(), OperandRole.SAME_FOCUS_ACTION));
        }
        return result;
    }

    @Override
    public boolean matches(Item item, XPathContext context) throws XPathException {
        if (!(item instanceof MapItem mapItem)) {
            return false;
        }
        for (MapPatternEntry entry : entries) {
            GroundedValue value = mapItem.get(entry.key);
            if (value == null) {
                return false;
            }
            if (!entry.sequenceMatcher.matches(value, context)) {
                return false;
            }
        }
        if (!extensible) {
            for (KeyValuePair kvp : mapItem.keyValuePairs()) {
                if (!allowedKeys.contains(kvp.key().asMapKey(40))) {
                    return false;
                }
            }
        }
        return true;
    }

    @Override
    public UType getUType() {
        return UType.FUNCTION;
    }

    @Override
    public ItemType getItemType() {
        return MapType.ANY_MAP_TYPE; // TODO: can do better
    }

    @Override
    public double getDefaultPriority() {
        // If the pattern contains at least one predicate, then +0.5.
        //  For the pattern {} (which matches an empty map), then +0.5.
        //  If the pattern contains at least one SequencePattern, then +0.25.
        //  If the pattern contains at least one MapEntryPattern, then 0.
        //  Otherwise (for the pattern {*}), -0.25.
        if (entries.isEmpty()) {
            return extensible ? -0.25 : 0.5;
        }
        for (MapPatternEntry entry : entries) {
            if (entry.sequenceMatcher != SequenceMatcher.UNIVERSAL_MATCHER) {
                return 0.25;
            }
        }
        return 0;
    }

    @Override
    public void export(ExpressionPresenter presenter) throws XPathException {
        throw new UnsupportedOperationException("Not yet implemented");
    }

    @Override
    public Pattern copy(RebindingMap rebindings) {
        List<MapPatternEntry> entries2 = new ArrayList<>(entries.size());
        for (MapPatternEntry entry : entries) {
            entries2.add(new MapPatternEntry(entry.key, entry.sequenceMatcher.copy(rebindings)));
        }
        return new MapPattern(entries2, extensible);
    }
}

