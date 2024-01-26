using Editor.Util;
using Editor.GameProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Editor.GameDev
{
    static class VisualStudio
    {
        private static EnvDTE80.DTE2? _vsInstance = null;
        //17.0 is Visual Studio 2022
        private static readonly string _progID = "VisualStudio.DTE.17.0";

        public static bool BuildSucceeded { get; private set; } = true;
        public static bool BuildDone { get; private set; } = true;

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        public static void OpenVisualStudio(string solutionPath)
        {
            IRunningObjectTable? rot = null;
            IEnumMoniker? monikerTable = null;
            IBindCtx? bindCtx = null;
            try
            {
                if (_vsInstance == null)
                {
                    // Finde and open visual studio
                    var hResult = GetRunningObjectTable(0, out rot);
                    if (hResult < 0 || rot == null) throw new COMException($"GetRunningObjectTable() returned HRESULT: {hResult:X8}");

                    // Get a snapshot of the running objects table.
                    rot.EnumRunning(out monikerTable);
                    monikerTable.Reset();

                    // Get a bind context object.
                    hResult = CreateBindCtx(0, out bindCtx);
                    if (hResult < 0 || bindCtx == null) throw new COMException($"CreateBindCtx() returned HRESULT: {hResult:X8}");

                    // Get the names of all running instances of Visual Studio.
                    IMoniker[] currentMoniker = new IMoniker[1];
                    while (monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
                    {
                        // Get the name of the current running Visual Studio instance.
                        string name = string.Empty;
                        currentMoniker[0]?.GetDisplayName(bindCtx, null, out name);
                        if (name.Contains(_progID))
                        {
                            // Get the instance of Visual Studio corresponding to the ROT entry.
                            hResult = rot.GetObject(currentMoniker[0], out object obj);
                            if (hResult < 0 || obj == null) throw new COMException($"Running object table's GetObject() returned HRESULT: {hResult:X8}");

                            // Check if the solution is already open
                            // If the solution is already open, activate it
                            EnvDTE80.DTE2? dte = obj as EnvDTE80.DTE2;
                            var solutionName = dte.Solution.FullName;
                            if (solutionName == solutionPath)
                            {
                                _vsInstance = dte;
                                break;
                            }
                        }
                    }

                    // If the solution is not open, open it
                    if (_vsInstance == null)
                    {
                        Type? visualStudioType = Type.GetTypeFromProgID(_progID, true);
                        _vsInstance = Activator.CreateInstance(visualStudioType) as EnvDTE80.DTE2;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, "failed to open Visual Studio");
            }
            finally
            {
                if (monikerTable != null) Marshal.ReleaseComObject(monikerTable);
                if (rot != null) Marshal.ReleaseComObject(rot);
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
            }
        }

        public static void CloseVisualStudio()
        {
            if (_vsInstance?.Solution.IsOpen == true)
            {
                _vsInstance.ExecuteCommand("File.SaveAll");
                _vsInstance.Solution.Close(true);
            }
            _vsInstance?.Quit();
        }

        public static bool AddFilesToSolution(string solution, string projectName, string[] files)
        {
            //Opens an instance of VS with the given solution, managing multiple solutions.
            Debug.Assert(files?.Length > 0);
            OpenVisualStudio(solution);

            try
            {
                if (_vsInstance != null)
                {
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                    else _vsInstance.ExecuteCommand("File.SaveAll");

                    foreach (EnvDTE.Project project in _vsInstance.Solution.Projects)
                    {
                        if (project.UniqueName.Contains(projectName))
                        {
                            //Adding a files to the solution
                            foreach (var file in files)
                            {
                                project.ProjectItems.AddFromFile(file);
                            }
                        }
                    }

                    //OpenFile can only open files that have already been added, so it is executed after AddFromFile
                    var cpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".cpp");
                    var hpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".h");
                    if (!string.IsNullOrEmpty(cpp)) _vsInstance.ExecuteCommand("File.OpenFile", cpp);
                    if (!string.IsNullOrEmpty(hpp)) _vsInstance.ExecuteCommand("File.OpenFile", hpp);

                    _vsInstance.MainWindow.Activate();
                    _vsInstance.MainWindow.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("failed to add files to Visual Studio project");
                return false;
            }
            return true;
        }

        private static void OnBuildSolutionBegin(string project, string projectConfig, string platform, string solutionConfig)
        {
            Logger.Log(MessageType.Info, $"Building {project}, {projectConfig}, {platform}, {solutionConfig}");
        }

        private static void OnBuildSolutionDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
        {
            if (BuildDone) return;

            if (success) Logger.Log(MessageType.Info, $"Building {projectConfig} configuration succeeded");
            else Logger.Log(MessageType.Error, $"Building {projectConfig} configuration failed");

            BuildDone = true;
            BuildSucceeded = success;
        }

        public static bool IsDebugging()
        {
            bool result = false;

            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    result = _vsInstance != null &&
                        (_vsInstance.Debugger.CurrentProgram != null || _vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    if (!result) System.Threading.Thread.Sleep(1000);
                }
            }
            return result;
        }

        public static void BuildSolution(Project project, string configName, bool showWindow = true)
        {

            // If VS is currently running a process, don't build
            if (IsDebugging())
            {
                Logger.Log(MessageType.Error, "Visual Studio is currenty running a process.");
                return;
            }

            // Opens an instance of VS with the given solution, managing multiple solutions.
            OpenVisualStudio(project.Solution);
            BuildDone = BuildSucceeded = false;

            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    if (_vsInstance == null) throw new Exception("Visual Studio is not running");

                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(project.Solution);
                    _vsInstance.MainWindow.Visible = showWindow;

                    _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildSolutionBegin;
                    _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += OnBuildSolutionDone;

                    // If a pbd is generated for each build, the number of files will be huge, so delete it each time
                    try
                    {
                        foreach (var pdbFile in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64\{configName}"), "*.pdb"))
                        {
                            File.Delete(pdbFile);
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.Message); }

                    _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(configName).Activate();
                    _vsInstance.ExecuteCommand("Build.BuildSolution");

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine($"Attempt {i}: failed to build {project.Name}");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

    }
}
