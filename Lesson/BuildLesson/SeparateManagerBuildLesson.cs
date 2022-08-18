using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuildLesson
{
    public class SeparateManagerBuildLesson : MonoBehaviour
    {
        private static SeparateManagerBuildLesson instance;
        public static SeparateManagerBuildLesson Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<SeparateManagerBuildLesson>();
                return instance;
            }
        }
        private const float RADIUS = 8f;
        // public float DistanceFactor { get; set; }
        private const float DISTANCE_FACTOR = 0.00526488976f;

        // variable
        private int childCount;
        private Vector3 centerPosition;
        private Vector3 targetPosition;
        private Vector3 positionCurrentObject;
        private Vector3 centerPosChildObject;
        private float angle;
        public Button btnSeparate;
        private bool isSeparating;
        public bool IsSeparating 
        {
            get
            {
                return isSeparating;
            }
            set
            {
                isSeparating = value;
                btnSeparate.GetComponent<Image>().sprite = isSeparating ? Resources.Load<Sprite>(PathConfig.SEPARATE_CLICKED_IMAGE) : Resources.Load<Sprite>(PathConfig.SEPARATE_UNCLICK_IMAGE);
            }
        }

        public void HandleSeparate(bool isSeparating)
        {
            IsSeparating = isSeparating;
            if (IsSeparating)
            {
                btnSeparate.interactable = false;
                SeparateOrganModel();
                btnSeparate.interactable = true;
            }
            else
            {
                btnSeparate.interactable = false;
                BackToPositionOrgan();
                btnSeparate.interactable = true;
            }
        }

        public void SeparateOrganModel()
        {
            childCount = ObjectManager.Instance.CurrentObject.transform.childCount;
            if (childCount <= 0) return;
            centerPosition = CalculateCentroid();

            for (int i = 0; i < childCount; i++)
            {
                targetPosition = ComputeTargetPosition(centerPosition, ObjectManager.Instance.ListchildrenOfOriginPosition[i]);
                StartCoroutine(MoveObjectWithLocalPosition(ObjectManager.Instance.CurrentObject.transform.GetChild(i).gameObject, targetPosition));
            }
        }

        private Vector3 CalculateCentroid()
        {
            Vector3 centroid = new Vector3(0, 0, 0);

            foreach (Vector3 localPosition in ObjectManager.Instance.ListchildrenOfOriginPosition)
            {
                centroid += localPosition;
            }
            centroid /= ObjectManager.Instance.ListchildrenOfOriginPosition.Count;
            return centroid;
        }

        public Vector3 ComputeTargetPosition(Vector3 origin, Vector3 target)
        {
            Vector3 dir = target - origin;
            return dir.normalized * DISTANCE_FACTOR;
        }

        public IEnumerator MoveObjectWithLocalPosition(GameObject moveObject, Vector3 targetPosition)
        {
            float timeSinceStarted = 0f;
            while (true)
            {
                timeSinceStarted += Time.deltaTime;
                moveObject.transform.localPosition = Vector3.Lerp(moveObject.transform.localPosition, targetPosition, timeSinceStarted);
                if (moveObject.transform.localPosition == targetPosition)
                {
                    yield break;
                }
                yield return null;
            }
        }

        public void BackToPositionOrgan()
        {
            // if (ObjectManager.Instance.ListchildrenOfOriginPosition.Count < 1)	        
            // {
            //     return;
            // }
            int childCount = ObjectManager.Instance.CurrentObject.transform.childCount;
            if (childCount < 0)
            {
                return;
            }
            for (int i = 0; i < childCount; i++)
            {
                if (ObjectManager.Instance.CurrentObject.transform.GetChild(i).gameObject.tag != TagConfig.LABEL_TAG)
                {
                    targetPosition = ObjectManager.Instance.ListchildrenOfOriginPosition[i];	
                    StartCoroutine(MoveObjectWithLocalPosition(ObjectManager.Instance.CurrentObject.transform.GetChild(i).gameObject, targetPosition));
                }
            }
        }
    }
}
