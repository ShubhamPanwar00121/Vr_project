using Assets.Project.ChessEngine;
using UnityEngine;

namespace Assets.Project.Chess3D.Pieces
{
    public class PieceWrapper : MonoBehaviour
    {
        public Square Square;
        public GameController gameController;
        public EventManager evManager;

        private bool clickTrue;
        public GameObject Pieces;

        void Start()
        {
            GameObject Pieces = GameObject.Find("Pieces");
            transform.parent = Pieces.transform;
        }

        void Update()
        {

        }

        public void ClickTrue()
        {
            clickTrue = true;
        }

        public void ClickFalse()
        {
            clickTrue = false;
        }

        public Vector3 originalPos;
        bool originSet = false;


        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == "Hands")
            {
                GameObject thePlayer = gameController.RHand.gameObject;
                GameObject thePlayerL = gameController.LHand.gameObject;

                IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                IsHoldingLeft isHoldingLeft = thePlayerL.GetComponent<IsHoldingLeft>();

                if (isHolding != null && isHoldingLeft != null)
                    if (!isHolding.inHands || !isHoldingLeft.inHands)
                    {
                        isHolding.inHands = true;
                        GameObject leftHandPointer = GameObject.Find("LeftController");
                        if (leftHandPointer != null)
                            isHoldingLeft.inHands = true;
                    }
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.tag == "Hands")
            {
                GameObject thePlayer = gameController.RHand.gameObject;
                IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                GameObject leftHandPointer = gameController.LHand.gameObject;
                IsHoldingLeft isHoldingLeft = leftHandPointer.GetComponent<IsHoldingLeft>();

                if (isHolding != null && isHoldingLeft != null)
                {
                    isHolding.inHands = false;

                    isHoldingLeft.inHands = false;
                }
            }
        }
    }
}
