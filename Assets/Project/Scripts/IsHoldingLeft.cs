using Assets.Project.Chess3D;
using Assets.Project.Chess3D.Pieces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Project.ChessEngine
{
    public class IsHoldingLeft : MonoBehaviour
    {
        public GameController gameController;
        public EventManager evManager;
        private bool clickTrue;
        bool isHoldingPiece;

        public float zCoordinate;
        public float xCoordinate;
        public float zCoordinateExit = 0;
        public float xCoordinateExit = 0;
        public GameObject RightHand;
        public PieceWrapper heldPieceSquare;
        public Spawner spawner;
        public bool inHands = false;

        public int grabPositionX;
        public int grabPositionZ;
        public Visualizer Visualizer;
        public Material selectedMat;
        public Material actuaMaterial;

        public InputActionReference grabBtn;

        private void OnEnable()
        {
            grabBtn.action.Enable();
            grabBtn.action.started += GrabPressed;
            grabBtn.action.canceled += GrabPressed;
        }

        private void OnDisable()
        {
            grabBtn.action.started -= GrabPressed;
            grabBtn.action.canceled -= GrabPressed;
            grabBtn.action.Disable();
        }

        public void GrabPressed(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                GrabPiece();
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                DropPiece();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Piece" && !inHands)
            {
                inHands = true;
                heldPieceSquare = other.transform.parent.GetComponent<PieceWrapper>();
                MeshRenderer mr = other.transform.parent.GetComponentInChildren<MeshRenderer>();
                actuaMaterial = mr.material;
                mr.material = selectedMat;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Piece" && inHands)
            {
                gameController.PickUpFalse();
                inHands = false;
                MeshRenderer mr = other.transform.parent.GetComponentInChildren<MeshRenderer>();
                heldPieceSquare = null;
                mr.material = actuaMaterial;
            }
        }

        public void GrabPiece()
        {

            if (inHands)
            {
                xCoordinate = transform.position.x;
                zCoordinate = transform.position.z;
                int sq64, sq120;

                sq64 = XAndZCoordinatesOfObject(xCoordinate, zCoordinate);
                if (sq64 < 0 || sq64 >= 64)
                {
                    print("not valid grab");
                }

                evManager.PickUpTrue();

                sq120 = Board.Sq120(sq64);

                gameController.PickUpTrue();
                gameController.SelectPiece(sq120, false, true, xCoordinate, zCoordinate);

                inHands = true;
            }
        }

        public void DropPiece()
        {
            if (!inHands) return;

            xCoordinateExit = transform.position.x;
            zCoordinateExit = transform.position.z;
            int sq64, sq120;

            sq64 = XAndZCoordinatesOfObject(xCoordinateExit, zCoordinateExit);

            if (sq64 < 0 || sq64 >= 64)
            {
                print("not valid grab");
            }

            if (gameController.IsValidCell(sq64))
            {
                sq120 = Board.Sq120(sq64);
                gameController.DoMove(sq120, false, true);
            }

            gameController.PickUpFalse();
            evManager.PickUpFalse();

            inHands = false;
        }

        public int RaycastCell(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 point = hit.point + new Vector3(-16, 0, 16);
                int i = (int)-point.x / 4;
                int j = (int)point.z / 4;
                return i * 8 + j;
            }
            return -1;
        }

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
    }
}