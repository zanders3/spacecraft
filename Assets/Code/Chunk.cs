using UnityEngine;
using System.Collections.Generic;
using System;

public enum BlockType
{
    Dirt,
	Empty,
	IronOre,
	UraniumOre,
	CopperOre
}

public struct Point3D
{
    public static Point3D Zero { get { return new Point3D(0, 0, 0); } }

	public Point3D(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public int x, y, z;
}

//Renders the voxel mesh itself. Changes as blocks are placed.
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
	public const int BlockSize = 8;
	BlockType[,,] blocks = new BlockType[BlockSize+2, BlockSize+2, BlockSize+2];

    public Entity Entity
    {
        get { return transform.parent.GetComponent<Entity>(); }
    }

    public Point3D ChunkPos;

	public void SetBlock(BlockType type, int x, int y, int z)
	{
		blocks[x+1, y+1, z+1] = type;
	}

	public BlockType GetBlock(int x, int y, int z)
	{
		return blocks[x+1, y+1, z+1];
	}
	
    void CopyNeighbourChunk(Point3D minL, Point3D maxL, Point3D offset, Point3D chunkOffset)
    {
        Chunk chunk = Entity.GetChunk(ChunkPos.x * BlockSize + chunkOffset.x, ChunkPos.y * BlockSize + chunkOffset.y, ChunkPos.z * BlockSize + chunkOffset.z);
        if (chunk == null)
        {
            for (int x = minL.x; x<maxL.x; x++)
                for (int y = minL.y; y<maxL.y; y++)
                    for (int z = minL.z; z<maxL.z; z++)
                        blocks[x, y, z] = BlockType.Empty;
        }
        else
        {
            for (int x = minL.x; x<maxL.x; x++)
                for (int y = minL.y; y<maxL.y; y++)
                    for (int z = minL.z; z<maxL.z; z++)
                        blocks[x, y, z] = chunk.blocks[x + offset.x, y + offset.y, z + offset.z];
        }
    }

	public void UpdateChunk()
	{
        //Copy neighbour chunks
        {
            //Top + Bottom
            CopyNeighbourChunk(new Point3D(1, 0, 1), new Point3D(BlockSize+1, 1, BlockSize+1), new Point3D(0, BlockSize, 0), new Point3D(0, -1, 0));
            CopyNeighbourChunk(new Point3D(1, BlockSize+1, 1), new Point3D(BlockSize+1, BlockSize+2, BlockSize+1), new Point3D(0, -BlockSize, 0), new Point3D(0, BlockSize, 0));
            //Forward + Back
            CopyNeighbourChunk(new Point3D(1, 1, 0), new Point3D(BlockSize+1, BlockSize+1, 1), new Point3D(0, 0, BlockSize), new Point3D(0, 0, -1));
            CopyNeighbourChunk(new Point3D(1, 1, BlockSize+1), new Point3D(BlockSize+1, BlockSize+1, BlockSize+2), new Point3D(0, 0, -BlockSize), new Point3D(0, 0, BlockSize));
            //Right + Left
            CopyNeighbourChunk(new Point3D(0, 1, 1), new Point3D(1, BlockSize+1, BlockSize+1), new Point3D(BlockSize, 0, 0), new Point3D(-1, 0, 0));
            CopyNeighbourChunk(new Point3D(BlockSize+1, 1, 1), new Point3D(BlockSize+2, BlockSize+1, BlockSize+1), new Point3D(-BlockSize, 0, 0), new Point3D(BlockSize, 0, 0));
        }

        //Update the chunk mesh
        {
    		List<Vector3> verts = new List<Vector3>();
    		List<Vector2> tex = new List<Vector2>();
    		List<Vector3> normals = new List<Vector3>();
    		List<int> tris = new List<int>();

    		//Right + Left
    		GenerateMesh(new Point3D( 1, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0), new Point3D(1, 0, 0), false, ref verts, ref normals, ref tex, ref tris);
    		GenerateMesh(new Point3D(-1, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0), new Point3D(0, 0, 0), true, ref verts, ref normals, ref tex, ref tris);
    		//Forward + Back
    		GenerateMesh(new Point3D(0, 0, 1), new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 1), true, ref verts, ref normals, ref tex, ref tris);
    		GenerateMesh(new Point3D(0, 0,-1), new Point3D(1, 0, 0), new Point3D(0, 1, 0), new Point3D(0, 0, 0), false, ref verts, ref normals, ref tex, ref tris);
    		//Up + Down
    		GenerateMesh(new Point3D(0,-1, 0), new Point3D(1, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 0, 0), true, ref verts, ref normals, ref tex, ref tris);
    		GenerateMesh(new Point3D(0, 1, 0), new Point3D(1, 0, 0), new Point3D(0, 0, 1), new Point3D(0, 1, 0), false, ref verts, ref normals, ref tex, ref tris);

            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null) mesh = new Mesh();

