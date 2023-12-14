using System;
using System.Collections.Generic;
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
using System.Xml;

namespace Editor.Util
{
    /// <summary>
    /// LoggerView.xaml の相互作用ロジック
    /// </summary>
    public partial class LoggerView : UserControl
    {

        public LoggerView()
        {
            InitializeComponent();

            contextMenu_Info.Checked += (_, _) => MessageFilter_Changes();
            contextMenu_Info.Unchecked += (_, _) => MessageFilter_Changes();
            contextMenu_Warnings.Checked += (_, _) => MessageFilter_Changes();
            contextMenu_Warnings.Unchecked += (_, _) => MessageFilter_Changes();
            contextMenu_Errors.Checked += (_, _) => MessageFilter_Changes();
            contextMenu_Errors.Unchecked += (_, _) => MessageFilter_Changes();
        }

        private void OnClear_Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }

        private void MessageFilter_Changes()
        {
            var filter = 0x0;
            if (contextMenu_Info.IsChecked == true) filter |= (int)MessageType.Info;
            if (contextMenu_Warnings.IsChecked == true) filter |= (int)MessageType.Warning;
            if (contextMenu_Errors.IsChecked == true) filter |= (int)MessageType.Error;
            Logger.SetMessageFilter(filter);
        }
    }
}
