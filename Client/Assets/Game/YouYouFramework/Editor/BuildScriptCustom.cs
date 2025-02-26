using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildScriptCustom.asset", menuName = "Addressables/Custom Build/Packed Variations")]
public class BuildScriptCustom : BuildScriptPackedMode
{
    protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
    {
        Debug.Log("开始构建");
        YouYouMenuExt.CompiltHotifxDll();
        var result = base.BuildDataImplementation<TResult>(builderInput);
        Debug.Log("完成构建");
        return result;
    }
}