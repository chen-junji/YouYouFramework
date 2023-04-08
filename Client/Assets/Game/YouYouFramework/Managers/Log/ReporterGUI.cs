#if !No_Reporter
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YouYou
{
    public class ReporterGUI : MonoBehaviour
    {

        Reporter reporter;
        Canvas ImageMask;

        private void OnDestroy()
        {
            ImageMask.enabled = false;
        }
        void Awake()
        {
            reporter = GetComponent<Reporter>();
            ImageMask = transform.Find("Canvas").GetComponent<Canvas>();
            ImageMask.enabled = true;
        }

        void OnGUI()
        {
            reporter.OnGUIDraw();
        }
    }
}

#endif