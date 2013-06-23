using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

public class ChunkTextureMapper
{
    public void MapTexture(BlockType type, ref List<Vector2> uvs)
    {
        BlockInfoAttribute blockInfo = BlockInfoAttribute.GetInfo(type);

        const float invTexWidth = 1.0f / 16.0f, invTexHeight = 1.0f / 16.0f;
        float offsetX = invTexWidth * blockInfo.TileX, offsetY = invTexHeight * (15 - blockInfo.TileY);

        uvs.Add(new Vector2(offsetX, offsetY));
        uvs.Add(new Vector2(offsetX + invTexWidth, offsetY));
        uvs.Add(new Vector2(offsetX, offsetY + invTexHeight));
        uvs.Add(new Vector2(offsetX + invTexWidth, offsetY + invTexHeight));
    }
}

