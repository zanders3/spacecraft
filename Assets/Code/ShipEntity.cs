using UnityEngine;
using System.Collections.Generic;

public class ShipEntity : Entity
{
    private int blockCount = 0;

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
        ship.transform.rotation = Quaternion.LookRotation(up) * Quaternion.AngleAxis(90.0f, Vector3.right);
    }

    public override void SetBlock(BlockType type, Point3D g)
    {
        if (type == BlockType.Empty)
            blockCount--;
        else
            blockCount++;

        if (blockCount <= 0)
            Destroy(gameObject);

        base.SetBlock(type, g);
    }
}

