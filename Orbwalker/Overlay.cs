using ImGuiScene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unmoveable
{
    internal class Overlay : Window
    {
        public Overlay() : base("UnmoveableOverlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize, true)
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
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_" + (!P.ShouldUnlock && !P.Config.ForceStopMoveCombat ? "g" : "w")), out var texture))
                {
                    if (ImGui.ImageButton(texture.ImGuiHandle, texture.GetSize(40 * P.Config.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * P.Config.SizeMod)))
                    {
                        P.Config.UnlockPermanently = false;
                        P.Config.ForceStopMoveCombat = false;
                    }
                    ImGui.SameLine();
                }
            }

            {
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_" + (!P.ShouldUnlock && P.Config.ForceStopMoveCombat ? "g" : "w")), out var texture))
                {
                    if (ImGui.ImageButton(texture.ImGuiHandle, texture.GetSize(40 * P.Config.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * P.Config.SizeMod)))
                    {
                        P.Config.UnlockPermanently = false;
                        P.Config.ForceStopMoveCombat = true;
                    }
                    ImGui.SameLine();
                }
            }

            ImGui.Dummy(new(20, 1));
            ImGui.SameLine();
            
            {
                if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_" + (P.ShouldUnlock ? "g" : "w")), out var texture))
                {
                    if(ImGui.ImageButton(texture.ImGuiHandle, texture.GetSize(40 * P.Config.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * P.Config.SizeMod)))
                    {
                        P.Config.UnlockPermanently = !P.Config.UnlockPermanently;
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
            return P.Config.Enabled && (P.Config.DisplayAlways || (Svc.Condition[ConditionFlag.BoundByDuty56] && P.Config.DisplayDuty) || (Svc.Condition[ConditionFlag.InCombat] && P.Config.DisplayBattle));
        }
    }
}
