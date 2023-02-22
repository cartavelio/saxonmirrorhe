using System;
using net.sf.saxon;
using net.sf.saxon.expr.instruct;
using Saxon.Impl.Overrides;
using Version = System.Version;

namespace SaxonCS3.Saxon.Cmd {
    public static class Command {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                Console.Error.WriteLine("Subcommand missing (query, transform, help, ...)");
            }
            else {
                string subCommand = args[0];
                string[] tail = new string[args.Length - 1];
                Array.Copy(args, 1, tail, 0, tail.Length);
                if (subCommand == "query") {
                    net.sf.saxon.Query.main(tail);
                } else if (subCommand == "transform") {
                    net.sf.saxon.Transform.main(tail);
                } else if (subCommand == "validate") {
                    com.saxonica.Validate.main(tail);
                } else if (subCommand == "version") {
                    net.sf.saxon.Version.main(tail);
                } else if (subCommand == "help") {
                    Console.Error.WriteLine("saxon (query|transform|validate|version) -?");
                // } else if (subCommand == "verify") {
                //     Verifier.main(args);
                }
                else {
                    Console.Error.WriteLine("Unknown subcommand: " + subCommand);
                }
            }
        }
    }
}