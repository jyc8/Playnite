﻿using Playnite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayniteUI.Controls
{
    public class GameMenu : ContextMenu
    {
        public bool ShowStartSection
        {
            get
            {
                return (bool)GetValue(GameProperty);
            }

            set
            {
                SetValue(GameProperty, value);
            }
        }

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("ShowStartSection", typeof(bool), typeof(GameMenu), new PropertyMetadata(true, ShowStartSectionPropertyChangedCallback));

        private static void ShowStartSectionPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as GameMenu;
            obj.InitializeItems();
        }

        private IResourceProvider resources;
        private GamesEditor editor;

        public IGame Game
        {
            get
            {
                if (DataContext is GameViewEntry entry)
                {
                    return entry.Game;
                }
                else if (DataContext is IEnumerable<GameViewEntry> entries)
                {
                    if (entries.Count() > 0)
                    {
                        return (entries.First() as GameViewEntry).Game;
                    }
                }
                else if (DataContext is IList<object> entries2)
                {
                    if (entries2.Count() > 0)
                    {
                        return (entries2.First() as GameViewEntry).Game;
                    }
                }

                return null;
            }
        }

        public List<IGame> Games
        {
            get
            {
                if (DataContext is IEnumerable<GameViewEntry> entries)
                {
                    if (entries.Count() == 1)
                    {
                        return null;
                    }

                    return entries.Select(a => (a as GameViewEntry).Game).ToList();
                }
                else if (DataContext is IList<object> entries2)
                {
                    if (entries2.Count() == 1)
                    {
                        return null;
                    }

                    return entries2.Select(a => (a as GameViewEntry).Game).ToList();
                }

                return null;
            }
        }

        static GameMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(typeof(GameMenu)));
        }

        public GameMenu() : this(App.GamesEditor)
        {
        }

        public GameMenu(GamesEditor editor)
        {
            this.editor = editor;
            resources = new ResourceProvider();
            DataContextChanged += GameMenu_DataContextChanged;
            InitializeItems();
        }

        private void GameMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InitializeItems();
        }

        public void InitializeItems()
        {
            Items.Clear();

            if (Games != null)
            {
                // Set Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = resources.FindString("FavoriteGame")
                };

                favoriteItem.Click += (s, e) =>
                {
                    editor.SetFavoriteGames(Games, true);
                };

                Items.Add(favoriteItem);

                var unFavoriteItem = new MenuItem()
                {
                    Header = resources.FindString("RemoveFavoriteGame")
                };

                unFavoriteItem.Click += (s, e) =>
                {
                    editor.SetFavoriteGames(Games, false);
                };

                Items.Add(unFavoriteItem);

                // Set Hide
                var hideItem = new MenuItem()
                {
                    Header = resources.FindString("HideGame")
                };

                hideItem.Click += (s, e) =>
                {
                    editor.SetHideGames(Games, true);
                };

                Items.Add(hideItem);

                var unHideItem = new MenuItem()
                {
                    Header = resources.FindString("UnHideGame")
                };

                unHideItem.Click += (s, e) =>
                {
                    editor.SetHideGames(Games, false);
                };

                Items.Add(unHideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("EditGame")
                };

                editItem.Click += (s, e) =>
                {
                    editor.EditGames(Games);
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("SetGameCategory")
                };

                categoryItem.Click += (s, e) =>
                {
                    editor.SetGamesCategories(Games);
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("RemoveGame")
                };

                removeItem.Click += (s, e) =>
                {
                    editor.RemoveGames(Games);
                };

                Items.Add(removeItem);
            }
            else if (Game != null)
            {
                // Play / Install
                if (ShowStartSection)
                {
                    bool added = false;
                    if (Game.IsInstalled)
                    {
                        var playItem = new MenuItem()
                        {
                            Header = resources.FindString("PlayGame"),
                            FontWeight = FontWeights.Bold
                        };

                        playItem.Click += (s, e) =>
                        {
                            editor.PlayGame(Game);
                        };

                        Items.Add(playItem);
                        added = true;
                    }
                    else if (Game.Provider != Provider.Custom)
                    {
                        var installItem = new MenuItem()
                        {
                            Header = resources.FindString("InstallGame"),
                            FontWeight = FontWeights.Bold
                        };

                        installItem.Click += (s, e) =>
                        {
                            editor.InstallGame(Game);
                        };

                        Items.Add(installItem);
                        added = true;
                    }

                    if (added)
                    {
                        Items.Add(new Separator());
                    }
                }

                // Custom Actions
                if (Game.OtherTasks != null && Game.OtherTasks.Count > 0)
                {
                    foreach (var task in Game.OtherTasks)
                    {
                        var taskItem = new MenuItem()
                        {
                            Header = task.Name
                        };

                        taskItem.Click += (s, e) =>
                        {
                            editor.ActivateAction(Game, task);
                        };

                        Items.Add(taskItem);
                    }

                    Items.Add(new Separator());
                }

                // Open Game Location
                if (Game.IsInstalled)
                {
                    var locationItem = new MenuItem()
                    {
                        Header = resources.FindString("OpenGameLocation")
                    };

                    locationItem.Click += (s, e) =>
                    {
                        editor.OpenGameLocation(Game);
                    };

                    Items.Add(locationItem);
                }

                // Create Desktop Shortcut
                var shortcutItem = new MenuItem()
                {
                    Header = resources.FindString("CreateDesktopShortcut")
                };

                shortcutItem.Click += (s, e) =>
                {
                    editor.CreateShortcut(Game);
                };

                Items.Add(shortcutItem);
                Items.Add(new Separator());

                // Toggle Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("RemoveFavoriteGame") : resources.FindString("FavoriteGame")
                };

                favoriteItem.Click += (s, e) =>
                {
                    editor.ToggleFavoriteGame(Game);
                };

                Items.Add(favoriteItem);

                // Toggle Hide
                var hideItem = new MenuItem()
                {
                    Header = Game.Hidden ? resources.FindString("UnHideGame") : resources.FindString("HideGame")
                };

                hideItem.Click += (s, e) =>
                {
                    editor.ToggleHideGame(Game);
                };

                Items.Add(hideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("EditGame")
                };

                editItem.Click += (s, e) =>
                {
                    editor.EditGame(Game);
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("SetGameCategory")
                };

                categoryItem.Click += (s, e) =>
                {
                    editor.SetGameCategories(Game);
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("RemoveGame")
                };

                removeItem.Click += (s, e) =>
                {
                    editor.RemoveGame(Game);
                };

                Items.Add(removeItem);

                // Uninstall
                if (Game.Provider != Provider.Custom)
                {
                    var uninstallItem = new MenuItem()
                    {
                        Header = resources.FindString("UninstallGame")
                    };

                    uninstallItem.Click += (s, e) =>
                    {
                        editor.UnInstallGame(Game);
                    };

                    Items.Add(uninstallItem);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
