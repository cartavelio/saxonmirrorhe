package commands;
import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XQueryCompiler;

import java.util.Random;

public class Saxon3483 implements Runnable {
    private static final Object printLock = new Object();
    private static final Object compilerLock = new Object();
    private static XQueryCompiler xqueryCompiler = null;

    private static int testCycle = 0;

    public static void main(String[] args) throws Exception {
        Random random = new Random();
        for (; ; ) {
            ++testCycle;
            synchronized(compilerLock) {
                xqueryCompiler = null;
            }

            Thread t1 = new Thread(new Saxon3483());
            Thread t2 = new Thread(new Saxon3483());

            t1.start();
            Thread.sleep(random.nextInt(8));
            t2.start();

            t1.join();
            t2.join();

            if (testCycle % 100 == 0)
                System.err.println("number of completed test cycles: " + testCycle);
        }
    }

    @Override
    public void run() {
        try {
            getXQueryCompiler().compile(QUERY);
        } catch (Exception e) {
            synchronized(printLock) {
                if (e.getMessage().contains("Internal Saxon error")) {
                    e.printStackTrace(System.err);
                    System.err.println("total number of test cycles: " + testCycle);
                    System.exit(1);
                } else {
                    System.err.println(e.getClass().getName() + ": " + e.getMessage());
                }
            }
        }
    }

    static XQueryCompiler getXQueryCompiler() throws SaxonApiException {
        synchronized(compilerLock) {
            if (xqueryCompiler == null) {
                try {
                    Configuration configuration = new EnterpriseConfiguration();
                    configuration.setConfigurationProperty(FeatureKeys.XQUERY_VERSION, "3.1");
                    configuration.setBooleanProperty(FeatureKeys.STREAMING_FALLBACK, true);
                    Processor processor = new Processor(configuration);
                    XQueryCompiler compiler = processor.newXQueryCompiler();
                    compiler.compileLibrary(MODULE);
                    xqueryCompiler = compiler;
                } catch (Throwable e) {
                    xqueryCompiler = null;
                    throw e;
                }
            }
            return xqueryCompiler;
        }
    }

    private static final String QUERY = "import module namespace m='MODULE';\n" +
            "for $row in saxon:stream(doc('uriresolver:resolve')/records/record)\n" +
            "return $row/[m:string(z1), m:string(t1), m:string(d1), m:string(z2), m:string(t2), m:string(d2)]\n";

