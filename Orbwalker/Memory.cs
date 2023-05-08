using Dalamud.Game.ClientState.GamePad;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbwalker
{
    internal unsafe class Memory : IDisposable
    {
        internal delegate bool UseActionDelegate(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
        internal Hook<UseActionDelegate> UseActionHook;
        internal Memory()
        {
            UseActionHook = Hook<UseActionDelegate>.FromAddress((nint)ActionManager.Addresses.UseAction.Value, UseActionDetour);
            SignatureHelper.Initialise(this);
            PluginLog.Debug($"forceDisableMovementPtr = {forceDisableMovementPtr:X16}");
        }

        internal void EnableDisableBuffer()
        {
            var enabled = P.Config.Enabled && P.Config.Buffer;
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
            if (P.Config.Enabled && P.Config.Buffer && !P.ShouldUnlock)
            {
                try
                {
                    InternalLog.Verbose($"{type}, {acId}, {target}");
                    if (P.DelayedAction == null && type == ActionType.Spell && Util.IsActionCastable(acId) && Util.GCD == 0 && AgentMap.Instance()->IsPlayerMoving != 0 && !am->ActionQueued)
                    {
                        P.DelayedAction = new(acId, 0, target, a5, a6, a7, a8);
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

        [Signature("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C6 0F 8A", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
        private nint forceDisableMovementPtr;
        internal ref int ForceDisableMovement => ref *(int*)(forceDisableMovementPtr + 4);

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
            
            InputData_IsInputIDKeyPressedHook.Enable();
            InputData_IsInputIDKeyClickedHook.Enable();
            InputData_IsInputIDKeyHeldHook.Enable();
            InputData_IsInputIDKeyReleasedHook.Enable();
        }

        internal void DisableHooks()
        {
            InputData_IsInputIDKeyPressedHook.Disable();
            InputData_IsInputIDKeyClickedHook.Disable();
            InputData_IsInputIDKeyHeldHook.Disable();
            InputData_IsInputIDKeyReleasedHook.Disable();
        }

        public void Dispose()
        {
            DisableHooks();
            InputData_IsInputIDKeyPressedHook.Dispose();
            InputData_IsInputIDKeyClickedHook.Dispose();
            InputData_IsInputIDKeyHeldHook.Dispose();
            InputData_IsInputIDKeyReleasedHook.Dispose();
            UseActionHook.Disable();
            UseActionHook.Dispose();
        }
    }
}
