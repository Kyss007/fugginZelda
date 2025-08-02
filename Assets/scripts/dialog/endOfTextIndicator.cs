using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class endOfTextIndicator : MonoBehaviour
{
    private TextMeshProUGUI text;

    public float smallTime = 0.2f;
    public float bigTime = 0.5f;

    private Coroutine thingyRoutine = null;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    
    private void Start()
    {
        if(thingyRoutine == null)
            thingyRoutine = StartCoroutine(doThingey());
    }

    private void OnEnable()
    {
        if(thingyRoutine == null)
                thingyRoutine = StartCoroutine(doThingey());
    }

    private void OnDisable()
    {
        StopCoroutine(thingyRoutine);
        thingyRoutine = null;
    }

    public IEnumerator doThingey()
    {
        text.text = "v";

        yield return new WaitForSecondsRealtime(smallTime);

        text.text = "V";

        yield return new WaitForSecondsRealtime(bigTime);

        StartCoroutine(doThingey());
    } 
}
