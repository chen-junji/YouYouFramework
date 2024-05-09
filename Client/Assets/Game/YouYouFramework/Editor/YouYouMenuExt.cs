using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YouYouMain;

public class YouYouMenuExt
{
    #region AssetBundleOpenPersistentDataPath 打开persistentDataPath
    [MenuItem("YouYouTools/打开persistentDataPath")]
    public static void AssetBundleOpenPersistentDataPath()
    {
        string output = Application.persistentDataPath;
        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }
        output = output.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", output);
    }
    #endregion

    #region AssetBundleCopyToStreamingAsstes 初始资源拷贝到StreamingAsstes
    [MenuItem("YouYouTools/初始资源拷贝到StreamingAsstes")]
    public static void AssetBundleCopyToStreamingAsstes()
    {
        string toPath = Application.streamingAssetsPath + "/AssetBundles/";

        if (Directory.Exists(toPath))
        {
            Directory.Delete(toPath, true);
        }
        Directory.CreateDirectory(toPath);

        IOUtil.CopyDirectory(Application.persistentDataPath, toPath);

        //重新生成版本文件
        //1.先读取persistentDataPath里边的版本文件 这个版本文件里 存放了所有的资源包信息

        byte[] buffer = IOUtil.GetFileBuffer(Application.persistentDataPath + "/VersionFile.bytes");
        string version = "";
        Dictionary<string, VersionFileEntity> dic = CheckVersionCtrl.LoadVersionFile(buffer, ref version);
        Dictionary<string, VersionFileEntity> newDic = new Dictionary<string, VersionFileEntity>();

        DirectoryInfo directory = new DirectoryInfo(toPath);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

        for (int i = 0; i < arrFiles.Length; i++)
        {
            FileInfo file = arrFiles[i];
            string fullName = file.FullName.Replace("\\", "/"); //全名 包含路径扩展名
            string name = fullName.Replace(toPath, "").Replace(".assetbundle", "").Replace(".unity3d", "");

            if (name.Equals("AssetInfo.json", System.StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("Windows", System.StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("Windows.manifest", System.StringComparison.CurrentCultureIgnoreCase)

                || name.Equals("Android", System.StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("Android.manifest", System.StringComparison.CurrentCultureIgnoreCase)

                || name.Equals("iOS", System.StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("iOS.manifest", System.StringComparison.CurrentCultureIgnoreCase)
                )
            {
                File.Delete(file.FullName);
                continue;
            }

            VersionFileEntity entity = null;
            dic.TryGetValue(name, out entity);


            if (entity != null)
            {
                newDic[name] = entity;
            }
        }

        StringBuilder sbContent = new StringBuilder();
        sbContent.AppendLine(version);

        foreach (var item in newDic)
        {
            VersionFileEntity entity = item.Value;
            string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", entity.AssetBundleName, entity.MD5, entity.Size, entity.IsFirstData ? 1 : 0, entity.IsEncrypt ? 1 : 0);
            sbContent.AppendLine(strLine);
        }

        IOUtil.CreateTextFile(toPath + "VersionFile.txt", sbContent.ToString());

        //=======================
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
                ms.WriteInt(int.Parse(arrInner[2]));
                ms.WriteByte(byte.Parse(arrInner[3]));
                ms.WriteByte(byte.Parse(arrInner[4]));
            }
        }

        string filePath = toPath + "/VersionFile.bytes"; //版本文件路径
        buffer = ms.ToArray();
        buffer = ZlibHelper.CompressBytes(buffer);
        FileStream fs = new FileStream(filePath, FileMode.Create);
        fs.Write(buffer, 0, buffer.Length);
        fs.Close();

        AssetDatabase.Refresh();
        Debug.Log("初始资源拷贝到StreamingAsstes完毕");
    }
    #endregion

    #region SetFBXAnimationMode 设置文件动画循环为true
    [MenuItem("YouYouTools/设置文件动画循环为true")]
    public static void SetFBXAnimationMode()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            string relatepath = AssetDatabase.GetAssetPath(objs[i]);

            if (relatepath.IsSuffix(".FBX", System.StringComparison.CurrentCultureIgnoreCase))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath + ".meta";
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("loopTime: 0") != -1)
                    {
                        line = "      loopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }

            if (relatepath.IsSuffix(".Anim", System.StringComparison.CurrentCultureIgnoreCase))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath;
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("m_LoopTime: 0") != -1)
                    {
                        line = "    m_LoopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }
        }
        AssetDatabase.Refresh();
    }
    #endregion


    #region GetAssetsPath 收集多个文件的路径到剪切板
    [MenuItem("Assets/收集多个文件的路径到剪切板")]
    public static void GetAssetsPath()
    {
        Object[] objs = Selection.objects;
        string relatepath = string.Empty;
        for (int i = 0; i < objs.Length; i++)
        {
            relatepath += AssetDatabase.GetAssetPath(objs[i]);
            if (i < objs.Length - 1) relatepath += "\n";
        }
        GUIUtility.systemCopyBuffer = relatepath;
        AssetDatabase.Refresh();
    }
    #endregion

    #region GetAssetsPath 收集多个文件的名字到剪切板
    [MenuItem("Assets/收集多个文件的名字到剪切板")]
    public static void GetAssetsName()
    {
        Object[] objs = Selection.objects;
        string relatepath = string.Empty;
        for (int i = 0; i < objs.Length; i++)
        {
            relatepath += objs[i].name;
            if (i < objs.Length - 1) relatepath += "\n";
        }
        GUIUtility.systemCopyBuffer = relatepath;
        AssetDatabase.Refresh();
    }
    #endregion
}
