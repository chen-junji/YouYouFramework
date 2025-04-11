#if !No_Reporter
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReporterGUI : MonoBehaviour
{

    Reporter reporter;
    Image ImageMask;

    private void OnDestroy()
    {
        ImageMask.enabled = false;
    }
    void Awake()
    {
        reporter = GetComponent<Reporter>();
        ImageMask = transform.Find("Canvas").GetComponent<Image>();
        ImageMask.enabled = true;
    }

    void OnGUI()
    {
        reporter.OnGUIDraw();
    }
}
#endif