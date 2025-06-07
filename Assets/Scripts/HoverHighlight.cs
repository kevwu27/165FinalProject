using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    public Material defaultMaterial;
    public Material highlightMaterial;

    private Renderer pieceRenderer;

    void Start()
    {
        pieceRenderer = GetComponent<Renderer>();
        if (pieceRenderer != null && defaultMaterial != null)
        {
            pieceRenderer.material = defaultMaterial;
        }
    }

    public void SetHighlight(bool highlighted)
    {
        if (pieceRenderer != null && highlightMaterial != null && defaultMaterial != null)
        {
            pieceRenderer.material = highlighted ? highlightMaterial : defaultMaterial;
        }
    }
} 