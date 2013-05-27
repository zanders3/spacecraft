using UnityEngine;
using System.Collections.Generic;

public class PlanetEntity : Entity
{
    public const float PlanetScale = Chunk.BlockSize;
    
    public override bool UseMeshCollider { get { return true; } }

    public override void InverseTransformVertex(Vector3 pos, Vector3 normal, out Vector3 chunkPos, out Vector3 chunkNormal)
    {
        base.InverseTransformVertex(pos, normal, out chunkPos, out chunkNormal);

        Vector3 surfNormal = chunkPos / PlanetScale;
        
        //TODO: fix this. Accurate at the surface. Not so much lower down, or above.
        float height = surfNormal.magnitude;
        surfNormal = InverseCubize(surfNormal / height) * PlanetScale;
        
        chunkPos = (surfNormal * height) + new Vector3(PlanetScale, PlanetScale, PlanetScale);
        //Debug.Log(chunkPos.x + ", " + chunkPos.y + ", " + chunkPos.z);
        
        int nx = (int)surfNormal.x, ny = (int)surfNormal.y, nz = (int)surfNormal.z;
        
        //TODO: this bit doesn't work for sides and backfaces.
        if (nx == PlanetScale)
            chunkNormal = Vector3.right;
        else if (nx == -PlanetScale)
            chunkNormal = Vector3.left;
        else if (ny == PlanetScale)
            chunkNormal = Vector3.up;
        else if (ny == -PlanetScale)
            chunkNormal = Vector3.down;
        else if (nz == PlanetScale)
            chunkNormal = Vector3.forward;
        else if (nz == -PlanetScale)
            chunkNormal = Vector3.back;
    }
    
    public override void TransformVertex(Point3D chunkPos, Vector3 pos, Vector3 normal, ref List<Vector3> verts, ref List<Vector3> normals)
    {
        Vector3 chunkWorldPos = new Vector3(chunkPos.x * Chunk.BlockSize, chunkPos.y * Chunk.BlockSize, chunkPos.z * Chunk.BlockSize);
        Vector3 surfNormal = (pos + chunkWorldPos) / PlanetScale;

        float height = 1.0f;
        if (Mathf.Abs(surfNormal.y) > 1.0f)
        {
            height = surfNormal.y > 0 ? surfNormal.y : -surfNormal.y;
            surfNormal.y = surfNormal.y > 0 ? 1.0f : -1.0f;
        }
        else if (Mathf.Abs(surfNormal.x) > 1.0f)
        {
            height = surfNormal.x > 0 ? surfNormal.x : -surfNormal.x;
            surfNormal.x = surfNormal.x > 0 ? 1.0f : -1.0f;
        }
        else if (Mathf.Abs(surfNormal.z) > 1.0f)
        {
            height = surfNormal.z > 0 ? surfNormal.z : -surfNormal.z;
            surfNormal.z = surfNormal.z > 0 ? 1.0f : -1.0f;
        }
        
        surfNormal = Cubize(surfNormal);
        
        verts.Add((surfNormal * height * PlanetScale) - chunkWorldPos);
        
        if (Vector3.Dot(surfNormal, normal) > 0.5f)
            normals.Add(surfNormal);
        else
            normals.Add(normal);
    }
    
    //http://mathproofs.blogspot.co.uk/2005/07/mapping-cube-to-sphere.html
    //Maps a 3D cube position [-1,1] to a sphere of radius 1
    Vector3 Cubize(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;
        
        return new Vector3(
            p.x * Mathf.Sqrt(1.0f - y2 * 0.5f - z2 * 0.5f + y2 * z2 * 0.33333f),
            p.y * Mathf.Sqrt(1.0f - z2 * 0.5f - x2 * 0.5f + z2 * x2 * 0.33333f),
            p.z * Mathf.Sqrt(1.0f - x2 * 0.5f - y2 * 0.5f + x2 * y2 * 0.33333f)
            );
    }
    
    //http://stackoverflow.com/questions/2656899/mapping-a-sphere-to-a-cube
    //Maps a 3D sphere position to a cube position [-1,1]
    Vector3 InverseCubize(Vector3 position)
    {
        double x, y, z;
        x = (float)position.x;
        y = (float)position.y;
        z = (float)position.z;
        
        double fx, fy, fz;
        fx = System.Math.Abs(x);
        fy = System.Math.Abs(y);
        fz = System.Math.Abs(z);
        
        const double inverseSqrt2 = 0.70710676908493042;
        
        if (fy >= fx && fy >= fz)
        {
            double a2 = x * x * 2.0;
            double b2 = z * z * 2.0;
            double inner = -a2 + b2 - 3;
            double innersqrt = -System.Math.Sqrt((inner * inner) - 12.0 * a2);
            
            if (x == 0.0 || x == -0.0)
            { 
                position.x = 0.0f; 
            } else
            {
                position.x = (float)(System.Math.Sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2);
            }
            
            if (z == 0.0 || z == -0.0)
            {
                position.z = 0.0f;
            } else
            {
                position.z = (float)(System.Math.Sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2);
            }
            
            if (position.x > 1.0)
                position.x = 1.0f;
            if (position.z > 1.0)
                position.z = 1.0f;
            
            if (x < 0)
                position.x = -position.x;
            if (z < 0)
                position.z = -position.z;
            
            if (y > 0)
            {
                // top face
                position.y = 1.0f;
            } else
            {
                // bottom face
                position.y = -1.0f;
            }
        } 
        else if (fx >= fy && fx >= fz)
        {
            double a2 = y * y * 2.0;
            double b2 = z * z * 2.0;
            double inner = -a2 + b2 - 3;
            double innersqrt = -System.Math.Sqrt((inner * inner) - 12.0 * a2);
            
            if (y == 0.0 || y == -0.0)
            { 
                position.y = 0.0f; 
            } 
            else
            {
                position.y = (float)(System.Math.Sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2);
            }
            
            if (z == 0.0 || z == -0.0)
            {
                position.z = 0.0f;
            } 
            else
            {
                position.z = (float)(System.Math.Sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2);
            }
            
            if (position.y > 1.0)
                position.y = 1.0f;
            if (position.z > 1.0)
                position.z = 1.0f;
            
            if (y < 0)
                position.y = -position.y;
            if (z < 0)
                position.z = -position.z;
            
            if (x > 0)
            {
                // right face
                position.x = 1.0f;
            } 
            else
            {
                // left face
                position.x = -1.0f;
            }
        } 
        else
        {
            double a2 = x * x * 2.0;
            double b2 = y * y * 2.0;
            double inner = -a2 + b2 - 3;
            double innersqrt = -System.Math.Sqrt((inner * inner) - 12.0 * a2);
            
            if (x == 0.0 || x == -0.0)
            { 
                position.x = 0.0f; 
            } else
            {
                position.x = (float)(System.Math.Sqrt(innersqrt + a2 - b2 + 3.0) * inverseSqrt2);
            }
            
            if (y == 0.0 || y == -0.0)
            {
                position.y = 0.0f;
            } else
            {
                position.y = (float)(System.Math.Sqrt(innersqrt - a2 + b2 + 3.0) * inverseSqrt2);
            }
            
            if (position.x > 1.0)
                position.x = 1.0f;
            if (position.y > 1.0)
                position.y = 1.0f;
            
            if (x < 0)
                position.x = -position.x;
            if (y < 0)
                position.y = -position.y;
            
            if (z > 0)
            {
                // front face
                position.z = 1.0f;
            } else
            {
                // back face
                position.z = -1.0f;
            }
        }
        
        return position;
    }
}

