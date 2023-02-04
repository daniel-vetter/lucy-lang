using System.Collections.Immutable;

namespace Lucy.Assembler
{
    public record Register(byte Index, string Name, OperandSize Size) : IRegisterOrMemory
    {
        public override string ToString() => Name;

        private static readonly ImmutableDictionary<OperandSize, ImmutableArray<Register>> _r;

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

        public static ImmutableArray<Register> All
        {
            get
            {
                var b = ImmutableArray.CreateBuilder<Register>();
                foreach (var group in _r)
                foreach (var r in group.Value)
                    b.Add(r);
                return b.ToImmutable();
            }
        }

        // ReSharper disable InconsistentNaming
        public static Register AL { get; } = new(0, "al", OperandSize.S8);
        public static Register CL { get; } = new(1, "cl", OperandSize.S8);
        public static Register DL { get; } = new(2, "dl", OperandSize.S8);
        public static Register BL { get; } = new(3, "bl", OperandSize.S8);
        public static Register AH { get; } = new(4, "ah", OperandSize.S8);
        public static Register CH { get; } = new(5, "ch", OperandSize.S8);
        public static Register DH { get; } = new(6, "dh", OperandSize.S8);
        public static Register BH { get; } = new(7, "bh", OperandSize.S8);

        public static Register AX { get; } = new(0, "ax", OperandSize.S16);
        public static Register CX { get; } = new(1, "cx", OperandSize.S16);
        public static Register DX { get; } = new(2, "dx", OperandSize.S16);
        public static Register BX { get; } = new(3, "bx", OperandSize.S16);
        public static Register SP { get; } = new(4, "sp", OperandSize.S16);
        public static Register BP { get; } = new(5, "bp", OperandSize.S16);
        public static Register SI { get; } = new(6, "si", OperandSize.S16);

        public static Register DI { get; } = new(7, "di", OperandSize.S16);
        

        /// <summary>
        /// Accumulator for operands and results data
        /// </summary>
        public static Register EAX { get; }  = new(0, "eax", OperandSize.S32);

        /// <summary>
        /// Count for string and loop operations
        /// </summary>
        public static Register ECX { get; }  = new(1, "ecx", OperandSize.S32);

        /// <summary>
        /// I/O pointer
        /// </summary>
        public static Register EDX { get; }  = new(2, "edx", OperandSize.S32);

        /// <summary>
        /// Pointer to data in the DS segment
        /// </summary>
        public static Register EBX { get; }  = new(3, "ebx", OperandSize.S32);

        /// <summary>
        /// Stack pointer (in the SS segment)
        /// </summary>
        public static Register ESP { get; }  = new(4, "esp", OperandSize.S32);

        /// <summary>
        /// Pointer to data in the stack (in the SS segment)
        /// </summary>
        public static Register EBP { get; }  = new(5, "ebp", OperandSize.S32);

        /// <summary>
        /// Pointer to data in the segment pointed to by the DS register; source pointer for string operations
        /// </summary>
        public static Register ESI { get; }  = new(6, "esi", OperandSize.S32);

        /// <summary>
        /// Pointer to data (or destination) in the segment pointed to by the ES register; destination pointer for string operations
        /// </summary>
        public static Register EDI { get; }  = new(7, "edi", OperandSize.S32);


        public static Register RAX { get; } = new(0, "rax", OperandSize.S64);
        public static Register RCX { get; } = new(1, "rcx", OperandSize.S64);
        public static Register RDX { get; } = new(2, "rdx", OperandSize.S64);
        public static Register RBX { get; } = new(3, "rbx", OperandSize.S64);
        public static Register RSP { get; } = new(4, "rsp", OperandSize.S64);
        public static Register RBP { get; } = new(5, "rbp", OperandSize.S64);
        public static Register RSI { get; } = new(6, "rsi", OperandSize.S64);
        public static Register RDI { get; } = new(7, "rdi", OperandSize.S64);
        // ReSharper restore InconsistentNaming
    }
}