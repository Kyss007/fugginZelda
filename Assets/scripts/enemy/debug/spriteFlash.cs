using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spriteFlash : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color flashColor;

    public float flashTime = 0.2f;
    public int flashAmount = 5;

    private Color ogColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ogColor = spriteRenderer.color;
    }

    public void Flash()
    {
        StartCoroutine(doFlash());
    }

    public IEnumerator doFlash()
    {
        for(int i = 0; i < flashAmount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashTime);
            spriteRenderer.color = ogColor;
            yield return new WaitForSeconds(flashTime);
        }
    }
}
