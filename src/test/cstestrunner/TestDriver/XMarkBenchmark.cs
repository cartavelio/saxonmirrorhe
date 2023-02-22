using System;
using System.Collections.Generic;
using System.Diagnostics;

using Saxon.Hej;
using Saxon.Hej.@event;
using Saxon.Hej.lib;
using Saxon.Hej.om;
using Saxon.Hej.query;
using Saxon.Hej.trace;
using Saxon.Hej.trans;
using Saxon.Hej.value;
using Saxon.Api;
using Saxon.Ejava.io;
using Saxon.Ejava.util;
using Singulink.Numerics;
using FeatureKeys = Saxon.Api.FeatureKeys;
using TreeModel = Saxon.Api.TreeModel;
using Version = System.Version;
using SysIO = System.IO;
using SysIoFile = System.IO.File;

using SysCollGen = System.Collections.Generic;

namespace TestRunner {
    public class XMarkBenchmark {


        private XMarkBenchmark() {
        }

        static bool pull = false;
        static bool lazy = false;
        static bool saproduct = false;
        static bool satests = false;
        static bool xslt = false;
        static bool checkResults = false;
        static bool val = false;
        static int test = -1;
        static string programPath = Environment.CurrentDirectory;
        static string dataPath = "";

        private static TreeModel treeModel = TreeModel.TinyTree;
        private bool threadStop = false;


        static string[] tests = {"xmark1.xml", "xmark4.xml", "xmark10.xml"};

        private static TreeModel[] models = {TreeModel.TinyTree, TreeModel.TinyTreeCondensed, TreeModel.LinkedTree};
        // new DOMObjectModel(), new JDOM2ObjectModel(),
        // new DOM4JObjectModel(), new XOMObjectModel(), new AxiomObjectModel(), new DominoTreeModel()};
//    static TreeModel[] models = {new DOMObjectModel()};

        public static readonly int MAX_RUNS = /*10000;*/ 1000;
        public static readonly long MAX_TIME = /*3_000_000_000_000L;*/ 30_000;

        public static void setProgramPath(string path) {
            programPath = path;
        }

        public static string getProgramPath() {
            return programPath;
        }

        public static void setDataPath(string path) {
            dataPath = path;
        }

        public static string getDataPath() {
            return dataPath;
        }

        /**
     * Main entry point (called via TestDriver -lang:XMark)
     *
     * @param argv command line arguments. The following arguments are recognised:
     *             <dl>
     *             <dt>-dir:directory</dt><dd>The directory containing the tests</dd>
     *             <dt>-pull</dt><dd>Run in pull mode</dd>
     *             <dt>-lazy</dt><dd>Enable lazy construction mode</dd>
     *             <dt>-sa</dt><dd>Run standard tests in schema-aware mode</dd>
     *             <dt>-satests</dt><dd>Run schema-aware versions of the tests in schema-aware mode</dd>
     *             <dt>-xslt</dt><dd>Run XSLT versions of the tests</dd>
     *             <dt>-runcomp</dt><dd>set bytecode on or off, or integer threshold</dd>
     *             <dt>-qN</dt><dd>Run query N only</dd>
     *             <dt>-size:N</dt><dd>Run only on size=N Mb files</dd>
     *             </dl>
     */
        public static void XMark(string[] argv) {
            XMarkBenchmark xmark = new XMarkBenchmark();
            xmark.go(argv);

        }

//    public void setTestDriverForm(TestDriverForm gui) {
//        guiForm = gui;
//    }

        private void println(string data) {
            Console.Error.WriteLine(data);
        }

        private void printError(string error, Exception e) {
            println(error + e.StackTrace);
        }

        /**
     * Print memory usage.
     */
        private static string printMemory() {
            // System.gc();
            //
            // long totalMemory = Runtime.getRuntime().totalMemory();
            // long freeMemory = Runtime.getRuntime().freeMemory();
            // return " memory ='" + ((totalMemory - freeMemory) / 1024) + "'";
            return "0Mb";
        }

