using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.ChessEngine
{
    public class ChildDestoyer : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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

    }
}