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
    }
}
