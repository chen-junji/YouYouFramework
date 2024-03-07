using Entitas;
using Entitas.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.ECS.System
{
    public class CreateActorSystem : IInitializeSystem
    {
        private readonly GameContext mGameContext;

        /// <param name="contexts"></param>
        public CreateActorSystem(Contexts contexts)
        {
            mGameContext = contexts.game;
        }

        public void Initialize()
        {
            Transform PoolRoot = GameObject.Find("PoolRoot").transform;

            //创建玩家
            for (int i = 0; i < 10; i++)
            {
                GameEntity entity = mGameContext.CreateEntity();
                entity.isPlayer = true;
                InitActor(entity, PoolRoot, i);
            }

            //创建怪物
            for (int i = 0; i < 10; i++)
            {
                GameEntity entity = mGameContext.CreateEntity();
                entity.isMonster = true;
                InitActor(entity, PoolRoot, i);
            }
        }

        private void InitActor(GameEntity entity, Transform parent, int index)
        {
            string gameName = entity.isPlayer ? "Player_" + index : "Monster_" + index;

            GameObject game = new GameObject(gameName);
            GameObject model = CreateModel(entity);
            model.transform.parent = game.transform;
            model.transform.localPosition = new Vector3(0, 1, 0);
            game.transform.parent = parent;
            game.transform.localPosition = Vector3.zero;
            entity.AddGameObject(game, model);
            game.Link(entity);

            entity.AddDestination(game.transform.position);
            NavMeshAgent nav = game.AddComponent<NavMeshAgent>();
            nav.speed = 50;
            nav.angularSpeed = 1000;
            nav.acceleration = 1000;
            nav.stoppingDistance = 8;
            entity.AddNavMeshAgent(nav);
        }

        private GameObject CreateModel(GameEntity entity)
        {
            GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Capsule.prefab");
            GameObject capsule = GameObject.Instantiate(obj);
            MeshRenderer meshRenderer = capsule.GetComponent<MeshRenderer>();
            entity.AddColor(meshRenderer.material, Color.red, Color.blue);
            return capsule;
        }
    }
}