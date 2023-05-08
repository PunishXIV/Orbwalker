using ECommons.Configuration;
using ECommons.GameHelpers;
using ECommons.Gamepad;
using ECommons.Reflection;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
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

        public Orbwalker(DalamudPluginInterface pluginInterface)
        {
            P = this;
            ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
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
            return P.Config.IsSlideAuto
                ? Svc.Condition[ConditionFlag.Casting]
                : Player.Object.IsCasting && Player.Object.TotalCastTime - Player.Object.CurrentCastTime > Config.Threshold;
        }

        internal bool IsUnlockKeyHeld()
        {
            if (Framework.Instance()->WindowInactive) return false;
            return P.Config.ControllerMode 
                ? P.Config.ReleaseButton != Dalamud.Game.ClientState.GamePad.GamepadButtons.None && (GamePad.IsButtonPressed(P.Config.ReleaseButton) || GamePad.IsButtonHeld(P.Config.ReleaseButton)) 
                : P.Config.ReleaseKey != Keys.None && IsKeyPressed(P.Config.ReleaseKey);
        }

        private void Framework_Update(Dalamud.Game.Framework framework)
        {
            PerformDelayedAction();

            if (P.Config.Enabled && Util.CanUsePlugin())
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
            if (P.Config.IsHoldToRelease)
            {
                ShouldUnlock = P.Config.UnlockPermanently || IsUnlockKeyHeld();
            }
            else
            {
                if (!IsReleaseButtonHeld && IsUnlockKeyHeld())
                {
                    P.Config.UnlockPermanently = !P.Config.UnlockPermanently;
                }

                ShouldUnlock = P.Config.UnlockPermanently;
                IsReleaseButtonHeld = IsUnlockKeyHeld();
            }
        }

        private bool ShouldPreventMovement()
        {
            return IsCastingOrDelayedAction() || IsCastableActionWithLowGCD() || IsInCombatWithLowGCDAndNotUnusableAction();
        }

        private bool IsCastingOrDelayedAction()
        {
            return IsCasting() || DelayedAction != null;
        }

        private bool IsCastableActionWithLowGCD()
        {
            var qid = ActionQueue.Get()->ActionID;
            return qid != 0 && Util.IsActionCastable(qid) && Util.GetRCorGDC() < 0.01;
        }

        private bool IsInCombatWithLowGCDAndNotUnusableAction()
        {
            var qid = ActionQueue.Get()->ActionID;
            return P.Config.ForceStopMoveCombat && Svc.Condition[ConditionFlag.InCombat] && Util.GetRCorGDC() < 0.01 && !(qid != 0 && !Util.IsActionCastable(qid));
        }

        private void HandleMovementPrevention()
        {
            if ((!P.Config.DisableMouseDisabling && Util.IsMouseMoveOrdered()) || P.Config.ControllerMode)
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
            P.Config.MoveKeys.Each(x =>
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
                P.Config.MoveKeys.Each(x =>
                {
                    if (IsKeyPressed((Keys)x))
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
