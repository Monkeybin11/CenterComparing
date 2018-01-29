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
using MahApps.Metro.Controls;
using System.IO;


namespace CenterComparing
{
    using static XmlTool;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Core CoreMain;
        List<MultiAnalysisDatacs> MulDatas;
        Config CurrentConfig = null;

        public MainWindow()
        {
            InitializeComponent();
           
            CoreMain = new Core();

            imgWindow.evtCurrentPos += x => this.BeginInvoke(() => lblPos.Content = x);
            imgWindow.evtDropFile +=  x => imgWindow.SetImage( CoreMain.LoadImageFromDrop(x) );
            imgWindow.evtImgWidthHeight += (w, h) => this.BeginInvoke( () => lblImgResolution.Content = w.ToString() + " x " + h.ToString());

            CoreMain.evtProcessedImg += x => imgWindow.SetImage(x);
            CoreMain.evtProcessedImg += x => imgWindow.RemoveRect();
            CoreMain.evtDistance += x =>
            {
                if (x == double.MaxValue)
                {
                    this.Dispatcher.BeginInvoke((Action)(() => txbDis.Text = "Fail"));
                }
                else
                {
                    this.Dispatcher.BeginInvoke((Action)(() => txbDis.Text = x.ToString("F4")));
                }
            };
            CoreMain.evtNumError += (num,err) => UpdateMultiStatus(num, err);
            CoreMain.evtMultiStart += () => this.Dispatcher.BeginInvoke((Action)(()=> { prbMain.IsIndeterminate = true; btnLoadMultiImg.IsEnabled = false; }));
            CoreMain.evtMultiEnd += () => this.Dispatcher.BeginInvoke((Action)(() => { prbMain.IsIndeterminate = false; btnLoadMultiImg.IsEnabled = true; }));

            imgWindow.SetMargin ( (int)nudMargin.Value );

            MulDatas = new List<MultiAnalysisDatacs>();

            Topmost = false;

            DataContext = this;

            dtgMain.ItemsSource = MulDatas;
        }

        public void Btn_Function_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var master = sender as Button;
                switch (master.Name)
                {
                    case "btnLoadImg":
                        imgWindow.SetImage(CoreMain.LoadImage());
                        break;

                    case "btnStart":

                        Config cfg = null;
                        if ((bool)tglConfig.IsChecked)
                        {
                            cfg = imgWindow.GetConfig(
                           (double)nudPixelResolution.Value,
                           (int)nudThreshold.Value);
                        }
                        else
                        {
                            cfg = CurrentConfig;
                           
                        }
                        if (cfg == null)
                        {
                            MessageBox.Show("Config is not setted");
                            return;
                        }
                        CoreMain.StartProcessing(cfg);
                        break;

                    case "btnClear":
                        imgWindow.Reset();
                        break;


                    case "btnSaveImg":
                        CoreMain.SaveImg();
                        break;

                    case "btnLoadConfig":
                        CurrentConfig = null;

                     
                        System.Windows.Forms.OpenFileDialog ofd1 = new System.Windows.Forms.OpenFileDialog();
                        ofd1.Filter = "Xml Config Files (*.xml) | *.xml";
                        if (ofd1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            CurrentConfig = ReadXmlClas<Config>(CurrentConfig, ofd1.FileName);
                        }

                        break;

                    case "btnExportConfig":
                        Config tempconfg = null;
                        if ((bool)tglConfig.IsChecked)
                        {
                            tempconfg = imgWindow.GetConfig(
                           (double)nudPixelResolution.Value,
                           (int)nudThreshold.Value);
                        }
                        else
                        {
                            tempconfg = CurrentConfig;
                            
                        }
                        if (tempconfg == null)
                        {
                            MessageBox.Show("Config is not setted");
                            return;
                        }
                        System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                        sfd.Filter = "Xml Config Files (*.xml) | *.xml";
                        if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            WriteXmlClass<Config>(tempconfg, sfd.FileName);
                        }

                        break;

                    case "btnApplyMouseConfig":
                        CurrentConfig = imgWindow.GetConfig(
                           (double)nudPixelResolution.Value,
                           (int)nudThreshold.Value);
                        break;


                    case "btnLoadMultiImg":
                        MulDatas = CoreMain.LoadImageMulti();
                        dtgMain.ItemsSource = MulDatas;
                        dtgMain.Items.Refresh();
                        
                        break;

                    case "btnMultiStart":
                        Config tempconfg2 = null;
                        if ((bool)tglConfig.IsChecked)
                        {
                            tempconfg2 = imgWindow.GetConfig(
                           (double)nudPixelResolution.Value,
                           (int)nudThreshold.Value);
                        }
                        else
                        {
                            tempconfg2 = CurrentConfig;
                           
                        }
                        if (tempconfg2 == null)
                        {
                            MessageBox.Show("Config is not setted");
                            return;
                        }
                        CoreMain.StartMultiProcessing( MulDatas , tempconfg2);
                        break;
                }
            }
            catch (Exception er)
            {
                System.Windows.MessageBox.Show("Some Error occur. please contact developer.Error Log will be saved. please choose log save path [Error Number 101 ] ");
                var ofd = new System.Windows.Forms.SaveFileDialog();
                ofd.Filter = "Image Files (*.bmp,*.png,*.jpg,*.jpeg) | *.bmp;*.png;*.jpg;*.jpeg";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(ofd.FileName,er.ToString());
                }
            }
        }

        void UpdateMultiStatus(int num, string error)
        {
            this.Dispatcher.BeginInvoke((Action)(()=> {
                MulDatas[num].error = error;
                dtgMain.Items.Refresh();
            }));
          
        }

        private void nudMargin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            imgWindow?.SetMargin((int)nudMargin.Value);
        }

    }
}
