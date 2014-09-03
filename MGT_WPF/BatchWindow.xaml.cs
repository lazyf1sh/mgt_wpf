using GeoIP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MGT_WPF
{
    /// <summary>
    /// Interaction logic for BatchWindow.xaml
    /// </summary>
    public partial class BatchWindow : Window
    {

        List<VisualIPData> visualIPData_ = new List<VisualIPData>();
        List<Percents> percents_ = new List<Percents>();
        public BatchWindow()
        {

            InitializeComponent();
            lstw.Visibility = System.Windows.Visibility.Collapsed;
            lstwPercents.Visibility = System.Windows.Visibility.Collapsed;
            run();
        }

        private async Task run()
        {
            if (Clipboard.ContainsText())
            {
                string clipboard = Clipboard.GetText();
                string[] lines = clipboard.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                prgrssbr.Maximum = lines.Length;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    prgrssbr.Value = i;
                    string ip = IPUtilities.ExtractFirstIpFromLine(line);
                    if (ip != null)
                    {
                        IPInfo info = await getinfo(ip);//long operation
                        VisualIPData vpdt = new VisualIPData(info.Data, lines[i]);
                        visualIPData_.Add(vpdt);
                    }
                }

                var query = visualIPData_.GroupBy(x => x.Organisation)
                 .Select(g => new { Value = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count);

                //generate percents and colors
                Random random = new Random();
                foreach (var field in query)
                {
                    Percents prcnts = new Percents();
                    prcnts.Field = field.Value;
                    prcnts.Percent = (int)((double)field.Count / (double)visualIPData_.Count * 100);

                    int colorR = random.Next(130, 255);
                    int colorG = random.Next(130, 255);
                    int colorB = random.Next(130, 255);

                    prcnts.BackgroundColor = new SolidColorBrush(Color.FromRgb((byte)colorR, (byte)colorG, (byte)colorB));
                    percents_.Add(prcnts);
                }

                //assign colors based on field.Value
                foreach (VisualIPData data in visualIPData_)
                {
                    foreach (Percents percents in percents_)
                    {
                        if (data.Organisation == percents.Field)
                        {
                            data.BackgroundColor = percents.BackgroundColor;
                        }
                    }
                }

                lstw.ItemsSource = visualIPData_;
                lstwPercents.ItemsSource = percents_;
                lstw.Visibility = System.Windows.Visibility.Visible;
                lstwPercents.Visibility = System.Windows.Visibility.Visible;

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
            this.Topmost = Properties.Settings.Default.BatchWindowTopMost;

            chckbxTopMost.IsChecked = Properties.Settings.Default.BatchWindowTopMost;
        }


        public class VisualIPData
        {
            public VisualIPData(IPData ipdata, string Clipdata_)
            {
                IPData _ipdata = ipdata.ShallowCopy();

                this.IPAddress = _ipdata.IPAddress;
                this.City = _ipdata.City;
                this.Country = _ipdata.Country;
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
            public string FlagPath
            {
                get
                {
                    string path = string.Format("{0}/flags-ico/{1}.ico", AppDomain.CurrentDomain.BaseDirectory, this.CountryCode.ToUpper());
                    return path;
                }
                set
                {
                    ;
                }
            }

            public string State { get; set; }
            public string Sld { get; set; }

            public string Clipdata { get; private set; }

            public SolidColorBrush BackgroundColor { get; set; }
        }

        public class Percents
        {
            public string Field { get; set; }
            public int Percent { get; set; }
            public SolidColorBrush BackgroundColor { get; set; }
        }

        private void chckbxTopMost_Click(object sender, RoutedEventArgs e)
        {
            
            if (this.Topmost)
            {
                ((CheckBox)sender).IsChecked = false;
                this.Topmost = false;
                Properties.Settings.Default.BatchWindowTopMost = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                ((CheckBox)sender).IsChecked = true;
                this.Topmost = true;
                Properties.Settings.Default.BatchWindowTopMost = true;
                Properties.Settings.Default.Save();
            }
            
        }
    }
}
