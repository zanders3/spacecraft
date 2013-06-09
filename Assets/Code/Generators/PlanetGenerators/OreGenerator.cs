using UnityEngine;

/// <summary>
/// Generates a specific type of Ore. Is contained within an enclosing Block Layer.
/// </summary>
public class OreGenerator
{
    /// <summary>
    /// Affects how big each deposit is.
    /// </summary>
    public float PosScale { get; set; }
    /// <summary>
    /// Affects how rare each deposit is.
    /// </summary>
    public float NoiseThreshold { get; set; }
    /// <summary>
    /// Adds a fixed offset to the generation so the noise isn't shared between ore generators.
    /// </summary>
    public float PosOffset { get; set; }

    public BlockType Type { get; set; }

    public bool Lookup(SimplexNoiseGenerator noiseGen, Vector3 pos)
    {
        pos.x += PosOffset;
        pos.y += PosOffset;
        pos.z += PosOffset;
        pos *= PosScale;
        
        return noiseGen.noise(pos.x, pos.y, pos.z) > NoiseThreshold;
    }
}

