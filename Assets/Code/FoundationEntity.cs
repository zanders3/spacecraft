using UnityEngine;

public class FoundationEntity : Entity
{
    protected override IChunkGenerator CreateGenerator()
    {
        return new FoundationGenerator();
    }

    public override void SetBlock(BlockType type, int gx, int gy, int gz)
    {
        if (gx >= 0 && gy > 0 && gz >= 0 && gx < Chunk.BlockSize && gy < Chunk.BlockSize && gz < Chunk.BlockSize)
            base.SetBlock(type, gx, gy, gz);
    }
}
