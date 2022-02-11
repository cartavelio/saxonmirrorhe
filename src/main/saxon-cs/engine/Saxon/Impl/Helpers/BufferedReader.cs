using System;
using System.IO;
using Saxon.Ejava.lang;
using Saxon.Ejava.util;
using Math = System.Math;
using StringBuilder = System.Text.StringBuilder;

namespace Saxon.Impl.Helpers {
    public class BufferedReader : System.IO.StreamReader {
        public BufferedReader(Stream input) : base(input) {
        }
    }
}