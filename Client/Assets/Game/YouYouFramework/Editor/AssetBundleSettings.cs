using YouYouMain;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YouYouFramework;

[CreateAssetMenu(menuName = "YouYouAsset/AssetBundleSettings")]
public class AssetBundleSettings : ScriptableObject
{
    public enum CusBuildTarget
    {
        Windows,
        Android,
        IOS
    }

    [HorizontalGroup("Common", LabelWidth = 75)]
    [VerticalGroup("Common/Left")]
    [LabelText("资源版本号")]
    public string AssetVersion = "1.0.0";

    [PropertySpace(10)]
    [VerticalGroup("Common/Left")]
    [LabelText("目标平台")]
    public CusBuildTarget CurrBuildTarget;

    public BuildTarget GetBuildTarget()
    {
        switch (CurrBuildTarget)
        {
            default:
            case CusBuildTarget.Windows:
                return BuildTarget.StandaloneWindows64;
            case CusBuildTarget.Android:
                return BuildTarget.Android;
            case CusBuildTarget.IOS:
                return BuildTarget.iOS;
        }
    }

    [PropertySpace(10)]
    [VerticalGroup("Common/Left")]
    [LabelText("参数")]
    public BuildAssetBundleOptions Options;

    [VerticalGroup("Common/Right")]
    [Button(ButtonSizes.Medium)]
    [LabelText("清空本地CDN资源包")]
    public void ClearAssetBundle()
    {
        if (Directory.Exists(TempPath))
        {
            Directory.Delete(TempPath, true);
        }
        EditorUtility.DisplayDialog("", "清空完毕", "确定");
    }

    /// <summary>
    /// 要收集的资源包
    /// </summary>
    List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

    [VerticalGroup("Common/Right")]
    [Button(ButtonSizes.Medium)]
    [LabelText("打包到本地CDN")]
    public void BuildAssetBundle()
    {
#if !ASSETBUNDLE
        Debug.LogError("请在YouYouEditor窗口下，把加载模式切换到AssetBundle， 才可以进行打包");
#else
        CopyHofixDll();

        builds.Clear();
        int len = Datas.Length;
        for (int i = 0; i < len; i++)
        {
            AssetBundleData assetBundleData = Datas[i];
            if (assetBundleData.IsBuild)
            {
                int lenPath = assetBundleData.Path.Length;
                for (int j = 0; j < lenPath; j++)
                {
                    //打包这个路径
                    string path = assetBundleData.Path[j];
                    BuildAssetBundleForPath(path, assetBundleData.Overall);
                }
            }
        }

        if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);

        if (builds.Count == 0) return;
        Debug.Log("builds count==" + builds.Count);

        BuildPipeline.BuildAssetBundles(TempPath, builds.ToArray(), Options, GetBuildTarget());
        Debug.Log("临时资源包打包完毕");

        CopyFile(TempPath);
        Debug.Log("拷贝到输出目录完毕");

        AssetBundleEncrypt();
        Debug.Log("资源包加密完毕");

        CreateDependenciesFile();
        Debug.Log("AssetInfo生成依赖关系文件完毕");

