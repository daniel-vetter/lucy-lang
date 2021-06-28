using System;
using System.Diagnostics;

namespace Lucy.Assembler.Parsing.Reader
{
    [DebuggerDisplay("{Context,nq}")]
    public class AsmReader
    {
        private string _code;
        private int _pos = 0;

        public AsmReader(string code, OperandSize defaultOperandSize)
        {
            _code = code;
            DefaultOperandSize = defaultOperandSize;
        }

        public OperandSize DefaultOperandSize { get; private set; }

        public char Read() => _pos < _code.Length ? _code[_pos++] : '\0';
        public char Read(out char ch)
        {
            ch = Read();
            return ch;
        }
        public string Read(int length)
        {
            if (_pos + length > _code.Length)
                length -= _pos + length - _code.Length;
            var result = _code.Substring(_pos, length);
            _pos += length;
            return result;
        }

        public PositionTransaction BeginTransaction() => new PositionTransaction(this, _pos);

        public void Seek(int offset) => _pos += offset;
        public void SeekTo(int position) => _pos = position;
        public char Peek(int offset = 0) => _pos + offset < _code.Length ? _code[_pos + offset] : '\0';

        public bool IsDone => _pos >= _code.Length;
        public int Position => _pos;

        public string Context
        {
            get
            {
                var size = 30;

                var p1Start = _pos - size;
                var p1Length = size;
                if (p1Start < 0)
                {
                    p1Length += p1Start;
                    p1Start = 0;
                }

                var p2Start = _pos;
                var p2Length = size;
                if (p2Start + p2Length > _code.Length)
                    p2Length -= p2Start + p2Length - _code.Length;


                var p1 = _code.Substring(p1Start, p1Length);
                var p2 = _code.Substring(p2Start, p2Length);
                return p1 + "|" + p2;
            }
        }
    }

    public struct PositionTransaction : IDisposable
    {
        private readonly AsmReader _code;
        private readonly int _position;
        private bool _commited;

        public PositionTransaction(AsmReader code, int position)
        {
            _code = code;
            _position = position;
            _commited = false;
        }

        public void Commit()
        {
            _commited = true;
        }

        public bool IsCommited => _commited;

        public void Dispose()
        {
            if (!_commited)
                _code.SeekTo(_position);
        }
    }
}