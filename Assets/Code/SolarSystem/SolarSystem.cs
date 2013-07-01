using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Manages entity instantiation, loading and removal for a solar system.
/// </summary>
public class SolarSystem : MonoBehaviour
{
    public Material Material;

    private float saveTimer = 0.0f;
    private List<Entity> entityList = new List<Entity>();
    private static SolarSystem instance;

    private const float saveInterval = 10.0f;
    private const string filename = "SolarSystem.index";
    private static readonly Dictionary<EntityType, Type> entityTypeMap = new Dictionary<EntityType, Type>()
    {
        { EntityType.Planet, typeof(PlanetEntity) },
        { EntityType.Ship, typeof(ShipEntity) }
    };

    void Start()
    {
        instance = this;

        List<EntityInfo> entities = EntityInfo.FromFile(filename);

        if (entities.Count == 0)
            entities = Generate();

        foreach (EntityInfo info in entities)
            InstantiateEntity(info);
    }

    List<EntityInfo> Generate()
    {
        return new List<EntityInfo>()
        {
            new EntityInfo()
            {
                ID = Guid.NewGuid().ToString().Replace("-", ""),
                Pos = Vector3.zero,
                Rot = Quaternion.identity,
                Type = EntityType.Planet
            }
        };
    }

    void Update()
    {
        saveTimer -= Time.deltaTime;
        if (saveTimer <= 0.0f)
        {
            Debug.Log("Save entity info");

            EntityInfo.ToFile(filename, entityList.Select(entity =>
            {
                string[] parts = entity.name.Split(' ');

                return new EntityInfo()
                {
                    ID = parts[2],
                    Type = (EntityType)Convert.ToInt32(parts[0]),
                    Pos = entity.transform.position,
                    Rot = entity.transform.rotation
                };
            }));

            saveTimer += saveInterval;
        }
    }

    public static void CreateShip(Vector3 pos, Vector3 up)
    {
        instance.InstantiateEntity(new EntityInfo()
        {
            ID = Guid.NewGuid().ToString().Replace("-", ""),
            Pos = pos,
            Rot = Quaternion.LookRotation(up) * Quaternion.AngleAxis(90.0f, Vector3.right),
            Type = EntityType.Ship
        });
    }

    void InstantiateEntity(EntityInfo info)
    {
        GameObject gameObject = new GameObject();
        gameObject.transform.position = info.Pos;
        gameObject.transform.rotation = info.Rot;
        gameObject.name = (int)info.Type + " " + info.Type + " " + info.ID;

        Entity entity = (Entity)gameObject.AddComponent(entityTypeMap[info.Type]);
        entity.Material = Material;
        entityList.Add(entity);

        saveTimer = 0.0f;
    }
}
