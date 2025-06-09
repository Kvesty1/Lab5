using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class TextCorrector
{
    static void Main(string[] args)
    {
        // Путь к рабочему столу
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string targetFolder = Path.Combine(desktopPath, "TextProcessing");

        // Создаем папку, если не существует
        Directory.CreateDirectory(targetFolder);

        // Путь к файлу с опечатками
        string typoFilePath = Path.Combine(targetFolder, "typos.txt");

        // Создаем файл с примерами, если он отсутствует
        if (!File.Exists(typoFilePath))
        {
            File.WriteAllText(typoFilePath, "првиет=привет\nпирвет=привет\nпревет=привет", Encoding.UTF8);
            Console.WriteLine("Файл typos.txt создан. Добавьте в него опечатки и перезапустите программу.");
            return;
        }

        // Загружаем словарь опечаток из файла
        var typoDictionary = LoadTypoDictionary(typoFilePath);

        // Обработка всех .txt файлов, кроме typos.txt
        foreach (string filePath in Directory.GetFiles(targetFolder, "*.txt"))
        {
            if (Path.GetFileName(filePath).Equals("typos.txt", StringComparison.OrdinalIgnoreCase))
                continue;

            string content = File.ReadAllText(filePath, Encoding.UTF8);

            content = CorrectTypos(content, typoDictionary);

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        Console.WriteLine("Обработка завершена.");
    }

    static Dictionary<string, string> LoadTypoDictionary(string path)
    {
        // Создаем словарь с игнорированием регистра ключей
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Читаем все строки из файла
        foreach (var line in File.ReadAllLines(path))
        {
            // Разделяем строку по символу '='
            var parts = line.Split('=');

            // Если строка разделилась на две части — это корректная пара
            if (parts.Length == 2)
            {
                // Левое значение — опечатка
                string typo = parts[0].Trim();
                // Правое значение — правильное слово
                string correct = parts[1].Trim();

                // Добавляем в словарь, если такого ключа ещё нет
                if (!dict.ContainsKey(typo))
                    dict[typo] = correct;
            }
        }

        return dict;
    }

    static string CorrectTypos(string text, Dictionary<string, string> typoDict)
    {
        // Если словарь пустой — сразу возвращаем текст без изменений
        if (typoDict.Count == 0)
            return text;

        // Создаем регулярное выражение для поиска всех ключей из словаря
        string pattern = @"\b(" + string.Join("|", typoDict.Keys.Select(Regex.Escape)) + @")\b";

        // Заменяем найденные опечатки на правильные слова
        return Regex.Replace(text, pattern, match =>
        {
            // Найденное слово в тексте
            string found = match.Value;
            // Ищем замену в словаре, учитывая регистр
            string correct = typoDict[found.ToLower()];

            // Сохраняем первую заглавную букву, если была
            return char.IsUpper(found[0])
                ? char.ToUpper(correct[0]) + correct.Substring(1)
                : correct;

        }, RegexOptions.IgnoreCase); // Игнорируем регистр при поиске
    }
}
