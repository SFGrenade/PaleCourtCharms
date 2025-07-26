using System;

namespace PaleCourtCharms
{
    internal static class DebugModHook
    {
        public static void GiveAllCharms(Action callback)
        {
            DebugMod.BindableFunctions.OnGiveAllCharms += callback;
        }

        public static void RemoveAllCharms(Action callback)
        {
            DebugMod.BindableFunctions.OnRemoveAllCharms += callback;
        }
    }
}