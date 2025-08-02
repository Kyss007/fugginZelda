using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class dialogOptionTextHelper : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    public GameObject selectIndicator;

    public TextMeshProUGUI optionText;

    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponentInParent<dialogOptionText>().setSelectedFinal(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInParent<dialogOptionText>().setSelected(this);
    }
}
