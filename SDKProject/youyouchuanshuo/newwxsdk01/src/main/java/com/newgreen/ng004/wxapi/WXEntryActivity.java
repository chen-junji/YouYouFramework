package com.newgreen.ng004.wxapi;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

import com.tencent.mm.sdk.openapi.IWXAPIEventHandler;
import com.tencent.mm.sdk.openapi.BaseReq;
import com.tencent.mm.sdk.openapi.BaseResp;
import com.tencent.mm.sdk.openapi.SendAuth;

import com.newgreen.ng004.UnityMainActivity;

public class WXEntryActivity extends Activity implements IWXAPIEventHandler {
    @Override
    public void onReq(BaseReq baseReq) {
    }

    @Override
    public void onResp(BaseResp baseResp) {

        if (baseResp.errCode == 0) {
            SendAuth.Resp resp = (SendAuth.Resp) baseResp;

            //一个坑
            //官方文档 SDK通过SendAuth的Resp返回数据给调用方
            //返回 ErrCode code state lang country

            //iOS返回的参数是正常的，而Android对应不上
            //resp.token 对应的是code

            //目标活动窗口 用onNewIntent 方法来接收参数

            //WXEntryActivity.this 当前活动    UnityMainActivity.class目标活动
            Intent intent = new Intent(WXEntryActivity.this, UnityMainActivity.class);

            //设置参数
            intent.putExtra("youyou_status", 0); //0表示微信登录
            intent.putExtra("state", resp.state);
            intent.putExtra("token", resp.token);

            //启动活动
            startActivity(intent);
            finish();

        } else {
            Intent intent = new Intent(WXEntryActivity.this, UnityMainActivity.class);
            intent.putExtra("youyou_status", -1);

            //启动活动
            startActivity(intent);
            finish();
        }
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Constants.wx_api.handleIntent(getIntent(), this);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        Constants.wx_api.handleIntent(intent, this);
    }
}