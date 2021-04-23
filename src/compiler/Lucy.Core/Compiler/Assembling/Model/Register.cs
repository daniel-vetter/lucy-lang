using System.Collections.Immutable;

namespace Disassembler.Assembling
{
    public record Register(byte Index, string Name, OperandSize Size) : RegisterOrMemory
    {
        public override string ToString() => Name;

        private static ImmutableDictionary<OperandSize, ImmutableArray<Register>> _r;

        static Register()
        {
            var b = ImmutableDictionary.CreateBuilder<OperandSize, ImmutableArray<Register>>();
            b[OperandSize.S64] = ImmutableArray.Create(RAX, RCX, RDX, RBX, RSP, RBP, RSI, RDI);
            b[OperandSize.S32] = ImmutableArray.Create(EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI);
            b[OperandSize.S16] = ImmutableArray.Create(AX, CX, DX, BX, SP, BP, SI, DI);
            b[OperandSize.S8] = ImmutableArray.Create(AL, CL, DL, BL, AH, CH, DH, BH);
            _r = b.ToImmutable();
        }

        public static Register FromIndex(byte index, OperandSize size) => _r[size][index];
        public static Register FromIndex(int index, OperandSize size) => _r[size][index];

        public static ImmutableArray<Register> FromOperandSize(params OperandSize[] size)
        {
            var b = ImmutableArray.CreateBuilder<Register>();
            foreach (var s in size)
                b.AddRange(_r[s]);
            return b.ToImmutable();
        }

        public static Register AL = new Register(0, "al", OperandSize.S8);
        public static Register CL = new Register(1, "cl", OperandSize.S8);
        public static Register DL = new Register(2, "dl", OperandSize.S8);
        public static Register BL = new Register(3, "bl", OperandSize.S8);
        public static Register AH = new Register(4, "ah", OperandSize.S8);
        public static Register CH = new Register(5, "ch", OperandSize.S8);
        public static Register DH = new Register(6, "dh", OperandSize.S8);
        public static Register BH = new Register(7, "bh", OperandSize.S8);

        public static Register AX = new Register(0, "ax", OperandSize.S16);
        public static Register CX = new Register(1, "cx", OperandSize.S16);
        public static Register DX = new Register(2, "dx", OperandSize.S16);
        public static Register BX = new Register(3, "bx", OperandSize.S16);
        public static Register SP = new Register(4, "sp", OperandSize.S16);
        public static Register BP = new Register(5, "bp", OperandSize.S16);
        public static Register SI = new Register(6, "si", OperandSize.S16);
        public static Register DI = new Register(7, "di", OperandSize.S16);

        /// <summary>
        /// Accumulator for operands and results data
        /// </summary>
        public static Register EAX = new Register(0, "eax", OperandSize.S32);

        /// <summary>
        /// Count for string and loop operations
        /// </summary>
        public static Register ECX = new Register(1, "ecx", OperandSize.S32);

        /// <summary>
        /// I/O pointer
        /// </summary>
        public static Register EDX = new Register(2, "edx", OperandSize.S32);

        /// <summary>
        /// Pointer to data in the DS segment
        /// </summary>
        public static Register EBX = new Register(3, "ebx", OperandSize.S32);

        /// <summary>
        /// Stack pointer (in the SS segment)
        /// </summary>
        public static Register ESP = new Register(4, "esp", OperandSize.S32);

        /// <summary>
        /// Pointer to data in the stack (in the SS segment)
        /// </summary>
        public static Register EBP = new Register(5, "ebp", OperandSize.S32);

        /// <summary>
        /// Pointer to data in the segment pointed to by the DS register; source pointer for string operations
        /// </summary>
        public static Register ESI = new Register(6, "esi", OperandSize.S32);

        /// <summary>
        /// Pointer to data (or destination) in the segment pointed to by the ES register; destination pointer for string operations
        /// </summary>
        public static Register EDI = new Register(7, "edi", OperandSize.S32);


        public static Register RAX = new Register(0, "rax", OperandSize.S64);
        public static Register RCX = new Register(1, "rcx", OperandSize.S64);
        public static Register RDX = new Register(2, "rdx", OperandSize.S64);
        public static Register RBX = new Register(3, "rbx", OperandSize.S64);
        public static Register RSP = new Register(4, "rsp", OperandSize.S64);
        public static Register RBP = new Register(5, "rbp", OperandSize.S64);
        public static Register RSI = new Register(6, "rsi", OperandSize.S64);
        public static Register RDI = new Register(7, "rdi", OperandSize.S64);
    }
}
