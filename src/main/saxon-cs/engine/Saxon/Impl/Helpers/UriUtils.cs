using System.Text.RegularExpressions;
using Sys = System;
namespace Saxon.Impl.Helpers {
    public class UriUtils {
        public static bool IsOpaque(System.Uri uri) {
            // Javadoc: An opaque URI is an absolute URI whose scheme-specific part does not begin with a slash character ('/').
            Sys.Console.Error.WriteLine("Uri.ISOpaque " + uri + " local path " + uri.LocalPath);
            return !uri.LocalPath.StartsWith("/");
        }
        public static System.Uri Normalize(System.Uri uri) {
            return uri;
        }

        public static string GetQuery(System.Uri uri) {
            string q = uri.Query;
            if (q.StartsWith("?")) {
                q = q.Substring(1);
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
            throw new(uri.ToString());
        }

        private static readonly Regex twoHexDigits = new Regex("[0-9a-fA-F][0-9a-fA-F]");
        public static bool passesAdditionalChecks(Sys.Uri uri) {
            if (uri.IsWellFormedOriginalString()) {
                return true;
            }
            
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
            // if (original.Contains("%")) {
            //     // If the original contains "%" check that it's followed by two hex digits
            //     return false;
            // }
            
            string escaped = System.Uri.EscapeUriString(original);
            if (uri.IsAbsoluteUri) {
                if (new System.Uri(escaped).IsWellFormedOriginalString()) {
                    return true;
                }
            }
            else {
                // The first path segment of a relative URI reference cannot contain a ":"
                int slash = original.IndexOf('/');
                if (slash < 0) {
                    slash = original.Length;
                }

                int colon = original.IndexOf(':');
                if (colon >= 0 && colon < slash) {
                    return false;
                }

                if (new System.Uri(new System.Uri("http://www.w3.org/"), escaped).IsWellFormedOriginalString()) {
                    return true;
                }
            }

            return false;
        }

    }
}

// Copyright (c) 2021 Saxonica Limited