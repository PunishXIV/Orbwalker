using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unmoveable
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct ActionQueue
    {
        [FieldOffset(0)]
        internal int QueueStatus;

        [FieldOffset(4)]
        internal ActionType ActionType;

        [FieldOffset(8)]
        internal uint ActionID;

        [FieldOffset(16)]
        internal long TargetID;

        [FieldOffset(24)]
        internal int Origin;

        [FieldOffset(28)]
        internal int Unk84;

        public static ActionQueue* Get()
        {
            return (ActionQueue*)((IntPtr)ActionManager.Instance() + 0x68);
        }
    }
}
