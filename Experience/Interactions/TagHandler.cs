using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TagHandler : MonoBehaviour
{
    private static TagHandler instance;
    public static TagHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TagHandler>();
            }
            return instance;
        }
    }

    public List<LabelObjectInfo> addedTags = new List<LabelObjectInfo>();

   void Update()
    {
        if (LabelManager.Instance.IsShowingLabel)
            OnMoveLabel();
    }

    public void AddLabel(LabelObjectInfo labelObjectInfor)
    {
        addedTags.Add(labelObjectInfor);
    }

    public void DeleteLabel()
    {
        addedTags.Clear();
    }

    public void OnMoveLabel()
    {
        foreach (LabelObjectInfo item in addedTags)
        {
            if (item.point != null)
            {
                DenoteLabel(item);
                MoveLabel(item);
            }
        }
    }

    public void DenoteLabel(LabelObjectInfo labelObjectInfo)
    {
        if (labelObjectInfo.point.transform.position.z > 0f)
            labelObjectInfo.point.SetActive(false);
        else
            labelObjectInfo.point.SetActive(true);

        if (labelObjectInfo.point.transform.GetChild(0).GetChild(labelObjectInfo.indexSideDisplay).position.x > 0f)
        {
            labelObjectInfo.indexSideDisplay = 1;      
            labelObjectInfo.point.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
            labelObjectInfo.point.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = labelObjectInfo.labelName;
            labelObjectInfo.point.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);          
        }
        else
        {
            labelObjectInfo.indexSideDisplay = 0;
            labelObjectInfo.point.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            labelObjectInfo.point.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = labelObjectInfo.labelName;
            labelObjectInfo.point.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
    }

    public void MoveLabel(LabelObjectInfo labelObjectInfo)
    {
        labelObjectInfo.point.transform.GetChild(0).GetChild(labelObjectInfo.indexSideDisplay).transform.LookAt(
                labelObjectInfo.point.transform.GetChild(0).GetChild(labelObjectInfo.indexSideDisplay).position + Camera.main.transform.rotation * Vector3.forward, 
                Camera.main.transform.rotation * Vector3.up);
    }

}
