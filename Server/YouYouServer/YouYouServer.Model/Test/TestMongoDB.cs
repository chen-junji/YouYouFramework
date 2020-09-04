//using System;
//using YouYouServer.Common;
//using YouYouServer.Common.DBData;
//using YouYouServer.Core.Logger;
//using YouYouServer.Model.Logic.DBModels;

//namespace YouYouServer.Model.Test
//{
//    public class TestMongoDB
//    {
//        /// <summary>
//        /// 测试添加
//        /// </summary>
//        public static void TestAdd()
//        {
//            for (int i = 0; i < 8890; i++)
//            {
//                RoleEntity roleEntity = new RoleEntity();
//                roleEntity.YFId = DBModelMgr.UniqueIDGameServer.GetUniqueID((int)ConstDefine.CollectionType.Role);
//                roleEntity.NickName = "悠游" + new Random().Next(1, 8888);
//                roleEntity.Level = 1;
//                roleEntity.CreateTime = DateTime.UtcNow;
//                roleEntity.UpdateTime = DateTime.UtcNow;

//                roleEntity.TaskList.Add(new TaskEntity() { TaskId = 1, CurrStatus = 0 });
//                roleEntity.TaskList.Add(new TaskEntity() { TaskId = 2, CurrStatus = 1 });

//                roleEntity.SkillDic[1] = new SkillEntity() { SkillId = 1, CurrLevel = 2 };
//                roleEntity.SkillDic[2] = new SkillEntity() { SkillId = 2, CurrLevel = 1 };
//                roleEntity.SkillDic[3] = new SkillEntity() { SkillId = 3, CurrLevel = 1 };

//                DBModelMgr.RoleDBModel.Add(roleEntity);

//                LoggerMgr.Log(Core.LoggerLevel.Log, 0, "Add Role YFID={0}", roleEntity.YFId);
//            }

//            Console.WriteLine("添加完毕");
//        }

//        public static void TestSearch()
//        {
//            RoleEntity roleEntity = DBModelMgr.RoleDBModel.GetEntity(4);
//            if (roleEntity != null)
//            {
//                SkillEntity skillEntity = null;
//                if (roleEntity.SkillDic.TryGetValue(1, out skillEntity))
//                {
//                    Console.WriteLine("roleEntity=" + skillEntity.CurrLevel);
//                }
//                else
//                {
//                    Console.WriteLine("技能不存在=" + 1);
//                    roleEntity.SkillDic[1] = new SkillEntity() { SkillId = 1, CurrLevel = 1 };
//                    DBModelMgr.RoleDBModel.Update(roleEntity);
//                }
//            }
//        }

//        public static void TestUniqueID()
//        {
//            //UniqueIDGameServer uniqueIDGameServer = new UniqueIDGameServer();

//            //for (int i = 0; i < 82; i++)
//            //{
//            //    long roleId = uniqueIDGameServer.GetUniqueID((int)CollectionType.Role);

//            //    Console.WriteLine("角色编号=" + roleId);
//            //}
//        }
//    }
//}
