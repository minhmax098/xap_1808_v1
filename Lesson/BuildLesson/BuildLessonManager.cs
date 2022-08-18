using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using System.Reflection;
using System.Runtime.Versioning;
using EasyUI.Toast;

namespace BuildLesson
{
    public class BuildLessonManager : MonoBehaviour
    {
        public Button btnLabel;
        public Button btnSeparate;
        public Button btnXray;
        public Button btnAdd;
        public Animator toggleListItemAnimator;
        public GameObject record; 
        public GameObject saveRecord; 
        public GameObject addVideo; 
        public GameObject upload; 
        public GameObject addAudio; 
        private GameObject label2D;

        public Image processImage;
        public Text processText;
        public Text processTypeText;
        
        void Start()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            // ObjectManager.Instance.InitOriginalExperience();
            InitInteractions();
            InitEvents();
            LoadObjectModel();
        }

        void LoadObjectModel()
        {
            Debug.Log("MINH LOAD MODEL: " + APIUrlConfig.DOMAIN_SERVER + StaticLesson.ModelFile);
                        Debug.Log("minhlh lesson title " + StaticLesson.LessonTitle);

            ObjectManager.Instance.LoadObjectAtRunTime(APIUrlConfig.DOMAIN_SERVER + StaticLesson.ModelFile);
            StartCoroutine(TrackLoadingModel());
        }

        void InitProcessCircle(string type)
        {
            processTypeText.text = type;
            processText.text = "0%";
            processImage.fillAmount = 0;
        }

        IEnumerator TrackLoadingModel()
        {
            InitProcessCircle(ModelConfig.downloadProcessType);
            while(!ObjectManager.Instance.isFinishDownloading)
            {
                processImage.fillAmount = ObjectManager.Instance.downloadingModelProcess;    
                processText.text = $"{(processImage.fillAmount *100f):N0} %";
                yield return null;
            }
            if (ObjectManager.Instance.isDownloadingSuccess)
            {
                InitProcessCircle(ModelConfig.loadFromLocalProcessType);
                while(ObjectManager.Instance.loadingFromLocalProcess < 1)
                {
                    processImage.fillAmount = ObjectManager.Instance.loadingFromLocalProcess;    
                    processText.text = $"{(processImage.fillAmount *100f):N0} %";
                    yield return null;
                }
                LoadingEffectManager.Instance.ShowLoadingEffect(false);
            }
            else
            {
                Toast.ShowCommonToast(ModelConfig.failedToDownloadModel, APIUrlConfig.SERVER_ERROR_RESPONSE_CODE);
                LoadingEffectManager.Instance.ShowLoadingEffect(false);
            }
        }

