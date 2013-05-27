using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    Entity targetEntity = null;
    Point3D targetIndex, targetPlaceIndex;
    Vector3 targetNormal;

    bool mouseDown = false;

    void Start()
    {
        Screen.lockCursor = true;
    }

    Vector3 gravityPosition = new Vector3(16.0f, 16.0f, -16.0f);

    void UpdatePlayerPosition()
    {
        float dx = Input.GetAxis("LookX");
        float dy = -Input.GetAxis("LookY");

        Vector3 up = (transform.position - gravityPosition).normalized;
        rigidbody.rotation = Quaternion.AngleAxis(dy, transform.right) * Quaternion.AngleAxis(dx, up) * Quaternion.LookRotation(transform.forward, up);

       // Vector3 movement = transform.TransformDirection(new Vector3(Input.GetAxis("MoveX"), 0.0f, Input.GetAxis("MoveZ"))) * 5.0f;
        Vector3 movement = transform.right * Input.GetAxis("MoveX") + Vector3.Cross(transform.right, up) * Input.GetAxis("MoveZ");

        rigidbody.freezeRotation = true;
        rigidbody.AddForce(movement - rigidbody.velocity * 0.1f, ForceMode.Impulse);
        rigidbody.AddForce(up * -10.0f);
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
                localPoint -= normal * 0.5f;

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

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(targetPlaceIndex.x + 0.5f, targetPlaceIndex.y + 0.5f, targetPlaceIndex.z + 0.5f), new Vector3(1.0f, 1.0f, 1.0f));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(targetIndex.x + 0.5f, targetIndex.y + 0.5f, targetIndex.z + 0.5f), new Vector3(1.0f, 1.0f, 1.0f));

            Gizmos.color = Color.green;
            Vector3 p = new Vector3(targetIndex.x + 0.5f, targetIndex.y + 0.5f, targetIndex.z + 0.5f);
            Gizmos.DrawLine(p, p + targetNormal);
        }
    }
}

