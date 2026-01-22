using System;
using System.Text.RegularExpressions;

namespace Automation.Core.Recorder
{
    public static class RouteNormalizer
    {
        /// <summary>
        /// Normalize route to a canonical relative form: prefer the tail starting at the last ".html" segment, including fragment.
        /// If BASE_URL is provided and url starts with it, we strip it and then take the tail.
        /// Fallbacks to pathname+fragment or "/".
        /// </summary>
        public static string Normalize(string? url, string? pathname, string? fragment, string? baseUrl = null)
        {
            string candidate = null;

            if (!string.IsNullOrWhiteSpace(url))
            {
                candidate = url;
                if (!string.IsNullOrWhiteSpace(baseUrl) && candidate.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    candidate = candidate.Substring(baseUrl.Length);
                }
            }

            if (string.IsNullOrWhiteSpace(candidate) && !string.IsNullOrWhiteSpace(pathname))
            {
                candidate = pathname;
            }

            if (string.IsNullOrWhiteSpace(candidate))
                candidate = "/";

            // If there is an embedded fragment provided separately, append if not present in candidate
            if (!string.IsNullOrWhiteSpace(fragment) && !candidate.Contains(fragment))
            {
                candidate = candidate + fragment;
            }

            // Try to extract tail starting from last '/<name>.html' occurrence
            var m = Regex.Match(candidate, @"(/[^/]*?\.html(?:[^\s]*)?)$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                var tail = m.Groups[1].Value;
                // Ensure it starts with a slash
                if (!tail.StartsWith("/")) tail = "/" + tail;
                return tail;
            }

            // No .html tail found — try to extract last path segment and include fragment
            var lastSlash = candidate.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < candidate.Length - 1)
            {
                var seg = candidate.Substring(lastSlash);
                // include fragment if present
                var fragIdx = candidate.IndexOf('#');
                if (fragIdx >= 0)
                {
                    var frag = candidate.Substring(fragIdx);
                    if (!seg.Contains('#')) seg = seg + frag;
                }

                return seg;
            }

            // As last resort, return candidate cleaned (remove file: or drive prefixes)
            // Remove leading file:// or file:///
            candidate = Regex.Replace(candidate, @"^file:\/\/+", string.Empty, RegexOptions.IgnoreCase);
            // Remove leading Windows drive prefix like C:\ or /C:
            candidate = Regex.Replace(candidate, @"^\/?[A-Za-z]:", string.Empty);
            if (!candidate.StartsWith("/")) candidate = "/" + candidate;

            return candidate;
        }
    }
}
