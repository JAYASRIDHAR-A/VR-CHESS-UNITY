using UnityEngine;

public class Highlighter : MonoBehaviour
{
    public Color highlightColor = Color.yellow; // Color to apply when highlighted.
    private Material[] originalMaterials;       // Store the original materials.
    private Material[] highlightMaterials;      // Temporary materials for highlighting.
    private Renderer objectRenderer;            // Renderer of the object.

    void Awake()
    {
        // Get the Renderer component attached to the object.
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            // Store the original materials.
            originalMaterials = objectRenderer.materials;

            // Create a new array for highlight materials.
            highlightMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                // Create a new material using the URP Lit shader.
                highlightMaterials[i] = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                highlightMaterials[i].color = highlightColor; // Apply the highlight color.
            }
        }
        else
        {
            Debug.LogError("Renderer not found on the object.");
        }
    }

    void OnEnable()
    {
        print(objectRenderer.materials.Length + " " + this.name);
        // Apply the highlight materials when the script is enabled.
        if (objectRenderer != null && highlightMaterials != null)
        {
            objectRenderer.materials = highlightMaterials; // Set the new highlighted materials.
        }
    }

    void OnDisable()
    {
        // Revert back to the original materials when the script is disabled.
        if (objectRenderer != null && originalMaterials != null)
        {
            objectRenderer.materials = originalMaterials; // Restore the original materials.
        }
    }
}
