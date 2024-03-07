using Entitas;
using Entitas.Unity;
using UnityEngine;

namespace Assets.Scripts.ECS.System
{
    /// <summary>
    /// 初始化系统
    /// </summary>
    public class InputSystem : IExecuteSystem, IInitializeSystem
    {
        private readonly GameContext mGameContext;
        private IGroup<GameEntity> PlayerGroup;
        private IGroup<GameEntity> MonsterGroup;

        /// <summary>
        /// 构造函数，获取上下文
        /// </summary>
        /// <param name="contexts"></param>
        public InputSystem(Contexts contexts)
        {
            mGameContext = contexts.game;
        }

        public void Initialize()
        {
            PlayerGroup = mGameContext.GetGroup(GameMatcher.AllOf(GameMatcher.Player, GameMatcher.GameObject, GameMatcher.NavMeshAgent));
            MonsterGroup = mGameContext.GetGroup(GameMatcher.AllOf(GameMatcher.Monster, GameMatcher.GameObject, GameMatcher.NavMeshAgent));
        }

        public void Execute()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ActorMove(PlayerGroup);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                ActorMove(MonsterGroup);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeTeam(PlayerGroup, true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeTeam(MonsterGroup, false);
            }
        }

        private void ActorMove(IGroup<GameEntity> group)
        {
            if (group == null) return;
            GameEntity[] arr = group.GetEntities();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            for (int i = 0; i < arr.Length; i++)
            {
                GameEntity entity = arr[i];
                entity.ReplaceDestination(hit.point);
            }
        }

        private void ChangeTeam(IGroup<GameEntity> group, bool isPlayer)
        {
            if (group == null) return;

            int numb = UnityEngine.Random.Range(1, 3);
            GameEntity[] arr = group.GetEntities();
            numb = Mathf.Min(numb, arr.Length - 1);
            if (numb <= 0) return;

            for (int i = 0; i < numb; i++)
            {
                GameEntity entity = arr[i];
                if (entity == null) continue;
                if (isPlayer)
                {
                    entity.isPlayer = false;
                    entity.isMonster = true;
                }
                else
                {
                    entity.isMonster = false;
                    entity.isPlayer = true;
                }
            }
        }
    }
}