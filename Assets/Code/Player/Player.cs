using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    enum PlayerMovement
    {
        Planet,
        Space,
        Ship
    }

    public static Player Instance = null;

    public Transform Head;

    PlayerMovement playerMovement = PlayerMovement.Planet;
    Entity targetEntity = null;
    Point3D targetIndex, targetPlaceIndex;
    Vector3 targetNormal;
    Vector3 hitPos;

    Vector3 gravityPosition = Vector3.zero;
    Vector3 up = Vector3.up, right = Vector3.right, forward = Vector3.forward;
    public bool isColliding = false;

    float jetpackFuel = 1.0f;

    bool mouseDown = false;

    PlayerInventory inventory = new PlayerInventory();

    void Start()
    {
        Instance = this;
        rigidbody.freezeRotation = true;
        Screen.lockCursor = true;
    }

    void OnCollisionEnter(Collision coll)
    {
        if (isColliding)
            return;

        Transform parent = coll.gameObject.transform.parent;
        if (parent != null)
        {
            if (parent.GetComponent<PlanetEntity>() != null)
            {
                playerMovement = PlayerMovement.Planet;
            }
            else if (parent.parent != null && parent.parent.GetComponent<FoundationEntity>() != null)
            {
                playerMovement = PlayerMovement.Ship;
                up = parent.parent.up;
            }
        }

        for (int i = 0; i<coll.contacts.Length; i++)
            if (Vector3.Dot(coll.contacts[i].normal, up) > 0.9f)
            {
                isColliding = true;
                return;
            }
    }

    void OnCollisionStay(Collision coll)
    {
        OnCollisionEnter(coll);
    }

    void OnCollisionExit()
    {
        isColliding = false;
    }

    void OnGUI()
    {
        GUIHelper.DrawQuad(new Rect(10.0f, Screen.height - 40.0f, Screen.width - 20.0f, 30.0f), new Color(0.6f, 0.6f, 0.0f));
        GUIHelper.DrawQuad(new Rect(10.0f, Screen.height - 40.0f, (Screen.width - 20.0f) * jetpackFuel, 30.0f), Color.yellow);
        GUI.Label(new Rect(12.0f, Screen.height - 35.0f, 100.0f, 20.0f), "Jet Pack");

        inventory.DrawGUI();
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
        else if (playerMovement == PlayerMovement.Ship)
        {
            rigidbody.AddForce(up * -10.0f);
        }

        right = Head.transform.right;
        forward = Head.transform.forward;

        rigidbody.rotation = Quaternion.LookRotation(up) * Quaternion.AngleAxis(90.0f, Vector3.right);
        Head.transform.rotation = Quaternion.AngleAxis(dy, right) * Quaternion.AngleAxis(dx, up) * Quaternion.LookRotation(forward, up);
        Head.transform.position = rigidbody.transform.position;

        Vector3 forwardMoveVector = playerMovement == PlayerMovement.Planet || playerMovement == PlayerMovement.Ship ? Vector3.Cross(right, up) : forward;

        Vector2 moveAxis = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("MoveX"), Input.GetAxis("MoveZ")), 1.0f);
        Vector3 movement = right * moveAxis.x + forwardMoveVector * moveAxis.y;

        if (isColliding)
        {
            Vector3 drag = rigidbody.velocity - (Vector3.Dot(up, rigidbody.velocity) * up);

            rigidbody.AddForce(movement - drag * 0.2f, ForceMode.Impulse);
        }
        else if (playerMovement == PlayerMovement.Space)
        {
            rigidbody.AddForce(movement * 10.0f - rigidbody.velocity * 0.2f, ForceMode.Impulse);
        }
        else
        {
            rigidbody.AddForce(movement);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (isColliding)
                rigidbody.AddForce(up, ForceMode.Impulse);
            else if (jetpackFuel > 0.0f)
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
        inventory.Update();

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

                Vector3 localPoint = chunk.Entity.InverseTransformVertex(chunk.Entity.transform.InverseTransformPoint(hit.point + hit.normal * -0.5f));
                Vector3 localPlacePoint = chunk.Entity.InverseTransformVertex(chunk.Entity.transform.InverseTransformPoint(hitPos + hit.normal * 0.5f));

                targetIndex = new Point3D(Mathf.FloorToInt(localPoint.x), Mathf.FloorToInt(localPoint.y), Mathf.FloorToInt(localPoint.z));
                targetPlaceIndex = new Point3D(Mathf.FloorToInt(localPlacePoint.x), Mathf.FloorToInt(localPlacePoint.y), Mathf.FloorToInt(localPlacePoint.z));

                if (!mouseDown)
                {
                    if (Input.GetButtonDown("Place"))
                    {
                        mouseDown = true;
                        chunk.Entity.SetBlock(inventory.SelectedItem, targetPlaceIndex.x, targetPlaceIndex.y, targetPlaceIndex.z);
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
            Gizmos.color = Color.green;
            targetEntity.DrawVoxelOutline(targetPlaceIndex, 1.0f);

            Gizmos.color = Color.red;
            targetEntity.DrawVoxelOutline(targetIndex, 1.0f);

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(hitPos, hitPos + targetNormal);
        }
    }
}

