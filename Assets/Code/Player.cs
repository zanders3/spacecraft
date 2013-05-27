using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    enum PlayerMovement
    {
        Planet,
        Space
    }

    PlayerMovement playerMovement = PlayerMovement.Space;
    Entity targetEntity = null;
    Point3D targetIndex, targetPlaceIndex;
    Vector3 targetNormal;

    bool mouseDown = false;

    void Start()
    {
        rigidbody.freezeRotation = true;
        Screen.lockCursor = true;
    }

    Vector3 gravityPosition = Vector3.zero;
    Vector3 up = Vector3.up;

    void UpdatePlayerPosition()
    {
        float dx = Input.GetAxis("LookX");
        float dy = -Input.GetAxis("LookY");

        if (playerMovement == PlayerMovement.Planet)
        {
            up = (transform.position - gravityPosition).normalized;
            rigidbody.AddForce(up * -20.0f);
        }

        rigidbody.rotation = Quaternion.AngleAxis(dy, transform.right) * Quaternion.AngleAxis(dx, up) * Quaternion.LookRotation(transform.forward, up);

        Vector3 forwardMoveVector = playerMovement == PlayerMovement.Planet ? Vector3.Cross(transform.right, up) : transform.forward;
        Vector3 movement = transform.right * Input.GetAxis("MoveX") + forwardMoveVector * Input.GetAxis("MoveZ");
        rigidbody.AddForce(movement - rigidbody.velocity * 0.1f, ForceMode.Impulse);
    }

    void Update()
    {
        UpdatePlayerPosition();

        targetEntity = null;

        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit))
        {
            Chunk chunk = hit.collider.GetComponent<Chunk>();
            if (chunk == null) chunk = hit.collider.transform.parent.GetComponent<Chunk>();

            if (chunk != null)
            {
                targetEntity = chunk.Entity;

                Vector3 normal, localPoint;
                chunk.Entity.InverseTransformVertex(hit.point, hit.normal, out localPoint, out normal);

                targetNormal = normal;
                //localPoint -= normal * 0.5f;

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

        if (!Input.GetButton("Place") && !Input.GetButton("Remove"))
            mouseDown = false;
    }

    void OnDrawGizmos()
    {
        if (targetEntity != null)
        {
            Gizmos.matrix = targetEntity.transform.localToWorldMatrix;

            {
                Vector3 tL = new Vector3(targetIndex.x, targetIndex.y, targetIndex.z);
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

            /*Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(targetPlaceIndex.x + 0.5f, targetPlaceIndex.y + 0.5f, targetPlaceIndex.z + 0.5f), new Vector3(1.0f, 1.0f, 1.0f));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(targetIndex.x + 0.5f, targetIndex.y + 0.5f, targetIndex.z + 0.5f), new Vector3(1.0f, 1.0f, 1.0f));

            Gizmos.color = Color.green;
            Vector3 p = new Vector3(targetIndex.x + 0.5f, targetIndex.y + 0.5f, targetIndex.z + 0.5f);
            Gizmos.DrawLine(p, p + targetNormal);*/
        }
    }
}

