package bugs;


import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.regex.CaseVariants;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XPathExecutable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.Type;
import net.sf.saxon.z.IntArraySet;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntToIntHashMap;
import net.sf.saxon.z.IntToIntMap;

import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.time.Duration;
import java.time.Instant;
import java.util.ArrayList;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.function.IntFunction;

public class Bug6225 {
        public static final int THREADS = 24;
        public static final int ITERATIONS = 1_000;

        public static void main(String[] args) throws InterruptedException {

            // Ensure all the maps have been built, so we are only testing the performance
            // of getting variants
            CaseVariants.getCaseVariants('a');
            CaseVariantsModified.getCaseVariants('a');

            final ExecutorService executorService = Executors.newFixedThreadPool(THREADS);

            try {
                XPathExecutable expr = new Processor().newXPathCompiler().compile("matches('ABC', 'abc', 'i')");
                for (int round = 1; round <= 10; round++) {
                    System.out.println("Round " + round);
                    runTest("CaseVariants", executorService, CaseVariants::getCaseVariants, expr);
                    runTest("CaseVariantsModified", executorService, CaseVariantsModified::getCaseVariants, expr);
                    runTest("CaseVariantsOnDemand", executorService, CaseVariantsOnDemand::getCaseVariants, expr);
                }
                executorService.shutdown();
            } catch (SaxonApiException e) {
                throw new RuntimeException(e);
            }
        }

        private static void runTest(final String name,
                                    final ExecutorService executorService,
                                    final IntFunction<int[]> func,
                                    final XPathExecutable expr) throws InterruptedException {

            final CountDownLatch startAllLatch = new CountDownLatch(1);
            final CountDownLatch threadReadyLatch = new CountDownLatch(THREADS);
            final CountDownLatch completionLatch = new CountDownLatch(THREADS);

            for (int k = 0; k < THREADS; k++) {
                executorService.submit(() -> {
                    threadReadyLatch.countDown();
                    try {
                        // All threads start together
                        startAllLatch.await();
                    } catch (InterruptedException e) {
                        throw new RuntimeException(e);
                    }
                    for (long j = 0; j < ITERATIONS; j++) {
                        for (int i = 0; i < 1000; i++) {
                            func.apply(i);
                            try {
                                expr.load().evaluate();
                            } catch (SaxonApiException e) {
                                throw new RuntimeException(e);
                            }
                        }
                    }
                    completionLatch.countDown();
                });
            }
            final Instant startTime = Instant.now();
            // Wait for all threads to be ready
            threadReadyLatch.await();
            // Release all threads
            startAllLatch.countDown();
            // Wait for all threads to complete
            completionLatch.await();
            final Duration duration = Duration.between(startTime, Instant.now());
            System.out.println(name + " completed in " + duration.toMillis() + "ms");
        }

        // --------------------------------------------------------------------------------


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Copyright (c) 2018 Saxonica Limited.
        // This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
        // If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
        // This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /**
         * Modified copy of {@link net.sf.saxon.regex.CaseVariants}.
         */
        private static class CaseVariantsModified {

            // Use one hashmap for characters with a single case variant, another for characters with multiple
            // case variants, to reduce the number of objects that need to be allocated
            // FIX: Make these volatile
            private static volatile IntToIntMap monoVariants = null;
            private static volatile IntHashMap<int[]> polyVariants = null;

            static void build() {

                // FIX: Use temporary local variables
                final IntToIntMap monoVariants = new IntToIntHashMap(2500);
                final IntHashMap<int[]> polyVariants = new IntHashMap<>(100);

                InputStream in = Version.platform.locateResource("casevariants.xml", new ArrayList<>());
                if (in == null) {
                    throw new RuntimeException("Unable to read casevariants.xml file");
                }

                Configuration config = new Configuration();
                ParseOptions options = new ParseOptions();
                options = options.withSchemaValidationMode(Validation.SKIP);
                options = options.withDTDValidationMode(Validation.SKIP);
                NodeInfo doc;
                try {
                    doc = config.buildDocumentTree(new StreamSource(in, "casevariants.xml"), options).getRootNode();
                } catch (XPathException e) {
                    throw new RuntimeException("Failed to build casevariants.xml", e);
                }

                AxisIterator iter = doc.iterateAxis(AxisInfo.DESCENDANT, new NameTest(Type.ELEMENT, NamespaceUri.NULL, "c", config.getNamePool()));
                while (true) {
                    NodeInfo item = iter.next();
                    if (item == null) {
                        break;
                    }
                    String code = item.getAttributeValue("", "n");
                    int icode = Integer.parseInt(code, 16);
                    String variants = item.getAttributeValue("", "v");
                    String[] vhex = variants.split(",");
                    int[] vint = new int[vhex.length];
                    for (int i = 0; i < vhex.length; i++) {
                        vint[i] = Integer.parseInt(vhex[i], 16);
                    }
                    if (vhex.length == 1) {
                        monoVariants.put(icode, vint[0]);
                    } else {
                        polyVariants.put(icode, vint);
                    }
                }
                // FIX: Set static class variables at the end
                CaseVariantsModified.polyVariants = polyVariants;
                // Must set this last as other threads will be checking this value
                CaseVariantsModified.monoVariants = monoVariants;
            }

