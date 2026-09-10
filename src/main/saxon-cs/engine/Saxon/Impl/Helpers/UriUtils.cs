using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using Saxon.Ejava.net;
using Saxon.Hej.str;
using Saxon.Hej.z;
using Sys = System;
namespace Saxon.Impl.Helpers {
    public class UriUtils {
        public static bool IsOpaque(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/').
            //Sys.Console.Error.WriteLine("Uri.ISOpaque " + uri + " local path " + uri.LocalPath);
            return uri.IsAbsoluteUri && !(uri.LocalPath.StartsWith("/") || uri.LocalPath.StartsWith("\\") || uri.IsFile || uri.IsUnc);
        }
        public static System.Uri Normalize(System.Uri uri) {
            return uri;
        }

        public static string GetQuery(System.Uri uri) {
            string q = uri.Query;
            if (q.StartsWith("?")) {
                q = q[1..];
            }
            if (q == "") {
                return null;
            } else {
                return System.Uri.UnescapeDataString(q);
            }
        }
        
        public static string GetFragment(System.Uri uri) {
            string q = uri.Fragment;
            if (q.StartsWith("#")) {
                q = q[1..];
            }
            if (q == "") {
                return null;
            } else {
                return System.Uri.UnescapeDataString(q);
            }
        }

        public static System.Uri Build(string scheme,
                string userInfo, string host, int port,
                string path, string query, string fragment) {
            Sys.UriBuilder builder = new Sys.UriBuilder(scheme, host, port, path);
            builder.UserName = userInfo;
            builder.Query = query;
            builder.Fragment = fragment;
            return builder.Uri;
        }
        
        public static Saxon.Ejava.net.URL ToURL(System.Uri uri) {
            return new URL(uri);
        }

        private static readonly Regex twoHexDigits = new Regex("[0-9a-fA-F][0-9a-fA-F]");
        public static bool passesAdditionalChecks(Sys.Uri uri) {
            if (uri.IsWellFormedOriginalString()) {
                return true;
            }
            
            // Check that any % characters are followed by two hex digits
            string original = uri.OriginalString;
            int index = 0;
            while (index >= 0) {
                index = original.IndexOf('%', index);
                if (index < 0) {
                    break;
                }
                if (index + 3 > original.Length ||
                    !twoHexDigits.Match(original.Substring(index + 1, 2)).Success) {
                    return false;
                }

                index += 3;
            }
            
            // Test to see if it's valid as an IRI. In principle, we could configure .NET to make System.Uri accept
            // IRIs, but it's a global switch, and we don't really want that. Instead, we check if it's well-formed
            // after replacing all non-ascii characters with ASCII alphabetics, and removing '%' characters
            
            UnicodeBuilder ub = new UnicodeBuilder();
            IntIterator chars = StringView.of(original).codePoints();
            while (chars.hasNext()) {
                int ch = chars.next();
                if (ch != '%') {
                    ub.append(ch < 127 ? ch : 'z');
                }
            }

            try {
                if (new Uri(ub.ToString(), UriKind.RelativeOrAbsolute).IsWellFormedOriginalString()) {
                    return true;
                }
            }
            catch (Exception e) {
                // ignore invalid URI failure
            }

            


            
            // string escaped = System.Uri.EscapeDataString(original);
            // if (uri.IsAbsoluteUri) {
            //     if (new System.Uri(escaped).IsWellFormedOriginalString()) {
            //         return true;
            //     }
            // }
            // else {
                // The first path segment of a relative URI reference cannot contain a ":"
                int slash = original.IndexOf('/');
                if (slash < 0) {
                    slash = original.Length;
                }

                int colon = original.IndexOf(':');
                if (colon >= 0 && colon < slash) {
                    return false;
                }

                // if (new System.Uri(new System.Uri("http://www.w3.org/"), escaped).IsWellFormedOriginalString()) {
                //     return true;
                // }
            //}

            return false;
        }

        public static string EscapeFilePath(System.Uri s) {
            StringBuilder builder = new();
            bool first = true;
            string input = s.ToString();
            int colon = input.IndexOf(':');
            foreach (string part in input.Substring(colon+1).Split('/')) {
                if (first) {
                    first = false;
                } else {
                    builder.Append('/');
                }
                builder.Append(System.Uri.EscapeDataString(part));
            }

            return input.Substring(0, colon+1) + builder.ToString();
        }

    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
