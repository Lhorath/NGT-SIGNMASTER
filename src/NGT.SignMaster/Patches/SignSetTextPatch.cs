using HarmonyLib;
using NerdyGamerTools.SignMaster.Core;

namespace NerdyGamerTools.SignMaster.Patches
{
    [HarmonyPatch(typeof(Sign), nameof(Sign.SetText))]
    internal static class SignSetTextPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref string text)
        {
            if (!SignMasterPlugin.IsOperational || string.IsNullOrEmpty(text))
            {
                return;
            }

            SignStyleResult result = SignMasterPlugin.Engine.Transform(
                text,
                SignMasterPlugin.CurrentOptions());

            if (result.Changed)
            {
                text = result.Text;
            }

            if (SignMasterPlugin.DebugLogging.Value)
            {
                SignMasterPlugin.Log.LogDebug(
                    $"Sign style resolution: status={result.Status}, label={result.NormalizedLabel ?? "(none)"}, changed={result.Changed}");
            }
        }
    }
}
