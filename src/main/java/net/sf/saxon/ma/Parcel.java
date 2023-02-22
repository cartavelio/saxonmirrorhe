package net.sf.saxon.ma;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.JavaExternalObjectType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.ObjectValue;

public class Parcel extends ObjectValue<GroundedValue> {

    public static JavaExternalObjectType TYPE = new JavaExternalObjectType(Parcel.class);

    public Parcel(GroundedValue content) {
        super(content);
    }

    /**
     * Determine the data type of the items in the expression, if possible
     *
     * @param th The TypeHierarchy.
     * @return for the default implementation: AnyItemType (not known)
     */
    @Override
    public ItemType getItemType(/*@Nullable*/ TypeHierarchy th) {
        return TYPE;
    }
}

