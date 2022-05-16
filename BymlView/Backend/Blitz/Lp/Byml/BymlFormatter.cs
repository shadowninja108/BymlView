using LibHac.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace LibBlitz.Lp.Byml
{
    public class BymlFormatter<T> : IFormatter where T : class
    {
        /* Utilities to manage backing fields. */
        const string Prefix = "<";
        const string Suffix = ">k__BackingField";
        private static string GetBackingFieldName(string propertyName) => $"{Prefix}{propertyName}{Suffix}";
        private static string? GetAutoPropertyName(string fieldName)
        {
            var match = Regex.Match(fieldName, $"{Prefix}(.+?){Suffix}");
            return match.Success ? match.Groups[1].Value : null;
        }

        public SerializationBinder? Binder { get; set; }
        public StreamingContext Context { get; set; }
        public ISurrogateSelector? SurrogateSelector { get; set; }

        public object Deserialize(Stream serializationStream)
        {
            return Deserialize(serializationStream.AsStorage());
        }

        public object Deserialize(IStorage storage)
        {
            /* Parse out BYML. */
            Byml by = new(storage);

            string NameFromField(FieldInfo fi)
            {
                var name = fi.Name;
                /* Unwrap the backing field name if present. */
                return GetAutoPropertyName(name) ?? name;
            }

            object HandleNode(Type t, IBymlNode b)
            {
                /* Construct generic member. */
                var obj = FormatterServices.GetUninitializedObject(t);
                var members = FormatterServices.GetSerializableMembers(obj.GetType(), Context);
                var memberData = new object[members.Length];

                var barr = b as BymlArrayNode;
                var bhash = b as BymlHashTable;

                for (int i = 0; i < members.Length; i++)
                {
                    FieldInfo fi = (FieldInfo)members[i];

                    var memberType = fi.FieldType;
                    var memberName = NameFromField(fi);

                    dynamic node = b;

                    if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        /* See if this array is a member. */
                        if (barr == null)
                        {
                            node = bhash[memberName];

                            if (node == null)
                                throw new Exception($"Node {fi.Name} does not exist!");

                            if (node.Id != BymlNodeId.Array)
                                throw new Exception($"Expected an array, got a {b.GetType().Name}");
                        } /* Otherwise, the root node is array. */

                        /* Get the generic type for the IList. */
                        var subtype = memberType.GenericTypeArguments.FirstOrDefault();
                        if (subtype == null)
                            throw new Exception("Cannot use a parameterless generic list!");

                        /* Create generic list of sub. */
                        var listOfSubType = typeof(List<>).MakeGenericType(subtype);
                        var list = (IList)Activator.CreateInstance(listOfSubType);

                        /* Iterate through nodes. */
                        foreach (var n in node.Array)
                            list.Add(HandleNode(subtype, n));

                        memberData[i] = list;

                        continue;
                    }

                    /* Are we dealing with a primitive node? */
                    if (bhash != null)
                        node = bhash[memberName];

                    /* Handle generic properties (and strings) */
                    if (memberType.IsPrimitive || memberType == typeof(string))
                    {
                        /* Is there a corresponding BYML node for field? */
                        if (node != null)
                        {
                            /* Ensure this node is a primitive BymlNode<> */
                            var nodeType = (Type)node.GetType();
                            if (!nodeType.IsGenericType || nodeType.GetGenericTypeDefinition() != typeof(BymlNode<>))
                                throw new Exception("Node does not have a primitive value!");

                            /* Get BYML node type. */
                            var nodeGenericParamType = nodeType.GetGenericArguments().FirstOrDefault();
                            if (nodeGenericParamType == null)
                                /* Shouldn't happen... */
                                throw new Exception();

                            /* Validate BYML node generic type matches the member type. */
                            if (memberType != nodeGenericParamType)
                                throw new Exception("Mismatched node type!");

                            var data = node.Data;

                            memberData[i] = Convert.ChangeType(data, memberType);
                        } 
                        else
                        {
                            /* See if there's a DefaultValue attribute associated with this field. */
                            var prop = obj.GetType().GetProperty(memberName);

                            /* Check both the field and property. */
                            var attr = fi.GetCustomAttribute<DefaultValueAttribute>();
                            attr ??= prop?.GetCustomAttribute<DefaultValueAttribute>();

                            /* Get value from the attribute. */
                            if (attr != null)
                                memberData[i] = attr.Value;
                        }
                    }
                    else
                    {
                        if (node == null)
                            throw new Exception($"Node {fi.Name} does not exist!");

                        /* Otherwise try treating it as a member. */
                        memberData[i] = HandleNode(memberType, node);
                    }
                }

                /* Copy in parsed data into object. */
                FormatterServices.PopulateObjectMembers(obj, members, memberData);

                return obj;
            }

            return HandleNode(typeof(T), by.Root);
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            throw new NotImplementedException();
        }

        public static T QuickDeserialize(IStorage storage)
        {
            var f = new BymlFormatter<T>();
            return (T)f.Deserialize(storage);
        }
    }
}
