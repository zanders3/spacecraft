using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a group of voxel chunks. A single entity is for example a planet. Chunks can move relative to each other and collide etc.
/// </summary>
[SelectionBase]
[ExecuteInEditMode]
public abstract class Entity : MonoBehaviour
{
    class PendingChunk
    {
        public Point3D ChunkPos;
        public Vector3 WorldPos;
        public float CameraDist;
    }

    public Material Material;
    public Camera Camera;

    //Chunks that will be processed on the next update
    private List<Chunk> chunkUpdates = new List<Chunk>();

    //A list of chunks that have yet to be generated
    private List<PendingChunk> pendingChunks = new List<PendingChunk>();

    //Chunks that are waiting to be saved to disk
    private List<Chunk> chunksChanged = new List<Chunk>();

    protected ChunkStore chunkStore;
    protected EntityStore entityStore;

    public virtual bool UseMeshCollider { get { return false; } }

    protected abstract void Setup(out List<Point3D> blocksToInit, out IChunkGenerator chunkGenerator, out string entityID);

    protected virtual void OnSetupCompleted()
    {
    }

    protected virtual void OnChunksUpdated()
    {
    }

	void Start()
	{
        if (!Application.isPlaying)
            return;

        List<Point3D> blocksToInit;
        IChunkGenerator generator;
        string entityID;
        Setup(out blocksToInit, out generator, out entityID);

        entityStore = new EntityStore(generator, entityID);
		chunkStore = new ChunkStore(entityStore, Material, transform);

        blocksToInit.AddRange(entityStore.GetSavedChunks());
        blocksToInit = blocksToInit.Distinct().ToList();

        pendingChunks = blocksToInit.Select(chunkPos => new PendingChunk()
        {
            ChunkPos = chunkPos,
            WorldPos = transform.TransformPoint(TransformVertex(new Vector3(chunkPos.x, chunkPos.y, chunkPos.z) * Chunk.BlockSize))
        }).ToList();

        OnSetupCompleted();
	}

    void OnDestroy()
    {
        if (entityStore != null)
        {
            entityStore.Dispose();
            entityStore = null;
        }
    }

    const float chunkInstantiateDistance = Chunk.BlockSize * 6;

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (pendingChunks.Count > 0)
        {
            for (int i = 0; i<pendingChunks.Count; i++)
                pendingChunks[i].CameraDist = Vector3.Distance(pendingChunks[i].WorldPos, Camera.transform.position);

            pendingChunks.Sort((a,b) => a.CameraDist.CompareTo(b.CameraDist));

            int numToProcess = Mathf.Min(pendingChunks.Count, 5);
            for (int i = 0; i<numToProcess; i++)
            {
                Point3D chunkPos = pendingChunks[i].ChunkPos;
                chunkUpdates.Add(chunkStore.Add(chunkPos.x * Chunk.BlockSize, chunkPos.y * Chunk.BlockSize, chunkPos.z * Chunk.BlockSize));
            }
            pendingChunks.RemoveRange(0, numToProcess);
        }

        if (chunkUpdates.Count > 0)
        {
            Debug.Log(chunkUpdates.Count + " updates " + pendingChunks.Count + " left");

            foreach (Chunk chunk in chunkUpdates)
                chunk.UpdateChunk();

            chunkUpdates.Clear();

            OnChunksUpdated();
        }

