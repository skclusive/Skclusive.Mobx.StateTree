using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public static class NodeExtensions
    {

        public static bool IsNode(this object node)
        {
            return node is ScalarNode || node is ObjectNode;
        }

        public static INode ResolveNodeByPath(this ObjectNode bnode, string path, bool failIfResolveFails = true)
        {
            return bnode.ResolveNodeByPaths(path.SplitJsonPath(), failIfResolveFails);
        }

        public static INode ResolveNodeByPaths(this ObjectNode bnode, IEnumerable<string> paths, bool failIfResolveFails = true)
        {
            return bnode.ResolveNodeByPaths(paths.ToArray(), failIfResolveFails);
        }

        public static INode ResolveNodeByPaths(this ObjectNode bnode, string[] paths, bool failIfResolveFails = true)
        {
            // counter part of getRelativePath
            // note that `../` is not part of the JSON pointer spec, which is actually a prefix format
            // in json pointer: "" = current, "/a", attribute a, "/" is attribute "" etc...
            // so we treat leading ../ apart...

            INode current = bnode;

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (path == "")
                {
                    current = current?.Root;
                    continue;
                }
                else if (path == "..")
                {
                    current = current?.Parent;
                    if (current != null)
                    {
                        // not everything has a parent
                        continue;
                    }
                }
                else if (path == "." || path == "")
                {
                    // '/bla' or 'a//b' splits to empty strings
                    continue;
                }
                else if (current != null)
                {
                    if (current is ScalarNode)
                    {
                        // check if the value of a scalar resolves to a state tree node (e.g. references)
                        // then we can continue resolving...
                        try
                        {
                            var value = current.Value;
                            if (value.IsStateTreeNode())
                            {
                                current = value.GetStateTreeNode();
                                // fall through
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!failIfResolveFails)
                            {
                                return null;
                            }
                            throw ex;
                        }
                    }

                    if (current is ObjectNode)
                    {
                        var subType = (current as ObjectNode).GetChildType(path);
                        if (subType != null)
                        {
                            current = (current as ObjectNode).GetChildNode(path);
                            if (current != null)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (failIfResolveFails)
                {
                    var join = paths.Take(i).JoinJsonPath();

                    var value = string.IsNullOrWhiteSpace(join) ? "/" : join;

                    throw new Exception($"Could not resolve '{path}' in path '{value}' while resolving '{paths.JoinJsonPath()}'");
                }
                else
                {
                    return null;
                }
            }
            return current;
        }
    }
}
