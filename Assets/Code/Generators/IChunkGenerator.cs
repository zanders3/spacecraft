using System;

public interface IChunkGenerator
{
    BlockType[,,] Generate(Point3D chunkPos);
}

