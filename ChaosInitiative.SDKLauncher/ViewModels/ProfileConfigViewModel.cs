using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using ChaosInitiative.SDKLauncher.Models;
using ChaosInitiative.SDKLauncher.Util;
using ReactiveUI;
using Steamworks;

namespace ChaosInitiative.SDKLauncher.ViewModels
{
    public class ProfileConfigViewModel : ReactiveObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; }

        public Profile Profile { get; set; }
        public Mount SelectedMount { get; set; }

        public bool ShowProtonSettings { get { return OperatingSystem.IsLinux(); } }

        // I don't know how to make the view be able to access proton versions
        public IReadOnlyList<int> GetInstalledProtonVersions => ToolsUtil.GetInstalledProtonVersions;

        public ReactiveCommand<Unit, Unit> OnClickClose { get; set; }
        public ReactiveCommand<Unit, Unit> OnClickBrowseMountPath { get; set; }
        public ReactiveCommand<Unit, Unit> OnClickBrowseAdditionalMount { get; set; }

        // Default constructor is used by designer
        public ProfileConfigViewModel() : this(Profile.GetDefaultProfile())
        {
        }

        public ProfileConfigViewModel(Profile profile)
        {
            Activator = new ViewModelActivator();

            Profile = profile;
            
            this.WhenActivated((disposable) =>
            {
                Disposable.Create(() =>
                {
                    
                }).DisposeWith(disposable);
            });
        }
        
        /*
        
        
        public async Task OnClickMountAdd()
        {
            AddMountDialog dialog = new AddMountDialog();

            Mount result = await dialog.ShowDialog<Mount>(MyWindow);

            if(result != null && !Profile.Mounts.ToList().Any(m => m.AppId == result.AppId || m.MountPath == result.MountPath) )
            {
                Profile.Mounts.Add(result);
            }
           
        }

        public void OnClickMountRemove()
        {
            if(SelectedMount != null && Profile.Mounts.Contains(SelectedMount))
            {
                Profile.Mounts.Remove(SelectedMount);
            }
        }

        public void OnClickClose()
        {
            MyWindow.Close();
        }
        */
    }
}