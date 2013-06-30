using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class StoredDictionary<T,U> : Dictionary<T, U>
{
    private string filename;

    public StoredDictionary(string filename)
    {
        this.filename = filename;

        if (File.Exists(filename))
        {
            Clear();
            foreach (string[] pair in File.ReadAllLines(filename).Select(line => line.Split(' ')).Where(pair => pair.Length == 2))
            {
                Add((T)Convert.ChangeType(pair[0], typeof(T)), (U)Convert.ChangeType(pair[1], typeof(U)));
            }
        }
    }

    public void SaveDictionary()
    {
        File.WriteAllLines(filename, this.Select(pair => pair.Key + " " + pair.Value).ToArray());
    }
}

/// <summary>
/// Saves and loads chunks for a single entity. This is achieved with two files, a position hash to file index file and the chunks themselves.
/// This implements chunk generator.
/// </summary>
public class EntityStore : IDisposable, IChunkGenerator
{
    private StoredDictionary<int, int> hashToFileOffset;
    private IChunkGenerator generator;
    private ChunkStorer chunkStorer;
    private int newChunkIndex;

    public EntityStore(IChunkGenerator generator, string entityID)
    {
        UnityEngine.Debug.Log(Path.GetFullPath(entityID + ".index"));
        this.generator = generator;
        this.hashToFileOffset = new StoredDictionary<int, int>(entityID + ".index");

        foreach (var pair in hashToFileOffset)
            newChunkIndex = Math.Max(newChunkIndex, pair.Value + 1);

        this.chunkStorer = new ChunkStorer(entityID + ".chunks");
    }

    public BlockType[,,] Generate(Point3D chunkPos)
    {
        int hash = ChunkStore.PositionHash(chunkPos);
        int chunkIndex;
        if (hashToFileOffset.TryGetValue(hash, out chunkIndex))
        {
            UnityEngine.Debug.Log("Load: " + chunkPos);
            return chunkStorer.Load(chunkIndex);
        }
        else
            return generator.Generate(chunkPos);
    }

    public void StoreChunk(Point3D chunkPos, BlockType[,,] blocks)
    {
        UnityEngine.Debug.Log("Store: " + chunkPos);
        int hash = ChunkStore.PositionHash(chunkPos);

        int chunkIndex;
        if (!hashToFileOffset.TryGetValue(hash, out chunkIndex))
        {
            chunkIndex = newChunkIndex++;

            hashToFileOffset.Add(hash, chunkIndex);
            hashToFileOffset.SaveDictionary();

            UnityEngine.Debug.Log("New Chunk: " + chunkIndex);
        }

        chunkStorer.Save(chunkIndex, blocks);
    }

    public void Dispose()
    {
        chunkStorer.Dispose();
    }
}
