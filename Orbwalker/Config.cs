using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool PreventPassage = false;
        public bool PreventTCJ = false;
        public bool PreventFlame = false;
        public bool PreventImprov = false;

        public bool Debug = false;
        public bool PVP = false;
        public bool BlockTP = false;
        public bool BlockReturn = false;
        public bool BlockMount = false;
    }
}
