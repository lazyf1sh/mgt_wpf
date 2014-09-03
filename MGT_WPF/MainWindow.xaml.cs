using GeoIP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MGT_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region imported winapi clipboard functions

        IntPtr viewerHandle = IntPtr.Zero;
        IntPtr installedHandle = IntPtr.Zero;

        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;

        [DllImport("user32.dll")]
        private extern static IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("user32.dll")]
        private extern static int ChangeClipboardChain(IntPtr hWnd, IntPtr hWndNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                installedHandle = hwndSource.Handle;
                viewerHandle = SetClipboardViewer(installedHandle);
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ChangeClipboardChain(this.installedHandle, this.viewerHandle);
            int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            e.Cancel = error != 0;

            base.OnClosing(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            this.viewerHandle = IntPtr.Zero;
            this.installedHandle = IntPtr.Zero;
            base.OnClosed(e);
        }

        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_CHANGECBCHAIN:
                    this.viewerHandle = lParam;
                    if (this.viewerHandle != IntPtr.Zero)
                    {
                        SendMessage(this.viewerHandle, msg, wParam, lParam);
                    }

                    break;

                case WM_DRAWCLIPBOARD:
                    OnClipboardChanged();

                    if (this.viewerHandle != IntPtr.Zero)
                    {
                        SendMessage(this.viewerHandle, msg, wParam, lParam);
                    }

                    break;

            }
            return IntPtr.Zero;
        }

        #endregion

        private void initApp()
        {
            Checks.checkPcName();
            Checks.checkLicence();
            Checks.checkLocation();
            
            if (!Directory.Exists(Paths.MgtAppdataFolder))
            {
                Directory.CreateDirectory(Paths.MgtAppdataFolder);
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            initApp();

        }

        #region interface

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowLeft = this.Left;
            Properties.Settings.Default.MainWindowTop = this.Top;
            Properties.Settings.Default.MainWindowActualWidth = this.ActualWidth;
            Properties.Settings.Default.MainWindowActualHeight = this.ActualHeight;
            Properties.Settings.Default.Save();
        }


        private HotKey _hotkey;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                _hotkey = new HotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.C, this);
                _hotkey.HotKeyPressed += (k) => startBatchWindows();
            }
            catch (Exception ex)
            {
                textBox_DebugLog.Text = ex.Message + Environment.NewLine + textBox_DebugLog.Text;
                textBox_DebugLog.Text = "Комбинация горячих клавиш занята" + Environment.NewLine + textBox_DebugLog.Text;
            }
            
            
            

            this.Left = Properties.Settings.Default.MainWindowLeft;
            this.Top = Properties.Settings.Default.MainWindowTop;
            this.Width = Properties.Settings.Default.MainWindowActualWidth;
            this.Height = Properties.Settings.Default.MainWindowActualHeight;

            chckbxTopMost.IsChecked = Properties.Settings.Default.MainWindowTopMost;

            
        }

        private void setFlagToForm(string ccode)
        {
            //http://bit.ly/12br8xA
            string tempPath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.User); // /temp/
            string pathToMgtsTempFolder = tempPath + @"\mgt\"; // temp/mgts/

            string downloadFlagURL = @"http://whois.domaintools.com/images/flags/" + ccode + @".gif"; // http://...ru.gif

            string pathForDownloadFlag = pathToMgtsTempFolder + ccode + @".gif"; // /temp/mgts/ru.gif

            Directory.CreateDirectory(pathToMgtsTempFolder);

            bool checkFlagExists = File.Exists(pathForDownloadFlag);

            Uri uri = new Uri(pathForDownloadFlag);


            if (checkFlagExists)
            {
                BitmapImage bitmap = new BitmapImage(uri);
                image_CountryFlag.Source = bitmap;
            }
            else
            {
                WebClient downloadFlagWebClient = new WebClient();
                downloadFlagWebClient.DownloadFile(downloadFlagURL, pathForDownloadFlag);
                BitmapImage bitmap = new BitmapImage(uri);
                image_CountryFlag.Source = bitmap;
            }
        }

        private bool isClipBoardMonitorActive = true;
        private void button_StopStart_Click(object sender, RoutedEventArgs e)
        {
            if (isClipBoardMonitorActive)
            {
                //Unregister clipboard viewer
                ChangeClipboardChain(this.installedHandle, this.viewerHandle);
                button_StopStart.Content = "start";
                isClipBoardMonitorActive = false;
            }
            else
            {
                //register clipboard viewer
                viewerHandle = SetClipboardViewer(installedHandle);
                button_StopStart.Content = "stop";
                isClipBoardMonitorActive = true;
            }
        }

        private void fillForm(IPData ipdata)
        {
            label_IP.Content = ipdata.IPAddress;
            label_Geo.Content = ipdata.Country;
            label_Carrier.Content = ipdata.Carrier;
            label_Org.Content = ipdata.Organisation;
            label_State.Content = ipdata.State;
            label_Sld.Content = ipdata.Sld;
            setFlagToForm(ipdata.CountryCode);

            Table table = new Table();
            table.RowGroups.Add(new TableRowGroup());
            table.RowGroups[0].Rows.Add(new TableRow());

            TableRow tabRow = table.RowGroups[0].Rows[0];

            tabRow.Cells.Add(new TableCell(new Paragraph(new Run("IP"))));
            tabRow.Cells.Add(new TableCell(new Paragraph(new Run(ipdata.IPAddress))));

            table.RowGroups[0].Rows.Add(new TableRow());
            tabRow = table.RowGroups[0].Rows[1];

            tabRow.Cells.Add(new TableCell(new Paragraph(new Run("IP2"))));
            tabRow.Cells.Add(new TableCell(new Paragraph(new Run(ipdata.IPAddress))));

            RichTextBox_History.Document.Blocks.Add(table);
        }

        #endregion

        #region logic

        private async void OnClipboardChanged()
        {
            string clipboardText;

            try
            {
                clipboardText = Clipboard.GetText();
            }
            catch (Exception ex)
            {
                writeToLog(ex.Message);
                return;
            }

            if (clipboardText == previousClipBoardContent)
            {
                writeToLog("[Interrupted] Same data copied.");
                return;
            }
            else
            {
                writeToLog("[Proceed] New data copied.");
                previousClipBoardContent = clipboardText;
            }

            string[] clipboardLines = clipboardText.Split('\n');

            if (clipboardLines.Length > 1)
            {
                return;
            }

            int maxLength = 1024;
            writeToLog(String.Format("Text maxLength: {0}", maxLength.ToString()));
            if (clipboardLines[0].Length > maxLength)
            {
                clipboardLines[0] = clipboardLines[0].Substring(0, maxLength);
            }

            writeToLog(String.Format("Line to go: {0}", clipboardLines[0]));

            IPInfo ipinfo = await getIpData(clipboardLines[0]);
            if (null != ipinfo && null != ipinfo.Data)
            {
                writeToLog(ipinfo.Data.Carrier);
                fillForm(ipinfo.Data);
                switch (ipinfo.Source)
                {
                    case GeoInfoSource.Ram:
                        this.Title = string.Format("MGT [{0}]", ipinfo.Source.ToString());
                        break;
                    case GeoInfoSource.SQLite:
                        this.Title = string.Format("MGT [{0}]", ipinfo.Source.ToString());
                        break;
                    case GeoInfoSource.Api:
                        this.Title = string.Format("MGT [{0}]", ipinfo.Source.ToString());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                writeToLog("ipdata is null");
            }
        }

        private int logIteration = 0;
        private void writeToLog(string log)
        {
            logIteration++;
            log = string.Format("{0}: {1}\n", DateTime.Now.ToString(), log);
            textBox_DebugLog.Text = log + textBox_DebugLog.Text;
        }

        string previousIp = null;
        private async Task<IPInfo> getIpData(string clipboard)
        {
            return await Task<IPInfo>.Factory.StartNew(() =>
            {
                IPInfo ipinfo = new IPInfo(clipboard);
                if (null == ipinfo.Data)
                {
                    return null;
                }
                else
                {

                    previousIp = ipinfo.Data.IPAddress;
                    return ipinfo;
                }
            });
        }

        string previousClipBoardContent = null;

        #endregion

        private void startBatchWindows()
        {
            BatchWindow batchWindow = new BatchWindow();
            batchWindow.Show();
        }

        private void btnBatch_Click(object sender, RoutedEventArgs e)
        {
            startBatchWindows();
        }

        private void chckbxTopMost_Click(object sender, RoutedEventArgs e)
        {
            if (this.Topmost)
            {
                ((CheckBox)sender).IsChecked = false;
                this.Topmost = false;
                Properties.Settings.Default.MainWindowTopMost = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                ((CheckBox)sender).IsChecked = true;
                this.Topmost = true;
                Properties.Settings.Default.MainWindowTopMost = true;
                Properties.Settings.Default.Save();
            }
        }



    }
}
