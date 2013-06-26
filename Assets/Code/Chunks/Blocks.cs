using System;
using System.Reflection;
using System.Linq;

public enum BlockType
{
    //'Natural' Blocks
    Empty,
    [BlockInfo("Dirt Block", 0, 0)]
    Dirt,
    [BlockInfo("Stone Block", 2, 0)]
    Stone,
    [BlockInfo("Iron Ore", 3, 0)]
    IronOre,
    [BlockInfo("Uranium Ore", 4, 0)]
    UraniumOre,
    [BlockInfo("Copper Ore", 5, 0)]
    CopperOre,
    [BlockInfo("Bedrock", 15, 0)]
    Bedrock,

    //'Man made' blocks
    [BlockInfo("Steel Block", 7, 0)]
    Steel,
    [BlockInfo("Power Core", "Prefabs/PowerCore", 1, 2, 1)]
    PowerCore,
    [BlockInfo("Thruster", "Prefabs/Thruster", 2, 2, 1)]
    Thruster,
    [BlockInfo("Pilot Seat", "Prefabs/PilotSeat", 1, 2, 1, true)]
    PilotSeat,
    [BlockInfo("Concrete", 0, 1)]
    Concrete
}

public class BlockInfoAttribute : Attribute
{
    public string Name;
    public int TileX;
    public int TileY;
    public bool IsPrefab, HasPlaceAction;

    public string Prefab;
    public Point3D PrefabSize;

    public BlockInfoAttribute(string name, int tileX, int tileY, bool hasPlaceAction = false)
    {
        Name = name;
        TileX = tileX;
        TileY = tileY;
        IsPrefab = false;
        HasPlaceAction = hasPlaceAction;
    }

    public BlockInfoAttribute(string name, string prefab, int sx, int sy, int sz, bool hasPlaceAction = false)
    {
        Name = name;
        Prefab = prefab;
        PrefabSize = new Point3D(sx, sy, sz);
        IsPrefab = true;
        HasPlaceAction = hasPlaceAction;
    }

    private static BlockInfoAttribute[] blockInfos;

    static BlockInfoAttribute()
    {
        string[] typeNames = System.Enum.GetNames(typeof(BlockType));
        blockInfos = new BlockInfoAttribute[typeNames.Length];
        UnityEngine.Debug.Log(typeNames.Length);
        for (int i = 0; i<typeNames.Length; i++)
        {
            MemberInfo info = typeof(BlockType).GetMember(typeNames[i]).FirstOrDefault();
            if (info != null)
            {
                blockInfos[i] = (BlockInfoAttribute)info.GetCustomAttributes(typeof(BlockInfoAttribute), false).FirstOrDefault();
            }
        }
    }

    public static BlockInfoAttribute GetInfo(BlockType type)
    {
        return blockInfos[(int)type];
    }
}