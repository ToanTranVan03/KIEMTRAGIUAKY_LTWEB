using TOEIC.Debugging;

namespace TOEIC;

public class TOEICConsts
{
    public const string LocalizationSourceName = "TOEIC";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "022b82baa27048a981e32135db08aafb";
}
