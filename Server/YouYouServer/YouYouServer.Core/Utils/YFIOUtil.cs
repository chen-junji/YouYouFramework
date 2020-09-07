using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YouYouServer.Core
{
    public class YFIOUtil
    {
        /// <summary>
        /// 读取本地文件到byte数组
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] GetBuffer(string path, bool deCompress)
        {
            byte[] buffer = null;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
            }
            return deCompress? ZlibHelper.DeCompressBytes(buffer): buffer;
        }
    }
}
