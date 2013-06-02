using System;

public class PlanetGenerator : IChunkGenerator
{
    private int radius;
    private FillGenerator fillGenerator;

    public PlanetGenerator(int radius)
    {
        this.radius = radius;
        this.fillGenerator = new FillGenerator(BlockType.Dirt);
    }

    public BlockType[,,] Generate(Point3D chunkPos)
    {
        /*if (Math.Abs(chunkPos.x) > radius && Math.Abs(chunkPos.y) > radius && Math.Abs(chunkPos.z) > radius)
        {

        }
        else*/
            return fillGenerator.Generate(chunkPos);
    }
}
