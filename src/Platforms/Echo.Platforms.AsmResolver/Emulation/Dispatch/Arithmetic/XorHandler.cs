using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>xpr</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Xor)]
    public class XorHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, 
            StackSlot argument1, StackSlot argument2)
        {
            argument1.Contents.AsSpan().Xor(argument2.Contents.AsSpan());
            return CilDispatchResult.Success();
        }
    }
}