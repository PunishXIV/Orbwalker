using Dalamud.Game.ClientState.GamePad;
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

namespace Orbwalker;

public unsafe class Orbwalker : IDalamudPlugin
{
    private const float GCDCutoff = 0.1f;
    internal static Orbwalker P;
    internal long BlockMovementUntil = 0;
    internal Config Config;
    internal DelayedAction DelayedAction;
    internal IPC IPC;
    private bool IsReleaseButtonHeld;
    internal Memory Memory;
    internal bool ShouldUnlock;
    internal bool ShouldBlock;
    private bool WasCancelled;

    public Orbwalker(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
        PunishLibMain.Init(pluginInterface, Name, null, PunishOption.DefaultKoFi);
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
        IPC = new();
    }
    public string Name => "Orbwalker";
    internal static Config C => P.Config;

    internal bool IsStronglyLocked => Environment.TickCount64 < BlockMovementUntil || PlayerHasNoMoveStatuses();

    public void Dispose()
    {
        Svc.Framework.Update -= Framework_Update;
        MoveManager.EnableMoving();
        MoveManager.EnableMouseMoving();
        Memory.Dispose();
        ECommonsMain.Dispose();
    }

    private bool IsCasting()
    {
        if (Player.Object.IsCasting)
        {
            if (Util.CheckTpRetMnt(Player.Object.CastActionId, (ActionType)Player.Object.CastActionType) || Util.CastingWalkableAction()) return false;
        }

        return C.IsSlideAuto
            ? Svc.Condition[ConditionFlag.Casting]
            : Player.Object.IsCasting && Player.Object.TotalCastTime - Player.Object.CurrentCastTime > Config.Threshold;
    }

    internal bool IsUnlockKeyHeld()
    {
        if (Framework.Instance()->WindowInactive) return false;
        return C.ControllerMode
            ? C.ReleaseButton != GamepadButtons.None && (GamePad.IsButtonPressed(C.ReleaseButton) || GamePad.IsButtonHeld(C.ReleaseButton))
            : C.ReleaseKey != Keys.None && IsKeyPressed((int)C.ReleaseKey);
    }

    internal bool IsBlockKeyHeld()
    {
        if (Framework.Instance()->WindowInactive) return false;
        return C.ControllerMode
            ? C.BlockButton != GamepadButtons.None && (GamePad.IsButtonPressed(C.BlockButton) || GamePad.IsButtonHeld(C.BlockButton))
            : C.BlockKey != Keys.None && IsKeyPressed((int)C.BlockKey);
    }

    private void Framework_Update(object framework)
    {
        PerformDelayedAction();

        if (C.Enabled && Util.CanUsePlugin())
        {
            UpdateShouldUnlock();
            UpdateShouldBlock();

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
            MoveManager.EnableMouseMoving();
        }
    }

    private void PerformDelayedAction()
    {
        if (DelayedAction != null && DelayedAction.actionId != 0 && !AgentMap.Instance()->IsPlayerMoving &&
            Player.Available)
        {
            ActionManager* actionManager = ActionManager.Instance();
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
    private void UpdateShouldBlock() => ShouldBlock = C.BlockAllMovement && IsBlockKeyHeld() && !IsUnlockKeyHeld();

    private bool ShouldPreventMovement() => IsCastingOrDelayedAction() || IsCastableActionWithLowGCD() || IsInCombatWithLowGCDAndNotUnusableAction() || IsStronglyLocked || ShouldBlock;

    private bool PlayerHasNoMoveStatuses()
    {
        IEnumerable<uint> blockList = Util.GetMovePreventionStatuses();
        if (Player.Available && Player.Status.Any(x => x.StatusId.EqualsAny(blockList))) return true;
        return false;
    }

    private bool IsCastingOrDelayedAction() => IsCasting() || DelayedAction != null;

    private bool IsCastableActionWithLowGCD()
    {
        uint qid = ActionQueue.Get()->ActionID;
        return qid != 0 && Util.IsActionCastable(qid) && Util.GetRCorGDC() < GCDCutoff;
    }

    private bool IsInCombatWithLowGCDAndNotUnusableAction()
    {
        uint qid = ActionQueue.Get()->ActionID;
        return C.ForceStopMoveCombat && Svc.Condition[ConditionFlag.InCombat] && Util.GetRCorGDC() < GCDCutoff && !(qid != 0 && !Util.IsActionCastable(qid));
    }

    private void HandleMovementPrevention()
    {
        if (C.ControllerMode || IsStronglyLocked)
        {
            MoveManager.DisableMoving();
        }
        else
        {
            MoveManager.EnableMoving();
        }

        if (!C.DisableMouseDisabling)
        {
            MoveManager.DisableMouseMoving();
        }
        else
        {
            MoveManager.EnableMouseMoving();
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
        MoveManager.EnableMouseMoving();
    }

    private void ResetCancelledMoveKeys()
    {
        if (WasCancelled)
        {
            WasCancelled = false;
            C.MoveKeys.Each(x =>
            {
                if (IsKeyPressed((int)x))
                {
                    DalamudReflector.SetKeyState(x, 3);
                    InternalLog.Debug($"Reenabling key {x}");
                }
            });
        }
    }
}