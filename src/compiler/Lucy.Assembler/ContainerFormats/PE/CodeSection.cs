﻿namespace Lucy.Assembler.ContainerFormats.PE
{
    public class CodeSection : ISection
    {
        private MemoryBlock _data;

        public CodeSection(MemoryBlock data)
        {
            _data = data;
        }
        public SectionData CreateData(bool is64Bit)
        {
            return new SectionData(".text", _data, SectionFlags.IMAGE_SCN_CNT_CODE | SectionFlags.IMAGE_SCN_MEM_EXECUTE | SectionFlags.IMAGE_SCN_MEM_READ);
        }
    }

    public record ImportAddressTableEntry(string DirectoryName, string LookupName)
    {
        public override string ToString()
        {
            return $"{DirectoryName}!{LookupName})";
        }
    }
}
