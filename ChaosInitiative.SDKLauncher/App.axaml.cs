using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChaosInitiative.SDKLauncher.ViewModels;
using ChaosInitiative.SDKLauncher.Views;
using MessageBox.Avalonia;
using Steamworks;
using System;
using System.IO;

namespace ChaosInitiative.SDKLauncher
{
    public class App : Application
    {
        public override void Initialize()
        {
            // Initialise Steam
            // This is probably a horrible space for it, but for proton settings we *need* to
            // initialise Steam before the profile is written or else we can't check to see
            // what proton versions the end user has installed. Feel free to move this elsewhere!

            // Check AppId from file, if we can't find the file then
            // default to P2CE's appid
            uint AppId;
            if (File.Exists("steam_appid.txt"))
			{
                var text = File.ReadAllLines("steam_appid.txt");
                AppId = uint.Parse(text[0]);
            }
            else
			{
                AppId = 440000;
			}

            InitializeSteamClient(AppId);

            AvaloniaXamlLoader.Load(this);
        }


        private void InitializeSteamClient(uint appId)
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
            {
                throw new Exception("Wrong application lifetime, contact a developer");
            }

            try
            {
                SteamClient.Init(appId);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Steam"))
                    throw;

                Directory.CreateDirectory("logs");
                File.WriteAllText($"logs/steam_error_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log", e.Message);
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Steam Error: Please check that steam is running, and you own the intended app.").Show();
            }
        }


        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            base.OnFrameworkInitializationCompleted();
        }
    }
}