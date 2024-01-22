using Editor.Util;
using Editor.GameProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Editor.GameDev
{
    /// <summary>
    /// NewScriptDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class NewScriptDialog : Window
    {
        //TODO: To be formatted to read from a text file.
        private static readonly string _cppCode = @"#include ""{0}.h""
namespace {1} {{
REGISTER_SCRIPT({0});
void {0}::begin_play()
{{
}}
void {0}::update(float dt)
{{
}}
}} // namespace {1}";

        private static readonly string _hCode = @"#pragma once
namespace {1} {{
class {0} : public aur::script::entity_script
{{
public:
    constexpr explicit {0}(aur::game_entity::entity entity)
        : aur::script::entity_script{{entity}} {{}}
    void begin_play() override;
    void update(float dt) override;
private:
}};
}} // namespace {1}";

        private static readonly string _namespace = GetNamespaceFromProjectName();

        public NewScriptDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            scriptPath.Text = @"GameCode\";
        }

        private static string GetNamespaceFromProjectName()
        {
            //Spaces are not allowed in project names, so replaced them with _.
            var projectName = Project.Current.Name;
            projectName = projectName.Replace(' ', '_');
            return projectName;
        }

        bool Validate()
        {
            bool isValid = false;
            var name = scriptName.Text.Trim();
            var path = scriptPath.Text.Trim();
            string errorMsg = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                errorMsg = "Type in a script name.";
            }
            else if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || name.Any(x => char.IsWhiteSpace(x)))
            {
                errorMsg = "Invalid character(s) used in script name.";
            }
            else if (string.IsNullOrEmpty(path))
            {
                errorMsg = "Select a valid script folder";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                errorMsg = "Invalid character(s) used in script path.";
            }
            else if (!Path.GetFullPath(Path.Combine(Project.Current.Path, path)).Contains(Path.Combine(Project.Current.Path, @"GameCode\")))
            {
                errorMsg = "Script must be added to (a sub-folder of) GameCode.";
            }
            else if (File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp"))) ||
                File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h"))))
            {
                errorMsg = $"script {name} already exists in this folder.";
            }
            else
            {
                isValid = true;
            }

            if (!isValid)
            {
                messageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;
            }
            else
            {
                messageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;
            }
            messageTextBlock.Text = errorMsg;
            return isValid;
        }

        private void OnScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validate()) return;
            var name = scriptName.Text.Trim();
            messageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {Project.Current.Name}";
        }

        private void OnScriptPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private async void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            IsEnabled = false;

            //visible --> hidden animation
            busyAnimation.Opacity = 0;
            busyAnimation.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            busyAnimation.BeginAnimation(OpacityProperty, fadeIn);

            try
            {
                //create script files
                var name = scriptName.Text.Trim();
                var path = Path.GetFullPath(Path.Combine(Project.Current.Path, scriptPath.Text.Trim()));
                var solution = Project.Current.Solution;
                var projectName = Project.Current.Name;
                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create script {scriptName.Text}");
            }
            finally
            {
                //hidden --> visible animation
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
                fadeOut.Completed += (s, e) =>
                {
                    busyAnimation.Opacity = 0;
                    busyAnimation.Visibility = Visibility.Hidden;
                    Close();
                };
                busyAnimation.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        private void CreateScript(string name, string path, string solution, string projectName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // writedown the cpp and h files
            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            using (var sw = File.CreateText(cpp))
            {
                sw.Write(string.Format(_cppCode, name, _namespace));
            }
            var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));
            using (var sw = File.CreateText(h))
            {
                sw.Write(string.Format(_hCode, name, _namespace));
            }

            string[] files = new string[] { cpp, h };

            //If it fails once, wait 1 second and retry up to 3 times
            for (int i = 0; i < 3; ++i)
            {
                if (!VisualStudio.AddFilesToSolution(solution, projectName, files)) System.Threading.Thread.Sleep(1000);
                else break;
            }
        }
    }
}

