using UnityEngine;

[ExecuteInEditMode]
public class OrbitTarget : MonoBehaviour
{
    public Transform Target;
    public float Distance = 100.0f;
    public float Speed = 1.0f;
    public float StartPos = 0.0f;

    private float currentPos = 0.0f;

    void Start()
    {
        currentPos = StartPos;
    }

    void Update()
    {
        if (Target == null)
            return;

        if (Application.isPlaying)
            currentPos += Speed * Time.deltaTime;
        else
            currentPos = StartPos;

        transform.position = Target.transform.position + new Vector3(Mathf.Sin(currentPos), 0.0f, Mathf.Cos(currentPos)) * Distance;
    }
}

