using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.Assembler.ContainerFormats.PE
{
    public class PortableExecutableBuilder
    {
        public ImmutableArray<byte> Code { get; set; } = ImmutableArray<byte>.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public bool Is64Bit { get; set; }
        public uint FileAlignment { get; } = 512;
        public uint SectionAlignment { get; } = 4096;

        private readonly List<ISection> _sections = new();

        public async Task Write(string path)
        {
            var m = new MemoryBlock();

            WriteDosStub(m);
            WriteFileHeader(m);
            WriteOptionalHeader(m);
            WriteSectionHeaders(m);
            WriteSections(m, out var sectionLayout);
            WriteAddressImports(m, sectionLayout);
            WriteChecksum(m);

            await File.WriteAllBytesAsync(path, m.ToArray());
        }

        private static void WriteAddressImports(MemoryBlock m, SectionLayout sectionLayout)
        {
            var annotationIndex = m.Annotations
                .GroupBy(x => x.Content)
                .ToDictionary(x => x.Key, x => x.ToArray());

            var imports = m.Annotations
                .Where(x => x.Content is AddressImport)
                .ToArray();

            var errors = new StringBuilder();
            foreach(var import in imports)
            {
                if (!annotationIndex.TryGetValue(((AddressImport)import.Content).Key, out var list))
                {
                    errors.AppendLine($"    Could not find the annotion \"{import.Content}\".");
                    continue;
                }
                    
                if (list.Length > 1)
                {
                    errors.AppendLine($"    Multiple address for annotation {import.Content} where found.");
                    continue;
                }
            }

            if (errors.Length > 0)
                throw new AssemblerException("Errors while resolving address imports:" + Environment.NewLine + errors.ToString());

            foreach (var import in imports)
            {
                var addressImport = (AddressImport)import.Content;
                var export = annotationIndex[addressImport.Key];
                var virtualAddress = sectionLayout.ConvertAddress(export.Single().Address, addressImport.Type);

                m.WriteUInt32ToAnnotation(import.Content, virtualAddress);
            }
        }

        private static void WriteChecksum(MemoryBlock m)
        {
            m.WriteUInt32ToAnnotation(new ChecksumAnnotation(), 0);

            long checksum = 0;
            var top = Math.Pow(2, 32);
            var data = m.ToArray();

            for (var i = 0; i < data.Length / 4; i++)
            {
                var dword = BitConverter.ToUInt32(data, i * 4);
                checksum = (checksum & 0xffffffff) + dword + (checksum >> 32);
                if (checksum > top)
                    checksum = (checksum & 0xffffffff) + (checksum >> 32);
            }

            checksum = (checksum & 0xffff) + (checksum >> 16);
            checksum += checksum >> 16;
            checksum &= 0xffff;

            checksum += (uint)data.Length;
            var cs = (uint)checksum;

            m.WriteUInt32ToAnnotation(new ChecksumAnnotation(), cs);
        }

        private void WriteSections(MemoryBlock m, out SectionLayout sectionLayout)
        {
            m.WriteZerosTillAddressIsMultipleOf(FileAlignment);
            m.WriteUInt32ToAnnotation(new SizeOfHeadersAnnotation(), m.Address);

            sectionLayout = new SectionLayout(512, 4096, FileAlignment, SectionAlignment);
            for (int i = 0; i < _sections.Count; i++)
            {
                ISection section = _sections[i];
                var data = section.CreateData(Is64Bit).Memory;

                if (section is ImportTableSection)
                {
                    m.WriteUInt32ToAnnotation(new ImportTableAddressAnnotation(), sectionLayout.CurrentVirtualAddress);
                    m.WriteUInt32ToAnnotation(new ImportTableSizeAnnotation(), data.Length);
                }

                m.WriteUInt32ToAnnotation(new SectionAnnotation(i, SectionAnnotationType.VirtualAddress), sectionLayout.CurrentVirtualAddress);
                m.WriteUInt32ToAnnotation(new SectionAnnotation(i, SectionAnnotationType.VirtualSize), data.Length);
                m.WriteUInt32ToAnnotation(new SectionAnnotation(i, SectionAnnotationType.RawDataAddress), m.Address);
                m.WriteUInt32ToAnnotation(new SectionAnnotation(i, SectionAnnotationType.RawDataSize), Align(data.Length, FileAlignment));

                m.WriteMemoryBlock(data);
                m.WriteZerosTillAddressIsMultipleOf(FileAlignment);

                sectionLayout.Add(data.Length);
            }

            m.WriteUInt32ToAnnotation(new SizeOfImageAnnotation(), sectionLayout.CurrentVirtualAddress);
        }

        private void WriteSectionHeaders(MemoryBlock m)
        {
            for (int i = 0; i < _sections.Count; i++)
            {
                ISection? section = _sections[i];
                var name = section.CreateData(Is64Bit).Name;
                if (name.Length > 8) throw new Exception("Name is too long. Only 8 characters supported.");
                var nameBytes = Encoding.UTF8.GetBytes(name);
                if (nameBytes.Length < 8)
                    nameBytes = nameBytes.Concat(new byte[8 - nameBytes.Length]).ToArray();

                m.WriteBytes(nameBytes);
                m.WriteUInt32(0, new SectionAnnotation(i, SectionAnnotationType.VirtualSize));                      //VirtualSize
                m.WriteUInt32(0, new SectionAnnotation(i, SectionAnnotationType.VirtualAddress));                   //VirtualAddress
                m.WriteUInt32(0, new SectionAnnotation(i, SectionAnnotationType.RawDataSize));                      //SizeOfRawData
                m.WriteUInt32(0, new SectionAnnotation(i, SectionAnnotationType.RawDataAddress));                   //PointerToRawData
                m.WriteUInt32(0);                                                                                   //PointerToRelocations
                m.WriteUInt32(0);                                                                                   //PointerToLinenumbers
                m.WriteUInt16(0);                                                                                   //NumberOfRelocations
                m.WriteUInt16(0);                                                                                   //NumberOfLinenumbers
                m.WriteUInt32((uint)section.CreateData(Is64Bit).Flags);
            }
        }

        private static void WriteDosStub(MemoryBlock m)
        {
            m.WriteBytes(new byte[]{
                0x4D, 0x5A, 0x80, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x10, 0x00, 0xFF, 0xFF, 0x00, 0x00,
                0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
                0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
                0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F,
                0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20,
                0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            });
        }

        private void WriteOptionalHeader(MemoryBlock m)
        {
            //Standard
            m.WriteUInt16((ushort)(Is64Bit ? PEFormat.PE32Plus : PEFormat.PE32));
            m.WriteInt8(1);                                                                                         //MajorLinkerVersion
            m.WriteInt8(73);                                                                                        //MinorLinkerVersion
            m.WriteUInt32(512);                                                                                     //CodeSectionSize
            m.WriteUInt32(1024);                                                                                    //InitializedDataSize
            m.WriteUInt32(0);                                                                                       //UninitializedDataSize
            m.WriteUInt32(0, new AddressImport(new EntryPointAnnotation(), AddressType.RelativeVirtualAddress));    //EntryPoint
            m.WriteUInt32(4096 * 3);                                                                                //CodeBase
            if (!Is64Bit)
                m.WriteUInt32(8192);                                                                                //Database

            //Windows specific
            if (Is64Bit)
                m.WriteUInt64(0x400000); //ImageBase
            else
                m.WriteUInt32(0x400000); //ImageBase

            m.WriteUInt32(SectionAlignment);                                                                        //SectionAlignment
            m.WriteUInt32(FileAlignment);                                                                           //FileAlignment
            m.WriteUInt16(1);                                                                                       //MajorOperatingSystemVersion
            m.WriteUInt16(0);                                                                                       //MinorOperatingSystemVersion
            m.WriteUInt16(0);                                                                                       //MajorImageVersion
            m.WriteUInt16(0);                                                                                       //MinorImageVersion
            m.WriteUInt16(3);                                                                                       //MajorSubsystemVersion
            m.WriteUInt16(10);                                                                                      //MinorSubsystemVersion
            m.WriteUInt32(0);                                                                                       //Win32Version
            m.WriteUInt32(0, new SizeOfImageAnnotation());                                                          //SizeOfImage
            m.WriteUInt32(0, new SizeOfHeadersAnnotation());                                                        //SizeOfHeaders
            m.WriteUInt32(0, new ChecksumAnnotation());                                                             //Checksum (https://practicalsecurityanalytics.com/pe-checksum/)
            m.WriteUInt16((ushort)Subsystem.IMAGE_SUBSYSTEM_WINDOWS_CUI);                                           //Subsystem
            m.WriteUInt16(0);                                                                                       //DllCharacteristics

            if (!Is64Bit)
            {
                m.WriteUInt32(4096);                                                                                //StackReserveSize
                m.WriteUInt32(4096);                                                                                //StackCommitSize
                m.WriteUInt32(65536);                                                                               //HeapReserveSize
                m.WriteUInt32(0);                                                                                   //HeapCommitSize
            }

            if (Is64Bit)
            {
                m.WriteUInt64(4096);                                                                                //StackReserveSize
                m.WriteUInt64(4096);                                                                                //StackCommitSize
                m.WriteUInt64(65536);                                                                               //HeapReserveSize
                m.WriteUInt64(0);                                                                                   //HeapCommitSize
            }

            m.WriteUInt32(0);                                                                                       //LoaderFlags

            m.WriteUInt32(16);                                                                                      //DataDirectoriesCount
            for (int i = 0; i < 16; i++)
            {
                if (i == 1)
                {
                    m.WriteUInt32(0, new ImportTableAddressAnnotation());
                    m.WriteUInt32(0, new ImportTableSizeAnnotation());
                    continue;
                }

                m.WriteUInt64(0);
            }
        }

        private void WriteFileHeader(MemoryBlock m)
        {
            m.WriteBytes(new byte[] { 0x50, 0x45, 0x00, 0x00 });                                                    //PE Identifier
            m.WriteUInt16((ushort)MachineTypes.IMAGE_FILE_MACHINE_I386);                                            //Machine Type    
            m.WriteUInt16((ushort)_sections.Count);                                                                 //Section Count
            m.WriteUInt32((uint)Timestamp.ToUnixTimeSeconds());                                                     //Timestamp
            m.WriteUInt32(0);                                                                                       //Pointer to symbol table (depricated)
            m.WriteUInt32(0);                                                                                       //Number of symbols (seems to be always 0)
            m.WriteUInt16((ushort)((Is64Bit ? 112 : 96) + 8 * 16));                                                 //OptionalHeaderSize
            m.WriteUInt16((ushort)(Characteristics.IMAGE_FILE_RELOCS_STRIPPED |                                     //Characteristics
                                   Characteristics.IMAGE_FILE_EXECUTABLE_IMAGE |
                                   Characteristics.IMAGE_FILE_LINE_NUMS_STRIPPED |
                                   Characteristics.IMAGE_FILE_LOCAL_SYMS_STRIPPED |
                                   Characteristics.IMAGE_FILE_32BIT_MACHINE));
        }

        private static uint Align(uint value, uint alignment) => value % alignment == 0 ? value : value + (alignment - value % alignment);

        public void AddSection(ISection section)
        {
            _sections.Add(section);
        }

        private record ChecksumAnnotation();
        private record SizeOfHeadersAnnotation();
        private record SizeOfImageAnnotation();
        private record ImportTableAddressAnnotation();
        private record ImportTableSizeAnnotation();
        private record SectionAnnotation(int Index, SectionAnnotationType Type);
        private enum SectionAnnotationType
        {
            VirtualSize,
            VirtualAddress,
            RawDataSize,
            RawDataAddress
        }
    }

    public record AddressImport(object Key, AddressType Type)
    {
        public override string ToString()
        {
            if (Type == AddressType.AbsoluteVirtualAddress)
                return $"VA(" + Key + ")";
            if (Type == AddressType.RelativeVirtualAddress)
                return $"RVA(" + Key + ")";

            throw new NotSupportedException("Unsupported address type: " + Type);
        }
    }

    public enum AddressType
    {
        AbsoluteVirtualAddress,
        RelativeVirtualAddress
    }

    public interface ISection
    {
        SectionData CreateData(bool is64Bit);
    }

    public class SectionData
    {
        public SectionData(string name, MemoryBlock memory, SectionFlags flags)
        {
            Name = name;
            Memory = memory;
            Flags = flags;
        }

        public string Name { get; }
        public MemoryBlock Memory { get; }
        public SectionFlags Flags { get; }
    }
}
