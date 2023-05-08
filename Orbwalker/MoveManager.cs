using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbwalker
{
    internal static class MoveManager
    {
        internal static readonly int[] BlockedKeys = new int[] { 321, 322, 323, 324, 325, 326 };
        internal static bool MovingDisabled { get; private set; } = false;

        internal unsafe static void EnableMoving()
        {
            if (MovingDisabled)
            {
                PluginLog.Debug($"Enabling moving, cnt {P.Memory.ForceDisableMovement}");
                P.Memory.DisableHooks();
                if (P.Memory.ForceDisableMovement > 0)
                {
                    P.Memory.ForceDisableMovement--;
                }
                MovingDisabled = false;
            }
        }

        internal static void DisableMoving()
        {
            if (!MovingDisabled)
            {
                PluginLog.Debug($"Disabling moving, cnt {P.Memory.ForceDisableMovement}");
                P.Memory.EnableHooks();
                P.Memory.ForceDisableMovement++;
                MovingDisabled = true;
            }
        }
    }
}
