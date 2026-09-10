package net.sf.saxon.type;

import java.util.List;

public class EnumerationUnionType extends LocalUnionType {

    public EnumerationUnionType(List<AtomicType> singletonEnums) {
        super(singletonEnums);
    }

    @Override
    public String getDescription() {
        StringBuilder sb = new StringBuilder("enum(");
        for (AtomicType at : getMemberTypes()) {
            SingletonEnumType et = (SingletonEnumType)at;
            sb.append('"').append(et.getValue()).append('"').append(',');
        }
        sb.setCharAt(sb.length()-1, ')');
        return sb.toString();
    }
}

