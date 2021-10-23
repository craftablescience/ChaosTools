using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ChaosInitiative.SDKLauncher.Models;
using ChaosInitiative.SDKLauncher.Util;
using ChaosInitiative.SDKLauncher.ViewModels;
using MessageBox.Avalonia;
using ReactiveUI;
using Steamworks;

namespace ChaosInitiative.SDKLauncher.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public static MainWindow Instance;

        protected Button EditProfileButton => this.FindControl<Button>("EditProfileButton");
        protected Button OpenToolsModeButton => this.FindControl<Button>("OpenToolsModeButton");
        protected Button OpenGameButton => this.FindControl<Button>("OpenGameButton");

        public Profile CurrentProfile => ViewModel.CurrentProfile;

        private string HammerArguments
        {
            get
            {
                string arguments;

                var additionalMount = CurrentProfile.AdditionalMount;
                
                if (additionalMount != null &&
                    !string.IsNullOrWhiteSpace(additionalMount.PrimarySearchPathDirectory) &&
                    !additionalMount.PrimarySearchPathDirectory.Equals("/") &&
                    !additionalMount.PrimarySearchPathDirectory.Equals("\\"))
                {
                    arguments = $"-mountmod \"{additionalMount.PrimarySearchPathDirectory}\"";
                }
                else
                {
                    arguments = "-nop4"; // Chaos doesn't need these, but valve games do
                }

                return arguments;
            }
        }

        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            Instance = this;

            this.WhenActivated(disposables =>
            {
                // This sets up all the correct paths to use to launch the tools and game
                // Note: This looks like it won't work here, but it really does, I promise
                //       Moving it outside this function crashes the application on load
                //InitializeSteamClient((uint)ViewModel.CurrentProfile.Mod.Mount.AppId);
                
                ViewModel.OnClickEditProfile.Subscribe(_ => EditProfile()).DisposeWith(disposables);
                ViewModel.OnClickOpenHammer.Subscribe(_ => 
                    LaunchTool(
                        "hammer",
                        Tools.Hammer
                    )
                ).DisposeWith(disposables);
                ViewModel.OnClickOpenModelViewer.Subscribe(_ =>
                    LaunchTool(
                        "hlmv",
                        Tools.ModelViewer,
                        true,
                        ViewModel.CurrentProfile.Mod.Mount.MountPath,
                        allowProton:false
                    )
                ).DisposeWith(disposables);

                ViewModel.OnClickLaunchGame.Subscribe(_ => LaunchGame());
            });

            Closing += OnClosing;
        }

        private void LaunchTool(string executableName, Tools tool, bool windowsOnly = false, string workingDir = null, string binDir = null, bool allowProton = true)
        {
            binDir ??= CurrentProfile.Mod.Mount.BinDirectory;
            workingDir ??= binDir;

            if (windowsOnly && allowProton && OperatingSystem.IsLinux())
            {
                binDir = binDir.Replace("linux64", "win64");
                workingDir = binDir;
            }
            
            string args = tool switch
            {
                Tools.Hammer => HammerArguments,
                Tools.ModelViewer => $"-game {CurrentProfile.Mod.Mount.PrimarySearchPath}",
                _ => ""
            };

            try
            {
                ToolsUtil.LaunchTool(binDir, executableName, args, windowsOnly, workingDir, allowProton);
            }
            catch (ToolsLaunchException e)
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Failed to launch tool: " + e.Message).Show();
            }
        }

        private void LaunchGame()
        {
            var launchGame = new LaunchGameWindow(CurrentProfile);
            launchGame.ShowDialog(this);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            ViewModel.Config.Save();
        }

        private void EditProfile()
        {
            var profileConfigWindow = new ProfileConfigWindow
            {
                DataContext = new ProfileConfigViewModel(CurrentProfile)
            };
            profileConfigWindow.ShowDialog(this);
        }

        private enum Tools
        {
            Hammer,
            ModelViewer
        }
    }
}