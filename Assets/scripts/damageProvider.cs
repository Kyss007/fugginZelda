using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class damageProvider : MonoBehaviour
{
    [System.Serializable]
    public struct damageType
    {
        public float damage;
        public bool doesKnockback;
        public float knockbackUpForce;
        public float knockbackBackForce;
    }

    public SerializedDictionary<string, damageType> damageList;

    public float currentDamage;
    public bool doesKnockback;
    public float currentKnockbackUpForce;
    public float currentKnockbackBackForce;

    public void updateDamageState(string damageName)
    {
        damageList.TryGetValue(damageName, out damageType newDamage);

        currentDamage = newDamage.damage;
        doesKnockback = newDamage.doesKnockback;
        currentKnockbackUpForce = newDamage.knockbackUpForce;
        currentKnockbackBackForce = newDamage.knockbackBackForce;
    }
}
