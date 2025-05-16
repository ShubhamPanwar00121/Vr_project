using System.Collections;
using System.Collections.Generic;
using UnityEngine;



    public abstract class PieceHands : MonoBehaviour
    {
        public string type;
        public int CurrentX { set; get; }
        public int CurrentY { set; get; }
        public int Value;
        public bool isWhite;
        public GameObject Object;
        public BoardManager _BoardManager;
        private BoardHighlights _BoardHighlights;

        private void Start()
        {
            _BoardManager = this.transform.parent.GetComponent<BoardManager>();

            _BoardHighlights = GameObject.Find("GameController").transform.GetComponent<BoardHighlights>();

        }
        public void SetPosition(int x, int y)
        {
            CurrentX = x;
            CurrentY = y;
        }

        public virtual bool[,] PossibleMoves()
        {
            return new bool[8, 8];
        }
        private void OnTriggerEnter(Collider collision)
        {

            if (collision.gameObject.tag == "Hands")
            {
                _BoardHighlights.HighlightSquares(PossibleMoves());
            }
        }

        private void OnTriggerExit(Collider collision)
        {

            if (collision.gameObject.tag == "Hands")
            {
                _BoardHighlights.HideHighlights(PossibleMoves());
            }
        }
    }
