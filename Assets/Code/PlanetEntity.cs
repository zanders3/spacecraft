using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A non moving Entity that represents a planet.
/// </summary>
public class PlanetEntity : Entity
{
    int planetScale = 3;
    public float PlanetScale { get { return Chunk.BlockSize * planetScale; } }
    
    public override bool UseMeshCollider { get { return true; } }

    protected override IChunkGenerator CreateGenerator()
    {
        return new PlanetGenerator(planetScale, this);
    }

    protected override List<Point3D> InitialiseBlocks()
    {
        List<Point3D> blocks = new List<Point3D>();
        for (int x = -planetScale*3; x<planetScale*3; x++)
            for (int y = -planetScale*3; y<planetScale*3; y++)
                for (int z = -planetScale*3; z<planetScale*3; z++)
                {
                    int xOut = x >= planetScale || x < -planetScale ? 1 : 0;
                    int yOut = y >= planetScale || y < -planetScale ? 1 : 0;
                    int zOut = z >= planetScale || z < -planetScale ? 1 : 0;
                    if ((xOut + yOut + zOut) == 1)
                        blocks.Add(new Point3D(x, y, z));
                }

        return blocks;
    }

    public override Vector3 TransformVertex(Vector3 pos)
    {
        Vector3 surfNormal = pos / PlanetScale;
        
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

        //surf normal at this point represents a point within the unit sphere. 
        //Height goes above 0 above the unit sphere to extrude above the surface.
        surfNormal = Cubize(surfNormal);
        return surfNormal * height * height * (PlanetScale * 0.2f);
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

    public override Vector3 InverseTransformVertex(Vector3 position)
    {
        Vector3 surfNormal = position / (PlanetScale * 0.2f);
        
        float height = surfNormal.magnitude;
        
        surfNormal = InverseCubize(surfNormal / height) * PlanetScale;
        position = surfNormal;
        
        height = (Mathf.Sqrt(height) * PlanetScale) - PlanetScale;

        Vector3 anyDir = new Vector3(Mathf.Abs(surfNormal.x), Mathf.Abs(surfNormal.y), Mathf.Abs(surfNormal.z));
        if (anyDir.y > anyDir.x && anyDir.y > anyDir.z)
        {
            position.y += surfNormal.y > 0.0f ? height : -height;
        }
        else if (anyDir.x > anyDir.y && anyDir.x > anyDir.z)
        {
            position.x += surfNormal.x > 0.0f ? height : -height;
        }
        else
        {
            position.z += surfNormal.z > 0.0f ? height : -height;
        }

        return position;
    }
    
    //http://stackoverflow.com/questions/2656899/mapping-a-sphere-to-a-cube
    //Maps a 3D sphere position to a cube position [-1,1]
    Vector3 InverseCubize(Vector3 position)
    {
        double x, y, z;
        x = position.x;
        y = position.y;
        z = position.z;
        
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
            } 
            else
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

    public override void SetBlock(BlockType type, Point3D g)
    {
        if (type == BlockType.PowerCore)
        {
            Vector3 pos = new Vector3(g.x, g.y, g.z);
            pos = TransformVertex(pos);
            Debug.Log(pos.normalized);
            ShipEntity.CreateShip(transform.TransformPoint(pos), pos.normalized);
        }
        else
            base.SetBlock(type, g);
    }
}
