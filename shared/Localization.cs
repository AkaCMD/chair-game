using Godot;

public partial class Localization : Node
{
    public override void _Ready()
    {
        string language = "automatic";

        if (language == "automatic")
        {
            string preferredLanguage = OS.GetLocaleLanguage();
            TranslationServer.SetLocale(preferredLanguage);
        }
        else
        {
            TranslationServer.SetLocale(language);
        }
    }
}
