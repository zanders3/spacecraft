using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    enum PlayerMovement
    {
        Planet,
        Space
    }

    public Transform Head;

    PlayerMovement playerMovement = PlayerMovement.Planet;
    Entity targetEntity = null;
    Point3D targetIndex, targetPlaceIndex;
    Vector3 targetNormal;
    Vector3 hitPos;

    bool mouseDown = false;

    void Start()
    {
        rigidbody.freezeRotation = true;
        Screen.lockCursor = true;
    }

    Vector3 gravityPosition = Vector3.zero;
    Vector3 up = Vector3.up, right = Vector3.right, forward = Vector3.forward;

    void UpdatePlayerPosition()
    {
        float dx = Input.GetAxis("LookX");
        float dy = -Input.GetAxis("LookY");

        if (playerMovement == PlayerMovement.Planet)
        {
            up = (transform.position - gravityPosition).normalized;
            rigidbody.AddForce(up * -20.0f);
        }

        right = Head.transform.right;
        forward = Head.transform.forward;

        rigidbody.rotation = Quaternion.LookRotation(up) * Quaternion.AngleAxis(90.0f, Vector3.right);
        Head.transform.rotation = Quaternion.AngleAxis(dy, right) * Quaternion.AngleAxis(dx, up) * Quaternion.LookRotation(forward, up);
        Head.transform.position = rigidbody.transform.position;

        Vector3 forwardMoveVector = playerMovement == PlayerMovement.Planet ? Vector3.Cross(right, up) : forward;
        Vector3 movement = right * Input.GetAxis("MoveX") + forwardMoveVector * Input.GetAxis("MoveZ");
        rigidbody.AddForce(movement - rigidbody.velocity * 0.1f, ForceMode.Impulse);
    }

    void Update()
    {
        UpdatePlayerPosition();

        targetEntity = null;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(Head.transform.position, forward), out hit))
        {
            Chunk chunk = hit.collider.GetComponent<Chunk>();
            if (chunk == null) chunk = hit.collider.transform.parent.GetComponent<Chunk>();

            if (chunk != null)
            {
                targetEntity = chunk.Entity;

                Vector3 normal, localPoint;
                hitPos = hit.point;
                chunk.Entity.InverseTransformVertex(hit.point, hit.normal, out localPoint, out normal);

                targetNormal = hit.normal;
                //TODO: better handling of normals for placement

                targetIndex = new Point3D(Mathf.FloorToInt(localPoint.x), Mathf.FloorToInt(localPoint.y), Mathf.FloorToInt(localPoint.z));
                targetPlaceIndex = new Point3D(targetIndex.x + (int)normal.x, targetIndex.y + (int)normal.y, targetIndex.z + (int)normal.z);

                if (!mouseDown)
                {
                    if (Input.GetButtonDown("Place"))
                    {
                        mouseDown = true;
                        chunk.Entity.SetBlock(BlockType.Dirt, targetPlaceIndex.x, targetPlaceIndex.y, targetPlaceIndex.z);
                    }
                    else if (Input.GetButtonDown("Remove"))
                    {
                        mouseDown = true;
                        chunk.Entity.SetBlock(BlockType.Empty, targetIndex.x, targetIndex.y, targetIndex.z);
                    }
                }
            }  
        }

        if (Input.GetKeyDown(KeyCode.Space) && !mouseDown)
        {
            mouseDown = true;
            playerMovement = playerMovement == PlayerMovement.Planet ? PlayerMovement.Space : PlayerMovement.Planet;
        }
        else if (!Input.GetButton("Place") && !Input.GetButton("Remove"))
            mouseDown = false;
    }

    void DrawVoxelOutline(Point3D target)
    {
        Vector3 tL = new Vector3(target.x, target.y, target.z);
        Vector3 tR = tL + Vector3.right;
        Vector3 bL = tL + Vector3.up;
        Vector3 bR = bL + Vector3.right;
        Vector3 fTL = tL + Vector3.forward;
        Vector3 fTR = fTL + Vector3.right;
        Vector3 fBL = fTL + Vector3.up;
        Vector3 fBR = fBL + Vector3.right;
        
        Vector3 normal = Vector3.up;
        targetEntity.TransformVertex(Point3D.Zero, ref tL, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref tR, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref bL, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref bR, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref fTL, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref fTR, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref fBL, ref normal);
        targetEntity.TransformVertex(Point3D.Zero, ref fBR, ref normal);
        
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

    void OnDrawGizmos()
    {
        if (targetEntity != null)
        {
            Gizmos.matrix = targetEntity.transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            DrawVoxelOutline(targetPlaceIndex);

            Gizmos.color = Color.red;
            DrawVoxelOutline(targetIndex);

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(hitPos, hitPos + targetNormal);
        }
    }
}

