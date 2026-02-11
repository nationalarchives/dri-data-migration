using System;
using System.Linq;

namespace Api;

public static class UriExtensions
{
    extension(Uri uri)
    {
        public string LastSegment() => uri.Segments.Any() ? uri.Segments[uri.Segments.Count() - 1] : string.Empty;
    }
}
