using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouMain;

public class LoadUtil 
{
    public static Dictionary<string, VersionFileEntity> LoadVersionFile(byte[] buffer, ref string version)
    {
        buffer = ZlibHelper.DeCompressBytes(buffer);

        Dictionary<string, VersionFileEntity> dic = new Dictionary<string, VersionFileEntity>();

        MMO_MemoryStream ms = new MMO_MemoryStream(buffer);

        int len = ms.ReadInt();

        for (int i = 0; i < len; i++)
        {
            if (i == 0)
            {
                version = ms.ReadUTF8String().Trim();
            }
            else
            {
                VersionFileEntity entity = new VersionFileEntity();
                entity.AssetBundleName = ms.ReadUTF8String();
                entity.MD5 = ms.ReadUTF8String();
                entity.Size = ms.ReadULong();
                entity.IsFirstData = ms.ReadByte() == 1;
                entity.IsEncrypt = ms.ReadByte() == 1;

                dic[entity.AssetBundleName] = entity;
            }
        }
        return dic;
    }
}
