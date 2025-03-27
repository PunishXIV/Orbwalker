using FFXIVClientStructs.FFXIV.Client.Game;
namespace Orbwalker;

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

    public static ActionQueue* Get() => (ActionQueue*)((nint)ActionManager.Instance() + 0x68);
}
