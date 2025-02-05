using YouYouFramework;
using UnityEngine;
using UnityEngine.UI;

public partial class CircleForm : UIFormBase
{
    [SerializeField] private RectTransform m_Trans_Circle;
    [SerializeField] private GameObject panel;
    [SerializeField] private Text tips;

    private float delay;

    protected override void OnEnable()
    {
        base.OnEnable();
        delay = 0f;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        panel.SetActive(false);
    }

    internal void SetText(string tips)
    {
        this.tips.text = tips;
    }

    private void Update()
    {
        delay += Time.deltaTime;
        if (delay >= 1f && !panel.activeSelf)
        {
            panel.SetActive(true);
        }
        m_Trans_Circle.Rotate(0, 0, -5, Space.Self);
    }
}
