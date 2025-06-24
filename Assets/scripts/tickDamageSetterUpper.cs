using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tickDamageSetterUpper : MonoBehaviour
{
    public string baseTickDamageName;

    private tickDamageProvider tickDamageProvider;

    private void Awake()
    {
        tickDamageProvider = GetComponent<tickDamageProvider>();
    }

    private void Start()
    {
        tickDamageProvider.updateDamageState(baseTickDamageName);
    }
}
