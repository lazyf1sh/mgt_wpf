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
using GeoIP;

namespace MGT_WPF
{
    /// <summary>
    /// Interaction logic for BatchWindow.xaml
    /// </summary>
    public partial class BatchWindow : Window
    {

        List<VisualIPData> visualIPData = new List<VisualIPData>();

        public BatchWindow()
        {

            InitializeComponent();
            lstw.Visibility = System.Windows.Visibility.Hidden;
            load1();
        }

        private async Task load1()
        {
            if (Clipboard.ContainsText())
            {
                string clipboard = Clipboard.GetText();
                string[] lines = clipboard.Split('\n');

                prgrssbr.Maximum = lines.Length;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    prgrssbr.Value = i;
                    string ip = IPUtilities.ExtractFirstIpFromLine(line);
                    if (ip != null)
                    {
                        IPInfo info = await getinfo(ip);//long operation
                        VisualIPData visualIPData_ = new VisualIPData(info.Data, lines[i]);



                        visualIPData.Add(visualIPData_);
                    }
                }

                lstw.ItemsSource = visualIPData;
                lstw.Visibility = System.Windows.Visibility.Visible;
                prgrssbr.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        public async Task<IPInfo> getinfo(string ip)
        {
            return await Task<IPInfo>.Factory.StartNew(() =>
                 {
                     IPInfo info = new IPInfo(ip);

                     return info;
                 });
        }

        //http://stackoverflow.com/questions/607827/what-is-the-difference-between-width-and-actualwidth-in-wpf
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.BatchWindowLeft = this.Left;
            Properties.Settings.Default.BatchWindowTop = this.Top;
            Properties.Settings.Default.BatchWindowWidth = this.Width;
            Properties.Settings.Default.BatchWindowHeight = this.Height;
            Properties.Settings.Default.Save();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = Properties.Settings.Default.BatchWindowLeft;
            this.Top = Properties.Settings.Default.BatchWindowTop;
            this.Width = Properties.Settings.Default.BatchWindowWidth;
            this.Height = Properties.Settings.Default.BatchWindowHeight;
        }


        public class VisualIPData
        {
            public VisualIPData(IPData ipdata, string Clipdata_)
            {
                IPData _ipdata = ipdata.ShallowCopy();

                this.IPAddress = _ipdata.IPAddress;
                this.City = _ipdata.City;
                this.Organisation = _ipdata.Organisation;
                this.Carrier = _ipdata.Carrier;
                this.CountryCode = _ipdata.CountryCode;
                this.State = _ipdata.State;
                this.Sld = _ipdata.Sld;

                this.Clipdata = Clipdata_;
            }

            public string IPAddress { get; set; }
            public string City { get; set; }
            public string Country { get; set; }
            public string Organisation { get; set; }
            public string Carrier { get; set; }
            public string CountryCode { get; set; }
            public string State { get; set; }
            public string Sld { get; set; }

            public string Clipdata { get; private set; }

            Brush BackgroundColor { get; set; }
        }
    }
}
