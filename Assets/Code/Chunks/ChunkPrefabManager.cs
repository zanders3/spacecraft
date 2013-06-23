using UnityEngine;
using System.Collections.Generic;
using System;

class PrefabResourcesCache
{
    private Dictionary<string, GameObject> prefabResources = new Dictionary<string, GameObject>();

    public GameObject GetPrefab(string prefab)
    {
        GameObject prefabResource;
        if (prefabResources.TryGetValue(prefab, out prefabResource))
            return prefabResource;

        prefabResource = (GameObject)Resources.Load(prefab);
        prefabResources.Add(prefab, prefabResource);

        return prefabResource;
    }
}

public class ChunkPrefabManager
{
    private Transform transform;
    private static PrefabResourcesCache resourceCache = new PrefabResourcesCache();
    private Dictionary<string, Stack<GameObject>> existingPrefabs = new Dictionary<string, Stack<GameObject>>();

    public ChunkPrefabManager(Transform transform)
    {
        this.transform = transform;
    }

    public void UpdatePrefabs(Point3D chunkPos, BlockType[,,] blocks)
    {
        Dictionary<string, Stack<GameObject>> newPrefabs = new Dictionary<string, Stack<GameObject>>();

        int cx = chunkPos.x * Chunk.BlockSize, cy = chunkPos.y * Chunk.BlockSize, cz = chunkPos.z * Chunk.BlockSize;
        for (int x = 1; x<Chunk.BlockSize+1; x++)
        {
            for (int y = 1; y<Chunk.BlockSize+1; y++)
            {
                for (int z = 1; z<Chunk.BlockSize+1; z++)
                {
                    BlockInfoAttribute blockInfo = BlockInfoAttribute.GetInfo(blocks[x,y,z]);
                    if (blockInfo != null && blockInfo.IsPrefab)
                    {
                        Vector3 pos = new Vector3(cx + x - 1, cy + y - 1, cz + z - 1);

                        //Either grab an existing prefab or create a new one
                        GameObject prefab;
                        Stack<GameObject> existingPrefabStack;
                        if (existingPrefabs.TryGetValue(blockInfo.Prefab, out existingPrefabStack) && existingPrefabStack.Count > 0)
                        {
                            prefab = existingPrefabStack.Pop();
                            prefab.transform.localPosition = pos;
                        }
                        else
                        {
                            prefab = (GameObject)GameObject.Instantiate(resourceCache.GetPrefab(blockInfo.Prefab), transform.TransformPoint(pos), transform.rotation);
                        }

                        prefab.transform.parent = transform;

                        //Add it to the new prefab list
                        Stack<GameObject> newPrefabStack;
                        if (newPrefabs.TryGetValue(blockInfo.Prefab, out newPrefabStack))
                            newPrefabStack.Push(prefab);
                        else
                        {
                            Stack<GameObject> newStack = new Stack<GameObject>();
                            newStack.Push(prefab);
                            newPrefabs.Add(blockInfo.Prefab, newStack);
                        }
                    }
                }
            }
        }

        //Finally remove all unused prefabs
        foreach (var pair in existingPrefabs)
            foreach (var prefab in pair.Value)
            {
            Debug.Log("DESTROY");
                GameObject.Destroy(prefab);
            }

        //Replace with the new structure
        existingPrefabs = newPrefabs;
    }
}

