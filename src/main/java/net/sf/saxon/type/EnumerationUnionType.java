package net.sf.saxon.type;

import net.sf.saxon.str.StringTool;

import java.util.ArrayList;
import java.util.List;

public class EnumerationUnionType extends LocalUnionType {

    public EnumerationUnionType(List<SingletonEnumType> singletonEnums) {
        super(new ArrayList<ItemType>(singletonEnums));
    }

    /**
     * Create an enumeration type permitting a defined set of values
     *
     * @param values the values to be permitted. Must be Latin1 strings.
     */
    public static EnumerationUnionType of(String... values) {
        List<SingletonEnumType> types = new ArrayList<>();
        for (String s : values) {
            types.add(new SingletonEnumType(StringTool.fromLatin1(s)));
        }
        return new EnumerationUnionType(types);
    }


    @Override
    public String getDescription() {
        StringBuilder sb = new StringBuilder("enum(");
        for (ItemType et : getMemberTypes()) {
            sb.append('"').append(((SingletonEnumType)et).getValue()).append('"').append(',');
        }
        sb.setCharAt(sb.length()-1, ')');
        return sb.toString();
    }
}

