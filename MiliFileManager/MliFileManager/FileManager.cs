using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MiLiFileManager
{
    public interface IFileManager
    {
        void SetConsole();
        void Orders();
        void WriteConfig();
    }
    class FileManager : IFileManager
    {
        private string _curDir; // текущая директория, отображаемая в командной строке
        private string _infopath; // ссылка на файл или каталог, определяющая отображение информации в блоке Info
        private string _errlogfile; // ссылка на файл ошибок (относительная)
        private int _width; //ширина окна консоли
        private int _height; //высота окна консоли
        private int _tabtop; // величина отступа сверху
        private int _numberoflines; // количество строк на страницу
        private int _tab; // величина отступа по горизонтали
        private int _div; // разделитель блока дерева файловой системы и нижнего блока (блок сообщений и блок информации)
        private int _split; // разделитель блока сообщений и блока информации
        private char _verticalborder;//символ вертикальной границы
        private char _horizontalborder;//символ горизонтальной страницы
        private char _divborder;//символ разделителя
        
        public void SetConsole() //метод, отвечающий за применение конфигурационных параметров и задание параметров консоли
        {
            Console.Title = "MiLi File Manager";
            IOdata config = new IOdata();
            
            config = config.GetConfig();
            
            Console.Clear();
            _curDir = config.StartDir;
            _infopath = config.InfoPath;
            
            if (!Directory.Exists(_curDir))
            {
                _curDir = "c:" + '\u005C' + "Program Files"; // для варианта, если каталог(curDir) удалили или переместили
            }
                
            if (!Directory.Exists(_infopath) && !File.Exists(_infopath))
            {
                _infopath = _curDir;
            }
            
            _errlogfile = config.Error;
            
            if (File.Exists(_errlogfile))
            {
                File.Delete(_errlogfile);
            }

            _width = config.Width;
            _height = config.Height;
            _numberoflines = config.NumberOfLines;
            _tabtop = config.Tabtop;
            _tab = config.Tab;
            _div = config.Div;
            _split = config.Split;
            _verticalborder = config.VerticalBorder;
            _horizontalborder = config.HorizontalBorder;
            _divborder = config.DivBorder;
            
            Console.SetWindowSize(_width, _height);
            
            Console.BufferWidth = _width;
            
            Console.BufferHeight = Console.LargestWindowHeight;
            
            Console.CursorVisible = true;
        } 
        public void WriteConfig() // метод отвечающий за сохранение пользовательских параметров (текущий каталог, информация в блоке Info, количество строк на страницу)
        {
            IOdata config = new IOdata();
            config = config.GetConfig();
            config.InfoPath = _infopath; // передача нового значения переменной для сохранения
            config.StartDir = _curDir; // передача нового значения переменной текущего каталога для сохранения
            config.NumberOfLines = _numberoflines; // передача нового значения переменной количества строк на странице для сохранения
            config.SetConfig(config);
        } 
        
        public void Orders() //метод, отвечающий за загрузку списка команд с описанием и передачи команды для выполнения соответствующему методу.
        {
            List<Commands> commandList = new List<Commands>(); 
            if (File.Exists("commands.xml"))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(List<Commands>));
                using (FileStream fs = new FileStream("commands.xml", FileMode.OpenOrCreate))
                {
                    commandList = (List<Commands>)formatter.Deserialize(fs); //получение списка команд с описанием 
                }
            }
            else
            {
                Message(6);
            }
            
            Frame(_curDir);
            Tree(_curDir, false, 0);
            Info(_infopath);
            
            //Console.SetCursorPosition(tab, div + 2);
            //Console.Write("Не указан источник, путь или имя для копирования. Повторите ввод.");
            
            string command;
            
            while (true)
            {
                command = GetCommand(_height - 1); // получение команды от пользователя
                if (command == "q" || command == "quit") //выход из цикла => из метода => из программы
                {
                    break;
                }

                try
                {
                    string[] com = command.Split(new char[] { ' ' }); // разделение по пробелам
                    
                    int length = com.Length;
                    
                    com[0].ToLower();
                    
                    switch (com[0])
                    {
                        case "cd":                      //команда смены текущего каталога
                            if (command.Length < com[0].Length + 1) //проверка на наличие пути или ключевого символа, фразы
                            {
                                Message(10);
                                break;
                            }
                            command = command.Substring(com[0].Length+1); //получение пути или имени из команды пользователя
                            Commands changedir = new Commands { Command = com[0], Target = command }; 
                            ChangeDir(changedir);
                            break;
                        
                        case "list":                    //команды просмотра содержимого каталогов
                        
                        case "ls":
                            Commands list = new Commands { Command = com[0] };
                            if (com.Length >= 2) // вариант когда команда ls или list "не пустая": содержит либо путь, либо аттрибут
                            {
                                list.Target = command.Substring(com[0].Length + 1);
                            }
                            List(list);
                            break;
                       
                        case "copy":                    // команды копирования файлов и каталогов
                        
                        case "cp":
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(44);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands copy = new Commands { Command = com[0], Target = command };
                            Copy(copy);
                            break;
                       
                        case "rm":                      // удаление файла
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands remove = new Commands { Command = com[0], Target = command };
                            RemoveFile(remove);
                            break;
                        
                        case "rmdir":                   //удаление каталога
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands rmdir = new Commands { Command = com[0], Target = command };
                            RmDir(rmdir);
                            break;
                        
                        case "mkdir":                   // создание каталога              
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands mkdir = new Commands { Command = com[0], Target = command };
                            MkDir(mkdir, false);
                            break;
                        
                        case "touch":                    // создание файла
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands touch = new Commands { Command = com[0], Target = command };
                            Touch(touch);
                            break;
                        
                        case "move":                    //перемещение файла или каталога
                        
                        case "mv":
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands move = new Commands { Command = com[0], Target = command, Mode = "mv"};
                            Move(move);
                            break;
                        
                        case "rename":                  //переименование файла или каталога
                        
                        case "nm":
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands rename = new Commands { Command = com[0], Target = command };
                            Rename(rename);
                            break;
                        
                        case "find":                    //поиск файла или каталога в текущем каталоге с подкаталогами
                        
                        case "fd":
                            if (command.Length < com[0].Length + 1)
                            {
                                Message(11);
                                break;
                            }
                            command = command.Substring(com[0].Length + 1);
                            Commands find = new Commands { Command = com[0], Target = command };
                            Find(find);
                            break;
                        
                        case "info":                    //вывод описания команды
                            if (length < 2)
                            {
                                _infopath = _curDir;
                                Info(_infopath);
                            }
                            else
                            {
                                command = command.Substring(5);
                                Info(command);
                            }
                            break;
                        
                        case "help":                    // вывод списка команд с синтаксисом
                        
                        case "h":   
                            if (commandList.Count == 0)
                            {
                                Message(6);
                                break;
                            }
                            
                            if (com.Length < 2)
                            {
                                CommandList(commandList); 
                            }
                            else if(com.Length == 2)
                            {
                                bool check = false;
                                
                                for (int i = 0; i < commandList.Count; i++)
                                {
                                    if (com[1] == commandList[i].Command)
                                    {
                                        InfoComm(commandList[i]);
                                        check = true;
                                        break;
                                    }
                                }
                                if (!check) Message(95);
                            }
                            else
                            {
                                Message(10);
                            }
                            break;
                       
                        case "config":                  //задание количества строк на страницу
                            if (com.Length < 2 || com.Length > 3)
                            {
                                Message(10);
                                break;
                            }
                            
                            int number;
                            
                            if (com.Length == 2)
                            {
                                number = -1;
                            }
                            else
                            {
                                bool check = Int32.TryParse(com[2], out number);
                                
                                if (!check)
                                {
                                    Message(96);
                                    break;
                                }
                            }
                            
                            Commands config = new Commands { Command = com[0], Mode = com[1], Value = number };
                            Config(config);
                            break;
                        default:
                            Message(8);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                    File.AppendAllText(_errlogfile, write_ex);
                    Message(7);
                }
            }
        }
        
        string GetCommand(int line) // метод, отвечающий за получение команды от пользователя.
        {
            CommandLine(_curDir);
            string command = "";
            while (command == "")
            {
                Console.SetCursorPosition(_curDir.Length + 1, line);
                command = Console.ReadLine();
                command = command.Trim();
            }
            
            ClearMessage();
            
            return command;
        }
        
        void ChangeDir(Commands command) //метод обработки команды смены текущего каталога (папки)
        {
            if (command.Target == "..") //на каталог "вверх", переход в родительский каталог
            {
                DirectoryInfo dir = new DirectoryInfo(_curDir);
                
                try
                {

                    if (dir.Parent != null) // проверка на корневую папку
                    {
                       _curDir = dir.Parent.FullName;
                    }
                    else
                    {
                        Message(21);
                    }
                }
                catch (Exception ex)
                {
                    string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                    
                    File.AppendAllText(_errlogfile, write_ex);
                    
                    Message(7);
                }
                
                return;
            }
            else if (command.Target == "root") //переход в корневой каталог
            {
                _curDir = Directory.GetDirectoryRoot(_curDir);
                                return;
            }
            else if (command.Target.IndexOf('\u005C') > 0) // проверка на наличие символа '\' в строке аргумента команды для случая указания полного пути
            {
                if (Directory.Exists(command.Target))
                    _curDir = command.Target;
                else
                {
                    Message(22);
                }
               
                return;
            }
            else if (command.Target.IndexOf('\u003A') == 1 && command.Target.IndexOf('\u005C') < 0) // второй символ в аргументе ':', но нет символа '\' (c: или е:)
            {
                string dir = command.Target + '\u005C';
                if (Directory.Exists(dir))
                {
                    _curDir = dir;
                }
                else
                {
                    Message(23);
                }
                
                return;
            }
            else if (command.Target.IndexOf('\u005C') < 0) //указывается имя папки без пути - переход в папку если она в текущем каталоге
            {
                string dir;
               
                if (_curDir == Directory.GetDirectoryRoot(_curDir))
                {
                    dir = _curDir + command.Target;
                }
                else
                {
                    dir = _curDir + '\u005C' + command.Target;
                }
                
                int count = 0;
                string dirforcheck = command.Target;
                
                for (int i = 0; i < dirforcheck.Length; i++)
                {
                    int indexOfChar = dirforcheck.IndexOf('.');

                    if (indexOfChar == 0)
                    {
                        count++;
                    }
                    
                    dirforcheck = dirforcheck.Substring(1);
                }
               
                if (Directory.Exists(dir) && count < 2 && command.Target != "/")
                {
                    _curDir = dir;
                    
                    return;
                }
                else
                {
                    Message(22);
                    
                    return;
                }
            }
            else
            {
                Message(22);
                
                return;
            }
        } 
        
        void List(Commands command) //метод обработки команды просмотра содержимого каталогов
        {
            //ProcessingUserCommand - обработка(разбор) команды пользователя и формирование экземпляра класса Commands для дальнейшего исполнения
            
            if (command.Target != null) // для исключения случая, когда отправлена команда ls или list без аттрибутов и пути
            {
                int indexmode = command.Target.LastIndexOf("-p") == -1 ? command.Target.Length : command.Target.LastIndexOf("-p"); // переменная, определяющая индекс вхождения аттрибута "-p"
                
                if (indexmode < command.Target.Length - 2) // указан аттрибут <-p> и номер страницы => вывод страницы пользователя
                {
                    int page;
                    string pagenumber = command.Target.Substring(indexmode + 3);
                    bool check = Int32.TryParse(pagenumber, out page);
                    
                    if (check)
                    {
                        if (indexmode == 0) //не указан путь в команде, а только Mode <-p> и Value(страница)
                        {
                            command.Target = null;
                        }
                        else //путь указан
                        {
                            command.Target = command.Target.Substring(0, indexmode - 1);
                        }
                        command.Mode = "-p";
                        command.Value = page;
                        command = CheckUserPage(command); //проверка страницы для пейджинга
                    }
                    else
                    {
                        Message(31);                        
                    }
                }
                else if (indexmode == 0) // вариант, когда не указан путь к каталогу и номера страниц
                {
                    command.Target = null;
                    command.Mode = "-p";
                }
                else if (indexmode != 0 && indexmode == command.Target.Length - 2) //вариант, когда не указан номер страницы, но есть путь и Mode <-p> - постраничный вывод
                {
                    command.Target = command.Target.Substring(0, indexmode - 1); ;
                    command.Mode = "-p";
                }
                //Если ничего из перечисленных условий не выполнилось, то задан только путь каталога => вывод последней страницы.
            }

            // PrintTree - Вывод в консоль дерева файлов и каталогов
            if (command.Target == null || command.Target == "")
            {
                if (command.Mode == "-p" && command.Value == 0) // для постраничного вывода по нажатию любой кнопки клавиатуры (команда ls -p)
                {
                    Tree(_curDir, true, command.Value);
                }
                else if (command.Mode == "-p" || command.Mode == null)  // определяем в методе Tree входящий параметр - модификатор в положение false
                {
                    Tree(_curDir, false, command.Value);
                }
                else
                {
                    Message(10);
                }
            }
            else // задан путь
            {
                if (Directory.Exists(command.Target))
                {
                    if (command.Mode == "-p" && command.Value == 0)
                    {
                        Tree(command.Target, true, command.Value);
                    }
                    else if (command.Mode == "-p")
                    {
                        Tree(command.Target, false, command.Value);
                    }
                    else if (command.Mode == null && command.Value == 0)
                    {
                        Tree(command.Target, false, command.Value);
                    }
                    else
                    {
                        Message(10);
                    }
                }
                else
                {
                    Message(12);
                }
            }
            
            return;
        }
        
        Commands CheckUserPage (Commands command) //проверка страницы, заданной пользователем, для вывода дерева файлов и каталогов методом List
        {
            int countpage = command.Target == null ? CountPages(_curDir) : CountPages(command.Target); // подсчет общего числа страниц каталоге (текущий или заданный) для пейджинга
            
            if (countpage < command.Value) //проверка, если page (user) больше диапазона страниц, то будем показывать последнюю страницу
            {
                Message(32);
                command.Value = countpage;
            }
            else if (command.Value < 1) //проверка, если page (user) меньше диапазона страниц, то будем показывать первую страницу
            {
                Message(33);
                command.Value = 1;
            }
            
            return command;
        }
        
        void Copy (Commands command) // метод, отвечающий за формирование команды на копирование файлов и каталогов
        {
            string checkpath = ":" + '\u005c'; //для определения начала задания пути
            int firstindex = command.Target.IndexOf(checkpath); // первый индекс вхождения символа <:\>
            int lastindex = command.Target.LastIndexOf(checkpath); // последний индекс вхождения символа <:\>
            string path;
            
            if (firstindex == -1) //символ <:\> отсутствует => создаем копию каталога или файла с указанным именем в текущем каталоге (если существует)
            {
                path = _curDir + '\u005c' + command.Target;
                if (!Directory.Exists(path))
                {
                    if (File.Exists(path))
                    {
                        string pathnewfile = GetFileNameCopy(path);
                        Commands copyfile = new Commands { Source = path, Target = pathnewfile };
                        CopyFile(copyfile);
                        Message(42);
                        return;
                    }
                    Message(10);
                    
                    return;
                }
                else
                {
                    string pathdir = GetDirNameCopy(path);
                    
                    Commands copydir = new Commands { Source = path, Target = pathdir };
                    
                    CopyDir(copydir);
                    Message(41);
                    
                    return;
                }
            }
            else if (firstindex == lastindex) // в команде указан только 1 путь к файлу или каталогу, который нужно скопировать (создаем копию в каталоге, где находится файл или каталог)
            {
                path = command.Target;
                
                if (!Directory.Exists(path))
                {
                    if (File.Exists(path))
                    {
                        string pathnewfile = GetFileNameCopy(path);
                        Commands copyfile = new Commands { Source = path, Target = pathnewfile };
                        CopyFile(copyfile);
                        Message(42);
                        return;
                    }
                    Message(10);
                    
                    return;
                }
                else
                {
                    string pathdir = GetDirNameCopy(path);
                    
                    Commands copydir = new Commands { Source = path, Target = pathdir };
                    
                    CopyDir(copydir);
                    Message(41);
                    
                    return;
                }
            }
            else // в команаде задано откуда и куда копировать
            {
                Commands copy = new Commands 
                {
                    Source = command.Target.Substring(0, lastindex - 2).Trim('\u005c'), //обрезаем в конце смвол <\>, если пользователь указал его 
                    Target = command.Target.Substring(lastindex - 1).Trim('\u005c')
                };
                
                string targetdir = copy.Target.Substring(0, copy.Target.LastIndexOf('\u005c')); //для проверки наличия каталога, куда копируем
                string sourceName = copy.Source.Substring(copy.Source.LastIndexOf('\u005c'));
                string targetName = copy.Target.Substring(copy.Target.LastIndexOf('\u005c'));
                
                Console.SetCursorPosition(_tab, _div + 3);
                Console.Write(sourceName);
                Console.SetCursorPosition(_tab, _div + 4);
                Console.Write(targetName);
                
                if (Directory.Exists(copy.Target) && sourceName!=targetName) // для исключения варианта с созданием копии каталога, но копированием содержимого в существующий каталог
                {
                    Message(10);
                    return;
                }
                
                if (!Directory.Exists(targetdir))
                {
                    Message(45);
                    return;
                }
                
                if (!Directory.Exists(copy.Source))
                {
                    if (File.Exists(copy.Source))
                    {
                        if (Directory.Exists(copy.Target))
                        {
                            Message(46);
                            return;
                        }
                        
                        copy.Target = GetFileNameCopy(copy.Target);
                        
                        try
                        {
                            CopyFile(copy);
                            Message(42);
                        }
                        catch (Exception ex)
                        {
                            string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                            File.AppendAllText(_errlogfile, write_ex);
                            Message(7);
                        }
                        
                        return;
                    }
                    Message(10);
                    return;
                }
                else
                {
                    try
                    {
                        string parentTarget = Directory.GetParent(copy.Target).FullName; 
                        string rootTarget = Directory.GetDirectoryRoot(copy.Target);
                        
                        while (parentTarget != rootTarget) //для исключения копирования родительского каталога в подкаталог (зацикливание)
                        {
                            if (copy.Source == parentTarget)
                            {
                                Message(43);
                                return;
                            }
                            parentTarget = Directory.GetParent(parentTarget).FullName;
                        }
                        
                        CopyDir(copy);
                        Message(41);
                    }
                    catch (Exception ex)
                    {
                        string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                        File.AppendAllText(_errlogfile, write_ex);
                        Message(7);
                    }
                    
                    return;
                }
            }
        }  
        
        void CopyDir(Commands command) // метод рекурсивного копирования каталога
        {
            MkDir(command, true); 
            
            try
            {
                foreach (string file in Directory.GetFiles(command.Source))
                {
                    string filecopied = command.Target + '\u005c' + Path.GetFileName(file);
                    File.Copy(file, filecopied);
                }
                
                foreach (string dir in Directory.GetDirectories(command.Source))
                {
                    Commands directory = new Commands { Source = dir, Target = command.Target + '\u005c' + Path.GetFileName(dir) };
                    CopyDir(directory);
                }
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                
                File.AppendAllText(_errlogfile, write_ex);
                
                Message(7);
            }
        } 
       
        void CopyFile(Commands command) // метод копирования файлов
        {
            try
            {
                File.Copy(command.Source, command.Target);
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                File.AppendAllText(_errlogfile, write_ex);
                Message(7);
            }
        } 
        
        void RemoveFile(Commands command) // метод удаления файла
        {
            int index = command.Target.LastIndexOf('\u005c'); // если нет символа '\' ('\u005c'), то index = -1;
            
            string path;
            
            if (index < 0) 
            {
                path = _curDir + '\u005c' + command.Target; //когда указали в пути имя удаляемого файла (удаляем из текущего каталога)
            }
            else
            {
                path = command.Target; //указан полный путь удаляемого файла
            }
            
            if (!File.Exists(path)) //проверка существования файла
            {
                Message(51);
                return;
            }
            
            try
            {
                File.Delete(path);
                Message(52);
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                File.AppendAllText(_errlogfile, write_ex);
                Message(7);
            }
            
            return;
        } 
        
        void RmDir(Commands command) //метод удаления каталога
        {
            int index = command.Target.LastIndexOf('\u005c'); // если нет символа '\' ('\u005c'), то index = -1;
            string path;
           
            if (index < 0)
            {
                path = _curDir + '\u005c' + command.Target; //когда указали в пути имя удаляемого каталога (удаляем из текущего каталога)
            }
            else
            {
                path = command.Target; //указан полный путь удаляемого каталога
            }
           
            if (!Directory.Exists(path)) //проверка существования каталога
            {
                Message(53);
                return;
            }
            
            try
            {
                Directory.Delete(path);
                
                Message(54);
            }
            catch // Если каталог не пустой, то требуется рекурсивное удаление
            {
                try
                {
                    RemoveDir(path);
                    
                    Message(55);
                }
                catch (Exception ex)
                {
                    string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                    
                    File.AppendAllText(_errlogfile, write_ex);
                    
                    Message(7);
                }
            }
            
            return;
        } 
        
        void RemoveDir(string path) // метод рекурсивного удаления каталога, если требуется при исполнении метода RmDir
        {
            try
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }
                
                if (Directory.GetDirectories(path).Length == 0)
                {
                    Directory.Delete(path);
                }
                
                foreach (string dir in Directory.GetDirectories(path))
                {
                    RemoveDir(dir);
                }
                
                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                
                File.AppendAllText(_errlogfile, write_ex);
                
                Message(7);
            }
        } 
        
        void MkDir(Commands command, bool copy) // метод создания каталогов, переменная bool copy - для формирования сообщений в случае создания каталога (false) и копирования каталога (true)
        {
            int index = command.Target.LastIndexOf('\u005c');
            string path;
            
            if (index < 0) // случай, когда указали в пути - имя нового каталога (создаем в текущем каталоге)
            {
                path = _curDir + '\u005c' + command.Target;
                
                string pathdir = GetDirNameCopy(path);
                
                Directory.CreateDirectory(pathdir);
                
                Message(81);
                return;
            }
            
            path = command.Target.Substring(0, index);
            
            if (Directory.Exists(path)) // проверка существования каталога, в котором создаем новый
            {
                string pathdir = GetDirNameCopy(command.Target);
                
                Directory.CreateDirectory(pathdir);
                
                if (copy)
                {
                    Message(41);
                }
                else
                {
                    Message(82);
                }
                return;
            }
            else
            {
                Message(12);
                return;
            }
        } 
        
        void Touch(Commands command) // метод создания файлов
        {
            int index = command.Target.LastIndexOf('\u005c');
            
            string path;
            
            if (index < 0) // случай, когда указали в пути - имя нового файла (создаем в текущем каталоге)
            {
                path = _curDir + '\u005c' + command.Target;
                
                string pathfile = GetFileNameCopy(path); //проверка и в случае существования файла корректировка имени в пути файла
                
                File.Create(pathfile);
                
                Message(83);
                return;
            }
            
            path = command.Target.Substring(0, index);
            
            if (Directory.Exists(path)) // проверка существования каталога, в котором создаем новый файл
            {
                string pathfile = GetFileNameCopy(command.Target);
                
                File.Create(pathfile);
                
                Message(84);
                return;
            }
            else
            {
                Message(12);
                return;
            }
        } 
        void Move(Commands command) //метод перемещения файла или каталога
        {
            string checkpath = ":" + '\u005c';
            int firstindex = command.Target.IndexOf(checkpath);
            int lastindex = command.Target.LastIndexOf(checkpath);
            
            if (firstindex < 0 || firstindex == lastindex) // проверка наличия 2-х путей (откуда куда)
            {
                Message(10);
                return;
            }
            
            Commands move = new Commands
            {
                Source = command.Target.Substring(0, lastindex - 2),
                Target = command.Target.Substring(lastindex - 1)
            };
            
            string path;
            
            try
            {
                if (!Directory.Exists(move.Source)) // проверяем существования источника копирования - каталога
                {
                    if (File.Exists(move.Source)) // проверяем существования источника копирования - файла
                    {
                        if (!File.Exists(move.Target)) // проверяем существования целевого объекта в новом месте
                        {
                            path = move.Target.Substring(0, move.Target.LastIndexOf('\u005c'));
                            
                            if (Directory.Exists(path))
                            {
                                File.Move(move.Source, move.Target);
                                if (command.Mode == "mv") Message(64); // для выбора сообщения. Если указан mode= "mv" - то перемещение, иначе - переименование
                                else Message(71);
                                return;
                            }
                            else
                            {
                                Message(63);
                                return;
                            }
                        }
                        else
                        {
                            if (command.Mode == "mv")
                            {
                                Message(62);
                            }
                            else
                            {
                                Message(72);
                            }
                            return;
                        }
                    }
                    else
                    {
                        Message(61);
                        return;
                    }
                }
                else //каталог- источник существует
                {
                    if (!Directory.Exists(move.Target))
                    {
                        path = move.Target.Substring(0, move.Target.LastIndexOf('\u005c'));
                        
                        if (Directory.Exists(path))
                        {
                            Directory.Move(move.Source, move.Target);
                            
                            if (command.Mode == "mv")
                            {
                                Message(66);
                            }
                            else
                            {
                                Message(73);
                            }
                            return;
                        }
                        else
                        {
                            if (command.Mode == "mv")
                            {
                                Message(63);
                            }
                            else
                            {
                                Message(10);
                            }
                            
                            return;
                        }
                    }
                    else
                    {
                        if (command.Mode == "mv")
                        {
                            Message(65);
                        }
                        else
                        {
                            Message(74);
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                
                File.AppendAllText(_errlogfile, write_ex);
                
                Message(7);
            }
        } 
       
        void Rename(Commands command) //метод переименования файла или каталога
        {
            string checkpath = ":" + '\u005c';
            
            int firstindex = command.Target.IndexOf(checkpath);
            
            int lastindex = command.Target.LastIndexOf(checkpath);
            
            try
            {
                if (firstindex == lastindex && firstindex >0) // указан 1 полный путь, переименование невозможно
                {
                    Message(10);
                    return;
                }
                else if (firstindex < 0) // указаны имена файлов или каталогов в текущем каталоге
                {
                    string[] com = command.Target.Split(new char[] { ' ' });
                    
                    if (com.Length < 2) // указано 1 имя
                    {
                        Message(10);
                        return;
                    }
                    else if (com.Length == 2) // указано 2 имени без пробелов
                    {
                        command.Target = _curDir + '\u005c' + com[0] +" " + _curDir + '\u005c' + com[1];
                       
                        command.Mode = "rn";
                        
                        Move(command);
                    }
                    else // случай, когда указаны два имени без пути с пробелами
                    {
                        string source = "", target = "";
                        
                        bool sourceNameExist = false;
                        
                        bool targetNameExist = false;
                        
                        for (int i = 0; i < com.Length; i++)
                        { 
                            if (sourceNameExist) // формирование нового имени каталога с пробелами
                            {
                                target = targetNameExist? target + " " + com[i] : target + com[i];
                                targetNameExist = true;
                            }
                            else
                            {
                                if (i == 0 && com[0] != "") // данная конструкция необходима для формирования имени исходного каталога начиная с пробела
                                {
                                    source = source + com[i];
                                }
                                else if(i == 0 && com[0]=="")
                                {
                                    source = source + " " + com[i];
                                }
                                else if(i == 1 && com[0] == "")
                                {
                                    source = source + com[i];
                                }
                                else
                                {
                                    source = source + " " + com[i];
                                }
                                string path = _curDir + '\u005c' + source;
                                if (i != 0 && com[i] != "") // для исключения проверки каталога с именем " "(пробел) - исключает ошибку и позволяет переименовать каталог, который начинается с пробела
                                {
                                    if (Directory.Exists(path) || File.Exists(path))
                                    {
                                        sourceNameExist = true;
                                    }
                                }
                            }
                        }
                        if (sourceNameExist)
                        {
                            command.Target = _curDir + '\u005c' + source + " " + _curDir + '\u005c' + target;
                            
                            command.Mode = "rn";
                            
                            Move(command);
                        }
                        else
                        {
                            Message(75);
                        }
                    }
                }
                else
                {
                    Commands name = new Commands
                    {
                        Source = command.Target.Substring(0, lastindex - 2),
                        
                        Target = command.Target.Substring(lastindex - 1),
                        
                        Mode = "rn"
                    };
                    Move(command);
                }
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                
                File.AppendAllText(_errlogfile, write_ex);
                
                Message(7);
            }
        }
        
        void Find(Commands command) // метод поиска файла или каталога по имени в текущем каталоге и подкаталогах
        {
            try
            {
                string[] foundFiles = Directory.GetFileSystemEntries(_curDir, command.Target, SearchOption.AllDirectories); // массив ссылок, где файл найден
                
                if (foundFiles.Length == 0)
                {
                    Message(90);
                    return;
                }
                else
                {
                    ClearMessage();
                    
                    Console.SetCursorPosition(_tab, _div + 1);
                    Console.Write("Объект найден по следующим адресам:");
                    
                    int count = 2;
                    for (int i = 0; i < foundFiles.Length; i++)
                    {
                        Console.SetCursorPosition(_tab, _div + count);
                        Console.Write($"{i + 1}. ");
                        
                        if (foundFiles[i].Length > _split - _tab - 5)
                        {
                            string path, fullpath = foundFiles[i];
                            
                            do
                            {
                                path = fullpath.Substring(0, _split - _tab - 5);
                                Console.Write(path);
                                
                                count = CheckCountMessage(count);
                                Console.SetCursorPosition(_tab, _div + count);
                                
                                fullpath = fullpath.Substring(path.Length);
                            } while (fullpath.Length > _split - _tab - 5);
                            
                            Console.SetCursorPosition(_tab, _div + count);
                            Console.Write(fullpath);
                            
                            count = CheckCountMessage(count);
                        }
                        else
                        {
                            Console.Write(foundFiles[i]);
                            
                            count = CheckCountMessage(count);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                
                File.AppendAllText(_errlogfile, write_ex);
                
                Message(7);
            }
        } 
        
        void Info(string path) // метод, определяющий выбор вывода информации о файле (выполняется InfoFile) или каталоге (выполняется InfoDir) в блоке информации
        {
            ClearInfo();
            
            if (Directory.Exists(path)) //проверка существования каталога по пути
            {
                InfoDir(path);
            }
            else if (Directory.Exists(_curDir + '\u005c' + path)) //проверка существования каталога по имени (path)
            {
                InfoDir(_curDir + '\u005c' + path);
            }
            else if (File.Exists(path)) //проверка существования файла по пути
            {
                InfoFile(path);
            }
            else if (File.Exists(_curDir + '\u005c' + path)) //проверка существования файла по по имени (path)
            {
                InfoFile(_curDir + '\u005c' + path);
            }
            else
            {
                Message(12);
               
                Info(_infopath);
                
                return;
            }
        } 
        
        void InfoDir(string path) //вывод информации о каталоге в блоке информации
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            string name = dir.FullName;
            
            if (name.Length > _width - _split - _tab) //если имя не помещается, то обрезаем его и добавляем '~'
            {
                int count = name.Length - _width + _split + _tab;
                
                name = name.Substring(0, name.Length - count);
                count = name.Length - name.LastIndexOf('\u005C') - 1;
                
                name = name.Substring(0, name.Length - count);
                name = name + '~';
            }
            
            Console.SetCursorPosition(_split + _tab, _div);
            Console.Write($"[{name}]");

            Console.SetCursorPosition(_split + _tab, _div + 2);
            Console.Write($"Name:  {dir.Name}");

            Console.SetCursorPosition(_split + _tab, _div + 3);
            Console.Write($"Attributes:  {dir.Attributes}");

            Console.SetCursorPosition(_split + _tab, _div + 4);
            
            int countFile = CountFiles(path);
            Console.Write("Files:        ");
            
            if (countFile != -1)
                Console.Write(countFile);
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("access denied");
                Console.ResetColor();
            }

            Console.SetCursorPosition(_split + _tab, _div + 5);
            int countDir = CountDirs(path);
            Console.Write("Directories:  ");
            
            if (countDir != -1)
            {
                Console.Write(countDir);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("access denied");
                Console.ResetColor();
            }

            Console.SetCursorPosition(_split + _tab, _div + 6);
            
            long size = GetDirSize(path);
            
            Console.Write("Size (byte):  ");
            
            if (size != -1)
            {
                Console.Write(size);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("access denied");
                Console.ResetColor();
            }

            Console.SetCursorPosition(_split + _tab, _div + 7);
            Console.Write($"Creation date:     {dir.CreationTime}");

            Console.SetCursorPosition(_split + _tab, _div + 8);
            Console.Write($"Modified date:     {dir.LastWriteTime}");

            Console.SetCursorPosition(_split + _tab, _div + 9);
            Console.Write($"Last Access date:  {dir.LastAccessTime}");
            
            _infopath = path;
        }
        
        void InfoFile(string path) //вывод информации о файле в блоке информации
        {
            FileInfo file = new FileInfo(path);
            
            string name = file.FullName;
            
            if (name.Length > _width - _split - _tab)
            {
                int count = name.Length - _width + _split + _tab;
                
                name = name.Substring(0, name.Length - count);
                
                count = name.Length - name.LastIndexOf('\u005C') - 1;
                
                name = name.Substring(0, name.Length - count);
                name = name + '~';
            }
            
            Console.SetCursorPosition(_split + _tab, _div);
            Console.Write($"[{name}]");

            Console.SetCursorPosition(_split + _tab, _div + 2);
            Console.Write($"Name:  {file.Name}");

            Console.SetCursorPosition(_split + _tab, _div + 3);
            Console.Write($"Attributes:{file.Attributes}");

            Console.SetCursorPosition(_split + _tab, _div + 4);
            Console.Write($"Extension:{file.Extension}");

            Console.SetCursorPosition(_split + _tab, _div + 5);
            Console.Write($"Size (byte): {file.Length}");

            Console.SetCursorPosition(_split + _tab, _div + 7);
            Console.Write($"Creation date:     {file.CreationTime}");

            Console.SetCursorPosition(_split + _tab, _div + 8);
            Console.Write($"Modified date:     {file.LastWriteTime}");

            Console.SetCursorPosition(_split + _tab, _div + 9);
            Console.Write($"Last Access date:  {file.LastAccessTime}");
            
            _infopath = path;
        }
        
        void InfoComm(Commands command) //метод вывода в окно сообщенний описания команды
        {
            ClearMessage();
            
            Console.SetCursorPosition(_tab, _div + 1);
            
            Console.Write($"      КОМАНДА -={command.Command}=-");
            
            Console.SetCursorPosition(_tab, _div + 2);
            
            if (command.Command == "q" || command.Command == "quit") // команда выхода не содержит заполненное поле Syntax
            {
                Console.Write($"Синтаксис: {command.Command}");
                Console.SetCursorPosition(_tab, _div + 3);
                Console.Write($"Описание: {command.Description}");
                
                return;
            }
            Console.Write($"Синтаксис: {command.Command}  {command.Syntax} ");
            Console.SetCursorPosition(_tab, _div + 3);
            Console.Write("Описание: ");
            
            int count = 4;
            
            string descripPRT, descrip = command.Description;
            
            do
            {
                Console.SetCursorPosition(_tab, _div + count);
                
                descripPRT = descrip.Substring(0, _split - _tab - 5);
                descripPRT = descripPRT.Substring(0, descripPRT.LastIndexOf(' '));
                descrip = descrip.Substring(descripPRT.Length);
                
                Console.Write(descripPRT);
                
                count = CheckCountMessage(count);
            } while (descrip.Length > _split - _tab - 5);
            
            Console.SetCursorPosition(_tab, _div + count);
            Console.Write(descrip);
        } 
        
        void CommandList(List<Commands> list) // метод постраничного вывода списка команд в окно сообщений.
        {
            ClearMessage();
            
            int count = 2;
            
            Console.SetCursorPosition(_tab, _div + 1);
            Console.WriteLine("Список команд:");
            
            for (int i = 0; i < list.Count; i++)
            {
                Console.SetCursorPosition(_tab, _div + count);
                Console.WriteLine($"{i + 1}.  {list[i].Command}    {list[i].Syntax} ");
                count = CheckCountMessage(count);
            }
            
            Console.SetCursorPosition(_tab, _div + count);
            Console.WriteLine("* Параметры c вариативным указанием.");
            count = CheckCountMessage(count);
            
            Console.SetCursorPosition(_tab, _div + count);
            Console.WriteLine("При составлении команды символы <  > не пишутся. ");
            count = CheckCountMessage(count);
            
            Console.SetCursorPosition(_tab, _div + count);
            Console.WriteLine("В качестве разделителя используется пробел.");
        }
        
        void Frame(string path) // отрисовка рамки и титулов граф в дереве с файлами и каталогами
        {
            for (int i = 1; i < _height - 1; i++) //левая и правая рамка
            {
                Console.SetCursorPosition(0, i);
                Console.Write(_verticalborder);
                Console.SetCursorPosition(_width - 1, i);
                Console.Write(_verticalborder);
            }
            
            for (int i = 1; i < _width - 1; i++) //верхняя и нижняя рамки, делитель блоков
            {
                Console.SetCursorPosition(i, 1);
                Console.Write(_horizontalborder);
                Console.SetCursorPosition(i, _div);
                Console.Write(_divborder);
                Console.SetCursorPosition(i, _height - 2);
                Console.Write(_horizontalborder);
            }
            
            for (int i = _div + 1; i < _height - 2; i++) // разделитель блока информации
            {
                Console.SetCursorPosition(_split, i);
                Console.Write(_verticalborder);
                Console.Write(_verticalborder);
            }
            
            Console.BackgroundColor = ConsoleColor.White; //титулы граф
            Console.ForegroundColor = ConsoleColor.Black;
            
            Console.SetCursorPosition(_tab + 5, _tabtop - 1);
            Console.Write("Name");
            
            Console.SetCursorPosition(_width - _tab - 48, _tabtop - 1);
            Console.Write("Type");
            
            Console.SetCursorPosition(_width - _tab - 37, _tabtop - 1);
            Console.Write("Size(byte)");
            
            Console.SetCursorPosition(_width - _tab - 18, _tabtop - 1);
            Console.Write("Creation date");
            
            Console.ResetColor();
        }
        void Tree(string path, bool mode, int userPage) // метод для вывода дерева файлов и каталогов, mode= true - постраничный вывод, false - выводит  1 страницу
        {
            if (userPage == 0) // значение по умолчанию (пользователь не задал значение страницы или Mode) - выводим последнюю страницу
            {
                userPage = CountPages(path);
            }
            
            ClearTree();
            
            Console.SetCursorPosition(_tab, 1); // вывод каталога, которому принадлежит дерево каталогов (левый верхний угол)
            Console.Write($"[ {path} ]");
            
            Console.SetCursorPosition(_width - 21, 1); // вывод текущей даты (правый верхний угол)
            Console.Write($"[Today: {DateTime.Now.ToString("dd.MM.yyyy")}]");
            
            string[] dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            
            int row = _tabtop; //переменная для определения ряда в окне дерева файлов и каталогов
            int cutPath; // количество символов в пути текущей директории для получения имени файла или директории в составе текущей
            char branch; // символ для построения древовидной структуры (либо символ <└>, либо пробел)
            
            int page = 1;
            
            DirectoryInfo info = new DirectoryInfo(path);
            
            for (int i = 0; i < dirs.Length; i++)
            {
                Console.SetCursorPosition(_tab, row);
                
                if (info.Parent != null) //орабатыввает случай когда родительским каталогом является Root каталог
                    cutPath = path.Length + 1;
                else
                    cutPath = path.Length;
                branch = ' ';
                
                if (userPage == page || mode) //для вывода необходимой страницы или постраничного вывода
                {
                    PrintTreeElement(dirs[i], cutPath, branch); //вывод имени каталога 1-го уровня
                    PrintDirInfo(dirs[i], row); //вывод краткой информации о каталоге в дереве
                }
                try
                {
                    string[] subdir = Directory.GetDirectories(dirs[i], "*", SearchOption.TopDirectoryOnly);
                    string[] subfiles = Directory.GetFiles(dirs[i], "*", SearchOption.TopDirectoryOnly);
                    
                    cutPath = dirs[i].Length + 1;
                    
                    for (int k = 0; k < subdir.Length; k++)
                    {
                        if (row == _tabtop + _numberoflines) //проверка страницы по текущему ряду
                        {
                            if (mode)
                            {
                                Paging(page, path, mode); //печать строки страниц на уровне разделителя (div) с выделением активной
                            }
                            page++;
                        }
                       
                        row = CheckPage(row); //проверка и изменение ряда
                       
                        Console.SetCursorPosition(_tab + 2, row);
                        
                        branch = '\u2514'; //символ  <└>
                        
                        if (userPage == page || mode)
                        {
                            PrintTreeElement(subdir[k], cutPath, branch); //вывод имени каталога 2-го уровня
                            PrintDirInfo(subdir[k], row); //вывод краткой информации о каталоге 2-го уровня в дереве
                        }
                    }
                    for (int k = 0; k < subfiles.Length; k++)
                    {
                        if (row == _tabtop + _numberoflines)
                        {
                            if (mode)
                            {
                                Paging(page, path, mode);
                            }
                            page++;
                        }
                        row = CheckPage(row);
                       
                        Console.SetCursorPosition(_tab + 2, row);
                        branch = '\u2514';
                        
                        if (userPage == page || mode)
                        {
                            PrintTreeElement(subfiles[k], cutPath, branch); //вывод имени файла в каталоге 1-го уровня
                            PrintFileInfo(subfiles[k], row); //вывод краткой информации о файле в каталоге 1-го уровня 
                        }
                    }
                }
                catch (Exception ex)
                {
                    string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                    File.AppendAllText(_errlogfile, write_ex); //запись ошибки в файл ошибок
                    Message(7);
                }
               
                if (row == _tabtop + _numberoflines)
                {
                    if (mode)
                    {
                        Paging(page, path, mode);
                    }
                    page++;
                }
                row = CheckPage(row);
            }
            
            if (info.Parent != null)
            {
                cutPath = path.Length + 1;
            }
            else
            {
                cutPath = path.Length;
            }
           
            for (int i = 0; i < files.Length; i++)
            {
                Console.SetCursorPosition(_tab, row);
                branch = ' ';
                if (userPage == page || mode)
                {
                    PrintTreeElement(files[i], cutPath, branch);  //вывод имени файла в текущем каталоге
                    PrintFileInfo(files[i], row); //вывод краткой информации о файле в каталоге 1-го уровня 
                }
                if (row == _tabtop + _numberoflines)
                {
                    if (mode)
                    {
                        Paging(page, path, mode);
                    }
                    page++;
                }
                row = CheckPage(row);
            }
            
            if (mode)
            {
                Console.SetCursorPosition(_tab, _div + 2);
                string clear = "";
                for (int i = _tab; i < _split; i++)
                    clear = clear + " ";
                Console.WriteLine(clear);
            }
            
            Paging(userPage, path, false);
        }
        
        void PrintDirInfo(string path, int row) //вывод информации о каталоге в дереве файлов и каталогов
        {
            Console.SetCursorPosition(_width - 25, row); // дата создания каталога
            
            DateTime creationDate = Directory.GetCreationTime(path);
            
            Console.Write(creationDate);
            Console.SetCursorPosition(_width - _tab - 50, row); // тип
            Console.Write("<DIR>");
        }
        
        void PrintFileInfo(string path, int row) //вывод информации о файле в дереве файлов и каталогов
        {
            Console.SetCursorPosition(_width - 25, row); // дата создания файла
            
            DateTime creationDate = File.GetCreationTime(path);
            
            Console.Write(creationDate);
            Console.SetCursorPosition(_width - 40, row); // размер в байтах
            
            FileInfo file = new FileInfo(path);
            
            long size = file.Length;
            
            Console.Write(size);
            Console.SetCursorPosition(_width - _tab - 50, row); // тип и расширение
            
            string extension = file.Extension;
            
            Console.Write($"<file{extension}>");
        }
        
        void PrintTreeElement(string path, int cutCount, char branch) // печать имени файла или каталога в дереве
        {
            string name = path.Remove(0, cutCount); // получение имени файла или каталога
            
            if (name.Length > _width - 65) //обрезка имени, если длинное и добавление символа '~'
            {
                int delta = name.Length - _width + 65;
                name = name.Substring(0, name.Length - delta) + '~';
                Console.Write($"{branch}{name}");
            }
            else
            {
                Console.Write($"{branch}{name}");
            }
        }
        
        long GetDirSize(string path) //рекурсивный способ рассчета размера каталога в байтах
        {
            long size = 0;
            
            try
            {
                string[] files = Directory.GetFiles(path);
                
                foreach (string file in files)
                {
                    size += (new FileInfo(file)).Length;
                }
                    
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    size += GetDirSize(dir);
                }
            }
            catch (Exception ex)
            {
                size = -1;
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                File.AppendAllText(_errlogfile, write_ex);
                Message(7);
            }
            
            return size;
        }
        
        int CountPages(string path) // подсчет количества страниц для Пэйджинга для каталога, который отображается в области дерева
        {
            string[] dir = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            int count = dir.Length + files.Length;
            
            for (int i = 0; i < dir.Length; i++)
            {
                try
                {
                    string[] subElement = Directory.GetFileSystemEntries(dir[i], "*", SearchOption.TopDirectoryOnly);
                    count += subElement.Length;
                }
                catch (Exception ex)
                {
                    string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                    File.AppendAllText(_errlogfile, write_ex);
                    Message(7);
                }
            }
            
            if (count % (_numberoflines + 1) != 0)
                count = count / (_numberoflines + 1) + 1;
            else
                count = count / (_numberoflines + 1);
           
            return count;
        }
        
        int CheckPage(int row) //возращает номер ряда в дереве для вывода элемента в консоль
        {
            if (row != _tabtop + _numberoflines) //если не достигли нижнего ряда в дереве
            {
                row++;
                return row;
            }
            row = _tabtop; //для начала новой страницы снова ряд 1 (в консоли 3)
            return row;
        }
        
        int CountFiles(string path) // подсчитывает количество файлов каталоге без учета файлов в подкаталогах
        {
            int count = 0;
            try
            {
                string[] files = Directory.GetFiles(path);
                count += files.Length;
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                File.AppendAllText(_errlogfile, write_ex);
                Message(7);
                return -1;
            }
           
            return count;
        }
        
        int CountDirs(string path) // подсчитывает количество каталогов в каталоге по пути path без учета каталогов в подкаталогах
        {
            int count = 0;
            
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                count += dirs.Length;
            }
            catch (Exception ex)
            {
                string write_ex = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace + "\n" + "\n";
                File.AppendAllText(_errlogfile, write_ex);
                Message(7);
                count = -1;
            }
            
            return count;
        }
        
        void CommandLine(string path) //выводит командную строку - внизу слева
        {
            Console.SetCursorPosition(0, _height - 1);

            for (int i = 0; i < _width; i++)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, _height - 1);
            Console.Write($"{path}>");
        }
        
        void Paging(int userPage, string path, bool mode) // определяет вид вывода номеров страниц пейджинга по средствам метода PrintPageNumber
        {
            int pages = CountPages(path);
            
            if (pages <= 7)
            {
                PrintPageNumber(1, pages, userPage, 1, "");
            }
            else
            {
                if (userPage < 6)
                {
                    PrintPageNumber(1, 5, userPage, 1, "");
                    PrintPageNumber(6, 6, userPage, 6, "...");
                    PrintPageNumber(pages, pages, userPage, 7, "");
                }
                else if (userPage > pages - 5)
                {
                    PrintPageNumber(1, 1, userPage, 1, "");
                    PrintPageNumber(pages - 5, pages - 5, userPage, 2, "...");
                    PrintPageNumber(pages - 4, pages, userPage, 3, "");
                }
                else
                {
                    PrintPageNumber(1, 1, userPage, 1, "");
                    PrintPageNumber(2, 2, userPage, 2, "...");
                    PrintPageNumber(userPage - 1, userPage + 1, userPage, 3, "");
                    PrintPageNumber(pages - 1, pages - 1, userPage, 6, "...");
                    PrintPageNumber(pages, pages, userPage, 7, "");
                }
            }
            
            if (mode)
            {
                ClearMessage();
                
                Console.SetCursorPosition(_tab, _div + 2);
                Console.WriteLine("Press anykey to continue...");
                Console.ReadKey();
                
                ClearTree();
                
                Console.SetCursorPosition(_tab, 1);
                Console.Write($"[ {path} ]");
            }
        }
        
        void PrintPageNumber(int number, int pages, int userPage, int startposition, string symbol) //обеспечивает вывод в консоли на разделителе номеров страниц и выделяет активную страницу
        {
            
            for (int i = number; i <= pages; i++)
            {
                Console.SetCursorPosition(_tab + (startposition - 1) * 5, _div);
                
                if (i < 100 && symbol != "...")
                    Console.Write("[ ");
                else
                    Console.Write("[");
                
                if (i == userPage)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(i);
                    Console.ResetColor();
                }
                else
                     if (symbol == "...")
                    Console.Write(symbol);
                else
                    Console.Write(i);
                
                if (i < 10 && symbol != "...")
                    Console.Write(" ]");
                else
                    Console.Write("]");
                startposition++;
            }
        }
        
        string GetFileNameCopy(string pathfile) // для метода копирования проверяет существование файла по указанному пути и при необходимости корректирует имя путем добавления (номер)
        {
            int count = 0;
            
            while (File.Exists(pathfile))
            {
                count++;
                
                FileInfo file = new FileInfo(pathfile);
                
                string filename;
                int numberofcut;
                
                if (count < 2)
                {
                    filename = pathfile.Substring(0, pathfile.Length - file.Extension.Length);
                }
                else
                {
                    if (count < 11)
                        numberofcut = 3;
                    else if (count < 101)
                        numberofcut = 4;
                    else if (count < 1001)
                        numberofcut = 5;
                    else          // создавать более 10000 копий врядли понадобиться
                    {
                        numberofcut = 6;
                    }
                    filename = pathfile.Substring(0, pathfile.Length - file.Extension.Length - numberofcut);
                }
                
                pathfile = filename + "(" + count + ")" + file.Extension;
            }
            
            return pathfile;
        } 
        
        string GetDirNameCopy(string pathdir) // для метода копирования проверяет существование каталога по указанному пути и при необходимости корректирует имя путем добавления (номер)
        {
            int count = 0;
            string path = pathdir;
            
            while (Directory.Exists(pathdir))
            {
                count++;
                pathdir = path + "(" + count + ")";
            }
            
            return pathdir;
        }
        
        int CheckCountMessage (int count) // проверка счетчика для постраничного вывода в блоке сообщений
        {
            if (count > _height - _div - 5)
            {
                Console.SetCursorPosition(_tab, _div + count+1);
                Console.Write("Press anykey to continue ...");
                Console.ReadKey();
                ClearMessage();
                count = 2;
            }
            else count++;
            
            return count;
        }
        
        void Config(Commands command) // метод для задания вывода количества строк на странице
        {
            if (command.Mode == "-i")
            {
                Message(94);
            }
            else if (command.Mode == "-c")
            {
                if (command.Value >0 && command.Value < _div - _tabtop)
                {
                    _numberoflines = command.Value -1;
                    Message(91);
                }
                else
                {
                    Message(93);
                }
            }
            else
            {
                Message(92);
            }
        } 
        
        void ClearMessage() // очистка блока сообщений
        {
            string clear = "";
            
            for (int i = _tab; i < _split; i++)
            {
                clear = clear + " ";
            }
            
            for (int k = _div + 1; k < _height - 2; k++)
            {
                Console.SetCursorPosition(_tab, k);
                Console.Write(clear);
            }
        }
        
        void ClearInfo() // очистка блока информации
        {
            string clear = "";
            
            for (int i = _split + 2; i < _width - 1; i++)
            {
                Console.SetCursorPosition(i - 2, _div);
                Console.Write(_divborder);
                clear = clear + " ";
            }
            
            for (int k = _div + 2; k < _height - 2; k++)
            {
                Console.SetCursorPosition(_split + 2, k);
                Console.Write(clear);
            }
        }
        void ClearTree() //очистка области дерева файлов и каталогов
        {
            string clear = "";
            
            for (int i = _tab; i < _width - _tab; i++)
            {
                if (i <= _split)
                {
                    Console.SetCursorPosition(i, _div);
                    Console.Write(_divborder);
                    Console.SetCursorPosition(i, 1);
                    Console.Write(_horizontalborder);
                }
                clear = clear + " ";
            }
            
            for (int k = _tabtop; k < _div; k++)
            {
                Console.SetCursorPosition(_tab, k);
                Console.Write(clear);
            }
        }
        
        void Message(int code) // сообщения и ошибки в блоке сообщений
        {
            ClearMessage();
            
            Console.SetCursorPosition(_tab, _div + 2);
            
            switch (code)
            {
                case 6:
                    Console.Write("Файл commands.xml не найден.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Список команд и описание команд не доступны.");
                    Console.SetCursorPosition(_tab, _div + 4);
                    Console.Write("За помощью обратитесь к разработчику.");
                    Console.SetCursorPosition(_tab, _div + 4);
                    Console.Write("Для выхода из программы команда q или quit.");
                    break;
                
                case 7:
                    Console.Write("При выполнении команды зафиксирована ошибка.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Информация об ошибке записана в файл error.log.");
                    break;
                
                case 8:
                    Console.Write("Неверная команда.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Список команд - h или help.");
                    Console.SetCursorPosition(_tab, _div + 4);
                    Console.Write("Описание команды - h <команда> или help <команда>.");
                    break;
                
                case 10:
                    Console.Write("Неверный синтаксис команды.");
                    break;
                
                case 11:
                    Console.Write("Не указан путь или имя файла. Повторите ввод.");
                    break;
                
                case 12:
                    Console.Write("Неверный путь в команде. Повторите ввод.");
                    break;
                
                case 21:
                    Console.Write("Текущая папка - корневой каталог.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Переход в каталог верхнего уровня невозможен.");
                    break;
                
                case 22:
                    Console.Write("Неверное имя папки или путь.");
                    break;
                
                case 23:
                    Console.Write("Неверная метка тома диска.");
                    break;
                
                default:
                    Console.Write("Неизвестный код сообщения.");
                    break;
                
                case 31:
                    Console.Write("Неверный формат номера страницы.");
                    break;
                
                case 32:
                    Console.Write("Ваша страница вне диапазона. Показана последняя страница.");
                    break;
                
                case 33:
                    Console.Write("Ваша страница вне диапазона. Показана первая страница.");
                    break;
                
                case 41:
                    Console.Write("Каталог успешно скопирован.");
                    break;
                
                case 42:
                    Console.Write("Файл успешно скопирован.");
                    break;
                
                case 43:
                    Console.Write("Нельзя скопировать родительский каталог в подкаталог.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Повторите ввод.");
                    break;
                
                case 44:
                    Console.Write("Не указан источник, путь или имя для копирования.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Повторите ввод.");
                    break;
                
                case 45:
                    Console.Write("Каталог назначения не существует. Повторите ввод.");
                    break;
                
                case 46:
                    Console.Write("Ошибка в пути назначения при копировании файла.");
                    break;
                
                case 51:
                    Console.Write("Файл не существует.");
                    break;
                
                case 52:
                    Console.Write("Файл удален.");
                    break;
                
                case 53:
                    Console.Write("Каталога не существует.");
                    break;
                
                case 54:
                    Console.Write("Каталог удален.");
                    break;
                
                case 55:
                    Console.Write("Каталог удален рекурсивно.");
                    break;
                
                case 61:
                    Console.Write("Ошибка в исходном пути. Повторите ввод.");
                    break;
                
                case 62:
                    Console.Write("Файл существует в каталоге назначения. Перемещение невозможно.");
                    break;
                
                case 63:
                    Console.Write("Неверный путь назначения. Повторите ввод.");
                    break;
                
                case 64:
                    Console.Write("Файл перемещен.");
                    break;
                
                case 65:
                    Console.Write("Каталог существует в месте назначения. Перемещение невозможно.");
                    break;
                
                case 66:
                    Console.Write("Каталог перемещен.");
                    break;
                
                case 67:
                    Console.Write("Ошибка перемещения.");
                    break;
                
                case 71:
                    Console.Write("Фаил переименован.");
                    break;
                
                case 72:
                    Console.Write("Переименование не осуществимо.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Файл с целевым именем существует.");
                    break;
                
                case 73:
                    Console.Write("Каталог переименован.");
                    break;
                
                case 74:
                    Console.Write("Переименование не осуществимо.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("Каталог с целевым именем существует.");
                    break;
                
                case 75:
                    Console.Write("Ошибка переименования.");
                    break;
                
                case 81:
                    Console.Write("Каталог создан в текущем каталоге.");
                    break;
                
                case 82:
                    Console.Write("Каталог создан по указанному пути.");
                    break;
                
                case 83:
                    Console.Write("Файл создан в текущем каталоге.");
                    break;
                
                case 84:
                    Console.Write("Файл создан по указанному пути.");
                    break;
                
                case 90:
                    Console.Write("Файл или каталог не найден в текущем каталоге ");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("и подкаталогах.");
                    break;
                
                case 91:
                    Console.Write("Задано новое количество строк на страницу. ");
                    break;
                
                case 92:
                    Console.Write("Неверный атрибут команды");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write("-i - текущеe значениe количества строк на странице.");
                    Console.SetCursorPosition(_tab, _div + 4);
                    Console.Write("-с <n> - заданиие нового (n) значения количества строк. ");
                    break;
                
                case 93:
                    Console.Write("Неверное значение количества строк.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write($"Допустимый диапазон значений количества строк - ");
                    Console.SetCursorPosition(_tab, _div + 4);
                    Console.Write($"от 1 до {_div - _tabtop - 1}.");
                    break;
                
                case 94:
                    Console.Write($"Количества строк на странице - {_numberoflines+1}.");
                    Console.SetCursorPosition(_tab, _div + 3);
                    Console.Write($"Допустимый диапазон значений количества строк - ");
                    Console.SetCursorPosition(_tab, _div + 4);
                    Console.Write($"от 1 до {_div - _tabtop - 1}.");
                    break;
                
                case 95:
                    Console.Write("Ошибка команды config.");
                    break;
                
                case 96:
                    Console.Write("Ошибка при задании количества строк на странице.");
                    break;
            }
        } 
    }
}