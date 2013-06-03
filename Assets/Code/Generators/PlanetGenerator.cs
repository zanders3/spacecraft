using UnityEngine;

public class PlanetGenerator : IChunkGenerator
{
    private int radius;
    private PlanetEntity planet;
    private FillGenerator fillGenerator;

    public PlanetGenerator(int radius, PlanetEntity planet)
    {
        this.radius = radius;
        this.planet = planet;
        this.fillGenerator = new FillGenerator(BlockType.Dirt);
    }

    public BlockType[,,] Generate(Point3D chunkPos)
    {
        BlockType[,,] blocks = new BlockType[Chunk.BlockSize, Chunk.BlockSize, Chunk.BlockSize];
        
        /*Vector3 tL = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
            Vector3 tR = tL + Vector3.right;
            Vector3 bL = tL + Vector3.up;
            Vector3 bR = bL + Vector3.right;
            Vector3 fTL = tL + Vector3.forward;
            Vector3 fTR = fTL + Vector3.right;
            Vector3 fBL = fTL + Vector3.up;
            Vector3 fBR = fBL + Vector3.right;
            
            planet.TransformVertex(Point3D.Zero, ref tL);
            planet.TransformVertex(Point3D.Zero, ref tR);
            planet.TransformVertex(Point3D.Zero, ref bL);
            planet.TransformVertex(Point3D.Zero, ref bR);
            planet.TransformVertex(Point3D.Zero, ref fTL);
            planet.TransformVertex(Point3D.Zero, ref fTR);
            planet.TransformVertex(Point3D.Zero, ref fBL);
            planet.TransformVertex(Point3D.Zero, ref fBR);*/

        for (int x = 0; x < Chunk.BlockSize; x++)
            for (int y = 0; y < Chunk.BlockSize; y++)
                for (int z = 0; z < Chunk.BlockSize; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    planet.TransformVertex(chunkPos, ref pos);  
                pos *= 0.1f;
                    float noise = Noise.Generate(pos.x, pos.y, pos.z);
                    blocks[x,y,z] = noise > 0.0f ? BlockType.Dirt : BlockType.Empty;
                }

        return blocks;
    }
}
