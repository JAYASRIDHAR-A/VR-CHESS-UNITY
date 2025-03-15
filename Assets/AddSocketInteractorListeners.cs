using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class AddSocketListeners : MonoBehaviour
{
    private void OnEnable()
    {
        AddListenersToSocketInteractor();
    }

    private void AddListenersToSocketInteractor()
    {
        // Get the XRSocketInteractor component
        XRSocketInteractor socketInteractor = GetComponent<XRSocketInteractor>();
        if (socketInteractor == null)
        {
            Debug.LogWarning("No XRSocketInteractor component found on this GameObject.");
            return;
        }

        // Get the CHESSBOARDBOXMANAGER component
        CHESSBOARDBOXMANAGER chessBoardBoxManager = GetComponent<CHESSBOARDBOXMANAGER>();
        if (chessBoardBoxManager == null)
        {
            Debug.LogWarning("No CHESSBOARDBOXMANAGER component found on this GameObject.");
            return;
        }

        // Remove existing listeners to prevent duplicates
        socketInteractor.selectEntered.RemoveAllListeners();
        socketInteractor.selectExited.RemoveAllListeners();

        // Add new listeners to the events
        socketInteractor.selectEntered.AddListener(args => chessBoardBoxManager.PiecePlaced());
        socketInteractor.selectExited.AddListener(args => chessBoardBoxManager.PieceGrabbed());

        // Mark the socket interactor as dirty to save changes in the editor
        EditorUtility.SetDirty(socketInteractor);

        Debug.Log("Listeners successfully added to XRSocketInteractor.");
    }
}
