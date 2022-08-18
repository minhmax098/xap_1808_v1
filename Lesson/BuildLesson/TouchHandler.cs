using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using System; 
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.Networking;
using System.Text;
using EasyUI.Toast;
using System.Threading.Tasks;

namespace BuildLesson
{
    public class TouchHandler : MonoBehaviour
    {
        private static TouchHandler instance; 
        public static TouchHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TouchHandler>();
                }
                return instance; 
            }
        }
        public static event Action onResetStatusFeature; 
        public static event Action<GameObject> onSelectChildObject; 

        public GameObject UIComponent;
        const float ROTATION_RATE = 0.08f;
        const float LONG_TOUCH_THRESHOLD = 0.8f; 
        const float ROTATION_SPEED = 0.5f; 
        float touchDuration = 0.0f; 
        Touch touch; 
        Touch touchZero; 
        Touch touchOne; 
        float originDelta; 
        Vector3 originScale;

        Vector3 originLabelScale = new Vector3(1f, 1f, 1f);
        Vector3 originLabelTagScale = new Vector3(7f, 1f, 1f);
        Vector3 originLineScale = new Vector3(1f, 1f, 1f); 
        Vector3 originScaleSelected;
        bool isMovingByLongTouch = false; 
        bool isLongTouch = false;
        float currentDelta;
        float scaleFactor;

        private GameObject currentSelectedObject; 
        private GameObject recentSelectedObject;
        private Vector3 centerPosition;
        private Vector3 mOffset; 
        private float mZCoord;
        private string currentSelectedLabelOrganName;
        private Vector3 hitPoint;
        private Vector2 hitPoint2D;

        // Panel DeleteTag
        public GameObject panelPopUpDeleteLabel;
        public Button btnExitPopupDeleteLabel; 
        public Button btnDeleteLabel; 
        public Button btnCancelDeleteLabel;

        // Panel AddActivities: addVideo, addAudio
        public GameObject panelAddActivities;
        public Button btnAddAudioLabel;
        public Button btnAddVideoLabel;
        public Button btnCancelAddActivities;

        private int calculatedSize = 10;
        GameObject labelEditObject;

        void Start()
        {
            InitUILabel();
        }

        void InitUILabel()
        {
            labelEditObject = Instantiate(Resources.Load(PathConfig.MODEL_TAG_EDIT) as GameObject);
            // Add functional to the buttons of labelEditObject
            labelEditObject.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(() => 
            {
                InputField inpField = labelEditObject.transform.GetChild(0).gameObject.GetComponent<InputField>();
                inpField.ActivateInputField();
                inpField.Select();
                inpField.onEndEdit.AddListener(delegate{OnEndEditLabel(inpField);});
            });
            // Button DeleteLabel
            labelEditObject.transform.GetChild(4).gameObject.GetComponent<Button>().onClick.AddListener(HandlerDeleteTag);
            btnDeleteLabel.onClick.AddListener(() => onClickYes(TagHandler.Instance.labelIds[TagHandler.Instance.currentEditingIdx])); 
            btnExitPopupDeleteLabel.onClick.AddListener(ExitPopupDeleteLabel);
            btnCancelDeleteLabel.onClick.AddListener(ExitPopupDeleteLabel);
            // Button AddLabel: addAudio, addVideo for Label
            labelEditObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(HandlerAddTag()));
            btnCancelAddActivities.onClick.AddListener(CancelAddActivities);
        }
        
        /// <summary>
        /// author: quyennt57
        /// aim: Detected touch on screen
        /// </summary>
        public void HandleTouchInteraction()    
        {
            if (ObjectManager.Instance.CurrentObject == null)
            {
                return;
            }
            if (Input.touchCount == 3)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved && Input.GetTouch(2).phase == TouchPhase.Moved)
                {
                    HandleSimultaneousThreeTouch(Input.GetTouch(1));
                }
            }
            else if (Input.touchCount == 2)
            {
                touchZero = Input.GetTouch(0);
                touchOne = Input.GetTouch(1);
                HandleSimultaneousTouch(touchZero, touchOne);
            }
            else if (Input.touchCount == 1)
            {
                touch = Input.GetTouch(0);
                if (touch.tapCount == 2)
                {
                    touch = Input.touches[0];
                    if (touch.phase == TouchPhase.Ended)
                    {
                        HandleDoupleTouch(touch);
                    }
                }
                else
                {
                    HandleSingleTouch(touch);
                }
            }
        }

        private void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began: 
                {
                    ResetLongTouch(); 
                    if (LabelManagerBuildLesson.Instance.IsLabelOnEdit)
                    {   
                        Debug.Log("Show normal label");
                        Debug.Log("Check: " + currentSelectedObject);
                        if (currentSelectedObject != null)
                        {
                            Debug.Log("message: ");
                            LabelManagerBuildLesson.Instance.IsLabelOnEdit = !LabelManagerBuildLesson.Instance.IsLabelOnEdit;
                            TagHandler.Instance.labelEditTag.SetActive(false);
                            TagHandler.Instance.ShowHideCurrentLabel(true);
                        }
                    }

                    if (!LabelManagerBuildLesson.Instance.IsLabelOnEdit)
                    {
                        Debug.Log("Hit: IsLabelOnEdit" + LabelManagerBuildLesson.Instance.IsLabelOnEdit);
                        // Construct a ray from the current touch coordinates
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit hit; 
                        if (Physics.Raycast(ray, out hit))
                        {
                            Debug.Log("Hit: " + hit.transform.gameObject.name);
                            if (hit.transform.gameObject.tag == TagConfig.labelModel)
                            {
                                Debug.Log("Hit the Normal label: ");
                                // Get the text inside the hit object 
                                string hitLabelText = hit.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text;
                                TagHandler.Instance.labelEditTag.transform.GetChild(0).GetComponent<InputField>().text = hitLabelText;
                                TagHandler.Instance.updateCurrentEditingIdx(hitLabelText);
                                TagHandler.Instance.labelEditTag.SetActive(true);
                                TagHandler.Instance.ShowHideCurrentLabel(false);
                                LabelManagerBuildLesson.Instance.IsLabelOnEdit = !LabelManagerBuildLesson.Instance.IsLabelOnEdit;
                            }
                        }
                    } 
                    break; 
                }
                case TouchPhase.Stationary: 
                {
                    if (!isLongTouch)
                    {
                        touchDuration += Time.deltaTime;
                        if (touchDuration > LONG_TOUCH_THRESHOLD)
                        {
                            isLongTouch = true;
                        }
                    }
                    break;
                }
                case TouchPhase.Moved:
                {
                    Rotate(touch);
                    break;
                }
                case TouchPhase.Ended: 
                {
                    if (isLongTouch)
                    {
                        DisplayInputFieldLabel();
                    }
                    ResetLongTouch(); 
                    break;
                }
                case TouchPhase.Canceled: 
                {
                    ResetLongTouch(); 
                    break;
                }
            }
        }

        private void Rotate(Touch touch)
        {
            ObjectManager.Instance.OriginObject.transform.Rotate(touch.deltaPosition.y * ROTATION_RATE, -touch.deltaPosition.x * ROTATION_RATE, 0, Space.World);
        }


        /// <summary>
        /// Author: quyennt57
        /// Purpose: Get hit point and display UI enter name label
        /// </summary>
        void DisplayInputFieldLabel()
        {
            // Get hit point and save data
            InforPointByHit inforPointByHit = Helper.GetObjectByTouch(touch.position);
            if (inforPointByHit == null)
                return;

            inforPointByHit.ObjectByHit = inforPointByHit.ObjectByHit != null ? inforPointByHit.ObjectByHit : ObjectManager.Instance.CurrentObject;
            inforPointByHit.hitPoint = inforPointByHit.ObjectByHit.transform.InverseTransformPoint(inforPointByHit.hitPoint);
            Debug.Log($"quyennt57 touch : {inforPointByHit.hitPoint} - {inforPointByHit.ObjectByHit}");
            // Add UI inputfield to enter name lable
            GameObject addLabelIcon = Instantiate(Resources.Load(PathConfig.MODEL_ADD_LABEL) as GameObject);
            Debug.Log("quyen " + addLabelIcon);
            addLabelIcon.transform.parent = UIComponent.transform;
            addLabelIcon.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
            addLabelIcon.transform.position = touch.position;
            addLabelIcon.transform.GetComponent<Button>().onClick.AddListener(() => LabelManagerBuildLesson.Instance.DisplayLabel2D(touch.position, addLabelIcon, inforPointByHit));
        }

        void ResetLongTouch()
        {
            touchDuration = 0f;
            isLongTouch = false;
        }

        private void Drag(Touch touch, GameObject obj)
        {
            if (obj != null)
            {
                obj.transform.position = Helper.GetTouchPositionAsWorldPoint(touch) + mOffset;
            }
        }

        private (GameObject, Vector3) GetChildOrganOnTouchByTag(Vector3 position)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit hit; 
            Debug.Log("Get Child: ");
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.transform.root.gameObject.tag == TagConfig.ORGAN_TAG)
                {
                    Debug.Log("Get Child later: ");
                    if (hit.collider.transform.parent == ObjectManager.Instance.CurrentObject.transform)
                    {
                        return (hit.collider.gameObject, hit.point);
                    }
                }
            }
            return (null, new Vector3(0f, 0f, 0f));
        }

        private void HandleDoupleTouch(Touch touch)
        {
            GameObject selectedObject = Helper.GetChildOrganOnTouchByTag2(touch.position);
            if (selectedObject == null || selectedObject == ObjectManager.Instance.OriginObject || ObjectManager.Instance.CurrentObject.transform.childCount < 1)
            {
                return;
            }
            onSelectChildObject?.Invoke(selectedObject);
        }

        void OnEndEditLabel(InputField input)
        {
            if (input.text.Length > 0) 
            {
                Debug.Log("Text has been entered: " + input.text);
                // Change the text inside the corresponding counter part
                TagHandler.Instance.addedTags[TagHandler.Instance.currentEditingIdx].transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMeshPro>().text = input.text;
                // Save the tag by calling API
            }
        }

        private void SetLabel(string name, Vector3 hitpoint, GameObject currentObject, GameObject parentObject, Vector3 rootPosition, GameObject label, GameObject editLabel)
        {
            Debug.Log("set label: ");
            GameObject sphere = label.transform.GetChild(2).gameObject;
            var spereRenderer = sphere.GetComponent<Renderer>(); 
            spereRenderer.material.SetColor("_Color", Color.red);

            Debug.Log("Current selected object: " + currentObject.name);
            Debug.Log("OriginScale: " + ObjectManager.Instance.OriginScale.x);
            Debug.Log("localScale: " + ObjectManager.Instance.OriginObject.transform.localScale.x);
            
            sphere.transform.localScale = new Vector3 (10f, 10f, 10f) * ObjectManager.Instance.OriginScale.x / ObjectManager.Instance.OriginObject.transform.localScale.x;
            sphere.transform.position = hitPoint; // Global variable

            GameObject line = label.transform.GetChild(0).gameObject; 
            GameObject labelName = label.transform.GetChild(1).gameObject;  
            labelName.transform.GetChild(0).GetComponent<TextMeshPro>().text = Helper.FormatString(name, calculatedSize);           
            Bounds parentBounds = Helper.GetParentBound(parentObject, rootPosition);
            Bounds objectBounds = currentObject.GetComponent<Renderer>().bounds;

            // Vector3 dir = hitPoint - rootPosition; 
            Vector3 dir = parentObject.transform.InverseTransformPoint(hitPoint) - parentObject.transform.InverseTransformPoint(rootPosition);    
            Debug.Log("Parent bounds magnitude : " + parentBounds.max.magnitude);
            Debug.Log("Originscale: " + ObjectManager.Instance.OriginScale.x);
            Debug.Log("Localscale X: " + ObjectManager.Instance.OriginObject.transform.localScale.x);
            labelName.transform.localPosition = parentBounds.max.magnitude * dir.normalized / ObjectManager.Instance.OriginScale.x;

            line.GetComponent<LineRenderer>().useWorldSpace = false;
            line.GetComponent<LineRenderer>().widthMultiplier = 0.25f * parentObject.transform.localScale.x;  // 0.2 -> 0.05 then 0.02 -> 0.005
            line.GetComponent<LineRenderer>().SetVertexCount(2);
            line.GetComponent<LineRenderer>().SetPosition(0, label.transform.InverseTransformPoint(hitPoint));
            line.GetComponent<LineRenderer>().SetPosition(1, label.transform.InverseTransformPoint(labelName.transform.position));
            line.GetComponent<LineRenderer>().SetColors(Color.black, Color.black);
            line.GetComponent<LineRenderer>().SetWidth(0.03f, 0.03f);

            // Set position and the text inside editLabel
            editLabel.transform.GetChild(0).GetComponent<InputField>().text = name;
            Vector2 endPoint = Camera.main.WorldToScreenPoint(labelName.transform.position);
            editLabel.transform.parent = UIComponent.transform;
            editLabel.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
            editLabel.transform.position = endPoint;
            // Update TagHandler, add normal Label into the TagHander 
            TagHandler.Instance.AddTag(label);
            TagHandler.Instance.updateCurrentEditingIdx(Helper.FormatString(name, calculatedSize));
            TagHandler.Instance.positionOriginLabel.Add(labelName.transform.localPosition);

            // Also update the counterpart
            if (TagHandler.Instance.labelEditTag == null) 
            {
                TagHandler.Instance.labelEditTag = editLabel;
            }  
            TagHandler.Instance.ShowHideCurrentLabel(false);
        }

        // API call to delete label by Id
        void onClickYes(int labelId)
        {
            Debug.Log("Delete label Id: " + labelId);
            DeleteLabel(labelId);
            TagHandler.Instance.deleteCurrentLabel();
            TagHandler.Instance.labelEditTag.SetActive(false);
            panelPopUpDeleteLabel.SetActive(false);
        }

        // Handle DeleteTag
        void HandlerDeleteTag()
        {
            panelPopUpDeleteLabel.SetActive(true);
        }

        void ExitPopupDeleteLabel()
        {
            panelPopUpDeleteLabel.SetActive(false);
        }

        // Handle AddTag: addAudio, addVideo
        IEnumerator HandlerAddTag()
        {   
            panelAddActivities.SetActive(true);
            btnAddAudioLabel.onClick.AddListener(AddAudioLabel);
            btnAddVideoLabel.onClick.AddListener(AddVideoLabel);
            Debug.Log("Handler Add Tag: ");
            yield return null;
        }
        void AddAudioLabel()
        {
            panelAddActivities.SetActive(false);
            ListItemsManager.Instance.panelAddAudio.SetActive(true);
            ListItemsManager.Instance.panelAddAudio.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = TagHandler.Instance.labelEditTag.transform.GetChild(0).GetComponent<InputField>().text;
        }
        void AddVideoLabel()
        {
            panelAddActivities.SetActive(false);
            ListItemsManager.Instance.panelAddVideo.SetActive(true);
            ListItemsManager.Instance.panelAddVideo.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = TagHandler.Instance.labelEditTag.transform.GetChild(0).GetComponent<InputField>().text;
        }
        void CancelAddActivities()
        {
            panelAddActivities.SetActive(false);
        }

        // Zoom in, zoom out
         /// <summary>
        /// author: quyennt57
        /// aim: Zoom object with label
        /// </summary>
        private void HandleSimultaneousTouch(Touch touchZero, Touch touchOne)
        {
            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                originDelta = Vector2.Distance(touchZero.position, touchOne.position);
                originScale = ObjectManager.Instance.OriginObject.transform.localScale;
            }
            else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
            {
                currentDelta = Vector2.Distance(touchZero.position, touchOne.position);
                scaleFactor = currentDelta / originDelta;
                ObjectManager.Instance.OriginObject.transform.localScale = originScale * scaleFactor;
                foreach (LabelObjectInfo item in TagHandler.Instance.listLabelObjects)
                {
                    item.point.transform.localScale = ObjectManager.Instance.OriginScaleLabel / (ObjectManager.Instance.OriginObject.transform.localScale.x / ObjectManager.Instance.OriginScale.x);
                }
            }
        }

        private void HandleSimultaneousThreeTouch(Touch touch)
        {
            Drag(touch, ObjectManager.Instance.OriginObject);
        }
        
        public async void DeleteLabel(int labelId)
        {
            try
            {
                DeleteLabelRequest deleteLabelRequest = new DeleteLabelRequest();
                string url = String.Format(APIUrlConfig.DELETE_LABEL, labelId);
                APIResponse<string> deleteLabelResponse = await UnityHttpClient.CallAPI<string>(url, UnityWebRequest.kHttpVerbDELETE, deleteLabelRequest);
                if (deleteLabelResponse.code == APIUrlConfig.SUCCESS_RESPONSE_CODE)
                {
                    Debug.Log("Delete label sucessful: ");
                }
            }
            catch (Exception e)
            {
                Debug.Log("Delete label failed: " + e.Message);
            }
        }
    }
}

