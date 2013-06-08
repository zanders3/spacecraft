using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a group of voxel chunks. A single entity is for example a planet. Chunks can move relative to each other and collide etc.
/// </summary>
public class Entity : MonoBehaviour
{
    public Material Material;

    private List<Chunk> chunkUpdates = new List<Chunk>();
    private ChunkStore chunkStore;

    public virtual bool UseMeshCollider { get { return true; } }

    protected virtual IChunkGenerator CreateGenerator()
    {
        return new PlanetGenerator(1, this);
    }

    protected virtual List<Point3D> InitialiseBlocks()
    {
        return new List<Point3D>();
    }

	void Start()
	{
		chunkStore = new ChunkStore(CreateGenerator(), Material, transform);
        foreach (Point3D pos in InitialiseBlocks())
        {
            chunkUpdates.Add(chunkStore.Add(pos.x * Chunk.BlockSize, pos.y * Chunk.BlockSize, pos.z * Chunk.BlockSize));
        }
	}

    void Update()
    {
        if (chunkUpdates.Count > 0)
        {
            Debug.Log(chunkUpdates.Count + " updates");

            foreach (Chunk chunk in chunkUpdates)
                chunk.UpdateChunk();

            chunkUpdates.Clear();
        }
    }
	
    public Chunk GetChunk(int cx, int cy, int cz)
    {
        return chunkStore.Get(cx, cy, cz);
    }

	public void SetBlock(BlockType type, int gx, int gy, int gz)
	{
		Chunk chunk = chunkStore.Add(gx, gy, gz);

        int x = gx < 0 ? Chunk.BlockSize - ((-gx-1) % Chunk.BlockSize) - 1 : gx % Chunk.BlockSize;
        int y = gy < 0 ? Chunk.BlockSize - ((-gy-1) % Chunk.BlockSize) - 1 : gy % Chunk.BlockSize;
        int z = gz < 0 ? Chunk.BlockSize - ((-gz-1) % Chunk.BlockSize) - 1 : gz % Chunk.BlockSize;

		chunk.SetBlock(type, x, y, z);

        if (x == 0)
            UpdateChunk(chunkStore.Get(gx - 1, gy, gz));
        else if (x == Chunk.BlockSize - 1)
            UpdateChunk(chunkStore.Get(gx + 1, gy, gz));

        if (y == 0)
            UpdateChunk(chunkStore.Get(gx, gy - 1, gz));
        else if (y == Chunk.BlockSize - 1)
            UpdateChunk(chunkStore.Get(gx, gy + 1, gz));

        if (z == 0)
            UpdateChunk(chunkStore.Get(gx, gy, gz - 1));
        else if (z == Chunk.BlockSize - 1)
            UpdateChunk(chunkStore.Get(gx, gy, gz + 1));

        UpdateChunk(chunk);
	}

    void UpdateChunk(Chunk chunk)
    {
        if (chunk != null && !chunkUpdates.Contains(chunk))
            chunkUpdates.Add(chunk);
    }

	public BlockType GetBlock(int x, int y, int z)
	{
		Chunk chunk = chunkStore.Get(x, y, z);
		if (chunk == null)
			return BlockType.Empty;
		else
			return chunk.GetBlock(x, y, z);
	}

    public virtual void TransformVertex(Point3D chunkPos, ref Vector3 pos)
    {
    }

    public virtual Vector3 InverseTransformVertex(Vector3 pos)
    {
        return transform.InverseTransformPoint(pos);
    }
}
