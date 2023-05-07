using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin;
using ECommons.Configuration;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using ECommons.Reflection;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Orbwalker;
using PInvoke;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;

namespace Unmoveable
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

        internal float GCD 
        { 
            get
            {
                var cd = ActionManager.Instance()->GetRecastGroupDetail(57);
                if (cd->IsActive == 0) return 0;
                return cd->Total - cd->Elapsed;
            }
        }

        bool IsCasting()
        {
            if (P.Config.IsSlideAuto)
            {
                return Svc.Condition[ConditionFlag.Casting];
            }
            else
            {
                return Player.Object.IsCasting && Player.Object.TotalCastTime - Player.Object.CurrentCastTime > Config.Threshold;
            }
        }

        internal bool IsUnlockKeyHeld()
        {
            return !Framework.Instance()->WindowInactive && (P.Config.ReleaseKey != Keys.None && IsKeyPressed(P.Config.ReleaseKey));
        }

        private void Framework_Update(Dalamud.Game.Framework framework)
        {
            if(DelayedAction != null && DelayedAction.actionId != 0 && AgentMap.Instance()->IsPlayerMoving == 0)
            {
                if (Player.Available)
                {
                    var a = ActionManager.Instance();
                    PluginLog.Debug($"Using action {DelayedAction}");
                    try
                    {
                        DelayedAction.Use();
                    }
                    catch (Exception e)
                    {
                        e.Log();
                    }
                }
                DelayedAction = null;
            }
            if (P.Config.Enabled && Util.CanUsePlugin())
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
                //DuoLog.Information($"{GCD}");
                var qid = ActionQueue.Get()->ActionID;
                if ((IsCasting() || DelayedAction != null || (qid != 0 && Util.IsActionCastable(qid) && GCD < 0.1) || (P.Config.ForceStopMoveCombat && Svc.Condition[ConditionFlag.InCombat] && GCD < 0.1 && !(qid != 0 && !Util.IsActionCastable(qid)))) && !ShouldUnlock)
                {
                    if (!P.Config.DisableMouseDisabling && Util.IsMouseMoveOrdered())
                    {
                        MoveManager.DisableMoving();
                    }
                    else
                    {
                        MoveManager.EnableMoving();
                    }
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
                else
                {
                    MoveManager.EnableMoving();
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
            }
            else
            {
                MoveManager.EnableMoving();
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