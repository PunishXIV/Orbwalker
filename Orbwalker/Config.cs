using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using ECommons.Configuration;
using ECommons.ExcelServices;
using System.Windows.Forms;

namespace Orbwalker
{
    public class Config : IEzConfig
    {
        public bool ForceStopMoveCombat = false;
        public Keys ReleaseKey = Keys.LControlKey;
        public bool ControllerMode = false; 
        public GamepadButtons ReleaseButton = GamepadButtons.L1;
        public float SizeMod = 1f;
        public bool UnlockPermanently = false;
        public bool Enabled = true;
        public bool Buffer = false;
        public bool DisableMouseDisabling = false;
        public HashSet<VirtualKey> MoveKeys = new()
        {
            VirtualKey.W,
            VirtualKey.A,
            VirtualKey.S,
            VirtualKey.D
        };

        public bool DisplayBattle = true;
        public bool DisplayDuty = true;
        public bool DisplayAlways = false;
        public bool IsHoldToRelease = true;
        public bool IsSlideAuto = true;
        public float Threshold = 0.5f;
        public int CastHold = 100;
        public float GroundedHold = 1f;

        public bool PreventPassage = false;
        public bool PreventFlame = false;
        public bool PreventImprov = false;
		public bool PreventPhantom = false;

		public bool Debug = false;
        public bool PVP = false;
        public bool BlockTP = false;
        public bool BlockReturn = false;

        public Dictionary<Job, bool> EnabledJobs = new();
    }
}
