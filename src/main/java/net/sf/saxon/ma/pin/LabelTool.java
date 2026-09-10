package net.sf.saxon.ma.pin;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.AtomicValue;

public class LabelTool {

    private LabelTool() {
    }

    public static Item addLabel(Item item, MapItem label) {
        switch (item.getGenre()) {
            case MAP:
                if (item instanceof LabeledMapItem) {
                    item = ((LabeledMapItem)item).getSubject();
                }
                return new LabeledMapItem((MapItem)item, label);
            case ARRAY:
                if (item instanceof LabeledArrayItem) {
                    item = ((LabeledArrayItem) item).getSubject();
                }
                return new LabeledArrayItem((ArrayItem) item, label);

            case ATOMIC:
//                if (item.getLabel() != null) {
//                    item = item.withoutLabel();
//                }
                return ((AtomicValue)item).withMetadata(
                        new TypeAndLabel(((AtomicValue)item).getItemType(), label));
            case FUNCTION:
            case NODE:
            case EXTERNAL:
            case ANY:
            default:
                throw new UnsupportedOperationException();
        }
    }

    public static Item removeLabel(Item item) {
        return addLabel(item, null);
    }
}
