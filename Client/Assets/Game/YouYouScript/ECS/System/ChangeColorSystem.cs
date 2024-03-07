using Entitas;
using Entitas.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ECS.System
{
    public class ChangeColorSystem : ReactiveSystem<GameEntity>
    {
        /// <param name="contexts"></param>
        public ChangeColorSystem(IContext<GameEntity> context) : base(context)
        {
        }

        protected override void Execute(List<GameEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                GameEntity entity = entities[i];
                if (entity == null) continue;
                entity.color.mat.color = entity.isPlayer ? entity.color.PlayerColor : entity.color.MonsterColor;
            }
        }

        protected override bool Filter(GameEntity entity)
        {
            return entity.hasColor;
        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
        {
            return context.CreateCollector(GameMatcher.AllOf(GameMatcher.Color).AnyOf(GameMatcher.Player, GameMatcher.Monster));
        }
    }
}