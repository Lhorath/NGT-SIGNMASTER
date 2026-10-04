using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NerdyGamerTools.SignMaster.Core;

namespace NerdyGamerTools.SignMaster
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SignMasterPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.nerdygamertools.signmaster";
        public const string PluginName = "NGT SignMaster";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log = null!;
        internal static SignStyleEngine Engine = null!;

        internal static ConfigEntry<bool> Enabled = null!;
        internal static ConfigEntry<bool> AutoStyleBareLabels = null!;
        internal static ConfigEntry<bool> EnableRolePrefixes = null!;
        internal static ConfigEntry<bool> RestyleExistingRichText = null!;
        internal static ConfigEntry<int> MaxStyledLength = null!;
        internal static ConfigEntry<bool> DebugLogging = null!;

        private Harmony? _harmony;

        private void Awake()
        {
            Log = Logger;
            BindConfiguration();

            try
            {
                SignStyleCatalog catalog = SignStyleCatalog.LoadEmbeddedDefaults(
                    Assembly.GetExecutingAssembly(),
                    "NerdyGamerTools.SignMaster.Resources.default-signs.tsv");

                Engine = new SignStyleEngine(catalog);

                _harmony = new Harmony(PluginGuid);
                _harmony.PatchAll();

                Logger.LogInfo(
                    $"{PluginName} {PluginVersion} loaded with {catalog.StyleCount} styles across " +
                    $"{catalog.LabelCount} labels.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to initialize {PluginName}: {ex}");
                Enabled.Value = false;
            }
        }

        internal static bool IsOperational =>
            Enabled != null && Enabled.Value && Engine != null;

        private void BindConfiguration()
        {
            Enabled = Config.Bind(
                "General",
                "Enabled",
                true,
                "Enable automatic sign styling.");

            AutoStyleBareLabels = Config.Bind(
                "General",
                "AutoStyleBareLabels",
                true,
                "Automatically style a plain label when it maps to exactly one known style.");

            EnableRolePrefixes = Config.Bind(
                "General",
                "EnableRolePrefixes",
                true,
                "Allow role prefixes such as P:, H:, T:, and S: to resolve ambiguous labels.");

            RestyleExistingRichText = Config.Bind(
                "General",
                "RestyleExistingRichText",
                false,
                "If enabled, SignMaster may replace manually formatted rich-text signs when their visible label matches a known rule.");

            MaxStyledLength = Config.Bind(
                "General",
                "MaxStyledLength",
                50,
                "Maximum stored sign-string length SignMaster will generate. The supplied 1.0.16 master is designed for 50 characters.");

            DebugLogging = Config.Bind(
                "Debug",
                "VerboseLogging",
                false,
                "Log style-resolution decisions to BepInEx.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        internal static SignStyleOptions CurrentOptions()
        {
            return new SignStyleOptions
            {
                AutoStyleBareLabels = AutoStyleBareLabels.Value,
                EnableRolePrefixes = EnableRolePrefixes.Value,
                RestyleExistingRichText = RestyleExistingRichText.Value,
                MaxStyledLength = System.Math.Max(1, MaxStyledLength.Value)
            };
        }
    }
}
