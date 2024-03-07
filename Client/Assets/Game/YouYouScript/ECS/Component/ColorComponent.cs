using Entitas;
using UnityEngine;

namespace Assets.Scripts.ECS.Component
{
    [Game]
    public class ColorComponent : IComponent
    {
        public Material mat;

        public Color PlayerColor;
        public Color MonsterColor;
    }
}
