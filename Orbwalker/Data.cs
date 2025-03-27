using ECommons.ExcelServices;
namespace Orbwalker;

internal class Data
{
    internal const uint PassageBuff = 1175;
    internal const uint FlamethrowerBuff = 1205;
    internal const uint ImprovisationBuff = 1827;
    internal const uint PhantomFlurryBuff = 2502;

    internal const uint Teleport = 5;
    internal const uint Return = 6;
    internal static uint[] PassageAction = new uint[] { 7385 };
    internal static uint[] FlamethrowerAction = new uint[] { 7418 };
    internal static uint[] ImprovisationAction = new uint[] { 16014 };
    internal static uint[] PhantomFlurryAction = new uint[] { 23288 };

    internal static Job[] CastingJobs = new[] { Job.SMN, Job.ACN, Job.RDM, Job.BLM, Job.THM, Job.WHM, Job.CNJ, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU, Job.PCT };
}
