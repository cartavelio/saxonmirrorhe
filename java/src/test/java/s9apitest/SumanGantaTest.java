package s9apitest;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XQueryCompiler;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import static org.junit.jupiter.api.Assertions.fail;


/**
 * Loads xquery module info by parsing the module text
 */
public class SumanGantaTest {
    private final int mIterations = 2000;

    @Test
    public void testModuleLink() {
        parseWithModuleLinking(false);
    }

    @Test
    public void testParse() {
        justParse(false);
    }

    /**
     * Parse and execute an expression with linked modules
     */
    public void justParse(boolean aSequential) {
        Processor proc = new Processor(true);

        try {
            String expression = getTestExpression();
            XQueryCompiler compiler = proc.newXQueryCompiler();
            compiler.setBaseURI(Testdata.fileUri("ganta/"));

            parse(compiler, expression, "justParse", mIterations, aSequential);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Parse and execute an expression with linked modules
     */
    public void parseWithModuleLinking(boolean aSequential) {
        Processor proc = new Processor(true);

        try {
            String expression = getTestExpressionSansLoc();
            proc.setConfigurationProperty(FeatureKeys.DEBUG_BYTE_CODE, true);
            XQueryCompiler comp = proc.newXQueryCompiler();

            Collection<String> moduleFiles = new ArrayList<String>();

            moduleFiles.add(Testdata.pathString("ganta/genBPEL.xq"));
            moduleFiles.add(Testdata.pathString("ganta/hello_notworking.xq"));
            moduleFiles.add(Testdata.pathString("ganta/avoscentral.xq"));
            addLibs(comp, moduleFiles);

            parse(comp, expression, "justParse", mIterations, aSequential);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void parse(XQueryCompiler compiler, final String aExpression, String aTestName, int aTimes, boolean aSequential) {
        try {
            long start = System.currentTimeMillis();
            List<Exception> errors = new ArrayList<>();
            Runnable task = new Runnable() {
                @Override
                public void run() {
                    try {
                        compiler.compile(aExpression);
                    } catch (Exception e) {
                        errors.add(e);
                        e.printStackTrace();
                        fail();
                    }
                }
            };
            if (aSequential) {
                executeSequential(task, aTimes);
            } else {
                executeConcurrent(task, aTimes);
            }
            if (!errors.isEmpty()) {
                fail(errors.get(0));
            }
            report(aTestName, System.currentTimeMillis() - start);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    private void report(String aTestName, long aTotal) {
        long avg = aTotal / mIterations;
        System.out.println(aTestName + ":" + aTotal + "ms (total), " + mIterations + " (iterations), " + avg + "ms (avg)");
    }

    private long executeConcurrent(Runnable aTask, int aTimes) throws Exception {
        long start = System.currentTimeMillis();
        ArrayList<Thread> threads = new ArrayList<Thread>();
        for (int i = 0; i < aTimes; ++i) {
            Thread t = new Thread(aTask);
            t.start();
            threads.add(t);
        }

        for (Thread t : threads) {
            t.join();
        }
        return System.currentTimeMillis() - start;
    }

    private long executeSequential(Runnable aTask, int aTimes) throws Exception {
        long start = System.currentTimeMillis();
        for (int i = 0; i < aTimes; ++i) {
            aTask.run();
        }
        return System.currentTimeMillis() - start;
    }

    private String getTestExpressionSansLoc() {
        String module1 = "import module namespace myf = 'urn:active-endpoints:bpep:function';\n";
        String module2 = "import module namespace avcf = 'urn:active-endpoints:avc:xquery:function';\n";
        String module3 = "import module namespace avsf = 'urn:activevos:avsf:xquery:function';\n";
        String expression = module1 + module2 + module3 + "myf:sayHello('Hello')";
        return expression;
    }

    private String getTestExpression() {
        String module1 = "import module namespace myf = 'urn:active-endpoints:bpep:function' at 'hello.xq';\n";
        String module2 = "import module namespace avcf = 'urn:active-endpoints:avc:xquery:function' at 'avoscentral.xq';\n";
        String module3 = "import module namespace avsf = 'urn:activevos:avsf:xquery:function' at 'genBPEL.xq';\n";
        String expression = module1 + module2 + module3 + "myf:sayHello('Hello')";
        return expression;
    }

    private void addLibs(XQueryCompiler compiler, Collection<String> aModuleFiles) {
        try {
            for (String file : aModuleFiles) {
                compiler.setBaseURI(new File(file).toURI());
                compiler.compileLibrary(new FileInputStream(file));
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }
}

