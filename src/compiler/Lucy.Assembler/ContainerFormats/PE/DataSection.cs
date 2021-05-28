using Lucy.Assembler.ContainerFormats.PE.RawStructs;
using System.Collections.Generic;

namespace Lucy.Assembler.ContainerFormats.PE
{
    public class DataSection : ISection
    {
        private readonly List<byte[]> _bytes = new();
        private readonly List<Annotation> _annotations = new();
        private uint _address = 0;

        public DataSection()
        {
        }

        public SectionData CreateData(bool is64Bit)
        {
            var m = new MemoryBlock();
            for (int i = 0; i < _bytes.Count; i++)
            {
                m.WriteAnnotation("rdata_" + i);
                m.WriteBytes(_bytes[i]);
            }
            foreach (var annotation in _annotations)
            {
                m.AddAnnotation(annotation.Address, annotation.Value);
            }
            return new SectionData(".rdata", m, SectionFlags.IMAGE_SCN_CNT_INITIALIZED_DATA | SectionFlags.IMAGE_SCN_MEM_READ);
        }

        public void Add(byte[] data, object? annotation = null)
        {
            if (annotation != null)
                _annotations.Add(new Annotation(_address, annotation));
            _bytes.Add(data);
            _address += (uint)data.Length;
        }

        private record Annotation(uint Address, object Value);

        public int EntryCount => _bytes.Count;
    }
}
