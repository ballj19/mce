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
using System.Windows.Shapes;

namespace mods
{
    /// <summary>
    /// Interaction logic for ProgramMotion.xaml
    /// </summary>
    public partial class ProgramMotion : Window
    {
        CanUsbComponentClass canPort;

        public ProgramMotion()
        {
            InitializeComponent();
            Window_Load();
        }

        private Window_Load()
        {
            // Populate USB Ports
            fillSerialNumberComboBox();
        }

        private void fillSerialNumberComboBox()
        {
            CanUSB.Items.Clear();
            // Populate USB Ports
            string[] adaptors = canPort.AdaptorSerialNumbers;
            if (adaptors.Length >= 1)
            {
                // Then we have found CANUSB adaptors. Add them to the dropdown and Select the first one
                foreach (string serialnumber in adaptors)
                {
                    comboBoxUsbPorts.Items.Add(serialnumber);
                }
                comboBoxUsbPorts.SelectedIndex = 0;
            }
            else
            {
                // No adaptors have been found
                comboBoxUsbPorts.Items.Add("NoneFound");
            }
        }
    }
}
