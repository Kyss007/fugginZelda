using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class healthProvider : MonoBehaviour
{
    public UnityEvent onDeath;
    public UnityEvent onDamage;
    public UnityEvent onHeal;

    public bool dead = false;

    [SerializeField] private float actualHealth = 100;
    [SerializeField] private float targetHealth = 100;
    [SerializeField] private float maxHealth = 100;
    public float fancyLerpedHealth = 100;

    [Space]
    public bool capDamage = false;
    public float afterHitInvincibility = 0.2f;
    public float lerpSpeed = 0.05f;
    public float lerpOffset = 1f;

    [Space]
    public float lastChanceThreshold = 5;
    private bool lastChanceGranted = false;

    private float lastDamageTime;

    private Coroutine tickDamageRoutine;
    private float currentTickDamageAmount;
    private float currentTickDamageInterval;
    private float currentTickDamageDuration;
    private float currentTick = 0;

    private void Start()
    {
        StartCoroutine(updateFancyLerp(targetHealth));
    }

    private void Update()
    {
        if (targetHealth > actualHealth)
        {
            actualHealth = targetHealth;
        }
        else if (targetHealth < actualHealth)
        {
            actualHealth = Mathf.Lerp(actualHealth, fancyLerpedHealth, lerpSpeed * Time.deltaTime);
        }

        if(!dead && targetHealth <= 0)
        {
            if (gameObject.CompareTag("Player"))
            {
                Debug.LogWarning("Player Death \n why did you have to kill my boy TT. he was everything i had. you bastard.");
            }

            onDeath.Invoke();
            dead = true;
        }
    }

    public IEnumerator updateFancyLerp(float input)
    {
        yield return new WaitForSeconds(lerpOffset);

        fancyLerpedHealth = input;

        StartCoroutine(updateFancyLerp(targetHealth));
    }

    public float getHealth()
    {
        return actualHealth;
    }

    public float getTargetHealth()
    {
        return targetHealth;
    }

    public float getMaxHealth()
    {
        return maxHealth;
    }

    public float damage(float amount)
    {
        if(Time.time - lastDamageTime >= afterHitInvincibility || !capDamage)
        {
            lastDamageTime = Time.time;

            targetHealth -= amount;

            if(targetHealth <= 0)
            {
                targetHealth = 0;

                if(amount >= lastChanceThreshold && !lastChanceGranted)
                {
                    lastChanceGranted = true;
                    targetHealth = 1;
                }
            }

            onDamage.Invoke();
        }

        return targetHealth;
    }

    public void doTickDamage(float amount, float interval, int duration)
    {
        if (amount != currentTickDamageAmount)
        {
            currentTickDamageAmount = amount;
        }

        if (duration != currentTickDamageDuration)
        {
            currentTickDamageDuration = duration;
        }

        if (interval != currentTickDamageInterval)
        {
            currentTickDamageInterval = interval;
        }

        currentTick = duration;

        if(tickDamageRoutine == null)
        {
            tickDamageRoutine = StartCoroutine(tickDamage());
        }
    }

    public IEnumerator tickDamage()
    {
        damage(currentTickDamageAmount);

        yield return new WaitForSeconds(1 / currentTickDamageInterval);

        currentTick--;

        if(currentTick < 0)
        {
            tickDamageRoutine = null;
        }
        else
        {
            StartCoroutine(tickDamage());
        }
    }

    public float heal()
    {
        targetHealth = maxHealth;

        onHeal.Invoke();
        return targetHealth;
    }

    public float heal(float amount)
    {
        targetHealth += amount;

        if(targetHealth >= maxHealth)
        {
            targetHealth = maxHealth;
        }

        onHeal.Invoke();
        return targetHealth;
    }
}
