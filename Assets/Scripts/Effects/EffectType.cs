namespace RaceManager.Effects
{
    public enum AudioType
    { 
        None,

        MenuTrack_00,
        MenuTrack_01,

        RaceTrack_00,
        RaceTrack_01,

        SFX_ButtonPressed,

        SFX_MetalCollision_Light,
        SFX_MetalCollision_Medium,
        SFX_MetalCollision_Heavy,

        SFX_PlasticCollision_Light,
        SFX_PlasticCollision_Medium,
        SFX_PlasticCollision_Heavy,

        SFX_StoneCollision_Light,
        SFX_StoneCollision_Medium,
        SFX_StoneCollision_Heavy,

        SFX_TreeCollision_Light,
        SFX_TreeCollision_Medium,
        SFX_TreeCollision_Heavy,

        SFX_GlassShards_Light,
        SFX_GlassShards_Medium,
        SFX_GlassShards_Heavy,

        SFX_DriftGravel,
        SFX_DriftAsphalt,
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

    public enum ParticleType
    {
        None,

        MetalCollision_Light,
        MetalCollision_Medium,
        MetalCollision_Heavy,

        GlassShards_Light,
        GlassShards_Medium,
        GlassShards_Heavy,
    }
}
