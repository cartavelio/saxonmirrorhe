package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.optim.StaticQueryContextEE;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.query.StaticQueryContext;

import java.io.File;
import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.Collection;


/**
 * Loads xquery module info by parsing the module text
 */
public class SumanGantaTest extends TestCase
{
   private int mIterations = 2000;

   public void testModuleLink()
   {
      parseWithModuleLinking(false);
   }

   public void testParse()
   {
      justParse(false);
   }

   /**
    * Parse and execute an expression with linked modules
    */
   public void justParse(boolean aSequential)
   {
      EnterpriseConfiguration mConfig;
      StaticQueryContextEE mStaticContext;

      try
      {
         String expression = getTestExpression();
         mConfig = new EnterpriseConfiguration();
         mStaticContext = new StaticQueryContextEE(mConfig);
         mStaticContext.setBaseURI(ConfigTest.DATA_DIR + "ganta/");

         parse(mStaticContext, expression, "justParse", mIterations, aSequential);
      }
      catch (Exception e)
      {
         e.printStackTrace();
         fail();
      }
   }

   /**
    * Parse and execute an expression with linked modules
    */
   public void parseWithModuleLinking(boolean aSequential)
   {
      EnterpriseConfiguration mConfig;
      StaticQueryContextEE mStaticContext;

      try
      {
         String expression = getTestExpressionSansLoc();
         mConfig = new EnterpriseConfiguration();
         mConfig.setConfigurationProperty(FeatureKeys.DEBUG_BYTE_CODE, true);
         mStaticContext = new StaticQueryContextEE(mConfig);

         Collection<String> moduleFiles = new ArrayList<String>();

         moduleFiles.add(ConfigTest.DATA_DIR + "ganta/genBPEL.xq");
         moduleFiles.add(ConfigTest.DATA_DIR + "ganta/hello_notworking.xq");
         moduleFiles.add(ConfigTest.DATA_DIR + "ganta/avoscentral.xq");
         addLibs(mStaticContext, moduleFiles);

         parse(mStaticContext, expression, "justParse", mIterations, aSequential);
      }
      catch (Exception e)
      {
         e.printStackTrace();
         fail();
      }
   }

   public void parse(final StaticQueryContextEE aStaticContext, final String aExpression, String aTestName, int aTimes, boolean aSequential)
   {
      try
      {
         long start = System.currentTimeMillis();
         Runnable task = new Runnable()
         {
            public void run()
            {
               try
               {
                  aStaticContext.compileQuery(aExpression);
               }
               catch (Exception e)
               {
                  e.printStackTrace();
                  fail();
               }
            }
         };
         if(aSequential)
            executeSequential(task, aTimes);
         else
            executeConcurrent(task, aTimes);
         report(aTestName, System.currentTimeMillis() - start);
      }
      catch (Exception e)
      {
         e.printStackTrace();
         fail();
      }
   }

   private void report(String aTestName, long aTotal)
   {
      long avg = aTotal/mIterations;
      System.out.println(aTestName + ":" + aTotal + "ms (total), " + mIterations + " (iterations), "+ avg + "ms (avg)");
   }

   private long executeConcurrent(Runnable aTask, int aTimes) throws Exception
   {
      long start = System.currentTimeMillis();
      ArrayList<Thread> threads = new ArrayList<Thread>();
      for(int i = 0; i < aTimes; ++i)
      {
         Thread t = new Thread(aTask);
         t.start();
         threads.add(t);
      }

      for(Thread t : threads)
         t.join();
      return System.currentTimeMillis() - start;
   }

   private long executeSequential(Runnable aTask, int aTimes) throws Exception
   {
      long start = System.currentTimeMillis();
      for(int i = 0; i < aTimes; ++i)
      {
         aTask.run();
      }
      return System.currentTimeMillis() - start;
   }

   private String getTestExpressionSansLoc()
   {
      String module1 = "import module namespace myf = 'urn:active-endpoints:bpep:function';\n";
      String module2 = "import module namespace avcf = 'urn:active-endpoints:avc:xquery:function';\n";
      String module3 = "import module namespace avsf = 'urn:activevos:avsf:xquery:function';\n";
      String expression = module1 + module2 + module3 + "myf:sayHello('Hello')";
      return expression;
   }

   private String getTestExpression()
   {
      String module1 = "import module namespace myf = 'urn:active-endpoints:bpep:function' at 'hello.xq';\n";
      String module2 = "import module namespace avcf = 'urn:active-endpoints:avc:xquery:function' at 'avoscentral.xq';\n";
      String module3 = "import module namespace avsf = 'urn:activevos:avsf:xquery:function' at 'genBPEL.xq';\n";
      String expression = module1 + module2 + module3 + "myf:sayHello('Hello')";
      return expression;
   }

   private void addLibs(StaticQueryContext aStaticContext, Collection<String> aModuleFiles)
   {
      try
      {
         for(String file : aModuleFiles)
         {
            aStaticContext.setBaseURI(new File(file).toURI().toString());
            aStaticContext.compileLibrary(new FileInputStream(file), null);
         }
      }
      catch(Exception e)
      {
         e.printStackTrace();
         fail();
      }
   }
}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



