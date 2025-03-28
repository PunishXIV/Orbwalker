using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Hooks;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Action = Lumina.Excel.Sheets.Action;

namespace Orbwalker;

internal unsafe class Memory : IDisposable
{
    // better for preventing mouse movements in both camera modes
    public delegate void MoveOnMousePreventerDelegate(MoveControllerSubMemberForMine* thisx, float wishdir_h, float wishdir_v, char arg4, byte align_with_camera, Vector3* direction);

    [Signature("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C7", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
    private nint forceDisableMovementPtr;
    
    [Signature("48 89 5C 24 ?? 56 41 56 41 57 48 83 EC 20 48 63 C2", DetourName = nameof(InputData_IsInputIDKeyClickedDetour), Fallibility = Fallibility.Infallible)]
    private Hook<InputData_IsInputIDKeyClickedDelegate> InputData_IsInputIDKeyClickedHook;
    
    [Signature("E8 ?? ?? ?? ?? 84 C0 74 35 EB 05", DetourName = nameof(InputData_IsInputIDKeyHeldDetour), Fallibility = Fallibility.Infallible)]
    private Hook<InputData_IsInputIDKeyHeldDelegate> InputData_IsInputIDKeyHeldHook;
    
    [Signature("E8 ?? ?? ?? ?? 33 DB 41 8B D5", DetourName = nameof(InputData_IsInputIDKeyPressedDetour), Fallibility = Fallibility.Infallible)]
    private Hook<InputData_IsInputIDKeyPressedDelegate> InputData_IsInputIDKeyPressedHook;
    
    [Signature("E8 ?? ?? ?? ?? 88 43 0F", DetourName = nameof(InputData_IsInputIDKeyReleasedDetour), Fallibility = Fallibility.Infallible)]
    private Hook<InputData_IsInputIDKeyReleasedDelegate> InputData_IsInputIDKeyReleasedHook;
    
    internal Hook<UseActionDelegate> UseActionHook;
    
    internal Memory()
    {
        UseActionHook = Svc.Hook.HookFromAddress<UseActionDelegate>((nint)ActionManager.MemberFunctionPointers.UseAction, UseActionDetour);
        SignatureHelper.Initialise(this);
        PluginLog.Debug($"forceDisableMovementPtr = {forceDisableMovementPtr:X16}");
        SendAction.Init((targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9) =>
        {
            if (Util.GetMovePreventionActions().Contains(actionId))
            {
                P.BlockMovementUntil = Environment.TickCount64 + 1000;
                PluginLog.Debug($"Blocking movement until {P.BlockMovementUntil} because of action {actionId}");
            }
        });
    }
    internal ref int ForceDisableMovement => ref *(int*)(forceDisableMovementPtr + 4);
    [Signature("48 8b C4 48 89 70 ?? 48 89 78 ?? 55 41 56 41 57", DetourName = nameof(MovementUpdate), Fallibility = Fallibility.Auto)]
    public static Hook<MoveOnMousePreventerDelegate>? MouseAutoMoveHook { get; set; } = null!;

    public void Dispose()
    {
        DisableHooks();
        DisableMouseAutoMoveHook();
        InputData_IsInputIDKeyPressedHook.Dispose();
        InputData_IsInputIDKeyClickedHook.Dispose();
        InputData_IsInputIDKeyHeldHook.Dispose();
        InputData_IsInputIDKeyReleasedHook.Dispose();
        UseActionHook.Disable();
        UseActionHook.Dispose();
    }

    internal void EnableDisableBuffer()
    {
        bool enabled = C.Enabled && C.Buffer;
        if (enabled && !UseActionHook.IsEnabled)
        {
            UseActionHook.Enable();
            PluginLog.Debug("UseActionHook enabled");
        }
        if (!enabled && UseActionHook.IsEnabled)
        {
            UseActionHook.Disable();
            PluginLog.Debug("UseActionHook disabled");
        }
    }

    private bool UseActionDetour(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8)
    {
        if (C.Enabled && C.Buffer && !P.ShouldUnlock && Util.CanUsePlugin())
        {
            try
            {
                InternalLog.Verbose($"{type}, {acId}, {target}");

                if (Svc.Data.GetExcelSheet<Action>().GetRow(acId).NotNull(out Action sheetAct) && sheetAct.TargetArea && sheetAct.Cast100ms > 0)
                    P.BlockMovementUntil = Environment.TickCount64 + (long)(P.Config.GroundedHold * 1000);

                // was ActionType.Spell before, changed because outdated
                if (P.DelayedAction == null && type == ActionType.Action && Util.IsActionCastable(acId) && Util.GCD == 0 && AgentMap.Instance()->IsPlayerMoving && !am->ActionQueued && !Util.CheckTpRetMnt(acId, type))
                {
                    P.DelayedAction = new(acId, type, 0, target, a5, a6, a7, a8);
                    return false;
                }
            }
            catch(Exception e)
            {
                e.Log();
            }
        }
        bool ret = UseActionHook.Original(am, type, acId, target, a5, a6, a7, a8);
        return ret;
    }
    [return: MarshalAs(UnmanagedType.U1)]
    
    public static void MovementUpdate(MoveControllerSubMemberForMine* thisx, float wishdir_h, float wishdir_v, char arg4, byte align_with_camera, Vector3* direction)
    {
        if (thisx->Unk_0x3F != 0)
            return;

        MouseAutoMoveHook.Original(thisx, wishdir_h, wishdir_v, arg4, align_with_camera, direction);
    }
    
    private byte InputData_IsInputIDKeyPressedDetour(nint a1, int key)
    {
        //InternalLog.Verbose($"Pressed: {key}");
        if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
        return InputData_IsInputIDKeyPressedHook.Original(a1, key);
    }
    
    private byte InputData_IsInputIDKeyClickedDetour(nint a1, int key)
    {
        //InternalLog.Verbose($"Clicked: {key}");
        if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
        return InputData_IsInputIDKeyClickedHook.Original(a1, key);
    }
    
    private byte InputData_IsInputIDKeyHeldDetour(nint a1, int key)
    {
        //InternalLog.Verbose($"Held: {key}");
        if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
        return InputData_IsInputIDKeyHeldHook.Original(a1, key);
    }
    
    private byte InputData_IsInputIDKeyReleasedDetour(nint a1, int key)
    {
        //InternalLog.Verbose($"Released: {key}");
        if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
        return InputData_IsInputIDKeyReleasedHook.Original(a1, key);
    }

    internal void EnableHooks()
    {
        InputData_IsInputIDKeyPressedHook.Enable();
        InputData_IsInputIDKeyClickedHook.Enable();
        InputData_IsInputIDKeyHeldHook.Enable();
        InputData_IsInputIDKeyReleasedHook.Enable();
    }

    internal void EnableMouseAutoMoveHook()
    {
        MouseAutoMoveHook.Enable();
    }

    internal void DisableHooks()
    {
        MouseAutoMoveHook.Disable();
        InputData_IsInputIDKeyPressedHook.Disable();
        InputData_IsInputIDKeyClickedHook.Disable();
        InputData_IsInputIDKeyHeldHook.Disable();
        InputData_IsInputIDKeyReleasedHook.Disable();
    }

    internal void DisableMouseAutoMoveHook()
    {
        MouseAutoMoveHook.Disable();
    }
    
    internal delegate bool UseActionDelegate(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
    
    private delegate byte InputData_IsInputIDKeyPressedDelegate(nint a1, int key);
    
    private delegate byte InputData_IsInputIDKeyClickedDelegate(nint a1, int key);
    
    private delegate byte InputData_IsInputIDKeyHeldDelegate(nint a1, int key);

    private delegate byte InputData_IsInputIDKeyReleasedDelegate(nint a1, int key);
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct UnkGameObjectStruct
{
    [FieldOffset(0xD0)] public int Unk_0xD0;
    [FieldOffset(0x101)] public byte Unk_0x101;
    [FieldOffset(0x1C0)] public Vector3 DesiredPosition;
    [FieldOffset(0x1D0)] public float NewRotation;
    [FieldOffset(0x1FC)] public byte Unk_0x1FC;
    [FieldOffset(0x1FF)] public byte Unk_0x1FF;
    [FieldOffset(0x200)] public byte Unk_0x200;
    [FieldOffset(0x2C6)] public byte Unk_0x2C6;
    [FieldOffset(0x3D0)] public GameObject* Actor; // Points to local player
    [FieldOffset(0x3E0)] public byte Unk_0x3E0;
    [FieldOffset(0x3EC)] public float Unk_0x3EC;
    [FieldOffset(0x3F0)] public float Unk_0x3F0;
    [FieldOffset(0x418)] public byte Unk_0x418;
    [FieldOffset(0x419)] public byte Unk_0x419;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct MoveControllerSubMemberForMine
{
    [FieldOffset(0x10)] public Vector3 Direction;
    [FieldOffset(0x20)] public UnkGameObjectStruct* ActorStruct;
    [FieldOffset(0x28)] public float Unk_0x28;
    [FieldOffset(0x38)] public float Unk_0x38;
    [FieldOffset(0x3C)] public byte Moved; // 1 when the character has moved
    [FieldOffset(0x3D)] public byte Rotated; // 1 when the character has rotated
    [FieldOffset(0x3E)] public byte MovementLock;
    [FieldOffset(0x3F)] public byte Unk_0x3F; // non-zero when moving with LMB+RMB
    [FieldOffset(0x40)] public byte Unk_0x40;
    [FieldOffset(0x44)] public float MoveSpeed;
    [FieldOffset(0x50)] public float* MoveSpeedMaximums;
    [FieldOffset(0x80)] public Vector3 ZoningPosition;
    [FieldOffset(0x90)] public float MoveDir;
    [FieldOffset(0x94)] public byte Unk_0x94;
    [FieldOffset(0xA0)] public Vector3 MoveForward; // direction output by MovementUpdate
    [FieldOffset(0xB0)] public float Unk_0xB0;
    [FieldOffset(0xB4)] public byte Unk_0xB4;
    [FieldOffset(0xF2)] public byte Unk_0xF2;
    [FieldOffset(0xF3)] public byte Unk_0xF3;
    [FieldOffset(0xF4)] public byte Unk_0xF4;
    [FieldOffset(0xF5)] public byte Unk_0xF5;
    [FieldOffset(0xF6)] public byte Unk_0xF6;
    [FieldOffset(0x104)] public byte Unk_0x104;
    [FieldOffset(0x110)] public Int32 WishdirChanged;
    [FieldOffset(0x114)] public float Wishdir_Horizontal;
    [FieldOffset(0x118)] public float Wishdir_Vertical;
    [FieldOffset(0x120)] public byte Unk_0x120;
    [FieldOffset(0x121)] public byte Rotated1;
    [FieldOffset(0x122)] public byte Unk_0x122;
    [FieldOffset(0x123)] public byte Unk_0x123;
    [FieldOffset(0x125)] public byte Unk_0x125;
    [FieldOffset(0x12A)] public byte Unk_0x12A;
}
