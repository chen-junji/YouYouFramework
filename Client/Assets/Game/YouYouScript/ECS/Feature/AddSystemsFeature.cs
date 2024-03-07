using Assets.Scripts.ECS.System;
using Entitas;

namespace Assets.Scripts.ECS
{
    /// <summary>
    /// 用于添加需要的系统 System
    /// </summary>
    public class AddSystemsFeature : Feature
    {
        public AddSystemsFeature(Contexts contexts) : base("AddSystemsFeature")
        {
            // 添加系统
            Add(new InputSystem(contexts));
            Add(new CreateActorSystem(contexts));
            Add(new ActorMoveSystem(contexts.game));
            Add(new ChangeColorSystem(contexts.game));
        }
    }
}
