using ECommons.ExcelServices;
using ECommons.EzIpcManager;
using System.Windows.Forms;

namespace Orbwalker
{
    class IPC
    {
        internal IPC()
        {
            EzIPC.Init(this);
        }


        [EzIPC] public bool PluginEnabled() => C.Enabled;
        [EzIPC] public bool MovementLocked() => MoveManager.MovingDisabled;
        [EzIPC] public bool IsSlideWindowAuto() => C.IsSlideAuto; //True for Automatic, False for Manual
        [EzIPC] public bool OrbwalkingMode() => C.ForceStopMoveCombat; //True for Slidecast, False for Slidelock
        [EzIPC] public bool BufferEnabled() => C.Buffer;
        [EzIPC] public bool ControllerModeEnabled() => C.ControllerMode;
        [EzIPC] public bool MouseButtonReleaseEnabled() => C.DisableMouseDisabling;
        [EzIPC] public bool PvPEnabled() => C.PVP;
        [EzIPC] public List<uint> EnabledJobs() => C.EnabledJobs.Where(x => x.Value).SelectMulti(x => (uint)x.Key).ToList();


        [EzIPC] public void SetPluginEnabled(bool v) => C.Enabled = v;
        [EzIPC] public void SetSlideAuto(bool v) => C.IsSlideAuto = v;
        [EzIPC] public void SetOrbwalkingMode(bool v) => C.ForceStopMoveCombat = v;
        [EzIPC] public void SetBuffer(bool v) => C.Buffer = v;
        [EzIPC] public void SetControllerMode(bool v) => C.ControllerMode = v;
        [EzIPC] public void SetMouseButtonRelease(bool v) => C.DisableMouseDisabling = v;
        [EzIPC] public void SetPvP(bool v) => C.PVP = v;
        [EzIPC] public void SetEnabledJob(uint job, bool v) => C.EnabledJobs[(Job)job] = v;

    }
}
