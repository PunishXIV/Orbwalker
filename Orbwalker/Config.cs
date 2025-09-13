using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.Keys;
using ECommons.ExcelServices;
using System.Windows.Forms;

namespace Orbwalker;

public class Config
{
    public bool ForceStopMoveCombat = false;
    public bool ControllerMode = false;
    public bool UnlockPermanently = false;
    public bool Enabled = true;
    public bool Buffer = false;
    public bool DisableMouseDisabling = false;
    public bool DisplayBattle = true;
    public bool DisplayDuty = true;
    public bool DisplayAlways = false;
    public bool DisplayVertical = false;
    public bool IsHoldToRelease = true;
    public bool BlockAllMovement = false;
    public bool IsSlideAuto = true;
    public bool PreventPassage = false;
    public bool PreventFlame = false;
    public bool PreventImprov = false;
    public bool PreventPhantom = false;
    public bool PreventMeditate = false;
    public bool Debug = false;
    public bool PVP = false;
    public bool BlockTP = false;
    public bool BlockReturn = false;
    public bool UseImguiOverlay = false;

    public Keys ReleaseKey = Keys.LControlKey;
    public Keys BlockKey = Keys.XButton1;
    public HashSet<VirtualKey> MoveKeys =
    [
        VirtualKey.W,
        VirtualKey.A,
        VirtualKey.S,
        VirtualKey.D
    ];

    public GamepadButtons ReleaseButton = GamepadButtons.L1;
    public GamepadButtons BlockButton = GamepadButtons.L2;

    public float SizeMod = 1f;
    public float Threshold = 0.5f;
    public float GroundedHold = 1f;

    public int CastHold = 100;

    public Dictionary<Job, bool> EnabledJobs = [];
}