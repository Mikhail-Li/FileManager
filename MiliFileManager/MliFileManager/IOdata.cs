using System;
using System.IO;
using System.Xml.Serialization;

namespace MiLiFileManager
{
    [Serializable]
    public class IOdata //класс, который содержит конфигурационные параметры
    {
        private string _fileConfig = "config.xml";
        public string StartDir { get; set; }
        public string InfoPath { get; set; }
        public string Error { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Tabtop { get; set; }
        public int Tab { get; set; }
        public int NumberOfLines { get; set; }
        public int Div { get; set; }
        public int Split { get; set; }
        public char VerticalBorder { get; set; }
        public char HorizontalBorder { get; set; }
        public char DivBorder { get; set; }
        
        public IOdata() { }
        
        public IOdata GetConfig() // загрузка конфигурационных параметров из файла (XML-десериализация)
        {
            IOdata confData = new IOdata();
            XmlSerializer formatter = new XmlSerializer(typeof(IOdata));
            
            try
            {
                using (FileStream fs = new FileStream(_fileConfig, FileMode.Open))
                {
                    confData = (IOdata)formatter.Deserialize(fs);
                }
            }
            catch
            {
                Console.SetCursorPosition(2, 2);
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Внимание. Конфигурационный файл не найден.");
                Console.ResetColor();
                Console.SetCursorPosition(2, 3);
                Console.Write("Будут загружены настройки по умолчанию.");
                Console.SetCursorPosition(2, 4);
                Console.Write("Для продолжения нажмите любую клавишу...");
                Console.ReadKey();
                confData = new IOdata
                {
                    StartDir = "c:" + '\u005C' + "Program Files",
                    InfoPath = StartDir,
                    Error = "error.log",
                    Width = 120,
                    Height = 40,
                    Tabtop = 3,
                    Tab = 4,
                    NumberOfLines = 22,
                    Div = 27,
                    Split = 69,
                    VerticalBorder = '\u007C',
                    HorizontalBorder = '\u2014',
                    DivBorder = '\u003D'
                };
                using (FileStream fs = new FileStream(_fileConfig, FileMode.Create))
                {
                    formatter.Serialize(fs, confData);
                }
            }
            
            return confData;
        }
        
        public void SetConfig (IOdata outdata) // запись конфигурационных параметров с учетом изменений в FileManager (XML-сериализация)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(IOdata));
            using (FileStream fs = new FileStream(_fileConfig, FileMode.Create))
            {
                formatter.Serialize(fs, outdata);
            }
        }
    }
}