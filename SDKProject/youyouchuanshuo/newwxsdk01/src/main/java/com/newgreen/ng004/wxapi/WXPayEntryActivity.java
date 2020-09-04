package com.newgreen.ng004.wxapi;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.tencent.mm.opensdk.constants.ConstantsAPI;
import com.tencent.mm.opensdk.modelbase.BaseReq;
import com.tencent.mm.opensdk.modelbase.BaseResp;
import com.tencent.mm.opensdk.openapi.IWXAPIEventHandler;
import com.newgreen.ng004.UnityMainActivity;

public class WXPayEntryActivity extends Activity implements IWXAPIEventHandler {

    @Override
    public void onReq(BaseReq baseReq) {

    }

    @Override
    public void onResp(BaseResp baseResp) {

        if (baseResp.getType() == ConstantsAPI.COMMAND_PAY_BY_WX) {

            Log.i("WXTest", "zmx baseResp errCode=" + baseResp.errCode);

            switch (baseResp.errCode) {
                case BaseResp.ErrCode.ERR_OK://成功
                    break;
                case BaseResp.ErrCode.ERR_USER_CANCEL://取消支付
                    break;
                case BaseResp.ErrCode.ERR_COMM:// -1
                    break;
            }

            Intent intent = new Intent(WXPayEntryActivity.this, UnityMainActivity.class);

            //设置参数
            intent.putExtra("youyou_status", 1); //1表示微信支付

            //启动活动
            startActivity(intent);
            finish();
        }

    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Constants.wxPayApi.handleIntent(getIntent(), this);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        Constants.wxPayApi.handleIntent(intent, this);
    }
}