using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExportSQL2MD.API
{
    public class FileSystem
    {
        // Source Folder Info
        static string sqlRepo, sqlFolder, sqlBranch, mdRepo, mdFolder, mdBranch;

        static string tempFolder = $"{mdRepo}\\Export-SQL2MD";
        bool discardComments = false;
        string specifFile = null;
        List<string> filesToBeIgnored = new List<string>() {
            "Excluir Etapa",
            "Configurar Acesso do Aluno"
        };
        public string separator = "==================================================================";
        private PowerShell powershell = PowerShell.Create();

        public void UpdateOneFile()
        {
            Console.Write("[!] Digite o nome do arquivo: ");
            specifFile = Console.ReadLine();
            specifFile = specifFile.ToLower().Replace(".sql", "").Replace(".md", "");
        }

        public void UpdateAllFiles()
        {

            //Trocar para branch onde estão os arquivos .SQL
            powershell.AddScript($"cd {sqlRepo}");
            powershell.AddScript($"git switch {sqlBranch}");
            powershell.Invoke();

            //Criar pasta temporária para a exportação
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
            Directory.CreateDirectory(tempFolder);

            //Percorrer arquivos .SQL
            Console.WriteLine(separator);
            Console.WriteLine($">> Exportando arquivos de {sqlFolder}");
            Console.WriteLine(separator);

            this.GerarTemplates();
            this.ExportarMds();

            Console.WriteLine();

            Console.WriteLine(separator);
            Console.WriteLine(">> Atualização de Sumários...");
            Console.WriteLine(separator);

            Console.WriteLine($@">> Atualizando {mdFolder}.md");

            this.GerarSumario();

            // Deletar Pasta Temporária
            Directory.Delete(tempFolder, true);

            ConsoleColor foreground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n>> Os arquivos foram exportados com sucesso!");
            Console.ResetColor();
            Console.WriteLine("\n>> Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        public void GerarTemplates()
        {
            //Ler Aquivos .SQL das pastas
            DirectoryInfo d = new DirectoryInfo(sqlFolder);
            FileInfo[] Files = d.GetFiles("*.sql"); //Getting Text files


            //Percorrer Arquivos Encontrados
            foreach (FileInfo file in Files)
            {
                var mustIgnore = false;
                filesToBeIgnored.ForEach(fileName =>
                {
                    if (fileName.Equals(file.Name.Replace(".sql", "")))
                        mustIgnore = true;
                });

                if (mustIgnore || (specifFile != null && !file.Name.ToLower().Equals(specifFile + ".sql")))
                    continue;

                Console.WriteLine($"Exportando \"{file.Name}\"");
                //Tranformar SQL em MD 
                var fileName = file.Name.Replace(" ", "-").Replace(".sql", ".md");
                var mdFile = File.CreateText($@"{tempFolder}\{fileName}");
                mdFile.WriteLine("```SQL");
                using (StreamReader sr = file.OpenText())
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                        mdFile.WriteLine(s);
                }
                mdFile.WriteLine("```");
                mdFile.Dispose();
            }
        }
        public void ExportarMds()
        {
            //Trocar para branch wikiMaster, onde estão os arquivos .MD
            powershell.AddScript($"cd {mdRepo}");
            powershell.AddScript($"git switch {mdBranch}");
            powershell.Invoke();

            //Percorrer pastas Novamente
            DirectoryInfo d = new DirectoryInfo($@"{tempFolder}");
            FileInfo[] Files = d.GetFiles("*.md");
            //Atualizar arquivos .MD
            foreach (FileInfo file in Files)
            {
                var destPath = $@"{mdFolder}\{file.Name}";
                var sourcePath = $@"{tempFolder}\{file.Name}";

                if (File.Exists(destPath))
                {
                    var destContents = File.ReadAllLines(destPath);
                    var overWritten = File.CreateText(destPath);
                    bool copyFromDest = true;

                    foreach (string line in destContents)
                    {
                        if (line.ToLower().Contains("```sql"))
                        {
                            copyFromDest = false;
                            var sourceContents = File.ReadAllLines(sourcePath);
                            foreach (string sourceLine in sourceContents)
                            {
                                bool addLine = true;

                                if (sourceLine.Equals("```"))
                                    addLine = false;

                                if (discardComments)
                                {
                                    if (sourceLine.TrimStart().StartsWith("--"))
                                    {
                                        addLine = false;
                                        if (sourceLine.ToLower().Contains("commit") ||
                                           sourceLine.ToLower().Contains("rollback"))
                                            addLine = true;
                                    }
                                    else if (sourceLine.Contains("--"))
                                    {
                                        overWritten.WriteLine(sourceLine.Split("--")[0]);
                                        continue;
                                    }
                                }

                                if (addLine)
                                    overWritten.WriteLine(sourceLine);
                            }
                        }
                        if (!copyFromDest && !line.ToLower().Contains("```sql") && line.ToLower().Contains("```"))
                            copyFromDest = true;

                        if (copyFromDest)
                            overWritten.WriteLine(line);
                    }
                    overWritten.Dispose();
                }
                else
                    File.Move(sourcePath, destPath);
            }
        }
        public void GerarSumario()
        {
            //Gerar .Md da pasta
            StreamWriter subPastaMD = File.CreateText($"{mdFolder}.md");
            subPastaMD.WriteLine($"# CNA {mdFolder}");
            subPastaMD.WriteLine($"---");
            subPastaMD.WriteLine($"ℹ️ Esta seção da Wiki guarda artigos e queries sobre o CNA {mdFolder}.");
            subPastaMD.WriteLine($"## Conteúdo 📖:");

            //Adicionar link do arquivo ao .MD da pasta
            DirectoryInfo d = new DirectoryInfo($@"{mdFolder}");
            FileInfo[] Files = d.GetFiles("*.md");
            foreach (FileInfo file in Files)
            {
                var fileName = file.Name.Replace(".md", "").Replace("-", " ");
                subPastaMD.WriteLine($" - [x][{fileName}](/{mdFolder}/{file.Name})");
            }
            subPastaMD.Dispose();
        }

        public void ReadAppSettings()
        {
            var jsonData = JObject.Parse(File.ReadAllText("API\\AppSettings.json")).Children();
            List<JToken> appSettings = jsonData.Children().First().ToList();

            List<MenuOption> wikis = new List<MenuOption>();

            wikis.Add(new MenuOption("[1] - Todos projetos"));
            for (int i = 0; i < appSettings.Count(); i++)
            {
                string wikiName = appSettings[i].Path.Replace("wikis", "").Replace("[", "").Replace("]", "").Replace(".", "").Replace("'", "");
                wikis.Add(new MenuOption("[" + (i + 2) + "] - " + wikiName, appSettings[i].First()));
            }

            Console.WriteLine("Qual wiki deseja atualizar?");
            Console.WriteLine();

            foreach (MenuOption wiki in wikis)
                Console.WriteLine(wiki.title);

            int userOption = askForOption();
            dynamic optionValue = wikis[userOption - 1].value;

            // Source Folder Info
            sqlRepo = optionValue["source"]["repo"];
            sqlFolder = $"{sqlRepo}\\{optionValue["source"]["folder"]}";
            sqlBranch = optionValue["source"]["branch"];

            // Target Folder Info
            mdRepo = optionValue["target"]["repo"];
            mdFolder = $"{mdRepo}\\{optionValue["source"]["folder"]}";
            mdBranch = optionValue["source"]["branch"];

        }

        private int askForOption()
        {
            Console.WriteLine();
            Console.Write("[!] - Digite o número da opção: ");

            int UserOption;
            if (int.TryParse(Console.ReadLine(), out UserOption))
            {
                return UserOption;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(">> Opção Inválida <<");
            Console.ResetColor();
            return askForOption();
        }

        private class MenuOption
        {
            public string title;
            public dynamic value;

            public MenuOption(string title, dynamic value = null)
            {
                this.title = title;
                this.value = value;
            }
        }
    }
}
