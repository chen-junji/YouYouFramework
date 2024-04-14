using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// 日志控制器
/// </summary>
public class LoggerCtrl
{
    public static LoggerCtrl Instance { get; private set; } = new();

    private List<string> m_LogArray;

    private string m_LogPath = null;
    private string ReporterPath = Application.persistentDataPath + "//Reporter";
    private int m_LogMaxCapacity = 500;
    private int m_CurrLogCount = 0;

    private int m_LogBufferMaxNumber = 10;

    internal void Init()
    {
        m_LogArray = new List<string>();

        if (string.IsNullOrEmpty(m_LogPath))
        {
            m_LogPath = ReporterPath + "//" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + "-Start.txt";
        }
    }

    internal void Write(LogType type, string writeFileData)
    {
        if (m_CurrLogCount >= m_LogMaxCapacity)
        {
            m_LogPath = ReporterPath + "//" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".txt";
            m_LogMaxCapacity = 0;
        }
        m_CurrLogCount++;

        if (!string.IsNullOrEmpty(writeFileData))
        {
            writeFileData = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff") + "|" + type.ToString() + "|" + writeFileData + "\r\n";
            AppendDataToFile(writeFileData);
        }
    }

    #region AppendDataToFile
    private void AppendDataToFile(string writeFileDate)
    {
        if (m_LogArray == null) return;
        if (!string.IsNullOrEmpty(writeFileDate))
        {
            m_LogArray.Add(writeFileDate);
        }

        if (m_LogArray.Count % m_LogBufferMaxNumber == 0)
        {
            SyncLog();
        }
    }
    #endregion

    #region CreateFile
    private void CreateFile(string pathAndName, string info)
    {
        if (!Directory.Exists(ReporterPath)) Directory.CreateDirectory(ReporterPath);

        StreamWriter sw;
        FileInfo t = new FileInfo(pathAndName);
        if (!t.Exists)
        {
            sw = t.CreateText();
        }
        else
        {
            sw = t.AppendText();
        }

        sw.WriteLine(info);

        sw.Close();

        sw.Dispose();
    }
    #endregion

    #region ClearLogArray
    private void ClearLogArray()
    {
        if (m_LogArray != null)
        {
            m_LogArray.Clear();
        }
    }
    #endregion

    #region SyncLog
    internal void SyncLog()
    {
        if (!string.IsNullOrEmpty(m_LogPath))
        {
            for (int i = 0; i < m_LogArray.Count; i++)
            {
                CreateFile(m_LogPath, m_LogArray[i]);
            }
            ClearLogArray();
        }
    }
    #endregion
}