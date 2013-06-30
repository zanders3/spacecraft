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

    public static void CreateShip(Material material, Vector3 pos, Vector3 up)
    {
        ShipEntity ship = new GameObject("Ship").AddComponent<ShipEntity>();
        ship.gameObject.AddComponent<Rigidbody>();
        ship.Material = material;
        ship.transform.position = pos;
        ship.transform.rotation = Quaternion.LookRotation(up) * Quaternion.AngleAxis(90.0f, Vector3.right);
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

