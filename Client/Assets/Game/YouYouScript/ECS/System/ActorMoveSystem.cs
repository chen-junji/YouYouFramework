using Entitas;
using Entitas.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ECS.System
{
    public class ActorMoveSystem : ReactiveSystem<GameEntity>
    {
        private readonly GameContext mGameContext;

        private IGroup<GameEntity> PlayerGroup;
        private IGroup<GameEntity> MonsterGroup;

        public ActorMoveSystem(IContext<GameEntity> context) : base(context)
        {
        }

        protected override void Execute(List<GameEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                GameEntity entity = entities[i];
                if (entity == null) continue;
                entity.navMeshAgent.Value.SetDestination(entity.destination.Value);
            }
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasNavMeshAgent
                && entity.hasDestination;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.AllOf(GameMatcher.Destination));
        }

    }
}