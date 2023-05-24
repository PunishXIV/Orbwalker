using Dalamud.Game.ClientState.GamePad;
using Dalamud.Interface.Components;
using ECommons.GameFunctions;
using ECommons.Gamepad;
using FFXIVClientStructs.FFXIV.Client.Game;
using Orbwalker;
using System.IO;
using System.Windows.Forms;
using ECommons.LanguageHelpers;
using Localization = ECommons.LanguageHelpers.Localization;
namespace Orbwalker
{
    internal unsafe static class UI
    {
        internal static void Draw()
        {
            ImGuiEx.EzTabBar("Default",
                ("Settings".Loc(), Settings, null, true),
                ("Debug", Debug, ImGuiColors.DalamudGrey3, true),
                InternalLog.ImGuiTab()

                );
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
                }
            }
            ImGui.SetCursorPos(cur);

            if (ImGui.Checkbox($"Enable Orbwalker".Loc(), ref P.Config.Enabled))
            {
                P.Memory.EnableDisableBuffer();
            }
            if(ImGui.BeginCombo("##langsel", P.Config.PluginLanguage == null?"Game language".Loc() : P.Config.PluginLanguage.Loc()))
            {
                if (ImGui.Selectable("Game language".Loc()))
                {
                    P.Config.PluginLanguage = null;
                    Localization.Init(GameLanguageString);
                }
                foreach (var x in GetAvaliableLanguages())
                {
                    if (ImGui.Selectable(x.Loc()))
                    {
                        P.Config.PluginLanguage = x;
                        Localization.Init(P.Config.PluginLanguage);
                    }
                }
                ImGui.EndCombo();
            }
            ImGuiEx.Text($"Movement".Loc());
            ImGuiGroup.BeginGroupBox();
            ImGuiEx.Text($"Slidecast Window Calibration:".Loc());
            ImGuiComponents.HelpMarker("Switches between automatic slidecast window calibration or allows you to set a manual value. Automatic mode is fully reliable but will always result in smaller slidecast windows than you can manually configure based on spellspeed/network latency.".Loc());
            Spacing(!P.Config.IsSlideAuto);
            ImGuiEx.RadioButtonBool("Automatic".Loc(), "Manual".Loc(), ref P.Config.IsSlideAuto, true);
            if (!P.Config.IsSlideAuto)
            {
                Spacing();
                ImGui.SetNextItemWidth(200f);
                ImGui.SliderFloat("Unlock at, s".Loc(), ref P.Config.Threshold, 0.1f, 1f);
            }
            ImGuiEx.Text($"Orbwalking Mode:".Loc());
            ImGuiComponents.HelpMarker("Switch between the two modes. \"Slidecast\" mode is the default and simply prevents player movement until the slidecast window is available, locking movement again to begin the next cast. You must be stationary for the first cast in most cases. \"Slidelock\" mode on the otherhand permanently locks the player from moving while in combat and only allows for movement during the slidecast window. The movement release key is the only way to enable movement when this mode is used.".Loc());
            Spacing();
            if (ImGui.RadioButton("Slidecast".Loc(), !P.Config.ForceStopMoveCombat))
            {
                P.Config.ForceStopMoveCombat = false;
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("Slidelock".Loc(), P.Config.ForceStopMoveCombat))
            {
                P.Config.ForceStopMoveCombat = true;
            }
            ImGui.SetNextItemWidth(200f);
            ImGui.Checkbox("Controller Mode".Loc(), ref P.Config.ControllerMode);

            if (P.Config.ControllerMode)
                DrawKeybind("Movement Release Button".Loc(), ref P.Config.ReleaseButton);
            else
                DrawKeybind("Movement Release Key".Loc(), ref P.Config.ReleaseKey);
            ImGuiComponents.HelpMarker("Bind a key to instantly unlock player movement and cancel any channeling cast. Note that movement is only enabled whilst the key is held, therefore a mouse button is recommended.".Loc());
            ImGui.Checkbox($"Permanently Release".Loc(), ref P.Config.UnlockPermanently);
            ImGuiComponents.HelpMarker("Releases player movement - used primarily by the release key setting above.".Loc());
            ImGuiEx.Text($"Release Key Mode:".Loc());
            ImGuiComponents.HelpMarker("Switches the movement release key from needing to be held, to becoming a toggle.".Loc());
            Spacing();
            ImGuiEx.RadioButtonBool("Hold".Loc(), "Toggle".Loc(), ref P.Config.IsHoldToRelease, true);

            if (ImGui.Checkbox($"Buffer Initial Cast (BETA)".Loc(), ref P.Config.Buffer))
            {
                P.Memory.EnableDisableBuffer();
            }
            ImGuiComponents.HelpMarker($"Removes the requirement for the player to be stationary when channeling the first cast by buffering it until movement is halted. This setting may cause strange behavior with plugins such as Redirect or ReAction, or prevent their options from working at all, be warned!".Loc());

            if (!P.Config.ControllerMode)
            {
                ImGui.Checkbox($"Enable Mouse Button Release".Loc(), ref P.Config.DisableMouseDisabling);
                ImGuiComponents.HelpMarker("Allows emergency movement via holding down MB1 and MB2 simultaneously.".Loc());
                ImGuiEx.TextV($"Movement keys:".Loc());
                ImGui.SameLine();
                ImGuiEx.SetNextItemWidth(0.8f);
                if (ImGui.BeginCombo($"##movekeys", $"{P.Config.MoveKeys.Print()}"))
                {
                    foreach (var x in Svc.KeyState.GetValidVirtualKeys())
                    {
                        ImGuiEx.CollectionCheckbox($"{x}", x, P.Config.MoveKeys);
                    }
                    ImGui.EndCombo();
                }
            }
            ImGuiGroup.EndGroupBox();

            ImGuiEx.Text($"Overlay".Loc());
            ImGuiGroup.BeginGroupBox();

            ImGuiEx.Text($"Display Overlay".Loc());
            ImGuiComponents.HelpMarker("Choose when to display the Orbwalker overlay when enabled.".Loc());
            Spacing(true); ImGui.Checkbox($"In Combat".Loc(), ref P.Config.DisplayBattle);
            Spacing(true); ImGui.Checkbox($"In Duty".Loc(), ref P.Config.DisplayDuty);
            Spacing(true); ImGui.Checkbox($"Always".Loc(), ref P.Config.DisplayAlways);
            Spacing();
            ImGui.SetNextItemWidth(100f);
            ImGui.SliderFloat($"Overlay scale".Loc(), ref P.Config.SizeMod.ValidateRange(0.5f, 2f), 0.8f, 1.2f);

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
                    ImGuiEx.Text(ImGuiColors.DalamudYellow, $"Now press new key...".Loc());
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
                    if (ImGui.Selectable("Auto-detect new key".Loc(), false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        KeyInputActive = text;
                    }
                    ImGuiEx.Text($"Select key manually:".Loc());
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
