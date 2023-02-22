////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package commands;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Query;
import net.sf.saxon.lib.Initializer;
import net.sf.saxon.lib.ModuleURIResolver;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on Query Command Line options
 */
public class QueryTests {

    @Test
    public void testQueryDirect() {
        String[] args = {
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

    @Test
    public void testQueryDirectWithSource() {
        String[] args = {
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

    @Test
    public void testQueryProjection() {
        String[] args = {
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

    @Test
    public void testQueryUpdate() {
        // This is an update test; copy the file into temp
        String tempfn = ConfigTest.DATA_DIR + "temp/books.xml";
        String origfn = ConfigTest.DATA_DIR + "books.xml";
        InputStream instream = null;
        OutputStream outstream = null;

        try {
            instream = new FileInputStream(new File(origfn));
            outstream = new FileOutputStream(new File(tempfn));
            byte[] buf = new byte[4096];
            int length = -1;
            while ((length = instream.read(buf)) > 0) {
                outstream.write(buf, 0, length);
            }
            instream.close();
            outstream.close();

            String[] args = {
                    "-qs:replace value of node (//PRICE)[1] with 3.65",
                    "-s:" + tempfn,
                    "-t",
                    "-update:on"};

            Query.main(args);
            // FIXME: If this is an update test, shouldn't we check if it updated?
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testQueryParams() {
        String[] args = {
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

    @Test
    public void testQueryInitializer() {
        String[] args = {
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

