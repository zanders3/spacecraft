using System;

public class FillGenerator : IChunkGenerator
{
    private BlockType type;

    public FillGenerator(BlockType type)
    {
        this.type = type;
    }

    public BlockType[,,] Generate(Point3D chunkPos)
    {
        BlockType[,,] blocks = new BlockType[Chunk.BlockSize, Chunk.BlockSize, Chunk.BlockSize];
        for (int x = 0; x<Chunk.BlockSize; x++)
            for (int y = 0; y<Chunk.BlockSize; y++)
                for (int z = 0; z<Chunk.BlockSize; z++)
                    blocks[x,y,z] = type;
        return blocks;
    }
}

