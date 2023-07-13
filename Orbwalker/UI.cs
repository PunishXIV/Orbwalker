using Dalamud.Game.ClientState.GamePad;
using Dalamud.Interface.Components;
using ECommons.GameFunctions;
using ECommons.Gamepad;
using FFXIVClientStructs.FFXIV.Client.Game;
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
                ("Extras", Extras, null, true),
                ("About", () => AboutTab.Draw(P), null, true),
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

        static void Debug()
        {
            ImGui.InputInt($"forceDisableMovementPtr", ref P.Memory.ForceDisableMovement);
            if (Svc.Targets.Target != null)
            {
                var addInfo = stackalloc uint[1];
                ImGuiEx.Text($"{ActionManager.Instance()->GetActionStatus(ActionType.Spell, 16541, Svc.Targets.Target.Struct()->GetObjectID(), outOptExtraInfo: addInfo)} / {*addInfo}");
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
                        if (IsKeyPressed(x))
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
