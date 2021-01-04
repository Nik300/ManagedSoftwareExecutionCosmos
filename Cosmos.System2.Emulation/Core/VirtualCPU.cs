using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Cosmos.System.Emulation
{
    public abstract class VirtualCPU
    {
        public VirtualMemory Cache;
        public byte arch;
        public bool isCacheInit;
        public VirtualCPU(byte arch, bool initCache = false, int CacheSize = 0)
        {
            this.arch = arch;
            isCacheInit = initCache;
            if (isCacheInit && CacheSize > 0)
                Cache = new VirtualMemory();
            else
                isCacheInit = false;
            return;
        }
    }
    public abstract class VirtualCPU32: VirtualCPU
    {
        public abstract uint GetRegData(byte regAddress);
        public abstract bool SetRegData(byte regAddress, uint Data);
        public abstract bool ClearRegData(byte regAddress);
        public abstract void LoadRegisters();
        public VirtualCPU32(bool initCache = false, int CacheSize = 0)
        : base(3, initCache, CacheSize)
        {
        }
    }
    public abstract class VirtualCPU16: VirtualCPU
    {
        public abstract ushort GetRegData(byte regAddress);
        public abstract bool SetRegData(byte regAddress, ushort Data);
        public abstract bool ClearRegData(byte regAddress);
        public VirtualCPU16(bool initCache = false, int CacheSize = 0)
        : base(2, initCache, CacheSize)
        {
        }
    }
    public abstract class VirtualCPU8: VirtualCPU
    {
        public abstract byte GetRegData(byte regAddress);
        public abstract bool SetRegData(byte regAddress, byte Data);
        public abstract bool ClearRegData(byte regAddress);
        public VirtualCPU8(bool initCache = false, int CacheSize = 0)
        : base(1, initCache, CacheSize)
        {
        }
    }
}
