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
    // trzeba pomyśleć nad jedną ważną rzeczą
    // mianowicie sprawdzania tagów początkowych i końcowych
    // nie może być tak, że dla początkowego tagu znajdziemy jakiś inny tag końcowy
    // o tej samej nazwie i zaliczymy to jakoby tag początkowy był poprawnie zamknięty.
    // musimy dlatego więc sprawdzać rodzica tagu początkowego i końcowego !!

    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Xml main_xml_file;
        private List<Line> xml_lines;
        private List<Error> errors_list = new List<Error>();
        private readonly Regex regexLettersNumbers = new Regex("^[a-zA-Z0-9]*$");
        private readonly Regex regexLetters = new Regex("^[a-zA-Z]*$");
        private readonly Regex regexAll = new Regex("^[a-zA-Z0-9.,\"' ]*$");
        private readonly Regex regexAttribute = new Regex("^[a-zA-Z]*=\"([a-zA-Z0-9]*)\"(>| )");
        private readonly Regex regexAttributeName = new Regex("(^[a-zA-Z]+[0-9]*)=");
        private readonly Regex regexAttributeValue = new Regex("\"([a-zA-Z]+[0-9]*)\"");
        private readonly Regex regexTagName = new Regex("^XML[A-Za-z0-9_]*");
        private readonly Regex regexTagNameFirstDigit = new Regex("^[0-9]+([A-Za-z0-9_]*)$");

        public MainWindow()
        {
            InitializeComponent();

        }
        // sprawdzanie deklaracji xml 
        public bool CheckXml(string line)
        {
            // pomyśleć o rozbudowanie trochę tego sprawdzania
            // póki co jest ubogie. 
            var characters = line;
            if (characters[0] == '<' && characters[1] == '?')
            {
                if (characters[line.Length - 1] == '>' && characters[line.Length - 2] == '?')
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        public int CheckAttribute(string line, int index, int lineNumber)
        {
            string subString = line.Substring(index);
            if (regexAttribute.IsMatch(subString.Trim()))
            {
                //Console.WriteLine("D: " + subString);
                return 1;
            }
            else if (subString.Trim().Contains("="))
            {
                return 2;
            }
            else
                return 3;
        }
        private Attribute GetAttribute(string line, int index, int lineNumber)
        {
            // ta metody do przebudowy
            var attr = new Attribute();
            string subString = line.Substring(index);
            if (regexAttribute.IsMatch(subString.Trim()))
            {
                Console.WriteLine("D: " + subString);
                var name_match = regexAttributeName.Match(subString.Trim());
                var attribute_name = name_match.Groups[1].Value;

                var value_match = regexAttributeValue.Match(subString.Trim());
                var attribute_value = value_match.Groups[1].Value;

                attr.AttributeId = 1;
                attr.AttributeName = attribute_name;
                attr.AttributeValue = attribute_value;
            }
            //Console.WriteLine(attr.AttributeName + ": " + attr.AttributeValue);
            return attr;
        }
        private Tag GetTag(string line, int index, int lineNumber)
        {
            string tagName = "";
            Tag tag = new Tag();
            List<Attribute> attr_list = new List<Attribute>();

            for (int i = index; i < line.Length; i++)
            {
                if (line[i] == '>')
                    break;
                else if (line[i] == ' ')
                {
                    // tutaj musimy sprawdzać czy został dodany w takim razie atrybut
                    if (this.CheckAttribute(line, i, lineNumber) == 1) // wszystko OK, jeżeli = 1
                    {
                        var attr = this.GetAttribute(line, i, lineNumber);
                        tag.HasAttribute = true;
                        attr_list.Add(attr); // dodajemy atrybut do listy atrybutów danego tagu.
                        // już wiemy że po spacji jest deklarowany atrybut
                        // tutaj jeszcze wyciągnijmy nazwę i wartość atrybutu
                        // następnie dodajmy go do listy atrybutów
                        break;
                    }
                    else if (this.CheckAttribute(line, i, lineNumber) == 2)
                    {
                        errors_list.Add(new Error() { ErrorName = "Bad attribute declaration", ErrorValue = "Attribute declarations can contain only letters and numbers without spaces", Warning = false, LineNumber = lineNumber });
                    }
                    else if (this.CheckAttribute(line, i, lineNumber) == 3)
                    {
                        tagName += line[i];
                        errors_list.Add(new Error() { ErrorName = "Bad tag declaration", ErrorValue = "Tag declarations cannot contain spaces", Warning = false, LineNumber = lineNumber });
                    }
                }
                else if (regexLetters.IsMatch(line[i].ToString()))
                {
                    tagName += line[i];
                }

            }
            tag.Attributes = attr_list;
            tag.TagName = tagName;
            tag.LineNumber = lineNumber;
            return tag;
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

            //check XML declaration
            if (xmlfile.HasXmlDec == false)
                errors_list.Add(new Error() { LineNumber = 0, ErrorName = "XML declaration", ErrorValue = "No xml declaration found in the document.", Warning = true });

            // check if root element exists
            if (xmlfile.StartingTagsList.First().TagName != xmlfile.EndingTagsList.Last().TagName)
            {
                if (xmlfile.HasXmlDec == true)
                    errors_list.Add(new Error() { LineNumber = 1, ErrorName = "Root element error", ErrorValue = "No root element declaration", Warning = false });
                else
                    errors_list.Add(new Error() { LineNumber = 0, ErrorName = "Root element error", ErrorValue = "No root element declaration", Warning = false });
            }

            // starting tags vs ending tags
            foreach (var tag in xmlfile.StartingTagsList)
            {
                if(tag.TagType == true)
                { // tag type TRUE oznacza root'a
                    Console.WriteLine("FALSE");
                    if (xmlfile.EndingTagsList.Where(t => t.TagName == tag.TagName).FirstOrDefault() == null)
                    {
                        errors_list.Add(new Error() { LineNumber = tag.LineNumber, ErrorName = "Tag error", ErrorValue = "Cannot find ending tag for already decalred starting tag (" + tag.TagName + ")", Warning = false });
                    }
                }
                else
                {
                    if (xmlfile.EndingTagsList.Where(t => t.TagName == tag.TagName && t.ChildLevel == tag.ChildLevel && t.Parent == tag.Parent).FirstOrDefault() == null)
                    {
                        errors_list.Add(new Error() { LineNumber = tag.LineNumber, ErrorName = "Tag error", ErrorValue = "Cannot find ending tag for already decalred starting tag (" + tag.TagName + ")", Warning = false });
                    }
                }
                
            }
            // check tags names - cannot start with number and "XML" word
            foreach(var tag in xmlfile.StartingTagsList)
            {
                if(regexTagNameFirstDigit.IsMatch(tag.TagName))
                {
                    errors_list.Add(new Error() { LineNumber = tag.LineNumber, ErrorName = "Tag error", ErrorValue = "Tag's name cannot start with digit (" + tag.TagName + ")", Warning = false });
                }
                if(regexTagName.IsMatch(tag.TagName))
                {
                    errors_list.Add(new Error() { LineNumber = tag.LineNumber, ErrorName = "Tag error", ErrorValue = "Tag's name cannot start with XML word (" + tag.TagName + ")", Warning = false });

                }
            }

            Console.WriteLine(String.Join("\n", xmlfile.StartingTagsList.Select(a => a.TagName + ":" + a.ChildLevel + ":" + (a.Parent != null ? a.Parent.TagName : "BRAK"))));
            Console.WriteLine("\n KON \n" + String.Join("\n", xmlfile.EndingTagsList.Select(a => a.TagName + ":" + a.ChildLevel + ":" + (a.Parent != null ? a.Parent.TagName : "BRAK"))));

            this.PrintErrors();
        }
        private void PrintErrors()
        {
            if (errors_list.Count() > 0)
            {
                MessageBox.Show("Document contains some errors! You can see details in the table below. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                error_list.ItemsSource = errors_list;

                foreach (var error in errors_list)
                {
                    Console.WriteLine("Line " + error.LineNumber + ": [" + error.ErrorName + "] " + error.ErrorValue);

                    DataGridRow row = (DataGridRow)file_lines.ItemContainerGenerator
                         .ContainerFromIndex(error.LineNumber);

                    row.Background = Brushes.Red;
                    row.Foreground = Brushes.WhiteSmoke;
                }
            }
            else
            {
                MessageBox.Show("No errors found in the document ", "Success", MessageBoxButton.OK, MessageBoxImage.Asterisk);

            }

        }
        private void Parse(List<Line> xml_lines)
        {
            List<Text> texts = new List<Text>();
            List<Tag> startingTags = new List<Tag>();
            List<Tag> endingTags = new List<Tag>();
            List<Error> errorsList = new List<Error>();
            bool HasXmlDec = false;
            int level = 0;
            int tag_id = 1;

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
                bool otherDec = false;
                bool textDec = false;
                bool xmlDec = false;


                string tempTag = "";
                int lineNumber = line.LineNumber;
                //bool hasError = line.HasError;

                var characters = line.Content;
                var charactersNumber = line.Content.Length;
                for (int i = 0; i < charactersNumber; i++)
                {

                    if (characters[i] == '<')
                    { // start
                        otherDec = true;
                        if (characters[i + 1] == '?')
                        { // początek deklaracji Xml
                            if (CheckXml(line.Content))
                                HasXmlDec = true;
                            else
                                HasXmlDec = false;
                        }
                        else if (characters[i + 1] == '/')
                        { // początek tagu zamykającego

                            endingTagDec = true; // zmienna pomocnicza
                            level--;
                            Tag endTag = GetTag(line.Content, i + 2, line.LineNumber);

                            // musimy ustalic temu tagowi jeszcze rodzica
                            var parent_tag = startingTags.Where(t => t.TagId < tag_id && t.ChildLevel == level - 1).OrderByDescending(t => t.TagId).FirstOrDefault();
                            if (parent_tag == null)
                            {
                                endTag.Parent = null;
                                endTag.TagType = true;
                            }
                            else
                                endTag.Parent = parent_tag;

                            endTag.ChildLevel = level;
                            endTag.TagId = tag_id;
                            tag_id++;

                            // przy każdym tagu kończącym obniżamy poziom o 1; 
                            endingTags.Add(endTag);
                            //Console.Write("END:"+endTagName);
                        }
                        else if (characters[i + 1] == ' ')
                            errors_list.Add(new Error() { ErrorName = "Bad tag declaration", ErrorValue = "Tag declarations cannot contain spaces", Warning = false, LineNumber = lineNumber });
                        else if(Char.IsDigit(characters[i+1]))
                            errors_list.Add(new Error() { LineNumber = lineNumber, ErrorName = "Tag error", ErrorValue = "Tag's name cannot start with digit", Warning = false });
                        else
                        {

                            // pierwszy znak to < następny to tekst.
                            // mamy więc do czynienia z tagiem początkowym
                            // pobieramy jego dane i tyle.
                            startingTagDec = true; // zmienna pomocnicza;

                            Tag startTag = GetTag(line.Content, i + 1, line.LineNumber);
                            startTag.ChildLevel = level;
                            startTag.TagId = tag_id;

                            // musimy ustalic temu tagowi jeszcze rodzica
                            var parent_tag = startingTags.Where(t => t.TagId < tag_id && t.ChildLevel == level - 1).OrderByDescending(t => t.TagId).FirstOrDefault();
                            if (parent_tag == null)
                            {
                                startTag.Parent = null;
                                startTag.TagType = true;
                            }
                            else
                                startTag.Parent = parent_tag;

                            tag_id++;
                            level++; // przy każdym tagu otwierającym podbijamy poziom o 1
                            startingTags.Add(startTag);
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
            if (HasXmlDec == true)
                xmlFile.HasXmlDec = true;
            else
                xmlFile.HasXmlDec = false;

            xmlFile.EndingTagsList = endingTags;
            xmlFile.StartingTagsList = startingTags;
            xmlFile.StartingTagsNumber = startingTags.Count();
            xmlFile.EndingTagsNumber = endingTags.Count();

            main_xml_file = xmlFile;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            xml_lines = new List<Line>();
            // Tworzymy listę 
            // otwieramy pole wybory 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xml";
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
                        //HasError = false
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