        public void stopThread(bool stop) {
            threadStop = stop;
        }


        private void go(string[] argv) {
            string dir = null;
            foreach (string arg in argv) {
                if (arg.StartsWith("-dir:")) {
                    dir = arg.Substring(5);
                }

                if (arg == "-pull") {
                    pull = true;
                }

                if (arg == "-lazy") {
                    lazy = true;
                }

                if (arg == "-sa") {
                    saproduct = true;
                }

                // means use Saxon-EE optimizer (not the schema-aware version of the tests)
                if (arg == "-satests") {
                    satests = true;
                }

                if (arg == "-xslt") {
                    xslt = true;
                }

                if (arg == "-check") {
                    checkResults = true;
                }

                if (arg == "-val") {
                    val = true;
                }

                if (arg.StartsWith("-q")) {
                    test = Int32.Parse(arg.Substring(2));
                }

                if (arg.StartsWith("-size:")) {
                    tests = new string[] {"xmark" + arg.Substring(6) + ".xml"};
                }

                if (arg.StartsWith("-tree")) {
                    string modelName = arg.Substring(6);
                    if (modelName.Equals("dom", StringComparison.OrdinalIgnoreCase)) {
                        //treeModel = new DOMObjectModel();
                    }
                    else if (modelName.Equals("domino", StringComparison.OrdinalIgnoreCase)) {
                        //treeModel = new DominoTreeModel();
                    }
                    else if (modelName.Equals("tinytree", StringComparison.OrdinalIgnoreCase)) {
                        treeModel = TreeModel.TinyTree;
                    }
                    else if (modelName.Equals("condensed", StringComparison.OrdinalIgnoreCase)) {
                        treeModel = TreeModel.TinyTreeCondensed;
                    }
                    else if (modelName.Equals("linked", StringComparison.OrdinalIgnoreCase)) {
                        treeModel = TreeModel.LinkedTree;
                    }
                    else {
                        throw new Exception("The TreeModel specified does not exist");
                    }

                }

                if (arg == "-?") {
                    println("Options: -pull -lazy -sa -xslt -runcomp -tree");
                }
            }

            if (dir == null) {
                dir = satests ? programPath + "//sa//" : programPath + "//";
            }

            if (xslt) {
                transform(lazy, saproduct, dir);
            }
            else {
                query(dir);
            }
        }

        /**
     * Run all the XQuery tests
     *
     * @param dir the directory containing the tests
     * @throws Exception
     */

