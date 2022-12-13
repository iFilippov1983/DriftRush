namespace RaceManager.Effects
{
    public enum AudioType
    { 
        None,

        MenuTrack_00,
        MenuTrack_01,

        RaceTrack_00,
        RaceTrack_01,

        SFX_CarHit,
        SFX_ButtonPressed,
    }

    public enum HapticType
    { 
        None,

        Selection,
        Success,
        Warning,
        Failure,
        Light,
        Medium,
        Heavy,
        Rigid,
        Soft
    }

    // Lofelt haptic types
    //
    // Selection : a light vibration on Android, and a light impact on iOS
    // Success : a light then heavy vibration on Android, and a success impact on iOS
    // Warning : a heavy then medium vibration on Android, and a warning impact on iOS
    // Failure : a medium / heavy / heavy / light vibration pattern on Android, and a failure impact on iOS
    // Light : a light impact on iOS and a short and light vibration on Android.
    // Medium : a medium impact on iOS and a medium and regular vibration on Android
    // Heavy : a heavy impact on iOS and a long and heavy vibration on Android
    // Rigid : a short and hard impact
    // Soft : a slightly longer and softer impact
}
