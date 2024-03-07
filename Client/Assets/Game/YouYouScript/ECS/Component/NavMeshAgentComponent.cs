using Entitas;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.ECS.Component
{
    [Game]
    public class NavMeshAgentComponent : IComponent
    {
        public NavMeshAgent Value;
    }
}