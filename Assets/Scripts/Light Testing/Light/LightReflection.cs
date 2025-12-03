using NUnit.Framework;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;



[RequireComponent(typeof(LineRenderer))]
public class LightReflection : MonoBehaviour
{
    [Header("Laser Parameters: ")]
    public List<Vector3> laserPoints;
    public RaycastHit[] hits;
    public float lazerDistance;
    private LineRenderer lineRenderer;
    public Material lineMaterial;
    private List<GameObject> laserPointMarkers = new List<GameObject>();
    [Space]

    [Header("Lens Collision: ")]
    public LayerMask lensLayer;
    public bool lensHit;
    [Space]
    [Tooltip("After Colliding, Offsets The Point By X Distance.")]
    public float lazerOffset;
    private Lens lens;
    Vector3 ImagePoint = Vector3.zero;
    private List<Vector3> imagePoints = new List<Vector3>();
    [Space]

    [Header("Prism Collision: ")]
    public LayerMask prismLayer;
    public bool prismHit;
    private Prism prism;
    private List<GameObject> prismSplitBeams = new List<GameObject>();
    private List<GameObject> splitRayMarkers = new List<GameObject>();
    [Space]

    [Header("Burnable Collision: ")]
    public LayerMask burnableLayer;
    public bool burnableHit;
    private Burnable burnable;
    [Space]

    [Header("Mirror Collision: ")]
    public LayerMask mirrorLayer;
    public bool mirrorHit;
    private Mirror mirror;
    [Space]


    [Header("Lantern Collision")]
    public LayerMask lanternLayer;
    public bool lanternHit;
    private Lantern lantern;
    public Lantern currentLanternHit;
    [Space]

    [Header("Projector Collision")]
    public LayerMask projectorLayer;
    public bool projectorHit;
    private Projector projector;
    public Transform parentObjectForRotation;  // Set this from Projector when updating
    public Quaternion lightRotationOffset = Quaternion.identity;
    [Space]

    [Header("Debug Visualization")]
    public GameObject obstructionPointMarkerPrefab;
    public GameObject imagePointMarkerPrefab;
    public GameObject endPointMarkerPrefab;
    private List<Vector3> obstructionPoints = new List<Vector3>();
    [Space]



    public float laserWidth;
    private float additionalDistanceUsed;

    private void OnDisable()
    {
        ClearMarkers();
        ClearPrismSplits();
        ClearSplitRayMarkers();
    }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;

