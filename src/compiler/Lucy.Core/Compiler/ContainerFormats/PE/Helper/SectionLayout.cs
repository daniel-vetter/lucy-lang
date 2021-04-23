using Disassembler.Assembling;
using System.Collections.Generic;

namespace Disassembler.ContainerFormats.PE.Helper
{
    internal class SectionLayout
    {
        public uint CurrentFileAddress { get; private set; }
        public uint CurrentVirtualAddress { get; private set; }

        private readonly uint _fileAlignment;
        private readonly uint _sectionAlignment;

        private readonly List<Entry> _entries = new();
        private Entry? _lastMatchedEntry;

        public SectionLayout(uint startFileAddress, uint startVirtualAddress, uint fileAlignment, uint sectionAlignment)
        {
            CurrentFileAddress = startFileAddress;
            CurrentVirtualAddress = startVirtualAddress;
            _fileAlignment = fileAlignment;
            _sectionAlignment = sectionAlignment;
        }

        public void Add(uint length)
        {
            if (_entries.Count == 0)
            {
                _entries.Add(new Entry(
                    FileAddress: Align(CurrentFileAddress, _fileAlignment),
                    RelativeVirtualAddress: Align(CurrentVirtualAddress, _sectionAlignment),
                    Size: length)
                );
            }
            else
            {
                var last = _entries[^1];
                _entries.Add(new Entry(
                    FileAddress: Align(last.FileAddress + length, _fileAlignment),
                    RelativeVirtualAddress: Align(last.RelativeVirtualAddress + length, _sectionAlignment),
                    Size: length)
                );
            }

            CurrentFileAddress += Align(length, _fileAlignment);
            CurrentVirtualAddress += Align(length, _sectionAlignment);
        }

        private uint Align(uint value, uint alignment)
        {
            var r = value % alignment;
            if (r == 0)
                return value;
            return value + (alignment - r);
        }

        public uint ConvertAddress(uint address, AddressType type)
        {
            Entry? matchingEntry = null;

            if (_lastMatchedEntry?.ContainsFileAddress(address) == true)
                matchingEntry = _lastMatchedEntry;
            else
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    var entry = _entries[i];
                    if (address >= entry.FileAddress && address < entry.FileAddress + entry.Size)
                    {
                        matchingEntry = entry;
                        _lastMatchedEntry = matchingEntry;
                        break;
                    }
                }
            }

            if (matchingEntry == null)
                throw new AssemblerException($"Could not convert file address 0x{address:X2} to file address.");

            var offset = address - matchingEntry.FileAddress;
            var convertedAddress = matchingEntry.RelativeVirtualAddress + offset;
            if (type == AddressType.AbsoluteVirtualAddress)
                convertedAddress += 0x400000;
            return convertedAddress;
        }

        private record Entry(uint FileAddress, uint RelativeVirtualAddress, uint Size)
        {
            public bool ContainsFileAddress(uint fileAddress)
            {
                return fileAddress >= FileAddress && fileAddress < FileAddress + Size;
            }
        }
    }
}
