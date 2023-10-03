using Dalamud.Interface.Internal;
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

namespace Orbwalker
{
    internal unsafe static class Util
    {
        internal static bool CheckTpRetMnt(uint acId, ActionType acType)
        {
            if (acId == Data.Teleport && !C.BlockTP) return true;
            if (acId == Data.Return && !C.BlockReturn) return true;
            if (acType == ActionType.Mount && !C.BlockMount) return true;
            return false;
        }

        internal static IEnumerable<uint> GetMovePreventionActions()
        {
            if (C.PreventFlame) foreach (var x in Data.FlamethrowerAction) yield return x;
            if (C.PreventImprov) foreach (var x in Data.ImprovisationAction) yield return x;
            if (C.PreventPassage) foreach (var x in Data.PassageAction) yield return x;
            if (C.PreventTCJ) foreach (var x in Data.TCJAction) yield return x;
        }

        internal static IEnumerable<uint> GetMovePreventionStatuses()
        {
            if (C.PreventFlame) yield return Data.FlamethrowerBuff;
            if (C.PreventImprov)  yield return Data.Improvisation;
            if (C.PreventPassage)  yield return Data.PassageBuff;
            if (C.PreventTCJ)  yield return Data.TCJBuff;
        }

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
            if (Svc.ClientState.IsPvP && !C.PVP) return false;
            if (C.BlockMount || C.BlockReturn || C.BlockTP) return true;

            Job currentJob = (Job)Player.Object.ClassJob.Id;

            if (!P.Config.EnabledJobs.ContainsKey(currentJob)) return false;
            return P.Config.EnabledJobs[currentJob];

            //if (currentJob == Job.PLD && C.PreventPassage) return true;
            //if (currentJob == Job.DNC && C.PreventImprov) return true;
            //if (currentJob == Job.MCH && C.PreventFlame) return true;
            //if (currentJob == Job.NIN && C.PreventTCJ) return true;
            //return currentJob.EqualsAny(Job.SMN, Job.ACN, Job.RDM, Job.BLM, Job.THM, Job.WHM, Job.CNJ, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU);
        }

        internal static Vector2 GetSize(this IDalamudTextureWrap t, float height)
        {
            return new Vector2(t.Width * (height / t.Height), height);
        }

        internal static bool IsActionCastable(uint id)
        {
            if (Util.GetMovePreventionActions().Contains(id)) return true;
            var actionSheet = Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();
            id = ActionManager.Instance()->GetAdjustedActionId(id);
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
