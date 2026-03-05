using Laincord.Theme;
using Laincord.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Laincord.Windows
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            PART_LaincordVersion.Text = "Laincord version " + Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)
#if LAINCORD_RC
                + " " + AssemblyInfo.RC_REVISION
#endif
                + "\n";

            string credits = "Laincord is a project by nullptr. Most assets belong to Microsoft, please don't sue!\n\n";

            // Miscellaneous resource credits:
            credits += "==== Credits for resources used in Laincord ==== \n\n";
            credits += "The delete icon in the file attachments editor was made by Laserman.\n";
            credits += "\n\n";

            // Get all scenes:
            credits += "==== Credits for scenes ==== \n\n";
            var scenes = ThemeService.Instance.Scenes;
            foreach (var scene in scenes)
            {
                credits += $"\"{scene.DisplayName}\" was made by {scene.Credit}\n";
            }
            credits += "\n\n";

            CreditsTextbox.Text = credits;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
