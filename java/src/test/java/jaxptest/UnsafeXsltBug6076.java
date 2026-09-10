package jaxptest;

import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadFactory;

/**
 * // bug 3159
 */
public class UnsafeXsltBug6076 implements Runnable {

    private static final String xsl = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"3.0\"  \n"
            + "      xmlns:map=\"http://www.w3.org/2005/xpath-functions/map\"\n"
            + "      xmlns:t=\"t\"\n"
            + "   xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n"
            + "   <xsl:param name=\"some-map\" select=\"map {}\"/>\n"
            + "\n"
            + "   <xsl:template name=\"xsl:initial-template\">\n"
            + "      <xsl:sequence select=\"t:allocate-names(('a','b','c'), ('aa','bb','cc'))\"/>\n"
            + "   </xsl:template>\n"
            + "   <!--\n"
            + "    Allocates unique names in the form $prefix{number}?.\n"
            + "    Note: that prefixes may coincide.\n"
            + "    Note: that result names shall be different not only using case.\n"
            + "      $prefixes - a name prefixes.\n"
            + "      $names - allocated names pool.\n"
            + "      Returns unique names.\n"
            + "  -->\n"
            + "   <xsl:function name=\"t:allocate-names\" as=\"xs:string*\">\n"
            + "      <xsl:param name=\"prefixes\" as=\"xs:string*\"/>\n"
            + "      <xsl:param name=\"names\" as=\"xs:string*\"/>\n"
            + "      \n"
            + "      <xsl:if test=\"exists($prefixes)\">\n"
            + "         <xsl:variable name=\"reserved\" as=\"map(xs:string, xs:string)\"\n"
            + "            select=\"map:merge($names!map{upper-case(.): .})\"/>\n"
            + "         <xsl:variable name=\"count\" as=\"xs:integer\" select=\"count($prefixes)\"/>\n"
            + "         \n"
            + "         <xsl:iterate select=\"$prefixes\">\n"
            + "            <xsl:param name=\"allocated\" as=\"map(xs:string, xs:integer+)\" select=\"\n"
            + "               map:merge\n"
            + "               (\n"
            + "               $prefixes!map{upper-case(.): 1}, \n"
            + "               map{'duplicates': 'combine'}\n"
            + "               )\"/>\n"
            + "            \n"
            + "            <xsl:param name=\"result\" as=\"map(xs:integer, xs:string)\" select=\"\n"
            + "               map:merge((1 to $count)!map{.: $prefixes=>subsequence(., 1)})\"/>\n"
            + "            \n"
            + "            <xsl:on-completion select=\"(1 to $count)!$result(.)\"/>\n"
            + "            \n"
            + "            <xsl:variable name=\"position\" as=\"xs:integer\" select=\"position()\"/>\n"
            + "            <xsl:variable name=\"key\" as=\"xs:string\" select=\"upper-case(.)\"/>\n"
            + "            \n"
            + "            <xsl:if test=\"\n"
            + "               $reserved=>map:contains($key) or \n"
            + "               (count($allocated($key)) > 1) or\n"
            + "               map:contains($t:reserved-words-map, .)\">\n"
            + "               \n"
            + "               <xsl:variable name=\"new-prefix\" as=\"xs:string\"\n"
            + "                  select=\"replace(., '\\d+$', '')\"/>\n"
            + "               <xsl:variable name=\"new-key-prefix\" as=\"xs:string\"\n"
            + "                  select=\"upper-case($new-prefix)\"/>\n"
            + "               \n"
            + "               <xsl:variable name=\"name\" as=\"xs:string\">\n"
            + "                  <xsl:iterate select=\"1 to $count + map:size($reserved) + 1\">\n"
            + "                     <xsl:variable name=\"new-key\" as=\"xs:string\"\n"
            + "                        select=\"$new-key-prefix || .\"/>\n"
            + "                     \n"
            + "                     <xsl:if test=\"\n"
            + "                        not($reserved=>map:contains($new-key)) and\n"
            + "                        not($allocated=>map:contains($new-key))\">\n"
            + "                        <xsl:break select=\"$new-prefix || .\"/>\n"
            + "                     </xsl:if>\n"
            + "                  </xsl:iterate>\n"
            + "               </xsl:variable>\n"
            + "               \n"
            + "               <xsl:next-iteration>\n"
            + "                  <xsl:with-param name=\"allocated\"\n"
            + "                     select=\"map:put($allocated, upper-case($name), 1)\"/>\n"
            + "                  <xsl:with-param name=\"result\"\n"
            + "                     select=\"map:put($result, $position, $name)\"/>\n"
            + "               </xsl:next-iteration>\n"
            + "            </xsl:if>\n"
            + "         </xsl:iterate>\n"
            + "      </xsl:if>\n"
            + "   </xsl:function>\n"
            + "   \n"
            + "   <xsl:variable name=\"t:csharp-reserved-words\" as=\"xs:string+\" select=\"\n"
            + "      'abstract', 'as',       'base',       'bool',      'break',\n"
            + "      'byte',     'case',     'catch',      'char',      'checked',\n"
            + "      'class',    'const',    'continue',   'decimal',   'default',\n"
            + "      'delegate', 'do',       'double',     'else',      'enum',\n"
            + "      'event',    'explicit', 'extern',     'false',     'finally',\n"
            + "      'fixed',    'float',    'for',        'foreach',   'goto',\n"
            + "      'if',       'implicit', 'in',         'int',       'interface',\n"
            + "      'internal', 'is',       'lock',       'long',      'namespace',\n"
            + "      'new',      'null',     'object',     'operator',  'out',\n"
            + "      'override', 'params',   'private',    'protected', 'public',\n"
            + "      'readonly', 'ref',      'return',     'sbyte',     'sealed',\n"
            + "      'short',    'sizeof',   'stackalloc', 'static',    'string',\n"
            + "      'struct',   'switch',   'this',       'throw',     'true',\n"
            + "      'try',      'typeof',   'uint',       'ulong',     'unchecked',\n"
            + "      'unsafe',   'ushort',   'using',      'virtual',   'void',\n"
            + "      'volatile', 'while'\"/>\n"
            + "   \n"
            + "   \n"
            + "   \n"
            + "   <!-- A total set of reserved words. -->\n"
            + "   <xsl:variable name=\"t:reserved-words\" as=\"xs:string*\"\n"
            + "      select=\"$t:csharp-reserved-words\"/>\n"
            + "   \n"
            + "   <!-- A map of reserved words. -->\n"
            + "   <xsl:variable name=\"t:reserved-words-map\" as=\"map(xs:string, xs:string)\"\n"
            + "      select=\"map:merge($t:reserved-words!map{.: .})\"/>\n"
            + "   \n"
            + "</xsl:stylesheet>";



