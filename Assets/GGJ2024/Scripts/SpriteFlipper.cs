using System.Collections.Generic;
using UnityEngine;

public class SpriteFlipper : MonoBehaviour
{
    public Dictionary<SpriteRenderer, float> spriteRenderers = new();

    void Awake()
    {
        foreach (SpriteRenderer spriteRenderer in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            if (spriteRenderer.transform != transform)
            {
                spriteRenderers.Add(spriteRenderer, spriteRenderer.transform.localPosition.z);
            }
        }
    }

    private void Update()
    {
        bool isFlipped = transform.root.localRotation.y != 0;
        foreach (KeyValuePair<SpriteRenderer, float> kvp in spriteRenderers)
        {
            SpriteRenderer renderer = kvp.Key;
            renderer.material.SetInt("_SpriteFlipped", isFlipped ? 1 : 0);
            renderer.transform.localPosition = new Vector3(renderer.transform.localPosition.x, renderer.transform.localPosition.y, kvp.Value * (isFlipped ? -1 : 1));
        }
    }
}
