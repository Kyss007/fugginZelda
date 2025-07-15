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

    void Start()
    {
        targetSuggestionGO.transform.SetParent(null);
        targetSuggestionGO.SetActive(false);

        targetSellectedGO.transform.SetParent(null);
        targetSellectedGO.SetActive(false);

        inputDriver = transform.parent.GetComponentInChildren<inputSystemInputDriver>();
        movementDriver = transform.parent.GetComponentInChildren<kccIMovementDriver>();
    }

    private void OnTriggerEnter(Collider other)
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
                targets.Remove(toBeRemovedTarget);

                //remove from suggested target
                if (toBeRemovedTarget == currentSuggestedTarget)
                {
                    currentSuggestedTarget = null;
                }

                //remove from sellected target
                if (toBeRemovedTarget == currentSellectedTarget)
                {
                    currentSellectedTarget = null;
                }
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null)
            {
                targets.Remove(targets[i]);
            }
        } 


        targetSuggestionGO.SetActive(false);
        targetSellectedGO.SetActive(false);

        isTargeting = inputDriver.getTargetInput();

        //early out wenn keine potentiellen targets
        if (targets.Count <= 0)
        {
            lastTargetsCount = 0;
            wasTargeting = false;

            lastSuggestedTarget = null;
            return;
        }


        //wenn kein target suggested wird oder wenn grade der erste target drinne ist soll der erste in der liste suggested werden
        if (currentSuggestedTarget == null || (lastTargetsCount <= 0 && targets.Count > 0))
        {
            //TODO: statt einfach den nächsten in der liste picken, den nächsten zum letzten target
            int index = 0;

            if (targets.Contains(lastSuggestedTarget))
            {
                index = targets.IndexOf(lastSuggestedTarget);

                if (index >= targets.Count - 1)
                    index = 0;
            }

            // falls nur ein target vorhanden ist nimm es trotzdem
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


        //target sellecten
        if (isTargeting && !wasTargeting)
        {
            currentSellectedTarget = currentSuggestedTarget;
            currentSuggestedTarget = null;
        }

        if (!isTargeting)
            currentSellectedTarget = null;

        //target suggestion dingen positionieren und an machen
        if (currentSuggestedTarget != null)
        {
            targetSuggestionGO.SetActive(true);

            targetSuggestionGO.transform.position = currentSuggestedTarget.transform.position + new Vector3(0, suggestionDisplayOffset, 0);
        }

        //sellected target dinge positionieren und an machen
        if (currentSellectedTarget != null)
        {
            targetSellectedGO.SetActive(true);

            targetSellectedGO.transform.position = currentSellectedTarget.transform.position;
        }

        if (isTargeting && currentSellectedTarget != null)
        {
            Vector2 currentLookDirection = new Vector2(transform.forward.x, transform.forward.z).normalized;

            Vector3 dir = currentSellectedTarget.transform.position - transform.parent.position;
            Vector2 targetLookDirection = new Vector2(dir.x, dir.z).normalized;

            Vector2 lerpedLookDirection = Vector2.Lerp(currentLookDirection, targetLookDirection, turnToTargetSpeed * Time.fixedDeltaTime);
            movementDriver.setLookInput(lerpedLookDirection);
        }


        lastTargetsCount = targets.Count;
        wasTargeting = isTargeting;
    }
}
