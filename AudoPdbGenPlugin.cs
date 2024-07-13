using System.Drawing;
using MelonLoader;

[assembly:MelonInfo(typeof(MelonAutoPdbGen.AutoPdbGenPlugin), "AutoPdbGen", "1.0.0", "Samboy063")]
[assembly:MelonGame]

namespace MelonAutoPdbGen;

public class AutoPdbGenPlugin : MelonPlugin
{
    public AutoPdbGenPlugin()
    {
        if (!MelonUtils.IsGameIl2Cpp())
        {
            LoggerInstance.WriteLine(Color.Yellow, 30);
            LoggerInstance.Warning("This game is not using IL2CPP, this plugin is useless.");
            LoggerInstance.WriteLine(Color.Yellow, 30);
            return;
        }

        //Only subscribe to events if the game is using IL2CPP
        MelonEvents.OnPreModsLoaded.Subscribe(OnPostAssemblyGeneration);
    }
    
    //Runs just after assemblies are generated
    private void OnPostAssemblyGeneration()
    {
        
    }
}