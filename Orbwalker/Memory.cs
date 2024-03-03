using Dalamud.Game.ClientState.GamePad;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.Hooks;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using MButtonHoldState = FFXIVClientStructs.FFXIV.Client.Game.Control.InputManager.MouseButtonHoldState;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace Orbwalker
{
    internal unsafe class Memory : IDisposable
    {
        internal delegate bool UseActionDelegate(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
        internal Hook<UseActionDelegate> UseActionHook;
        internal Memory()
        {
            UseActionHook = Svc.Hook.HookFromAddress<UseActionDelegate>((nint)ActionManager.Addresses.UseAction.Value, UseActionDetour);
            SignatureHelper.Initialise(this);
            //PluginLog.Debug($"forceDisableMovementPtr = {forceDisableMovementPtr:X16}");
            SendAction.Init((long targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9) =>
            {
                if (Util.GetMovePreventionActions().Contains(actionId))
                {
                    P.BlockMovementUntil = Environment.TickCount64 + 1000;
                    PluginLog.Debug($"Blocking movement until {P.BlockMovementUntil} because of action {actionId}");
                }
            });
        }

        internal void EnableDisableBuffer()
        {
            var enabled = C.Enabled && C.Buffer;
            if(enabled && !UseActionHook.IsEnabled)
            {
                UseActionHook.Enable();
                PluginLog.Debug($"UseActionHook enabled");
            }
            if (!enabled && UseActionHook.IsEnabled)
            {
                UseActionHook.Disable();
                PluginLog.Debug($"UseActionHook disabled");
            }
        }

        private bool UseActionDetour(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8)
        {
            if (C.Enabled && C.Buffer && !P.ShouldUnlock && Util.CanUsePlugin())
            {
                try
                {
                    InternalLog.Verbose($"{type}, {acId}, {target}");

                    if (Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().TryGetFirst(x => x.RowId == acId, out var sheetAct) && sheetAct.TargetArea && sheetAct.Cast100ms > 0)
                        P.BlockMovementUntil = Environment.TickCount64 + (long)(P.Config.GroundedHold * 1000);

                    // was ActionType.Spell before, changed because outdated
                    if (P.DelayedAction == null && ((type == ActionType.Action && Util.IsActionCastable(acId)) || type == ActionType.Mount) && Util.GCD == 0 && AgentMap.Instance()->IsPlayerMoving != 0 && !am->ActionQueued && !Util.CheckTpRetMnt(acId, type))
                    {
                        P.DelayedAction = new(acId, type, 0, target, a5, a6, a7, a8);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    e.Log();
                }
            }
            var ret = UseActionHook.Original(am, type, acId, target, a5, a6, a7, a8);
            return ret;
        }

        // [Signature("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C6 0F 8A", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
        // private nint forceDisableMovementPtr;
        // internal ref int ForceDisableMovement => ref *(int*)(forceDisableMovementPtr + 4);

        // better for preventing mouse movements in both camera modes
        public unsafe delegate byte MoveOnMousePreventorDelegate(MoveControllerSubMemberForMine* thisx);
        [Signature("40 55 53 48 8d 6c 24 c8 48 81 ec 38 01 00 00", DetourName = nameof(MovementUpdate), Fallibility = Fallibility.Auto)]
        private static Hook<MoveOnMousePreventorDelegate>? MouseMovePreventerHook { get; set; } = null!;
        [return: MarshalAs(UnmanagedType.U1)]
        public unsafe byte MovementUpdate(MoveControllerSubMemberForMine* thisx) { // was static before.
            // get the current mouse button hole state, note that because we are doing this during the move update,
            // we are getting and updating the mouse state PRIOR to the game doing so, allowing us to change it
            MButtonHoldState* hold = InputManager.GetMouseButtonHoldState();
            MButtonHoldState original = *hold;
            // modify the hold state
            if (*hold == (MButtonHoldState.Left | MButtonHoldState.Right)) {
                *hold = 0;
            }
            //PluginLog.Debug($"{((IntPtr)hold).ToString("X")}");
            // update the original
            byte ret = MouseMovePreventerHook.Original(thisx);
            // restore the original
            *hold = original;
            // return 
            return ret;
        }




        delegate byte InputData_IsInputIDKeyPressedDelegate(nint a1, int key);
        [Signature("E8 ?? ?? ?? ?? 84 C0 48 63 03", DetourName =nameof(InputData_IsInputIDKeyPressedDetour), Fallibility = Fallibility.Infallible)]
        Hook<InputData_IsInputIDKeyPressedDelegate> InputData_IsInputIDKeyPressedHook;
        byte InputData_IsInputIDKeyPressedDetour(nint a1, int key)
        {
            //InternalLog.Verbose($"Pressed: {key}");
            if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
            return InputData_IsInputIDKeyPressedHook.Original(a1, key);
        }


        delegate byte InputData_IsInputIDKeyClickedDelegate(nint a1, int key);
        [Signature("E9 ?? ?? ?? ?? 83 7F 44 02", DetourName = nameof(InputData_IsInputIDKeyClickedDetour), Fallibility = Fallibility.Infallible)]
        Hook<InputData_IsInputIDKeyClickedDelegate> InputData_IsInputIDKeyClickedHook;
        byte InputData_IsInputIDKeyClickedDetour(nint a1, int key)
        {
            //InternalLog.Verbose($"Clicked: {key}");
            if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
            return InputData_IsInputIDKeyClickedHook.Original(a1, key);
        }


        delegate byte InputData_IsInputIDKeyHeldDelegate(nint a1, int key);
        [Signature("E8 ?? ?? ?? ?? 84 C0 74 08 85 DB", DetourName = nameof(InputData_IsInputIDKeyHeldDetour), Fallibility = Fallibility.Infallible)]
        Hook<InputData_IsInputIDKeyHeldDelegate> InputData_IsInputIDKeyHeldHook;
        byte InputData_IsInputIDKeyHeldDetour(nint a1, int key)
        {
            //InternalLog.Verbose($"Held: {key}");
            if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
            return InputData_IsInputIDKeyHeldHook.Original(a1, key);
        }


        delegate byte InputData_IsInputIDKeyReleasedDelegate(nint a1, int key);
        [Signature("E8 ?? ?? ?? ?? 88 43 0F", DetourName = nameof(InputData_IsInputIDKeyReleasedDetour), Fallibility = Fallibility.Infallible)]
        Hook<InputData_IsInputIDKeyReleasedDelegate> InputData_IsInputIDKeyReleasedHook;
        byte InputData_IsInputIDKeyReleasedDetour(nint a1, int key)
        {
            //InternalLog.Verbose($"Released: {key}");
            if (key.EqualsAny(MoveManager.BlockedKeys)) return 0;
            return InputData_IsInputIDKeyReleasedHook.Original(a1, key);
        }

        internal void EnableHooks()
        {
            MouseMovePreventerHook.Enable();            
            InputData_IsInputIDKeyPressedHook.Enable();
            InputData_IsInputIDKeyClickedHook.Enable();
            InputData_IsInputIDKeyHeldHook.Enable();
            InputData_IsInputIDKeyReleasedHook.Enable();
        }

        internal void DisableHooks()
        {
            MouseMovePreventerHook.Disable();
            InputData_IsInputIDKeyPressedHook.Disable();
            InputData_IsInputIDKeyClickedHook.Disable();
            InputData_IsInputIDKeyHeldHook.Disable();
            InputData_IsInputIDKeyReleasedHook.Disable();
        }

        public void Dispose()
        {
            DisableHooks();
            MouseMovePreventerHook.Dispose();
            InputData_IsInputIDKeyPressedHook.Dispose();
            InputData_IsInputIDKeyClickedHook.Dispose();
            InputData_IsInputIDKeyHeldHook.Dispose();
            InputData_IsInputIDKeyReleasedHook.Dispose();
            UseActionHook.Disable();
            UseActionHook.Dispose();
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UnkGameObjectStruct {
        [FieldOffset(0xD0)] public int Unk_0xD0;
        [FieldOffset(0x101)] public byte Unk_0x101;
        [FieldOffset(0x1C0)] public Vector3 DesiredPosition;
        [FieldOffset(0x1D0)] public float NewRotation;
        [FieldOffset(0x1FC)] public byte Unk_0x1FC;
        [FieldOffset(0x1FF)] public byte Unk_0x1FF;
        [FieldOffset(0x200)] public byte Unk_0x200;
        [FieldOffset(0x2C6)] public byte Unk_0x2C6;
        [FieldOffset(0x3D0)] public GameObject* Actor; // points to local player
        [FieldOffset(0x3E0)] public byte Unk_0x3E0;
        [FieldOffset(0x3EC)] public float Unk_0x3EC;
        [FieldOffset(0x3F0)] public float Unk_0x3F0;
        [FieldOffset(0x418)] public byte Unk_0x418;
        [FieldOffset(0x419)] public byte Unk_0x419;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MoveControllerSubMemberForMine {
        [FieldOffset(0x10)] public Vector3 Direction;
        [FieldOffset(0x20)] public UnkGameObjectStruct* ActorStruct;
        [FieldOffset(0x28)] public uint Unk_0x28;
        [FieldOffset(0x3C)] public byte Moved;
        [FieldOffset(0x3D)] public byte Rotated;
        [FieldOffset(0x3E)] public byte MovementLock;
        [FieldOffset(0x3F)] public byte Unk_0x3F;
        [FieldOffset(0x40)] public byte Unk_0x40;
        [FieldOffset(0x80)] public Vector3 ZoningPosition;
        [FieldOffset(0xF4)] public byte Unk_0xF4;
        [FieldOffset(0x80)] public Vector3 Unk_0x80;
        [FieldOffset(0x90)] public float MoveDir;
        [FieldOffset(0x94)] public byte Unk_0x94;
        [FieldOffset(0xA0)] public Vector3 MoveForward;
        [FieldOffset(0xB0)] public float Unk_0xB0;
        [FieldOffset(0x104)] public byte Unk_0x104;
        [FieldOffset(0x110)] public Int32 WishdirChanged;
        [FieldOffset(0x114)] public float Wishdir_Horizontal;
        [FieldOffset(0x118)] public float Wishdir_Vertical;
        [FieldOffset(0x120)] public byte Unk_0x120;
        [FieldOffset(0x121)] public byte Rotated1;
        [FieldOffset(0x122)] public byte Unk_0x122;
        [FieldOffset(0x123)] public byte Unk_0x123;
    }
}
