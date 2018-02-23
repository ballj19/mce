using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Schedule
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            ClockOut clockout = new ClockOut();
            clockout.Show();

            // Ask the user if they want to allow the session to end
            string msg = string.Format("{0}. End session?", e.ReasonSessionEnding);
            MessageBoxResult result = MessageBox.Show(msg, "Session Ending", MessageBoxButton.YesNo);

            // End session, if specified
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
