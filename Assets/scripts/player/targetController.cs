using System.Collections.Generic;
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

        inputDriver = transform.parent.GetComponentInChildren<kccIinputDriver>();
        movementDriver = transform.parent.GetComponentInChildren<kccIMovementDriver>();
    }

    private void OnTriggerEnter(Collider other) => addNewTarget(other);
    private void OnTriggerStay(Collider other) => addNewTarget(other);

    private void addNewTarget(Collider other)
    {
        if (other.gameObject.TryGetComponent<target>(out target newTarget) && !newTarget.isLassoTarget)
        {
            if (!targets.Contains(newTarget)) targets.Add(newTarget);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<target>(out target toBeRemovedTarget))
        {
            if (targets.Contains(toBeRemovedTarget)) removeTarget(toBeRemovedTarget);
        }
    }

    public void removeTarget(target toBeRemovedTarget)
    {
        if (toBeRemovedTarget == null) return;

        if (targets.Contains(toBeRemovedTarget))
        {
            targets.Remove(toBeRemovedTarget);
        }

        // Reset references if they match the removed target
        if (toBeRemovedTarget == currentSuggestedTarget) currentSuggestedTarget = null;
        if (toBeRemovedTarget == currentSellectedTarget) currentSellectedTarget = null;
        if (toBeRemovedTarget == lastSuggestedTarget) lastSuggestedTarget = null;
    }

    private void Update()
    {
        if(!gameObject.activeSelf)
            return;

        isTargeting = inputDriver.getTargetInput();

        if (isTargeting && currentSellectedTarget != null)
        {
            Vector2 currentLookDirection = new Vector2(transform.forward.x, transform.forward.z);

            Vector3 worldDir = currentSellectedTarget.transform.position - transform.parent.position;
            Vector2 targetLookDirection = new Vector2(worldDir.x, worldDir.z);

            if (targetLookDirection.sqrMagnitude > 0.001f)
            {
                targetLookDirection.Normalize();
                
                if(currentLookDirection.sqrMagnitude < 0.001f) 
                    currentLookDirection = new Vector2(transform.parent.forward.x, transform.parent.forward.z);
                else
                    currentLookDirection.Normalize();

                Vector2 stabilizedDirection = Vector2.MoveTowards(
                    currentLookDirection, 
                    targetLookDirection, 
                    turnToTargetSpeed * Time.deltaTime
                );

                movementDriver.setLookInput(stabilizedDirection);
            }
        }
    }

    private void LateUpdate()
    {
        if(!gameObject.activeSelf)
            return;

        if (currentSellectedTarget == null) currentSellectedTarget = null;
        if (currentSuggestedTarget == null) currentSuggestedTarget = null;
        if (lastSuggestedTarget == null) lastSuggestedTarget = null;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null) targets.RemoveAt(i);
        }

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

        if (targets.Count <= 0)
        {
            lastTargetsCount = 0;
            wasTargeting = false;
            lastSuggestedTarget = null;
            return;
        }

        if (currentSuggestedTarget == null || (lastTargetsCount <= 0 && targets.Count > 0))
        {
            int index = 0;
            if (targets.Contains(lastSuggestedTarget))
            {
                index = targets.IndexOf(lastSuggestedTarget);
                if (index >= targets.Count - 1) index = 0;
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

        if (isTargeting && !wasTargeting)
        {
            currentSellectedTarget = currentSuggestedTarget;
            currentSuggestedTarget = null;
        }

        if (!isTargeting) currentSellectedTarget = null;

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

        lastTargetsCount = targets.Count;
        wasTargeting = isTargeting;
    }

    void OnDisable()
    {
        if(targetSuggestionGO != null)
            targetSuggestionGO.SetActive(false);
        
        if(targetSellectedGO != null)
            targetSellectedGO.SetActive(false);
    }

    void OnEnable()
    {
        targetSuggestionGO.SetActive(true);
        targetSellectedGO.SetActive(true);
    }
}