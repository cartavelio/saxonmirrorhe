using System;
using Saxon.Hej;
using Saxon.Hej.expr.instruct;
using Saxon.Impl.Overrides;
using Version = System.Version;

namespace Saxon.Cmd {
    public static class Command {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                Console.Error.WriteLine("Subcommand missing (query, transform, validate, help, ...)");
            }
            else {
                string subCommand = args[0];
                string[] tail = new string[args.Length - 1];
                Array.Copy(args, 1, tail, 0, tail.Length);
                if (subCommand == "query") {
                    Saxon.Hej.Query.main(tail);
                } else if (subCommand == "transform") {
                    Saxon.Hej.Transform.main(tail);
                } else if (subCommand == "validate") {
#if EE                    
                    Saxon.Eej.Validate.main(tail);
#endif
                } else if (subCommand == "version") {
                    Saxon.Hej.Version.main(tail);
                } else if (subCommand == "help") {
                    Console.Error.WriteLine("saxon (query|transform|validate|version) -?");
                // } else if (subCommand == "verify") {
                //     Verifier.main(args);
                }
                else {
                    Console.Error.WriteLine("Unknown subcommand: " + subCommand + " (use query, transform, validate, help, ...)");
                }
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited