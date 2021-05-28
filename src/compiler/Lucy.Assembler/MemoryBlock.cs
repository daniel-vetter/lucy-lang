using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Lucy.Assembler
{
    public class MemoryBlock //TODO: Perf
    {
        public uint Address { get; set; }
        public uint Length => (uint)_list.Count;
        public ImmutableArray<Annotation> Annotations => _annotations.ToImmutableArray();

        private readonly List<Annotation> _annotations = new();
        private readonly Dictionary<object, List<uint>> _annotationIndex = new();

        private readonly List<byte> _list = new();

        public void ReadBytes(Span<byte> destination)
        {
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = _list[(int)Address];
                Address++;
            }
        }

        public void WriteBytes(Span<byte> source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                var b = source[i];

                if (Address == _list.Count)
                    _list.Add(b);
                else
                    _list[(int)Address] = b;
                Address++;
            }
        }

        public void WriteZerosTillAddressIsMultipleOf(uint alignment)
        {
            while (Address % alignment != 0)
                WriteInt8(0);
        }

        public byte[] ToArray() => _list.ToArray();
        public ImmutableArray<byte> ToImmutableArray() => _list.ToImmutableArray();

        public void WriteMemoryBlock(MemoryBlock data)
        {
            var offset = Address;
            WriteBytes(data.ToArray());
            foreach (var annotationEntry in data._annotations)
            {
                AddAnnotation(offset + annotationEntry.Address, annotationEntry.Content);
            }
        }

        [Obsolete("Dont use this")]
        public void WriteAnnotation(string key)
        {
            AddAnnotation(Address, key);
        }

        public void WriteAnnotation(object key)
        {
            AddAnnotation(Address, key);
        }

        public void AddAnnotation(uint address, object key)
        {
            _annotations.Add(new Annotation(address, key));
            if (_annotationIndex.TryGetValue(key, out var list))
            {
                list.Add(address);
            }
            else
            {
                _annotationIndex.Add(key, new List<uint>() { address });
            }
        }

        public void WriteUInt8(byte value, object annotation)
        {
            WriteAnnotation(annotation);
            WriteUInt8(value);
        }

        public void WriteUInt16(ushort value, object annotation)
        {
            WriteAnnotation(annotation);
            WriteUInt16(value);
        }

        public void WriteUInt32(uint value, object annotation)
        {
            WriteAnnotation(annotation);
            WriteUInt32(value);
        }

        public void WriteUInt64(ulong value, object annotation)
        {
            WriteAnnotation(annotation);
            WriteUInt64(value);
        }

        public void WriteUInt8ToAnnotation(object annotation, byte value)
        {
            var org = Address;
            if (!_annotationIndex.TryGetValue(annotation, out var addresses))
            {
                Address = org;
                throw new Exception("Could not find annotation: " + annotation);
            }

            foreach (var address in addresses)
            {
                Address = address;
                WriteUInt8(value);
            }
            Address = org;
        }

        public void WriteUInt16ToAnnotation(object annotation, ushort value)
        {
            var org = Address;
            if (!_annotationIndex.TryGetValue(annotation, out var addresses))
            {
                Address = org;
                throw new Exception("Could not find annotation: " + annotation);
            }

            foreach (var address in addresses)
            {
                Address = address;
                WriteUInt16(value);
            }
            Address = org;
        }

        public void WriteUInt32ToAnnotation(object annotation, uint value)
        {
            var org = Address;
            if (!_annotationIndex.TryGetValue(annotation, out var addresses))
            {
                Address = org;
                throw new Exception("Could not find annotation: " + annotation);
            }

            foreach (var address in addresses)
            {
                Address = address;
                WriteUInt32(value);
            }
            Address = org;
        }

        public void WriteUInt64ToAnnotation(object annotation, ulong value)
        {
            var org = Address;
            if (!_annotationIndex.TryGetValue(annotation, out var addresses))
            {
                Address = org;
                throw new Exception("Could not find annotation: " + annotation);
            }

            foreach (var address in addresses)
            {
                Address = address;
                WriteUInt64(value);
            }
            Address = org;
        }

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

    public record Annotation(uint Address, object Content);
}
