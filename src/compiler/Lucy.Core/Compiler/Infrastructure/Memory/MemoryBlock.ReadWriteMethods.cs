using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler.Infrastructure.Memory
{
    public partial class MemoryBlock
    {
        public void WriteInt8(sbyte value) => WriteUInt8((byte)value);
        public void WriteUInt8(byte value)
        {
            Span<byte> buffern = stackalloc byte[1];
            buffern[0] = value;
            WriteBytes(buffern);
        }

        public void WriteInt16(short value) => WriteUInt16((ushort)value);
        public void WriteUInt16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            WriteBytes(buffer);
        }

        public void WriteInt32(int value) => WriteUInt32((uint)value);
        public void WriteUInt32(uint value)
        {
            Span<byte> buffer = stackalloc byte[4];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            WriteBytes(buffer);
        }

        public void WriteInt64(ulong value) => WriteUInt64((uint)value);
        public void WriteUInt64(ulong value)
        {
            Span<byte> buffer = stackalloc byte[8];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);
            WriteBytes(buffer);
        }

        public byte[] ReadBytes(uint count)
        {
            var result = new byte[count];
            ReadBytes(result);
            return result;
        }

        public sbyte ReadInt8() => (sbyte)ReadUInt8();
        public byte ReadUInt8()
        {
            Span<byte> buffer = stackalloc byte[1];
            ReadBytes(buffer);
            return buffer[0];
        }

        public ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            ReadBytes(buffer);
            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        public int ReadInt32() => (int)ReadUInt32();
        public uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            ReadBytes(buffer);
            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        }

        public long ReadInt64() => (long)ReadUInt64();
        public ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            ReadBytes(buffer);
            uint lo = (uint)(buffer[0] | buffer[1] << 8 |
                             buffer[2] << 16 | buffer[3] << 24);
            uint hi = (uint)(buffer[4] | buffer[5] << 8 |
                             buffer[6] << 16 | buffer[7] << 24);
            return (ulong)hi << 32 | lo;
        }

        public void WriteNullTerminatedString(string str, Encoding encoding)
        {
            var bytes = encoding.GetBytes(str);
            WriteBytes(bytes);
            WriteInt8(0);
        }

        public string ReadNullTerminatedString(Encoding encoding)
        {
            List<byte> bytes = new();
            while (true)
            {
                var b = ReadUInt8();
                if (b == 0)
                    break;
                bytes.Add(b);
            }

            return encoding.GetString(bytes.ToArray());
        }
    }
}
