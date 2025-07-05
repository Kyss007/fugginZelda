using System.Collections.Generic;
using UnityEngine;

public class targetController : MonoBehaviour
{
    public GameObject targetSuggestionGO;
    [Space]

    public LayerMask targetLayer;
    public List<target> targets = new List<target>();
    public target currentSuggestedTarget = null;
    public float suggestionDisplayOffset = 2.5f;

    private int lastTargetsCount = 0;

    void Start()
    {
        targetSuggestionGO.transform.SetParent(null);
        targetSuggestionGO.SetActive(false);
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

        if (targets.Count <= 0)
            return;

        if (currentSuggestedTarget != null)
        {
            targetSuggestionGO.SetActive(true);

            targetSuggestionGO.transform.position = currentSuggestedTarget.transform.position + new Vector3(0, suggestionDisplayOffset, 0);
        }

        if (currentSuggestedTarget == null || lastTargetsCount <= 0 && targets.Count > 0)
        {
            currentSuggestedTarget = targets[0];
        }

        lastTargetsCount = targets.Count;
    }
}
