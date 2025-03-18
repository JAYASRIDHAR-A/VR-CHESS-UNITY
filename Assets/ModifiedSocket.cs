using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ModifiedSocket : XRSocketInteractor
{
    [SerializeField] CHESSBOARDBOXMANAGER chessBoardBoxManager = null;
    [SerializeField] int eventCount = 0;
    protected override void Awake()
    {
        chessBoardBoxManager = GetComponent<CHESSBOARDBOXMANAGER>();
        attachTransform = transform.GetChild(0);
        base.Awake();
        selectEntered.AddListener(args => chessBoardBoxManager.PiecePlaced());
        selectExited.AddListener(args => chessBoardBoxManager.PieceGrabbed());
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (eventCount > 1)
        {
            //chessBoardBoxManager.PiecePlaced();
            base.OnSelectEntered(args);
        }
        else { eventCount++; }

       
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
            //chessBoardBoxManager.PieceGrabbed();
            base.OnSelectExited(args);

    }
}
