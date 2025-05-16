using Assets.Project.Chess3D;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace Assets.Project.ChessEngine
{
    public class EventManager : MonoBehaviour
    {
        public GameController gc;
        public Camera lHand;
        public Camera rHand;
        private bool clickTrue;
        private bool rightClickTrue;
        public Transform Pointer;
        private float nextActionTime = 0.0f;
        public float period = 0.5f;
        public int sq64;

        private bool blocked = false;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor xrRight;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor xrLeft;
        private XRController xrRightController;
        private XRController xrLeftController;
        private GameObject cameraL;
        private GameObject cameraR;

        public GameObject RightHand;
        public float zCoord;
        public float xCoord;
        public InputActionReference destroyPiecesReference;



        void Start()
        {
            gc = GameObject.Find("GameController").transform.GetComponent<GameController>();
        }

        bool inHands = false;
        public void PickUpTrue()
        {
            inHands = true;
        }
        public void PickUpFalse()
        {
            inHands = false;
        }

        void Update()
        {

        }

        private void OnEnable()
        {
            destroyPiecesReference.action.Enable();
            destroyPiecesReference.action.performed += OnDestroyPiecesPerformed;
        }

        private void OnDisable()
        {
            destroyPiecesReference.action.Disable();
            destroyPiecesReference.action.performed -= OnDestroyPiecesPerformed;
        }
        private void OnDestroyPiecesPerformed(InputAction.CallbackContext context)
        {
            gc.DestroyPieces();
        }



        void FixedUpdate()
        {

            if (blocked) return;
            if (!(gc.OnTurn is Human))
            {

                return;
            }
            if (clickTrue && !inHands)
            {


                RaycastHit hit;
                Ray ray = new Ray(lHand.transform.position, lHand.transform.forward);

                LayerMask mask = LayerMask.GetMask("Pieces");
                int sq64, sq120;

                if (Physics.Raycast(lHand.transform.position, lHand.transform.forward, out hit, 100, mask) && gc.SelectedPiece == null)
                {

                    sq64 = RaycastCell(ray);

                    sq120 = Board.Sq120(sq64);
                    gc.SelectPiece(sq120, false, false);
                    return;
                }

                else if (gc.SelectedPiece != null)
                {

                }
            }
            else if (!clickTrue && gc.SelectedPiece != null && !inHands)
            {
                Ray ray = new Ray(lHand.transform.position, lHand.transform.forward);

                int sq64, sq120;
                sq64 = RaycastCell(ray);

                if (gc.IsValidCell(sq64))
                {
                    sq120 = Board.Sq120(sq64);
                    gc.DoMove(sq120, false, false);
                }
            }
        }


        bool released;
        public void ReleaseFromHandCoordinates()
        {
            released = true;

        }


        public void ClickTrue()
        {
            clickTrue = true;
        }

        public void ClickFalse()
        {
            clickTrue = false;
        }

        public void RightClickTrue()
        {
            rightClickTrue = true;
        }

        public void RightClickFalse()
        {
            rightClickTrue = false;
        }

        int squareCoord = 0;

        public int XAndZCoordinatesOfObject(float xCoord, float zCoord)
        {
            Vector3 boardOrigin = new Vector3(-0.1557f, 0, -0.1978f);

            float squareSizeX = 0.0446f;
            float squareSizeZ = 0.0443f;

            Vector3 local = new Vector3(xCoord, 0, zCoord) - boardOrigin;

            int rank = 7 - Mathf.FloorToInt(local.x / squareSizeX);
            int file = Mathf.FloorToInt(local.z / squareSizeZ);

            if (file < 0 || file > 7 || rank < 0 || rank > 7)
            {
                return -1;
            }

            return rank * 8 + file;
        }

        public int RaycastCell(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 point = hit.point + new Vector3(-0.18f, 0f, 0.18f);
                int i = (int)(-point.x / 0.045f);
                int j = (int)(point.z / 0.045f);
                return i * 8 + j;
            }
            return -1;
        }

        public void BlockEvents()
        {
            blocked = true;
        }
    }
}