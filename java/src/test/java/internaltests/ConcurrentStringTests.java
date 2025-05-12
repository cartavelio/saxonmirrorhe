////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package internaltests;

import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.Test;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Future;

/**
 * Test aspects of Saxon Unicode handling, especially surrogate pairs
 */
public class ConcurrentStringTests {


        private static final int AS_STRING_TASKS_COUNT = 50000;
        private static final int GET_STRING_LENGTH_TASKS_COUNT = 50000;
        private static final int TASKS_COUNT = AS_STRING_TASKS_COUNT + GET_STRING_LENGTH_TASKS_COUNT;
        private static final int THREADS = 128;
        private static final int NUMBER_OF_ITERATIONS = 5;


    @Test
        public void testThreadSafe()
                throws InterruptedException, ExecutionException {
            for (int i = 0; i < NUMBER_OF_ITERATIONS; i++) {
                System.out.println("Iteration = " + i);
                runTheTest();
                System.out.println("done.");
            }
        }

        private void runTheTest() throws InterruptedException, ExecutionException {
            ExecutorService executorService = java.util.concurrent.Executors.newFixedThreadPool(THREADS);

            String input = "Domain";

            List<Future<Boolean>> results = executorService.invokeAll(createTasks(input));

            verifyResults(results);
            executorService.shutdown();
        }

        private List<Callable<Boolean>> createTasks(final String input) {
            StringValue stringValue = StringValue.makeStringValue(input);

            List<Callable<Boolean>> tasks = new ArrayList<Callable<Boolean>>();
            for (int i = 0; i < TASKS_COUNT; i++) {
                //tasks.add(new AsStringCallable(stringValue));
                tasks.add(new GetStringLengthCallable(stringValue));
            }
            return tasks;
        }

        // FIXME: what should this be now?
        private void verifyResults(List<Future<Boolean>> results) throws ExecutionException, InterruptedException {
            for (int i = 0; i < TASKS_COUNT; i++) {
                Future<Boolean> future = results.get(i);
                Boolean result = future.get();
                // org.junit.Assert.assertThat(result, CoreMatchers.equalTo(true));
            }
        }

        private static class GetStringLengthCallable implements Callable<Boolean> {
            private final StringValue stringValue;

            public GetStringLengthCallable(StringValue stringValue) {
                this.stringValue = stringValue;
            }

            @Override
            public Boolean call() throws Exception {
                try {
                    stringValue.getLength();
                } catch (Exception e) {
                    System.out.println("Error has been reproduced for getStringLength! " + e.getMessage());
                    e.printStackTrace();
                    return false;
                }
                return true;
            }
        }

//        private static class AsStringCallable implements Callable<Boolean> {
//            private final StringValue stringValue;
//
//            public AsStringCallable(StringValue stringValue) {
//                this.stringValue = stringValue;
//            }
//
//            public Boolean call() {
//                try {
//                    stringValue.asString();
//                } catch (Exception e) {
//                    System.out.println("Error has been reproduced for asString! " + e.getMessage());
//                    e.printStackTrace();
//                    return false;
//                }
//                return true;
//            }
//        }



}

