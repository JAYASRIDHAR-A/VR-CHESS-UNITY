using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ModifiedSocket : UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor
{
    [SerializeField] CHESSBOARDBOXMANAGER chessBoardBoxManager = null;
    [SerializeField] int eventCount = 0;
    [SerializeField] public bool CanActivate = true;
    protected override void Awake()
    {
        chessBoardBoxManager = GetComponent<CHESSBOARDBOXMANAGER>();
        attachTransform = transform.GetChild(0);
        base.Awake();
        selectEntered.AddListener(args => chessBoardBoxManager.PiecePlaced( args));
        selectExited.AddListener(args => chessBoardBoxManager.PieceGrabbed());
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (eventCount > 1&&CanActivate)
        {
            print(this.name);
            //chessBoardBoxManager.PiecePlaced();
            base.OnSelectEntered(args);
        }
        else { eventCount++; }

       
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (CanActivate)
        {
            print(this.name);
            //chessBoardBoxManager.PieceGrabbed();
            base.OnSelectExited(args);
        }
    }
}
