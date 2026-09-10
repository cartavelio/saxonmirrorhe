////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Gizmo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.linked.DocumentImpl;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;

import static net.sf.saxon.event.Builder.LINKED_TREE;

/**
 * Test aspects of content completion in Gizmo
 */
public class GizmoCommandTests {
    private final ConfigTest configTest = new ConfigTest();
    
    /**
     * Execute a Gizmo command and test the result
     * @param command the command to be executed. On entry the current document will be the "books.xml" sample document
     * @param assertion XPath assertion to be tested. The context item is the Gizmo current document after
     *                  executing the command; the value of variable $output is set to the console output produced
     *                  by the command. The test fails unless the effective boolean value of the assertion is true.
     */

    private void runTest(String command, String assertion) {
        try {
            Gizmo gizmo = new Gizmo();
            Configuration config = gizmo.getConfiguration();
            Processor proc = new Processor(config);
            config.setTreeModel(LINKED_TREE);
            TreeInfo doc;
            try {
                doc = config.buildDocumentTree(new StreamSource(configTest.getDataDir() + "books.xml"));
            } catch (Exception ex) {
                System.err.println("CAUGHT ONE");
                ex.printStackTrace();
                throw ex;
            }
            gizmo.setCurrentDoc((DocumentImpl) doc.getRootNode());
            String result = gizmo.executeTestCommand(command);
            XPathCompiler xpc = proc.newXPathCompiler();
            xpc.declareVariable(new QName("output"));
            XPathSelector xp = xpc.compile(assertion).load();
            xp.setContextItem(new XdmNode(gizmo.getCurrentDoc()));
            xp.setVariable(new QName("output"), new XdmAtomicValue(result));
            boolean ok = xp.effectiveBooleanValue();
            if (!ok) {
                if (assertion.contains("$output")) {
                    System.err.println(result);
                } else {
                    System.err.println(QueryResult.serialize(gizmo.getCurrentDoc()));
                }
            }
            Assertions.assertTrue(ok);
        } catch (NoClassDefFoundError ex) {
            ex.printStackTrace();
            throw ex;
        } catch (XPathException | SaxonApiException e) {
            Assertions.fail(e.getMessage());
            e.printStackTrace();
        }
    }

    private void runMultiTest(String[] commands, String assertion) {
        try {
            Gizmo gizmo = new Gizmo();
            Configuration config = gizmo.getConfiguration();
            Processor proc = new Processor(config);
            config.setTreeModel(LINKED_TREE);
            TreeInfo doc = config.buildDocumentTree(new StreamSource(configTest.getDataDir() + "books.xml"));
            gizmo.setCurrentDoc((DocumentImpl) doc.getRootNode());
            StringBuilder resultString = new StringBuilder();
            for (String command : commands) {
                String result = gizmo.executeTestCommand(command);
                resultString.append(result);
            }
            XPathCompiler xpc = proc.newXPathCompiler();
            xpc.declareVariable(new QName("output"));
            xpc.setSchemaAware(true);
            XPathSelector xp = xpc.compile(assertion).load();
            xp.setContextItem(new XdmNode(gizmo.getCurrentDoc()));
            xp.setVariable(new QName("output"), new XdmAtomicValue(resultString.toString()));
            boolean ok = xp.effectiveBooleanValue();
            if (!ok) {
                if (assertion.contains("$output")) {
                    System.err.println(resultString);
                } else {
                    System.err.println(QueryResult.serialize(gizmo.getCurrentDoc()));
                }
            }
            Assertions.assertTrue(ok);
        } catch (XPathException | SaxonApiException e) {
            Assertions.fail(e.getMessage());
            e.printStackTrace();
        }
    }

    @Test
    public void copy001() {
        runTest("copy //PRICE", "count(/PRICE) eq 6");
    }

    @Test
    public void copy002() {
        runTest("copy //PRICELESS", "empty(/node())");
    }

    @Test
    public void delete001() {
        runTest("delete //PRICE", "empty(//PRICE)");
    }

    @Test
    public void delete002() {
        runTest("delete //Q{}PRICE", "empty(//PRICE)");
    }

    @Test
    public void delete003() {
        runTest("delete //ITEM", "empty(//PRICE)");
    }

    @Test
    public void delete004() {
        runTest("delete //AUTHOR", "count(//PRICE) eq 6");
    }

    @Test
    public void delete005() {
        runTest("delete /BOOKLIST/BOOKS/ITEM/AUTHOR", "empty(//AUTHOR) and count(//PRICE) eq 6");
    }

