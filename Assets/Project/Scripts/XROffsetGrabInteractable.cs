using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;


    public class XROffsetGrabInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        private Vector3 initialAttachLocalPos;
        private Quaternion initialAttachLocalRot;

        // Start is called before the first frame update
        void Start()
        {
            // Create attach point
            if (!attachTransform)
            {
                GameObject grab = new GameObject("Grab Pivot");
                grab.transform.SetParent(transform, false);
                attachTransform = grab.transform;
            }

            initialAttachLocalPos = attachTransform.localPosition;
            initialAttachLocalRot = attachTransform.localRotation;
        }
      
        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(OnSelectEnteredHandler);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(OnSelectEnteredHandler);
        }

    // Update is called once per frame
    private void OnSelectEnteredHandler(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject;

            if (interactor is XRDirectInteractor)
            {
                attachTransform.position = interactor.transform.position;
                attachTransform.rotation = interactor.transform.rotation;
            }
            else
            {
                attachTransform.localPosition = initialAttachLocalPos;
                attachTransform.localRotation = initialAttachLocalRot;
            }
        }


}