    private static final XsltExecutable templates;
    private final static Processor proc = new Processor(true);

    static {

        try {
            templates = proc.newXsltCompiler().compile(new StreamSource(new StringReader(xsl)));
        } catch (SaxonApiException e) {
            throw new RuntimeException(e);
        }
    }

    public static void main(String[] args) throws Throwable {
        final int threadSize = 30;
        final CountDownLatch latch = new CountDownLatch(threadSize);
        final ThreadFactory threadFactory = Executors.defaultThreadFactory();
        final Thread[] threads = new Thread[threadSize];
        for (int i = 0; i < threadSize; ++i) {
            threads[i] = threadFactory.newThread(new UnsafeXsltBug6076(latch));
        }
        for (int i = 0; i < threadSize; ++i) {
            threads[i].start();
        }
        for (int i = 0; i < threadSize; ++i) {
            threads[i].join();
        }
    }

    private final CountDownLatch latch;

    public UnsafeXsltBug6076(final CountDownLatch latch) {
        this.latch = latch;
    }

    @Override
    public void run() {
        try {
            latch.countDown();
            latch.await();
            final int size = 1000;
            final ByteArrayOutputStream output = new ByteArrayOutputStream();
            final Serializer result = proc.newSerializer(output);
            for (int i = 0; i < size; ++i) {
                output.reset();
                final Xslt30Transformer transformer;
                synchronized(templates) {
                    transformer = templates.load30();
                }
                transformer.callTemplate(null, result);
            }
        } catch (Throwable t) {
            t.printStackTrace(System.out);
        }
    }

}

