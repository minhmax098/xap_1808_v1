using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems; 

namespace CreateLesson
{
    public class InteractionUI : MonoBehaviour
    {
        private GameObject backToBack;
        private GameObject cancelBtn;
        private static InteractionUI instance; 
        public static InteractionUI Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<InteractionUI>();
                }
                return instance; 
            }
        }
        void Start()
        {
            InitUI(); 
            SetActions(); 
        }
        void InitUI()
        {
            backToBack = GameObject.Find("BackBtn"); 
            cancelBtn = GameObject.Find("CancelBtn");
        }
        void SetActions()
        {
            backToBack.GetComponent<Button>().onClick.AddListener(BackTo3DStore);
            cancelBtn.GetComponent<Button>().onClick.AddListener(CancelButtonToBack3DStore);
        }
        void BackTo3DStore()
        {
            if(ScenePrevious.scenePrevious == SceneConfig.interactiveModel)
            {
                BackOrLeaveApp.Instance.BackToPreviousScene(SceneManager.GetActiveScene().name);
                SceneManager.LoadScene(SceneConfig.interactiveModel);
            }
            else
            {
                if(ScenePrevious.scenePrevious == SceneConfig.storeModel)
                {
                    BackOrLeaveApp.Instance.BackToPreviousScene(SceneManager.GetActiveScene().name);
                    SceneManager.LoadScene(SceneConfig.storeModel);
                }
            }
            
        }
        void CancelButtonToBack3DStore()
        {
            if(ScenePrevious.scenePrevious == SceneConfig.interactiveModel)
            {
                BackOrLeaveApp.Instance.BackToPreviousScene(SceneManager.GetActiveScene().name);
                SceneManager.LoadScene(SceneConfig.interactiveModel);
            }
            else
            {
                if(ScenePrevious.scenePrevious == SceneConfig.storeModel)
                {
                    BackOrLeaveApp.Instance.BackToPreviousScene(SceneManager.GetActiveScene().name);
                    SceneManager.LoadScene(SceneConfig.storeModel);
                }
            }
        }
    }
}