            /**
             * Get the case variants of a character
             *
             * @param code the character whose case variants are required
             * @return the case variants of the character, excluding the character itself
             */

            // FIX: Method no longer synchronized
            public static int[] getCaseVariants(int code) {
                // FIX: Use local variables to avoid additional volatile reads
                IntToIntMap monoVariants = CaseVariantsModified.monoVariants;
                if (monoVariants == null) {
                    synchronized(CaseVariantsModified.class) {
                        monoVariants = CaseVariantsModified.monoVariants;
                        if (monoVariants == null) {
                            build();
                            monoVariants = CaseVariantsModified.monoVariants;
                        }
                    }
                }
                int mono = monoVariants.get(code);
                if (mono != monoVariants.getDefaultValue()) {
                    return new int[]{mono};
                } else {
                    int[] result = polyVariants.get(code);
                    if (result == null) {
                        return IntArraySet.EMPTY_INT_ARRAY;
                    } else {
                        return result;
                    }
                }
            }

            /**
             * Get the case variants of roman letters (A-Z, a-z), other than the letters A-Z and a-z themselves
             */

            /*@NotNull*/ public static int[] ROMAN_VARIANTS = {0x0130, 0x0131, 0x212A, 0x017F};
        }


    /**
     * Modified copy of {@link net.sf.saxon.regex.CaseVariants}.
     */
    private static class CaseVariantsOnDemand {

        // See https://en.wikipedia.org/wiki/Initialization-on-demand_holder_idiom

        // Use one hashmap for characters with a single case variant, another for characters with multiple
        // case variants, to reduce the number of objects that need to be allocated
        // FIX: Make these volatile
        private final IntToIntMap monoVariants = new IntToIntHashMap(2500);
        private final IntHashMap<int[]> polyVariants = new IntHashMap<>(100);

        private CaseVariantsOnDemand() {
            build();
        }

        private static class Holder {
            static final CaseVariantsOnDemand INSTANCE = new CaseVariantsOnDemand();
        }

        private static CaseVariantsOnDemand getInstance() {
            return Holder.INSTANCE;
        }

        private void build() {

//            // FIX: Use temporary local variables


            InputStream in = Version.platform.locateResource("casevariants.xml", new ArrayList<>());
            if (in == null) {
                throw new RuntimeException("Unable to read casevariants.xml file");
            }

            Configuration config = new Configuration();
            ParseOptions options = new ParseOptions();
            options = options.withSchemaValidationMode(Validation.SKIP);
            options = options.withDTDValidationMode(Validation.SKIP);
            NodeInfo doc;
            try {
                doc = config.buildDocumentTree(new StreamSource(in, "casevariants.xml"), options).getRootNode();
            } catch (XPathException e) {
                throw new RuntimeException("Failed to build casevariants.xml", e);
            }

            AxisIterator iter = doc.iterateAxis(AxisInfo.DESCENDANT, new NameTest(Type.ELEMENT, NamespaceUri.NULL, "c", config.getNamePool()));
            while (true) {
                NodeInfo item = iter.next();
                if (item == null) {
                    break;
                }
                String code = item.getAttributeValue("", "n");
                int icode = Integer.parseInt(code, 16);
                String variants = item.getAttributeValue("", "v");
                String[] vhex = variants.split(",");
                int[] vint = new int[vhex.length];
                for (int i = 0; i < vhex.length; i++) {
                    vint[i] = Integer.parseInt(vhex[i], 16);
                }
                if (vhex.length == 1) {
                    monoVariants.put(icode, vint[0]);
                } else {
                    polyVariants.put(icode, vint);
                }
            }
//            // FIX: Set static class variables at the end
//            CaseVariantsModified.polyVariants = polyVariants;
//            // Must set this last as other threads will be checking this value
//            CaseVariantsModified.monoVariants = monoVariants;
        }

        /**
         * Get the case variants of a character
         *
         * @param code the character whose case variants are required
         * @return the case variants of the character, excluding the character itself
         */

        // FIX: Method no longer synchronized
        public static int[] getCaseVariants(int code) {
            // FIX: Use local variables to avoid additional volatile reads
            IntToIntMap monoVariants = getInstance().monoVariants;
            //if (monoVariants == null) {
//            synchronized(CaseVariantsModified.class) {
//                monoVariants = CaseVariantsModified.monoVariants;
//                if (monoVariants == null) {
//                    build();
//                    monoVariants = CaseVariantsModified.monoVariants;
//                }
//            }
            //}
            int mono = monoVariants.get(code);
            if (mono != monoVariants.getDefaultValue()) {
                return new int[]{mono};
            } else {
                int[] result = getInstance().polyVariants.get(code);
                if (result == null) {
                    return IntArraySet.EMPTY_INT_ARRAY;
                } else {
                    return result;
                }
            }
        }

        /**
         * Get the case variants of roman letters (A-Z, a-z), other than the letters A-Z and a-z themselves
         */

        /*@NotNull*/ public static int[] ROMAN_VARIANTS = {0x0130, 0x0131, 0x212A, 0x017F};
    }
    }



