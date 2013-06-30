using System;
using System.IO;

public class ChunkStorer : IDisposable
{
    public int MaxIndex { get; private set; }
    public const int ChunkLength = Chunk.BlockSize * Chunk.BlockSize * Chunk.BlockSize;

    private FileStream fileStream;

    public ChunkStorer(string filename)
    {
        fileStream = new FileStream(filename, FileMode.OpenOrCreate);
        MaxIndex = (int)(fileStream.Length / ChunkLength);

        if (fileStream.Length % ChunkLength != 0)
            throw new InvalidDataException("File length must be a multiple of ChunkLength");
    }

    /// <summary>
    /// Loads the chunk from the file. Throws an exception if an non-existent index is requested.
    /// </summary>
    public BlockType[,,] Load(int index)
    {
        if (index >= MaxIndex)
            throw new ArgumentException("index >= MaxIndex (" + index + " >= " + MaxIndex + ")");

        fileStream.Seek(index * ChunkLength, SeekOrigin.Begin);

        BlockType[,,] blocks = new BlockType[Chunk.BlockSize,Chunk.BlockSize,Chunk.BlockSize];
        for (int x = 0; x<Chunk.BlockSize; x++)
            for (int y = 0; y<Chunk.BlockSize; y++)
                for (int z = 0; z<Chunk.BlockSize; z++)
                     blocks[x,y,z] = (BlockType)fileStream.ReadByte();

        return blocks;
    }
    
    /// <summary>
    /// Saves a chunk to the file. Block dimensions must be the Chunk.BlockSize.
    /// </summary>
    public void Save(int index, BlockType[,,] blocks)
    {
        if (blocks.Length != ChunkLength)
            throw new ArgumentException("Chunk length != block length (" + ChunkLength + " != " + blocks.Length + ")");

        fileStream.Seek(index * ChunkLength, SeekOrigin.Begin);

        for (int x = 0; x<Chunk.BlockSize; x++)
            for (int y = 0; y<Chunk.BlockSize; y++)
                for (int z = 0; z<Chunk.BlockSize; z++)
                    fileStream.WriteByte((byte)blocks[x, y, z]);
    }

    public void Dispose()
    {
        if (fileStream != null)
            fileStream.Close();
    }
}