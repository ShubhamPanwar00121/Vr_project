using Assets.Project.Chess3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Project.ChessEngine
{
    public class ContinueTrigger : MonoBehaviour
    {
        public GameObject thisGameObject;
        public GameObject gameController;
        public GameObject MulitplayerGameController;
        public GameObject portCullis;
        public GameObject Menu;
        public MenuUiController MenuUiController;
        public bool isWhite = false;
        public bool isBlack = false;
        bool istrue;
        private GameController gc;
        bool gameStartedAlready = false;

        // Start is called before the first frame update
        void Start()
        {

          
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ResetPlayers()
        {
            isWhite = false;
            isBlack = false;
        }



        public void VrButttonActivationWhite()
        {
            gameController.SetActive(true);
            isWhite = true;
            MenuUiController.StartClickedWhite();
            StartCoroutine(Waiter());
        }

        public void VrButttonActivationBlack()
        {
            isBlack = true;
            gameController.SetActive(true);
            MenuUiController.StartClickedBlack();
            //StartCoroutine(Waiter());
        }




        public IEnumerator Waiter()
        {
            yield return new WaitForSeconds(1);
            // Menu.SetActive(false);
        }

        public IEnumerator WaiterWhiteStart()
        {
            yield return new WaitForSeconds(4);
            VrButttonActivationBlack();
        }

        public IEnumerator WaiterWhite()
        {
            yield return new WaitForSeconds(2);
            MenuUiController.StartClickedWhite();
        }
    }
}