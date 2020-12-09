using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace Saxon9NUnit
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
			string[] my_args = { "/run:SaxonNUnit.TestXsltTransformer.TestDotNetDOMSource", Assembly.GetExecutingAssembly().Location };

            int returnCode = 0;// new NUnitLite.AutoRun().Execute(my_args);//NUnit.ConsoleRunner.Runner.Main(my_args);

            if (returnCode != 0)
                Console.Beep();
        }
    }
}