        void Update()
        {
            // Check whether the pannel is opened 
            Debug.Log("Value of the animator: " + toggleListItemAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList));
            label2D = GameObject.FindWithTag("Tag2D");
            Debug.Log("Label 2d: " + label2D);
            if (!toggleListItemAnimator.GetBool(AnimatorConfig.isShowMeetingMemberList) && 
                !record.activeSelf && 
                !saveRecord.activeSelf && 
                !addVideo.activeSelf && 
                !upload.activeSelf && 
                !addAudio.activeSelf && 
                label2D == null)
            {
                TouchHandler.Instance.HandleTouchInteraction();
            }
            EnableFeature();
        }

        private void UpdateTagFromServer()
        {
            Debug.Log("LOADING LABELS FROM SERVER " + ObjectManager.Instance.OriginObject.name);
            
            TagHandler.Instance.GetTagFromServer(LessonManager.lessonId, ObjectManager.Instance.OriginObject);
        }

        void EnableFeature()
        {
            btnLabel.interactable = (ObjectManager.Instance.CurrentObject != null);
                                    // && LabelManagerBuildLesson.CheckAvailableLabel(ObjectManager.Instance.CurrentObject));
            btnXray.interactable = (ObjectManager.Instance.CurrentObject != null);
            btnSeparate.interactable = (ObjectManager.Instance.CurrentObject != null);
                                    // && ObjectManager.Instance.CheckObjectHaveChild(ObjectManager.Instance.CurrentObject));
        }

        void OnEnable()
        {
            TouchHandler.onSelectChildObject += OnSelectChildObject;
            TreeNodeManagerBuildLesson.onClickNodeTree += OnClickNodeTree;
            // ObjectManager.onResetObject += OnResetObject;
            // ObjectManager.onInitOrganSuccessfully += UpdateTagFromServer;
             ObjectManager.onLoadedObjectAtRuntime += OnLoadedObjectAtRuntime;
        }

        void OnDisable()
        {
            TouchHandler.onSelectChildObject -= OnSelectChildObject;
            TreeNodeManagerBuildLesson.onClickNodeTree -= OnClickNodeTree; 
            // ObjectManager.onResetObject -= OnResetObject;
            // ObjectManager.onInitOrganSuccessfully -= UpdateTagFromServer;
            ObjectManager.onLoadedObjectAtRuntime -= OnLoadedObjectAtRuntime;
        }

         void OnLoadedObjectAtRuntime()
        {
            Debug.Log($"minhlh17 OnLoadedObjectAtRuntime");
            ObjectManager.Instance.InitGameObject();
            LoadingEffectManager.Instance.ShowLoadingEffect(false);
            // InitUI();
        }

        void OnResetObject()
        {
            TreeNodeManagerBuildLesson.Instance.ClearAllNodeTree();
            XRayManager.Instance.HandleXRayView(XRayManager.Instance.IsMakingXRay);
            Helper.ResetStatusFeature();
        }

        void OnSelectChildObject(GameObject selectedObject)
        {
            OnResetStatusFeature();
            TreeNodeManagerBuildLesson.Instance.DisplaySelectedObject(selectedObject, ObjectManager.Instance.CurrentObject);
            ObjectManager.Instance.ChangeCurrentObject(selectedObject);
            TreeNodeManagerBuildLesson.Instance.CreateChildNodeUI(selectedObject.name);
        }

        void OnClickNodeTree(string nodeName)
        {
            OnResetStatusFeature();
            XRayManager.Instance.HandleXRayView(false);
            if (nodeName != ObjectManager.Instance.CurrentObject.name)
            {
                GameObject selectedObject = GameObject.Find(nodeName);
                TreeNodeManagerBuildLesson.Instance.DisplayAllChildSelectedObject(selectedObject);

                ObjectManager.Instance.ChangeCurrentObject(selectedObject);
                TreeNodeManagerBuildLesson.Instance.RemoveItem(nodeName);
            }
            ObjectManager.Instance.OriginObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        void InitInteractions()
        {
            XRayManager.Instance.IsMakingXRay = false;
            SeparateManagerBuildLesson.Instance.IsSeparating = false;
            LabelManagerBuildLesson.Instance.IsShowingLabel = false;
        }

        void InitEvents()
        {
            btnAdd.onClick.AddListener(ToggleMenuAdd); 
            btnLabel.onClick.AddListener(HandleLabelView);
            btnSeparate.onClick.AddListener(HandleSeparation); 
            btnXray.onClick.AddListener(HandleXRayView);
        }

        void ToggleMenuAdd()
        {
            PopUpBuildLessonManager.Instance.IsClickedAdd = !PopUpBuildLessonManager.Instance.IsClickedAdd;
            PopUpBuildLessonManager.Instance.ShowListAdd(PopUpBuildLessonManager.Instance.IsClickedAdd);
        }

        void HandleLabelView()
        {
            LabelManagerBuildLesson.Instance.IsShowingLabel = !LabelManagerBuildLesson.Instance.IsShowingLabel;
            LabelManagerBuildLesson.Instance.HandleLabelView(LabelManagerBuildLesson.Instance.IsShowingLabel);
        }

        void HandleSeparation()
        {
            SeparateManagerBuildLesson.Instance.IsSeparating = !SeparateManagerBuildLesson.Instance.IsSeparating;
            SeparateManagerBuildLesson.Instance.HandleSeparate(SeparateManagerBuildLesson.Instance.IsSeparating);
        }

        void HandleXRayView()
        {
            XRayManager.Instance.IsMakingXRay = !XRayManager.Instance.IsMakingXRay;
            XRayManager.Instance.HandleXRayView(XRayManager.Instance.IsMakingXRay);
        }

        void OnResetStatusFeature()
        {
            LabelManagerBuildLesson.Instance.IsShowingLabel = false;
            LabelManagerBuildLesson.Instance.HandleLabelView(LabelManagerBuildLesson.Instance.IsShowingLabel);

            SeparateManagerBuildLesson.Instance.IsSeparating = false;
            SeparateManagerBuildLesson.Instance.HandleSeparate(SeparateManagerBuildLesson.Instance.IsSeparating);
        }
    }
}