        CreateVersionFile();
        Debug.Log("VersionFile生成版本文件完毕");
#endif
    }

    #region TempPath OutPath
    /// <summary>
    /// 临时目录
    /// </summary>
    public string TempPath
    {
        get
        {
            return Application.dataPath + "/../" + AssetBundleSavePath + "/" + AssetVersion + "_Temp/" + CurrBuildTarget;
        }
    }

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutPath
    {
        get
        {
            return TempPath.Replace("_Temp", "");
        }
    }
    #endregion

    #region CopyFile 拷贝文件到正式目录
    /// <summary>
    /// 拷贝文件到正式目录
    /// </summary>
    /// <param name="oldPath"></param>
    private void CopyFile(string oldPath)
    {
        if (Directory.Exists(OutPath))
        {
            Directory.Delete(OutPath, true);
        }

        IOUtil.CopyDirectory(oldPath, OutPath);
        DirectoryInfo directory = new DirectoryInfo(OutPath);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*.y", SearchOption.AllDirectories);
        int len = arrFiles.Length;
        for (int i = 0; i < len; i++)
        {
            FileInfo file = arrFiles[i];
            File.Move(file.FullName, file.FullName.Replace(".ab.y", ".assetbundle"));
        }
    }
    #endregion

    #region AssetBundleEncrypt 资源包加密
    /// <summary>
    /// 资源包加密
    /// </summary>
    /// <param name="path"></param>
    private void AssetBundleEncrypt()
    {
        int len = Datas.Length;
        for (int i = 0; i < len; i++)
        {
            AssetBundleData assetBundleData = Datas[i];
            if (assetBundleData.IsEncrypt && assetBundleData.IsBuild)
            {
                //如果需要加密
                for (int j = 0; j < assetBundleData.Path.Length; j++)
                {
                    string path = OutPath + "/" + assetBundleData.Path[j];

                    if (assetBundleData.Overall)
                    {
                        //不是遍历文件夹打包 说明这个路径就是一个包
                        path = path + ".assetbundle";

                        AssetBundleEncryptFile(path);
                    }
                    else
                    {
                        AssetBundleEncryptFolder(path);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加密文件夹下所有文件
    /// </summary>
    /// <param name="folderPath"></param>
    private void AssetBundleEncryptFolder(string folderPath, bool isDelete = false)
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

        foreach (FileInfo file in arrFiles)
        {
            AssetBundleEncryptFile(file.FullName, isDelete);
        }
    }

    /// <summary>
    /// 加密文件
    /// </summary>
    /// <param name="filePath"></param>
    private void AssetBundleEncryptFile(string filePath, bool isDelete = false)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        byte[] buffer = null;

        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
        }

        buffer = SecurityUtil.Xor(buffer);

        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }
    }
    #endregion

    #region OnCreateDependenciesFile 生成依赖关系文件
    /// <summary>
    /// 生成依赖关系文件
    /// </summary>
    private void CreateDependenciesFile()
    {
        //第一次循环 把所有的Asset存储到一个列表里

        //临时列表
        List<AssetInfoEntity> tempLst = new List<AssetInfoEntity>();
        Dictionary<string, AssetInfoEntity> tempDic = new Dictionary<string, AssetInfoEntity>();

        //循环设置文件夹包括子文件里边的项
        for (int i = 0; i < Datas.Length; i++)
        {
            AssetBundleData assetBundleData = Datas[i];
            if (assetBundleData.IsBuild)
            {
                for (int j = 0; j < assetBundleData.Path.Length; j++)
                {
                    string path = Application.dataPath + "/" + assetBundleData.Path[j];
                    //Debug.LogError("CreateDependenciesFile path=" + path);
                    CollectFileInfo(tempLst, tempDic, path);
                }
            }
        }

        //资源列表
        List<AssetInfoEntity> assetList = new List<AssetInfoEntity>();
        for (int i = 0; i < tempLst.Count; i++)
        {
            AssetInfoEntity entity = tempLst[i];

            AssetInfoEntity newEntity = new AssetInfoEntity();
            newEntity.AssetFullPath = entity.AssetFullPath;
            newEntity.AssetBundleFullPath = entity.AssetBundleFullPath;

            assetList.Add(newEntity);

            newEntity.DependsAssetBundleList = new List<string>();
            string[] arr = AssetDatabase.GetDependencies(entity.AssetFullPath, true);
            foreach (string str in arr)
            {
                if (!str.IsSuffix(".cs") && tempDic.ContainsKey(str))
                {
                    //把多余的依赖AB包剔除掉，比如依赖AB包==主AB包， 或者依赖AB包已经存在于DependsAssetBundleList内
                    if (!newEntity.AssetBundleFullPath.Equals(tempDic[str].AssetBundleFullPath) &&
                        !newEntity.DependsAssetBundleList.Contains(tempDic[str].AssetBundleFullPath))
                    {
                        //把依赖资源 加入到依赖资源列表
                        newEntity.DependsAssetBundleList.Add(tempDic[str].AssetBundleFullPath);
                    }
                }
            }
        }

        //生成一个Json文件
        string targetPath = OutPath;
        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        string strJsonFilePath = targetPath + "/AssetInfo.json"; //依赖文件路径
        IOUtil.CreateTextFile(strJsonFilePath, assetList.ToJson());

        //生成二进制文件
        MMO_MemoryStream ms = new MMO_MemoryStream();
        ms.WriteInt(assetList.Count);
        for (int i = 0; i < assetList.Count; i++)
        {
            AssetInfoEntity entity = assetList[i];
            ms.WriteUTF8String(entity.AssetFullPath);
            ms.WriteUTF8String(entity.AssetBundleFullPath);

            if (entity.DependsAssetBundleList != null)
            {
                //添加依赖资源
                int depLen = entity.DependsAssetBundleList.Count;
                ms.WriteInt(depLen);
                for (int j = 0; j < depLen; j++)
                {
                    ms.WriteUTF8String(entity.DependsAssetBundleList[j]);
                }
            }
            else
            {
                ms.WriteInt(0);
            }
        }

        string filePath = targetPath + "/AssetInfo.bytes"; //版本文件路径
        byte[] buffer = ms.ToArray();
        buffer = ZlibHelper.CompressBytes(buffer);
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            fs.Write(buffer, 0, buffer.Length);
        }
    }
    /// <summary>
    /// 收集文件信息
    /// </summary>
    private void CollectFileInfo(List<AssetInfoEntity> tempLst, Dictionary<string, AssetInfoEntity> tempDic, string folderPath)
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);
        if (directory.Exists == false) return;

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

        for (int i = 0; i < arrFiles.Length; i++)
        {
            FileInfo file = arrFiles[i];
            if (file.Extension == ".meta") continue;
            if (file.FullName.IndexOf(".idea") != -1) continue;

            //绝对路径
            string filePath = file.FullName;
            //Debug.LogError("filePath==" + filePath);

            AssetInfoEntity entity = new AssetInfoEntity();
            //相对路径
            entity.AssetFullPath = filePath.Substring(filePath.IndexOf("Assets\\")).Replace("\\", "/");
            //Debug.LogError("AssetFullName==" + entity.AssetFullName);

            entity.AssetBundleFullPath = (GetAssetBundleName(entity.AssetFullPath) + ".assetbundle").ToLower();
            tempLst.Add(entity);
            tempDic.Add(entity.AssetFullPath, entity);
        }
    }
    #endregion

    #region CreateVersionFile 生成版本文件
    /// <summary>
    /// 生成版本文件
    /// </summary>
    private void CreateVersionFile()
    {
        string path = OutPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string strVersionFilePath = path + "/VersionFile.txt"; //版本文件路径

        //如果版本文件存在 则删除
        IOUtil.DeleteFile(strVersionFilePath);

        StringBuilder sbContent = new StringBuilder();

        DirectoryInfo directory = new DirectoryInfo(path);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

        sbContent.AppendLine(AssetVersion);
        for (int i = 0; i < arrFiles.Length; i++)
        {
            FileInfo file = arrFiles[i];

            if (file.Extension == ".manifest")
            {
                continue;
            }
            string fullName = file.FullName; //全名 包含路径扩展名

            //相对路径
            string name = fullName.Substring(fullName.IndexOf(CurrBuildTarget.ToString()) + CurrBuildTarget.ToString().Length + 1);

            string md5 = EncryptUtil.GetFileMD5(fullName); //文件的MD5
            if (md5 == null) continue;

            string size = file.Length.ToString(); //文件大小

            bool isFirstData = false; //是否初始数据
            bool isEncrypt = false;
            bool isBreak = false;

            for (int j = 0; j < Datas.Length; j++)
            {
                foreach (string mPath in Datas[j].Path)
                {
                    string tempPath = mPath;

                    name = name.Replace("\\", "/");
                    if (name.IndexOf(tempPath, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        isFirstData = Datas[j].IsFirstData;
                        isEncrypt = Datas[j].IsEncrypt;
                        isBreak = true;
                        break;
                    }
                }
                if (isBreak) break;
            }

            string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", name, md5, size, isFirstData ? 1 : 0, isEncrypt ? 1 : 0);
            sbContent.AppendLine(strLine);
        }

        IOUtil.CreateTextFile(strVersionFilePath, sbContent.ToString());

        MMO_MemoryStream ms = new MMO_MemoryStream();
        string str = sbContent.ToString().Trim();
        string[] arr = str.Split('\n');
        int len = arr.Length;
        ms.WriteInt(len);
        for (int i = 0; i < len; i++)
        {
            if (i == 0)
            {
                ms.WriteUTF8String(arr[i]);
            }
            else
            {
                string[] arrInner = arr[i].Split('|');
                ms.WriteUTF8String(arrInner[0]);
                ms.WriteUTF8String(arrInner[1]);
                ms.WriteULong(ulong.Parse(arrInner[2]));
                ms.WriteByte(byte.Parse(arrInner[3]));
                ms.WriteByte(byte.Parse(arrInner[4]));
            }
        }

        string filePath = path + "/VersionFile.bytes"; //版本文件路径
        byte[] buffer = ms.ToArray();
        buffer = ZlibHelper.CompressBytes(buffer);
        FileStream fs = new FileStream(filePath, FileMode.Create);
        fs.Write(buffer, 0, buffer.Length);
        fs.Close();
        fs.Dispose();
    }

    #endregion

    #region GetAssetBundleName 获取资源包的名称
    /// <summary>
    /// 获取资源包的名称
    /// </summary>
    /// <param name="assetFullName"></param>
    /// <returns></returns>
    private string GetAssetBundleName(string assetFullName)
    {
        int len = Datas.Length;
        //循环设置文件夹包括子文件里边的项
        for (int i = 0; i < len; i++)
        {
            AssetBundleData assetBundleData = Datas[i];
            for (int j = 0; j < assetBundleData.Path.Length; j++)
            {
                if (assetFullName.IndexOf(assetBundleData.Path[j], StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    if (assetBundleData.Overall)
                    {
                        //文件夹是个整包 则返回这个特文件夹名字
                        return assetBundleData.Path[j].ToLower();
                    }
                    else
                    {
                        //零散资源
                        //return assetFullName.Substring(0, assetFullName.LastIndexOf('.')).ToLower().Replace("assets/", "");
                        return assetFullName.ToLower().Replace("assets/", "");
                    }
                }
            }
        }
        return null;
    }
    #endregion

    #region CopyHofixDll 热更Dll复制到Hotfix目录
    /// <summary>
    /// 热更Dll复制到Hotfix目录
    /// </summary>
    public void CopyHofixDll()
    {
        HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(GetBuildTarget());

        string CodeDir = "Assets/Game/Download/Hotfix/";

        string ScriptAssembliesDir = Application.dataPath + "/../" + "HybridCLRData/HotUpdateDlls/" + GetBuildTarget().ToString() + "/Assembly-CSharp.dll";
        File.Copy(ScriptAssembliesDir, Path.Combine(CodeDir, "Assembly-CSharp.dll.bytes"), true);

        string aotMetaAssemblyDir = Application.dataPath + "/../" + "HybridCLRData/AssembliesPostIl2CppStrip/" + GetBuildTarget().ToString() + "/";
        foreach (var aotDllName in HotfixManager.aotMetaAssemblyFiles)
        {
            File.Copy(aotMetaAssemblyDir + aotDllName + ".dll", Path.Combine(CodeDir, aotDllName + ".dll.bytes"), true);
        }

        AssetDatabase.Refresh();
        Debug.Log("热更dll和补充元数据dll, 复制到Game/Download/Hotfix完成");
    }
    #endregion

    /// <summary>
    /// 根据路径打包资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="overall">打成一个资源包</param>
    private void BuildAssetBundleForPath(string path, bool overall)
    {
        string fullPath = Application.dataPath + "/" + path;
        //Debug.LogError("fullPath=" + fullPath);

        //1.拿到文件夹下所有文件
        DirectoryInfo directory = new DirectoryInfo(fullPath);
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

        if (overall)
        {
            //打成一个资源包
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = path + ".ab";
            build.assetBundleVariant = "y";
            string[] arr = GetValidateFiles(arrFiles);
            build.assetNames = arr;
            builds.Add(build);
        }
        else
        {
            //每个文件打成一个包
            string[] arr = GetValidateFiles(arrFiles);
            for (int i = 0; i < arr.Length; i++)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                //build.assetBundleName = arr[i].Substring(0, arr[i].LastIndexOf('.')).Replace("Assets/", "") + ".ab";
                build.assetBundleName = arr[i].Replace("Assets/", "") + ".ab";
                build.assetBundleVariant = "y";
                build.assetNames = new string[] { arr[i] };

                //Debug.LogError("assetBundleName==" + build.assetBundleName);
                builds.Add(build);
            }
        }
    }

    private string[] GetValidateFiles(FileInfo[] arrFiles)
    {
        List<string> lst = new List<string>();

        int len = arrFiles.Length;
        for (int i = 0; i < len; i++)
        {
            FileInfo file = arrFiles[i];
            if (!file.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase))
            {
                lst.Add("Assets" + file.FullName.Replace("\\", "/").Replace(Application.dataPath, ""));
            }
        }

        return lst.ToArray();
    }


    [LabelText("资源包保存路径")]
    [FolderPath]
    /// <summary>
    /// 资源包保存路径
    /// </summary>
    public string AssetBundleSavePath;

    [LabelText("勾选进行编辑")]
    public bool IsCanEditor;

    [EnableIf("IsCanEditor")]
    [BoxGroup("AssetBundleSettings")]
    public AssetBundleData[] Datas;

    //必须加上可序列化标记
    [Serializable]
    public class AssetBundleData
    {
        [LabelText("名称")]
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        [LabelText("是否要打包")]
        public bool IsBuild = true;

        [LabelText("文件夹为一个资源包")]
        /// <summary>
        /// 文件夹为一个资源包
        /// </summary>
        public bool Overall;

        [LabelText("是否初始资源")]
        /// <summary>
        /// 是否初始资源
        /// </summary>
        public bool IsFirstData = true;

        [LabelText("是否加密")]
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncrypt;

        [FolderPath(ParentFolder = "Assets")]
        /// <summary>
        /// 路径
        /// </summary>
        public string[] Path;
    }
}