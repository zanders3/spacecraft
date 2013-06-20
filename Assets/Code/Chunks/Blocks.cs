using System;

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
    
    //'Man made' blocks
    [BlockInfo("Steel Block", 7, 0)]
    Steel,
    [BlockInfo("Titanium Block", 8, 0)]
    Titanium,
    [BlockInfo("Fabricator", 10, 0)]
    Fabricator,
    [BlockInfo("Reactor Core", 9, 0)]
    ReactorCore,
    [BlockInfo("Thruster", 15, 0)]
    Thruster,
    [BlockInfo("Concrete", 0, 1)]
    Concrete
}

public class BlockInfoAttribute : Attribute
{
    public string Name { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }

    public BlockInfoAttribute(string name, int tileX, int tileY)
    {
        Name = name;
        TileX = tileX;
        TileY = tileY;
    }
}