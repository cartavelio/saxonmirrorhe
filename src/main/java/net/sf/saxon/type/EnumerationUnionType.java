package net.sf.saxon.type;

import java.util.List;

public class EnumerationUnionType extends LocalUnionType<SingletonEnumType> {

    public EnumerationUnionType(List<SingletonEnumType> singletonEnums) {
        super(singletonEnums);
    }

    @Override
    public String getDescription() {
        StringBuilder sb = new StringBuilder("enum(");
        for (SingletonEnumType et : getMemberTypes()) {
            sb.append('"').append(et.getValue()).append('"').append(',');
        }
        sb.setCharAt(sb.length()-1, ')');
        return sb.toString();
    }
}

