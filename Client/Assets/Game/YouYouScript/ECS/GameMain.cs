using Assets.Scripts.ECS;
using Entitas;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    //缓存所有系统
    private Systems m_Systems;

    void Start()
    {
        var context = Contexts.sharedInstance;

        m_Systems = new Feature("System").Add(new AddSystemsFeature(context));

        m_Systems.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        m_Systems.Execute();
    }

    private void LateUpdate()
    {
        m_Systems.Cleanup();
    }
}
