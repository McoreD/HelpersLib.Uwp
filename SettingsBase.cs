using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpersLib
{
    public abstract class SettingsBase<T> where T : SettingsBase<T>, new()
    {
        public delegate void SettingsSavedEventHandler(T settings, string filePath, bool result);
        public event SettingsSavedEventHandler SettingsSaved;

        public string FilePath { get; private set; }

        public string ApplicationVersion { get; set; }

        public bool IsFirstTimeRun
        {
            get
            {
                return string.IsNullOrEmpty(ApplicationVersion);
            }
        }

        protected virtual void OnSettingsSaved(string filePath, bool result)
        {
            if (SettingsSaved != null)
            {
                SettingsSaved((T)this, filePath, result);
            }
        }

        public bool Save(string filePath)
        {
            FilePath = filePath;
            ApplicationVersion = AppInfo.Version;

            bool result = SaveInternal(this, FilePath, true);

            OnSettingsSaved(FilePath, result);

            return result;
        }

        public bool Save()
        {
            return Save(FilePath);
        }

        public Task<bool> SaveAsync(string filePath)
        {
            return Task.Run(() => Save(filePath));
        }

        public Task<bool> SaveAsync(Stream s)
        {
            return Task.Run(() => SaveInternal(this, s));
        }

        public void SaveAsync()
        {
            SaveAsync(FilePath);
        }

        public static T Load(string filePath)
        {
            T setting = LoadInternal(filePath, true);

            if (setting != null)
            {
                setting.FilePath = filePath;
            }

            return setting;
        }

        private static bool SaveInternal(object obj, string filePath, bool createBackup)
        {
            string typeName = obj.GetType().Name;
            DebugHelper.WriteLine("{0} save started: {1}", typeName, filePath);

            bool isSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    lock (obj)
                    {
                        Helpers.CreateDirectoryIfNotExist(filePath);

                        string tempFilePath = filePath + ".temp";

                        isSuccess = SaveInternal(obj, new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read));

                        if (File.Exists(filePath))
                        {
                            if (createBackup)
                            {
                                File.Copy(filePath, filePath + ".bak", true);
                            }

                            File.Delete(filePath);
                        }

                        File.Move(tempFilePath, filePath);

                        isSuccess = true;
                    }
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
            finally
            {
                DebugHelper.WriteLine("{0} save {1}: {2}", typeName, isSuccess ? "successful" : "failed", filePath);
            }

            return isSuccess;
        }

        private static bool SaveInternal(object obj, Stream s)
        {
            bool isSuccess = false;

            try
            {
                if (s != null)
                {
                    lock (obj)
                    {
                        using (StreamWriter streamWriter = new StreamWriter(s))
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                        {
                            jsonWriter.Formatting = Formatting.Indented;
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.ContractResolver = new WritablePropertiesOnlyResolver();
                            serializer.Converters.Add(new StringEnumConverter());
                            serializer.Serialize(jsonWriter, obj);
                            jsonWriter.Flush();
                        }

                        isSuccess = true;
                    }
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return isSuccess;
        }

        private static T LoadInternal(string filePath, bool checkBackup)
        {
            string typeName = typeof(T).Name;

            if (!string.IsNullOrEmpty(filePath))
            {
                DebugHelper.WriteLine("{0} load started: {1}", typeName, filePath);

                try
                {
                    if (File.Exists(filePath))
                    {
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            if (fileStream.Length > 0)
                            {
                                T settings;

                                using (StreamReader streamReader = new StreamReader(fileStream))
                                using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                                {
                                    JsonSerializer serializer = new JsonSerializer();
                                    serializer.Converters.Add(new StringEnumConverter());
                                    serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
                                    serializer.Error += (sender, e) => e.ErrorContext.Handled = true;
                                    settings = serializer.Deserialize<T>(jsonReader);
                                }

                                if (settings == null)
                                {
                                    throw new Exception(typeName + " object is null.");
                                }

                                DebugHelper.WriteLine("{0} load finished: {1}", typeName, filePath);

                                return settings;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e, typeName + " load failed: " + filePath);
                }

                if (checkBackup)
                {
                    return LoadInternal(filePath + ".bak", false);
                }
            }

            DebugHelper.WriteLine("{0} not found. Loading new instance.", typeName);

            return new T();
        }
    }
}