using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Chess3D.Dependency;
using Assets.Project.ChessEngine;
using Assets.Project.ChessEngine.Exceptions;
using UnityEngine.InputSystem;

namespace Assets.Project.ChessEngine
{
    public class MenuUiController : MonoBehaviour
    {
        public Dropdown DifficultyChooser;
        public bool pieceChoice1 = true;
        public bool pieceChoice2 = false;
        private readonly string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public GameObject handMenu;
        public InputActionReference handMenuToggleActionReference; // Reference to your Input Action

        void Start()
        {



        }

        void Update()
        {


        }

        private void OnDisable()
        {

        }

        private void OnExitClicked()
        {
            Application.Quit();
        }

        public void StartClickedWhite()
        {
            SceneLoading.Context.Clear();

            SceneLoading.Context.Inject(SceneLoading.Parameters.WhiteChoice, "Human");
            SceneLoading.Context.Inject(SceneLoading.Parameters.BlackChoice, "Bot");
            SceneLoading.Context.Inject(SceneLoading.Parameters.WhiteDepth, 4);
            SceneLoading.Context.Inject(SceneLoading.Parameters.BlackDepth, Convert.ToInt32(DifficultyChooser.options[DifficultyChooser.value].text));
            SceneLoading.Context.Inject(SceneLoading.Parameters.Fen, "");

            try
            {
                Board board = new Board(StartFen);
                SceneLoading.Context.Inject(SceneLoading.Parameters.Board, board);

            }
            catch (Exception)
            {
            }
        }

        public void StartClickedBlack()
        {
            SceneLoading.Context.Clear();

            SceneLoading.Context.Inject(SceneLoading.Parameters.WhiteChoice, "Bot");
            SceneLoading.Context.Inject(SceneLoading.Parameters.BlackChoice, "Human");
            SceneLoading.Context.Inject(SceneLoading.Parameters.WhiteDepth, Convert.ToInt32(DifficultyChooser.options[DifficultyChooser.value].text));
            SceneLoading.Context.Inject(SceneLoading.Parameters.BlackDepth, 4);
            SceneLoading.Context.Inject(SceneLoading.Parameters.Fen, "");

            try
            {
                Board board = new Board(StartFen);
                SceneLoading.Context.Inject(SceneLoading.Parameters.Board, board);
                print("board created");
            }
            catch (Exception)
            {
                print("board not created");
            }
        }

        IEnumerator LoadYourAsyncScene()
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        IEnumerator LoadScene()
        {
            yield return null;

            //Begin to load the Scene you specify
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Game");
            //Don't let the Scene activate until you allow it to
            asyncOperation.allowSceneActivation = false;
            Debug.Log("Pro :" + asyncOperation.progress);
            //When the load is still in progress, output the Text and progress bar
            while (!asyncOperation.isDone)
            {
                //Output the current progress
                //m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";

                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f)
                {
                    //Change the Text to show the Scene is ready
                    // m_Text.text = "Press the space bar to continue";
                    //Wait to you press the space key to activate the Scene
                    // if (Input.GetKeyDown(KeyCode.Space))
                    //Activate the Scene
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

    }
}