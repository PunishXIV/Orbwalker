using ECommons.Configuration;
using ECommons.GameHelpers;
using ECommons.Gamepad;
using ECommons.Reflection;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using PunishLib;
using System.Windows.Forms;

namespace Orbwalker
{
    public unsafe class Orbwalker : IDalamudPlugin
    {
        public string Name => "Orbwalker";
        internal static Orbwalker P;
        internal Memory Memory;
        internal Config Config;
        bool WasCancelled = false;
        internal bool ShouldUnlock = false;
        bool IsReleaseButtonHeld = false;
        internal DelayedAction DelayedAction = null;
        internal static Config C => P.Config;
        internal long BlockMovementUntil = 0;

        public Orbwalker(IDalamudPluginInterface pluginInterface)
        {
            P = this;
            ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
            PunishLibMain.Init(pluginInterface, this.Name, null, PunishOption.DefaultKoFi);
            new TickScheduler(delegate
            {
                Memory = new();
                Config = EzConfig.Init<Config>();
                EzConfigGui.Init(UI.Draw);
                EzCmd.Add("/orbwalker", EzConfigGui.Open);
                Svc.Framework.Update += Framework_Update;
                EzConfigGui.WindowSystem.AddWindow(new Overlay());
                Memory.EnableDisableBuffer();
            });
        }

        bool IsCasting()
        {
            if (Player.Object.IsCasting)
            {
                if(Util.CheckTpRetMnt(Player.Object.CastActionId, (ActionType)Player.Object.CastActionType)) return false;
                if (C.BlockMount && Player.Object.CastActionType == (byte)ActionType.Mount)
                {
                    BlockMovementUntil = Environment.TickCount64 + 100;
                    return true;
                }
            }
            
            return C.IsSlideAuto
                ? Svc.Condition[ConditionFlag.Casting]
                : Player.Object.IsCasting && Player.Object.TotalCastTime - Player.Object.CurrentCastTime > Config.Threshold;
        }

        internal bool IsUnlockKeyHeld()
        {
            if (Framework.Instance()->WindowInactive) return false;
            return C.ControllerMode 
                ? C.ReleaseButton != Dalamud.Game.ClientState.GamePad.GamepadButtons.None && (GamePad.IsButtonPressed(C.ReleaseButton) || GamePad.IsButtonHeld(C.ReleaseButton)) 
                : C.ReleaseKey != Keys.None && IsKeyPressed((int) C.ReleaseKey);
        }

        private void Framework_Update(object framework)
        {
            PerformDelayedAction();

            if (C.Enabled && Util.CanUsePlugin())
            {
                UpdateShouldUnlock();

                if (ShouldPreventMovement() && !ShouldUnlock)
                {
                    HandleMovementPrevention();
                }
                else
                {
                    EnableMoving();
                    ResetCancelledMoveKeys();
                }
            }
            else
            {
                MoveManager.EnableMoving();
            }
        }

        private void PerformDelayedAction()
        {
            if (DelayedAction != null && DelayedAction.actionId != 0 && AgentMap.Instance()->IsPlayerMoving == 0 &&
                Player.Available)
            {
                var actionManager = ActionManager.Instance();
                PluginLog.Debug($"Using action {DelayedAction}");
                try
                {
                    DelayedAction.Use();
                }
                catch (Exception e)
                {
                    e.Log();
                }

                DelayedAction = null;
            }
        }

        private void UpdateShouldUnlock()
        {
            if (C.IsHoldToRelease)
            {
                ShouldUnlock = C.UnlockPermanently || IsUnlockKeyHeld();
            }
            else
            {
                if (!IsReleaseButtonHeld && IsUnlockKeyHeld())
                {
                    C.UnlockPermanently = !C.UnlockPermanently;
                }

                ShouldUnlock = C.UnlockPermanently;
                IsReleaseButtonHeld = IsUnlockKeyHeld();
            }
        }

        internal bool IsStronglyLocked => Environment.TickCount64 < BlockMovementUntil || PlayerHasNoMoveStatuses();

        private bool ShouldPreventMovement()
        {
            return IsCastingOrDelayedAction() || IsCastableActionWithLowGCD() || IsInCombatWithLowGCDAndNotUnusableAction() || IsStronglyLocked;
        }

        bool PlayerHasNoMoveStatuses()
        {
            var blockList = Util.GetMovePreventionStatuses();
            if (Player.Available && Player.Status.Any(x => x.StatusId.EqualsAny(blockList))) return true;
            return false;
        }

        private bool IsCastingOrDelayedAction()
        {
            return IsCasting() || DelayedAction != null;
        }

        const float GCDCutoff = 0.1f;

        private bool IsCastableActionWithLowGCD()
        {
            var qid = ActionQueue.Get()->ActionID;
            return qid != 0 && Util.IsActionCastable(qid) && Util.GetRCorGDC() < GCDCutoff;
        }

        private bool IsInCombatWithLowGCDAndNotUnusableAction()
        {
            var qid = ActionQueue.Get()->ActionID;
            return C.ForceStopMoveCombat && Svc.Condition[ConditionFlag.InCombat] && Util.GetRCorGDC() < GCDCutoff && !(qid != 0 && !Util.IsActionCastable(qid));
        }

        private void HandleMovementPrevention()
        {
            if (!C.DisableMouseDisabling || C.ControllerMode || IsStronglyLocked)
            {
                MoveManager.DisableMoving();
            }
            else
            {
                MoveManager.EnableMoving();
            }

            CancelMoveKeys();
        }

        private void CancelMoveKeys()
        {
            C.MoveKeys.Each(x =>
            {
                if (Svc.KeyState.GetRawValue(x) != 0)
                {
                    Svc.KeyState.SetRawValue(x, 0);
                    WasCancelled = true;
                    InternalLog.Debug($"Cancelling key {x}");
                }
            });
        }

        private void EnableMoving()
        {
            MoveManager.EnableMoving();
        }

        private void ResetCancelledMoveKeys()
        {
            if (WasCancelled)
            {
                WasCancelled = false;
                C.MoveKeys.Each(x =>
                {
                    if (IsKeyPressed((int) x))
                    {
                        DalamudReflector.SetKeyState(x, 3);
                        InternalLog.Debug($"Reenabling key {x}");
                    }
                });
            }
        }

        public void Dispose()
        {
            Svc.Framework.Update -= Framework_Update;
            MoveManager.EnableMoving();
            Memory.Dispose();
            ECommonsMain.Dispose();
        }
    }
}
