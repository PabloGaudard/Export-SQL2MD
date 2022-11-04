using System;
using System.Collections.Generic;
using System.Text;

namespace ExportSQL2MD
{
    class SplashScreen
    {
        public int DataAtual;
        public void Show()
        {
            string SoftwareName = "ExportSQL2MD";
            string Version = "1.0";
            string CopyrightName = "Pablo Gaudard.";
            DateTime AnoAtual = DateTime.Today;

            Console.WriteLine("Nome: " + SoftwareName);
            Console.WriteLine("Versão: " + Version);
            Console.WriteLine("Criado por: " + CopyrightName);

            string StrAnoAtual = AnoAtual.ToString("yyyy");
            int IntAnoAtual = Convert.ToInt32(StrAnoAtual);
            DataAtual = IntAnoAtual;

            if (IntAnoAtual == 2022)
            {
                Console.WriteLine("Copyright © " + IntAnoAtual + " | " + CopyrightName + " All rights reserved.");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Copyright © 2022 - " + IntAnoAtual + " | " + CopyrightName + " All rights reserved.");
                Console.WriteLine();
            }
            Console.WriteLine("[" + SoftwareName + "] - Em Execução...");
            Console.WriteLine();
        }
    }
}
