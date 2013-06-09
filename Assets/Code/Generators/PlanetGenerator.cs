using UnityEngine;

public class PlanetGenerator : IChunkGenerator
{
    private Entity planet;
    private SimplexNoiseGenerator noiseGen;
    private int radius;

    public PlanetGenerator(int radius, Entity planet)
    {
        this.noiseGen = new SimplexNoiseGenerator();
        this.planet = planet;
        this.radius = radius;
    }

    private float Lookup(Vector3 pos)
    {
        float height = 1.0f;
        if (Mathf.Abs(pos.y) > 1.0f)
            height = pos.y > 0 ? pos.y : -pos.y;
        else if (Mathf.Abs(pos.x) > 1.0f)
            height = pos.x > 0 ? pos.x : -pos.x;
        else if (Mathf.Abs(pos.z) > 1.0f)
            height = pos.z > 0 ? pos.z : -pos.z;

        planet.TransformVertex(Point3D.Zero, ref pos);

        pos += new Vector3(30000.0f, 30000.0f, 30000.0f);

        return noiseGen.noise(pos.x, pos.y, pos.z) * 3.0f + (2.0f - height);
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

    bool LookupResource(Vector3 pos, float posScale, float noiseThreshold, float posOffset)
    {
        pos.x += posOffset;
        pos.y += posOffset;
        pos.z += posOffset;
        pos *= posScale;

        return noiseGen.noise(pos.x, pos.y, pos.z) > noiseThreshold;
    }

    public BlockType[,,] Generate(Point3D chunkPos)
    {
        BlockType[,,] blocks = new BlockType[Chunk.BlockSize, Chunk.BlockSize, Chunk.BlockSize];

        const float maxF = 1.0f;
        Vector3 p = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
        float v000 = Lookup(p);
        float v100 = Lookup(p + new Vector3(maxF, 0.0f, 0.0f));
        float v010 = Lookup(p + new Vector3(0.0f, maxF, 0.0f));
        float v001 = Lookup(p + new Vector3(0.0f, 0.0f, maxF));
        float v101 = Lookup(p + new Vector3(maxF, 0.0f, maxF));
        float v011 = Lookup(p + new Vector3(0.0f, maxF, maxF));
        float v110 = Lookup(p + new Vector3(maxF, maxF, 0.0f));
        float v111 = Lookup(p + new Vector3(maxF, maxF, maxF));

        bool inBedrock = chunkPos.x < radius && chunkPos.y < radius && chunkPos.z < radius && chunkPos.x >= -radius && chunkPos.y >= -radius && chunkPos.z >= -radius;

        float invBlockSize = 1.0f / Chunk.BlockSize;
        for (int x = 0; x < Chunk.BlockSize; x++)
        {
            for (int y = 0; y < Chunk.BlockSize; y++)
            {
                for (int z = 0; z < Chunk.BlockSize; z++)
                {
                    float noise = TrilinearInterpolate(
                        x * invBlockSize, y * invBlockSize, z * invBlockSize,
                        v000, v100, v010, v001, v101, v011, v110, v111);

                    BlockType type = BlockType.Empty;
                    if (noise > 0.0f)
                    {
                        if (inBedrock)
                            type = BlockType.Thruster;
                        else
                        {
                            Vector3 pos = new Vector3(x, y, z);
                            planet.TransformVertex(chunkPos, ref pos);

                            if (LookupResource(pos, 0.1f, 0.26f, 0.0f))
                                type = BlockType.UraniumOre;
                            else if (LookupResource(pos, 0.08f, 0.25f, 0.0f))
                                type = BlockType.CopperOre;
                            else if (LookupResource(pos, 0.06f, 0.23f, 1000.0f))
                                type = BlockType.IronOre;
                            else
                                type = BlockType.Stone;
                        }
                    }
                    blocks[x,y,z] = type;
                }
            }
        }

        return blocks;
    }
}
