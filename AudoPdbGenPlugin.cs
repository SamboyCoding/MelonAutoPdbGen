using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;

[assembly:MelonInfo(typeof(MelonAutoPdbGen.AutoPdbGenPlugin), "AutoPdbGen", "1.0.0", "Samboy063")]
[assembly:MelonGame] //Universal
[assembly:MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)] //PdbGen is Windows only

namespace MelonAutoPdbGen;

public class AutoPdbGenPlugin : MelonPlugin
{
    private string? LastGameAssemblyHash;
    private readonly HttpClient _httpClient = new();
    private readonly MelonPreferences_ReflectiveCategory _configCategory = MelonPreferences.CreateCategory<Config>("AutoPdbGen");

    public override void OnEarlyInitializeMelon()
    {
        if (!MelonUtils.IsGameIl2Cpp())
        {
            LoggerInstance.WriteLine(Color.Yellow, 30);
            LoggerInstance.Warning("This game is not using IL2CPP, this plugin is useless.");
            LoggerInstance.WriteLine(Color.Yellow, 30);
            return;
        }
        
        var configInstance = _configCategory.GetValue<Config>();
        LastGameAssemblyHash = configInstance.LastGameAssemblyHash;

        //Only subscribe to events if the game is using IL2CPP
        MelonEvents.OnPreModsLoaded.Subscribe(OnPostAssemblyGeneration);
        
        LoggerInstance.Msg("AutoPdbGen initialized. Waiting for post-assembly generation event. LastGameAssemblyHash: " + LastGameAssemblyHash);
    }

    //Runs just after assemblies are generated
    private void OnPostAssemblyGeneration()
    {
        LoggerInstance.Msg("IL2CPP interop assemblies should now be available, checking PDB status");
        
        //Check if the last game assembly hash is different from the current one
        var assemblyPath = Path.Combine(MelonEnvironment.GameRootDirectory, "GameAssembly.dll");
        string actualAssemblyHash;

        using (var stream = File.OpenRead(assemblyPath)) 
            actualAssemblyHash = BitConverter.ToString(SHA256.Create().ComputeHash(stream)).Replace("-", "");
        
        if (actualAssemblyHash == LastGameAssemblyHash)
        {
            LoggerInstance.Msg("GameAssembly hash has not changed, skipping PDB generation.");
            return;
        }

        LoggerInstance.Msg("GameAssembly hash has changed, generating PDBs.");
        
        //Check if we have the required files
        var resourcesPath = Path.Combine(MelonEnvironment.UserDataDirectory, "AutoPdbGenResources");
        if(!Directory.Exists(resourcesPath))
            Directory.CreateDirectory(resourcesPath);

        var requiredAssets = new[] { "UnhollowerPdbGen.exe", "tbbmalloc.dll", "mspdbcore.dll", "msobj140.dll" };
        
        foreach (var assetName in requiredAssets)
        {
            var resourcePath = Path.Combine(resourcesPath, assetName);
            if (File.Exists(resourcePath)) 
                continue;

            //Asset is missing, download it
            var downloadUrl = $"https://github.com/SamboyCoding/Il2CppAssemblyUnhollower/releases/download/pdbgen/{assetName}";
            LoggerInstance.Msg($"Downloading missing asset: {assetName} from {downloadUrl}...");

            using var response = _httpClient.GetStreamAsync(downloadUrl).Result;
            using var fileStream = File.Create(resourcePath);
            
            response.CopyTo(fileStream);
            
            LoggerInstance.Msg($"Downloaded {assetName} successfully.");
        }
        
        LoggerInstance.Msg($"All required assets are now present. Executing {requiredAssets[0]}...");
        
        //Run the PDB generation process
        //<UnhollowerPdbGen.exe> <GameAssembly.dll> <Il2CppAssemblies\MethodAddressToToken.db>
        var methodAddressToTokenDbPath = Path.Combine(MelonEnvironment.Il2CppAssembliesDirectory, "MethodAddressToToken.db");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(resourcesPath, requiredAssets[0]),
                Arguments = $"\"{assemblyPath}\" \"{methodAddressToTokenDbPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        LoggerInstance.Msg("PDB generation process completed. The PDB should be next to GameAssembly.dll.");
        
        //Save the new hash
        var configInst = _configCategory.GetValue<Config>();
        configInst.LastGameAssemblyHash = actualAssemblyHash;
        _configCategory.SaveToFile();
    }
}