using System;
using System.Collections.Generic;
using System.Linq;
using Saxon.Api;
using NUnit.Framework;
using System.Diagnostics;
using System.Xml;
using System.Net;
using System.IO;

namespace SaxonNUnit
{
    class TestCollection
    {
        [Test]
        public void TestDirectoryNoRecurse()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("collection(.)!document-uri(.)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "trax/xml/?select=*.xml"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(5, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDirectoryMatch()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = "3.1";
                c.DeclareVariable(new QName("a"));
                c.DeclareVariable(new QName("b"));
                XPathExecutable exec = c.Compile("collection($a||'trax/xml/?match='||encode-for-uri($b))!document-uri(.)");
                XPathSelector eval = exec.Load();
                eval.SetVariable(new QName("a"), new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR));
                eval.SetVariable(new QName("b"), new XdmAtomicValue("[A-Za-z]+\\.xml"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(5, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDirectoryNoRecurse30()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.0");
                XPathExecutable exec = c.Compile("collection(.)!document-uri(.)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "trax/xml/?select=*.xml"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(5, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDirectoryRecurse()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("collection(.)!document-uri(.)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "trax/xml/?select=*.xml;recurse=yes"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(7, val.Count, "found7");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDirectoryWithContentType()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("collection(.)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "trax/xml/?select=*.xml;recurse=yes;content-type=text/plain"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(7, val.Count, "found7");
                Assert.True(val.ItemAt(0) is XdmAtomicValue, "isString");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDirectoryUriNoRecurse()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("uri-collection(.)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue(ConfigTest.DATA_DIR2 + "trax/xml/?select=*.xml"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(5, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDirectoryUriRecurse()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("uri-collection(.)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue(ConfigTest.DATA_DIR2 + "trax/xml/?select=*.xml;recurse=yes"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(7, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestParallelSpeed()
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    speedTest(true);
                    speedTest(false);
                }
                TimeSpan serial = speedTest(false);
                Console.WriteLine("Serial processing (HE): " + serial.Milliseconds + "ms");
                TimeSpan parallel = speedTest(true);
                Console.WriteLine("Parallel processing (EE): " + parallel.Milliseconds + "ms");
                Assert.True(parallel < serial, "not faster!");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private TimeSpan speedTest(bool parallel)
        {
            Processor p = new Processor(parallel);
            Stopwatch stopwatch = Stopwatch.StartNew();
            TimeSpan startTime = stopwatch.Elapsed;

            XPathCompiler c = p.NewXPathCompiler();
            XPathExecutable exec = c.Compile("sum(collection(.)!count(//*))");
            XPathSelector eval = exec.Load();
            eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "collection/?select=*.xml;recurse=yes"));
            XdmValue val = eval.Evaluate();
            stopwatch.Stop();
            TimeSpan apiTime = stopwatch.Elapsed;
            Console.WriteLine("Found " + val + " nodes");
            return apiTime;
        }

        [Test]
        public void TestStable()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("count(collection(.)//* | collection(.)//*)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "collection/?select=*.xml;recurse=yes;stable=yes"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(10680, ((XdmAtomicValue)val).GetLongValue());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestUnstable()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("count(collection(.)//* | collection(.)//*)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "collection/?select=*.xml;recurse=yes;stable=no"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(21360, ((XdmAtomicValue)val).GetLongValue());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestStrictValidation()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("collection(.)/* instance of element(*, xs:untyped)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "?select=books.xml;recurse=no;validation=strict"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(false, ((XdmAtomicValue)val).GetBooleanValue());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestNoValidation()
        {
            try
            {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable exec = c.Compile("collection(.)/* instance of element(*, xs:untyped)");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "?select=books.xml;recurse=no;validation=skip"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(true, ((XdmAtomicValue)val).GetBooleanValue());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestJSONCollection()
        {
            try
            {
                Processor p = new Processor(true);
                p.SetProperty(Feature<bool>.ALLOW_MULTITHREADING, false);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = "3.1";
                XPathExecutable exec = c.Compile("for $X in (collection(.))" +
                    "return (if ($X instance of node()) then name($X/*) else $X('id'))");
                XPathSelector eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file:///" + ConfigTest.DATA_DIR + "collection.xml"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual("1.0e0\n\"BOOKLIST\"", val.ToString());
            }
            catch (Exception e)
            {

                Assert.Fail(e.Message);
            }
        }

        /*
    [Test]
    public void TestCustomMediaType()
    {
        try
        {
            Processor p = new Processor(true);
            Configuration config = p.Implementation;
            config.registerFileExtension("json", "personal/plain");
            config.registerMediaType("personal/plain", new ResourceFactory()
            {
                public Resource makeResource(Configuration config, AbstractResourceCollection.InputDetails details) throws XPathException {
                return new JSONResource(details);
            }
        });
        XPathCompiler c = p.NewXPathCompiler();
        c.XPathLanguageVersion = "3.1";
        XPathExecutable exec = c.Compile("for $X in (collection(.))" +
                                                 "return (if ($X instance of node()) then name($X/*) else $X('id'))");
        XPathSelector eval = exec.Load();
        eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection.xml"));
        XdmValue val = eval.Evaluate();
        Assert.AreEqual("1.0e0\n\"BOOKLIST\"", val.ToString());
    } catch (Exception e) {
           
            Assert.Fail(e.Message);
}
    }
    */

        /*  [Test]
          public void TestMixedCollection()
      {
          try
          {
              Processor p = new Processor(true);
              XPathCompiler c = p.NewXPathCompiler();
              c.XPathLanguageVersion = "3.1";
              XPathExecutable exec = c.Compile("count(collection(.))");
              XPathSelector eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection2/?select=*"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(2, Integer.parseInt(val.ToString()));
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestMixedCollectionWithMetadata()
      {
          try
          {
              Processor p = new Processor(true);
              XPathCompiler c = p.NewXPathCompiler();
              c.XPathLanguageVersion = "3.1";
              XPathExecutable exec = c.Compile("collection(.)");
              XPathSelector eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection2/?select=*.xml;metadata=yes"));
              XdmValue val = eval.Evaluate();
              for (XdmItem item : val)
              {
                  Assert.True(item is XdmFunctionItem);
                  XdmFunctionItem map = (XdmFunctionItem)item;
                  XdmAtomicValue name = (XdmAtomicValue)map.call(p, new XdmAtomicValue("canonical-path"));
                  XdmAtomicValue length = (XdmAtomicValue)map.call(p, new XdmAtomicValue("length"));
                  XdmAtomicValue lastModified = (XdmAtomicValue)map.call(p, new XdmAtomicValue("last-modified"));
                  XdmAtomicValue contentType = (XdmAtomicValue)map.call(p, new XdmAtomicValue("content-type"));
                  Assert.AreEqual("application/xml", contentType.ToString());
                  Console.WriteLine(name + " " + lastModified + " " + length + " " + contentType);
              }
              Assert.AreEqual(1, val.Count);
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestMixedCollectionMetadataFetch()
      {
          try
          {
              Processor p = new Processor(true);
              XPathCompiler c = p.NewXPathCompiler();
              c.XPathLanguageVersion = "3.1";
              String query = "for $m in collection(.||'?metadata=yes') " +
                             "return if ($m?content-type='application/xml') then $m?fetch() else ()";
              XPathExecutable exec = c.Compile(query);
              XPathSelector eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection2"));
              XdmValue val = eval.Evaluate();
              foreach (XdmItem item in val)
              {
                  Assert.True(item is XdmNode);
              }
              Assert.AreEqual(1, val.Count);
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }


      [Test]
          public void TestCollectionWithHttp()
      {
          try
          {
              Processor p = new Processor(true);
              XPathCompiler c = p.NewXPathCompiler();
              c.XPathLanguageVersion = "3.1";
              XPathExecutable exec = c.Compile("count(collection(.))");
              XPathSelector eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("https://dev.saxonica.com/tests/collection/catalog.xml"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(2, Integer.parseInt(val.ToString()));
          }
          catch (Exception e)
          {

              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestMixedWithHttpCollection()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("for $X in (collection(.))" +
                  "return if ($X instance of node()) then name($X/*) else $X('id')");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection2.xml"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual("1.0e0\n\"html\"", val.ToString());
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestMixedWithHttpCollection2()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("count(collection(.))");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3.xml"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(3, Integer.parseInt(val.ToString()));
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarCollection()
      {
          try
          {
              Processor p = new Processor(true);
              XPathCompiler c = p.NewXPathCompiler();
              c.XPathLanguageVersion = "3.1";
              XPathExecutable exec = c.Compile("count(collection(.))");
              XPathSelector eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9-test.jar"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(34, Integer.parseInt(val.ToString()));
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarCollection2()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("for $c at $p in collection(.) return ($p || ' ' || local-name-from-QName(saxon:type-annotation($c)))");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9-test.jar"));
              XdmValue val = eval.Evaluate();
              Assert.True(val.ToString().contains("34 base64Binary"));
          }
          catch (Exception e)
          {

              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarCollectionUri()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("for $x at $p in uri-collection(.) return ($p || ' ' || xs:string($x))");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9-test.jar"));
              XdmValue val = eval.Evaluate();
              System.out.println(val.ToString());
              Assert.True(true, "Uri retrieved successfully: ");
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarCollectionUriWithOptions()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("for $x in uri-collection(.) return switch(substring-after(xs:string($x), '!/META-INF/'))" +
                  "case 'MANIFEST.MF' return true()" +
                  "case 'SAXONICA.SF' return true()" +
                  "case 'SAXONICA.RSA' return true()" +
                  "default return false()");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9-test.jar?select=META-INF/*"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(true, ((XdmAtomicValue)val.itemAt(1)).GetBooleanValue());
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarCollectionUriWithXmlOptions()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("for $x in collection(.) return " +
                  "if($x instance of node()) then true() else false()");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9he.jar?select=*.xsl;validation=skip"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(true, ((XdmAtomicValue)val.itemAt(1)).GetBooleanValue());
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarCollectionUriWithXmlOptions2()
      {
          try
          {
              Processor p = new Processor(true);
              XQueryCompiler c = p.newXQueryCompiler();
              XQueryExecutable exec = c.Compile("for $x in collection(.) return " +
                                                        "if($x instance of node()) then true() else false()");
              XQueryEvaluator eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9he.jar?select=*.xsl;validation=skip;content-type=text/plain"));
              XdmValue val = eval.Evaluate();
              Assert.AreEqual(false, ((XdmAtomicValue)val.itemAt(1)).GetBooleanValue());
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }

      [Test]
          public void TestJarMetadataCollection()
      {
          try
          {
              Processor p = new Processor(true);
              XPathCompiler c = p.NewXPathCompiler();
              c.XPathLanguageVersion = "3.1";
              XPathExecutable exec = c.Compile("collection(.)");
              XPathSelector eval = exec.Load();
              eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "collection3/saxon9-test.jar?metadata=yes"));
              XdmValue val = eval.Evaluate();
              for (XdmItem item : val)
              {
                  XdmFunctionItem map = (XdmFunctionItem)item;
                  Assert.True(map.call(p, new XdmAtomicValue("entry-name")) != null);
                  Assert.True(map.call(p, new XdmAtomicValue("last-modified")).getUnderlyingValue() is DateTimeValue);
              }

              Assert.AreEqual(34, val.Count);
          }
          catch (Exception e)
          {
              Assert.Fail(e.Message);
          }
      }*/

        class MyCollectionFinder : ICollectionFinder
        {

            Processor proc;
            public MyCollectionFinder(Processor proc)
            {
                this.proc = proc;
            }
            public IResourceCollection FindCollection(DynamicContext context, String collectionURI)
            {
                return new DirectoryCollection(proc, "http://example.com/collection", ConfigTest.DATA_DIR + "trax/", new URIQueryParameters("select=*.xml;recurse=yes", proc));
            }

            public bool IsStable()
            {
                return true;
            }
        }

        class MyCollection : IResourceCollection
        {
            public string CollectionURI()
            {
                throw new NotImplementedException();
            }

            public List<IResource> GetResources(DynamicContext context)
            {
                throw new NotImplementedException();
            }

            public List<string> GetResourceURIs(DynamicContext context)
            {
                throw new NotImplementedException();
            }
        }


        class MyCollectionFinder2 : ICollectionFinder
        {

            Processor proc;
            public MyCollectionFinder2(Processor proc)
            {
                this.proc = proc;
            }
            public IResourceCollection FindCollection(DynamicContext context, String collectionURI)
            {


                
                return new DirectoryCollection(proc, "http://example.com/collection", ConfigTest.DATA_DIR + "collection2/", new URIQueryParameters("select=*.xml;recurse=yes", proc));
            }

            public bool IsStable()
            {
                return true;
            }
        }



        [Test]
        public void TestCollectionFinder()
        {
            try
            {
                Processor p = new Processor(true);
                p.CollectionFinder = new StandardCollectionFinder();//new MyCollectionFinder(p);
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryExecutable exec = c.Compile("for $x in collection(.) return " +
                    "if($x instance of node()) then true() else false()");
                XQueryEvaluator eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("http://example.com/collection"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(7, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        public class DtdIgnoringResolver : XmlResolver
        {
            private readonly XmlResolver _innerResolver;

            public DtdIgnoringResolver(XmlResolver innerResolver)
            {
                _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));
            }

            public override ICredentials Credentials
            {
                set { _innerResolver.Credentials = value; }
            }

            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                if (!string.IsNullOrEmpty(absoluteUri?.OriginalString) && IsDtdOrSchema(absoluteUri.OriginalString))
                    return Stream.Null;
                return _innerResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }

            private static bool IsDtdOrSchema(string filePath)
            {
                return
                    filePath.EndsWith(".dtd", StringComparison.InvariantCultureIgnoreCase) ||
                    filePath.EndsWith(".xsd", StringComparison.InvariantCultureIgnoreCase);
            }
        }


        [Test]
        public void TestCollectionFinder2()
        {
            try
            {
                Processor p = new Processor(true);
                XsltCompiler compiler = p.NewXsltCompiler();
                var builder = p.NewDocumentBuilder();
                compiler.XmlResolver = new DtdIgnoringResolver(builder.XmlResolver);
               // p.CollectionFinder = new MyCollectionFinder2(p);

                Xslt30Transformer transformer = compiler.Compile(
                   new Uri(ConfigTest.DATA_DIR + "collection4.xsl")).Load30();
                
                //fails to work with a XmlTextReader
                // Create a serializer
                StringWriter sw = new StringWriter();
                Serializer serializer = p.NewSerializer(sw);
                //transformer.InitialTemplate = new QName("main");

                transformer.InputXmlResolver = new DtdIgnoringResolver(builder.XmlResolver); ;
                transformer.CallTemplate(new QName("main"), serializer);
                //transformer.Run(serializer);

                Console.WriteLine(sw.ToString());
                Assert.True(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        /*

            [Test]
            public void TestRegisterMediaType()
        {
            try
            {
                Processor p = new Processor(true);
                final Configuration config = p.Implementation;
                config.registerFileExtension("java", "text/plain");
                config.registerFileExtension("xq", "text/plain");
                XQueryCompiler c = p.newXQueryCompiler();
                XQueryExecutable exec = c.Compile("for $x in collection(.) return " +
                                                          "if($x instance of xs:string) then true() else false()");
                XQueryEvaluator eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "ganta"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(6, val.Count);
                for (XdmItem item : val)
                {
                    Assert.True(((XdmAtomicValue)item).GetBooleanValue(), "isText");
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
            public void TestMediaTypeConfigFile()
        {
            try
            {
                String cf = "<configuration xmlns='http://saxon.sf.net/ns/configuration'\n" +
                        "               edition='EE'\n" +
                        "               label='Some label'>\n" +
                        "          <resources>\n" +
                        "            <fileExtension extension='xq' mediaType='text/plain'/>" +
                        "            <fileExtension extension='java' mediaType='text/plain'/>" +
                        "          </resources></configuration>";
                Processor p = new Processor(new StreamSource(new StringReader(cf)));
                XQueryCompiler c = p.newXQueryCompiler();
                XQueryExecutable exec = c.Compile("for $x in collection(.) return " +
                                                          "if($x instance of xs:string) then true() else false()");
                XQueryEvaluator eval = exec.Load();
                eval.ContextItem = (new XdmAtomicValue("file://" + ConfigTest.DATA_DIR + "ganta"));
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(6, val.Count);
                for (XdmItem item : val)
                {
                    Assert.True(((XdmAtomicValue)item).GetBooleanValue(), "isText");
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
            public void TestRegisteredCollection()
        {
            try
            {
                Processor p = new Processor(true);
                Configuration config = p.Implementation;
                List<File> contents = new ArrayList<>();
                contents.add(new File(ConfigTest.DATA_DIR + "trax/xml/cities.xml"));
                contents.add(new File(ConfigTest.DATA_DIR + "trax/xml/baz.xml"));
                config.registerCollection("ftp://myCollection", new AbstractResourceCollection(config) {
                        @Override
                        public Iterator<String> getResourceURIs(XPathContext context) throws XPathException {
                    return contents.stream().map(File::toURI).map(URI::toString).iterator();
                }

                @Override
                        public Iterator<? extends Resource> getResources(XPathContext context) throws XPathException {
                    return contents.stream().map(file-> {
                        try
                        {
                            StreamSource ss = new StreamSource(file);
                            NodeInfo doc = config.buildDocumentTree(ss).getRootNode();
                            return new XmlResource(config, doc);
                        }
                        catch (XPathException e)
                        {
                            throw new UncheckedXPathException(e);
                        }
                    }).iterator();
                }
            });
            XPathCompiler c = p.NewXPathCompiler();
            c.XPathLanguageVersion = "3.1";
            XPathExecutable exec = c.Compile("count(uri-collection(.))");
            XPathSelector eval = exec.Load();
            eval.ContextItem = (new XdmAtomicValue("ftp://myCollection"));
            XdmValue val = eval.Evaluate();
            Assert.AreEqual(2, Integer.parseInt(val.ToString()));
            exec = c.Compile("collection(.)/* ! name()");
            eval = exec.Load();
            eval.ContextItem = (new XdmAtomicValue("ftp://myCollection"));
            val = eval.Evaluate();
            Assert.AreEqual("cities", val.itemAt(0).getStringValue(), "one");
            Assert.AreEqual("foo:document", val.itemAt(1).getStringValue(), "two");
        } catch (Exception e) {
                    Assert.Fail(e.Message);
                }
            }

            public static class Finder implements CollectionFinder
        {
            @Override
                public ResourceCollection findCollection(XPathContext context, String collectionURI) throws XPathException
        {
                    if (collectionURI.equals("http://example.com/collection")) {
                File dir = new File(ConfigTest.DATA_DIR + "collection");
                URIQueryParameters params = new URIQueryParameters("select=*.xml", context.getConfiguration());
                return new DirectoryCollection(context.getConfiguration(), collectionURI, dir, params);
            } else {
                throw new XPathException("Collection " + collectionURI + " not found");
            }
        }
            }

            [Test]
            public void TestCollectionFinderConfigFile()
        {
            try
            {
                String cf = "<configuration xmlns='http://saxon.sf.net/ns/configuration'\n" +
                        "               edition='EE'\n" +
                        "               label='Some label'>\n" +
                        "          <global collectionFinder='s9apitest.TestCollections$Finder'/>\n" +
                        "    </configuration>";
                Processor p = new Processor(new StreamSource(new StringReader(cf)));
                XQueryCompiler c = p.newXQueryCompiler();
                XQueryExecutable exec = c.Compile("distinct-values(collection(\"http://example.com/collection\")!node-name(*))");
                XQueryEvaluator eval = exec.Load();
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(39, val.Count);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }*/
    }

}
