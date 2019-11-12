using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public static class PathExtensions
    {
        public static (IJsonPatch, IJsonPatch) SplitPatch(this IReversibleJsonPatch patch)
        {
            return (patch.StripPatch(), patch.InvertPatch());
        }

        public static IJsonPatch StripPatch(this IReversibleJsonPatch patch)
        {
            // strips `oldvalue` information from the patch, so that it becomes a patch conform the json-patch spec
            // this removes the ability to undo the patch

            return new JsonPatch
            {
                Operation = patch.Operation,

                Value = patch.Value,

                Path = patch.Path
            };
        }

        public static IJsonPatch InvertPatch(this IReversibleJsonPatch patch)
        {
            return new JsonPatch
            {
                Operation = patch.Operation == JsonPatchOperation.Add ? JsonPatchOperation.Remove : patch.Operation == JsonPatchOperation.Remove ? JsonPatchOperation.Add : patch.Operation,

                Value = patch.OldValue,

                Path = patch.Path
            };
        }

        /***
         * escape slashes and backslashes
         * http://tools.ietf.org/html/rfc6901
         */
        public static string EscapeJsonPath(this string path)
        {
            return path.Replace("~", "~1").Replace("/", "~0");
        }

        /***
         * unescape slashes and backslashes
         */
        public static string UnEscapeJsonPath(this string path)
        {
            return path.Replace("~0", "/").Replace("~1", "~");
        }

        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return string.Join(separator, strings);
        }

        public static string JoinJsonPath(this IEnumerable<string> paths)
        {
            return paths.ToArray().JoinJsonPath();
        }

        /***
         * Generates a json-path compliant json path from path parts
         */
        public static string JoinJsonPath(this string[] paths)
        {
            if (paths.Length == 0)
            {
                return string.Empty;
            }

            return $"/{paths.Select(path => path.EscapeJsonPath()).Join("/")}";
        }

        /***
         * Splits and decodes a json path into several parts
         */
        public static string[] SplitJsonPath(this string path)
        {
            // `/` refers to property with an empty name, while `` refers to root itself!
            var paths = path.Split('/').Select(p => p.UnEscapeJsonPath()).ToArray();

            // path '/a/b/c' -> a b c
            // path '../../b/c -> .. .. b c
            return paths[0] == string.Empty ? paths.Skip(1).ToArray() : paths;
        }
    }
}
