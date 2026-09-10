package net.sf.saxon.ma.pin;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.Shape;
import net.sf.saxon.ma.map.ShapedMap;
import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;

public class LabelTool {

    public final static Twine8 PARENT_KEY = new Twine8("parent");
    public final static Twine8 KEY_KEY = new Twine8("key");
    public final static Twine8 POSITION_KEY = new Twine8("position");
    public final static Twine8 ANCESTORS_KEY = new Twine8("ancestors");
    public final static Twine8 PATH_KEY = new Twine8("path");
    public final static Twine8 PINNED_KEY = new Twine8("pinned");


    public final static Shape NAVIGATION_LABEL_SHAPE = new Shape(
            PARENT_KEY,
            KEY_KEY,
            POSITION_KEY,
            ANCESTORS_KEY,
            PATH_KEY);
    public final static Shape PIN_LABEL_SHAPE = new Shape(PINNED_KEY);
    public final static ShapedMap PIN_LABEL = new ShapedMap(PIN_LABEL_SHAPE, BooleanValue.TRUE);

    private LabelTool() {
    }

    public static Item addLabel(Item item, StringMap label) {
        switch (item.getGenre()) {
            case MAP:
                if (item instanceof LabeledMapItem) {
                    item = ((LabeledMapItem)item).getSubject();
                }
                return new LabeledMapItem((MapItem)item, label);
            case NODE:
                return new LabeledNode((NodeInfo)item, label);
            case FUNCTION:
            case EXTERNAL:
            case ANY:
            default:
                throw new UncheckedXPathException("Not-yet-implemented labeling operation", "ZZZZ9999");

            case ARRAY:
                if (item instanceof LabeledArrayItem) {
                    item = ((LabeledArrayItem) item).getSubject();
                }
                return new LabeledArrayItem((ArrayItem) item, label);
            case ATOMIC:
                return ((AtomicValue)item).withMetadata(
                        new TypeAndLabel(((AtomicValue)item).getItemType(), label));
        }
    }

    public static Item removeLabel(Item item) {
        return addLabel(item, null);
    }
}
