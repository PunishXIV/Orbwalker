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
                return cd->IsActive == 0 ? 0 : cd->Total - cd->Elapsed;
            }
        }

        internal static float GetRCorGDC()
        {
            float castTimeRemaining = Player.Object.CastActionId != 0
                ? Player.Object.TotalCastTime - Player.Object.CurrentCastTime
                : 0;
        
            return Math.Max(castTimeRemaining, GCD);
        }

        internal static bool CanUsePlugin()
        {
            if (!Player.Available) return false;

            Job currentJob = (Job)Player.Object.ClassJob.Id;
            return currentJob.EqualsAny(Job.SMN, Job.RDM, Job.BLM, Job.WHM, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU);
        }

        internal static Vector2 GetSize(this TextureWrap t, float height)
        {
            return new Vector2(t.Width * (height / t.Height), height);
        }

        internal static bool IsActionCastable(uint id)
        {
            var actionSheet = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();
            var actionRow = actionSheet.GetRow(id);

            if (actionRow?.Cast100ms <= 0)
            {
                return false;
            }

            var actionManager = ActionManager.Instance();
            var adjustedCastTime = ActionManager.GetAdjustedCastTime(ActionType.Spell, id);

            return adjustedCastTime > 0;
        }

        internal static bool IsMouseMoveOrdered()
        {
            return IsKeyPressed(Keys.LButton) && IsKeyPressed(Keys.RButton);
        }
    }
}
