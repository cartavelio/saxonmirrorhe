using System;
using System.Collections.Generic;
using System.IO;
using Saxon.Api;
using System.Diagnostics;
using Saxon.Hej;

namespace SaxonCS_NUnit
{
    class TestTimingPerformance
    {

        public static TimeSpan Time(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        
        

        //[Test]
        // public void TestPerformance()
        // {
        //    // _processor.SetProperty("http://saxon.sf.net/feature/debugByteCode", "true");
        //     _processor.SetProperty("http://saxon.sf.net/feature/generateByteCode", "true");
        //     string sourcePath = Path.GetFullPath("C:\\work\\misc\\xmark\\xmark10.xml");
        //     string stylesheetPath = Path.GetFullPath("C:\\work\\misc\\xmark\\q12.xsl");
        //     string outputPath = Path.ChangeExtension(sourcePath, ".out.xml");
        //     Stopwatch stopwatch = Stopwatch.StartNew();
        //     RunSaxonApi(sourcePath, stylesheetPath, outputPath);
        //     stopwatch.Stop();
        //     TimeSpan apiTime  = stopwatch.Elapsed;
        //
        //     stopwatch = Stopwatch.StartNew();
        //     RunTransformCommand(sourcePath, stylesheetPath, outputPath);
        //     stopwatch.Stop();
        //     TimeSpan transTime =  stopwatch.Elapsed;
        //     RunSaxonApi2(sourcePath, stylesheetPath /*"C:\\work\\tests\\XSL\\Master\\Master.xsl"*/, outputPath);
        //     Console.WriteLine("Time to execute Saxon Api: " + apiTime.Milliseconds + "ms (" + apiTime.Seconds + " sec) and Elapsed:" + apiTime);
        //
        //     Console.WriteLine("Time to execute Saxon command: " + transTime.Milliseconds + "ms (" + transTime.Seconds + " sec) and Elapsed:" + transTime);
        //     Assert.True(true, "******************XXXXXXXXXXXXXXX Time to execute Saxon Api: " + apiTime.Milliseconds + "ms (" + apiTime.Seconds + " sec) and Elapsed:" + apiTime + "\n" + "Time to execute Saxon command: " + transTime.Milliseconds + "ms (" + transTime.Seconds + " sec) and Elapsed:" + transTime);
        //
        // }

        private readonly Processor _processor = new Processor(true);

        public void RunSaxonApi(string sourcePath, string stylesheetPath, string outputPath)
        {

            using (var source = File.OpenRead(sourcePath))
            using (var stylesheet = File.OpenRead(stylesheetPath))
            {
                Uri stylesheetUri, sourceUri;
                if (!Uri.TryCreate(sourcePath, UriKind.RelativeOrAbsolute, out sourceUri))
                    throw new ArgumentException(string.Format("Could not create an URI from source path '{0}'.", sourcePath), "sourcePath");
                if (!Uri.TryCreate(stylesheetPath, UriKind.RelativeOrAbsolute, out stylesheetUri))
                    throw new ArgumentException(string.Format("Could not create an URI from stylesheet path '{0}'.", stylesheetPath), "stylesheetPath");

                var compiler = _processor.NewXsltCompiler();

                compiler.BaseUri = stylesheetUri; //required to resolve stylesheet includes
                List<Error> errorList = new();
                compiler.ErrorReporter = error => errorList.Add(error);
                var executable = compiler.Compile(stylesheet);
                var transformer = executable.Load();
                var builder = _processor.NewDocumentBuilder();
                //	builder.XmlResolver = new DtdIgnoringResolver(builder.XmlResolver);
                //required to ignore DTDs in System IDs
                builder.BaseUri = sourceUri;
                transformer.InitialContextNode = builder.Build(source);
                var destination = _processor.NewSerializer();
                using (var output = File.Open(outputPath, FileMode.Create))
                {
                    destination.OutputStream = output;
                    transformer.Run(destination);
                }
            }
        }


        // public void RunSaxonApi2(string sourcePath, string stylesheetPath, string outputPath)
        // {
        //
        //     using (var source = File.OpenRead(sourcePath))
        //     using (var stylesheet = File.OpenRead(stylesheetPath))
        //     {
        //         Uri stylesheetUri, sourceUri;
        //         if (!Uri.TryCreate(sourcePath, UriKind.RelativeOrAbsolute, out sourceUri))
        //             throw new ArgumentException(string.Format("Could not create an URI from source path '{0}'.", sourcePath), "sourcePath");
        //         if (!Uri.TryCreate(stylesheetPath, UriKind.RelativeOrAbsolute, out stylesheetUri))
        //             throw new ArgumentException(string.Format("Could not create an URI from stylesheet path '{0}'.", stylesheetPath), "stylesheetPath");
        //
        //         var compiler = _processor.NewXsltCompiler();
        //
        //         compiler.BaseUri = stylesheetUri; //required to resolve stylesheet includes
        //         List<Error> errorList = new();
        //         compiler.ErrorReporter = errorList.Add;
        //          ConcurrentDictionary<Uri, XsltExecutable> _executables = new ConcurrentDictionary<Uri, XsltExecutable>();
        //         XsltExecutable executable = null;// compiler.Compile(stylesheet);
        //         try {
        //             executable = _executables.GetOrAdd(new Uri(stylesheetPath), uri => compiler.Compile(uri));
        //         }
        //         catch (java.lang.ClassNotFoundException e) {
        //             e.printStackTrace();
        //             Console.WriteLine();
        //             throw e;
        //         }
        //         catch (java.io.FileNotFoundException e)
        //         {
        //             e.printStackTrace();
        //             Console.WriteLine();
        //             throw e;
        //         }
        //         var transformer = executable.Load();
        //         var builder = _processor.NewDocumentBuilder();
        //         //	builder.XmlResolver = new DtdIgnoringResolver(builder.XmlResolver);
        //         //required to ignore DTDs in System IDs
        //         builder.BaseUri = sourceUri;
        //         transformer.InitialContextNode = builder.Build(source);
        //         var destination = _processor.NewSerializer();
        //         using (var output = File.Open(outputPath, FileMode.Create))
        //         {
        //             destination.SetOutputStream(output);
        //             transformer.Run(destination);
        //         }
        //     }
        // }



        public void RunTransformCommand(string sourcePath, string stylesheetPath, string outputPath)
        {
            Transform transform = new ();
            transform.doTransform(new[] { "-s:" + stylesheetPath, "-xsl:" + stylesheetPath, "-o:" + outputPath });
        }

        static void Main1(string[] args)
        {
            args = new[] { "C:\\work\\misc\\xmark\\xmark10.xml", "C:\\work\\misc\\xmark\\q12.xsl", "C:\\work\\misc\\xmark\\outputvs.xml" };
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage: SaxonTransform input stylesheet[output]");
                return;
            }
            try
            {
                string sourcePath = Path.GetFullPath(args[0]);
                string stylesheetPath = Path.GetFullPath(args[1]);
                string outputPath = args.Length == 3 ? Path.GetFullPath(args[2]) : Path.ChangeExtension(sourcePath, ".out.xml");
                TestTimingPerformance prog = new TestTimingPerformance();
                Console.WriteLine("Starting transform at {0}", DateTime.Now);
                var sw = Stopwatch.StartNew();
                //TransformDirect(sourcePath, stylesheetPath, outputPath);
                prog.RunTransformCommand(sourcePath, stylesheetPath, outputPath);
                sw.Stop();
                Console.WriteLine("Finished transform at {0} (after {1})", DateTime.Now, sw.Elapsed);
                Console.WriteLine();
                Console.WriteLine("===========================");
                Console.WriteLine("Starting Saxon.Api at {0}", DateTime.Now);
                sw = Stopwatch.StartNew();
                prog.RunSaxonApi(sourcePath, stylesheetPath, outputPath);
                sw.Stop();
                Console.WriteLine("Finished transform at {0} (after {1})", DateTime.Now, sw.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception: {0}", ex);
            }
        }
    }
}
