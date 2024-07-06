using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbwalker
{
    internal class Data
    {
        internal const uint PassageBuff = 1175;
        internal static uint[] PassageAction = new uint[] { 7385 };
        internal const uint TCJBuff = 1186;
        internal static uint[] TCJAction = new uint[] { 7403 };
        internal const uint FlamethrowerBuff = 1205;
        internal static uint[] FlamethrowerAction = new uint[] { 7418 };
        internal const uint Improvisation = 1827;
        internal static uint[] ImprovisationAction = new uint[] { 16014 };

        internal const uint Teleport = 5;
        internal const uint Return = 6;
        internal const uint Mount = 4;

        internal static Job[] CastingJobs = new Job[] { Job.SMN, Job.ACN, Job.RDM, Job.BLM, Job.THM, Job.WHM, Job.CNJ, Job.SCH, Job.AST, Job.SGE, Job.RPR, Job.SAM, Job.BLU, Job.PCT };
    }
}
