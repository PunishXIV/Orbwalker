using ECommons;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.MathHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Unmoveable
{
    internal unsafe static class Util
    {
        internal static float GCD
        {
            get
            {
                var cd = ActionManager.Instance()->GetRecastGroupDetail(57);
                if (cd->IsActive == 0) return 0;
                return cd->Total - cd->Elapsed;
            }
        }

        internal static float GetRCorGDC()
        {
            float ret1 = 0;
            if(Player.Object.CastActionId != 0)
            {
                ret1 = Player.Object.TotalCastTime - Player.Object.CurrentCastTime;
            }
            return Math.Max(ret1, GCD);
        }

        internal static bool CanUsePlugin()
        {
            if (!Player.Available) return false;
            if (((Job)Player.Object.ClassJob.Id).EqualsAny(Job.SMN, Job.RDM, Job.BLM, Job.WHM, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU)) return true;
            return false;
        }

        internal static Vector2 GetSize(this TextureWrap t, float height)
        {
            return new Vector2(t.Width * (height / t.Height), height);
        }

        internal static bool IsActionCastable(uint id)
        {
            return Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(id)?.Cast100ms > 0 && ActionManager.GetAdjustedCastTime(ActionType.Spell, ActionManager.Instance()->GetAdjustedActionId(id)) > 0;
        }

        internal static bool IsMouseMoveOrdered()
        {
            /*if (Bitmask.IsBitSet(User32.GetKeyState((int)Keys.W), 15)) return true;
            if (Bitmask.IsBitSet(User32.GetKeyState((int)Keys.A), 15)) return true;
            if (Bitmask.IsBitSet(User32.GetKeyState((int)Keys.S), 15)) return true;
            if (Bitmask.IsBitSet(User32.GetKeyState((int)Keys.D), 15)) return true;*/
            if (IsKeyPressed(Keys.LButton) && IsKeyPressed(Keys.RButton)) return true;
            return false;
        }
    }
}
