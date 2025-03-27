namespace Orbwalker;

internal static class MoveManager
{
    internal static readonly int[] BlockedKeys = new[] { 321, 322, 323, 324, 325, 326 };
    internal static bool MovingDisabled { get; private set; }
    internal static bool MouseMovingDisabled { get; private set; }

    internal static void EnableMoving()
    {
        if (MovingDisabled)
        {
            PluginLog.Debug("Enabling moving"); // , cnt {P.Memory.ForceDisableMovement}");
            // Handle WASD Movement (and LMB+RMB Movement, if enabled)
            P.Memory.DisableHooks();
            // Handle Controller based Movement
            if (C.ControllerMode)
            {
                if (P.Memory.ForceDisableMovement > 0)
                {
                    P.Memory.ForceDisableMovement--;
                }
            }
            MovingDisabled = false;
        }
    }

    internal static void EnableMouseMoving()
    {
        if (MouseMovingDisabled)
        {
            PluginLog.Debug("Enabling Mouse Moving");
            // Handle WASD Movement (and LMB+RMB Movement, if enabled)
            P.Memory.DisableMouseAutoMoveHook();
            MouseMovingDisabled = false;
        }
    }

    internal static void DisableMoving()
    {
        if (!MovingDisabled)
        {
            PluginLog.Debug("Disabling moving"); // , cnt {P.Memory.ForceDisableMovement}");
            // Handle WASD Movement
            P.Memory.EnableHooks();

            // Handle LMB+RMB movement
            if (!C.DisableMouseDisabling)
            {
                P.Memory.EnableMouseAutoMoveHook();
            }

            // Handle Controller based Movement
            if (C.ControllerMode)
            {
                P.Memory.ForceDisableMovement++;
            }
            MovingDisabled = true;
        }
    }

    internal static void DisableMouseMoving()
    {
        if (!MouseMovingDisabled)
        {
            PluginLog.Debug("Disabling Mouse moving");
            // Handle LMB+RMB movement
            P.Memory.EnableMouseAutoMoveHook();
            MouseMovingDisabled = true;
        }
    }
}
