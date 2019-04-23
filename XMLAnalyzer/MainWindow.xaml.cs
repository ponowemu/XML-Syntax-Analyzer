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

namespace XMLAnalyzer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Tworzymy listę 
            List<Xml> xml_lines = new List<Xml>();
            // otwieramy pole wybory 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "XML files (*.xml)|*.xml|Text files (*.txt)|*.txt";
            int counter = 0;
            string line; 
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document 
                string filePath = dlg.FileName;
                StreamReader file = new System.IO.StreamReader(filePath);
                while ((line = file.ReadLine()) != null)
                {
                    xml_lines.Add(new Xml() {
                        Content = line,
                        LineNumber = counter,
                        HasError = false
                    });

                    Console.WriteLine(line);
                    counter++;
                }

                file.Close();
            }
            //file_lines.AutoGenerateColumns = false;
            file_lines.ItemsSource = xml_lines;
            
        }
    }
}
