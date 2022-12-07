using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Xsl;

namespace Lucy.Core.Model;

public class HashBuilder : IDisposable
{
    private readonly MemoryStream _ms;
    private readonly BinaryWriter _bw;

    public HashBuilder()
    {
        _ms = new MemoryStream();
        _bw = new BinaryWriter(_ms, Encoding.UTF8, leaveOpen: false);
    }

    public void Add(string str)
    {
        _bw.Write((byte)0x01);
        _bw.Write(str);
    }

    public void Add(Type type)
    {
        _bw.Write((byte)0x02);
        _bw.Write(type.FullName ?? throw new Exception("Could not get type"));
    }

    public void BeginList()
    {
        _bw.Write((byte)0x03);
    }

    public void Add(IHashable? hashable)
    {
        if (hashable == null)
            _bw.Write((byte)0x00);
        else
        {
            _bw.Write((byte)0x04);
            _bw.Write(hashable.GetFullHash());
        }
    }

    public void Add(int number)
    {
        _bw.Write((byte)0x05);
        _bw.Write(number);
    }

    public void Add(double number)
    {
        _bw.Write((byte)0x06);
        _bw.Write(number);
    }
    
    public byte[] Build()
    {
        return MD5.HashData(_ms.GetBuffer().AsSpan()[..(int) _ms.Length]);
    }

    public void Dispose()
    {
        _bw.Dispose();
    }
}