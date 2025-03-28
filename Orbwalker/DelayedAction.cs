﻿using FFXIVClientStructs.FFXIV.Client.Game;
namespace Orbwalker;

internal unsafe class DelayedAction
{
    internal uint a5, a6, a7;
    internal void* a8;
    internal uint actionId;
    internal long execAt;
    internal long targetId;
    internal ActionType type;

    internal DelayedAction(uint actionId, ActionType type, long execAt, long targetId, uint a5, uint a6, uint a7, void* a8)
    {
        this.actionId = actionId;
        this.execAt = execAt;
        this.targetId = targetId;
        this.a5 = a5;
        this.a6 = a6;
        this.a7 = a7;
        this.a8 = a8;
        this.type = type;
        PluginLog.Debug($"Generated delayed action: {this}");
    }

    internal void Use()
    {
        P.Memory.UseActionHook.Original.Invoke(ActionManager.Instance(), type, actionId, targetId, a5, a6, a7, a8);
    }

    public override string ToString() => $"[id={actionId}, type={type}, execAt={execAt}, target={targetId:X16}, a5={a5}, a6={a6}, a7={a7}, a8={(nint)a8:X16}]";
}
