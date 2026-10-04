namespace NerdyGamerTools.SignMaster.Core
{
    public sealed class SignStyle
    {
        public SignStyle(SignRole role, string label, string styledText)
        {
            Role = role;
            Label = label;
            StyledText = styledText;
        }

        public SignRole Role { get; }
        public string Label { get; }
        public string StyledText { get; }
    }
}
