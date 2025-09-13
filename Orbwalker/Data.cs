using ECommons.ExcelServices;
namespace Orbwalker;

internal class Data
{
    internal const uint Teleport = 5;
    internal const uint Return = 6;

    internal const uint PassageBuff = 1175;
    internal const uint FlamethrowerBuff = 1205;
    internal const uint ImprovisationBuff = 1827;
    internal const uint PhantomFlurryBuff = 2502;
    internal const uint MeditateBuff = 1231;

    internal static uint[] PassageAction = [7385];
    internal static uint[] FlamethrowerAction = [7418];
    internal static uint[] ImprovisationAction = [16014];
    internal static uint[] PhantomFlurryAction = [23288];
    internal static uint[] MeditateAction = [7497];

    internal static Job[] CastingJobs = [Job.SMN, Job.ACN, Job.RDM, Job.BLM, Job.THM, Job.WHM, Job.CNJ, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU, Job.PCT];
    internal static Job[] HandJobs = [Job.CRP, Job.BSM, Job.ARM, Job.GSM, Job.LTW, Job.WVR, Job.ALC, Job.CUL];
    internal static Job[] LandJobs = [Job.MIN, Job.BTN, Job.FSH];
}
