using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LabelManager : MonoBehaviour
{
    private static LabelManager instance;
    public static LabelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LabelManager>();
            }
            return instance;
        }
    }

    private const float RADIUS = 12f;
    private const float LONG_LINE_FACTOR = 6f;
    // UI
    public Button btnLabel;
    private bool isShowingLabel;
    public bool IsShowingLabel
    {
        get
        {
            return isShowingLabel;
        }
        set
        {
            isShowingLabel = value;
            btnLabel.GetComponent<Image>().sprite = isShowingLabel ? Resources.Load<Sprite>(PathConfig.LABEL_CLICKED_IMAGE) : Resources.Load<Sprite>(PathConfig.LABEL_UNCLICK_IMAGE);
        }
    }

    public void HandleLabelView(bool currentLabelStatus)
    {
        IsShowingLabel = currentLabelStatus;
        if (IsShowingLabel)
        {
            btnLabel.interactable = false;
            CreateLabel();
            btnLabel.interactable = true;
        }
        else
        {
            btnLabel.interactable = false;
            ClearLabel();
            btnLabel.interactable = true;
        }
    }

    public bool CheckAvailableLabel(GameObject obj)
    {
        if (StaticLesson.ListLabel.Length <= 0)
            return false;

        string levelObject = Helper.GetLevelObjectInLevelParent(obj);
        foreach (Label item in StaticLesson.ListLabel)
        {
            if (item.level == levelObject)
                return true;
        }
        return false;
    }

    public void CreateLabel()
    {
        ClearLabel();

        string levelObject = "";
        int subLevel = 0;
        GameObject subObject = null;
        Vector3 point;

        levelObject = Helper.GetLevelObjectInLevelParent(ObjectManager.Instance.CurrentObject);
        foreach (Label itemInforLabel in StaticLesson.ListLabel)
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
                point = new Vector3(itemInforLabel.coordinates.x, itemInforLabel.coordinates.y, itemInforLabel.coordinates.z);
                SetLabel(point, subObject, itemInforLabel.labelName);
            }
        }
    }

    public void ClearLabel()
    {
        foreach (LabelObjectInfo item in TagHandler.Instance.addedTags)
        {
            Destroy(item.point);
        }
        TagHandler.Instance.DeleteLabel();
    }

    public void SetLabel(Vector3 hitPoint, GameObject parent, string text)
    {
        GameObject point = Instantiate(Resources.Load(PathConfig.MODEL_POINT) as GameObject);
        point.tag = TagConfig.LABEL_TAG;
        point.transform.SetParent(parent.transform, false);
        point.transform.localPosition = hitPoint;
        point.transform.localScale = ObjectManager.Instance.OriginScaleLabel / (ObjectManager.Instance.OriginObject.transform.localScale.x / ObjectManager.Instance.OriginScale.x);

        LineRenderer lineRenderer;
        if(point.GetComponent<LineRenderer>() == null){
            lineRenderer = point.AddComponent<LineRenderer>();
        }else{
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
        TagHandler.Instance.AddLabel(labelObjectInfo);
    }

    public Vector3 CirclePosition(GameObject obj, float radius, Vector3 point)
    {
        Vector3 v = point - obj.transform.position;
        v = (v.normalized * radius) + obj.transform.position;
        return v;
    }
}