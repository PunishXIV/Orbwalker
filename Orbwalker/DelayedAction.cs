using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbwalker
{
    internal unsafe class DelayedAction
    {
        internal uint actionId = 0;
        internal long execAt = 0;
        internal long targetId = 0;
        internal uint a5, a6, a7;
        internal void* a8;

        internal DelayedAction(uint actionId, long execAt, long targetId, uint a5, uint a6, uint a7, void* a8)
        {
            this.actionId = actionId;
            this.execAt = execAt;
            this.targetId = targetId;
            this.a5 = a5;
            this.a6 = a6;
            this.a7 = a7;
            this.a8 = a8;
            PluginLog.Debug($"Generated delayed action: {this}");
        }

        internal void Use()
        {
            P.Memory.UseActionHook.Original.Invoke(ActionManager.Instance(), ActionType.Spell, actionId, targetId, a5, a6, a7, a8);
        }

        public override string ToString()
        {
            return $"[id={actionId}, execAt={execAt}, target={targetId:X16}, a5={a5}, a6={a6}, a7={a7}, a8={(nint)a8:X16}]";
        }
    }
}
