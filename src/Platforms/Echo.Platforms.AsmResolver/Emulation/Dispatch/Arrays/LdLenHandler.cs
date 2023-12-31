using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldlen</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldlen)]
    public class LdLenHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var arrayAddress = stack.Pop().Contents;
            var result = factory.RentNativeInteger(true);
            
            try
            {
                var arrayAddressSpan = arrayAddress.AsSpan();
                switch (arrayAddressSpan)
                {
                    case { IsFullyKnown: false }:
                        result.AsSpan().MarkFullyUnknown();
                        break;

                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);

                    default:
                        arrayAddressSpan.AsObjectHandle(context.Machine).ReadArrayLength(result.AsSpan(0, 32));
                        break;
                }

                stack.Push(new StackSlot(result, StackSlotTypeHint.Integer));
                return CilDispatchResult.Success();
            }
            finally
            {
                factory.BitVectorPool.Return(arrayAddress);
            }
        }
    }
}