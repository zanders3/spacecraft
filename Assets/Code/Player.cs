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

    Vector3 gravityPosition = Vector3.zero;
    Vector3 up = Vector3.up, right = Vector3.right, forward = Vector3.forward;
    bool isColliding = false;

    float jetpackFuel = 1.0f;

    bool mouseDown = false;

    void Start()
    {
        rigidbody.freezeRotation = true;
        Screen.lockCursor = true;
    }

    void OnCollisionStay()
    {
        isColliding = true;
    }

    void DrawQuad(Rect position, Color color) 
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0,0,color);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }

    void OnGUI()
    {
        DrawQuad(new Rect(10.0f, Screen.height - 40.0f, Screen.width - 20.0f, 30.0f), new Color(0.6f, 0.6f, 0.0f));
        DrawQuad(new Rect(10.0f, Screen.height - 40.0f, (Screen.width - 20.0f) * jetpackFuel, 30.0f), Color.yellow);
    }

    void UpdatePlayerPosition()
    {
        float dx = Input.GetAxis("LookX");
        float dy = -Input.GetAxis("LookY");

        if (playerMovement == PlayerMovement.Planet)
        {
            up = (transform.position - gravityPosition).normalized;
            rigidbody.AddForce(up * -10.0f);
        }

        right = Head.transform.right;
        forward = Head.transform.forward;

        rigidbody.rotation = Quaternion.LookRotation(up) * Quaternion.AngleAxis(90.0f, Vector3.right);
        Head.transform.rotation = Quaternion.AngleAxis(dy, right) * Quaternion.AngleAxis(dx, up) * Quaternion.LookRotation(forward, up);
        Head.transform.position = rigidbody.transform.position;

        Vector3 forwardMoveVector = playerMovement == PlayerMovement.Planet ? Vector3.Cross(right, up) : forward;

        Vector2 moveAxis = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("MoveX"), Input.GetAxis("MoveZ")), 1.0f);
        Vector3 movement = right * moveAxis.x + forwardMoveVector * moveAxis.y;

        if (isColliding)
        {
            Vector3 drag = rigidbody.velocity - (Vector3.Dot(up, rigidbody.velocity) * up);
            isColliding = false;

            rigidbody.AddForce(movement - drag * 0.2f, ForceMode.Impulse);
        }
        else if (playerMovement == PlayerMovement.Space)
        {
            rigidbody.AddForce(movement - rigidbody.velocity * 0.2f);
        }
        else
        {
            rigidbody.AddForce(movement);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (jetpackFuel > 0.0f)
            {
                jetpackFuel = Mathf.Max(jetpackFuel - Time.deltaTime * 2.0f, 0.0f);
                rigidbody.AddForce(up * (isColliding ? 1000.0f : 20.0f));
            }
        }
        else if (jetpackFuel < 1.0f - Time.deltaTime)
            jetpackFuel += Time.deltaTime * 0.5f;
        else
            jetpackFuel = 1.0f;
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
                targetNormal = hit.normal;
                targetEntity = chunk.Entity;
                hitPos = hit.point;

                Vector3 localPoint = chunk.Entity.InverseTransformVertex(hit.point + hit.normal * -0.5f);
                Vector3 localPlacePoint = chunk.Entity.InverseTransformVertex(hitPos + hit.normal * 0.5f);

                targetIndex = new Point3D(Mathf.FloorToInt(localPoint.x), Mathf.FloorToInt(localPoint.y), Mathf.FloorToInt(localPoint.z));
                targetPlaceIndex = new Point3D(Mathf.FloorToInt(localPlacePoint.x), Mathf.FloorToInt(localPlacePoint.y), Mathf.FloorToInt(localPlacePoint.z));

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

        targetEntity.TransformVertex(Point3D.Zero, ref tL);
        targetEntity.TransformVertex(Point3D.Zero, ref tR);
        targetEntity.TransformVertex(Point3D.Zero, ref bL);
        targetEntity.TransformVertex(Point3D.Zero, ref bR);
        targetEntity.TransformVertex(Point3D.Zero, ref fTL);
        targetEntity.TransformVertex(Point3D.Zero, ref fTR);
        targetEntity.TransformVertex(Point3D.Zero, ref fBL);
        targetEntity.TransformVertex(Point3D.Zero, ref fBR);
        
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

