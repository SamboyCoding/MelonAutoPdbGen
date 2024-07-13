using Tomlet.Attributes;

namespace MelonAutoPdbGen;

public class Config
{
    [TomlPrecedingComment("SHA256 hash of the last game assembly that a PDB was generated from, if any. Used to determine if a new PDB is needed.")]
    public string? LastGameAssemblyHash;
}