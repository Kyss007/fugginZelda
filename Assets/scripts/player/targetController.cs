using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class targetController : MonoBehaviour
{
    public GameObject targetSuggestionGO;
    public GameObject targetSellectedGO;
    [Space]

    public LayerMask targetLayer;
    public List<target> targets = new List<target>();
    public target currentSuggestedTarget = null;
    public target currentSellectedTarget = null;
    public float suggestionDisplayOffset = 2.5f;
    public float turnToTargetSpeed = 160f;
    [Space]
    public bool isTargeting = false;
    public bool wasTargeting = false;

    private int lastTargetsCount = 0;

    private target lastSuggestedTarget = null;

    private kccIinputDriver inputDriver;
    private kccIMovementDriver movementDriver;

    private Camera camera;

    void Start()
    {
        camera = Camera.main;

        targetSuggestionGO.transform.SetParent(null);
        targetSuggestionGO.SetActive(false);

        targetSellectedGO.transform.SetParent(null);
        targetSellectedGO.SetActive(false);

        inputDriver = transform.parent.GetComponentInChildren<inputSystemInputDriver>();
        movementDriver = transform.parent.GetComponentInChildren<kccIMovementDriver>();
    }

    private void OnTriggerEnter(Collider other)
    {
        addNewTarget(other);
    }

    private void OnTriggerStay(Collider other)
    {
        addNewTarget(other);
    }

    private void addNewTarget(Collider other)
    {
        target newTarget = null;
        other.gameObject.TryGetComponent<target>(out newTarget);

        if (newTarget != null)
        {
            if (!targets.Contains(newTarget))
            {
                targets.Add(newTarget);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        target toBeRemovedTarget = null;
        other.gameObject.TryGetComponent<target>(out toBeRemovedTarget);

        if (toBeRemovedTarget != null)
        {
            if (targets.Contains(toBeRemovedTarget))
            {
                removeTarget(toBeRemovedTarget);
            }
        }
    }

    public void removeTarget(target toBeRemovedTarget)
    {
        targets.Remove(toBeRemovedTarget);

        if (toBeRemovedTarget == currentSuggestedTarget)
        {
            currentSuggestedTarget = null;
        }

        if (toBeRemovedTarget == currentSellectedTarget)
        {
            currentSellectedTarget = null;
        }

        if (toBeRemovedTarget == lastSuggestedTarget)
        {
            lastSuggestedTarget = null;
        }
    }

    private void Update()
    {
        // Keep input detection in Update so button presses are never missed
        isTargeting = inputDriver.getTargetInput();
    }

    private void LateUpdate()
    {
        // 1. Cleanup Nulls and Dead References
        if (currentSellectedTarget == null) currentSellectedTarget = null;
        if (currentSuggestedTarget == null) currentSuggestedTarget = null;
        if (lastSuggestedTarget == null) lastSuggestedTarget = null;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
            {
                targets.RemoveAt(i);
            }
        }

        // 2. Frustum Check (Remove targets not seen by camera)
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (!GeometryUtility.TestPlanesAABB(planes, targets[i].GetComponent<Collider>().bounds))
            {
                if (targets[i] == currentSuggestedTarget) currentSuggestedTarget = null;
                if (targets[i] == currentSellectedTarget) currentSellectedTarget = null;

                targets.RemoveAt(i);
            }
        } 

        targetSuggestionGO.SetActive(false);
        targetSellectedGO.SetActive(false);

        // 3. Early out if empty
        if (targets.Count <= 0)
        {
            lastTargetsCount = 0;
            wasTargeting = false;
            lastSuggestedTarget = null;
            return;
        }

        // 4. Suggestion Logic
        if (currentSuggestedTarget == null || (lastTargetsCount <= 0 && targets.Count > 0))
        {
            int index = 0;

            if (targets.Contains(lastSuggestedTarget))
            {
                index = targets.IndexOf(lastSuggestedTarget);
                if (index >= targets.Count - 1)
                    index = 0;
            }

            if (targets.Count == 1)
            {
                if (targets[0] != currentSellectedTarget)
                {
                    currentSuggestedTarget = targets[0];
                    lastSuggestedTarget = currentSuggestedTarget;
                }
            }
            else
            {
                for (int i = index; i < targets.Count; i++)
                {
                    if (targets[i] != currentSellectedTarget && targets[i] != lastSuggestedTarget)
                    {
                        currentSuggestedTarget = targets[i];
                        lastSuggestedTarget = currentSuggestedTarget;
                        break;
                    }
                }
            }
        }

        // 5. Select Target
        if (isTargeting && !wasTargeting)
        {
            currentSellectedTarget = currentSuggestedTarget;
            currentSuggestedTarget = null;
        }

        if (!isTargeting)
            currentSellectedTarget = null;

        // 6. Visual Updates
        if (currentSuggestedTarget != null)
        {
            targetSuggestionGO.SetActive(true);
            targetSuggestionGO.transform.position = currentSuggestedTarget.transform.position + new Vector3(0, suggestionDisplayOffset, 0);
        }

        if (currentSellectedTarget != null)
        {
            targetSellectedGO.SetActive(true);
            targetSellectedGO.transform.position = currentSellectedTarget.transform.position;
        }

        // 7. Rotation Logic (Use Time.deltaTime for LateUpdate)
        if (isTargeting && currentSellectedTarget != null)
        {
            Vector2 currentLookDirection = new Vector2(transform.forward.x, transform.forward.z).normalized;

            Vector3 dir = currentSellectedTarget.transform.position - transform.parent.position;
            Vector2 targetLookDirection = new Vector2(dir.x, dir.z).normalized;

            Vector2 lerpedLookDirection = Vector2.Lerp(currentLookDirection, targetLookDirection, turnToTargetSpeed * Time.deltaTime);
            movementDriver.setLookInput(lerpedLookDirection);
        }

        lastTargetsCount = targets.Count;
        wasTargeting = isTargeting;
    }
}