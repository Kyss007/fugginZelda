using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class slidderValueLabel : MonoBehaviour
{
    public Slider slider;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Start()
    {
        text.text = slider.value.ToString();
        slider.onValueChanged.AddListener(updateText);
    }

    public void updateText(float value)
    {
        text.text = value.ToString();
    }
}
