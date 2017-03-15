
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml.Linq;
using System.Threading;

namespace M_P_4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.exitBtn.Click += (s, ea) => Application.Current.Shutdown();
            this.clearBtn.Click += (s, ea) => setListsToDefaults();
            this.btnTransform.Click += (s, ea) => btnTransform_Click();

            this.btnTML.Click += ((s, ea) =>
                {
                    setListsToDefaults();
                    string SafeFileName = String.Empty;
                    string file = this.dialogFindFile(ref SafeFileName);
                    if (string.IsNullOrEmpty(file))
                    {
                        System.Media.SystemSounds.Beep.Play();
                        MessageBox.Show("Choose a *.wav file", "No file Chosen");
                    }
                    else
                    {
                        XDocument xmlDocument = null;
                        string TMLSavedFile = Environment.CurrentDirectory + SafeFileName + ".tml";

                        listBox2.ControlUpdater(new Action(() =>
                            {
                                try
                                {
                                    xmlDocument = Master_Project.Transformer.TMLler.createDoc(file);
                                    listBox2.ItemsSource = xmlDocument.Root.DescendantNodesAndSelf().ToList<XNode>();
                                }
                                catch (NullReferenceException nullRefE)
                                {
                                    System.Media.SystemSounds.Exclamation.Play();
                                    MessageBox.Show(String.Format("NullReference Exception Raised:Unable to upadate ListBox's value due to xmlDocument refers to NULL \n Detailes: {0} \n Call Seq: {1}, \n ", nullRefE.Message, nullRefE.StackTrace), "NullReferenceException: Unable to create xmlDocument");
                                }
                                catch (ArgumentException argE)
                                {
                                    System.Media.SystemSounds.Exclamation.Play();
                                    MessageBox.Show(String.Format("Argument Exception Raised: {0} \n Thorwn by: {1} \n Message: {2} \n Sourse: {3} \n Target Method: {4}", argE.Data, argE.InnerException, argE.Message, argE.Source, argE.TargetSite), "ArgumentException: Unsupported file type");
                                }
                            }));
                        if (xmlDocument != null)
                        {
                            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(TMLSavedFile))
                            {
                                writer.WriteLine(xmlDocument);
                                xmlDocument = null;
                            }
                            try
                            {
                                using (Process proc = new Process())
                                {
                                    proc.StartInfo.FileName = "wordpad.exe";
                                    proc.StartInfo.Arguments = TMLSavedFile;
                                    proc.Start();
                                }
                            }
                            catch
                            {
                                using (Process proc = new Process())
                                {
                                    proc.StartInfo.FileName = TMLSavedFile;
                                    proc.Start();
                                }
                            }
                        }

                    }
                });
        }

        #region auxiliary_methods

        private string dialogFindFile(ref string safeFileName)
        {
            Microsoft.Win32.OpenFileDialog openfile = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Wav audio files|*.wav|All files|*.*",
                Multiselect = false,
                InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (!openfile.ShowDialog().Value) return null;
            safeFileName = openfile.SafeFileName;
            return openfile.FileName;
        }

        private void btnTransform_Click()
        {
            string SafeFileName = null;
            string file = this.dialogFindFile(ref SafeFileName);
            if (string.IsNullOrEmpty(file))
            {
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Choose a *.wav file", "No file Chosen");
            }
            else
            {
                listBox1.Visibility = Visibility.Visible;
                btnTransform.IsEnabled = false;
                M_P_4.Transformer.Transformer transformer = new M_P_4.Transformer.Transformer(file);

                new Thread(delegate()
                {
                    listBox1.ControlUpdater(new Action(() =>
                        {
                            string wavIntoLetters = transformer.WAVintoASCIIstringOfBytes(file);
                            string[] toList = transformer.in2Dimarray(wavIntoLetters, 18);
                            foreach (string item in toList)
                            {
                                listBox1.Items.Add(item);
                            }
                        }));
                }).Start();

                new Thread(delegate()
                {
                    listBox1.ControlUpdater(new Action(() =>
                        {
                            string wavIntoLetters2 = transformer.WAVintoBaseHex(file);
                            string[] toList2 = transformer.in2Dimarray(wavIntoLetters2, 2);
                            listBox2.ItemsSource = null;
                            for (int i = 0; i < toList2.Length; i++)
                            {
                                listBox2.Items.Add(String.Format("{0}   0x:{1}", i + 1, toList2[i]));
                            }
                        }));
                }).Start();
            }
        }


        private void setListsToDefaults()
        {
            if (listBox1.Visibility != System.Windows.Visibility.Collapsed)
                listBox1.Visibility = System.Windows.Visibility.Collapsed;
            if (!btnTransform.IsEnabled)
                btnTransform.IsEnabled = true;
            if (listBox1.Items.Count > 0)
            {
                listBox1.Items.Clear();
                listBox1.ItemsSource = null;
            }
            if (listBox2.Items.Count > 0)
            {
                (listBox2 as ItemsControl).ItemsSource = null;
                listBox2.Items.Clear();
            }
        }


        #endregion
    }
    public static class ControlExtender
    {
        public static void ControlUpdater(this Control control, Action delegetor)
        {
            if (!control.Dispatcher.CheckAccess())
            {
               control.Dispatcher.BeginInvoke(delegetor);
            }
            else
            {
                delegetor.Invoke();
            }
        }
    }
}
