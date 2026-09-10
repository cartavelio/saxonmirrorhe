////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
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
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XQueryExecutable;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on Query Command Line options
 */
public class QueryTests {
    private final ConfigTest configTest = new ConfigTest();

    /**
     * Run a query using the command line interface capturing the result as a string
     * @param args the command line arguments
     * @return the contents of System.out
     */

    private static String runQuery(String[] args) {
        try {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            System.setOut(new PrintStream(baos));
            Query.main(args);
            return baos.toString("utf-8");
        } catch (Exception e) {
            fail(e);
            return "";
        }
    }

    @Test
    public void testQueryDirect() {
        String[] args = {
                "-qs:2+2",
                "-t",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("4", result);
    }

    @Test
    public void testQueryDirectWithSource() {
        String[] args = {
                "-qs:count(//*)",
                "-s:" + configTest.getDataDir() + "books.xml",
                "-t",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("78", result);
    }

    @Test
    public void testQueryLineNumbering() {
        String[] args = {
                "-qs:saxon:line-number((//AUTHOR)[1])",
                "-s:" + configTest.getDataDir() + "books.xml",
                "-l",
                "-t",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("6", result);
    }

    @Test
    public void testQueryLineNumberingOff() {
        String[] args = {
                "-qs:saxon:line-number((//AUTHOR)[1])",
                "-s:" + configTest.getDataDir() + "books.xml",
                "-l:off",
                "-t",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("-1", result);
    }

    @Test
    public void testQueryLineNumberingOnDoc() {
        String[] args = {
                "-qs:saxon:line-number((doc('" + configTest.getDataDir() + "books.xml')//AUTHOR)[1])",
                "-l:on",
                "-t",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("6", result);
    }

    @Test
    public void testQueryLineNumberingOnParam() {
        String[] args = {
                "-qs:declare variable $doc external; saxon:line-number(($doc//AUTHOR)[1])",
                "-l:on",
                "-t",
                "+doc=" + configTest.getDataDir() + "books.xml",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("6", result);
    }

    @Test
    public void testQueryDirectWithJsonSource() {
        String[] args = {
                "-qs:?name",
                "-json:" + configTest.getDataDir() + "sample.json",
                "-t",
                "!omit-xml-declaration=yes"};
        String result = runQuery(args);
        assertEquals("A green door", result);
    }



    @Test
    public void testQueryUpdate() {
        // This is an update test; copy the file into temp
        String tempfn = configTest.getTempDir() + "books.xml";
        String origfn = configTest.getDataDir() + "books.xml";
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
            // TODO: If this is an update test, shouldn't we check if it updated?
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

    @Test
    public void testQueryInitializerUsingSystemProperty() {
        try {
            System.setProperty("SAXON_INITIALIZER", "commands.QueryTests$MyInitializer");
            Processor proc = new Processor(true);
            XQueryExecutable exec = proc.newXQueryCompiler().compile(
                    "import module namespace h = \"http://example.org/ns/hello\"; h:new-hello('world')");
            exec.load().run(proc.newSerializer(System.out));
            System.clearProperty("SAXON_INITIALIZER");
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    public static class MyInitializer
            implements Initializer {

        @Override
        public void initialize(Configuration config) {
            System.err.println("Initialize...");
            config.setModuleURIResolver(new Resolver());
        }

        private static final class Resolver
                implements ModuleURIResolver {

            @Override
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

