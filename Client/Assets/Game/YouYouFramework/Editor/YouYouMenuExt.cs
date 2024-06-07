using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class YouYouMenuExt
{
    [MenuItem("Assets/Sprite导出png")]
    public static void SpriteToPng()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            // 假设你已经在Unity Editor中选择了要导出的Sprite  
            Sprite selectedSprite = objs[i] as Sprite;
            if (selectedSprite == null)
            {
                Debug.LogError("No Sprite selected in the Editor!");
                continue;
            }

            // 设置导出路径和文件名  
            string exportPath = $"E:/{objs[0].name}/"; // 你可以根据需要修改这个路径  
            string fileName = selectedSprite.name + ".png";
            string fullPath = Path.Combine(exportPath, fileName);

            // 确保导出目录存在  
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            // 从Sprite获取Texture2D  
            Texture2D texture2D = new Texture2D((int)selectedSprite.rect.width, (int)selectedSprite.rect.height);
            Color[] colors = selectedSprite.texture.GetPixels((int)selectedSprite.rect.x, (int)selectedSprite.rect.y, (int)selectedSprite.rect.width, (int)selectedSprite.rect.height);
            texture2D.SetPixels(colors);
            texture2D.Apply();

            // 导出为PNG  
            File.WriteAllBytes(fullPath, texture2D.EncodeToPNG());
            Debug.Log("Sprite exported to: " + fullPath);
        }
    }

}
