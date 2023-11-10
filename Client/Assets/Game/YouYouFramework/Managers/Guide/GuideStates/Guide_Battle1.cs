using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace YouYou
{
    public class Guide_Battle1 : Singleton<Guide_Battle1>
    {
        private GuideGroup GuideGroup;

        //第一关 新手引导
        public void EnterBattle1()
        {
            if (!GameEntry.Guide.OnStateEnter(GuideState.Battle1)) return;

            GameEntry.Guide.GuideGroup = GuideGroup = new GuideGroup();
            GuideRoutine guideRoutine = null;

            //第一步
            guideRoutine = new GuideRoutine();
            guideRoutine.OnEnter = () =>
            {
                //打开镂空遮罩，只有镂空区域可点击
                FormHollow.ShowDialog("FormGuide_Quest1_Dialog1");

                //监听按钮点击, 触发下一步
                Button button = null;
                GuideUtil.CheckBtnNext(button);
            };
            guideRoutine.OnExit = () =>
            {
                //步骤结束时， 把镂空遮罩关了
                GameEntry.UI.CloseUIForm<FormHollow>();
            };
            GuideGroup.AddGuide(guideRoutine);

            //第二步
            guideRoutine = new GuideRoutine();
            guideRoutine.OnEnter = () =>
            {
                //打开镂空遮罩, 全屏可点击（点击了就触发下一步）
                FormHollow2.ShowDialog("FormGuide_Quest1_Dialog1");
            };
            guideRoutine.GuideKey = "TestKey";//名字可填可不填， 填了就可以用于外部映射
            GuideGroup.AddGuide(guideRoutine);

            //第三步
            GuideGroup.AddGuide(() =>
            {
                //监听开关打开, 触发下一步
                Toggle toggle = null;
                GuideUtil.CheckToggleNext(toggle);
            });

            //第四步
            GuideGroup.AddGuide(() =>
            {
                //监听事件 触发下一步
                GuideUtil.CheckEventNext("EventName");
            });

            //启动新手引导组（大步骤）
            GuideGroup.Run(() =>
            {
                //多个小步骤全部做完了， 网络存档
                GuideModel.Instance.GuideCompleteOne(GameEntry.Guide.CurrentState);
            });
        }
    }
}