using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NerdyGamerTools.SignMaster.Core
{
    public sealed class SignStyleOptions
    {
        public bool AutoStyleBareLabels { get; set; } = true;
        public bool EnableRolePrefixes { get; set; } = true;
        public bool RestyleExistingRichText { get; set; }
        public int MaxStyledLength { get; set; } = 50;
    }

    public enum SignStyleStatus
    {
        Applied,
        AlreadyStyled,
        Empty,
        RawBypass,
        UnknownLabel,
        AmbiguousLabel,
        RoleMismatch,
        RichTextPreserved,
        DisabledBareMatching,
        TooLong
    }

    public sealed class SignStyleResult
    {
        public SignStyleResult(string text, bool changed, SignStyleStatus status, string? normalizedLabel = null)
        {
            Text = text;
            Changed = changed;
            Status = status;
            NormalizedLabel = normalizedLabel;
        }

        public string Text { get; }
        public bool Changed { get; }
        public SignStyleStatus Status { get; }
        public string? NormalizedLabel { get; }
    }

    public sealed class SignStyleEngine
    {
        private static readonly Regex RolePrefixPattern = new Regex(
            @"^\s*(?<role>P|PORTAL|H|HEADER|T|TROPHY|S|STORAGE)\s*:\s*(?<label>.+?)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex RichTextTagPattern = new Regex(
            @"<[^>]+>",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly SignStyleCatalog _catalog;

        public SignStyleEngine(SignStyleCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public SignStyleResult Transform(string? input, SignStyleOptions options)
        {
            options ??= new SignStyleOptions();

            if (string.IsNullOrWhiteSpace(input))
            {
                return new SignStyleResult(input ?? string.Empty, false, SignStyleStatus.Empty);
            }

            if (_catalog.IsKnownStyledText(input))
            {
                return new SignStyleResult(input, false, SignStyleStatus.AlreadyStyled);
            }

            if (input.StartsWith("RAW:", StringComparison.OrdinalIgnoreCase))
            {
                string raw = input.Substring(4).TrimStart();
                return new SignStyleResult(raw, !string.Equals(raw, input, StringComparison.Ordinal), SignStyleStatus.RawBypass);
            }

            string candidate = input;
            bool hasRichText = LooksLikeRichText(candidate);

            if (hasRichText)
            {
                if (!options.RestyleExistingRichText)
                {
                    return new SignStyleResult(input, false, SignStyleStatus.RichTextPreserved);
                }

                candidate = ExtractVisibleLabel(candidate);
            }

            if (options.EnableRolePrefixes && TryParseRolePrefix(candidate, out SignRole role, out string explicitLabel))
            {
                IReadOnlyList<SignStyle> roleMatches = _catalog.Find(explicitLabel, role);
                if (roleMatches.Count == 0)
                {
                    return new SignStyleResult(input, false, SignStyleStatus.RoleMismatch, SignStyleCatalog.NormalizeLabel(explicitLabel));
                }

                return Apply(roleMatches[0], input, options);
            }

            if (!options.AutoStyleBareLabels)
            {
                return new SignStyleResult(input, false, SignStyleStatus.DisabledBareMatching);
            }

            string normalized = SignStyleCatalog.NormalizeLabel(candidate);
            IReadOnlyList<SignStyle> matches = _catalog.Find(normalized);

            if (matches.Count == 0)
            {
                return new SignStyleResult(input, false, SignStyleStatus.UnknownLabel, normalized);
            }

            if (matches.Count > 1)
            {
                return new SignStyleResult(input, false, SignStyleStatus.AmbiguousLabel, normalized);
            }

            return Apply(matches[0], input, options);
        }

        private static SignStyleResult Apply(SignStyle style, string original, SignStyleOptions options)
        {
            if (CountUnicodeCharacters(style.StyledText) > options.MaxStyledLength)
            {
                return new SignStyleResult(
                    original,
                    false,
                    SignStyleStatus.TooLong,
                    SignStyleCatalog.NormalizeLabel(style.Label));
            }

            return new SignStyleResult(
                style.StyledText,
                !string.Equals(style.StyledText, original, StringComparison.Ordinal),
                SignStyleStatus.Applied,
                SignStyleCatalog.NormalizeLabel(style.Label));
        }

        internal static int CountUnicodeCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < value.Length; index++)
            {
                if (char.IsHighSurrogate(value[index]) &&
                    index + 1 < value.Length &&
                    char.IsLowSurrogate(value[index + 1]))
                {
                    index++;
                }

                count++;
            }

            return count;
        }

        private static bool TryParseRolePrefix(string input, out SignRole role, out string label)
        {
            Match match = RolePrefixPattern.Match(input);
            if (!match.Success)
            {
                role = default;
                label = string.Empty;
                return false;
            }

            string roleToken = match.Groups["role"].Value.ToUpperInvariant();
            label = match.Groups["label"].Value.Trim();

            switch (roleToken)
            {
                case "P":
                case "PORTAL":
                    role = SignRole.Portal;
                    return true;
                case "H":
                case "HEADER":
                    role = SignRole.Header;
                    return true;
                case "T":
                case "TROPHY":
                    role = SignRole.Trophy;
                    return true;
                case "S":
                case "STORAGE":
                    role = SignRole.Storage;
                    return true;
                default:
                    role = default;
                    return false;
            }
        }

        private static bool LooksLikeRichText(string value)
        {
            int open = value.IndexOf('<');
            int close = value.IndexOf('>');
            return open >= 0 && close > open;
        }

        private static string ExtractVisibleLabel(string value)
        {
            int newlineIndex = value.IndexOf("\\n", StringComparison.Ordinal);
            string firstLine = newlineIndex >= 0 ? value.Substring(0, newlineIndex) : value;
            return RichTextTagPattern.Replace(firstLine, string.Empty).Trim();
        }
    }
}
