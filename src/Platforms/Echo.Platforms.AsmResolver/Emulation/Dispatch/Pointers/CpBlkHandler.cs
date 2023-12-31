using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>cpblk</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Cpblk)]
    public class CpBlkHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            var size = stack.Pop();
            var source = stack.Pop();
            var destination = stack.Pop();

            try
            {
                // Get concrete addresses and size.
                long? sourceAddress = source.Contents.IsFullyKnown
                    ? source.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveSourcePointer(context, instruction, source);
                
                long? destinationAddress = destination.Contents.IsFullyKnown
                    ? destination.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveDestinationPointer(context, instruction, destination);
                
                uint actualSize = size.Contents.IsFullyKnown
                    ? size.Contents.AsSpan().U32
                    : context.Machine.UnknownResolver.ResolveBlockSize(context, instruction, size);
                
                // Check for null addresses.
                if (sourceAddress == 0 || destinationAddress == 0)
                    return CilDispatchResult.NullReference(context);

                // Perform the copy.
                var buffer = new BitVector((int) (actualSize * 8), false);
                
                // If source address is unknown, leave the buffer with unknown bits.
                if (sourceAddress.HasValue)
                    context.Machine.Memory.Read(sourceAddress.Value, buffer);

                // If we don't know where we are writing to, assume it is writing to "somewhere" successfully.
                if (destinationAddress.HasValue)
                    context.Machine.Memory.Write(destinationAddress.Value, buffer);
            }
            finally
            {
                factory.BitVectorPool.Return(destination.Contents);
                factory.BitVectorPool.Return(source.Contents);
                factory.BitVectorPool.Return(size.Contents);
            }
            
            return CilDispatchResult.Success();
        }
    }
}