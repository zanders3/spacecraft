using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlanetGenerator : IChunkGenerator
{
    private static readonly List<BlockLayerInfo> layerInfo = new List<BlockLayerInfo>()
    {
        new BlockLayerInfo()
        {
            HeightLimit = 9.0f,
            HeightScale = 0.0f,
            Type = BlockType.Thruster
        },
        new BlockLayerInfo()
        {
            HeightLimit = 18.0f,
            HeightScale = 16.0f,
            Type = BlockType.Stone,
            Ores = new List<OreGenerator>()
            {
                new OreGenerator()
                {
                    PosScale = 0.1f,
                    NoiseThreshold = 0.26f,
                    Type = BlockType.UraniumOre
                },
                new OreGenerator()
                {
                    PosScale = 0.08f,
                    NoiseThreshold = 0.25f,
                    Type = BlockType.CopperOre
                },
                new OreGenerator()
                {
                    PosScale = 0.06f,
                    NoiseThreshold = 0.23f,
                    PosOffset = 1000.0f,
                    Type = BlockType.IronOre
                }
            }
        },
        new BlockLayerInfo()
        {
            HeightLimit = 20.0f,
            HeightScale = 16.0f,
            Type = BlockType.Dirt
        }
    };
    
    private Entity entity;
    private SimplexNoiseGenerator noiseGen;
    private List<LayerGenerator> layerGens = new List<LayerGenerator>();

    public PlanetGenerator(Entity entity)
    {
        this.noiseGen = new SimplexNoiseGenerator();
        this.entity = entity;
    }
    
    public BlockType[,,] Generate(Point3D chunkPos)
    {
        if (layerGens.Count != layerInfo.Count)
        {
            layerGens = new List<LayerGenerator>(layerInfo.Select(info => new LayerGenerator(info, entity, noiseGen)));
        }

        BlockType[,,] blocks = new BlockType[Chunk.BlockSize, Chunk.BlockSize, Chunk.BlockSize];

        for (int i = 0; i<layerGens.Count; i++)
            layerGens[i].CalculateChunk(chunkPos);

        for (int x = 0; x < Chunk.BlockSize; x++)
        {
            for (int y = 0; y < Chunk.BlockSize; y++)
            {
                for (int z = 0; z < Chunk.BlockSize; z++)
                {
                    BlockType type = BlockType.Empty;
                    for (int i = 0; i<layerGens.Count; i++)
                    {
                        type = layerGens[i].Lookup(x, y, z);
                        if (type != BlockType.Empty)
                            break;
                    }

                    blocks[x,y,z] = type;
                }
            }
        }

        return blocks;
    }
}