    private static final String MODULE =
            "xquery version '3.1';\n" +
                    "module namespace m='MODULE';\n" +
                    "\n" +
                    "declare namespace map='http://www.w3.org/2005/xpath-functions/map';\n" +
                    "declare namespace array='http://www.w3.org/2005/xpath-functions/array';\n" +
                    "\n" +
                    "declare function m:guess($row as item(), $attributes-enabled as xs:boolean) as element(column)*\n" +
                    "{\n" +
                    "  typeswitch ($row)\n" +
                    "  case node() return\n" +
                    "    let $columns :=\n" +
                    "    (\n" +
                    "      $row/@*[$attributes-enabled and namespace-uri(.) ne 'http://www.w3.org/2001/XMLSchema-instance'],\n" +
                    "      $row/*\n" +
                    "    )\n" +
                    "    let $count := count($columns)\n" +
                    "  \n" +
                    "    for $column at $i in $columns\n" +
                    "  \n" +
                    "    let $local-name := local-name($column)\n" +
                    "    let $namesake-count := count(subsequence($columns, 1, $i - 1)[local-name() = $local-name])\n" +
                    "    let $namesake-suffix := string($namesake-count + 1)[$namesake-count > 0]\n" +
                    "  \n" +
                    "    let $steps := $column/ancestor-or-self::node()[. >> $row]\n" +
                    "    let $node-name := node-name($column)\n" +
                    "    let $path-index := 1 + count($column/preceding-sibling::*[node-name(.) = $node-name])\n" +
                    "    let $path := string-join($steps/m:eqname(.), '/')\n" +
                    "    let $type := xs:QName($column/@xsi:type)\n" +
                    "    return\n" +
                    "      element column\n" +
                    "      {\n" +
                    "        element name {string-join(($local-name, $namesake-suffix), '_')},\n" +
                    "        element locator {if ($column instance of attribute()) then $path else concat($path, '[', string($path-index), ']')},\n" +
                    "        element type\n" +
                    "        {\n" +
                    "          if (namespace-uri-from-QName($type) = 'http://www.w3.org/2001/XMLSchema') then\n" +
                    "            local-name-from-QName($type)\n" +
                    "          else\n" +
                    "            ()\n" +
                    "        },\n" +
                    "        element sample {string($column)},\n" +
                    "        element last {$i eq $count}\n" +
                    "      }\n" +
                    "      \n" +
                    "  case map(*) return\n" +
                    "    let $count := map:size($row)\n" +
                    "    let $column-names :=\n" +
                    "      for $column-name in map:keys($row)\n" +
                    "      order by $column-name\n" +
                    "      return $column-name\n" +
                    "    for $column at $i in $column-names\n" +
                    "    let $value := $row?($column)\n" +
                    "    return\n" +
                    "      element column\n" +
                    "      {\n" +
                    "        element name {$column},\n" +
                    "        element locator {m:lookup-expr($column)},\n" +
                    "        element type {m:type($value)},\n" +
                    "        element sample {m:value($value)},\n" +
                    "        element last {$i eq $count}\n" +
                    "      }\n" +
                    "\n" +
                    "  case array(*) return\n" +
                    "    let $count := array:size($row)\n" +
                    "    for $i in (1 to $count)\n" +
                    "    let $value := $row?($i)\n" +
                    "    return\n" +
                    "      element column\n" +
                    "      {\n" +
                    "        element name {concat('Column ', $i)},\n" +
                    "        element locator {concat('?', $i)},\n" +
                    "        element type {m:type($value)},\n" +
                    "        element sample {m:value($value)},\n" +
                    "        element last {$i eq $count}\n" +
                    "      }\n" +
                    "\n" +
                    "  default return\n" +
                    "    element column\n" +
                    "    {\n" +
                    "      element name {'Column'},\n" +
                    "      element locator {'.'},\n" +
                    "      element type {m:type($row)},\n" +
                    "      element sample {m:value($row)},\n" +
                    "      element last {true()}\n" +
                    "    }\n" +
                    "};\n" +
                    "\n" +
                    "declare function m:eqname($node)\n" +
                    "{\n" +
                    "  let $namespace := namespace-uri($node)\n" +
                    "  let $local-name := local-name($node)\n" +
                    "  return\n" +
                    "    string-join\n" +
                    "    ((\n" +
                    "    '@'[$node instance of attribute()],\n" +
                    "      ('Q{', $namespace, '}')[$namespace],\n" +
                    "    $local-name\n" +
                    "  ))\n" +
                    "};\n" +
                    "\n" +
                    "declare function m:lookup-expr($name)\n" +
                    "{\n" +
                    "  try\n" +
                    "  {\n" +
                    "    concat('?', string($name cast as xs:NCName))\n" +
                    "  }\n" +
                    "  catch *\n" +
                    "  {\n" +
                    "    concat('?(''', replace(replace($name, '''', ''''''), '&amp;', '&amp;amp;'), ''')')\n" +
                    "  }\n" +
                    "};\n" +
                    "\n" +
                    "declare function m:type($value)\n" +
                    "{\n" +
                    "  typeswitch ($value)\n" +
                    "  case empty-sequence() return ()\n" +
                    "  case xs:double return 'xs:double'\n" +
                    "  case xs:boolean return 'xs:boolean'\n" +
                    "  case xs:integer return 'xs:integer'\n" +
                    "  case xs:long return 'xs:long'\n" +
                    "  default return 'xs:string'\n" +
                    "};\n" +
                    "\n" +
                    "declare function m:value($item as item()?) as node()+\n" +
                    "{\n" +
                    "  typeswitch ($item)\n" +
                    "  case empty-sequence() | map(*) | array(*) return\n" +
                    "    attribute xsi:nil {true()}\n" +
                    "  case node() return\n" +
                    "  (\n" +
                    "    $item/@xsi:nil,\n" +
                    "    $item/@xsi:type,\n" +
                    "    text {$item}\n" +
                    "  )\n" +
                    "  default return\n" +
                    "    text {m:string($item)}\n" +
                    "};\n" +
                    "\n" +
                    "declare function m:string($item as item()?) as xs:string?\n" +
                    "{\n" +
                    "  typeswitch ($item)\n" +
                    "  case empty-sequence() | map(*) | array(*) return\n" +
                    "    ()\n" +
                    "  case node() return\n" +
                    "    string($item)[not(xs:boolean($item/@xsi:nil))] \n" +
                    "  default return\n" +
                    "   string($item)\n" +
                    "};\n";
}
