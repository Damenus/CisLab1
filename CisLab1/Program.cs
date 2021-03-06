﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;



namespace CisLab1
{

    public static class Extensions
    {
        public static string RAHS(this FileSystemInfo file)
        {
            string rahs = "";
            FileAttributes attributes = file.Attributes;

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                rahs += "r";
            }
            else
            {
                rahs += "-";
            }

            if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
            {
                rahs += "a";
            }
            else
            {
                rahs += "-";
            }

            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                rahs += "h";
            }
            else
            {
                rahs += "-";
            }

            if ((attributes & FileAttributes.System) == FileAttributes.System)
            {
                rahs += "s";
            }
            else
            {
                rahs += "-";
            }
            
             
            return rahs;
        }

        public static string EldestElement(this DirectoryInfo dir)
        {
            DateTime eldest = dir.CreationTime;
            
            

            return eldest.ToString();
        }
    }

    [Serializable]
    class CustomStringComparer : IComparer<string>
    {
        private readonly IComparer<string> _baseComparer;
        public CustomStringComparer(IComparer<string> baseComparer)
        {
            _baseComparer = baseComparer;
        }

        public int Compare(string x, string y)
        {
            if(x.Length > y.Length)
                return 1;
            else if(x.Length < y.Length)
                return -1;
            /*
             if (_baseComparer.Compare(x, y) == 0)
                return 0;

            // "b" comes before everything else
            if (_baseComparer.Compare(x, "b") == 0)
                return -1;
            if (_baseComparer.Compare(y, "b") == 0)
                return 1;

            // "c" comes next
            if (_baseComparer.Compare(x, "c") == 0)
                return -1;
            if (_baseComparer.Compare(y, "c") == 0)
                return 1;
            */
            return _baseComparer.Compare(x, y);
        }
    }

    [Serializable]
    class DiskFile
    {
        private enum Types
        {
            Directory,
            File
        };


        private SortedDictionary<string, long> elements; 

        private long size;
        private DirectoryInfo dir;
        private FileSystemInfo root;       
        private List<DiskFile> children;
        private string name;
        private string location;
        private Types type;

        public string Name
        {
            get { return this.name; }
        }
        
        public string Location {
            get { return this.location; }
            set { this.location = value; } 
        }


        public DiskFile(String location)
        {
            //sprawdzenie czy to folder czy plik
            FileAttributes attr = File.GetAttributes(location);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                this.type = Types.Directory;
                var file = new DirectoryInfo(location);
                this.children = new List<DiskFile>();
                
                this.root = file;
                this.dir = file; 
                this.location = location;
                this.name = file.Name;

                var files = file.GetFileSystemInfos();
                foreach (var f in files)
                {
                    this.children.Add(new DiskFile(f.FullName));                   
                }
                this.size = this.children.Count;
 
            }               
            else
            {
                this.type = Types.File;
               
                var file = new FileInfo(location);

                this.root = file;  
                this.location = location;
                this.name = file.Name;
                this.size = file.Length;
            }
            

        }

        public void WriteTree(int level = 1)
        {
            for (int i = 0; i < level; i++)
            {
                Console.Write("\t");
            }

            Console.Write("{0} ",this.name);

            if (this.type == Types.Directory)
                Console.Write(" ({0}) ", this.children.Count);
            else
                Console.Write(" {0}  bajts ", this.size);

            Console.Write(" {0} \n", this.root.RAHS());
           // Console.Write("\n");

            level++;
            if(this.type == Types.Directory)
                foreach (DiskFile f in this.children) {
                    f.WriteTree(level);                
                }

        }

        public void TheEldest()
        {            
            string eldest = this.dir.EldestElement(); 

            Console.WriteLine();
            Console.WriteLine("Najstarszy plik: {0}", eldest);
        }

        public void AddToSortedColection()
        {
            this.elements = new SortedDictionary<string, long>(new CustomStringComparer(StringComparer.CurrentCulture));
            foreach(DiskFile f in this.children)
            {
                this.elements.Add(f.name, f.size);
            }
        }

        public void Serialization()
        {
            IFormatter formatter = new BinaryFormatter();

            Stream outputStream = File.OpenWrite("dictionary.bin");
            formatter.Serialize(outputStream, this.elements);
            outputStream.Close();

            Stream inputStream = File.OpenRead("dictionary.bin");
            SortedDictionary<string, long> elements2 = (SortedDictionary<string, long>)formatter.Deserialize(inputStream);

            Console.WriteLine();
            foreach (KeyValuePair<string, long> kvp in elements2)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
        }


    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Given argument is: {0}",args[0]);

            DiskFile rootDirectory = new DiskFile(args[0]);
            rootDirectory.WriteTree();

            rootDirectory.TheEldest();

            rootDirectory.AddToSortedColection();

            rootDirectory.Serialization();            
            
            Console.ReadKey();


        }
    }
}