            mesh.Clear();

            if (verts.Count > 0)
            {
        		mesh.vertices = verts.ToArray();
        		mesh.normals = normals.ToArray();
        		mesh.triangles = tris.ToArray();
        		mesh.uv = tex.ToArray();
            }

    		GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        //Update the collision mesh
        UpdateCollision();
	}

    void UpdateCollision()
    {
		if (Entity.UseMeshCollider)
        {
            MeshCollider collider = GetComponent<MeshCollider>();
            if (collider == null) collider = gameObject.AddComponent<MeshCollider>();

            collider.sharedMesh = null;
            collider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        } 
        else
        {
            Stack<Transform> existingBoxes = new Stack<Transform>();
            for (int i = 0; i<transform.childCount; i++)
                existingBoxes.Push(transform.GetChild(i));
        
            for (int x = 1; x<BlockSize+1; x++)
            {
                for (int y = 1; y<BlockSize+1; y++)
                {
                    for (int z = 1; z<BlockSize+1; z++)
                    {
                        if (blocks [x, y, z] != BlockType.Empty)
                        {
                            if (blocks [x + 1, y, z] == BlockType.Empty || blocks [x, y + 1, z] == BlockType.Empty || blocks [x, y, z + 1] == BlockType.Empty ||
                                blocks [x - 1, y, z] == BlockType.Empty || blocks [x, y - 1, z] == BlockType.Empty || blocks [x, y, z - 1] == BlockType.Empty)
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

                                box.localPosition = new Vector3(x - 1, y - 1, z - 1);
                            }
                        }
                    }
                }
            }

            while (existingBoxes.Count > 0)
                Destroy(existingBoxes.Pop().gameObject);
        }
    }

	void GenerateMesh(Point3D n, Point3D t, Point3D bn, Point3D o, bool flip, ref List<Vector3> verts, ref List<Vector3> normals, ref List<Vector2> tex, ref List<int> tris)
	{
		Vector3 normal = new Vector3(n.x, n.y, n.z);

		for (int x = 1; x<BlockSize+1; x++)
		{
			for (int y = 1; y<BlockSize+1; y++)
			{
				for (int z = 1; z<BlockSize+1; z++) 
				{
					if (blocks[x,y,z] != BlockType.Empty && blocks[x+n.x,y+n.y,z+n.z] == BlockType.Empty)
					{
						int i = verts.Count;
						if (flip)
						{
							tris.Add(i); tris.Add(i+1); tris.Add(i+2);
							tris.Add(i+1); tris.Add(i+3); tris.Add(i+2);
						}
						else
						{
							tris.Add(i); tris.Add(i+2); tris.Add(i+1);
							tris.Add(i+1); tris.Add(i+2); tris.Add(i+3);
						}

						int ox = x + o.x - 1, oy = y + o.y - 1, oz = z + o.z - 1;
                        Vector3 posA, posB, posC, posD;
						AddVertex(ChunkPos, new Vector3(ox, 		oy, 		 oz), ref verts, out posA);
						AddVertex(ChunkPos, new Vector3(ox+t.x,		oy+t.y, 	 oz+t.z), ref verts, out posB);
						AddVertex(ChunkPos, new Vector3(ox+bn.x,	oy+bn.y, 	 oz+bn.z), ref verts, out posC);
						AddVertex(ChunkPos, new Vector3(ox+t.x+bn.x,oy+t.y+bn.y, oz+t.z+bn.z), ref verts, out posD);

                        Vector3 faceNormal = Vector3.Cross((posC - posA), (posB - posA)).normalized;
                        if (Vector3.Dot(faceNormal, normal) < 0.0f)
                            faceNormal = -faceNormal;

                        faceNormal = transform.InverseTransformDirection(faceNormal);

                        normals.Add(faceNormal);
                        normals.Add(faceNormal);
                        normals.Add(faceNormal);
                        normals.Add(faceNormal);

						tex.Add(new Vector2(0.0f, 0.0f));
						tex.Add(new Vector2(1.0f, 0.0f));
						tex.Add(new Vector2(0.0f, 1.0f));
						tex.Add(new Vector2(1.0f, 1.0f));
					}
				}
			}
		}
	}

    void AddVertex(Point3D chunkPos, Vector3 pos, ref List<Vector3> verts, out Vector3 oPos)
    {
        Vector3 normal = Vector3.zero;
        Entity.TransformVertex(chunkPos, ref pos, ref normal);
        oPos = pos;
        verts.Add(pos);
    }

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(
			new Vector3(BlockSize * 0.5f, BlockSize * 0.5f, BlockSize * 0.5f),
			new Vector3(BlockSize, BlockSize, BlockSize)
		);
	}
}

