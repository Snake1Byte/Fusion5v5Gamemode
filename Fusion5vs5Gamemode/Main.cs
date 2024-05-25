using System.Reflection;
using BoneLib;
using FieldInjector;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Utilities.DebugTools;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using MelonLoader;
using Module = LabFusion.SDK.Modules.Module;

namespace Fusion5vs5Gamemode;

public class Main : MelonMod
{
    public const string NAME = "Fusion5vs5Gamemode";
    public const string VERSION = "0.0.1";
    public const string AUTHOR = "Snake1Byte";

    public override void OnInitializeMelon()
    {
        SerialisationHandler.Inject<Fusion5vs5GamemodeDescriptor>();
        SerialisationHandler.Inject<Invoke5vs5UltEvent>();
        ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
        GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
        ImpactPropertiesPatches.Patch();
        // ProjectileRicochet.Enable();

#if DEBUG
        Hooking.OnLevelInitialized += DebugTools.StartGamemodeWithGame;
#endif
    }
    
#if DEBUG
    public override void OnUpdate()
    {
        DebugTools.OnUpdate();
    }
#endif
}

public class FusionModule : Module
{
    public static FusionModule? Instance { get; private set; }

    public override void OnModuleLoaded()
    {
        Instance = this;
    }
}
