using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.DebugUI.Services
{
    public class LineReader
    {
        private readonly string _path;
        private int _used;
        private byte[] _data = new byte[_minBufferSize];
        private int _consumed;
        private static readonly int _minBufferSize = 1024 * 4;

        public LineReader(string path)
        {
            _path = path;
        }

        public async Task<ImmutableArray<string>> ReadMoreFromFile()
        {
            using var file = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            file.Seek(_used, SeekOrigin.Begin);

            while (true)
            {
                EnsureBufferSize();
                var len = await file.ReadAsync(_data, _used, _data.Length - _used);
                if (len == 0)
                {
                    break;
                }
                _used += len;
            }

            return FindNextStrings();
        }

        private ImmutableArray<string> FindNextStrings()
        {
            var result = ImmutableArray.CreateBuilder<string>();
            for (int i = _consumed;  i < _used; i++)
            {
                if (_data[i] != '\n')
                    continue;

                var str = Encoding.UTF8.GetString(_data, _consumed, i - _consumed + 1);
                _consumed = i + 1;
                result.Add(str);
            }
            return result.ToImmutable();
        }

        private void EnsureBufferSize()
        {
            if (_data.Length - _used > _minBufferSize)
                return;

            var newData = new byte[_data.Length * 2];
            Array.Copy(_data, newData, _used);
            _data = newData;
        }
    }
}
