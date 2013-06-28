using System;
using UnityEngine;
using System.Collections.Generic;

public class ChunkCollisionManager
{
    private MeshCollider collider;
    private MeshFilter filter;
    private Stack<Transform> existingBoxes = new Stack<Transform>();
    private bool useMeshCollider;

    public ChunkCollisionManager(bool useMeshCollider, GameObject gameObject)
    {
        this.useMeshCollider = useMeshCollider;

        filter = gameObject.GetComponent<MeshFilter>();

        if (useMeshCollider)
        {
            collider = gameObject.GetComponent<MeshCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<MeshCollider>();
        }
    }

    public void UpdateCollision(BlockType[,,] blocks, Point3D chunkPos, Transform transform)
    {
        if (useMeshCollider)
        {
            collider.sharedMesh = null;
            collider.sharedMesh = filter.sharedMesh;
        } 
        else
        {
            Stack<Transform> newBoxes = new Stack<Transform>();

            int cx = chunkPos.x * Chunk.BlockSize, cy = chunkPos.y * Chunk.BlockSize, cz = chunkPos.z * Chunk.BlockSize;
            for (int x = 1; x<Chunk.BlockSize+1; x++)
            {
                for (int y = 1; y<Chunk.BlockSize+1; y++)
                {
                    for (int z = 1; z<Chunk.BlockSize+1; z++)
                    {
                        if (blocks[x, y, z] != BlockType.Empty)
                        {
                            if (blocks[x + 1, y, z] == BlockType.Empty || blocks[x, y + 1, z] == BlockType.Empty || blocks[x, y, z + 1] == BlockType.Empty ||
                                blocks[x - 1, y, z] == BlockType.Empty || blocks[x, y - 1, z] == BlockType.Empty || blocks[x, y, z - 1] == BlockType.Empty)
                            {
                                Transform box;
                                if (existingBoxes.Count > 0)
                                    box = existingBoxes.Pop();
                                else
                                {
                                    BoxCollider collider = new GameObject("Collider").AddComponent<BoxCollider>();
                                    collider.transform.parent = transform;
                                    collider.transform.localRotation = Quaternion.identity;
                                    collider.center = new Vector3(0.5f, 0.5f, 0.5f);
                                    box = collider.transform;
                                }
                                
                                box.localPosition = new Vector3(cx + x - 1, cy + y - 1, cz + z - 1);

                                newBoxes.Push(box);
                            }
                        }
                    }
                }
            }

            while (existingBoxes.Count > 0)
                GameObject.Destroy(existingBoxes.Pop().gameObject);

            existingBoxes = newBoxes;
        }
    }
}
