﻿using NUnit.Framework;
using Playnite;
using Playnite.MetaProviders;
using Playnite.Models;
using Playnite.Providers.Steam;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.MetaProviders
{
    [TestFixture]
    public class MetadataDownloaderTests
    {
        [Test]
        public void RealSearchTest()
        {          
            var client = new ServicesClient("http://localhost:8080/");
            var provider = new IGDBMetadataProvider(client);
            var downloader = new MetadataDownloader(null, null, null, null, provider);

            // Matches exactly
            var result = downloader.DownloadGameData("Grand Theft Auto IV", "", provider);
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);

            // Roman numerals test
            result = downloader.DownloadGameData("Quake 3 Arena", "", provider);
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("Quake III Arena", result.GameData.Name);

            // THE test
            result = downloader.DownloadGameData("Witcher 3: Wild Hunt", "", provider);
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("The Witcher 3: Wild Hunt", result.GameData.Name);

            // No subtitle test
            result = downloader.DownloadGameData("The Witcher 3", "", provider);
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("The Witcher 3: Wild Hunt", result.GameData.Name);

            // & / and test
            result = downloader.DownloadGameData("Command and Conquer", "", provider);
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
            Assert.AreEqual("Command & Conquer", result.GameData.Name);
        }

        [Test]
        public void SearchTest()
        {
            var igdbProvider = new MockMetadataProvider
            {
                GetSupportsIdSearchHandler = () => false                
            };

            var downloader = new MockMetadataDownloader(null, null, null, null, igdbProvider);

            var dbString = "Quake III Arena";
            igdbProvider.SetGenericHandlers(dbString);
            var result = downloader.DownloadGameData("Quake 3 Arena", "", igdbProvider);
            Assert.AreEqual(dbString, result.GameData.Name);

            dbString = "The Witcher 3: Wild Hunt";
            igdbProvider.SetGenericHandlers(dbString);
            result = downloader.DownloadGameData("Witcher 3: Wild Hunt", "", igdbProvider);
            Assert.AreEqual(dbString, result.GameData.Name);
            result = downloader.DownloadGameData("The Witcher 3", "", igdbProvider);
            Assert.AreEqual(dbString, result.GameData.Name);

            dbString = "Command & Conquer";
            igdbProvider.SetGenericHandlers(dbString);
            result = downloader.DownloadGameData("Command and Conquer", "", igdbProvider);
            Assert.AreEqual(dbString, result.GameData.Name);

            dbString = "Tom Clancy's Rainbow Six: Siege";
            igdbProvider.SetGenericHandlers(dbString);
            result = downloader.DownloadGameData("Tom Clancy's Rainbow Six Siege", "", igdbProvider);
            Assert.AreEqual(dbString, result.GameData.Name);
        }

        [Test]
        public void DirectIdTest()
        {
            var provider = new SteamMetadataProvider();
            var downloader = new MetadataDownloader(provider, null, null, null, null);
            var result = downloader.DownloadGameData("", "578080", provider);            
            Assert.IsNotNull(result.GameData);
            Assert.IsNotNull(result.Image);
        }
    }
}
