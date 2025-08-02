using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogOptionText : MonoBehaviour
{
    public GameObject dialogOptionTextPrefab;

    public List<dialogOptionTextHelper> helpers;

    public int optionWasSelected = -1;

    private int currentlySelected = 0;
    private int lastSelected = -1;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 50 * helpers.Count);

        if(currentlySelected != lastSelected)
        {
            for(int i = 0; i < helpers.Count; i++)
            {
                helpers[i].selectIndicator.SetActive(false);

                if(i == currentlySelected)
                {
                    helpers[i].selectIndicator.SetActive(true);
                }
            }
        }

        lastSelected = currentlySelected;
    }

    public int optionSelected()
    {
        return optionWasSelected;
    }

    public void addOption(string optionString)
    {
        dialogOptionTextHelper newHelper = Instantiate(dialogOptionTextPrefab, transform).GetComponent<dialogOptionTextHelper>();
        
        newHelper.optionText.text = optionString;

        helpers.Add(newHelper);
    }

    public void setSelectedFinal(dialogOptionTextHelper helper)
    {
        if(helpers.Contains(helper))
            optionWasSelected = helpers.IndexOf(helper);
    }

    public void setSelected(dialogOptionTextHelper helper)
    {
        if(helpers.Contains(helper))
            currentlySelected = helpers.IndexOf(helper);
    }
}
