using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class DebugSerializedProperties : MonoBehaviour
{
    private void Start()
    {
        DebugSerializedPropertiesOfXRSocketInteractor();
    }

    private void DebugSerializedPropertiesOfXRSocketInteractor()
    {
        XRSocketInteractor socketInteractor = GetComponent<XRSocketInteractor>();
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
