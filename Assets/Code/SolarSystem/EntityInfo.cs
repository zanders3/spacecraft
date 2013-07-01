using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum EntityType
{
    Ship,
    Planet
}

public class EntityInfo
{
    public string ID;
    public EntityType Type;
    public Vector3 Pos;
    public Quaternion Rot;
    
    static EntityInfo FromString(string line)
    {
        string[] parts = line.Split(',');
        return new EntityInfo()
        {
            ID = parts[0], 
            Type = (EntityType)Convert.ToInt32(parts[1]),
            Pos = new Vector3(Convert.ToSingle(parts[2]), Convert.ToSingle(parts[3]), Convert.ToSingle(parts[4])),
            Rot = new Quaternion(Convert.ToSingle(parts[5]), Convert.ToSingle(parts[6]), Convert.ToSingle(parts[7]), Convert.ToSingle(parts[8]))
        };
    }
    
    public override string ToString()
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
            ID,
            (int)Type,
            Pos.x, Pos.y, Pos.z,
            Rot.x, Rot.y, Rot.z, Rot.w
        );
    }

    public static List<EntityInfo> FromFile(string filename)
    {
        if (File.Exists(filename))
            return File.ReadAllLines(filename).Select(line => FromString(line)).ToList();
        else
            return new List<EntityInfo>();
    }

    public static void ToFile(string filename, IEnumerable<EntityInfo> entities)
    {
        File.WriteAllLines(filename, entities.Select(info => info.ToString()).ToArray());
    }
}
