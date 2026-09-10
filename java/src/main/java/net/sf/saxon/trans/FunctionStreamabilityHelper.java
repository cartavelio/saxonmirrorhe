package net.sf.saxon.trans;

public class FunctionStreamabilityHelper {

    public static boolean isConsuming(FunctionStreamability value) {
        return value == FunctionStreamability.ABSORBING
                || value == FunctionStreamability.SHALLOW_DESCENT
                || value == FunctionStreamability.DEEP_DESCENT;
    }

    public static boolean isStreaming(FunctionStreamability value) {
        return value != FunctionStreamability.UNCLASSIFIED;
    }

    public static FunctionStreamability of(String v) {
        return switch (v) {
            case "unclassified" -> FunctionStreamability.UNCLASSIFIED;
            case "absorbing" -> FunctionStreamability.ABSORBING;
            case "inspection" -> FunctionStreamability.INSPECTION;
            case "filter" -> FunctionStreamability.FILTER;
            case "shallow-descent" -> FunctionStreamability.SHALLOW_DESCENT;
            case "deep-descent" -> FunctionStreamability.DEEP_DESCENT;
            case "ascent" -> FunctionStreamability.ASCENT;
            default -> throw new IllegalArgumentException();
        };
    }
}

//
// The contents of this file are subject to the Mozilla Public License Version 1.0 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Saxonica Limited
//
// Portions created by (your name) are Copyright (C) (your legal entity). All Rights Reserved.
//
// Contributor(s): none.
//
