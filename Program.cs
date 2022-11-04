using System;

namespace ExportSQL2MD
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ResetColor();
            var Splash = new SplashScreen();
            Splash.Show();

            var FS = new API.FileSystem();

            FS.ReadAppSettings();

            try
            {
                var UserOption = 1;//Convert.ToInt32(Console.ReadLine());
                Console.WriteLine();


                if (UserOption == 1)
                {
                    Console.WriteLine(FS.separator);
                    Console.WriteLine(">> Atualizar Todos Arquivos <<");
                    FS.UpdateAllFiles();
                }
                else if (UserOption == 2)
                {
                    Console.WriteLine(FS.separator);
                    Console.WriteLine(">> Atualizar Arquivo Específico <<");
                    Console.WriteLine(FS.separator);
                    FS.UpdateOneFile();
                    FS.UpdateAllFiles();
                }
                else
                {
                    ConsoleColor foreground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(">> Opção Inválida <<");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ConsoleColor foreground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n>> Você não escolheu uma opção. Execute o programa novamente! <<");
                Console.ReadKey();
                Console.ResetColor();
            }
        }
    }
}
