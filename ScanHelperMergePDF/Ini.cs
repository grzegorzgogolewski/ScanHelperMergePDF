using System;
using System.Collections.Specialized;
using System.Configuration;

namespace ScanHelperMergePDF
{
    public static class Ini
    {
        public static void ReadAllSettings()  
        {  
            try  
            {  
                var appSettings = ConfigurationManager.AppSettings;  

                if (appSettings.Count == 0)  
                {  
                    Console.WriteLine("AppSettings is empty.");  
                }  
                else  
                {  
                    foreach (string key in appSettings.AllKeys)  
                    {  
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);  
                    }  
                }  
            }  
            catch (ConfigurationErrorsException)  
            {  
                Console.WriteLine("Error reading app settings");  
            }  
        }  

        public static string ReadSetting(string key)  
        {  
            try  
            {  
                NameValueCollection appSettings = ConfigurationManager.AppSettings;  

                return appSettings[key] ?? string.Empty;

            }  
            catch (ConfigurationErrorsException)
            {
                return string.Empty;
            }  
        } 

        public static int SaveSetting(string key, string value)
        {
            int result;

            try
            {
                Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;

                if (settings[key] == null)
                {
                    settings.Add(key, value);
                    result = 0;
                }
                else
                {
                    settings[key].Value = value;
                    result = 1;
                }

                configFile.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                result = -1;
            }

            return result;
        }
    }
}
