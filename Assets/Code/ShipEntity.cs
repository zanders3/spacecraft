using UnityEngine;
using System.Collections.Generic;

public class ShipEntity : Entity
{
    public override bool UseMeshCollider
    {
        get
        {
            return false;
        }
    }

    protected override void Setup(out List<Point3D> blocksToInit, out IChunkGenerator chunkGenerator, out string entityID)
    {
        blocksToInit = new List<Point3D>() { Point3D.Zero };
        chunkGenerator = new FillGenerator(BlockType.Empty);
        entityID = "Ship";
    }

    protected override void OnSetupCompleted()
    {
        SetBlock(BlockType.PowerCore, Point3D.Zero);
    }

    protected override void OnChunksUpdated()
    {
        int mass = chunkStore.Mass;

        Debug.Log("Mass " + mass);
        if (mass <= 0)
        {
            Destroy(gameObject);
        }
    }
}

