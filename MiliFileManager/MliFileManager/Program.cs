using System;

namespace MiLiFileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            FileManager fileManager = new FileManager();
            
            fileManager.SetConsole(); //Вызов метода интерфейса FileManager для загрузки данных из config.xml и применения настроек 
            
            fileManager.Orders(); // Вызов метода интерфейса FileManager для управления коммандами
            
            fileManager.WriteConfig(); // Вызов метода интерфейса FileManager для записи конфигурационных данных в config.xml с учетом изменений
        }
    }
}