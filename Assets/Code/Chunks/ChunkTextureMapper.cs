using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

public class ChunkTextureMapper
{
    private BlockInfoAttribute[] blockInfos;

    public ChunkTextureMapper()
    {
        string[] typeNames = System.Enum.GetNames(typeof(BlockType));
        blockInfos = new BlockInfoAttribute[typeNames.Length];

        for (int i = 0; i<typeNames.Length; i++)
        {
            MemberInfo info = typeof(BlockType).GetMember(typeNames[i]).FirstOrDefault();
            if (info != null)
            {
                blockInfos[i] = (BlockInfoAttribute)info.GetCustomAttributes(typeof(BlockInfoAttribute), false).FirstOrDefault();
            }
        }
    }

    public void MapTexture(BlockType type, ref List<Vector2> uvs)
    {
        BlockInfoAttribute blockInfo = blockInfos[(int)type];
        if (blockInfo == null)
            throw new InvalidOperationException("Missing BlockInfo attribute!");

        const float invTexWidth = 1.0f / 16.0f, invTexHeight = 1.0f / 1.0f;
        float offsetX = invTexWidth * blockInfo.TileX, offsetY = invTexHeight * blockInfo.TileY;

        uvs.Add(new Vector2(offsetX, offsetY));
        uvs.Add(new Vector2(offsetX + invTexWidth, offsetY));
        uvs.Add(new Vector2(offsetX, offsetY + invTexHeight));
        uvs.Add(new Vector2(offsetX + invTexWidth, offsetY + invTexHeight));
    }
}