        public void query(string dir) {
            //long per_millisecs = 1000000;
            //long ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            string options = "";
            string when = DateTimeValue.getCurrentDateTime(null).getStringValue();
            if (saproduct) options += "-sa ";
            if (satests) options += "-satests ";
            if (pull) options += "-pull ";
            if (lazy) options += "-lazy ";
            if (val) options += "-val ";

            println("<xmark-results product='Saxon' lang='query' version='" + Saxon.Hej.Version.getProductVersion() +
                    "' options='" + options + "' when='" + when + "'>");
            Stopwatch stopWatch = new Stopwatch();

            const double thresholdSec = 2;
            foreach (TreeModel model in models) {
                if (treeModel != model) {
                    continue;
                }
                Processor proc = new Processor(true);
                XQueryCompiler comp = proc.NewXQueryCompiler();
                
                foreach (string test1 in tests) {
                    proc.SetProperty(Saxon.Api.Feature.TRACE_OPTIMIZER_DECISIONS, false);
                    if (saproduct) {
                        // if (satests) {
                        //     //AutoActivate.activate(config);
                        //     config.setSchemaValidationMode(Validation.STRICT);
                        //     config.addSchemaSource(new StreamSource(new File(dir + "schema.xsd")),
                        //                            config.makeErrorReporter());
                        //     config.setBooleanProperty(FeatureKeys.USE_TYPED_VALUE_CACHE, false);
                        // }
                        // if (val) {
                        //     config.loadSchema(new File(dir + "sa//schema.xsd").toURI().toString());
                        //     config.setSchemaValidationMode(Validation.STRICT);
                        // }
                    }



                    long pstart = now();
                    XdmNode doc = null;
                    try {
                        DocumentBuilder builder = proc.NewDocumentBuilder();
                        doc = builder.Build(new Uri("file://" + dir + test1, UriKind.Absolute));
                    }
                    catch (Exception e) {
                        printError("Error loading the file: " + dir + test1, e);
                    }

                    long pend = now();

                    Console.WriteLine("  <file name='" + test1 +
                                      "' size='" + new SysIO.FileInfo(dir + test1).Length +
                                      "' build-time='" + (pend - pstart) +
                                      "' model='" + model + "'" + printMemory() + ">");
                    Properties props = new Properties();
                    props.setProperty("method", "xml");
                    props.setProperty("indent", "yes");

                    for (int q = 1; q <= 20; q++) {
                        if (test != -1 && test != q) {
                            continue;
                        }

                        try {
                            SysCollGen.List<long> measures = new SysCollGen.List<long>();
                            File query = new File(dir + 'q' + q + ".xq");
                            comp.BaseUri = query.toURI();
                            comp.SchemaAware = val;
                            XQueryExecutable exp =
                                comp.Compile(new SysIO.FileStream(dir + 'q' + q + ".xq", SysIO.FileMode.Open));
                            //exp.explain(new ExpressionPresenter(config, new StreamResult(System.out)));
                            int runs = 0;
                            long totalTime = 0;
                            long min = Int64.MaxValue;
                            ;
                            long max = 0;
                            IDestination sink = new NullDestination();
                            for (int t = 0; t < MAX_RUNS; t++) {
                                XQueryEvaluator eval = exp.Load();
                                eval.ContextItem = doc;
                                IDestination result = sink;
                                long start, end;
                                stopWatch.Reset();
                                stopWatch.Start();
                                eval.Run(result);
                                stopWatch.Stop();
                                runs++;

                                long time = stopWatch.Elapsed.Ticks;
                                //System.err.println("time = " + time);
                                if (time < min) min = time;
                                if (time > max) max = time;
                                measures.Add(time);
                                totalTime += time;
                                if (runs == 1) {
                                    Instrumentation.report();
                                    Instrumentation.reset();
                                }

                                if (totalTime / TimeSpan.TicksPerMillisecond > MAX_TIME && t >= 2) break;
                            }

                            long medianValue = median(measures);
                            double avg = (double) (totalTime) / runs;
                            println("    <query q='" + q +
                                    "' avg='" + avg / TimeSpan.TicksPerMillisecond +
                                    "' median='" + (double) medianValue / TimeSpan.TicksPerMillisecond +
                                    "' runs='" + runs +
                                    //"' totalTime='" + ((double) totalTime / per_millisecs) +
                                    "' min='" + ((double) min / TimeSpan.TicksPerMillisecond) +
                                    "' max='" + ((double) max / TimeSpan.TicksPerMillisecond) + "'/>");
                            if (threadStop) {
                                break;
                            }
                        }
                        catch (XPathException err) {
                            println("Error in test " + q);
                            println(err.getMessageAndLocation());
                        }
                    }

                    println("  </file>");
                    if (threadStop) {
                        break;
                    }
                }
            }

            println("</xmark-results>");
        }

        protected internal static long now() {
            return Saxon.Ejava.lang.System.nanoTime();
        }

        private long median(SysCollGen.List<long> measures) {
            long[] m = measures.ToArray();
            Arrays.sort(m);
            return m[(m.Length / 2) - 1];
        }

        /**
     * Run an XSLT transformation
     *
     * @param lazy true if lazy construction mode is to be set
     * @param sa   true if schema-aware
     * @param dir  the directory containing the tests
     * @throws Exception
     */