        if (chunksChanged.Count > 0)
        {
            Debug.Log("Saving " + chunksChanged.Count);

            foreach (Chunk chunk in chunksChanged)
                entityStore.StoreChunk(chunk.ChunkPos, chunk.GetBlocks());

            chunksChanged.Clear();
        }
    }
	
    public Chunk GetChunk(int cx, int cy, int cz)
    {
        return chunkStore.Get(cx, cy, cz);
    }

    public virtual void BlockAction(int x, int y, int z)
    {
    }

	public virtual void SetBlock(BlockType type, Point3D g)
	{
		Chunk chunk = chunkStore.Add(g.x, g.y, g.z);

        int x = g.x < 0 ? Chunk.BlockSize - ((-g.x-1) % Chunk.BlockSize) - 1 : g.x % Chunk.BlockSize;
        int y = g.y < 0 ? Chunk.BlockSize - ((-g.y-1) % Chunk.BlockSize) - 1 : g.y % Chunk.BlockSize;
        int z = g.z < 0 ? Chunk.BlockSize - ((-g.z-1) % Chunk.BlockSize) - 1 : g.z % Chunk.BlockSize;

		chunk.SetBlock(type, x, y, z);

        chunksChanged.Add(chunk);

        if (x == 0)
            UpdateChunk(chunkStore.Get(g.x - 1, g.y, g.z));
        else if (x == Chunk.BlockSize - 1)
            UpdateChunk(chunkStore.Get(g.x + 1, g.y, g.z));

        if (y == 0)
            UpdateChunk(chunkStore.Get(g.x, g.y - 1, g.z));
        else if (y == Chunk.BlockSize - 1)
            UpdateChunk(chunkStore.Get(g.x, g.y + 1, g.z));

        if (z == 0)
            UpdateChunk(chunkStore.Get(g.x, g.y, g.z - 1));
        else if (z == Chunk.BlockSize - 1)
            UpdateChunk(chunkStore.Get(g.x, g.y, g.z + 1));

        UpdateChunk(chunk);
	}

    void UpdateChunk(Chunk chunk)
    {
        if (chunk != null && !chunkUpdates.Contains(chunk))
            chunkUpdates.Add(chunk);
    }

	public BlockType GetBlock(int gx, int gy, int gz)
	{
		Chunk chunk = chunkStore.Get(gx, gy, gz);

        int x = gx < 0 ? Chunk.BlockSize - ((-gx-1) % Chunk.BlockSize) - 1 : gx % Chunk.BlockSize;
        int y = gy < 0 ? Chunk.BlockSize - ((-gy-1) % Chunk.BlockSize) - 1 : gy % Chunk.BlockSize;
        int z = gz < 0 ? Chunk.BlockSize - ((-gz-1) % Chunk.BlockSize) - 1 : gz % Chunk.BlockSize;

		if (chunk == null)
			return BlockType.Empty;
		else
			return chunk.GetBlock(x, y, z);
	}

    public virtual Vector3 TransformVertex(Vector3 pos)
    {
        return pos;
    }

    public virtual Vector3 InverseTransformVertex(Vector3 pos)
    {
        return pos;
    }

    public void DrawVoxelOutline(Point3D target, float size)
    {
        Vector3 tL = new Vector3(target.x, target.y, target.z);
        Vector3 tR = tL + Vector3.right * size;
        Vector3 bL = tL + Vector3.up * size;
        Vector3 bR = bL + Vector3.right * size;
        Vector3 fTL = tL + Vector3.forward * size;
        Vector3 fTR = fTL + Vector3.right * size;
        Vector3 fBL = fTL + Vector3.up * size;
        Vector3 fBR = fBL + Vector3.right * size;
        
        tL = TransformVertex(tL);
        tR = TransformVertex(tR);
        bL = TransformVertex(bL);
        bR = TransformVertex(bR);
        fTL = TransformVertex(fTL);
        fTR = TransformVertex(fTR);
        fBL = TransformVertex(fBL);
        fBR = TransformVertex(fBR);
        
        Gizmos.DrawLine(tL, tR);
        Gizmos.DrawLine(tR, bR);
        Gizmos.DrawLine(bR, bL);
        Gizmos.DrawLine(bL, tL);
        
        Gizmos.DrawLine(tL, fTL);
        Gizmos.DrawLine(tR, fTR);
        Gizmos.DrawLine(bR, fBR);
        Gizmos.DrawLine(bL, fBL);
        
        Gizmos.DrawLine(fTL, fTR);
        Gizmos.DrawLine(fTR, fBR);
        Gizmos.DrawLine(fBR, fBL);
        Gizmos.DrawLine(fBL, fTL);
    }
}
