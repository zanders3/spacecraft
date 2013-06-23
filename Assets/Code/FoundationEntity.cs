using UnityEngine;
using System.Collections.Generic;

public class FoundationEntity : Entity
{
    protected override IChunkGenerator CreateGenerator()
    {
        return new FoundationGenerator();
    }

    protected override List<Point3D> InitialiseBlocks()
    {
        return new List<Point3D>()
        {
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 0, 1),
            new Point3D(1, 0, 1)
        };
    }

    public override void SetBlock(BlockType type, int gx, int gy, int gz)
    {
        if (gx >= 0 && gy > 0 && gz >= 0 && gx < Chunk.BlockSize*2 && gy < Chunk.BlockSize*2 && gz < Chunk.BlockSize*2)
            base.SetBlock(type, gx, gy, gz);
    }
}
