namespace Orbwalker;

internal class ImGuiOverlay : Window
{
    public ImGuiOverlay() : base("OrbwalkerOverlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse, true)
    {
        RespectCloseHotkey = false;
        IsOpen = true;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(0, 0),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(3, 3));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(3);
    }

    public override void Draw()
    {
        var drawList = ImGui.GetWindowDrawList();
        var buttonSize = 40 * C.SizeMod;

        var activeColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 1f, 0.3f, 1f));
        var inactiveColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1f));

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Vector2.Zero);

        //SLIDECAST BUTTON (ICE SKATE)
        {
            var slidecastActive = !P.ShouldUnlock && !C.ForceStopMoveCombat;
            var slidecastColor = slidecastActive ? activeColor : inactiveColor;

            var startPos = ImGui.GetCursorScreenPos();
            ImGui.InvisibleButton("##slidecast", new Vector2(buttonSize, buttonSize));

            DrawIceSkateIcon(drawList, startPos, buttonSize, slidecastColor);

            if (ImGui.IsItemClicked())
            {
                C.UnlockPermanently = false;
                C.ForceStopMoveCombat = false;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(slidecastActive ? "Slidecast Mode (Active)" : "Slidecast Mode (Click to enable)");
            }

            if (!C.DisplayVertical)
            {
                ImGui.SameLine(0, 0);
            }
        }

        //LOCK MOVEMENT BUTTON (PADLOCK)
        {
            var lockActive = !P.ShouldUnlock && C.ForceStopMoveCombat;
            var lockColor = lockActive ? activeColor : inactiveColor;

            var startPos = ImGui.GetCursorScreenPos();
            ImGui.InvisibleButton("##lockslide", new Vector2(buttonSize, buttonSize));

            DrawLockIcon(drawList, startPos, buttonSize, lockColor);

            if (ImGui.IsItemClicked())
            {
                C.UnlockPermanently = false;
                C.ForceStopMoveCombat = true;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(lockActive ? "Lock Movement (Active)" : "Lock Movement (Click to enable)");
            }

            if (!C.DisplayVertical)
            {
                ImGui.SameLine(0, 0);
            }
        }

        //DISABLE PLUGIN BUTTON
        {
            var disabledActive = P.ShouldUnlock;
            var disabledColor = disabledActive ? activeColor : inactiveColor;

            var startPos = ImGui.GetCursorScreenPos();
            ImGui.InvisibleButton("##disabled", new Vector2(buttonSize, buttonSize));

            DrawDisabledIcon(drawList, startPos, buttonSize, disabledColor);

            if (ImGui.IsItemClicked())
            {
                C.UnlockPermanently = !C.UnlockPermanently;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(disabledActive ? "Plugin Disabled (Click to enable)" : "Plugin Enabled (Click to disable)");
            }
        }

        ImGui.PopStyleVar(3);
    }

    //ICE SKATE ICON
    private void DrawIceSkateIcon(ImDrawListPtr drawList, Vector2 pos, float size, uint color)
    {
        var center = pos + new Vector2(size / 2, size / 2);
        var scale = size / 40f;
        var thickness = 2.0f * scale;

        //Boot Outline
        var bootPoints = new Vector2[]
        {
            center + new Vector2(-6 * scale, -10 * scale),
            center + new Vector2(-6 * scale, -6 * scale),
            center + new Vector2(-6 * scale, 2 * scale),
            center + new Vector2(-5 * scale, 5 * scale),
            center + new Vector2(-3 * scale, 6 * scale),
            center + new Vector2(2 * scale, 6 * scale),
            center + new Vector2(7 * scale, 6 * scale),
            center + new Vector2(10 * scale, 5 * scale),
            center + new Vector2(11 * scale, 3 * scale),
            center + new Vector2(11 * scale, 1 * scale),
            center + new Vector2(9 * scale, -1 * scale),
            center + new Vector2(6 * scale, -2 * scale),
            center + new Vector2(2 * scale, -3 * scale),
            center + new Vector2(0 * scale, -6 * scale),
            center + new Vector2(0 * scale, -10 * scale),
        };

        drawList.AddPolyline(ref bootPoints[0], bootPoints.Length, color, ImDrawFlags.Closed, thickness);

        //Boot Laces
        drawList.AddLine(center + new Vector2(-4 * scale, -7 * scale), center + new Vector2(-1 * scale, -7 * scale), color, thickness * 0.6f);
        drawList.AddLine(center + new Vector2(-4 * scale, -4 * scale), center + new Vector2(-1 * scale, -4 * scale), color, thickness * 0.6f);
        drawList.AddLine(center + new Vector2(-4 * scale, -1 * scale), center + new Vector2(-1 * scale, -1 * scale), color, thickness * 0.6f);

        //Skate Blade
        var bladeY = center.Y + 11 * scale;
        drawList.AddLine(new Vector2(center.X - 5 * scale, bladeY), new Vector2(center.X + 11 * scale, bladeY), color, thickness * 1.5f);

        //Blade Holders/Posts
        drawList.AddLine(new Vector2(center.X - 2 * scale, center.Y + 6 * scale), new Vector2(center.X - 2 * scale, bladeY), color, thickness * 0.8f);
        drawList.AddLine(new Vector2(center.X + 6 * scale, center.Y + 6 * scale), new Vector2(center.X + 6 * scale, bladeY), color, thickness * 0.8f);
    }

    //PADLOCK ICON
    private void DrawLockIcon(ImDrawListPtr drawList, Vector2 pos, float size, uint color)
    {
        var center = pos + new Vector2(size / 2, size / 2);
        var scale = size / 40f;
        var thickness = 2.0f * scale;

        //Lock Body (Rectangle)
        var bodyTop = center.Y + 0;
        var bodyLeft = center.X - 8 * scale;
        var bodyRight = center.X + 8 * scale;
        var bodyBottom = center.Y + 10 * scale;

        drawList.AddRect(
            new Vector2(bodyLeft, bodyTop),
            new Vector2(bodyRight, bodyBottom),
            color,
            3 * scale,
            ImDrawFlags.None,
            thickness
        );

        //Lock Shackle (Top Loop)
        var shacklePoints = new Vector2[]
        {
            new Vector2(center.X - 6 * scale, bodyTop),
            new Vector2(center.X - 6 * scale, center.Y - 4 * scale),
            new Vector2(center.X - 6 * scale, center.Y - 8 * scale),
            new Vector2(center.X - 4 * scale, center.Y - 10 * scale),
            new Vector2(center.X + 4 * scale, center.Y - 10 * scale),
            new Vector2(center.X + 6 * scale, center.Y - 8 * scale),
            new Vector2(center.X + 6 * scale, center.Y - 4 * scale),
            new Vector2(center.X + 6 * scale, bodyTop)
        };

        drawList.AddPolyline(ref shacklePoints[0], shacklePoints.Length, color, ImDrawFlags.None, thickness);

        //Keyhole
        drawList.AddCircleFilled(new Vector2(center.X, center.Y + 5 * scale), 2 * scale, color);
        drawList.AddRectFilled(
            new Vector2(center.X - 1 * scale, center.Y + 5 * scale),
            new Vector2(center.X + 1 * scale, center.Y + 8 * scale),
            color
        );
    }

    //PROHIBITION SIGN ICON
    private void DrawDisabledIcon(ImDrawListPtr drawList, Vector2 pos, float size, uint color)
    {
        var center = pos + new Vector2(size / 2, size / 2);
        var scale = size / 40f;
        var radius = 12 * scale;
        var thickness = 2.3f * scale;

        //Outer Circle
        drawList.AddCircle(center, radius, color, 0, thickness);

        //Diagonal Line
        var angle = -MathF.PI / 4;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        var lineStart = center + new Vector2(cos * radius * 0.7f, sin * radius * 0.7f);
        var lineEnd = center - new Vector2(cos * radius * 0.7f, sin * radius * 0.7f);

        drawList.AddLine(lineStart, lineEnd, color, thickness);
    }

    public override bool DrawConditions() => C.Enabled && Util.CanUsePlugin() && (C.DisplayAlways || Svc.Condition[ConditionFlag.BoundByDuty56] && C.DisplayDuty || Svc.Condition[ConditionFlag.InCombat] && C.DisplayBattle);
}