    @Test
    public void delete006() {
        runTest("delete //ITEM[AUTHOR='Jasper Fforde']", "count(//ITEM) eq 4");
    }

    @Test
    public void follow001() {
        runTest("follow //ITEM with <caption/>", "count(//caption[preceding-sibling::*[1][self::ITEM]]) eq 6");
    }

    @Test
    public void follow002() {
        runTest("follow //ITEM with <caption>{string(AUTHOR)}</caption>", "every $c in //caption satisfies $c eq $c/preceding-sibling::*[1]/AUTHOR");
    }

    @Test
    public void list001() {
        runTest("list //AUTHOR ", "contains($output, '/BOOKLIST/BOOKS[1]/ITEM[6]/AUTHOR[1]')");
    }

    @Test
    public void list002() {
        runTest("list distinct-values(//AUTHOR)", "contains($output, 'Jasper Fforde') and contains($output, 'Thomas Hardy') and contains($output, 'Jane Austen') ");
    }

    @Test
    public void paths001() {
        runTest("paths", "contains($output, '/BOOKLIST/BOOKS/ITEM/DIMENSIONS')");
    }

    @Test
    public void precede001() {
        runTest("precede //ITEM with <caption/>", "count(//caption[following-sibling::*[1][self::ITEM]]) eq 6");
    }

    @Test
    public void precede002() {
        runTest("precede //ITEM with <caption>{string(AUTHOR)}</caption>", "every $c in //caption satisfies $c eq $c/following-sibling::*[1]/AUTHOR");
    }

    @Test
    public void prefix001() {
        runTest("prefix //ITEM with <caption/>", "count(//ITEM/*[1][self::caption]) eq 6");
    }

    @Test
    public void prefix002() {
        runTest("prefix //ITEM with attribute{'author'}{string(AUTHOR)}", "count(//ITEM[@author = AUTHOR]) eq 6");
    }

    @Test
    public void prefix003() {
        runTest("prefix //ITEM with attribute{'id'}{count(preceding-sibling::*)+1}", "(//ITEM)[2]/@id = '2'");
    }

    @Test
    public void rename001() {
        runTest("rename //* as lower-case(name())", "count(//item/price) eq 6");
    }

    @Test
    public void rename002() {
        runTest("rename //@CAT as 'category'", "count(//ITEM/@category) eq 6 and count(//ITEM/@CAT) eq 0");
    }

    @Test
    public void rename003() {
        runTest("rename //Q{}ITEM as 'item'", "count(//item/PRICE) eq 6 and empty(//ITEM)");
    }

    @Test
    public void rename004() {
        runTest("rename //Q{}PUBLISHER as 'AUTHOR'", "count(//ITEM/AUTHOR) eq 12");
    }


    @Test
    public void replace001() {
        runTest("replace //AUTHOR/text() with 'Anon'", "count(//AUTHOR[.='Anon']) eq 6");
    }

    @Test
    public void replace002() {
        runTest("replace //AUTHOR with <Writer>{string(.)}</Writer>", "count(distinct-values(//Writer)) = 4");
    }

    @Test
    public void set001() {
        runMultiTest(new String[]{"set $x = 'Jasper Fforde'", "show count(//ITEM[AUTHOR eq $x])"}, "contains($output, '2')");
    }

    @Test
    public void show001() {
        runTest("show", "contains($output, '<BOOKLIST') and contains($output, '</BOOKLIST>')");
    }

    @Test
    public void show002() {
        runTest("show //ITEM[1]/AUTHOR", "contains($output, '>Jane Austen</AUTHOR') ");
    }

    @Test
    public void show003() {
        runTest("show 2+2", "contains($output, '4') ");
    }

    @Test
    public void strip001() {
        runTest("strip", "empty(//text()[not(normalize-space())])");
    }

    @Test
    public void suffix001() {
        runTest("suffix //ITEM with <caption/>", "count(//ITEM/*[last()][self::caption]) eq 6");
    }

    @Test
    public void update001() {
        runTest("update //ITEM[1]/AUTHOR with 'AUSTEN, JANE'", "//ITEM[1]/AUTHOR eq 'AUSTEN, JANE'");
    }

    @Test
    public void validate001() {
        runMultiTest(new String[]{
                "schema " + configTest.getDataDir() + "books.xsd",
                "validate"
        }, "data((//PRICE)[1]) instance of xs:decimal");
    }
    


}

