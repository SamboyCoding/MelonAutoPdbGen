# MelonAutoPdbGen

MelonLoader plugin to automatically download and execute UnhollowerPdbGen to generate a PDB file for GameAssembly.dll, at the appropriate time. 

Only relevant for IL2CPP games on Windows. The plugin won't do anything in other situations.

## Features

- Automatically downloads required files for PDB Generation from [here](https://github.com/SamboyCoding/Il2CppAssemblyUnhollower/releases/tag/pdbgen)
- Automatically executes generation at the correct stage in the game loading process so that things work properly across updates and mods can use the PDB immediately.
- Automatically detects game updates and re-generates the PDB.
- Basically it's just fully automatic, throw this in your plugin folder and forget about it.
