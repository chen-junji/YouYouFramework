
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace YouYouServer.Model.Test
//{
//    public class TestRedis
//    {
//        /// <summary>
//        /// 测试字符串
//        /// </summary>
//        public static void TestString()
//        {
//            //RedisHelper.Set("name", "youyou");

//            //string str = RedisHelper.Get("name");

//            string str = RedisHelper.CacheShell("name2", -1, () => { return "youyou3"; });
//            Console.WriteLine("得到了一个key的值=" + str);
//        }

//        /// <summary>
//        /// 测试哈希
//        /// </summary>
//        public static void TestHash()
//        {
//            string key = "RoleHash";
//            //RedisHelper.HSet(key, "1", "youyou1");
//            //RedisHelper.HSet(key, "2", "youyou2");
//            //RedisHelper.HSet(key, "3", "youyou3");
//            //RedisHelper.HSet(key, "4", "youyou4");
//            //RedisHelper.HSet(key, "5", "youyou5");

//            //string value = RedisHelper.HGet(key, "6");

//            //string value = RedisHelper.CacheShell(key, "6", -1, () => { return "youyou6"; });

//            //var ret = RedisHelper.HScan(key, 0);
//            //var arr = ret.Items;

//            //Console.WriteLine("得到了一个哈希的值 field=" + arr[0].field);
//            //Console.WriteLine("得到了一个哈希的值 value=" + arr[0].value);

//            long len = RedisHelper.HLen(key);
//            Console.WriteLine("哈希的数量=" + len);
//        }

//        /// <summary>
//        /// 测试列表
//        /// </summary>
//        public static void TestList()
//        {
//            string key = "list";

//            //RedisHelper.RPush(key, "元素5","元素6","元素7");

//            //long len = RedisHelper.LLen(key);

//            //string str = RedisHelper.LIndex(key, 10);

//            //string[] arr = RedisHelper.LRange(key, 5, 9);
//            //foreach (string str in arr)
//            //{

//            //    Console.WriteLine("得到了一个列表元素=" + str);
//            //}

//            //RedisHelper.LInsertAfter(key, "元素3", "元素3After");
//            //RedisHelper.LSet(key, 0, "元素4修改");
//            //队列
//            string str = RedisHelper.RPop(key);

//            Console.WriteLine("从列表中pop元素=" + str);
//        }

//        /// <summary>
//        /// 测试集合
//        /// </summary>
//        public static void TestSet()
//        {
//            string key = "nickname";

//            //集合插入
//            //RedisHelper.SAdd(key, "悠游4", "悠游5", "悠游6", "悠游7", "悠游8");

//            ////重复数据返回0 否则 1
//            //long ret = RedisHelper.SAdd(key, "悠游9");

//            ////查找元素是否存在
//            //bool isExis =  RedisHelper.SIsMember(key, "悠游10");

//            ////查询数量
//            //long count = RedisHelper.SCard(key);

//            //取出并移除数据
//            //string[] arr= RedisHelper.SPop(key,3);

//            //foreach (string str in arr)
//            //{
//            //    Console.WriteLine("集合=" + str);
//            //}

//            //
//            //RedisHelper.SRem(key, "悠游3");

//            string[] arr = RedisHelper.SMembers(key);
//            foreach (string str in arr)
//            {
//                Console.WriteLine("集合=" + str);
//            }
//        }

//        /// <summary>
//        /// 测试有序集合
//        /// </summary>
//        public static void TestZSet()
//        {
//            string key = "rank_fatting";

//            RedisHelper.Del(key);
//            for (int i = 1; i <= 30; i++)
//            {
//                int roldId = 1;
//                double score = new Random().Next(1, 100);
//                Console.WriteLine("有序集合score=" + score);
//                RedisHelper.ZAdd(key, (score, roldId));
//            }

//            //返回的角色编号
//            //string[] arr = RedisHelper.ZRange(key, 0, 20);
//            //foreach (string str in arr)
//            //{
//            //    Console.WriteLine("有序集合=" + str);
//            //}

//            //最常用排序
//            //(string member, double score)[] lst = RedisHelper.ZRangeWithScores(key, 0, 9);
//            //foreach (var item in lst)
//            //{
//            //    Console.WriteLine("有序集合member=" + item.member);
//            //    Console.WriteLine("有序集合score=" + item.score);
//            //}

//            //总数量
//            //long count = RedisHelper.ZCard(key);

//            ////根据分数范围找角色ID
//            //string[] arr = RedisHelper.ZRangeByScore(key, 50, 60);
//            //foreach (string str in arr)
//            //{
//            //    Console.WriteLine("有序集合=" + str);
//            //}
//        }
//    }
//}