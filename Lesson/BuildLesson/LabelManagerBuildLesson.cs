using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;
using EasyUI.Toast;
using System.Threading.Tasks;

namespace BuildLesson 
{
    public class LabelManagerBuildLesson : MonoBehaviour
    {
        private static LabelManagerBuildLesson instance; 
        public static LabelManagerBuildLesson Instance
        {
            get 
            {
                if (instance == null)
                {
                    // centerPosition = Helper.CalculateCentroid(ObjectManager.Instance.OriginObject);
                    instance = FindObjectOfType<LabelManagerBuildLesson>(); 
                }
                return instance; 
            }
        }
        private int calculatedSize = 10;
        public GameObject UIComponent;

        const float RADIUS = 12f;
        public GameObject btnLabel;
        
        private List<Vector3> pointPositions = new List<Vector3>();
        public List<GameObject> listLabelObjects = new List<GameObject>(); 
        public List<GameObject> listLabelObjectsOnEditMode = new List<GameObject>(); 

        private bool isLabelOnEdit = false;
        private bool isShowingLabel = true;

        public bool IsLabelOnEdit { get; set;}
        
        public bool IsShowingLabel 
        {    
            get
            {
                return isShowingLabel;
            }
            set
            {
                Debug.Log("LabelManagerBuildLesson isShowingLabel call"); 
                isShowingLabel = value; 
                btnLabel.GetComponent<Image>().sprite = !isShowingLabel ? Resources.Load<Sprite>(PathConfig.LABEL_UNCLICK_IMAGE) : Resources.Load<Sprite>(PathConfig.LABEL_CLICKED_IMAGE);
            }
        }

        void Start()
        {
            InitUI();
        }

        void InitUI()
        {
            btnLabel = GameObject.Find("BtnLabel");
        }

        public void Update()
        {
            pointPositions.Add(transform.position);
        }
        
        private static Vector3 centerPosition;

        public async Task SaveCoordinate(PostModelLabel postModelLabel)
        {
            try
            {
                APIResponse<DataCoordinate> coordinateResponse = await UnityHttpClient.CallAPI<DataCoordinate>(APIUrlConfig.POST_CREATE_MODEL_LABEL, UnityWebRequest.kHttpVerbPOST, postModelLabel);
                if (coordinateResponse.code == APIUrlConfig.SUCCESS_RESPONSE_CODE)
                {
                    Debug.Log("save coordinate sucessfully: " + coordinateResponse.data.labelId);
                    // TagHandler.Instance.AddLabelId(coordinateResponse.data.labelId);
                }
            }
            catch (Exception e)
            {   
                Debug.Log("save coordinate fail: " + e.Message);
            }
        }

        public void DisplayLabel2D(Vector3 position, GameObject destroyObj, InforPointByHit inforPointByHit)
        {
            // Create label 2D
            GameObject label2D = Instantiate(Resources.Load(PathConfig.MODEL_TAG_CONFIG) as GameObject);
            label2D.tag = "Tag2D";
            label2D.transform.SetParent(UIComponent.transform, false);
            label2D.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
            label2D.transform.position = position;
            label2D.transform.GetChild(0).gameObject.GetComponent<InputField>().onValueChanged.AddListener(ValidateCreatedLabel);
            label2D.transform.GetChild(0).gameObject.GetComponent<InputField>().ActivateInputField();
            label2D.transform.GetChild(0).gameObject.GetComponent<InputField>().Select();
            label2D.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
            label2D.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() => OnCreatedLabel(label2D, inforPointByHit));
            label2D.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => CancelCreatedLabel(label2D));
            Destroy(destroyObj);  

            void ValidateCreatedLabel(string data)
            {
                IsValidCreatedLabel(data);
            }