        if (laserPoints == null) laserPoints = new List<Vector3>();
    }


    private void Update()
    {
        ClearMarkers();
        ClearPrismSplits();
        ClearSplitRayMarkers();

        //Laser Setup:
        Vector3 ObjectPosition = transform.position;
        Vector3 ObjectDirection = transform.up;
        float remainingLazerDistance = lazerDistance;

        laserPoints.Add(ObjectPosition);
        List<Collider> lensesHit = new List<Collider>();

        Vector3? previousImage = null;

        //Setting a Distance To Avoid Infinite Looping:
        while (remainingLazerDistance > 0f)
        {
            //Ray Setup:
            Ray ray = new Ray(ObjectPosition, ObjectDirection);
            hits = Physics.RaycastAll(ray, remainingLazerDistance, lensLayer | prismLayer | burnableLayer | mirrorLayer | lanternLayer | projectorLayer, QueryTriggerInteraction.Ignore);

            //No Lens Collision + End of Ray:
            if (!ClosestValidHit(hits, lensesHit, out RaycastHit hit))
            {
                Vector3 endPoint = ObjectPosition + ObjectDirection * remainingLazerDistance;
                laserPoints.Add(endPoint);

                if (endPointMarkerPrefab != null)
                {
                    GameObject endMarker = Instantiate(endPointMarkerPrefab, endPoint, Quaternion.identity);
                    laserPointMarkers.Add(endMarker);
                }
                break;
            }

            //Object Refrences:
            lens = hit.collider.GetComponent<Lens>() ?? hit.collider.GetComponentInParent<Lens>();
            prism = hit.collider.GetComponent<Prism>() ?? hit.collider.GetComponentInParent<Prism>();
            burnable = hit.collider.GetComponent<Burnable>() ?? hit.collider.GetComponentInParent<Burnable>();
            mirror = hit.collider.GetComponent<Mirror>() ?? hit.collider.GetComponentInParent<Mirror>();
            lantern = hit.collider.GetComponent<Lantern>() ?? hit.collider.GetComponentInParent<Lantern>();
            projector = hit.collider.GetComponent<Projector>() ?? hit.collider.GetComponentInParent<Projector>();

            //Null Object Checks:
            if (lens == null && prism == null && burnable == null && mirror == null && lantern == null && projector == null)
            {
                laserPoints.Add(ObjectPosition + ObjectDirection * remainingLazerDistance);
                break;
            }

            //Lens Collison:
            if (lens != null)
            {
                lensHit = true;
                HandleLensHit(hit, lensesHit, ref ObjectPosition, ref ObjectDirection, ref remainingLazerDistance, ref previousImage);
                continue;
            }

            //Prism Collision:
            if (prism != null)
            {
                prismHit = true;
                HandlePrismHit(hit, prism, ObjectDirection, remainingLazerDistance - hit.distance);
                break;
            }

            //Burnable Collision:
            if (burnable != null)
            {
                burnableHit = true;
                HandleBurnableHit(hit);
                break;
            }

            //Mirror Collision:
            if (mirror != null)
            {
                mirrorHit = true;
                HandleMirrorHit(hit, mirror, ref ObjectPosition, ref ObjectDirection, ref remainingLazerDistance);
                continue;
            }

            //Lantern Collision:
            if (lantern != null)
            {
                lanternHit = true;
                HandleLanternHit(hit);
                break;
            }

            //Projector Collision:
            if (projector != null)
            {
                projectorHit = true;
                HandleProjectorHit(hit);
                break;
            }
        }

        //Function Used to Display Hit Points & Image Points:
        Visualize();
    }

    private void HandleLensHit(RaycastHit hit, List<Collider> lensesHit, ref Vector3 ObjectPosition, ref Vector3 ObjectDirection, ref float remainingLazerDistance, ref Vector3? previousImage)
    {
        lensesHit.Add(hit.collider);
        laserPoints.Add(hit.point);
        obstructionPoints.Add(hit.point);

        Vector3 objectPosForThisLens = previousImage ?? ObjectPosition;

        //When Calculating Image Location:
        if (CalculateImagePoint(objectPosForThisLens, hit.point, lens, out Vector3 calculatedImagePoint))
        {
            //Save Image Point to Use As Object Position For Obstruction:
            previousImage = calculatedImagePoint;

            //Setup Distance Line between hit Location and Image Location:
            Vector3 toImage = calculatedImagePoint - hit.point;
            float toImageDistance = toImage.magnitude;
            Vector3 toImageDir = toImage.normalized;

            //Check For Any Additional Lens Positions Between Hit and Image Positions:
            if (HandleObstructionRecursive(hit.point, toImageDir, toImageDistance, lensesHit, out Vector3 finalImagePoint, out Vector3 nextPosition, out Vector3 nextDirection, out float distanceUsed, previousImage, additionalDistanceUsed))
            {
                ImagePoint = finalImagePoint;

                ObjectPosition = nextPosition;
                ObjectDirection = nextDirection;

                remainingLazerDistance -= distanceUsed;

                imagePoints.Add(finalImagePoint);
                laserPoints.Add(finalImagePoint);
            }
            else
            {
                //No obstruction, Single Lens:
                ImagePoint = calculatedImagePoint;
                imagePoints.Add(ImagePoint);
                laserPoints.Add(ImagePoint);

                ObjectDirection = (ImagePoint - hit.point).normalized;
                ObjectPosition = ImagePoint + ObjectDirection * lazerOffset;

                remainingLazerDistance -= toImageDistance;
                remainingLazerDistance += additionalDistanceUsed;
            }
        }
    }

    private void HandlePrismHit(RaycastHit hit, Prism prism, Vector3 incomingDir, float remainingDistance)
    {
        //Null Check:
        if (prism == null || prism.amountOfSplits <= 0 || prism.range <= 0f || prism.range > 2f * Mathf.PI) return;

        //Contact Point:
        laserPoints.Add(hit.point);
        obstructionPoints.Add(hit.point);

        //Calculate Image Point, To Be Used as Center Point For Prism Split:
        Vector3 centerDir = incomingDir.normalized;
        Vector3 centerImagePoint = hit.point + centerDir * remainingDistance;

        //Calculate Angle Each Ray Is Seperated By:
        float angleStep = prism.range / prism.amountOfSplits;
        float halfRange = prism.range / 2f;

        //For Each Ray:
        for (int i = 0; i < prism.amountOfSplits; i++)
        {
            //Apply the Relative Angle in XY Axis:
            float relativeAngle = -halfRange + i * angleStep;
            Vector3 splitDir = Quaternion.AngleAxis(Mathf.Rad2Deg * relativeAngle, Vector3.forward) * centerDir;

            //Create Empty GameObject, Child of Initial Ray:
            GameObject splitObj = new GameObject($"Split Ray - {i}");
            splitObj.transform.parent = transform;
            splitObj.transform.position = hit.point;
            splitObj.transform.rotation = Quaternion.LookRotation(Vector3.forward, splitDir);

            //Line Render Setup, For Visualization:
            LineRenderer lr = splitObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial != null ? lineMaterial : lineRenderer.material;
            lr.startWidth = laserWidth * 0.8f;
            lr.endWidth = laserWidth * 0.8f;

            //New list, Holds Information of Singular Ray:
            List<Vector3> splitPoints = TraceSplitRay(hit.point, splitDir, remainingDistance);
            lr.positionCount = splitPoints.Count;
            lr.SetPositions(splitPoints.ToArray());

            //Add Ray to List that Holds All Other Split Rays:
            prismSplitBeams.Add(splitObj);
        }
    }

    private List<Vector3> TraceSplitRay(Vector3 origin, Vector3 dir, float maxDistance)
    {
        //[Origin == prism collision point]:
        //[Direction == angle calculated based off angleStep]:
        //[Remaining == current ray distance upon collision]:
        List<Vector3> points = new List<Vector3> { origin };
        Vector3 currentPos = origin;
        Vector3 currentDir = dir;
        float remaining = maxDistance;

        //List holding All Lenses that Collided with current ray (avoids collision issues):
        List<Collider> hitLenses = new List<Collider>();
        Vector3? previousImage = null;

        //While there is still Distance on the Ray:
        while (remaining > 0f)
        {
            //Ray Setup:
            Ray ray = new Ray(currentPos, currentDir);
            RaycastHit[] hits = Physics.RaycastAll(ray, remaining, lensLayer | prismLayer | burnableLayer | mirrorLayer | lanternLayer | projectorLayer);

            //No Collision + End of Ray:
            if (!ClosestValidHit(hits, hitLenses, out RaycastHit hit))
            {
                Vector3 endPoint = currentPos + currentDir * remaining;
                points.Add(endPoint);

                //Another Object (Point) at End of Ray (For Visualization):
                if (obstructionPointMarkerPrefab != null)
                {
                    GameObject dataPoint = Instantiate(endPointMarkerPrefab, endPoint, Quaternion.identity);
                    splitRayMarkers.Add(dataPoint);
                }
                break;
            }

            //Object Refrences:
            Lens hitLens = hit.collider.GetComponent<Lens>() ?? hit.collider.GetComponentInParent<Lens>();
            Prism hitPrism = hit.collider.GetComponent<Prism>() ?? hit.collider.GetComponentInParent<Prism>();
            Burnable hitBurnable = hit.collider.GetComponent<Burnable>() ?? hit.collider.GetComponentInParent<Burnable>();
            Mirror hitMirror = hit.collider.GetComponent<Mirror>() ?? hit.collider.GetComponentInParent<Mirror>();
            Lantern hitLantern = hit.collider.GetComponent<Lantern>() ?? hit.collider.GetComponentInParent<Lantern>();
            Projector hitProjector = hit.collider.GetComponent<Projector>() ?? hit.collider.GetComponentInParent<Projector>();

            //Lens Collision:
            if (hitLens != null)
            {
                if (CalculateImagePoint(previousImage ?? currentPos, hit.point, hitLens, out Vector3 imagePoint))
                {
                    points.Add(hit.point);
                    points.Add(imagePoint);

                    //Collision Points:
                    if (obstructionPointMarkerPrefab != null)
                    {
                        GameObject dataPoint = Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity);
                        splitRayMarkers.Add(dataPoint);
                    }

                    //Image Points:
                    if (imagePointMarkerPrefab != null)
                    {
                        GameObject dataPoint = Instantiate(imagePointMarkerPrefab, imagePoint, Quaternion.identity);
                        splitRayMarkers.Add(dataPoint);
                    }

                    currentPos = imagePoint + (imagePoint - hit.point).normalized * lazerOffset;
                    currentDir = (imagePoint - hit.point).normalized;
                    remaining -= Vector3.Distance(hit.point, imagePoint);
                    previousImage = imagePoint;
                    hitLenses.Add(hit.collider);
                    continue;
                }
            }

            //Prism collision:
            if (hitPrism != null)
            {
                points.Add(hit.point);

                if (obstructionPointMarkerPrefab != null) splitRayMarkers.Add(Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity));
                HandlePrismHit(hit, hitPrism, currentDir, remaining);

                break;
            }

            //Burnable collision:
            if (hitBurnable != null)
            {
                points.Add(hit.point);

                if (obstructionPointMarkerPrefab != null) splitRayMarkers.Add(Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity));
                HandleBurnableHit(hit);

                break;
            }

            //Mirror collision:
            if (hitMirror != null)
            {
                points.Add(hit.point);

                if (obstructionPointMarkerPrefab != null) splitRayMarkers.Add(Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity));
                HandleMirrorHit(hit, hitMirror, ref currentPos, ref currentDir, ref remaining);

                continue;
            }

            //Lantern Collision:
            if (hitLantern != null)
            {
                points.Add(hit.point);
                if (obstructionPointMarkerPrefab != null) splitRayMarkers.Add(Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity));
                HandleLanternHit(hit);

                break;
            }

            //Projector Collision:
            if (hitProjector != null)
            {
                points.Add(hit.point);
                if (obstructionPointMarkerPrefab != null) splitRayMarkers.Add(Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity));
                HandleProjectorHit(hit);

                break;
            }

            //Collision with non-lens Surface:
            points.Add(hit.point);
            if (obstructionPointMarkerPrefab != null)
            {
                GameObject dataPoint = Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity);
                splitRayMarkers.Add(dataPoint);
            }
            break;
        }
        return points;
    }

    private void HandleBurnableHit(RaycastHit hit)
    {
        burnable = hit.collider.GetComponent<Burnable>();
        if (burnable != null)
        {
            burnableHit = true;
            burnable.hitsThisFrame++;

            laserPoints.Add(hit.point);
            obstructionPoints.Add(hit.point);
        }
        else
        {
            burnableHit = false;
        }
    }

    private void HandleMirrorHit(RaycastHit hit, Mirror mirror, ref Vector3 ObjectPosition, ref Vector3 ObjectDirection, ref float remainingLazerDistance)
    {
        //Record hit:
        laserPoints.Add(hit.point);
        obstructionPoints.Add(hit.point);

        //Incoming Direction & Surface Normal:
        Vector3 incomingDir = ObjectDirection.normalized;
        Vector3 hitNormal = hit.normal.normalized;

        Vector3 newDir;

        //Default Reflection Direction:
        if (mirror.useSurfaceNormal)
        {
            newDir = Vector3.Reflect(incomingDir, hitNormal);
        }
        //Custon Reflection Direction:
        else
        {
            Vector3 rotationAxis = Vector3.Cross(incomingDir, hitNormal);
            if (rotationAxis == Vector3.zero) rotationAxis = Vector3.up;

            //Rotate The Ray By Parameter Angle:
            newDir = Quaternion.AngleAxis(mirror.reflectionAngle, rotationAxis) * Vector3.Reflect(incomingDir, hitNormal);
        }

        newDir.Normalize();

        //Update Direction & Position:
        ObjectDirection = newDir;
        ObjectPosition = hit.point + ObjectDirection * lazerOffset;

        remainingLazerDistance -= hit.distance;

        //Endpoint:
        if (obstructionPointMarkerPrefab != null)
        {
            GameObject mirrorMarker = Instantiate(obstructionPointMarkerPrefab, hit.point, Quaternion.identity);
            laserPointMarkers.Add(mirrorMarker);
        }
    }

    private void HandleLanternHit(RaycastHit hit)
    {
        lantern = hit.collider.GetComponent<Lantern>();
        if (lantern != null)
        {
            lanternHit = true;
            currentLanternHit = lantern;

            //Increment The Lantern Activation Time:
            lantern.hitsThisFrame++;

            laserPoints.Add(hit.point);
            obstructionPoints.Add(hit.point);

            //If Enough Increments & Bool Becomes True:
            if (lantern.activeLantern && LanternTravel.Instance != null)
            {
                //If The Hit Lantern IS NOT In The List:
                if (!LanternTravel.Instance.ActivatedLanterns.Contains(lantern))
                {
                    LanternTravel.Instance.ActivatedLanterns.Add(lantern);
                }
            }
        }
        else
        {
            lanternHit = false;
        }
    }

    private void HandleProjectorHit(RaycastHit hit)
    {
        projector = hit.collider.GetComponent<Projector>();
        if (projector != null)
        {
            projectorHit = true;

            projector.RegisterHit();

            //Set Beam Light Rotation Parent & Offset:
            if (projector.beamLight != null)
            {
                projector.beamLight.parentObjectForRotation = projector.ParentObject;
                projector.beamLight.lightRotationOffset = projector.lightRotationOffset;
            }

            //Update Beam Light Reflection:
            UpdateProjectorLightReflection(projector);

            laserPoints.Add(hit.point);
            obstructionPoints.Add(hit.point);
        }
        else
        {
            projectorHit = false;
        }
    }

    private void UpdateProjectorLightReflection(Projector projector)
    {
        if (projector == null || projector.beamLight == null || projector.ParentObject == null) return;

        //Set Beam Light Position -> Beam Root Position:
        projector.beamLight.transform.position = projector.beamRoot.position;

        //Set Beam Light Rotation -> Parent Object Rotation + Offset:
        projector.beamLight.transform.rotation = projector.ParentObject.rotation * projector.lightRotationOffset;

        //Calculate New Distance Based on Hits This Frame:
        float newDistance = Mathf.Max(projector.hitsThisFrame * projector.lengthPerHit, 0.001f);

        //Update Beam Light Parameters:
        projector.beamLight.lazerDistance = newDistance;
        projector.beamLight.laserWidth = projector.beamWidth;

        //Update Beam Light Visual:
        projector.beamLight.UpdateLaserVisual();
    }

    public void UpdateLaserVisual()
    {
        //Ensure Line Renderer is Assigned:
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.LogWarning("LineRenderer component missing!");
                return;
            }
        }

        //Set Line Renderer Width:
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        laserPoints.Clear();

        //Calculate Final Rotation:
        Quaternion finalRotation = parentObjectForRotation.rotation * lightRotationOffset;

        //Calculate Output Direction (+Y in Local Space):
        Vector3 outputDirection = finalRotation * Vector3.up;

        //Calculate Axis (-Z in Local Space):
        Vector3 axis = finalRotation * -Vector3.forward;

        //Calculate Beam Direction (Perpendicular to Axis and Output Direction):
        Vector3 beamDirection = Vector3.Cross(axis, outputDirection).normalized;

        //Calculate Origin Point:
        Vector3 origin = transform.position;

        //Calculate Laser Points:
        laserPoints.Add(origin);
        laserPoints.Add(origin + beamDirection * lazerDistance);

        //Update Line Renderer Positions:
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());
    }

    private void ClearMarkers()
    {
        foreach (var marker in laserPointMarkers)
            if (marker != null) Destroy(marker);

        laserPointMarkers.Clear();
        obstructionPoints.Clear();
        imagePoints.Clear();
        laserPoints.Clear();

        lensHit = false;
        prismHit = false;
        burnableHit = false;
        mirrorHit = false;
        lanternHit = false;
        projectorHit = false;
    }

    private void ClearPrismSplits()
    {
        foreach (var beam in prismSplitBeams)
            if (beam != null) Destroy(beam);

        prismSplitBeams.Clear();

        prismHit = false;
    }

    private void ClearSplitRayMarkers()
    {
        foreach (var marker in splitRayMarkers)
            if (marker != null) Destroy(marker);

        splitRayMarkers.Clear();
    }

    private bool ClosestValidHit(RaycastHit[] hitArray, List<Collider> lensesHit, out RaycastHit closestHit)
    {
        //Default Parameters:
        closestHit = default;
        float closestDist = Mathf.Infinity;
        bool found = false;

        //Ensure Raycast Hits can only hit the Same Target Once, But Can Hit Previous Lenses:
        foreach (var hit in hitArray)
        {
            if (lensesHit.Count > 0 && hit.collider == lensesHit[lensesHit.Count - 1]) continue;

            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                closestHit = hit;
                found = true;
            }
        }
        return found;
    }

    private bool CalculateImagePoint(Vector3 objectPos, Vector3 hitPoint, Lens lens, out Vector3 imagePoint)
    {
        //Default Image Point:
        imagePoint = Vector3.zero;
        additionalDistanceUsed = 0f;
        if (lens == null) return false;

        //Focal Length:     [Convex = Positive]     [Concave = Negative]
        float f = lens.isConvex ? Mathf.Abs(lens.focalLength) : -Mathf.Abs(lens.focalLength);

        //Object Distance:
        float p = Vector3.Distance(objectPos, hitPoint);

        //Case to avoid 0:
        if (Mathf.Abs(p) < 0.001f || Mathf.Abs(f - p) < 0.001f) return false;

        //Image Distance:
        float i = 1f / ((1f / f) - (1f / p));

        //Intial Object Height [Based on Hit Point]
        float initialHeight = objectPos.y - hitPoint.y;

        //Magnification:
        float magnification = i / p;

        //Image Height:
        float finalHeight = magnification * initialHeight;

        //Final Image Position:
        Vector3 imageDirection = (i >= 0) ? (hitPoint - objectPos).normalized : -(hitPoint - objectPos).normalized;
        Vector3 baseImagePoint = hitPoint + imageDirection * Mathf.Abs(i);

        Vector3 adjustedImagePoint = new Vector3(baseImagePoint.x, hitPoint.y + finalHeight, baseImagePoint.z);

        //Increases Laser Ray By Given Additional Distance:
        if (lens.additionalDistance != 0f)
        {
            adjustedImagePoint += imageDirection * lens.additionalDistance;
            additionalDistanceUsed = lens.additionalDistance;
        }

        imagePoint = adjustedImagePoint;
        return true;
    }


    private bool HandleObstructionRecursive(Vector3 currentHitPoint, Vector3 toImageDir, float toImageDistance, List<Collider> lensesHit, out Vector3 finalImagePoint, out Vector3 nextPosition, out Vector3 nextDirection, out float totalDistanceUsed, Vector3? incomingObjectPoint, float extraDistanceUsed = 0f)
    {
        //Default Parameters for outputs:
        finalImagePoint = currentHitPoint;
        nextPosition = currentHitPoint;
        nextDirection = toImageDir;
        totalDistanceUsed = 0f;


        //Ray Setup:
        Ray obstructionRay = new Ray(currentHitPoint, toImageDir);
        float rayDistance = toImageDistance + extraDistanceUsed;
        RaycastHit[] obstructionHits = Physics.RaycastAll(obstructionRay, rayDistance, lensLayer | prismLayer | burnableLayer | mirrorLayer | lanternLayer | projectorLayer);


        //No Lens Collision: [Return False Since This Function is Used to Check Multiple Lens Collisions]
        if (!ClosestValidHit(obstructionHits, lensesHit, out RaycastHit obstructionHit)) return false;


        var nextLens = obstructionHit.collider.GetComponent<Lens>() ?? obstructionHit.collider.GetComponentInParent<Lens>();
        var hitMirror = obstructionHit.collider.GetComponent<Mirror>() ?? obstructionHit.collider.GetComponentInParent<Mirror>();
        var hitPrism = obstructionHit.collider.GetComponent<Prism>() ?? obstructionHit.collider.GetComponentInParent<Prism>();
        var hitBurnable = obstructionHit.collider.GetComponent<Burnable>() ?? obstructionHit.collider.GetComponentInParent<Burnable>();
        var hitLantern = obstructionHit.collider.GetComponent<Lantern>() ?? obstructionHit.collider.GetComponentInParent<Lantern>();
        var hitProjector = obstructionHit.collider.GetComponent<Projector>() ?? obstructionHit.collider.GetComponentInParent<Projector>();

        //Lens Collison:
        if (nextLens != null)
        {
            //Add Lens to a Hit Collection:
            lensesHit.Add(obstructionHit.collider);

            //Add Lens Posistion For Markers:
            obstructionPoints.Add(obstructionHit.point);
            laserPoints.Add(obstructionHit.point);

            Vector3 objectPos = incomingObjectPoint ?? currentHitPoint;

            //When Calculating Image Location:
            if (CalculateImagePoint(objectPos, obstructionHit.point, nextLens, out Vector3 newImagePoint))
            {
                imagePoints.Add(newImagePoint);

                //Set the laser path:
                Vector3 nextDir = (newImagePoint - obstructionHit.point).normalized;
                Vector3 nextPos = newImagePoint + nextDir * lazerOffset;

                //Set the laser remaining Distance:
                float nextDist = Vector3.Distance(obstructionHit.point, newImagePoint);

                //Recursively check for further obstructions down the new path of "newImagePoint":
                if (HandleObstructionRecursive(obstructionHit.point, nextDir, nextDist, lensesHit, out Vector3 deeperImage, out Vector3 deeperPos, out Vector3 deeperDir, out float deeperUsed, newImagePoint, extraDistanceUsed + additionalDistanceUsed))
                {
                    finalImagePoint = deeperImage;
                    nextPosition = deeperPos;
                    nextDirection = deeperDir;

                    totalDistanceUsed = Vector3.Distance(currentHitPoint, obstructionHit.point) + deeperUsed + extraDistanceUsed;

                    return true;
                }
                else
                {
                    //No Recursive Checks, 1 Obstruction:
                    finalImagePoint = newImagePoint;
                    nextPosition = nextPos;
                    nextDirection = nextDir;

                    totalDistanceUsed = Vector3.Distance(currentHitPoint, newImagePoint) + extraDistanceUsed;

                    return true;
                }
            }
        }
        //Mirror Collision:
        else if (hitMirror != null)
        {
            mirrorHit = true;

            //Mark hit:
            obstructionPoints.Add(obstructionHit.point);
            laserPoints.Add(obstructionHit.point);

            HandleMirrorHit(obstructionHit, hitMirror, ref currentHitPoint, ref toImageDir, ref toImageDistance);

            //Update outputs:
            finalImagePoint = currentHitPoint;
            nextPosition = currentHitPoint;
            nextDirection = toImageDir;
            totalDistanceUsed = lazerDistance - toImageDistance; // distance used so far

            return true;
        }
        //Prism Collision:
        else if (hitPrism != null)
        {
            prismHit = true;

            //Mark hit:
            obstructionPoints.Add(obstructionHit.point);
            laserPoints.Add(obstructionHit.point);

            HandlePrismHit(obstructionHit, hitPrism, toImageDir, toImageDistance);

            //Update outputs:
            totalDistanceUsed = Vector3.Distance(currentHitPoint, obstructionHit.point) + extraDistanceUsed;
            finalImagePoint = obstructionHit.point;
            nextPosition = obstructionHit.point;
            nextDirection = toImageDir;

            return true;
        }
        //Burnable Collision:
        else if (hitBurnable != null)
        {
            burnableHit = true;

            //Mark hit:
            obstructionPoints.Add(obstructionHit.point);
            laserPoints.Add(obstructionHit.point);


            HandleBurnableHit(obstructionHit);

            //Update outputs:
            totalDistanceUsed = Vector3.Distance(currentHitPoint, obstructionHit.point) + extraDistanceUsed;
            finalImagePoint = obstructionHit.point;
            nextPosition = obstructionHit.point;
            nextDirection = toImageDir;

            return true;
        }
        //Lantern Collision:
        else if (hitLantern != null)
        {
            lanternHit = true;

            //Mark hit:
            obstructionPoints.Add(obstructionHit.point);
            laserPoints.Add(obstructionHit.point);

            HandleLanternHit(obstructionHit);

            //Update outputs:
            totalDistanceUsed = Vector3.Distance(currentHitPoint, obstructionHit.point) + extraDistanceUsed;
            finalImagePoint = obstructionHit.point;
            nextPosition = obstructionHit.point;
            nextDirection = toImageDir;

            return true;
        }
        //Projector Collision:
        else if (hitProjector != null)
        {
            projectorHit = true;

            //Mark hit:
            obstructionPoints.Add(obstructionHit.point);
            laserPoints.Add(obstructionHit.point);

            HandleProjectorHit(obstructionHit);

            //Update outputs:
            totalDistanceUsed = Vector3.Distance(currentHitPoint, obstructionHit.point) + extraDistanceUsed;
            finalImagePoint = obstructionHit.point;
            nextPosition = obstructionHit.point;
            nextDirection = toImageDir;

            return true;
        }

        return false;
    }


    private void Visualize()
    {
        //Visualization of Line Render For Light Source:
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());


        //Marker Visualization For Obstruction Points:
        if (obstructionPointMarkerPrefab != null)
        {
            foreach (var point in obstructionPoints)
            {
                GameObject marker = Instantiate(obstructionPointMarkerPrefab, point, Quaternion.identity);
                laserPointMarkers.Add(marker);
            }
        }


        //Marker Visualization For Image Locations:
        if (imagePointMarkerPrefab != null)
        {
            foreach (var point in imagePoints)
            {
                GameObject marker = Instantiate(imagePointMarkerPrefab, point, Quaternion.identity);
                laserPointMarkers.Add(marker);
            }
        }
    }
}