        public void transform(bool lazy, bool sa, string dir) {

            Console.WriteLine("<xmark-results product='Saxon' lang='xslt' version='" +
                              Saxon.Hej.Version.getProductVersion() + "'>");

            for (int f = 0; f < tests.Length; f++) {
                Processor proc = new Processor(true);
                DocumentBuilder builder = proc.NewDocumentBuilder();
                long pstart = now();
                XdmNode doc = builder.Build(SysIoFile.OpenText(dir + tests[f]));
                long pend = now();

                Console.WriteLine("  <file name='" + tests[f] +
                                  "' size='" + new SysIO.FileInfo(dir + tests[f]).Length +
                                  "' schema-aware='" + sa +
                                  "' build-time='" + (pend - pstart) + "'>");

                for (int q = 1; q <= 20; q++) {
                    //if (q==9) continue;
                    Saxon.Ejava.io.File sheet = new (dir + 'q' + q + ".xsl");
                    if (!sheet.exists()) {
                        continue;
                    }

                    XsltExecutable templates =
                        proc.NewXsltCompiler().Compile(System.IO.File.OpenText(dir + 'q' + q + ".xsl"));

                    int runs = 0;
                    long totalTime = 0;
                    long min = Int64.MaxValue;
                    long max = 0;
                    IDestination sink = new NullDestination();
                    for (int t = 0; t < 10; t++) {
                        IDestination result = sink;
                        long start, end;

                        start = now();
                        Xslt30Transformer transformer = templates.Load30();
                        transformer.GlobalContextItem = doc;
                        transformer.ApplyTemplates(doc, result);
                        end = now();

                        runs++;
                        long time = (end - start);
                        if (time < min) min = time;
                        if (time > max) max = time;
                        totalTime += time;
                        if (totalTime > 1000 && t >= 2) break;
                    }

                    Console.WriteLine("    <query q='" + q +
                                      "' avg='" + (totalTime / runs) +
                                      "' runs='" + runs +
                                      "' min='" + min +
                                      "' max='" + max + "'/>");
                }

                Console.WriteLine("  </file>");
            }

            Console.WriteLine("</xmark-results>");
        }

//     /**
//      * Compare two files containing test results
//      *
//      * @param actual the actual results
//      * @param gold   the reference results
//      */
//
//     private static void compare(File actual, File gold) {
//         try {
//             // This is decoding bytes assuming the platform default encoding
//             FileReader reader1 = new FileReader(actual);
//             FileReader reader2 = new FileReader(gold);
//             char[] contents1 = new char[65536];
//             char[] contents2 = new char[65536];
//             int size1 = reader1.read(contents1, 0, 65536);
//             int size2 = reader2.read(contents2, 0, 65536);
//             reader1.close();
//             reader2.close();
//             int offset1 = 0;
//             int offset2 = 0;
//             if (contents1[0] == '\u00ef' && contents1[1] == '\u00bb' && contents1[2] == '\u00bf') {
//                 offset1 += 3;
//             }
//             if (contents2[0] == '\u00ef' && contents2[1] == '\u00bb' && contents2[2] == '\u00bf') {
//                 offset2 += 3;
//             }
//             string actResult = (size1 == -1 ? "" : new string(contents1, offset1, size1 - offset1));
//             string refResult = (size2 == -1 ? "" : new string(contents2, offset2, size2 - offset2));
//
// //            actResult = actResult.replace("\r\n", "\n");
// //            refResult = refResult.replace("\r\n", "\n");
//             if (actResult.equals(refResult)) {
//                 System.err.println("Results compared OK");
//             } else {
//                 System.err.println("*** Results do not match");
//             }
//         } catch (Exception err) {
//             System.err.println("Failed to compare with gold results: " + err.getMessage());
//         }
//     }

        //private static readonly DecimalFormat format = new DecimalFormat("######0.00");



    }
}