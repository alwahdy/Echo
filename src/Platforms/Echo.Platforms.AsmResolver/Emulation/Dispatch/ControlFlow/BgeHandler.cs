using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>bge</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Bge, CilCode.Bge_S, CilCode.Bge_Un, CilCode.Bge_Un_S)]
    public class BgeHandler : BinaryBranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedCondition(CilInstruction instruction) =>
            instruction.OpCode.Code is CilCode.Bge or CilCode.Bge_S;

        /// <inheritdoc />
        protected override Trilean EvaluateCondition(CilInstruction instruction, 
            StackSlot argument1,
            StackSlot argument2)
        {
            var contents1 = argument1.Contents.AsSpan();
            var contents2 = argument2.Contents.AsSpan();

            bool isSigned = IsSignedCondition(instruction);
            if (argument1.TypeHint == StackSlotTypeHint.Integer)
                return contents1.IntegerIsGreaterThanOrEqual(contents2, isSigned);

            return contents1.IsEqualTo(contents2) || contents1.FloatIsGreaterThan(contents2, isSigned);
        }
    }
}