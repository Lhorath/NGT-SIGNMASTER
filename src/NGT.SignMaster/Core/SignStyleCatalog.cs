using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NerdyGamerTools.SignMaster.Core
{
    public sealed class SignStyleCatalog
    {
        private readonly Dictionary<string, List<SignStyle>> _byLabel;
        private readonly HashSet<string> _styledTexts;

        private SignStyleCatalog(IEnumerable<SignStyle> styles)
        {
            List<SignStyle> materialized = styles.ToList();
            Styles = materialized.AsReadOnly();

            _byLabel = new Dictionary<string, List<SignStyle>>(StringComparer.OrdinalIgnoreCase);
            _styledTexts = new HashSet<string>(StringComparer.Ordinal);

            foreach (SignStyle style in materialized)
            {
                string key = NormalizeLabel(style.Label);
                if (!_byLabel.TryGetValue(key, out List<SignStyle>? bucket))
                {
                    bucket = new List<SignStyle>();
                    _byLabel.Add(key, bucket);
                }

                SignStyle? existingRole = bucket.FirstOrDefault(existing => existing.Role == style.Role);
                if (existingRole != null)
                {
                    if (string.Equals(existingRole.StyledText, style.StyledText, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate SignMaster rule for role '{style.Role}' and label '{style.Label}'.");
                    }

                    throw new InvalidOperationException(
                        $"Conflicting SignMaster rules for role '{style.Role}' and label '{style.Label}'. " +
                        "A role/label pair must resolve to exactly one styled string.");
                }

                bucket.Add(style);
                _styledTexts.Add(style.StyledText);
            }
        }

        public IReadOnlyList<SignStyle> Styles { get; }
        public int StyleCount => Styles.Count;
        public int LabelCount => _byLabel.Count;

        public static SignStyleCatalog LoadEmbeddedDefaults(Assembly assembly, string resourceName)
        {
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException(
                    $"Embedded SignMaster style resource '{resourceName}' could not be found.");
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            return ParseTsv(reader);
        }

        public static SignStyleCatalog ParseTsv(TextReader reader)
        {
            var styles = new List<SignStyle>();
            string? line;
            int lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split(new[] { '\t' }, 3);
                if (parts.Length != 3)
                {
                    throw new FormatException(
                        $"SignMaster style data line {lineNumber} must contain Role, Label, and StyledText.");
                }

                if (!Enum.TryParse(parts[0], true, out SignRole role))
                {
                    throw new FormatException(
                        $"Unknown SignMaster role '{parts[0]}' on line {lineNumber}.");
                }

                string label = parts[1].Trim();
                string styledText = parts[2];

                if (label.Length == 0 || styledText.Length == 0)
                {
                    throw new FormatException(
                        $"SignMaster style data line {lineNumber} contains an empty label or style.");
                }

                styles.Add(new SignStyle(role, label, styledText));
            }

            if (styles.Count == 0)
            {
                throw new FormatException("SignMaster style data did not contain any rules.");
            }

            return new SignStyleCatalog(styles);
        }

        public IReadOnlyList<SignStyle> Find(string label)
        {
            string key = NormalizeLabel(label);
            return _byLabel.TryGetValue(key, out List<SignStyle>? styles)
                ? styles
                : Array.Empty<SignStyle>();
        }

        public IReadOnlyList<SignStyle> Find(string label, SignRole role)
        {
            return Find(label).Where(style => style.Role == role).ToArray();
        }

        public bool IsKnownStyledText(string text) => _styledTexts.Contains(text);

        public static string NormalizeLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            bool pendingSpace = false;

            foreach (char c in value.Trim())
            {
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = true;
                    continue;
                }

                if (pendingSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(char.ToUpperInvariant(c));
                pendingSpace = false;
            }

            return builder.ToString();
        }
    }
}
