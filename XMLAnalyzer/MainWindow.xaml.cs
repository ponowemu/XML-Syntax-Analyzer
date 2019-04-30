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
        private Xml main_xml_file;
        private readonly Regex regexLettersNumbers = new Regex("^[a-zA-Z0-9]*$");
        private readonly Regex regexLetters = new Regex("^[a-zA-Z]*$");
        private readonly Regex regexAll = new Regex("^[a-zA-Z0-9.,\"' ]*$");

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
        public string GetTextValue(string line, int index)
        {
            string text = "";
            for (int i = index; i < line.Length; i++)
            {
                if (line[i] == '<' && line[i + 1] == '/')
                    break;
                else
                {
                    if (regexAll.IsMatch(line[i].ToString()))
                        text += line[i];
                }

            }
            return text;
        }
        private void Validate(Xml xmlfile)
        {
            // starting tags vs ending tags
            foreach(var tag in xmlfile.StartingTagsList)
            {
                if (xmlfile.EndingTagsList.Where(t => t.TagName == tag.TagName).FirstOrDefault() == null)
                {
                    Console.WriteLine("Cannot find ending tag for already decalred starting tag");
                    DataGridRow row = (DataGridRow)file_lines.ItemContainerGenerator.ContainerFromIndex(tag.LineNumber);
                    row.Background = Brushes.Red;

                }
            }
        }
        private void Parse(List<Line> xml_lines)
        {
            List<Text> texts = new List<Text>();
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
                bool startingTagDec = false;
                bool endingTagDec = false;
                bool xmlDec = false;
                bool otherDec = false;
                bool textDec = false;

                string tempTag = "";
                int lineNumber = line.LineNumber;
                bool hasError = line.HasError;

                var characters = line.Content;
                var charactersNumber = line.Content.Length;
                for (int i = 0; i < charactersNumber; i++)
                {
                    if (characters[i] == '<')
                    { // start
                        otherDec = true;
                        if (characters[i + 1] == '?')
                        { // początek deklaracji Xml
                            xmlDec = true;
                        }
                        else if (characters[i + 1] == '/')
                        { // początek tagu zamykającego

                            endingTagDec = true; // zmienna pomocnicza

                            string endTagName = GetTagName(line.Content, i + 2);
                            endingTags.Add(new Tag()
                            {
                                TagName = endTagName,
                                LineNumber = lineNumber
                            });
                            //Console.Write("END:"+endTagName);
                        }
                        else if (characters[i + 1] == ' ')
                            errorsList.Add(new Error() { ErrorName = "Bad tag declaration", ErrorValue = "Tag declarations cannot contain spaces", Warning = false, LineNumber = lineNumber });
                        else
                        {
                            startingTagDec = true; // zmienna pomocnicza;

                            string startTagName = GetTagName(line.Content, i + 1);
                            startingTags.Add(new Tag()
                            {
                                TagName = startTagName,
                                LineNumber = lineNumber
                            });
                            //Console.WriteLine("START:"+startTagName);
                        }

                    }
                    else if (characters[i] == '>')
                    { // koniec
                        // czyścimy zmienne pomocnicze
                        startingTagDec = false;
                        xmlDec = false;
                        otherDec = false;
                        endingTagDec = false;

                    }
                    else if (regexLettersNumbers.IsMatch(characters[i].ToString()) && startingTagDec == false && endingTagDec == false && xmlDec == false && otherDec == false)
                    { // chyba tutaj już rozpatrujemy zwykły tekst
                        if (!regexAll.IsMatch(characters[i - 1].ToString()))
                        {
                            string value = GetTextValue(line.Content, i);
                            //Console.WriteLine("TEKST: " + line.Content +  " :TEKST");
                            texts.Add(new Text()
                            {
                                TextId = 1,
                                Value = value
                            });
                            //Console.WriteLine("TE:" + value + "CC");
                        }

                    }
                }
            }

            xmlFile.EndingTagsList = endingTags;
            xmlFile.StartingTagsList = startingTags;
            xmlFile.StartingTagsNumber = startingTags.Count();
            xmlFile.EndingTagsNumber = endingTags.Count();

            main_xml_file = xmlFile;
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
            this.Parse(xml_lines);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Validate(main_xml_file);
        }
    }
}
