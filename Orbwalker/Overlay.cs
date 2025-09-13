using Dalamud.Interface.Textures.TextureWraps;
using System.IO;
namespace Orbwalker;

internal class Overlay : Window
{
    public Overlay() : base("OrbwalkerOverlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize, true)
    {
        RespectCloseHotkey = false;
        IsOpen = true;
        ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_w"), out IDalamudTextureWrap _);
        ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_g"), out IDalamudTextureWrap _);
        ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_w"), out IDalamudTextureWrap _);
        ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_g"), out IDalamudTextureWrap _);
        ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_w"), out IDalamudTextureWrap _);
        ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_g"), out IDalamudTextureWrap _);
    }

    public override void Draw()
    {
        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("slidecast_" + (!P.ShouldUnlock && !C.ForceStopMoveCombat ? "g" : "w")), out IDalamudTextureWrap texture))
            {
                if (ImGui.ImageButton(texture.Handle, texture.GetSize(40 * C.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * C.SizeMod)))
                {
                    C.UnlockPermanently = false;
                    C.ForceStopMoveCombat = false;
                }
                ImGui.SameLine();
            }
        }

        {
            if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("lockslide_" + (!P.ShouldUnlock && C.ForceStopMoveCombat ? "g" : "w")), out IDalamudTextureWrap texture))
            {
                if (ImGui.ImageButton(texture.Handle, texture.GetSize(40 * C.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * C.SizeMod)))
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
            if (ThreadLoadImageHandler.TryGetTextureWrap(GetImagePath("disabled_" + (P.ShouldUnlock ? "g" : "w")), out IDalamudTextureWrap texture))
            {
                if (ImGui.ImageButton(texture.Handle, texture.GetSize(40 * C.SizeMod), Vector2.Zero, Vector2.One, (int)(10f * C.SizeMod)))
                {
                    C.UnlockPermanently = !C.UnlockPermanently;
                }
            }
        }
    }

    private string GetImagePath(string name) => Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", $"{name}.png");

    public override bool DrawConditions() => !C.UseImguiOverlay && C.Enabled && Util.CanUsePlugin() && (C.DisplayAlways || Svc.Condition[ConditionFlag.BoundByDuty56] && C.DisplayDuty || Svc.Condition[ConditionFlag.InCombat] && C.DisplayBattle);
}