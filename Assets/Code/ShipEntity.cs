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

    protected override void Setup()
    {
        SetBlock(BlockType.PowerCore, Point3D.Zero);
    }

    protected override IChunkGenerator CreateGenerator()
    {
        return new FillGenerator(BlockType.Empty);
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

