// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System.Collections.Generic;
using System.Text;

namespace x2net.xpiler
{
    public class TypeSpec
    {
        public string Type { get; set; }
        public List<TypeSpec> Details { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder(Type);
            if (Details != null && Details.Count != 0)
            {
                sb.Append('(');
                for (int i = 0, count = Details.Count; i < count; ++i)
                {
                    if (i != 0) { sb.Append(", "); }
                    sb.Append(Details[i].ToString());
                }
                sb.Append(')');
            }
            return sb.ToString();
        }
    }

    class TypeProperty
    {
        public bool IsCollection { get; set; }
        public bool IsPrimitive { get; set; }
        public bool DetailRequired { get; set; }
    }

    static class Types
    {
        private static IDictionary<string, TypeProperty> types;

        static Types()
        {
            types = new Dictionary<string, TypeProperty>();

            // Primitive types
            types.Add("bool", new TypeProperty
            {
                IsPrimitive = true, 
                DetailRequired = false
            });
            types.Add("byte", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("int8", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("int16", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("int32", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("int64", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("float32", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("float64", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("string", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });
            types.Add("datetime", new TypeProperty
            {
                IsPrimitive = true,
                DetailRequired = false
            });

            // Collection types
            types.Add("bytes", new TypeProperty {
                IsCollection = true,
                IsPrimitive = false,
                DetailRequired = false
            });
            types.Add("list", new TypeProperty
            {
                IsCollection = true,
                IsPrimitive = false,
                DetailRequired = true
            });
            types.Add("map", new TypeProperty
            {
                IsCollection = true,
                IsPrimitive = false,
                DetailRequired = true
            });
        }

        public static bool IsBuiltin(string type)
        {
            return types.ContainsKey(type);
        }

        public static bool IsCollection(string type)
        {
            TypeProperty typeProperty;
            return (types.TryGetValue(type, out typeProperty) ?
                typeProperty.IsCollection : false);
        }

        public static bool IsPrimitive(string type)
        {
            TypeProperty typeProperty;
            return (types.TryGetValue(type, out typeProperty) ?
                typeProperty.IsPrimitive : false);
        }

        public static TypeSpec Parse(string s)
        {
            int index = 0;
            return ParseTypeSpec(s, ref index);
        }

        private static TypeSpec ParseTypeSpec(string s, ref int index)
        {
            string type = null;
            List<TypeSpec> details = null;

            var backMargin = 0;
            var start = index;
            for (; index < s.Length; ++index)
            {
                var c = s[index];
                if (c == '(' && index < (s.Length - 1))
                {
                    type = s.Substring(start, index - start).Trim();
                    ++index;
                    details = ParseDetails(s, ref index);
                    backMargin = 1;
                    break;
                }
                else if (c == ',')
                {
                    ++index;
                    backMargin = 1;
                    break;
                }
                else if (c == ')')
                {
                    break;
                }
            }
            if ((object)type == null)
            {
                type = s.Substring(start, index - start - backMargin).Trim();
            }
            return (type.Length == 0 ? null :
                new TypeSpec { Type = type, Details = details });
        }

        private static List<TypeSpec> ParseDetails(string s, ref int index)
        {
            List<TypeSpec> details = new List<TypeSpec>();

            var start = index;
            for (; index < s.Length; ++index)
            {
                var c = s[index];
                if (c == ',')
                {
                    continue;
                }
                if (c == ')')
                {
                    ++index;
                    break;
                }
                else
                {
                    var detail = ParseTypeSpec(s, ref index);
                    if ((object)detail != null)
                    {
                        details.Add(detail);
                        --index;
                    }
                }
            }
            return (details.Count == 0 ? null : details);
        }
    }
}
