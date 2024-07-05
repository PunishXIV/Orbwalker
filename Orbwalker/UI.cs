using Dalamud.Game.ClientState.GamePad;
using Dalamud.Interface.Components;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Gamepad;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using Orbwalker;
using PunishLib;
using PunishLib.ImGuiMethods;
using System.IO;
using System.Windows.Forms;
using ThreadLoadImageHandler = ECommons.ImGuiMethods.ThreadLoadImageHandler;

namespace Orbwalker
{
    internal unsafe static class UI
    {
        internal static void Draw()
        {
            ImGuiEx.EzTabBar("Default",
                ("Settings", Settings, null, true),
                ("Classes/Jobs", Jobs, null, true),
                ("Extras", Extras, null, true),
                ("About", () => AboutTab.Draw(P.Name), null, true),
                ("Debug".NullWhenFalse(C.Debug), Debug, ImGuiColors.DalamudGrey3, true),
                InternalLog.ImGuiTab(C.Debug)

                );
        }

        static void Extras()
        {

            ImGuiEx.Text($"Job Specific Options");
            ImGuiGroup.BeginGroupBox();
            ImGuiEx.Text($"Block movement when these actions are active:");
            ImGuiEx.Spacing(); ImGui.Checkbox("Passage of Arms (PLD)", ref C.PreventPassage);
            ImGuiEx.Spacing(); ImGui.Checkbox("Ten Chi Jin (NIN)", ref C.PreventTCJ);
            ImGuiEx.Spacing(); ImGui.Checkbox("Flamethrower (MCH)", ref C.PreventFlame);
            ImGuiEx.Spacing(); ImGui.Checkbox("Improvisation (DNC)", ref C.PreventImprov);
            ImGuiGroup.EndGroupBox();


            ImGuiEx.Text($"Miscellaneous Casts");
            ImGuiGroup.BeginGroupBox();
            ImGuiEx.Text($"Block movement when these actions are being casted:");
            ImGuiEx.Spacing(); ImGui.Checkbox("Teleport", ref C.BlockTP);
            ImGuiEx.Spacing(); ImGui.Checkbox("Return", ref C.BlockReturn);
            ImGuiEx.Spacing(); ImGui.Checkbox("Mount", ref C.BlockMount);
            ImGuiGroup.EndGroupBox();

            ImGuiEx.Text($"PvP Settings");
            ImGuiGroup.BeginGroupBox();
            ImGui.Checkbox($"Enable PvP Orbwalking", ref C.PVP);
            ImGuiComponents.HelpMarker("Allows use of Orbwalker when in PvP game modes such as Frontlines and Crystaline Conflict. This is considered an unfair advantage over other players so use at your own risk (and conscious).");
            ImGuiGroup.EndGroupBox();
        }

        static void Spacing(bool cont = false)
        {
            ImGuiEx.TextV($" {(cont ? "├" : "└")} ");
            ImGui.SameLine();
        }

