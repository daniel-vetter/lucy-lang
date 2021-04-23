using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Disassembler.Infrastructure.Memory
{
    public partial class MemoryBlock //TODO: Perf
    {
        public uint Address { get; set; }
        public uint Length => (uint)_list.Count;
        public ImmutableArray<Annotation> Annotations => _annotations.ToImmutableArray();

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
            while ((Address % alignment) != 0)
                WriteInt8(0);
        }

        public byte[] ToArray() => _list.ToArray();
        public ImmutableArray<byte> ToImmutableArray() => _list.ToImmutableArray();

        public void WriteMemoryBlock(MemoryBlock data)
        {
            var offset = Address;
            WriteBytes(data.ToArray());
            foreach(var annotationEntry in data._annotations)
            {
                AddAnnotation(offset + annotationEntry.Address, annotationEntry.Content);
            }
        }
    }
}
