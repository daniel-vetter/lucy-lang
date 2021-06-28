using Lucy.Assembler.Infrastructure;
using System.Linq;

namespace Lucy.Assembler.ContainerFormats.Flat
{
    public static class FlatBinaryBuilder
    {
        public static byte[] Build(AsmModule module, OperandSize defaultOperandSize)
        {
            var m = new MemoryBlock();
            var w = new MachineCodeWriter(defaultOperandSize, m);
            foreach(var operation in module.Stataments.OfType<Operation>())
                operation.Write(w);

            var labelIndex = m.Annotations
                .Where(x => x.Content is AsmLabelAnnotation)
                .ToDictionary(x => ((AsmLabelAnnotation)x.Content).Key, x => x.Address);

            foreach(var annotation in m.Annotations.Where(x => x.Content is AsmLabelRequestAnnotation))
            {
                var requestKey = ((AsmLabelRequestAnnotation)annotation.Content).Key;
                var requestAddress = annotation.Address;

                labelIndex.TryGetValue(requestKey, out var labelAddress);
                m.Address = requestAddress;
                m.WriteUInt32(labelAddress);
            }

            return m.ToArray();
        }
    }
}
