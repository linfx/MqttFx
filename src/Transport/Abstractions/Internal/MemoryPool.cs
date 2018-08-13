using System.Buffers;

namespace nMqtt.Transport.Abstractions.Internal
{
    public static class MemoryPool
    {
        public static MemoryPool<byte> Create()
        {
#if DEBUG
            return new DiagnosticMemoryPool(CreateSlabMemoryPool());
#else
            return CreateSlabMemoryPool();
#endif
        }

        public static MemoryPool<byte> CreateSlabMemoryPool()
        {
            return new SlabMemoryPool();
        }

        public static readonly int MinimumSegmentSize = 4096;
    }
}
