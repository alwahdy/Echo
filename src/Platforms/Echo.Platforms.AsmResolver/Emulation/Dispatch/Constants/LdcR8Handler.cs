using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldc.r8</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldc_R8)]
    public class LdcR8Handler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var value = context.Machine.ValueFactory.BitVectorPool.Rent(64, false);
            value.AsSpan().Write((double) instruction.Operand!);
            
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(value, StackSlotTypeHint.Float));
            
            return CilDispatchResult.Success();
        }
    }
}