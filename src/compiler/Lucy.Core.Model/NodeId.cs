using System.Text;

namespace Lucy.Core.Model
{
    public class NodeId : IHashable
    {
        public NodeId(string documentPath, string nodePath)
        {
            DocumentPath = documentPath;
            NodePath = nodePath;
            _str = DocumentPath + "!" + NodePath;
            _hash = Encoding.UTF8.GetBytes(_str);
        }

        private static NodeId _unitialized = new NodeId("!", "Uninitalized");
        public static NodeId Uninitalized => _unitialized;

        public string DocumentPath { get; }
        public string NodePath { get; }

        private string _str;
        private byte[] _hash;

        public byte[] GetFullHash() => _hash;

        public override bool Equals(object? obj)
        {
            return obj is NodeId id && DocumentPath == id.DocumentPath && NodePath == id.NodePath;
        }

        public override int GetHashCode()
        {
            return _str.GetHashCode();
        }

        public override string ToString() => _str;

        public bool IsRoot => NodePath.IndexOf('.') == -1;
        public NodeId Parent
        {
            get
            {
                var lastIndex = NodePath.LastIndexOf('.');
                if (lastIndex == -1)
                    throw new Exception("Current node is already the root node id.");
                return new NodeId(DocumentPath, NodePath[..lastIndex]);
            }
        }

        public static bool operator ==(NodeId? id1, NodeId? id2)
        {
            if (ReferenceEquals(id1, id2)) return true;
            if (ReferenceEquals(id1, null)) return false;
            if (ReferenceEquals(id2, null)) return false;
            return id1.Equals(id2);
        }

        public static bool operator !=(NodeId? id1, NodeId? id2)
        {
            return !(id1 == id2);
        }
    }
}
