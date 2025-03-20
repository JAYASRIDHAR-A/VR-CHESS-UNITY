using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class DebugSerializedProperties : MonoBehaviour
{
    private void Start()
    {
        DebugSerializedPropertiesOfXRSocketInteractor();
    }

    private void DebugSerializedPropertiesOfXRSocketInteractor()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        if (socketInteractor == null)
        {
            Debug.LogWarning("No XRSocketInteractor component found on this GameObject.");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(socketInteractor);
        SerializedProperty property = serializedObject.GetIterator();

        Debug.Log("Listing all serialized properties of XRSocketInteractor:");
        while (property.NextVisible(true))
        {
            Debug.Log($"Property Name: {property.name}, Type: {property.type}");
        }
    }
}
