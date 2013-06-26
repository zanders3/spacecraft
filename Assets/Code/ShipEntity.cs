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

    public static void CreateShip(Vector3 pos, Vector3 up)
    {
        ShipEntity ship = new GameObject("Ship").AddComponent<ShipEntity>();
        ship.transform.position = pos;
        ship.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);
    }
}

