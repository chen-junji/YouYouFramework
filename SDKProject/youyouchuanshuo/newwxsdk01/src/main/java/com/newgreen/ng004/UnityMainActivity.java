package com.newgreen.ng004;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.content.Intent;

import com.tencent.mm.opensdk.modelpay.PayReq;
import com.tencent.mm.sdk.openapi.SendAuth;

import com.newgreen.ng004.ALiApi.PayResult;
import com.alipay.sdk.app.PayTask;
import android.annotation.SuppressLint;
import android.os.Handler;
import android.os.Message;
import android.text.TextUtils;
import android.widget.Toast;
import java.util.Map;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import com.newgreen.ng004.wxapi.Constants;

public class UnityMainActivity extends UnityPlayerActivity {

    @Override
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);

        // 1.通过WXAPIFactory工厂，获取IWXAPI的实例
        Constants.wx_api = com.tencent.mm.sdk.openapi.WXAPIFactory.createWXAPI(this, null);

        // 2.将该app注册到微信
        Constants.wx_api.registerApp(Constants.APP_ID);

        Constants.wxPayApi = com.tencent.mm.opensdk.openapi.WXAPIFactory.createWXAPI(this, null);
        Constants.wxPayApi.registerApp(Constants.APP_ID);

        Log.i("youyou", "youyou UnityMainActivity 将该app注册到微信");
    }

    //接收Unity发过来的消息
    public void DoAndroidAction(String actionName, String params) {
        switch (actionName) {
            case "login":
                WXLogIn(params);
                break;
            case "pay":
                WXPay(params);
                break;
            case "payv2":
                payV2(params);
                break;
        }
    }

    //发送消息给Unity
    public void DoUnityAction(String actionName, String params) {
        //这里的UnityPlayer.UnitySendMessage（）；是Android工程调用Unity中的方法
        UnityPlayer.UnitySendMessage("AndroidInterface", "DoUnityAction", String.format("%s^%s", actionName, params));
    }

    //当活动重新激活并返回后，在这里接受数据
    @Override
    public void onNewIntent(Intent intent) {
        super.onNewIntent(intent);

        int youyou_status = intent.getIntExtra("youyou_status", -1); //这里需要设置默认值
        switch (youyou_status) {
            case -1:
                break;
            case 0:
                String state = intent.getStringExtra("state");
                String token = intent.getStringExtra("token");
                DoUnityAction("SendCode", String.format("%s\t%s", state, token));
                break;
            case 1:
                //微信支付
                break;
        }
    }

    private void WXLogIn(String params) {
        Log.i("youyou", "youyou WXLogIn");
        // 第三步 调用微信登录
        SendAuth.Req req = new SendAuth.Req();
        req.scope = "snsapi_userinfo";
        req.state = params;
        Constants.wx_api.sendReq(req);
    }

    private void WXPay(String params) {
        String[] arr = params.split("\t");

        PayReq req = new PayReq();

        req.appId = Constants.APP_ID;
        req.partnerId = arr[0];
        req.prepayId = arr[1];
        req.nonceStr = arr[2];
        req.timeStamp = arr[3];
        req.packageValue = arr[4];
        req.sign = arr[5];
        Constants.wxPayApi.sendReq(req);
    }

    private static final int SDK_PAY_FLAG = 1;
    public  void payV2(final String orderInfo) {

        Runnable payRunnable = new Runnable() {

            @Override
            public void run() {
                PayTask alipay = new PayTask(getActivity());
                Map<String, String> result = alipay.payV2(orderInfo, true);

                Message msg = new Message();
                msg.what = SDK_PAY_FLAG;
                msg.obj = result;
                mHandler.sendMessage(msg);
            }
        };

        Thread payThread = new Thread(payRunnable);
        payThread.start();
    }
    @SuppressLint("HandlerLeak")
    private Handler mHandler = new Handler() {
        @SuppressWarnings("unused")
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case SDK_PAY_FLAG: {
                    @SuppressWarnings("unchecked")
                    PayResult payResult = new PayResult((Map<String, String>) msg.obj);
                    /**
                     * 对于支付结果，请商户依赖服务端的异步通知结果。同步通知结果，仅作为支付结束的通知。
                     */
                    String resultInfo = payResult.getResult();// 同步返回需要验证的信息
                    String resultStatus = payResult.getResultStatus();
                    // 判断resultStatus 为9000则代表支付成功
                    if (TextUtils.equals(resultStatus, "9000")) {
                        // 该笔订单是否真实支付成功，需要依赖服务端的异步通知。
                        Toast.makeText(getActivity(), "支付成功", Toast.LENGTH_SHORT).show();
                    } else {
                        // 该笔订单真实的支付结果，需要依赖服务端的异步通知。
                        Toast.makeText(getActivity(), "支付失败", Toast.LENGTH_SHORT).show();
                    }
                    DoUnityAction("AliPayTest","TestMethod");
                    break;
                }
//                case SDK_AUTH_FLAG: {
//                    @SuppressWarnings("unchecked")
//                    AuthResult authResult = new AuthResult((Map<String, String>) msg.obj, true);
//                    String resultStatus = authResult.getResultStatus();
//
//                    // 判断resultStatus 为“9000”且result_code
//                    // 为“200”则代表授权成功，具体状态码代表含义可参考授权接口文档
//                    if (TextUtils.equals(resultStatus, "9000") && TextUtils.equals(authResult.getResultCode(), "200")) {
//                        // 获取alipay_open_id，调支付时作为参数extern_token 的value
//                        // 传入，则支付账户为该授权账户
//                        showAlert(PayDemoActivity.this, getString(R.string.auth_success) + authResult);
//                    } else {
//                        // 其他状态值则为授权失败
//                        showAlert(PayDemoActivity.this, getString(R.string.auth_failed) + authResult);
//                    }
//                    break;
                // }
                default:
                    break;
            }
        };
    };

    private Activity _unityActivity;
    /**
     * 获取unity项目的上下文
     * @return
     */
    Activity getActivity(){
        if(null == _unityActivity) {
            try {
                Class<?> classtype = Class.forName("com.unity3d.player.UnityPlayer");
                Activity activity = (Activity) classtype.getDeclaredField("currentActivity").get(classtype);
                _unityActivity = activity;
            } catch (ClassNotFoundException e) {

            } catch (IllegalAccessException e) {

            } catch (NoSuchFieldException e) {

            }
        }
        return _unityActivity;
    }


}