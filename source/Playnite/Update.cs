﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using System.Windows;

namespace Playnite
{
    public class Update
    {
        public class UpdateData
        {
            public class ReleaseNote
            {
                public string version
                {
                    get; set;
                }

                public string file
                {
                    get; set;
                }
            }

            public string version
            {
                get; set;
            }

            public string url
            {
                get; set;
            }

            public string notesUrlRoot
            {
                get; set;
            }

            public List<ReleaseNote> releases
            {
                get; set;
            }
        }

        public class ReleaseNoteData
        {
            public string Version
            {
                get; set;
            }

            public string Note
            {
                get; set;
            }
        }

        private static string updateDataUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["UpdateUrl"];
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private UpdateData latestData;

        private string updaterPath
        {
            get
            {
                return Path.Combine(Paths.TempPath, "update.exe");
            }
        }

        private string downloadCompletePath
        {
            get
            {
                return Path.Combine(Paths.TempPath, "download.done");
            }
        }

        public List<ReleaseNoteData> LatestReleaseNotes
        {
            get; set;
        }

        public bool IsUpdateAvailable
        {
            get
            {
                return GetLatestVersion().CompareTo(GetCurrentVersion()) > 0;
            }
        }

        public void DownloadReleaseNotes()
        {
            LatestReleaseNotes = new List<ReleaseNoteData>();
            var current = GetCurrentVersion();

            foreach (var version in latestData.releases)
            {
                if ((new Version(version.version)).CompareTo(current) > 0)
                {
                    var noteUrl = latestData.notesUrlRoot + version.file;
                    var note = Web.DownloadString(noteUrl);
                    LatestReleaseNotes.Add(new ReleaseNoteData()
                    {
                        Version = version.version,
                        Note = note
                    });
                }
            }
        }           

        public void DownloadUpdate()
        {
            if (latestData == null)
            {
                GetLatestVersion();
            }

            DownloadUpdate(latestData.url);
        }

        public void DownloadUpdate(string url)
        {
            logger.Info("Downloading new update from " + url);
            Directory.CreateDirectory(Paths.TempPath);

            if (File.Exists(downloadCompletePath) && File.Exists(updaterPath))
            {                
                var info = FileVersionInfo.GetVersionInfo(updaterPath);
                if (info.FileVersion == GetLatestVersion().ToString())
                {
                    logger.Info("Update already ready to install");
                    return;
                }
                else
                {
                    File.Delete(downloadCompletePath);
                }
            }

            Web.DownloadFile(url, updaterPath);
            File.Create(downloadCompletePath);
        }

        public void InstallUpdate()
        {            
            var portable = Settings.IsPortable ? "/Portable 1" : "/Portable 0";
            logger.Info("Installing new update to {0}, in {1} mode", Paths.ProgramFolder, portable);

            Task.Factory.StartNew(() =>
            {
                Process.Start(updaterPath, string.Format(@"/ProgressOnly 1 {0} /D={1}", portable, Paths.ProgramFolder));
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }     
        
        public Version GetLatestVersion()
        {
            var dataString = Web.DownloadString(updateDataUrl);
            latestData = JsonConvert.DeserializeObject<Dictionary<string, UpdateData>>(dataString)["stable"];
            return new Version(latestData.version);
        }

        public static Version GetCurrentVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        }
    }
}
