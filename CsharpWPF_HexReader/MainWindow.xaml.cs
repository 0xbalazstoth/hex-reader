using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace CsharpWPF_HexReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _openedFile;
        public int clickCounter;
        public string changedSystemBtnText;

        public string OpenedFile
        {
            get
            {
                return _openedFile;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.FontFamily = new FontFamily("Fairfax SM");
        }

        private void appBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(1);
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void MakeVisibleHiddenItems()
        {
            Dispatcher.Invoke(new Action(() => {
                scrllViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                lblOffsetHeader.Visibility = Visibility.Visible;
                lblOpenedFileName.Visibility = Visibility.Visible;
                hdrOpenedFileName.Visibility = Visibility.Visible;
            }), DispatcherPriority.ContextIdle);
        }

        public void ClearElements()
        {
            Dispatcher.Invoke(new Action(() => {
                txtBoxBytes.Text = null;
                txtBoxDecodedText.Text = null;
                this.grdLineCounter.Children.Clear();
            }), DispatcherPriority.ContextIdle);
        }

        public void AppendTextToTextBoxes(StringBuilder sbBytes, StringBuilder sbDecodedStrings)
        {
            Dispatcher.Invoke(new Action(() => {
                txtBoxBytes.Text = sbBytes.ToString();
                txtBoxDecodedText.Text = sbDecodedStrings.ToString();
            }), DispatcherPriority.ContextIdle);
        }

        public void ShowLineCount(int lineCounter)
        {
            Label[] labels = new Label[lineCounter];
            int currentLine = 0;
            Dispatcher.Invoke(new Action(() => {
                int marginTop = -20;
                currentLine = -16;
                for (int i = 0; i < labels.Length; i++)
                {
                    string decimalNumber = (currentLine += 16).ToString();
                    int number = int.Parse(decimalNumber);
                    string hex = number.ToString("X8");

                    marginTop += 20;
                    labels[i] = new Label();

                    labels[i].Content = hex;
                    labels[i].Width = 90;
                    labels[i].FontSize = 20;
                    labels[i].FontFamily = new FontFamily("Fairfax SM");
                    labels[i].Foreground = (Brush)new BrushConverter().ConvertFrom("#FF4A4238");
                    labels[i].Margin = new Thickness(0, marginTop, 0, 0);
                    labels[i].SetValue(Grid.RowProperty, i);
                    this.grdLineCounter.Children.Add(labels[i]);
                }
            }), DispatcherPriority.ContextIdle);
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            //Make visible hidden elements
            MakeVisibleHiddenItems();

            //Clear last file content from textbox
            ClearElements();

            //Return readcontent, filename
            Task<(List<string>, string)> tasks = Task.Run(() =>
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Title = "Open file.";
                openFile.Multiselect = false;
                if (openFile.ShowDialog() == true)
                {
                    _openedFile = openFile.FileName;
                }

                //Read bytes
                ReadBytes read = new ReadBytes();
                read.Opened(_openedFile);

                return (read.ReadFileContent, openFile.FileName);
            });

            //Display file name in header and give foreground color
            Dispatcher.Invoke(new Action(() => {
                string fileName = tasks.Result.Item2;

                var fc = new BrushConverter();
                lblOpenedFileName.Content = $"{System.IO.Path.GetFileName(fileName)}";
                lblOpenedFileName.Foreground = (Brush)fc.ConvertFrom("#FF4A4238");
            }), DispatcherPriority.ContextIdle);

            //Make hex string and decoded string
            Dispatcher.Invoke(new Action(() => {
                List<string> ReadContent = tasks.Result.Item1;
                string fileName = tasks.Result.Item2;

                StringBuilder sbBytes = new StringBuilder();
                StringBuilder sbDecodedStrings = new StringBuilder();

                int index = -1;
                int lineCounter = 1;
                foreach (var item in ReadContent)
                {
                    index++;
                    if (index >= 0 && index <= 15)
                    {
                        sbBytes.Append(item + " ");
                        sbDecodedStrings.Append(HexToASCII(item).Replace("\r", ".").Replace("\n", ".").Replace("\0", "."));
                    }
                    else if (index % 16 == 0)
                    {
                        sbBytes.Append("\n");
                        sbDecodedStrings.Append("\n");
                        lineCounter++;
                    }

                    if (!(index >= 0 && index <= 15))
                    {
                        sbBytes.Append(item + " ");
                        sbDecodedStrings.Append(HexToASCII(item).Replace("\r", ".").Replace("\n", ".").Replace("\0", "."));
                    }
                }

                //Show line count on label elements
                ShowLineCount(lineCounter);

                //Append hex and decoded string to textboxes
                AppendTextToTextBoxes(sbBytes, sbDecodedStrings);
            }), DispatcherPriority.ContextIdle);
        }

        public string HexToASCII(string fromHex)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < fromHex.Length; i += 2)
                {
                    string hs = string.Empty;

                    hs = fromHex.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;
                }
                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return "";
        }

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(txtBoxBytes.Text.Replace("\r", "").Replace("\n", "").Replace(" ", ""));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                txtBoxBytes.Text = null;
                txtBoxDecodedText.Text = null;
                lblOpenedFileName.Content = null;
                scrllViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                lblOffsetHeader.Visibility = Visibility.Hidden;
                lblOpenedFileName.Visibility = Visibility.Hidden;
                hdrOpenedFileName.Visibility = Visibility.Hidden;
                scrllViewer.ScrollToTop();
            }), DispatcherPriority.ContextIdle);

            //Clear last file content from textbox
            ClearElements();
        }

        private void txtBoxBytes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DisableSpaceKey(e);
            DisableUndoRedo(e);
        }

        private void txtBoxDecodedText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DisableSpaceKey(e);
            DisableUndoRedo(e);
        }

        public void DisableSpaceKey(KeyEventArgs e)
        {
            //Space kikapcsolása
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        public void DisableUndoRedo(KeyEventArgs e)
        {
            //Ctrl + z & Ctrl + y kikapcsolása
            if (e.Key == Key.Z && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                e.Handled = true;

            if (e.Key == Key.Y && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                e.Handled = true;
        }

        private void btnChangeSystem_Click(object sender, RoutedEventArgs e)
        {
            clickCounter++;

            foreach (var lbl in grdLineCounter.Children.Cast<Label>())
            {
                //Akkor vált, ha páratlan a kattintások száma
                if (clickCounter % 2 != 0)
                {
                    //Hex > Dec
                    int decValue = int.Parse(lbl.Content.ToString(), System.Globalization.NumberStyles.HexNumber);
                    lbl.Content = decValue.ToString();
                    changedSystemBtnText = "Hex";
                }
                //Akkor vált, ha páros a kattintások száma
                else if (clickCounter % 2 == 0)
                {
                    //Dec > Hex
                    string decimalNumber = int.Parse(lbl.Content.ToString()).ToString();
                    int number = int.Parse(decimalNumber);
                    string hex = number.ToString("X8");
                    changedSystemBtnText = "Dec";
                    lbl.Content = hex;
                }
            }

            //Gomb szövegének módosítása
            Dispatcher.Invoke(new Action(() => {
                btnChangeSystem.Content = changedSystemBtnText;
            }), DispatcherPriority.ContextIdle);
        }
    }
}
