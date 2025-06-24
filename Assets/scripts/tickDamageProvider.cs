using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class tickDamageProvider : MonoBehaviour
{
    public SerializedDictionary<string, tickDamageValues> tickDamageList;

    [System.Serializable]
    public struct tickDamageValues
    {
        public float amount;
        public float interval;
        public int duration;
    }

    public tickDamageValues currentDamage;

    public void updateDamageState(string damageName)
    {
        tickDamageList.TryGetValue(damageName, out tickDamageValues newDamage);

        currentDamage = newDamage;
    }
}
