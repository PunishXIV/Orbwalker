using Dalamud.Interface.Textures.TextureWraps;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Action = Lumina.Excel.Sheets.Action;

namespace Orbwalker;

internal static unsafe class Util
{
    internal static float GCD
    {
        get
        {
            RecastDetail* cd = ActionManager.Instance()->GetRecastGroupDetail(57);
            return cd->IsActive == 0 ? 0 : cd->Total - cd->Elapsed;
        }
    }
    internal static bool CheckTpRetMnt(uint acId, ActionType acType)
    {
        if (acId == Data.Teleport && !C.BlockTP) return true;
        if (acId == Data.Return && !C.BlockReturn) return true;
        return false;
    }

    internal static IEnumerable<uint> GetMovePreventionActions()
    {
        if (C.PreventFlame)
            foreach(uint x in Data.FlamethrowerAction)
                yield return x;
        if (C.PreventImprov)
            foreach(uint x in Data.ImprovisationAction)
                yield return x;
        if (C.PreventPassage)
            foreach(uint x in Data.PassageAction)
                yield return x;
        if (C.PreventPhantom)
            foreach(uint x in Data.PhantomFlurryAction)
                yield return x;
    }

    internal static IEnumerable<uint> GetMovePreventionStatuses()
    {
        if (C.PreventFlame) yield return Data.FlamethrowerBuff;
        if (C.PreventImprov) yield return Data.ImprovisationBuff;
        if (C.PreventPassage) yield return Data.PassageBuff;
        if (C.PreventPhantom) yield return Data.PhantomFlurryBuff;
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

        Job currentJob = (Job)Player.Object.ClassJob.RowId;

        if (!P.Config.EnabledJobs.ContainsKey(currentJob)) return false;
        return P.Config.EnabledJobs[currentJob];

        //if (currentJob == Job.PLD && C.PreventPassage) return true;
        //if (currentJob == Job.DNC && C.PreventImprov) return true;
        //if (currentJob == Job.MCH && C.PreventFlame) return true;
        //if (currentJob == Job.NIN && C.PreventTCJ) return true;
        //return currentJob.EqualsAny(Job.SMN, Job.ACN, Job.RDM, Job.BLM, Job.THM, Job.WHM, Job.CNJ, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU);
    }

    internal static Vector2 GetSize(this IDalamudTextureWrap t, float height) => new(t.Width * (height / t.Height), height);

    internal static bool IsActionCastable(uint id)
    {
        if (CastingWalkableAction()) return false;
        if (GetMovePreventionActions().Contains(id)) return true;
        ExcelSheet<Action> actionSheet = Svc.Data.GetExcelSheet<Action>();
        id = ActionManager.Instance()->GetAdjustedActionId(id);
        Action actionRow = actionSheet.GetRow(id);

        if (actionRow.Cast100ms <= 0)
        {
            return false;
        }

        ActionManager* actionManager = ActionManager.Instance();
        int adjustedCastTime = ActionManager.GetAdjustedCastTime(ActionType.Action, id);

        return adjustedCastTime > 0;
    }

    internal static bool CastingWalkableAction()
    {
        var id = Player.Object.CastActionId;
        if (id is 29391 or 29402 || Player.Object.CastActionType == (byte)ActionType.Mount)
            return true;

        return false;
    }
}
