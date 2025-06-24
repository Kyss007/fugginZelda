using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class displayPlayerHealth : MonoBehaviour
{
    public healthProvider healthProvider;

    public Slider healthSlider;
    public Slider damageSlider;

    public TextMeshProUGUI text;

    private void Update()
    {
        healthSlider.maxValue = healthProvider.getMaxHealth();
        damageSlider.maxValue = healthProvider.getMaxHealth();

        healthSlider.value = healthProvider.getTargetHealth();
        damageSlider.value = healthProvider.getHealth();

        text.text = healthProvider.getTargetHealth().ToString();
    }
}