        static void Settings()
        {
            var cur = ImGui.GetCursorPos();
            if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "q.png"), out var t))
            {
                ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 20);
                ImGui.Image(t.ImGuiHandle, new(20, 20));
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    if (ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "t.png"), out var t2))
                    {
                        ImGui.Image(t2.ImGuiHandle, new Vector2(t2.Width, t2.Height));
                    }
                    ImGui.EndTooltip();
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Right))
                    {
                        C.Debug = !C.Debug;
                    }
                }
            }
            ImGui.SetCursorPos(cur);

            if (ImGui.Checkbox($"Enable Orbwalker", ref C.Enabled))
            {
                P.Memory.EnableDisableBuffer();
            }
            ImGuiEx.Text($"Movement");
            ImGuiGroup.BeginGroupBox();
            ImGuiEx.Text($"Slidecast Window Calibration:");
            ImGuiComponents.HelpMarker("Switches between automatic slidecast window calibration or allows you to set a manual value. Automatic mode is fully reliable but will always result in smaller slidecast windows than you can manually configure based on spellspeed/network latency.");
            Spacing(!C.IsSlideAuto);
            ImGuiEx.RadioButtonBool("Automatic", "Manual", ref C.IsSlideAuto, true);
            if (!C.IsSlideAuto)
            {
                Spacing();
                ImGui.SetNextItemWidth(200f);
                ImGui.SliderFloat("Unlock at, s", ref C.Threshold, 0.1f, 1f);
            }
            ImGuiEx.Text($"Orbwalking Mode:");
            ImGuiComponents.HelpMarker("Switch between the two modes. \"Slidecast\" mode is the default and simply prevents player movement until the slidecast window is available, locking movement again to begin the next cast. You must be stationary for the first cast in most cases. \"Slidelock\" mode on the otherhand permanently locks the player from moving while in combat and only allows for movement during the slidecast window. The movement release key is the only way to enable movement when this mode is used.");
            Spacing();
            if (ImGui.RadioButton("Slidecast", !C.ForceStopMoveCombat))
            {
                C.ForceStopMoveCombat = false;
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("Slidelock", C.ForceStopMoveCombat))
            {
                C.ForceStopMoveCombat = true;
            }

            if (ImGui.Checkbox($"Buffer Initial Cast", ref C.Buffer))
            {
                P.Memory.EnableDisableBuffer();
            }
            ImGuiComponents.HelpMarker($"Removes the requirement for the player to be stationary when channeling the first cast by buffering it until movement is halted. This setting may cause strange behavior with plugins such as Redirect or ReAction, or prevent their options from working at all, be warned!");

            ImGui.SetNextItemWidth(200f);
            ImGui.Checkbox("Controller Mode", ref C.ControllerMode);

            if (C.ControllerMode)
                DrawKeybind("Movement Release Button", ref C.ReleaseButton);
            else
                DrawKeybind("Movement Release Key", ref C.ReleaseKey);
            ImGuiComponents.HelpMarker("Bind a key to instantly unlock player movement and cancel any channeling cast. Note that movement is only enabled whilst the key is held, therefore a mouse button is recommended.");
            ImGui.Checkbox($"Permanently Release", ref C.UnlockPermanently);
            ImGuiComponents.HelpMarker("Releases player movement - used primarily by the release key setting above.");
            ImGuiEx.Text($"Release Key Mode:");
            ImGuiComponents.HelpMarker("Switches the movement release key from needing to be held, to becoming a toggle.");
            Spacing();
            ImGuiEx.RadioButtonBool("Hold", "Toggle", ref C.IsHoldToRelease, true);

            if (!C.ControllerMode)
            {
                ImGui.Checkbox($"Enable Mouse Button Release", ref C.DisableMouseDisabling);
                ImGuiComponents.HelpMarker("Allows emergency movement via holding down MB1 and MB2 simultaneously.");
                ImGuiEx.TextV($"Movement keys:");
                ImGui.SameLine();
                ImGuiEx.SetNextItemWidth(0.8f);
                if (ImGui.BeginCombo($"##movekeys", $"{C.MoveKeys.Print()}"))
                {
                    foreach (var x in Svc.KeyState.GetValidVirtualKeys())
                    {
                        ImGuiEx.CollectionCheckbox($"{x}", x, C.MoveKeys);
                    }
                    ImGui.EndCombo();
                }
            }

            ImGui.PushItemWidth(300);
            ImGuiEx.TextV($"Ground Targeting Hold (seconds):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);

            if (ImGui.InputFloat("###GroundedHoldConfig", ref P.Config.GroundedHold,0,0,"%.2g"))
            {
                if (P.Config.GroundedHold <= 0)
                    P.Config.GroundedHold = 0;

                if (P.Config.GroundedHold % 0.01f != 0)
                    P.Config.GroundedHold = (float)Math.Round(P.Config.GroundedHold, 2, MidpointRounding.ToNegativeInfinity);
            }

            ImGuiComponents.HelpMarker($"Spells which have cast times that are also targeted on the ground (mainly found in BLU) register casting as the target appears, and will cancel cast times if moving as it's confirmed. " +
                $"This option forces your character to stop as the target appears for however long as to not cancel the cast pre-emptively.");

            ImGuiGroup.EndGroupBox();

            ImGuiEx.Text($"Overlay");
            ImGuiGroup.BeginGroupBox();

            ImGuiEx.Text($"Display Overlay");
            ImGuiComponents.HelpMarker("Choose when to display the Orbwalker overlay when enabled.");
            Spacing(true); ImGui.Checkbox($"In Combat", ref C.DisplayBattle);
            Spacing(true); ImGui.Checkbox($"In Duty", ref C.DisplayDuty);
            Spacing(true); ImGui.Checkbox($"Always", ref C.DisplayAlways);
            Spacing();
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderFloat($"Overlay scale", ref C.SizeMod.ValidateRange(0.5f, 2f), 0.8f, 1.2f);

            ImGuiGroup.EndGroupBox();
        }

        static void Jobs()
        {
            ImGuiEx.Text($"Jobs");
            ImGuiComponents.HelpMarker("Select the jobs you wish to use Orbwalker's movement locking features on. Not all jobs have cast times, but if you have the extra features enabled for the general actions it will apply to those jobs.");
            ImGuiGroup.BeginGroupBox();
            ImGuiEx.TextV("Toggle:");
            ImGui.SameLine();
            if (ImGui.Button("All"))
            {
                var jobs = Enum.GetValues<Job>().Where(x => x != Job.ADV);
                var b = jobs.All(x => C.EnabledJobs.TryGetValue(x, out var v) && v);
                foreach (var x in jobs)
                {
                    C.EnabledJobs[x] = !b;
                }
            }
            ImGui.SameLine();
            if (ImGui.Button($"Casters"))
            {
                var b = Data.CastingJobs.All(x => C.EnabledJobs.TryGetValue(x, out var v) && v);
                foreach (var x in Data.CastingJobs)
                {
                    C.EnabledJobs[x] = !b;
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("DoL/DoH"))
            {
                var jobs = Svc.Data.GetExcelSheet<ClassJob>().Where(x => x.ClassJobCategory.Value.RowId.EqualsAny<uint>(33, 32)).Select(x => x.RowId).Cast<Job>();
                var b = jobs.All(x => C.EnabledJobs.TryGetValue(x, out var v) && v);
                foreach (var x in jobs)
                {
                    C.EnabledJobs[x] = !b;
                }
            }
            ImGui.Separator();
            ImGui.Columns(5, "###JobGrid", false);
            foreach (var job in Enum.GetValues<Job>())
            {
                if (job == Job.ADV) continue;
                if (!P.Config.EnabledJobs.ContainsKey(job))
                    P.Config.EnabledJobs[job] = false;

                bool val = P.Config.EnabledJobs[job];
                if (ImGui.Checkbox($"{job}", ref val))
                {
                    if (val)
                    {
                        switch (job)
                        {
                            case Job.WHM:
                            case Job.CNJ:
                                P.Config.EnabledJobs[Job.WHM] = val;
                                P.Config.EnabledJobs[Job.CNJ] = val;
                                break;
                            case Job.BLM:
                            case Job.THM:
                                P.Config.EnabledJobs[Job.BLM] = val;
                                P.Config.EnabledJobs[Job.THM] = val;
                                break;
                            case Job.MNK:
                            case Job.PGL:
                                P.Config.EnabledJobs[Job.MNK] = val;
                                P.Config.EnabledJobs[Job.PGL] = val;
                                break;
                            case Job.ACN:
                            case Job.SMN:
                            case Job.SCH:
                                P.Config.EnabledJobs[Job.ACN] = val;
                                P.Config.EnabledJobs[Job.SMN] = val;
                                P.Config.EnabledJobs[Job.SCH] = val;
                                break;
                            case Job.MRD:
                            case Job.WAR:
                                P.Config.EnabledJobs[Job.MRD] = val;
                                P.Config.EnabledJobs[Job.WAR] = val;
                                break;
                            case Job.PLD:
                            case Job.GLA:
                                P.Config.EnabledJobs[Job.PLD] = val;
                                P.Config.EnabledJobs[Job.GLA] = val;
                                break;
                            case Job.ROG:
                            case Job.NIN:
                                P.Config.EnabledJobs[Job.ROG] = val;
                                P.Config.EnabledJobs[Job.NIN] = val;
                                break;
                            case Job.BRD:
                            case Job.ARC:
                                P.Config.EnabledJobs[Job.BRD] = val;
                                P.Config.EnabledJobs[Job.ARC] = val;
                                break;
                            case Job.LNC:
                            case Job.DRG:
                                P.Config.EnabledJobs[Job.LNC] = val;
                                P.Config.EnabledJobs[Job.DRG] = val;
                                break;
                            case Job.CUL:
                            case Job.ALC:
                            case Job.BSM:
                            case Job.GSM:
                            case Job.ARM:
                            case Job.LTW:
                            case Job.CRP:
                            case Job.WVR:
                                P.Config.EnabledJobs[Job.CUL] = val;
                                P.Config.EnabledJobs[Job.ALC] = val;
                                P.Config.EnabledJobs[Job.BSM] = val;
                                P.Config.EnabledJobs[Job.GSM] = val;
                                P.Config.EnabledJobs[Job.ARM] = val;
                                P.Config.EnabledJobs[Job.LTW] = val;
                                P.Config.EnabledJobs[Job.CRP] = val;
                                P.Config.EnabledJobs[Job.WVR] = val;
                                break;
                            case Job.BTN:
                            case Job.MIN:
                            case Job.FSH:
                                P.Config.EnabledJobs[Job.BTN] = val;
                                P.Config.EnabledJobs[Job.MIN] = val;
                                P.Config.EnabledJobs[Job.FSH] = val;
                                break;
                        }
                    }

                    P.Config.EnabledJobs[job] = val;
                }
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
            ImGuiGroup.EndGroupBox();
        }

        static void Debug()
        {
            //ImGui.InputInt($"forceDisableMovementPtr", ref P.Memory.ForceDisableMovement);
            if (Svc.Targets.Target != null)
            {
                var addInfo = stackalloc uint[1];
                // was spell before, fixed
                ImGuiEx.Text($"{ActionManager.Instance()->GetActionStatus(ActionType.Action, 16541, Svc.Targets.Target.Struct()->EntityId, outOptExtraInfo: addInfo)} / {*addInfo}");
            }
            ImGuiEx.Text($"GCD: {Util.GCD}\nRCorGRD:{Util.GetRCorGDC()}");
        }

        static string KeyInputActive = null;
        static bool DrawKeybind(string text, ref Keys key)
        {
            bool ret = false;
            ImGui.PushID(text);
            ImGuiEx.Text($"{text}:");
            ImGui.Dummy(new(20, 1));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200f);
            if (ImGui.BeginCombo("##inputKey", $"{key}"))
            {
                if (text == KeyInputActive)
                {
                    ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...");
                    foreach (var x in Enum.GetValues<Keys>())
                    {
                        if (IsKeyPressed((int) x))
                        {
                            KeyInputActive = null;
                            key = x;
                            ret = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (ImGui.Selectable("Auto-detect new key", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        KeyInputActive = text;
                    }
                    ImGuiEx.Text($"Select key manually:");
                    ImGuiEx.SetNextItemFullWidth();
                    ImGuiEx.EnumCombo("##selkeyman", ref key);
                }
                ImGui.EndCombo();
            }
            else
            {
                if (text == KeyInputActive)
                {
                    KeyInputActive = null;
                }
            }
            if (key != Keys.None)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    key = Keys.None;
                    ret = true;
                }
            }
            ImGui.PopID();
            return ret;
        }

        static bool DrawKeybind(string text, ref GamepadButtons key)
        {
            bool ret = false;
            ImGui.PushID(text);
            ImGuiEx.Text($"{text}:");
            ImGui.Dummy(new(20, 1));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200f);
            if (ImGui.BeginCombo("##inputKey", $"{GamePad.ControllerButtons[key]}"))
            {
                if (text == KeyInputActive)
                {
                    ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...");
                    foreach (var x in GamePad.ControllerButtons)
                    {
                        if (GamePad.IsButtonPressed(x.Key))
                        {
                            KeyInputActive = null;
                            key = x.Key;
                            ret = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (ImGui.Selectable("Auto-detect new key", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        KeyInputActive = text;
                    }
                    ImGuiEx.Text($"Select key manually:");
                    ImGuiEx.SetNextItemFullWidth();
                    if (ImGui.BeginCombo("##selkeyman", GamePad.ControllerButtons[key]))
                    {
                        foreach (var button in GamePad.ControllerButtons)
                        {
                            if (ImGui.Selectable($"{button.Value}", button.Key == key))
                                key = button.Key;
                        }

                        ImGui.EndCombo();
                    }
                }
                ImGui.EndCombo();
            }
            else
            {
                if (text == KeyInputActive)
                {
                    KeyInputActive = null;
                }
            }
            if (key != GamepadButtons.None)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    key = GamepadButtons.None;
                    ret = true;
                }
            }
            ImGui.PopID();
            return ret;
        }
    }
}
