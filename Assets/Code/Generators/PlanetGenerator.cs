using UnityEngine;

public class PlanetGenerator : IChunkGenerator
{
    private int radius;
    private PlanetEntity planet;
    private FillGenerator fillGenerator;
    private SimplexNoiseGenerator noiseGen;

    public PlanetGenerator(int radius, PlanetEntity planet)
    {
        this.noiseGen = new SimplexNoiseGenerator();
        this.radius = radius;
        this.planet = planet;
        this.fillGenerator = new FillGenerator(BlockType.Dirt);
    }

    private float Lookup(Vector3 pos)
    {
        planet.TransformVertex(Point3D.Zero, ref pos);

        pos = pos * 0.1f + new Vector3(30000.0f, 30000.0f, 30000.0f);
        return noiseGen.noise(pos.x, pos.y, pos.z);
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
            (v111 * x * y * z);///WTFFFFFFF
    }

    public BlockType[,,] Generate(Point3D chunkPos)
    {
        BlockType[,,] blocks = new BlockType[Chunk.BlockSize, Chunk.BlockSize, Chunk.BlockSize];

        Vector3 p = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
        float v000 = Lookup(p);
        float v100 = Lookup(p + new Vector3(1.0f, 0.0f, 0.0f));
        float v010 = Lookup(p + new Vector3(0.0f, 1.0f, 0.0f));
        float v001 = Lookup(p + new Vector3(0.0f, 0.0f, 1.0f));
        float v101 = Lookup(p + new Vector3(1.0f, 0.0f, 1.0f));
        float v011 = Lookup(p + new Vector3(0.0f, 1.0f, 1.0f));
        float v110 = Lookup(p + new Vector3(1.0f, 1.0f, 0.0f));
        float v111 = Lookup(p + new Vector3(1.0f, 1.0f, 1.0f));

        float invBlockSize = 1.0f / Chunk.BlockSize;
        for (int x = 0; x < Chunk.BlockSize; x++)
            for (int y = 0; y < Chunk.BlockSize; y++)
                for (int z = 0; z < Chunk.BlockSize; z++)
                {
                    float noise = TrilinearInterpolate(
                        x * invBlockSize, y * invBlockSize, z * invBlockSize,
                        v000, v100, v010, v001, v101, v011, v110, v111);

                    blocks[x,y,z] = noise > 0.0f ? BlockType.Dirt : BlockType.Empty;
                }

        return blocks;
    }
}
