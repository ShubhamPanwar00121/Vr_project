using Assets.Project.Chess3D.Pieces;
using Assets.Project.ChessEngine;
using Assets.Project.ChessEngine.Pieces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Chess3D
{
    public class Spawner : MonoBehaviour
    {
        public List<Transform> piecePrefabs;
        public GameObject Pieces;
        public GameController gc;
        public EventManager em;
        public Move moveFromSQq;
        public Transform pieceParent;

        void Start()
        {
        }

        public void DoMove(Move move)
        {
            moveFromSQq = move;
            if (move.IsEnPassant)
            {
                if (gc.Board.OnTurn == ChessEngine.Color.White)
                {
                    DestroyPiece(gc.Board.Pieces[(int)move.ToSq - 10]);
                }
                else
                {
                    DestroyPiece(gc.Board.Pieces[(int)move.ToSq + 10]);
                }
            }
            else if (move.IsCastle)
            {
                switch (move.ToSq)
                {
                    case Square.C1:
                        MovePiece(gc.Board.Pieces[(int)Square.A1], Board.Sq64((int)Square.D1));
                        break;
                    case Square.C8:
                        MovePiece(gc.Board.Pieces[(int)Square.A8], Board.Sq64((int)Square.D8));
                        break;
                    case Square.G1:
                        MovePiece(gc.Board.Pieces[(int)Square.H1], Board.Sq64((int)Square.F1));
                        break;
                    case Square.G8:
                        MovePiece(gc.Board.Pieces[(int)Square.H8], Board.Sq64((int)Square.F8));
                        break;
                }
            }

            if (move.CapturedPiece != null)
            {
                DestroyPiece(gc.Board.Pieces[(int)move.ToSq]);
            }

            if (move.PromotedPiece.HasValue)
            {
                DestroyPiece(gc.Board.Pieces[(int)move.FromSq]);
            }
            else MovePiece(gc.Board.Pieces[(int)move.FromSq], Board.Sq64((int)move.ToSq));
        }

        public PieceWrapper SpawnPiece(Piece piece)
        {
            Vector3 worldPoint = ToWorldPoint(Board.Sq64((int)piece.Square));
            Transform transform = Instantiate(piecePrefabs[piece.Index], pieceParent);
            transform.position = new Vector3(worldPoint.x, transform.position.y, worldPoint.z);
            transform.parent = Pieces.transform;
            PieceWrapper wrapper = transform.GetComponent<PieceWrapper>();
            wrapper.Square = piece.Square;
            wrapper.gameController = gc;
            wrapper.evManager = em;
            return wrapper;
        }

        IEnumerator WaitAndRetryDestroy(Piece piece)
        {
            //Print the time of when the function is first called.
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(2);
            DestroyPiece(piece);

            //just like the failed piece move go to the square 

        }

        public void DestroyPiece(Piece piece)
        {
            try
            {
                PieceWrapper wrapper = FindPieceWrapper(piece);
                if (wrapper == null)
                {
                    StartCoroutine(WaitAndRetryDestroy(piece));
                }

                wrapper.gameObject.SetActive(false);
                wrapper.gameObject.transform.position = new Vector3(100, 100, 100);

            }
            catch (Exception e)
            {
                Debug.Log(gc.Board.ToString());
                throw e;
            }
        }

        public PieceWrapper heldPiece;

        public void SetPieceWrapper(PieceWrapper pWrapper)
        {
            heldPiece = pWrapper;
        }

        public void MovePiece(Piece piece, int sq64)
        {
            Vector3 worldPoint = ToWorldPoint(sq64);
            PieceWrapper wrapper = FindPieceWrapper(piece);
            if (wrapper == null)
            {

                if (gc.inHandsRight)
                {
                    GameObject thePlayer = GameObject.Find("HandPointer");

                    IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                    wrapper = isHolding.heldPieceSquare;
                }
                else
                {
                    GameObject thePlayer = GameObject.Find("HandPointerLeft");

                    IsHoldingLeft isHolding = thePlayer.GetComponent<IsHoldingLeft>();
                    wrapper = isHolding.heldPieceSquare;
                }


            }
            wrapper.Square = (Square)Board.Sq120(sq64);
            wrapper.transform.position = new Vector3(worldPoint.x, 0.0078f, worldPoint.z);
            wrapper.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            wrapper.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            wrapper.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            wrapper.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        public PieceWrapper pickUpSquare;

        public PieceWrapper CurrentSqure()
        {
            if (gc.inHandsRight)
            {
                GameObject thePlayer = GameObject.Find("HandPointer");
                IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                return isHolding.heldPieceSquare;
            }
            else
            {
                GameObject thePlayer = GameObject.Find("HandPointerLeft");
                IsHoldingLeft isHoldingLeft = thePlayer.GetComponent<IsHoldingLeft>();
                return isHoldingLeft.heldPieceSquare;
            }

        }

        public PieceWrapper FindPieceWrapper(Piece piece)
        {
            //somehow get the pawns to spawn in the pieces game object?????
            PieceWrapper[] pieceWrappers = UnityEngine.Object.FindObjectsByType<PieceWrapper>(FindObjectsSortMode.None);
            foreach (PieceWrapper pieceWrapper in pieceWrappers)
            {

                if (pieceWrapper.Square == piece.Square)
                {
                    return pieceWrapper;
                }
                else if (pieceWrapper == null)
                {
                    SpawnPieceOnFailedMove(piece);
                }

            }
            return CurrentSqure();

        }

        public void SpawnPieceOnFailedMove(Piece piece)
        {
            Vector3 worldPoint = ToWorldPoint(Board.Sq64((int)piece.Square));
            Transform transform = Instantiate(piecePrefabs[piece.Index], pieceParent);
            transform.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            transform.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            transform.position = new Vector3(worldPoint.x, transform.position.y, worldPoint.z);

            transform.gameObject.GetComponent<PieceWrapper>().gameController = gc;
            transform.gameObject.GetComponent<PieceWrapper>().evManager = em;
        }

        public void GreenSquare(Move move)
        {
            GreenSqaureGo(gc.Board.Pieces[(int)move.ToSq]);
        }

        public void GreenSqaureGo(Piece piece)
        {
            PieceWrapper wrapper = FindPieceWrapper(piece);

            Vector3 worldPoint = ToWorldPoint(Board.Sq64((int)piece.Square));

            wrapper.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            wrapper.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            wrapper.transform.position = new Vector3(worldPoint.x, 1, worldPoint.z);


        }

        public void IntermediateFunctionGoodMove
            (Move move, Vector3 moveCubeSquare)
        {
            ReturnToSquare(gc.Board.Pieces[(int)move.ToSq], moveCubeSquare);
        }



        public void IntermediateFunction(Move move, Vector3 moveCubeSquare)
        {
            if (move == null) print("error in move");
            var piece = gc.Board.Pieces[(int)move.FromSq];
            ReturnToSquare(piece, moveCubeSquare);
        }


        public void ReturnToSquare(Piece piece, Vector3 moveToSquare)
        {
            PieceWrapper wrapper = FindPieceWrapper(piece);
            StartCoroutine(SmoothMove(wrapper.gameObject, moveToSquare, 0.5f)); // Adjust time as needed
        }
        IEnumerator SmoothMove(GameObject objectToMove, Vector3 targetPosition, float duration)
        {
            // Ensure Rigidbody does not affect the movement
            Rigidbody rb = objectToMove.GetComponent<Rigidbody>();
            bool wasKinematic = rb.isKinematic;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float time = 0;
            Vector3 startPosition = objectToMove.transform.position;
            Quaternion startRotation = objectToMove.transform.rotation;
            // Target rotation to ensure 'y' is up. Adjust as necessary.
            Quaternion targetRotation = Quaternion.Euler(0, objectToMove.transform.eulerAngles.y, 0);

            if (objectToMove != null)
            {
                while (time < duration)
                {
                    if (objectToMove != null)
                    {
                        objectToMove.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                        objectToMove.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time / duration);
                        time += Time.deltaTime;
                    }
                    yield return null;
                }
            }

            // Ensure the object is exactly at the target position and rotation
            objectToMove.transform.position = targetPosition;
            objectToMove.transform.rotation = targetRotation;

            // Restore Rigidbody state
            rb.isKinematic = wasKinematic;
        }

        public Vector3 ToWorldPoint(int cellNumber)
        {
            int j = cellNumber % 8;
            int i = cellNumber / 8;
            return new Vector3(i * -0.045f + 0.18f, 1, j * 0.045f - 0.18f);
        }
    }
}
