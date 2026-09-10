////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package commands;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.Query;
import net.sf.saxon.lib.Initializer;
import net.sf.saxon.lib.ModuleURIResolver;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

/**
 * JUnit tests on Query Command Line options
 */
public class QueryTests extends TestCase {

    public void testQueryDirect() {
        String args[] = {
                "-qs:2+2",
                "-t",
                "!omit-xml-declaration=yes"};
        try {
            Query.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testQueryDirectWithSource() {
        String args[] = {
                "-qs:count(//*)",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-t",
                "!omit-xml-declaration=yes"};
        try {
            Query.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testQueryProjection() {
        String args[] = {
                "-qs:count(//CATEGORY)",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-t",
                "-projection:on"};
        try {
            Query.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testQueryUpdate() {
        String args[] = {
                "-qs:replace value of node (//PRICE)[1] with 3.65",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-t",
                "-update:on"};
        try {
            Query.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testQueryParams() {
        String args[] = {
                "-qs:declare variable $x external; declare variable $y external; $x+$y",
                "!omit-xml-declaration=yes",
                "x=3",
                "y=4"};
        try {
            Query.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testQueryInitializer() {
        String args[] = {
                "-qs:import module namespace h = \"http://example.org/ns/hello\"; h:new-hello('world')",
                "-init:commands.QueryTests$MyInitializer"};
        try {
            Query.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    public static class MyInitializer
            implements Initializer {

        public void initialize(Configuration config) {
            System.err.println("Initialize...");
            config.setModuleURIResolver(new Resolver());
        }

        private static final class Resolver
                implements ModuleURIResolver {

            public StreamSource[] resolve(String ns, String base, String[] locs) {
                System.err.println("Resolve: " + ns + ", " + base);
                String mod = "module namespace h = \"http://example.org/ns/hello\"; " +
                        "declare function h:new-hello($x){'Hello ' || $x};";
                StringReader sr = new StringReader(mod);
                StreamSource ss = new StreamSource(sr,"http://example.org/ns/hello");
                return new StreamSource[]{ss};
            }
        }
    }


}

