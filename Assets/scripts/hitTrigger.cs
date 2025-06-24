using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hitTrigger : MonoBehaviour
{
    public bool isEnabled = true;

    public bool doesTickDamage = false;

    public damageProvider damageProvider;
    public tickDamageProvider tickDamageProvider;

    private void OnTriggerEnter(Collider other)
    {
        if (!isEnabled)
        {
            return;
        }

        hitReciever hitReciever = other.GetComponent<hitReciever>();

        if (hitReciever != null)
        {
            hitReciever.onHit.Invoke();

            if (!doesTickDamage)
            {
                hitReciever.healthProvider?.damage(damageProvider.currentDamage);
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!isEnabled)
        {
            return;
        }

        hitReciever hitReciever = other.GetComponent<hitReciever>();

        if (hitReciever != null)
        {
            hitReciever.onHit.Invoke();

            if (doesTickDamage)
            {
                hitReciever.healthProvider?.doTickDamage(tickDamageProvider.currentDamage.amount, tickDamageProvider.currentDamage.interval, tickDamageProvider.currentDamage.duration);
            }
        }
    }

    public void enableHitDetection()
    {
        isEnabled = true;
    }

    public void disableHitDetection()
    {
        isEnabled = false;
    }
}
