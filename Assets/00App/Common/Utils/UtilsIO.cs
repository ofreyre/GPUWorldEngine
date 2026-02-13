using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace DBG.Utils.IO
{
    public class UtilsIO
    {
        public static string persistentDataPath;

        public static void Save<T>(T data, string relativePath)
        {
            SaveAbsolute(data, Application.persistentDataPath + "/" + relativePath);
        }
        public static void SaveParallel<T>(T data, string relativePath)
        {
            SaveAbsolute(data, persistentDataPath + "/" + relativePath);
        }

        public static void SaveAbsolute<T>(T data, string absolutePath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenWrite(absolutePath);
            bf.Serialize(file, data);
            file.Close();
        }

        public static T Load<T>(string relativePath)
        {
            return LoadAbsolute<T>(Application.persistentDataPath + "/" + relativePath);
        }

        public static T LoadParallel<T>(string relativePath)
        {
            return LoadAbsolute<T>(persistentDataPath + "/" + relativePath);
        }

        public static T LoadAbsolute<T>(string absolutePath)
        {
            Debug.Log("LoadAbsolute");
            if (File.Exists(absolutePath))
            {
                Debug.Log("LoadAbsolute 1");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(absolutePath, FileMode.Open);
                T data = (T)bf.Deserialize(file);
                file.Close();
                return data;
            }
            return default;
        }

        public static bool DeleteFile(string relativePath)
        {
            return DeleteFileAbsolute(Application.persistentDataPath + "/" + relativePath);
        }

        public static bool DeleteFileAbsolute(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                return true;
            }
            return false;
        }

        public static bool FileExist(string relativePath)
        {
            return FileExistAbsolute(Application.persistentDataPath + "/" + relativePath);
        }

        public static bool FileExistParallel(string relativePath)
        {
            return FileExistAbsolute(persistentDataPath + "/" + relativePath);
        }

        public static bool FileExistAbsolute(string absolutePath)
        {
            return File.Exists(absolutePath);
        }

        public static bool FolderExist(string relativePath)
        {
            return FolderExistAbsolute(Application.persistentDataPath + "/" + relativePath);
        }

        public static bool FolderExistParallel(string relativePath)
        {
            return FolderExistAbsolute(persistentDataPath + "/" + relativePath);
        }

        public static bool FolderExistAbsolute(string absolutePath)
        {
            return Directory.Exists(absolutePath);
        }

        public static bool CreateFolder(string relativePath)
        {
            return CreateFolderAbsolute(Application.persistentDataPath + "/" + relativePath);
        }

        public static bool CreateFolderParallel(string relativePath)
        {
            return CreateFolderAbsolute(persistentDataPath + "/" + relativePath);
        }

        public static bool CreateFolderAbsolute(string absolutePath)
        {
            if (Directory.Exists(absolutePath))
            {
                return false;
            }
            Directory.CreateDirectory(absolutePath);
            return true;
        }

        public static bool DeleteFolder(string relativePath)
        {
            return DeleteFolderAbsolute(Application.persistentDataPath + "/" + relativePath);
        }

        public static bool DeleteFolderParallel(string relativePath)
        {
            return DeleteFolderAbsolute(persistentDataPath + "/" + relativePath);
        }

        public static bool DeleteFolderAbsolute(string absolutePath)
        {
            if (!Directory.Exists(absolutePath))
            {
                return false;
            }
            Directory.Delete(absolutePath, true);
            return true;
        }

        public static void ClearFolder(string relativePath)
        {
            ClearFolderAbsolute(Application.persistentDataPath + "/" + relativePath);
        }

        public static void ClearFolderParallel(string relativePath)
        {
            ClearFolderAbsolute(persistentDataPath + "/" + relativePath);
        }

        public static void ClearFolderAbsolute(string absolutePath)
        {
            if (!Directory.Exists(absolutePath))
            {
                return;
            }
            Array.ForEach(Directory.GetFiles(absolutePath), delegate (string path) { File.Delete(path); });
        }

        public static string[] FolderFiles(string relativePath, string searchPattern = "")
        {
            return FolderFilesAbsolute(Application.persistentDataPath + "/" + relativePath, searchPattern);
        }

        public static string[] FolderFilesParallel(string relativePath, string searchPattern = "")
        {
            return FolderFilesAbsolute(persistentDataPath + "/" + relativePath, searchPattern);
        }

        public static string[] FolderFilesAbsolute(string absolutePath, string searchPattern = "")
        {
            if (!Directory.Exists(absolutePath))
            {
                return null;
            }
            if (searchPattern == "")
            {
                return Directory.GetFiles(absolutePath, searchPattern);
            }
            return Directory.GetFiles(absolutePath, searchPattern);
        }

        public static bool MoveFile(string absoluteFrom, string absoluteTo)
        {
            if (FileExistAbsolute(absoluteFrom))
            {
                File.Move(absoluteFrom, absoluteTo);
                return true;
            }
            return false;
        }

        public static string Path2FileName(string absolutePath)
        {
            return Path.GetFileName(absolutePath);
        }
    }
}
