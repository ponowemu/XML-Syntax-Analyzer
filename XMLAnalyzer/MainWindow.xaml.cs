using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private readonly Regex regexLettersNumbers = new Regex("^[a-zA-Z0-9]*$");
        private readonly Regex regexLetters = new Regex("^[a-zA-Z]*$");

        public MainWindow()
        {
            InitializeComponent();


        }
        public string GetTagName(string line, int index)
        {
            string tagName = "";

            for (int i = index; i < line.Length; i++)
            {
                if (line[i] == ' ' || line[i] == '>')
                    break;
                else if (regexLetters.IsMatch(line[i].ToString()))
                {
                    tagName += line[i];
                }

            }

            return tagName;
        }
        private void Validate(List<Line> xml_lines)
        {
            List<Tag> startingTags = new List<Tag>();
            List<Tag> endingTags = new List<Tag>();
            List<Error> errorsList = new List<Error>();

            var xmlFile = new Xml()
            {
                HasError = false,
                HasRoot = false,
                LineNumber = xml_lines.Count()
            };

            // walidujemy poprawność każdej linijki.
            foreach (var line in xml_lines)
            {

                string tempTag = "";
                int lineNumber = line.LineNumber;
                bool hasError = line.HasError;

                var characters = line.Content;
                var charactersNumber = line.Content.Length;
                for (int i = 0; i < charactersNumber; i++)
                {
                    if (characters[i] == '<')
                    { // start

                        if (characters[i + 1] == '?')
                        { // początek deklaracji Xml

                        }
                        else if (characters[i + 1] == '/')
                        { // początek tagu zamykającego
                            string endTagName = GetTagName(line.Content, i + 2);
                            endingTags.Add(new Tag() {
                                TagName = endTagName
                            });
                            Console.Write("END:"+endTagName);
                        }
                        else if (characters[i + 1] == ' ')
                            errorsList.Add(new Error() { ErrorName = "Bad tag declaration", ErrorValue = "Tag declarations cannot contain spaces", Warning = false, LineNumber = lineNumber });
                        else
                        {
                            string startTagName = GetTagName(line.Content, i + 1);
                            startingTags.Add(new Tag() { TagName = startTagName });
                            Console.WriteLine("START:"+startTagName);
                        }

                    }
                    else if (characters[i] == '>')
                    { // koniec

                    }
                    else if (regexLettersNumbers.IsMatch(characters[i].ToString()))
                    { // chyba tutaj już rozpatrujemy zwykły tekst

                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Tworzymy listę 
            List<Line> xml_lines = new List<Line>();
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
                    xml_lines.Add(new Line()
                    {
                        Content = line,
                        LineNumber = counter,
                        HasError = false
                    });
                    counter++;
                }

                file.Close();
            }
            //file_lines.AutoGenerateColumns = false;
            file_lines.ItemsSource = xml_lines;
            this.Validate(xml_lines);

        }
    }
}
