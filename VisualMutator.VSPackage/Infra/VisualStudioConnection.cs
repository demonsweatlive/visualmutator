﻿namespace PiotrTrzpil.VisualMutator_VSPackage.Model
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Windows.Forms;
    using EnvDTE;
    using EnvDTE80;
    using log4net;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Settings;
    using UsefulTools.ExtensionMethods;
    using UsefulTools.Paths;
    using UsefulTools.Wpf;
    using VisualMutator.Infrastructure;

    #endregion

    public class VisualStudioConnection : IHostEnviromentConnection
    {
        private readonly Package _package;

        private readonly DTE2 _dte;

        private readonly SolutionEvents _solutionEvents;

        private BuildEvents _buildEvents;

        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private SettingsManager _settingsManager;

        private readonly Subject<EventType> _subject;


        public VisualStudioConnection(Package package)
        {
            _package = package;
            _dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            _solutionEvents = ((Events2)_dte.Events).SolutionEvents;
            _buildEvents = ((Events2)_dte.Events).BuildEvents;
            _subject = new Subject<EventType>();
        
        }

        public SettingsManager SettingsManager
        {
            get
            {
                return _settingsManager;
            }
        }


        public NativeWindowInfo WindowInfo
        {
            get
            {
                Window vsWindow = _dte.MainWindow;
                return new NativeWindowInfo(new IntPtr(vsWindow.HWnd), vsWindow.Top, 
                    vsWindow.Left, vsWindow.Width, vsWindow.Height);
                    //new IntPtr(vsWindow.HWnd); 
            }
        }

        public IWin32Window GetWindow()
        {
            Window vsWindow = _dte.MainWindow;

            // Get the handle to the non-WPF owner window

            IntPtr ownerWindowHandle = new IntPtr(vsWindow.HWnd); // Get hWnd for non-WPF window

            return new WindowWrapper(ownerWindowHandle);
        }

  
     
        public IObservable<EventType> Events
        {
            get { return _subject; }
        }

    

       
        public void Initialize()
        {
         
           // _buildEvents.OnBuildBegin += () => _subject.OnNext(EventType.HostClosed)
          //  _buildEvents.OnBuildDone += _buildEvents_OnBuildDone;

            _solutionEvents.Opened += () => _subject.OnNext(EventType.HostOpened);
            _solutionEvents.AfterClosing += () => _subject.OnNext(EventType.HostClosed);

            _settingsManager = new ShellSettingsManager(_package);


            if (_dte.Solution.IsOpen)
            {
                _subject.OnNext(EventType.HostOpened);
            }
        }

        public void OpenFile(string className)
        {
            IEnumerable<ProjectItem> projectItems = _dte.Solution.Cast<Project>()
                .SelectMany(p => p.ProjectItems.Cast<ProjectItem>()).ToList();
            ProjectItem projectItem = projectItems.First(i => i.Name == className);
        }


        public string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public IEnumerable<DirectoryPathAbsolute> GetProjectPaths()
        {

            return from project in _dte.Solution.Cast<Project>()
                   let confManager = project.ConfigurationManager
                   where confManager != null
                         && confManager.ActiveConfiguration != null
                         && confManager.ActiveConfiguration.IsBuildable
                   let values = project.Properties.Cast<Property>().ToDictionary(prop => prop.Name)
                   where values.ContainsKey("LocalPath")
                   select values["LocalPath"].Value.CastTo<string>().ToDirPathAbs();
        }
        public IEnumerable<FilePathAbsolute> GetProjectAssemblyPaths()
        {
            return from project in _dte.Solution.Cast<Project>()
                   where project.ConfigurationManager != null
                   let config = project.ConfigurationManager.ActiveConfiguration
                   where config != null && config.IsBuildable
                   let values = project.Properties.Cast<Property>().ToDictionary(p => p.Name)
                   where values.ContainsKey("LocalPath") && values.ContainsKey("OutputFileName")
                   where config.Properties.Cast<Property>().Any(p => p.Name == "OutputPath")
                   let localPath = values["LocalPath"].Value.CastTo<string>()
                   let outputFileName = values["OutputFileName"].Value.CastTo<string>()
                   let outputDir = (string)config.Properties.Cast<Property>()
                        .Single(p => p.Name == "OutputPath").Value
                   select Path.Combine(localPath, outputDir, outputFileName).ToFilePathAbs();
        }

        public string GetMutantsRootFolderPath()
        {
            var slnPath =
                (string)
                _dte.Solution.Properties.Cast<Property>().Single(p => p.Name == "Path").Value;
            return Directory.GetParent(slnPath).CreateSubdirectory("visal_mutator_mutants").FullName;
        }

     

        public class WindowWrapper : IWin32Window
        {
            private IntPtr _hwnd;

            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }

            public IntPtr Handle
            {
                get
                {
                    return _hwnd;
                }
            }
        }
    }
}