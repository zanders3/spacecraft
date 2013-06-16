using System;
using UnityEngine;
using System.Collections.Generic;

public class BlockLayerInfo
{
    public float HeightScale { get; set; }
    public float HeightLimit { get; set; }
    public BlockType Type { get; set; }
    
    /// <summary>
    /// Represents a list of ores to generate for this layer. The first ore takes priority, so you want the rarest ores first.
    /// </summary>
    public List<OreGenerator> Ores { get; set; }
}

/// <summary>
/// Represents a layer of a planet surface. e.g. Bedrock layer.
/// Each layer can contain multiple ores. WE MUST MINE DEEPAH
/// </summary>
public class LayerGenerator
{
    private const float InvBlockSize = 1.0f / Chunk.BlockSize;
    private float v000, v100, v010, v001, v101, v011, v110, v111;
    private BlockLayerInfo info;
    private Entity entity;
    private SimplexNoiseGenerator noiseGen;
    private int planetScale;
    private Point3D chunkPos;

    public LayerGenerator(int planetScale, BlockLayerInfo info, Entity entity, SimplexNoiseGenerator noiseGen)
    {
        this.planetScale = planetScale;
        this.info = info;
        this.entity = entity;
        this.noiseGen = noiseGen;
    }

    private float GetHeight(Vector3 pos)
    {
        const float negOffset = 0.5f / Chunk.BlockSize;
        
        float height = 1.0f;
        if (Mathf.Abs(pos.y) > planetScale)
            height = pos.y > 0 ? pos.y : -pos.y-negOffset;
        else if (Mathf.Abs(pos.x) > planetScale)
            height = pos.x > 0 ? pos.x : -pos.x-negOffset;
        else if (Mathf.Abs(pos.z) > planetScale)
            height = pos.z > 0 ? pos.z : -pos.z-negOffset;
        
        return height * Chunk.BlockSize;
    }

    private float Lookup(Vector3 pos)
    {
        float height = GetHeight(pos / Chunk.BlockSize);
        
        pos = entity.TransformVertex(pos);
        
        pos += new Vector3(30000.0f, 30000.0f, 30000.0f);
        
        return noiseGen.noise(pos.x, pos.y, pos.z) * info.HeightScale * planetScale  + (info.HeightLimit * planetScale - height);
    }

    /// <summary>
    /// Sets the chunk position. The noise is interpolated in each corner to smooth it.
    /// </summary>
    /// <param name="chunkPos">Chunk position.</param>
    public void CalculateChunk(Point3D chunkPos)
    {
        this.chunkPos = chunkPos;

        const float maxF = Chunk.BlockSize;
        Vector3 p = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z) * Chunk.BlockSize;
        v000 = Lookup(p);
        v100 = Lookup(p + new Vector3(maxF, 0.0f, 0.0f));
        v010 = Lookup(p + new Vector3(0.0f, maxF, 0.0f));
        v001 = Lookup(p + new Vector3(0.0f, 0.0f, maxF));
        v101 = Lookup(p + new Vector3(maxF, 0.0f, maxF));
        v011 = Lookup(p + new Vector3(0.0f, maxF, maxF));
        v110 = Lookup(p + new Vector3(maxF, maxF, 0.0f));
        v111 = Lookup(p + new Vector3(maxF, maxF, maxF));
    }

    //http://paulbourke.net/miscellaneous/interpolation/
    float TrilinearInterpolate(float x, float y, float z, float v000, float v100, float v010, float v001, float v101, float v011, float v110, float v111)
    {
        float ix = 1.0f - x, iy = 1.0f - y, iz = 1.0f - z;
        return 
                (v000 * ix * iy * iz) +
                (v100 * x * iy * iz) +
                (v010 * ix * y * iz) +
                (v001 * ix * iy * z) +
                (v101 * x * iy * z) +
                (v011 * ix * y * z) +
                (v110 * x * y * iz) +
                (v111 * x * y * z);
    }

    public BlockType Lookup(int x, int y, int z)
    {
        float noise = TrilinearInterpolate(
            x * InvBlockSize, y * InvBlockSize, z * InvBlockSize,
            v000, v100, v010, v001, v101, v011, v110, v111);

        if (noise <= 0.0f)
            return BlockType.Empty;
        else
        {
            Vector3 pos = new Vector3(x + chunkPos.x * Chunk.BlockSize, y + chunkPos.y * Chunk.BlockSize, z + chunkPos.z * Chunk.BlockSize);
            pos = entity.TransformVertex(pos);

            if (info.Ores != null)
            {
                foreach (OreGenerator ore in info.Ores)
                    if (ore.Lookup(noiseGen, pos))
                        return ore.Type;
            }

            return info.Type;
        }
    }
}

