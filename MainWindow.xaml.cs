using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

namespace Tasks_05___DNS_und_HTML_Tags
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReadFile();
            
        }
        public static List<Task>TaskListe = new List<Task>();
        public static List<Task<string>> TaskListeString = new List<Task<string>>();
        public void ReadFile()
        {
            // string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Adressen.txt");
            string filePath = "Adressen.txt";
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = "";
               while((line =sr.ReadLine()) != null)
                {
                    NameListBox.Items.Add(line);
                    try
                    {
                        IPListBox.Items.Add(GetIP(line));
                    }
                    catch 
                    {
                        IPListBox.Items.Add("Fehler beim auflösen");

                    }

                    try
                    {
                        TagsListBox.Items.Add(CountTags(GetSourceCode(line)));

                    }
                    catch 
                    {
                        TagsListBox.Items.Add("Fehler beim einlesen");

                    }



                }
            }
            
        }
        public string GetIP(string str)
        {
            string ipAdressen = "";
            IPHostEntry host = Dns.GetHostEntry(str);
            foreach (IPAddress ip in host.AddressList)
            {
                ipAdressen += ip.ToString() + ",";
            } // HTML
            ipAdressen = ipAdressen.Remove(ipAdressen.Length - 1);
            return ipAdressen;
        }
        public (string,Task) GetSourceCode(string adresse)
        {
            if(adresse.Contains(','))               //Wenn mehrere Adressen im String sind, erste nehmen
            {
                adresse = adresse.Split(',')[0];
            }
            
            //TaskListe.Clear();

           
                Task<string> task = Task.Run<string>(() =>
                {
                    if (adresse != null)
                    {
                        HttpClient client = new HttpClient();
                        try
                        {
                            string html = client.GetStringAsync("https://" + adresse).Result;       //yahoo.de liefert statt dem Quelltext nur 'OK\r\n' ?
                            return html;
                        }
                        catch
                        {
                            Task.Run(() => MessageBox.Show(adresse + " konnte nicht gelesen werden"));  //Messagebox blockiert Mainwindow nicht durch Task
                            string html = "error";
                            return html;
                        }
                        
                    }
                    return null;
                });
                TaskListe.Add(task);
                Task.WaitAll(TaskListe.ToArray());

                return (task.Result, task);
          
      
           
        }

        public string CountTags((string source, Task t)tuple)
        {
            if(tuple.source == "error")
            {
                return "Quelltext konnte nicht gelesen werden";
            }
           Task<string> CountTagsTask = tuple.t.ContinueWith(t =>
            {

                Dictionary<string, int> dict = new Dictionary<string, int>();
                string[] zuSuchendeTags = { "<html", "<a", "<img" };

            


                foreach (string tag in zuSuchendeTags)
                {
                    dict.Add(tag, Regex.Matches(tuple.source, @tag).Count);
                }
                //Task.WaitAny(tuple.t);
                string tagString = "";
                foreach (KeyValuePair<string, int> kvp in dict)
                {
                    tagString += kvp.Key + " - " + kvp.Value + ",";
                }
                return tagString;

            });
            return CountTagsTask.Result;
           
        }
    }
}
