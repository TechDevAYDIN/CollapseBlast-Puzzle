using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TDA.BlastTest
{
    public class CameraController : MonoBehaviour
    {
        public GameField field;
        [SerializeField] private RectTransform rect;
        public Transform pivot;


        // Start is called before the first frame update
        void Start()
        {
            transform.position = new Vector3(-1000, transform.position.y, -10);
            StartCoroutine(FindSize());
        }

        IEnumerator FindSize()
        {
            var camera = GetComponent<Camera>();
            yield return new WaitWhile(() => field.worldRect == Rect.zero);
            var fieldRect = field.worldRect;
            while (true)
            {
                var centerRect = GetWorldRect(rect, rect.lossyScale);
                if (fieldRect.width < centerRect.width && fieldRect.height < centerRect.height)
                    camera.orthographicSize -= Time.deltaTime * 10;
                else
                    camera.orthographicSize += Time.deltaTime * 10;
                if ((Mathf.Abs(fieldRect.width - centerRect.width) < 0.1f && fieldRect.height < centerRect.height) || (Mathf.Abs(fieldRect.height - centerRect.height) < 0.1f &&
                                                                                                                       fieldRect.width < centerRect.width))
                    break;
                yield return new FixedUpdate();
            }
            var rectPosition = (Vector3)((Vector2)rect.position - field.GetCenter());
            var targetPos = camera.transform.position - rectPosition;
            transform.DOMove(targetPos, 1);
        }
        private Rect GetWorldRect(RectTransform rt, Vector2 scale)
        {
            // Convert the rectangle to world corners and grab the top left
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 topLeft = corners[0];

            // Rescale the size appropriately based on the current Canvas scale
            Vector2 scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);

            return new Rect(topLeft, scaledSize);
        }
    }
}