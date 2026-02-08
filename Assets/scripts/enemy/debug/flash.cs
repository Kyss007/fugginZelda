using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flash : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Material flashMaterial;

    public float flashTime = 0.2f;
    public int flashAmount = 5;

    private Material ogMaterial;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        ogMaterial = meshRenderer.material;
    }

    public void Flash()
    {
        if(gameObject.activeSelf)
            StartCoroutine(doFlash());
    }

    public IEnumerator doFlash()
    {
        for(int i = 0; i < flashAmount; i++)
        {
            meshRenderer.material = flashMaterial;
            yield return new WaitForSeconds(flashTime);
            meshRenderer.material = ogMaterial;
            yield return new WaitForSeconds(flashTime);
        }
    }
}
