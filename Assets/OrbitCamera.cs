using UnityEngine;

[ExecuteInEditMode]
public class OrbitCamera : MonoBehaviour
{
    public PlanetEntity TargetPlanet;
    public float Zoom = 10.0f;
    public Vector2 View;

    void Update()
    {
        if (TargetPlanet == null)
            TargetPlanet = GameObject.FindObjectOfType<PlanetEntity>();

        if (Application.isPlaying)
        {
            Zoom += Input.GetAxis("Mouse ScrollWheel") * 10.0f;
            View += new Vector2(Input.GetAxis("MoveX") * 0.1f, Input.GetAxis("MoveZ") * 0.1f);
            View.y = Mathf.Clamp(View.y, 0.001f, Mathf.PI - 0.001f);
        }

        float zoom = Zoom * Mathf.Sin(View.y);
        transform.position = TargetPlanet.transform.position + new Vector3(Mathf.Sin(View.x) * zoom, Mathf.Cos(View.y) * Zoom, Mathf.Cos(View.x) * zoom); 
        transform.LookAt(TargetPlanet.transform.position, Vector3.up);
    }
}
