package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicReference;

import static org.junit.jupiter.api.Assertions.assertEquals;

// Bug 2644 - Gunther Rademacher

public class ConfigDeadlockTest {
    private static final String logFile = "/Users/mike/Desktop/log.txt";

    private static class TestForSaxonBug2327 {

        protected static void testForDeadlock(Thread thread1, Thread thread2) throws InterruptedException {
            thread1.start();
            thread2.start();

            // allow 5 seconds for threads to complete
            long timeout = 5000;
            long timestamp = System.currentTimeMillis();
            thread1.join(timeout);
            thread2.join(Math.max(1, timestamp + timeout - System.currentTimeMillis()));
            if (thread1.isAlive() && thread2.isAlive()) {
                try {
                    PrintStream log = new PrintStream(new FileOutputStream(logFile));
                    log.println("THREAD 1");
                    for (StackTraceElement frame : thread1.getStackTrace()) {
                        log.println("    " + frame.toString());
                    }
                    log.println("THREAD 2");
                    for (StackTraceElement frame : thread2.getStackTrace()) {
                        log.println("    " + frame.toString());
                    }
                } catch (FileNotFoundException e) {
                    e.printStackTrace();
                }
            }
            System.out.println(thread1.isAlive() && thread2.isAlive() ? Result.SUSPECTED_DEADLOCK : Result.NORMAL_COMPLETION);
        }
    }

    private static class UseWorkaroundForSaxonBug2327 extends TestForSaxonBug2327 {

        @SuppressWarnings("unused")
        public static void main(String[] args) throws InterruptedException {
            testForDeadlock(
                    new Thread() {
                        @Override
                        public void run() {
                            new Configuration();
                            new EnterpriseConfiguration();
                        }
                    },
                    new Thread() {
                        @Override
                        public void run() {
                            new Configuration();
                        }
                    }
            );
        }

    }

    private static class UseFixForSaxonBug2327 extends TestForSaxonBug2327 {

        @SuppressWarnings("unused")
        public static void main(String[] args) throws InterruptedException {
            testForDeadlock(
                    new Thread() {
                        @Override
                        public void run() {
                            new EnterpriseConfiguration();
                        }
                    },
                    new Thread() {
                        @Override
                        public void run() {
                            new Configuration();
                        }
                    }
            );
        }

    }

    private enum Result {
        SUSPECTED_DEADLOCK,
        NORMAL_COMPLETION
    }

    //@Test(invocationCount = 10)
    public static void testWorkaroundForSaxonBug2327() throws IOException {
        Process process = execute(UseWorkaroundForSaxonBug2327.class);
        assertEquals(Result.NORMAL_COMPLETION, Result.valueOf(waitForNextOutputLine(process, 10000)));
    }

    //@Test(invocationCount = 10)
    public static void testFixForSaxonBug2327() throws IOException {
        Process process = execute(UseFixForSaxonBug2327.class);
        assertEquals(Result.NORMAL_COMPLETION, Result.valueOf(waitForNextOutputLine(process, 10000)));
    }

    private static Process execute(Class<?> clazz) throws IOException {
        List<String> args = new ArrayList<String>();
        args.add(System.getProperty("java.home") + File.separator + "bin" + File.separator + "java");
        args.add("-cp");
        args.add(System.getProperty("java.class.path"));
        args.add(clazz.getName());
        return Runtime.getRuntime().exec(args.toArray(new String[args.size()]));
    }

    private static String waitForNextOutputLine(final Process process, long waitTime) throws IOException {
        final AtomicReference<String> result = new AtomicReference<String>();
        final AtomicBoolean gotResult = new AtomicBoolean(false);
        final AtomicReference<IOException> exception = new AtomicReference<IOException>();
        Thread reader = new Thread("result reader") {
            @Override
            public void run() {
                try {
                    result.set(nextOutputLine(process));
                    gotResult.set(true);
                } catch (IOException e) {
                    exception.set(e);
                }
            }
        };
        reader.start();
        try {
            reader.join(waitTime);
        } catch (InterruptedException e) {
        }
        if (exception.get() != null)
            throw exception.get();
        if (!gotResult.get())
            throw new RuntimeException("waitForNextOutputLine timed out after " + waitTime + "ms");
        return result.get();
    }

    public static String nextOutputLine(Process process) throws IOException {
        BufferedReader reader = new BufferedReader(new InputStreamReader(process.getInputStream(), StandardCharsets.UTF_8));
        return reader.readLine();
    }

    public static void testInitialization() {
        new Configuration();
    }

    public static void main(String[] args) {
        new Configuration();
    }
}
