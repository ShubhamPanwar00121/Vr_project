using Assets.Project.Chess3D.Pieces;
using Assets.Project.ChessEngine;
using Assets.Project.ChessEngine.Pieces;
using Chess3D.Dependency;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace Assets.Project.Chess3D
{
    public class GameController : MonoBehaviour
    {
        public EventManager EventManager;
        public Spawner Spawner;

        public Visualizer Visualizer;

        public Board Board { get; private set; }
        public Piece SelectedPiece { get; set; }

        public IPlayer[] Players { get; private set; }
        public IPlayer OnTurn { get; private set; }

        private int playerWins = 0;
        private int aiWins = 0;
        private int aiLosses = 0;
        private int playerLosses = 0;
        private bool isAiWhite = false;
        private bool isAiBlack = false;
        private bool isHumamWhite = false;
        private bool isHumamBlack = false;

        private int consecutiveWins = 0;


        public ContinueTrigger ContinueTrigger;
        public MenuUiController MenuUiController;

        public GameObject snapPrefab;
        public GameObject[,] snapZones { set; get; }
        private const float TILE_SIZE = 0.045f;
        public InputActionReference resetPiecesReference;
        public GameObject AiLastMove;
        public GameObject badMove;
        public GameObject gameEnvironment;
        private bool playerIsWinner = false;
        private bool aiIsWinner = false;

        public EventManager eventManager;
        public GameObject pawn;
        private bool isSelected;
        public bool inHandsSpawnerRef;
        private bool isSelected2;
        private float xCoordMoveSquare;
        private float zCoordMoveSquare;

        private float xCoordGoodMoveSquare;
        private float zCoordGoodMoveSquare;
        public bool inHandsRight;
        private bool unableToSelect;

        void Start()
        {

            SpawnAllSnapzones();

            Board = SceneLoading.Context.Resolve<Board>(SceneLoading.Parameters.Board);

            Players = new IPlayer[2];

            var wc = SceneLoading.Context.Resolve<string>(SceneLoading.Parameters.WhiteChoice);
            var bc = SceneLoading.Context.Resolve<string>(SceneLoading.Parameters.BlackChoice);
            var wd = SceneLoading.Context.Resolve<int>(SceneLoading.Parameters.WhiteDepth);
            var bd = SceneLoading.Context.Resolve<int>(SceneLoading.Parameters.BlackDepth);

            if (wc.Equals("Human"))
            {
                OnTurn = Players[0] = new Human(this, "White player");
                isHumamWhite = true;
            }
            else
            {
                OnTurn = Players[0] = new Bot(this, "White player", wd);
                isAiWhite = true;
            }

            if (bc.Equals("Human"))
            {
                Players[1] = new Human(this, "Black player");
                isHumamBlack = true;
            }
            else
            {
                Players[1] = new Bot(this, "Black player", bd);
                isAiBlack = true;
            }

            SetupPiecesMarble();



            StartGame();
        }

        private void OnEnable()
        {
            resetPiecesReference.action.Enable();
            resetPiecesReference.action.performed += OnResetPiecesPerformed;
        }

        private void OnDisable()
        {
            resetPiecesReference.action.Disable();
            resetPiecesReference.action.performed -= OnResetPiecesPerformed;
        }

        private void OnResetPiecesPerformed(InputAction.CallbackContext context)
        {
            DestroyPieces();
            playerWins += 1;
            PlayerPrefs.SetInt("playerWins", playerWins);
        }

        private void Update()
        {

        }


        //snapzone sector
        void SpawnAllSnapzones()
        {
            snapZones = new GameObject[8, 8];
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    SpawnSnapZone(i, j);
                }
            }
        }

        void TurnOnSnapZones()
        {

        }

        void SnapZoneDestroyer()
        {
            GameObject[] zones;

            zones = GameObject.FindGameObjectsWithTag("SnapSphere");

            foreach (GameObject zone in zones)
            {
                zone.SetActive(false);
                StartCoroutine(Staller(zone));

            }
        }

        public IEnumerator Staller(GameObject zone)
        {
            yield return new WaitForSeconds(4f);
            zone.SetActive(true);

        }

        void SpawnSnapZone(int x, int y)
        {
            GameObject temp = Instantiate(snapPrefab, GetTileCentre(x, y), Quaternion.Euler(0, 0, 0)) as GameObject;
            temp.transform.SetParent(transform);
            snapZones[x, y] = temp;
        }

        void ResetSnapZone(int x, int y)
        {
            snapZones[x, y] = null;
            SpawnSnapZone(x, y);
        }

        void ResetAllSnapZones()
        {
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    ResetSnapZone(i, j);
                }
            }
        }

        public void DestroyPieces()
        {
            PieceWrapper[] pieceWrappers = Object.FindObjectsByType<PieceWrapper>(FindObjectsSortMode.None);

            foreach (PieceWrapper pieceWrapper in pieceWrappers)
            {
                pieceWrapper.gameObject.SetActive(false);
                pieceWrapper.gameObject.transform.position = new Vector3(pieceWrapper.gameObject.transform.position.x, pieceWrapper.gameObject.transform.position.y - 1000, pieceWrapper.gameObject.transform.position.z);
                pieceWrapper.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().colliders.Clear();

                Destroy(pieceWrapper.gameObject);
            }

            ResetAllSnapZones();
            SetupPiecesMarble();

            //need to find out what pieces have spawned


            if (pieceWrappers.Length == 0)
            {

            }
        }

        Vector3 GetTileCentre(int x, int y)
        {
            //Vector3 origin = Vector3.zero;
            Vector3 origin = new Vector3(-0.18f, 0.0241f, -0.18f);
            origin.x += TILE_SIZE * x + TILE_SIZE / 2;
            origin.z += TILE_SIZE * y + TILE_SIZE / 2;

            return origin + new Vector3(TILE_SIZE / 2, 0, -TILE_SIZE / 2);
        }

        public void SetupPieces()
        {
            foreach (Piece piece in Board.Pieces)
            {
                if (piece == null || piece is OffLimits) continue;
                Spawner.SpawnPiece(piece);
            }
        }

        public void ToggleGrab(GameObject obj)
        {
            if (obj.GetComponent<XROffsetGrabInteractable>() != null)
            {
                GameObject.Destroy(obj.GetComponent<XROffsetGrabInteractable>());
            }
            else
            {
                obj.AddComponent<XROffsetGrabInteractable>();
            }
        }

        public void SetGrab(GameObject obj, bool enable)
        {
            if (enable)
            {
                if (obj.GetComponent<XROffsetGrabInteractable>() == null)
                {
                    obj.AddComponent<XROffsetGrabInteractable>();
                }
            }
            else
            {
                if (obj.GetComponent<XROffsetGrabInteractable>() != null)
                {
                    GameObject.Destroy(obj.GetComponent<XROffsetGrabInteractable>());
                }
            }
        }

        public void SetupPiecesMarble()
        {
            foreach (Piece piece in Board.Pieces)
            {
                if (piece == null || piece is OffLimits) continue;
                Spawner.SpawnPiece(piece);
            }
        }



        private async void StartGame()
        {
            while (true)
            {
                OnTurn = Players[(int)Board.OnTurn];
                if (NoPossibleMoves()) break;

                Move move = await OnTurn.CalculateNextMove();

                if (OnTurn is Bot)
                {

                    Bot bot = OnTurn as Bot;
                    //Reset the board if needed

                    SnapZoneDestroyer();

                    SelectPiece((int)move.FromSq, false, false);
                    DoMove((int)move.ToSq, false, false);
                }
                else
                {
                    await OnTurn.SelectPiece();
                    if (SelectedPiece == null) continue;
                    await OnTurn.DoMove();

                }
            }

            EndGame();


        }
        //
        public IEnumerator Waiter(Bot bot, Move move)
        {
            yield return new WaitForSeconds(0.5f);
            print("00121 1");
            SelectPiece((int)move.FromSq, false, false);
            DoMove((int)move.ToSq, false, false);
        }

        public void EndGameTimer()
        {
            aiWins++;
            playerLosses++;
            PlayerPrefs.SetInt("aiWins", aiWins);
            PlayerPrefs.SetInt("playerLosses", playerLosses);
        }



        public void ClearChildren()
        {
            Debug.Log(transform.childCount);
            int i = 0;

            //Array to hold all child obj
            GameObject[] allChildren = new GameObject[transform.childCount];

            //Find all child obj and store to that array
            foreach (Transform child in transform)
            {
                allChildren[i] = child.gameObject;
                i += 1;
            }

            //Now destroy them
            foreach (GameObject child in allChildren)
            {
                DestroyImmediate(child.gameObject);
            }

            Debug.Log(transform.childCount);
        }



        public void EndGame()
        {

            IPlayer winner = Players[(int)Board.OnTurn ^ 1];
            EventManager.BlockEvents();

            if (OnTurn is Human)
            {
                consecutiveWins = 0;
                PlayerPrefs.SetInt("consecutiveWins", consecutiveWins);
                PlayerPrefs.SetInt("aiWins", aiWins);
                PlayerPrefs.SetInt("playerLosses", playerLosses);
            }
            else if (OnTurn is Bot)
            {
                PlayerPrefs.SetInt("consecutiveWins", consecutiveWins);
                PlayerPrefs.SetInt("playerWins", playerWins);
                PlayerPrefs.SetInt("aiLosses", aiLosses);
            }

            StartCoroutine(TimeWaster());
        }

        IEnumerator TimeWaster()
        {
            yield return new WaitForSeconds(5);
            SceneManager.LoadScene("Game");

        }

        private bool NoPossibleMoves()
        {
            MoveList moveList = Board.GenerateAllMoves();
            foreach (Move move in moveList)
            {
                if (Board.DoMove(move))
                {
                    Board.UndoMove();
                    return false;
                }
            }

            aiIsWinner = true;
            return true;
        }

        public bool playerTurn = false;

        private void ReleaseHumanSemaphore()
        {
            Human human = OnTurn as Human;
            playerTurn = true;
            if (human != null)
            {
                if (human.Semaphore.CurrentCount == 0) human.Semaphore.Release();
            }
        }



        public void RemoveHighlight()
        {
            //Visualizer.RemoveHighlightFromPiece(SelectedPiece);
        }

        public void SelectPiece(int sq120, bool inHandsRight, bool inHandsLeft, float xVal = 0, float zVal = 0)
        {
            isSelected = true;

            GameObject[] objects = GameObject.FindGameObjectsWithTag("Highlight");
            foreach (GameObject o in objects)
            {
                Destroy(o);
            }

            SelectedPiece = Board.Pieces[sq120];

            if (OnTurn is Human)
            {
                if (inHandsRight)
                {
                    unableToSelect = true;
                    inHandsSpawnerRef = true;
                    inHandsRight = true;
                    GameObject thePlayer = RHand.gameObject;
                    IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                    xCoordMoveSquare = isHolding.xCoordinate;
                    zCoordMoveSquare = isHolding.zCoordinate;
                    GameObject instance = Instantiate(Resources.Load("MoveCube")) as GameObject;
                    instance.transform.position = ToWorldPoint(eventManager.XAndZCoordinatesOfObject(xVal, zVal));


                }
                else if (inHandsLeft)
                {
                    unableToSelect = true;
                    inHandsSpawnerRef = true;
                    inHandsRight = false;

                    GameObject thePlayer = LHand.gameObject;
                    IsHoldingLeft isHolding = thePlayer.GetComponent<IsHoldingLeft>();
                    xCoordMoveSquare = isHolding.xCoordinate;
                    zCoordMoveSquare = isHolding.zCoordinate;
                    GameObject instance = Instantiate(Resources.Load("MoveCube")) as GameObject;
                    instance.transform.position = ToWorldPoint(eventManager.XAndZCoordinatesOfObject(xVal, zVal));
                }
            }

            ReleaseHumanSemaphore();
        }


        public void RemoveHighlightMk2(int sq120)
        {
            SelectedPiece = Board.Pieces[sq120];
        }


        public void SelectPiecePrimary(int sq120)
        {

            SelectedPiece = Board.Pieces[sq120];
            Visualizer.HighlightPiece(SelectedPiece);

            if (SelectedPiece != null)
            {
                if (SelectedPiece.Square == (Square)sq120)
                {
                    SelectedPiece = null;
                    ReleaseHumanSemaphore();
                    return;
                }
            }

        }

        Move theMove;

        public void DoMove(int sq120, bool inHandsRight, bool inHandsLeft)
        {
            if (SelectedPiece.Square == (Square)sq120)
            {

                SelectedPiece = null;
                ReleaseHumanSemaphore();
                playerTurn = false;
                return;
            }
            if (OnTurn is Bot)
            {
                GameObject instance = Instantiate(AiLastMove) as GameObject;
                instance.transform.position = ToWorldPoint(Board.Sq64(sq120));
            }

            Move foundMove = null;
            MoveList moveList = Board.GenerateAllMoves();

            foreach (Move move in moveList)
            {
                if (move.FromSq == SelectedPiece.Square && move.ToSq == (Square)sq120)
                {
                    foundMove = move;

                    break;
                }
                else if (move.FromSq == SelectedPiece.Square)
                {
                    theMove = move;
                }
            }

            var IllegalMovePiece = SelectedPiece;
            SelectedPiece = null;

            if (Board.MoveExists(foundMove))
            {
                if (OnTurn is Human)
                {
                    if (inHandsRight && unableToSelect)
                    {
                        float xCoord, zCoord;
                        GameObject thePlayer = RHand;
                        IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                        xCoordGoodMoveSquare = RHand.transform.position.x;
                        zCoordGoodMoveSquare = RHand.transform.position.z;
                        GameObject instance = Instantiate(Resources.Load("GoodMove")) as GameObject;
                        instance.transform.position = ToWorldPoint(eventManager.XAndZCoordinatesOfObject(RHand.transform.position.x, RHand.transform.position.z));
                        inHandsSpawnerRef = false;
                        Vector3 moveCubeSquare = new Vector3(xCoordGoodMoveSquare, 0.0075f, zCoordGoodMoveSquare);
                        Spawner.IntermediateFunction(foundMove, moveCubeSquare);

                        inHandsRight = false;
                    }
                    else if (inHandsLeft && unableToSelect)
                    {
                        float xCoord, zCoord;
                        GameObject thePlayer = LHand;
                        IsHoldingLeft isHolding = thePlayer.GetComponent<IsHoldingLeft>();
                        xCoordGoodMoveSquare = LHand.transform.position.x;
                        zCoordGoodMoveSquare = LHand.transform.position.z;
                        GameObject instance = Instantiate(Resources.Load("GoodMove")) as GameObject;
                        instance.transform.position = ToWorldPoint(eventManager.XAndZCoordinatesOfObject(LHand.transform.position.x, LHand.transform.position.z));
                        inHandsSpawnerRef = false;
                        Vector3 moveCubeSquare = new Vector3(xCoordGoodMoveSquare, 0.0075f, zCoordGoodMoveSquare);
                        Spawner.IntermediateFunction(foundMove, moveCubeSquare); 
                    }
                }
                Spawner.DoMove(foundMove);
                Board.DoMove(foundMove);
                if (foundMove.PromotedPiece.HasValue)
                {
                    Spawner.SpawnPiece(Board.Pieces[(int)foundMove.ToSq]);
                }
            }
            else
            {
                if (OnTurn is Human)
                {
                    if (inHandsRight && unableToSelect)
                    {

                        float xCoord, zCoord;
                        GameObject thePlayer = RHand;
                        IsHolding isHolding = thePlayer.GetComponent<IsHolding>();
                        xCoord = isHolding.xCoordinate;
                        zCoord = isHolding.zCoordinate;
                        GameObject instance = Instantiate(badMove) as GameObject;
                        instance.transform.position = ToWorldPoint(eventManager.XAndZCoordinatesOfObject(RHand.transform.position.x, RHand.transform.position.z));
                        inHandsSpawnerRef = false;
                        Vector3 moveCubeSquare = new Vector3(xCoordMoveSquare, 0.0075f, zCoordMoveSquare);
                        Spawner.IntermediateFunction(theMove, moveCubeSquare);
                        inHandsRight = false;

                    }
                    else if (inHandsLeft && unableToSelect)
                    {
                        float xCoord, zCoord;
                        GameObject thePlayer = LHand;
                        IsHoldingLeft isHolding = thePlayer.GetComponent<IsHoldingLeft>();
                        xCoord = isHolding.xCoordinate;
                        zCoord = isHolding.zCoordinate;
                        GameObject instance = Instantiate(badMove) as GameObject;
                        instance.transform.position = ToWorldPoint(eventManager.XAndZCoordinatesOfObject(LHand.transform.position.x, LHand.transform.position.z));
                        inHandsSpawnerRef = false;
                        Vector3 moveCubeSquare = new Vector3(xCoordMoveSquare, 0.0075f, zCoordMoveSquare);
                        Spawner.IntermediateFunction(theMove, moveCubeSquare);
                    }
                }

            }

            unableToSelect = false;

            ReleaseHumanSemaphore();
        }

        public GameObject RHand;
        public GameObject LHand;

        public void ReturnToOriginalSquare()
        {
            Vector3 moveCubeSquare = new Vector3(xCoordMoveSquare, 0.0075f, zCoordMoveSquare);
            Spawner.IntermediateFunction(theMove, moveCubeSquare);
        }

        IEnumerator ReturnPieceToBoard(Move move)
        {
            //Print the time of when the function is first called.
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(2);

            Vector3 moveCubeSquare = new Vector3(xCoordGoodMoveSquare, 0.0075f, zCoordGoodMoveSquare);
            Spawner.IntermediateFunction(move, moveCubeSquare);
            //just like the failed piece move go to the square 

        }



        bool rightClickTrue = false;
        bool clickTrue = false;
        bool inHands = false;
        bool inHandCustom = false;

        private GameController gc;

        public void RightClickTrue()
        {
            rightClickTrue = true;
        }

        public void RightClickFalse()
        {
            rightClickTrue = false;
        }
        public void ClickTrue()
        {

            clickTrue = true;
        }

        public void PickUpTrue()
        {
            inHands = true;
        }

        public bool IsInHandsCustom()
        {
            return inHandCustom;
        }

        public void SetInaHandsCustom(bool b)
        {
            inHandCustom = b;
        }

        public void PickUpFalse()
        {
            inHands = false;
        }


        public void ClickFalse()
        {
            clickTrue = false;
        }

        public string GetCellString(int cellNumber)
        {
            int j = cellNumber % 8;
            int i = cellNumber / 8;
            return char.ConvertFromUtf32(j + 65) + "" + (i + 1);
        }

        public Vector3 ToWorldPoint(int cellNumber)
        {
            int file = cellNumber % 8;         
            int rank = 7 - (cellNumber / 8);   

            Vector3 boardOrigin = new Vector3(-0.1557f, 0, -0.1978f);
            float squareSizeX = 0.0446f;
            float squareSizeZ = 0.0443f;

            float x = boardOrigin.x + (rank + 0.5f) * squareSizeX;
            float z = boardOrigin.z + (file + 0.5f) * squareSizeZ;

            return new Vector3(x, 0.0114f, z);  
        }

        public Vector3 ToWorldPointReal(int cellNumber)
        {
            int j = cellNumber % 8;
            int i = cellNumber / 8;
            return new Vector3(i, 0.0114f, j);
        }

        public bool IsValidCell(int cellNumber)
        {
            return cellNumber >= 0 && cellNumber <= 63;
        }
    }
}
