using BymlView.Backend;
using LibBlitz.Lp.Byml;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using LibBlitz.Sead;

namespace BymlView.Frontend.Models
{
    public class BymlNodeAdapter : ReactiveObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();

        public struct NodeInfo
        {
            public IBymlNode Node;
            public BymlNodeAdapter? Parent;
            public string Name;
            public bool Root;
            public bool Hashed;
            public bool SkipHash;
        }

        /* Unchanging members. */
        private readonly IBymlNode Node;

        /* Reactive members. */
        private BymlNodeAdapter? _parent;
        public BymlNodeAdapter? Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private readonly ObservableAsPropertyHelper<string> _displayName;
        public string DisplayName => _displayName.Value;

        private bool _hashedName;
        public bool HashedName
        {
            get => _hashedName;
            set => this.RaiseAndSetIfChanged(ref _hashedName, value);
        }

        private object? _value;
        public object? Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public bool Root { get; }

        // private NodeInfo Info;

        private bool _initChildren;
        private readonly ObservableCollection<BymlNodeAdapter> _children = new();

        /* Defer building children to when needed. */
        private ObservableCollection<BymlNodeAdapter> Children
        {
            get
            {
                if (!_initChildren)
                {
                    BuildChildren();
                }
                _initChildren = true;
                return _children;
            }
        }

        public BymlNodeAdapter(Byml root, string name) : this(new()
        {
            Node = root.Root,
            Name = name,
            Root = true,
            SkipHash = true,
        }) { }

        public BymlNodeAdapter(NodeInfo info)
        {
            Node = info.Node;
            Parent = info.Parent;

            /* Try to see if it's a hash. */
            if (!info.SkipHash &&
                !info.Hashed &&
                TryDehash(ref info.Name))
            {
                HashedName = true;
            }

            Name = info.Name;
            Root = info.Root;

            Value = ValueImpl;
            
            _displayName = this.WhenAnyValue(
                    x => x.Name, x => x.Value, CreateDisplayName)
                .ToProperty(this, x => x.DisplayName, out _displayName);
        }

        private static Regex ArrayWithNumberPattern = new(@"\[*[0-9]\]");

        private bool TryDehash(ref string name)
        {
            var n = name;

            /* See if we can parse out a hex number. */
            if (!uint.TryParse(n, System.Globalization.NumberStyles.HexNumber, null, out var hash))
                return false;
            /* See if it's part of our hash dictionary. */
            var dehash = HashDb.FindByHash(hash);
            if (dehash == null)
            {
                /* See if we can predict the hash name. */


                /* Nothing to do with the root node. */
                if (Parent == null || Parent.Root)
                    return false;

                /* Nothing to infer if the parent isn't hash and hasn't been de-hashed. */
                if (Parent.IsHashTable || !Parent.HashedName)
                    return false;

                n = $"{Parent.Name}[].{n}";
                var testHash = HashCrc32.CalcStringHash(n);

                if (testHash != hash)
                    return false;

                HashDb.TryAddHash(n, testHash);
            }

            /* Found a match, this is a hashed name. */
            name = dehash;

            if (Parent is { IsHashTable: true, HashedName: false })
            {
                if (ArrayWithNumberPattern.IsMatch(name))
                {
                    var testStr = name.Substring(0, name.IndexOf('['));
                    var testHash = HashCrc32.CalcStringHash(testStr);

                    if (Parent.Name == $"{testHash:x}")
                    {
                        HashDb.TryAddHash(testStr, testHash);
                        Parent.Name = testStr;
                        Parent.HashedName = true;
                    }
                }
            }

            return true;
        }

        private string CreateDisplayName(string name, object? value)
        {
            string displayName = name;

            if (!displayName.Contains('.'))
                displayName += $" ({NodeId})";

            if (value != null)
                displayName += $" : {value}";

            return displayName;
        }

        private object? ValueImpl
        {
            get
            {
                if (IsValueNode)
                {
                    /* Just return the data. */
                    dynamic node = Node;
                    return node.Data;
                }

                /* TODO: other node types? */

                return null;
            }
        }

        private void BuildChildren()
        {
            /* Value nodes have no children. */
            if (IsValueNode)
                return;

            /* Setup a node info struct. */
            NodeInfo info = new()
            {
                Parent = this,
                Root = false
            };

            if (IsHashTable)
            {
                var by = (BymlHashTable)Node;

                foreach (var pair in by.Pairs)
                {
                    info.Node = pair.Value;
                    info.Name = pair.Name;
                    _children.Add(new BymlNodeAdapter(info));
                }
                return;
            }

            if(IsArray)
            {
                /* Array elements don't need hashed. */
                /* TODO: change later? */
                info.SkipHash = true;

                var by = (BymlArrayNode)Node;
                var array = by.Array;

                for (int i = 0; i < array.Length; i++)
                {
                    info.Node = array[i];
                    info.Name = $"[{i}]";
                    _children.Add(new BymlNodeAdapter(info));
                }
                return;
            }
        }


        /* Convenience tools. */
        public BymlNodeId NodeId => Node.Id;
        public bool IsValueNode => Byml.IsValueBymlNode(NodeId);
        public bool IsHashTable => NodeId == BymlNodeId.Hash;
        public bool IsArray => NodeId == BymlNodeId.Array;
    }
}
