using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbwalker
{
    internal class Overlay : Window
    {
        public Overlay() : base("OrbwalkerOverlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize, true)
        {
            this.RespectCloseHotkey = false;
            this.IsOpen = true;
            ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_w"), out _);
            ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_g"), out _);
            ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_w"), out _);
            ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_g"), out _);
            ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_w"), out _);
            ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_g"), out _);
        }

        public override void Draw()
        {

            {
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_" + (!P.ShouldUnlock && !C.ForceStopMoveCombat ? "g" : "w")), out var texture))
                {
                    if (ImGui.ImageButton(texture.ImGuiHandle, texture.GetSize(40 * C.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * C.SizeMod)))
                    {
                        C.UnlockPermanently = false;
                        C.ForceStopMoveCombat = false;
                    }
                    ImGui.SameLine();
                }
            }

            {
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_" + (!P.ShouldUnlock && C.ForceStopMoveCombat ? "g" : "w")), out var texture))
                {
                    if (ImGui.ImageButton(texture.ImGuiHandle, texture.GetSize(40 * C.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * C.SizeMod)))
                    {
                        C.UnlockPermanently = false;
                        C.ForceStopMoveCombat = true;
                    }
                    ImGui.SameLine();
                }
            }

            ImGui.Dummy(new(20, 1));
            ImGui.SameLine();
            
            {
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_" + (P.ShouldUnlock ? "g" : "w")), out var texture))
                {
                    if(ImGui.ImageButton(texture.ImGuiHandle, texture.GetSize(40 * C.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * C.SizeMod)))
                    {
                        C.UnlockPermanently = !C.UnlockPermanently;
                    }
                }
            }
        }

        string GetImagePath(string name)
        {
            return Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", $"{name}.png");
        }

        public override bool DrawConditions()
        {
            return C.Enabled && Util.CanUsePlugin() && (C.DisplayAlways || (Svc.Condition[ConditionFlag.BoundByDuty56] && C.DisplayDuty) || (Svc.Condition[ConditionFlag.InCombat] && C.DisplayBattle));
        }
    }
}
