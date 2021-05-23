using Lucy.Assembler.ContainerFormats.PE.RawStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.Assembler.ContainerFormats.PE
{
    public class ImportTableSection : ISection
    {
        private readonly HashSet<ImportTableEntry> _entries = new();

        public SectionData CreateData(bool is64Bit)
        {
            var m = new MemoryBlock();

            //Import Directory Table
            var directories = _entries.GroupBy(x => x.Directory).Select(x => new { DirectoryName = x.Key, Lookups = x.ToArray() }).ToArray();
            for (int i = 0; i < directories.Length; i++)
            {
                m.WriteUInt32(0, new AddressImport("ImportLookupTableOffsets_" + i, AddressType.RelativeVirtualAddress));
                m.WriteUInt32(0);
                m.WriteUInt32(0);
                m.WriteUInt32(0, new AddressImport("DirectoryNamesOffsets_" + i, AddressType.RelativeVirtualAddress));
                m.WriteUInt32(0, new AddressImport("ImportAddressTableOffsets_" + i, AddressType.RelativeVirtualAddress));
            }
            m.WriteBytes(new byte[20]);

            //Import Directory Table Strings
            for (int i = 0; i < directories.Length; i++)
            {
                m.AddAnnotation(new AddressExport("DirectoryNamesOffsets_" + i));
                m.WriteNullTerminatedString(directories[i].DirectoryName, Encoding.ASCII);
            }

            //Did not find any documentation for this, but it seems to work
            while (m.Address % 4 != 0)
                m.WriteInt8(0);

            //Import Lookup Table
            for (int i = 0; i < directories.Length; i++)
            {
                m.AddAnnotation(new AddressExport("ImportLookupTableOffsets_" + i));
                for (int j = 0; j < directories[i].Lookups.Length; j++)
                {
                    m.WriteUInt32(0, new AddressImport($"HintNameOffset_{i}_{j}", AddressType.RelativeVirtualAddress));
                }
                m.WriteUInt32(0);
            }

            //Import Address Table
            for (int i = 0; i < directories.Length; i++)
            {
                m.AddAnnotation(new AddressExport("ImportAddressTableOffsets_" + i));
                for (int j = 0; j < directories[i].Lookups.Length; j++)
                {
                    if (is64Bit)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        m.AddAnnotation(new AddressExport(new ImportAddressTableEntry(directories[i].DirectoryName, directories[i].Lookups[j].Lookup)));
                        m.WriteUInt32(0, new AddressImport($"HintNameOffset_{i}_{j}", AddressType.RelativeVirtualAddress));
                    }
                }

                if (is64Bit)
                    m.WriteUInt64(0);
                else
                    m.WriteUInt32(0);
            }

            //Hint/Name Table
            for (int i = 0; i < directories.Length; i++)
            {
                for (int j = 0; j < directories[i].Lookups.Length; j++)
                {
                    m.AddAnnotation(new AddressExport($"HintNameOffset_{i}_{j}"));
                    m.WriteUInt16(0);
                    var symbolName = directories[i].Lookups[j].Lookup;
                    if (symbolName == null)
                        throw new NotSupportedException("Writing import symbols without name is not supported");
                    m.WriteNullTerminatedString(symbolName, Encoding.ASCII);
                    if (m.Address % 2 != 0)
                        m.WriteInt8(0);
                }
            }

            return new SectionData(".idata", m, SectionFlags.IMAGE_SCN_CNT_INITIALIZED_DATA | SectionFlags.IMAGE_SCN_MEM_READ);
        }

        public void Add(ImportTableEntry entry)
        {
            _entries.Add(entry);
        }
    }

    public record ImportTableEntry
    {
        public ImportTableEntry(string directory, string lookup)
        {
            Directory = directory;
            Lookup = lookup;
        }

        public string Directory { get; }
        public string Lookup { get; }
    }
}
