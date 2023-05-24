namespace RaceManager.Effects
{
    public enum AudioType
    { 
        None = 0,

        MenuTrack_00 = 100,
        MenuTrack_01 = 101,
        MenuTrack_02 = 102,
        MenuTrack_03 = 103,

        RaceTrack_00 = 200,
        RaceTrack_01 = 201,
        RaceTrack_02 = 202,
        RaceTrack_03 = 203,

        SFX_ButtonPressed = 1000,
        SFX_DriftScoresCount = 1001,

        SFX_MetalCollision_Light = 1100,
        SFX_MetalCollision_Medium = 1101,
        SFX_MetalCollision_Heavy = 1102,

        SFX_PlasticCollision_Light = 1200,
        SFX_PlasticCollision_Medium = 1201,
        SFX_PlasticCollision_Heavy = 1202,

        SFX_StoneCollision_Light = 1300,
        SFX_StoneCollision_Medium = 1301,
        SFX_StoneCollision_Heavy = 1302,

        SFX_TreeCollision_Light = 1400,
        SFX_TreeCollision_Medium = 1401,
        SFX_TreeCollision_Heavy = 1402,

        SFX_GlassShards_Light = 1500,
        SFX_GlassShards_Medium = 1501,
        SFX_GlassShards_Heavy = 1502,

        SFX_DriftGravel = 1600,
        SFX_DriftAsphalt = 1601,
    }

    public enum ParticleType
    {
        None,

        MetalCollision_Light,
        MetalCollision_Medium,
        MetalCollision_Heavy,

        Drift_Asphalt,
        Drift_Gravel,
        Drift_Grass,
        Drift_Dirt
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
