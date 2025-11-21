using System.Collections.Generic;
using UnityEngine;

public class LightAuraFollower : MonoBehaviour
{
    [Header("Sphere Settings: ")]
    public string sphereName;
    public int resolution = 30;       
    public float radius = 2f;
    [Space]

    [Header("References: ")]
    public Transform target;          
    public GameObject pointPrefab;
    [Space]

    [Header("Light Reflection Parameters: ")]
    public float laserDistance = 10f;
    public float laserWidth = 0.01f;
    public Material lineMaterial;
    [Space]
    public LayerMask lensLayer;
    public float laserOffset = 0.05f;
    [Space]
    public LayerMask prismLayer;
    [Space]
    public LayerMask burnableLayer;
    [Space]
    public LayerMask mirrorLayer;
    [Space]
    public LayerMask lanternLayer;
    [Space]
    public LayerMask projectorLayer;

    [Header("Debug Prefabs: ")]
    public GameObject obstructionPointMarkerPrefab;
    public GameObject imagePointMarkerPrefab;
    public GameObject endPointMarkerPrefab;
    [Space]

    [Header("Runtime:")]
    [Tooltip("Used To Store 'pointPrefab' In a List.")]
    [SerializeField] private List<GameObject> points = new List<GameObject>();

    void Start()
    {
        //Calculate The Spherical Positions using the parametric equation of a sphere:
        GenerateSpherePoints();
    }

    void Update()
    {
        //Constanlty Update The Sphere Position To Follow The Player:
        if (target != null)
        {
            transform.position = target.position;
        }
    }

    void GenerateSpherePoints()
    {
        if (pointPrefab == null)
        {
            Debug.LogWarning("Point Objects NOT ASSIGNED!!!!");
            return;
        }

        //Empty The List -> Clear Reducing the Index Size:
        foreach (var p in points)
        {
            if (p != null) Destroy(p);
        }
        points.Clear();

        //Create A Container Object To Hold The 'pointPrefab' Objects:
        GameObject auraParent = new GameObject(sphereName);
        auraParent.transform.SetParent(transform, false);

        //Sphere Coordinates Using Theta & Phi:
        for (int i = 0; i <= resolution; i++)
        {
            //THETA:
            float theta = Mathf.PI * i / resolution;

            for (int j = 0; j <= resolution; j++)
            {
                //PHI:
                float phi = 2 * Mathf.PI * j / resolution;

                //Parametric Equation Of Sphere:
                float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = radius * Mathf.Cos(theta);

                Vector3 localPos = new Vector3(x, y, z);

                //Instantiate The Positions Based off Local Position -> 2D Array:
                GameObject point = Instantiate(pointPrefab, auraParent.transform);
                point.transform.localPosition = localPos;
                point.name = $"Point_{i}_{j}";

                //Face Direction Away From Player To Portrude Outwards:
                if (target != null)
                {
                    Vector3 directionFromPlayer = (point.transform.position - target.position).normalized;
                    point.transform.up = directionFromPlayer;
                }

                //Apply the Laser Component To 'pointPrefab' Object:
                LightReflection reflection = point.GetComponent<LightReflection>();
                if (reflection == null)
                    reflection = point.AddComponent<LightReflection>();

                //Assign Parameters To Newly Created Laser:
                reflection.lazerDistance = laserDistance;
                reflection.laserWidth = laserWidth;
                reflection.lineMaterial = lineMaterial;
                reflection.lensLayer = lensLayer;
                reflection.lazerOffset = laserOffset;

                reflection.prismLayer = prismLayer;
                reflection.burnableLayer = burnableLayer;
                reflection.mirrorLayer = mirrorLayer;
                reflection.lanternLayer = lanternLayer;
                reflection.projectorLayer = projectorLayer;

                reflection.obstructionPointMarkerPrefab = obstructionPointMarkerPrefab;
                reflection.imagePointMarkerPrefab = imagePointMarkerPrefab;
                reflection.endPointMarkerPrefab = endPointMarkerPrefab;

                //Store Reference To List:
                points.Add(point);
            }
        }
    }

    public void SetAuraActive(bool active)
    {
        Transform auraParent = transform.Find(sphereName);
        if (auraParent != null)
            auraParent.gameObject.SetActive(active);
    }
}
