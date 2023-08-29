using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreAnimation : MonoBehaviour
{
    private TextMeshProUGUI textmesh;
    public float time = 1;
    public Vector3 animTarget = Vector2.up * 100;

    private void Awake()
    {
        textmesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        textmesh.color = Color.white;
        transform.DOMoveY(transform.position.y + animTarget.y, time).OnComplete(() => gameObject.SetActive(false));
        textmesh.DOColor(new Color(1, 1, 1, 0), time);
    }
}
