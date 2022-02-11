////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test bytecode generation for conversion of primitive types. See bug 4014. The
 * problem is that the casting tests in QT3 either execute prematurely at compile time,
 * or drop type information, so the bytecode path doesn't get tested
 */

@Tag("bytecode")
public class CompiledConversionTest {

    private void test(XdmAtomicValue from, String query, XdmAtomicValue expected) {
        try {
            String fromType = from.getPrimitiveTypeName().getLocalName();

            // First test compileToItem() - typical query is "xs:boolean(.)"
            String query2 = "declare context item as xs:" + fromType + " external; " + query;
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.THRESHOLD_FOR_HOTSPOT_BYTE_CODE, 1);
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryEvaluator eval = comp.compile(query2).load();
            eval.setContextItem(from);
            XdmValue result = eval.evaluateSingle();
            assertEquals(expected, result);

            // Now test compileToPrimitive() - typical query is "xs:boolean(.) eq xs:boolean(.)"
            eval = comp.compile(query2 + " eq " + query).load();
            eval.setContextItem(from);
            result = eval.evaluateSingle();
            assertEquals(new XdmAtomicValue(true), result);
        } catch (SaxonApiException err) {
            fail();
        }
    }

    @Test
    public void testFromBoolean() {
        test(new XdmAtomicValue(true), "xs:string(.)", new XdmAtomicValue("true"));
        test(new XdmAtomicValue(true), "xs:int(.)", new XdmAtomicValue(1));
        test(new XdmAtomicValue(true), "xs:long(.)", new XdmAtomicValue(1));
        test(new XdmAtomicValue(true), "xs:double(.)", new XdmAtomicValue(1.0e0));
        test(new XdmAtomicValue(true), "xs:float(.)", new XdmAtomicValue(1.0e0));
        test(new XdmAtomicValue(true), "xs:untypedAtomic(.)", new XdmAtomicValue("true"));
    }

    @Test
    public void testFromString() {
        test(new XdmAtomicValue("1"), "xs:boolean(.)", new XdmAtomicValue(true));
        test(new XdmAtomicValue("3"), "xs:int(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue("3"), "xs:long(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue("3"), "xs:double(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue("3"), "xs:float(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue("3"), "xs:untypedAtomic(.)", new XdmAtomicValue("3"));
    }

    @Test
    public void testFromInt() {
        test(new XdmAtomicValue(1), "xs:boolean(.)", new XdmAtomicValue(true));
        test(new XdmAtomicValue(3), "xs:int(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue(3), "xs:long(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue(3), "xs:double(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue(3), "xs:float(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue(3), "xs:untypedAtomic(.)", new XdmAtomicValue("3"));
        test(new XdmAtomicValue(3), "xs:string(.)", new XdmAtomicValue("3"));
    }

    @Test
    public void testFromDouble() {
        test(new XdmAtomicValue(1.0e0), "xs:boolean(.)", new XdmAtomicValue(true));
        test(new XdmAtomicValue(3.0e0), "xs:int(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue(3.0e0), "xs:long(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue(3.0e0), "xs:double(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue(3.0e0), "xs:float(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue(3.0e0), "xs:untypedAtomic(.)", new XdmAtomicValue("3"));
        test(new XdmAtomicValue(3.0e0), "xs:string(.)", new XdmAtomicValue("3"));
    }

    @Test
    public void testFromFloat() {
        test(new XdmAtomicValue((float)1.0e0), "xs:boolean(.)", new XdmAtomicValue(true));
        test(new XdmAtomicValue((float)3.0e0), "xs:int(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue((float)3.0e0), "xs:long(.)", new XdmAtomicValue(3));
        test(new XdmAtomicValue((float)3.0e0), "xs:double(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue((float)3.0e0), "xs:float(.)", new XdmAtomicValue(3.0e0));
        test(new XdmAtomicValue((float)3.0e0), "xs:untypedAtomic(.)", new XdmAtomicValue("3"));
        test(new XdmAtomicValue((float)3.0e0), "xs:string(.)", new XdmAtomicValue("3"));
    }

}