            bool IsValidCreatedLabel(string data)
            {
                if (string.IsNullOrEmpty(data))
                {
                    Debug.Log("Empty data: ");
                    label2D.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
                    return false;
                }
                else
                {
                    label2D.transform.GetChild(1).gameObject.GetComponent<Button>().interactable = true;
                    return true;
                }  
            }      
        }

        /// <summary>
        /// Author: quyennt57
        /// Purpose: Create label
        /// </summary>
        /// <param name="hitPoint">Position create label on object</param>
        /// <param name="parent">Point is belong to subobject ~ parent</param>
        /// <param name="text">Name label</param>

        void CreateLabel(Vector3 hitPoint, GameObject parent, string text)
        {
            GameObject point = Instantiate(Resources.Load(PathConfig.MODEL_POINT) as GameObject, hitPoint, Quaternion.identity);
            point.transform.SetParent(parent.transform, false);
            point.transform.localScale = ObjectManager.Instance.OriginScaleLabel / (ObjectManager.Instance.OriginObject.transform.localScale.x / ObjectManager.Instance.OriginScale.x);
            
            LineRenderer lineRenderer;
            if(point.GetComponent<LineRenderer>() == null)
            {
                lineRenderer = point.AddComponent<LineRenderer>();
            }
            else
            {
                lineRenderer = point.GetComponent<LineRenderer>();
            }
            lineRenderer.SetVertexCount(2);
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetWidth(0.01f, 0.01f);
            lineRenderer.material.color = Color.black;

            Vector3 targetPoint = CirclePosition(ObjectManager.Instance.OriginObject, RADIUS, hitPoint); 
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, targetPoint);

            GameObject labelObject = Instantiate(Resources.Load(PathConfig.MODEL_TAG_LABEL) as GameObject);
            labelObject.tag = TagConfig.LABEL_TAG;
            labelObject.transform.SetParent(point.transform, false);
            labelObject.transform.localPosition = targetPoint;

            int indexShowLabel = 0;
            if (hitPoint.x >= 0)
                indexShowLabel = 1;
            else
                indexShowLabel = 0;
            labelObject.transform.GetChild(indexShowLabel).gameObject.SetActive(true);
            labelObject.transform.GetChild(indexShowLabel).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = text;

            LabelObjectInfo labelObjectInfo = new LabelObjectInfo();
            labelObjectInfo.point = point;
            labelObjectInfo.indexSideDisplay = indexShowLabel;
            labelObjectInfo.labelName = text;
            labelObjectInfo.level = Helper.GetLevelObjectInLevelParent(ObjectManager.Instance.CurrentObject);
            labelObjectInfo.subLevel = Helper.GetLevelObjectInLevelParent(parent);
            TagHandler.Instance.AddLabel(labelObjectInfo);
        }

        /// <summary>
        /// Author: quyennt57
        /// Purpose: Create point second of line
        /// </summary>
        /// <param name="obj">Current object</param>
        /// <param name="radius">Distance from positon current object to target point</param>
        /// <param name="point">point on object</param>
        public Vector3 CirclePosition(GameObject obj, float radius, Vector3 point)
        {
            Vector3 v = point - obj.transform.position;
            v = (v.normalized * radius) + obj.transform.position;
            return v;
        }

        void OnCreatedLabel(GameObject destroyedObj, InforPointByHit inforPointByHit)
        {
            PostModelLabel postModelLabel = new PostModelLabel();
            postModelLabel.lessonId = SaveLesson.lessonId;
            postModelLabel.modelId = SaveLesson.modelId;
            postModelLabel.labelName = destroyedObj.transform.GetChild(0).gameObject.GetComponent<InputField>().text;
            postModelLabel.coordinates = Coordinate.InitCoordinate(inforPointByHit.hitPoint);
            postModelLabel.level = Helper.GetLevelObjectInLevelParent(ObjectManager.Instance.CurrentObject);
            postModelLabel.subLevel = Helper.GetLevelObjectInLevelParent(inforPointByHit.ObjectByHit);

            LabelManagerBuildLesson.Instance.SaveCoordinate(postModelLabel);
            CreateLabel(inforPointByHit.hitPoint, inforPointByHit.ObjectByHit, destroyedObj.transform.GetChild(0).gameObject.GetComponent<InputField>().text);
            LabelManagerBuildLesson.Instance.IsLabelOnEdit = !LabelManagerBuildLesson.Instance.IsLabelOnEdit;    

            Destroy(destroyedObj);
        }

        void CancelCreatedLabel(GameObject destroyObj)
        {
            Destroy(destroyObj);
        }

        public void HandleLabelView(bool currentLabelStatus)
        {
            IsShowingLabel = currentLabelStatus;
            if (IsShowingLabel)
            {
                btnLabel.GetComponent<Button>().interactable = false;
                CreateLabel();
                btnLabel.GetComponent<Button>().interactable = true;
            }
            else
            {
                btnLabel.GetComponent<Button>().interactable = false;
                ClearLabel();
                btnLabel.GetComponent<Button>().interactable = true;
            }
        }

        public void CreateLabel()
        {
            ClearLabel();

            string levelObject = "";
            int subLevel = 0;
            GameObject subObject = null;
            Vector3 point;

            levelObject = Helper.GetLevelObjectInLevelParent(ObjectManager.Instance.CurrentObject);
            foreach (LabelObjectInfo itemInforLabel in TagHandler.Instance.listLabelObjects)
            {
                if (itemInforLabel.level == levelObject)
                {
                    if (itemInforLabel.level == itemInforLabel.subLevel)
                    {
                        subObject = ObjectManager.Instance.CurrentObject;
                    }
                    else 
                    {
                        subLevel = (int)char.GetNumericValue(itemInforLabel.subLevel[itemInforLabel.subLevel.Length - 1]); 
                        subObject = ObjectManager.Instance.CurrentObject.transform.GetChild(subLevel).gameObject;
                    }
                    CreateLabel(itemInforLabel.point.transform.localPosition, subObject, itemInforLabel.labelName);
                }
            }
        }

        public void ClearLabel()
        {
            foreach (LabelObjectInfo item in TagHandler.Instance.listLabelObjects)
            {
                Destroy(item.point);
            }
            TagHandler.Instance.DeleteLabel();
        }
        

        // public void updateCenterPosition()
        // {
        //     Debug.Log("After init object: " + ObjectManager.Instance.OriginObject);
        //     centerPosition = Helper.CalculateCentroid(ObjectManager.Instance.OriginObject);
        // }

        // public Bounds GetParentBound(GameObject parentObject, Vector3 center)
        // {
        //     foreach (Transform child in parentObject.transform)
        //     {
        //         center += child.gameObject.GetComponent<Renderer>().bounds.center;
        //     }
        //     center /= parentObject.transform.childCount;
        //     Bounds bounds = new Bounds(center, Vector3.zero);
        //     foreach(Transform child in parentObject.transform)
        //     {
        //         bounds.Encapsulate(child.gameObject.GetComponent<Renderer>().bounds);
        //     }
        //     return bounds;
        // }

        // public string getIndexGivenGameObject(GameObject rootObject, GameObject targetObject)
        // {
        //     var result = new System.Text.StringBuilder();
        //     while(targetObject != rootObject)
        //     {
        //         result.Insert(0, targetObject.transform.GetSiblingIndex().ToString());
        //         result.Insert(0, "-");
        //         targetObject = targetObject.transform.parent.gameObject;
        //     }
        //     result.Insert(0, "0");
        //     return result.ToString();
        // }

        // public void HandleLabelView(bool currentLabelStatus) 
        // {
        //     IsShowingLabel = currentLabelStatus;
        //     ShowHideLabels(IsShowingLabel);
        //     TagHandler.Instance.ShowHideTags(IsShowingLabel);
        // }

        // private void ShowHideLabels(bool isShowing)
        // {
        //     foreach(GameObject label in listLabelObjects)
        //     {
        //         label.SetActive(isShowingLabel);
        //     }    
        // }
    }
}
