using CodeIDX.Helpers;
using CodeIDX.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeIDX.ViewModels
{
    public class IndexViewModel : ViewModel
    {

        private DateTime _LastFullRefresh;
        private string _IndexDirectory;
        private List<string> _SourceDirectories;
        private string _StorePath;
        private string _Name;

        public DateTime LastFullRefresh
        {
            get
            {
                return _LastFullRefresh;
            }
            set
            {
                if (_LastFullRefresh != value)
                {
                    _LastFullRefresh = value;
                    FirePropertyChanged("LastFullRefresh");
                }
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        public string StorePath
        {
            get
            {
                return _StorePath;
            }
            set
            {
                if (_StorePath != value)
                {
                    _StorePath = value;
                    FirePropertyChanged("StorePath");
                }
            }
        }

        public string IndexDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_IndexDirectory))
                    GenerateValidIndexDirectoryPath();

                return _IndexDirectory;
            }
            set
            {
                _IndexDirectory = value;

                if (!string.IsNullOrEmpty(_IndexDirectory))
                    StorePath = Path.GetDirectoryName(_IndexDirectory);

                FirePropertyChanged("IndexDirectory");
            }
        }

        public string IndexFile { get; private set; }

        public List<string> SourceDirectories
        {
            get
            {
                return _SourceDirectories;
            }
            set
            {
                _SourceDirectories = value;
                FirePropertyChanged("SourceDirectories");
            }
        }

        public List<string> FileFilters { get; set; }

        public IndexViewModel()
        {
            SourceDirectories = new List<string>();
            FileFilters = new List<string> { "*.cs", "*.txt", "*.xml", "*.xaml" };
        }

        internal void AddSourceDirectory(string directoryPath)
        {
            if (!SourceDirectories.Contains(directoryPath))
            {
                SourceDirectories.Add(directoryPath);
                FirePropertyChanged("SourceDirectories");
            }
        }

        internal void SaveIndexFile()
        {
            if (string.IsNullOrEmpty(IndexFile))
                IndexFile = Path.Combine(StorePath, Name + Constants.IndexFileExtension);

            using (FileStream fs = new FileStream(IndexFile, FileMode.Create, FileAccess.Write))
            {
                var indexElement = new XElement("IndexFile",
                    new XAttribute("Name", Name),
                    new XAttribute("IndexDirectory", IndexDirectory),
                    new XAttribute("LastFullRefresh", LastFullRefresh),

                    //File filters
                    new XElement("FileFilters",
                    from filter in FileFilters
                    select new XElement("Filter", Trim(filter))),

                    //Source directories
                    new XElement("SourceDirectories",
                        from dir in SourceDirectories
                        select new XElement("Directory", Trim(dir))));

                indexElement.Save(fs);
            }
        }

        /// <summary>
        /// Trim blanks and end line
        /// </summary>
        private string Trim(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = StringHelper.Trim(text, Environment.NewLine);
            text = StringHelper.Trim(text, "\n");

            return text;
        }

        public static IndexViewModel Load(string indexFile)
        {
            if (string.IsNullOrEmpty(indexFile) || !File.Exists(indexFile))
                return null;

            try
            {
                using (var fs = new FileStream(indexFile, FileMode.Open, FileAccess.Read))
                {
                    var indexElement = XElement.Load(fs);

                    DateTime lastFullRefresh = DateTime.MinValue;
                    string name = string.Empty;
                    string indexDirectory = string.Empty;

                    XAttribute lastFullRefreshAttribute = indexElement.Attribute("LastFullRefresh");
                    XAttribute nameField = indexElement.Attribute("Name");
                    XAttribute indexDirectoryField = indexElement.Attribute("IndexDirectory");

                    if (nameField != null)
                        name = nameField.Value;
                    if (indexDirectoryField != null)
                        indexDirectory = indexDirectoryField.Value;
                    if (lastFullRefreshAttribute != null)
                        DateTime.TryParse(lastFullRefreshAttribute.Value, out lastFullRefresh);

                    var fileFilters = indexElement.Element("FileFilters").Elements("Filter").Select(cur => cur.Value);
                    var sourceDirectories = indexElement.Element("SourceDirectories").Elements("Directory").Select(cur => cur.Value);

                    return new IndexViewModel
                    {
                        IndexFile = indexFile,
                        Name = name,
                        LastFullRefresh = lastFullRefresh,
                        IndexDirectory = indexDirectory,
                        SourceDirectories = new List<string>(sourceDirectories),
                        FileFilters = new List<string>(fileFilters)
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorProvider.Instance.LogError(string.Format("Index could not be loaded: {0}", indexFile), ex);
                return null;
            }
        }

        private void GenerateValidIndexDirectoryPath()
        {
            //don't generate new path when indexDirectory was already set
            if (!string.IsNullOrEmpty(_IndexDirectory))
                return;

            if (string.IsNullOrEmpty(StorePath) || string.IsNullOrEmpty(Name))
                return;

            string directoryPath = Path.Combine(StorePath, string.Format("{0}_{1}", Name, Constants.IndexDirectoryNameSuffix));
            if (!IsValidIndexDirectory(directoryPath))
            {
                for (int num = 1; ; num++)
                {
                    directoryPath = Path.Combine(StorePath, string.Format("{0}{1}_{2}", Name, num, Constants.IndexDirectoryNameSuffix));

                    if (IsValidIndexDirectory(directoryPath))
                        break;
                }
            }

            IndexDirectory = directoryPath;
        }

        private bool IsValidIndexDirectory(string directoryPath)
        {
            return !Directory.Exists(directoryPath) || !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }

    }
}
