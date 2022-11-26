using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Lucy.Core.Parsing;

[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Code
{
    private readonly string _code;
    private int _pos;

    public Code(string code)
    {
        _code = code;
    }

    public char Read() => _pos < _code.Length ? _code[_pos++] : '\0';
    public char Read(out char ch)
    {
        ch = Read();
        return ch;
    }
    public string Read(int length)
    {
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

    private string DebuggerDisplay
    {
        get => _code.Substring(_pos);
    }
}

public struct PositionTransaction : IDisposable
{
    private readonly Code _code;
    private readonly int _position;
    private bool _commited;

    public PositionTransaction(Code code, int position)
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