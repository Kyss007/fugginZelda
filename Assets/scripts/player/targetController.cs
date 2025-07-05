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
    [Space]
    public bool isTargeting = false;
    public bool wasTargeting = false;

    private int lastTargetsCount = 0;

    private kccIinputDriver inputDriver;

    void Start()
    {
        targetSuggestionGO.transform.SetParent(null);
        targetSuggestionGO.SetActive(false);

        targetSellectedGO.transform.SetParent(null);
        targetSellectedGO.SetActive(false);

        inputDriver = transform.parent.GetComponentInChildren<inputSystemInputDriver>();
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
            }
        }
    }

    private void Update()
    {
        targetSuggestionGO.SetActive(false);
        targetSellectedGO.SetActive(false);

        isTargeting = inputDriver.getTargetInput();

        //early out wenn keine potentiellen targets
        if (targets.Count <= 0)
        {
            lastTargetsCount = 0;
            wasTargeting = false;
            return;
        }


        //wenn kein target suggested wird oder wenn grade der erste target drinne ist soll der erste in der liste suggested werden
        if (currentSuggestedTarget == null || lastTargetsCount <= 0 && targets.Count > 0)
        {   
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != currentSellectedTarget)
                {
                    currentSuggestedTarget = targets[i];
                    break;
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


        lastTargetsCount = targets.Count;
        wasTargeting = isTargeting;
    }
}
