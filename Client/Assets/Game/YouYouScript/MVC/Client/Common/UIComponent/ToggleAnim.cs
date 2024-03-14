using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[DisallowMultipleComponent]
[RequireComponent(typeof(Toggle))]
public class ToggleAnim : MonoBehaviour
{
    [SerializeField] private bool IsOffPlay;

    private Toggle m_Toggle;
    private float BegScale;

    void Start()
    {
        BegScale = transform.localScale.x;

        m_Toggle = GetComponent<Toggle>();
        m_Toggle.onValueChanged.AddListener((isOn) =>
        {
            if (IsOffPlay || isOn) transform.DOScale(BegScale * 0.9f, 0.05f).SetUpdate(true).OnComplete(() => transform.DOScale(BegScale * 1.1f, 0.05f).SetUpdate(true).OnComplete(() => transform.DOScale(BegScale, 0.05f).SetUpdate(true)));
        });
    }
}