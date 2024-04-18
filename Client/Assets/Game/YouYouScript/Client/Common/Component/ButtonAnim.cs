using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonAnim : MonoBehaviour
{
    private float BegScale;
    private Button m_Button;

    void Start()
    {
        BegScale = transform.localScale.x;

        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(() =>
        {
            transform.DOScale(BegScale * 0.9f, 0.05f).SetUpdate(true).OnComplete(() =>
            {
                transform.DOScale(BegScale * 1.1f, 0.05f).SetUpdate(true).OnComplete(() =>
                {
                    transform.DOScale(BegScale, 0.05f).SetUpdate(true);
                });
            });
        });
    }
}