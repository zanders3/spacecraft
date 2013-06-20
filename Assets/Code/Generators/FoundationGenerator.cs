using System;

public class FoundationGenerator : IChunkGenerator
{
    public BlockType[,,] Generate(Point3D chunkPos)
    {
        BlockType[,,] blocks = new BlockType[Chunk.BlockSize, Chunk.BlockSize, Chunk.BlockSize];
        for (int x = 0; x<Chunk.BlockSize; x++)
            for (int z = 0; z<Chunk.BlockSize; z++)
                blocks[x, 0, z] = BlockType.Concrete;

        return blocks;
    }
}

