﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace EasySave_Graphique
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static RemoteAccess _remoteAccess;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            _remoteAccess = new RemoteAccess();

            Thread Remote = new Thread(_remoteAccess.ServerPart);
            Remote.Start();
            // Load the language setting from the config.json file
            //_remoteAccess.ServerPart();
            string language = LoadLanguageFromConfigFile();
            
            // Set the CultureInfo of the current thread to the loaded language
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

            // Set the Culture property of the Resources class
            EasySave_Graphique.language.Resources.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
        }

        private string LoadLanguageFromConfigFile()
        {
            string configJson = File.ReadAllText("../../../config.json");
            JArray config = JArray.Parse(configJson);
            JObject languageItem = config.Children<JObject>()
                .FirstOrDefault(dict => dict.ContainsKey("Name") && dict["Name"].ToString() == "Lang");
            if (languageItem != null)
            {
                return languageItem["Lang"].ToString();
            }
            return "en"; // default language
        }
    }
}