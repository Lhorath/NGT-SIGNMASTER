using NerdyGamerTools.SignMaster.Core;

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

string dataPath = Path.Combine(AppContext.BaseDirectory, "default-signs.tsv");
using var reader = File.OpenText(dataPath);
SignStyleCatalog catalog = SignStyleCatalog.ParseTsv(reader);
var engine = new SignStyleEngine(catalog);
var options = new SignStyleOptions();

Assert(catalog.StyleCount == 285, $"Expected 285 styles, got {catalog.StyleCount}");
Assert(catalog.LabelCount == 256, $"Expected 256 labels, got {catalog.LabelCount}");

SignStyleResult wood = engine.Transform("WOOD", options);
Assert(wood.Changed, "WOOD should auto-style");
Assert(wood.Text == @"<#bf8><size=3>WOOD\n<size=5>🌳", "WOOD style mismatch");

SignStyleResult normalizedWood = engine.Transform("  wood  ", options);
Assert(normalizedWood.Text == wood.Text, "Case/whitespace normalization failed");

SignStyleResult ambiguous = engine.Transform("MEADOWS", options);
Assert(!ambiguous.Changed, "Bare MEADOWS must not guess a role");
Assert(ambiguous.Status == SignStyleStatus.AmbiguousLabel, "MEADOWS should report ambiguity");

SignStyleResult portalMeadows = engine.Transform("P:MEADOWS", options);
Assert(portalMeadows.Text == @"<#bf8><size=3>MEADOWS\n<size=5>🌱", "Portal MEADOWS mismatch");

SignStyleResult trophyMeadows = engine.Transform("T:MEADOWS", options);
Assert(trophyMeadows.Text == @"<#bf8><size=3>MEADOWS\n<size=5>🏆🌱", "Trophy MEADOWS mismatch");

SignStyleResult headerMeadows = engine.Transform("H:MEADOWS", options);
Assert(headerMeadows.Text.StartsWith("<#bf8><u><cspace=6><size=5>MEADOWS"), "Header MEADOWS mismatch");

SignStyleResult storageCopper = engine.Transform("S:COPPER", options);
SignStyleResult portalCopper = engine.Transform("P:COPPER", options);
Assert(storageCopper.Text.StartsWith("<#8d8>"), "Storage COPPER should use Black Forest color");
Assert(portalCopper.Text.StartsWith("<#ccc>"), "Portal COPPER should use Mining color");

SignStyleResult raw = engine.Transform("RAW:WOOD", options);
Assert(raw.Changed && raw.Text == "WOOD" && raw.Status == SignStyleStatus.RawBypass, "RAW bypass failed");

SignStyleResult alreadyStyled = engine.Transform(wood.Text, options);
Assert(!alreadyStyled.Changed && alreadyStyled.Status == SignStyleStatus.AlreadyStyled, "Known style should be idempotent");

SignStyleResult customRichText = engine.Transform("<#abc>WOOD", options);
Assert(!customRichText.Changed && customRichText.Status == SignStyleStatus.RichTextPreserved, "Manual rich text should be preserved");

SignStyleResult unknown = engine.Transform("MY CUSTOM SIGN", options);
Assert(!unknown.Changed && unknown.Status == SignStyleStatus.UnknownLabel, "Unknown label should remain unchanged");

var shortLimit = new SignStyleOptions { MaxStyledLength = 10 };
SignStyleResult tooLong = engine.Transform("WOOD", shortLimit);
Assert(!tooLong.Changed && tooLong.Status == SignStyleStatus.TooLong, "Length guard failed");

SignStyleResult reserved = engine.Transform("P:FROST CAVE", options);
Assert(reserved.Text.Contains("<i>"), "Reserved portal must preserve italic state");

SignStyleResult mainHub = engine.Transform("P:MAIN HUB", options);
Assert(mainHub.Text.Contains("<u>"), "MAIN HUB return portal must preserve underline state");

Console.WriteLine("All NGT SignMaster core tests passed.");
