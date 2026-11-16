using System.Collections.Generic;
using System.Windows;
using RegexSearch.Core.Models;
using RegexSearch.Core.Services;

namespace RegexSearch.WPF
{
    /// <summary>
    /// Логика взаимодействия главного окна текстового редактора.
    /// Связывает элементы интерфейса с RecognitionService из RegexSearch.Core.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Сервис с обоими алгоритмами распознавания
        private readonly RecognitionService _service = new RecognitionService();

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Кнопка «Поиск через Regex»: запуск режима регулярных выражений.
        /// </summary>
        private void RegexButton_Click(object sender, RoutedEventArgs e)
        {
            RunSearch(useRegex: true);
        }

        /// <summary>
        /// Кнопка «Поиск через автомат»: запуск автоматного парсера.
        /// </summary>
        private void AutomatonButton_Click(object sender, RoutedEventArgs e)
        {
            RunSearch(useRegex: false);
        }

        /// <summary>
        /// Кнопка «Сравнить алгоритмы»: запускает оба режима
        /// и сравнивает их результаты между собой.
        /// </summary>
        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            string text = InputText.Text;

            List<MatchResult> regexResults = _service.FindByRegex(text);
            List<MatchResult> automatonResults = _service.FindByAutomaton(text);

            // Показываем результаты автоматного режима
            ResultsGrid.ItemsSource = automatonResults;

            // Сравниваем: и количество, и содержимое каждой записи
            bool identical = AreResultsIdentical(regexResults, automatonResults);

            SummaryText.Text = $"Сравнение выполнено. Regex: {regexResults.Count} совпад., " +
                               $"автомат: {automatonResults.Count} совпад. " +
                               (identical
                                   ? "Результаты алгоритмов совпадают."
                                   : "ВНИМАНИЕ: результаты алгоритмов РАЗЛИЧАЮТСЯ!");
        }

        /// <summary>
        /// Кнопка «Очистить»: сбрасывает таблицу результатов и статусную строку.
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsGrid.ItemsSource = null;
            SummaryText.Text = "Результаты поиска появятся здесь.";
            StatusText.Text = "Результаты очищены.";
        }

        /// <summary>
        /// Общая логика одиночного поиска: получает текст, вызывает нужный
        /// режим сервиса, заполняет таблицу и статусную строку.
        /// </summary>
        private void RunSearch(bool useRegex)
        {
            string text = InputText.Text;

            List<MatchResult> results = useRegex
                ? _service.FindByRegex(text)
                : _service.FindByAutomaton(text);

            // DataGrid привязываем к списку MatchResult напрямую:
            // WPF сам прочитает свойства Number, Value, Position, Length
            ResultsGrid.ItemsSource = results;

            if (results.Count == 0)
            {
                SummaryText.Text = "Совпадений не найдено.";
                StatusText.Text = "Поиск завершён: совпадений нет.";
            }
            else
            {
                SummaryText.Text = $"Найдено совпадений: {results.Count}. " +
                                   $"Первое — \"{results[0].Value}\" на позиции {results[0].Position}.";
                StatusText.Text = "Поиск завершён успешно.";
            }
        }

        /// <summary>
        /// Сравнение результатов двух режимов по содержимому,
        /// а не только по количеству записей.
        /// </summary>
        private static bool AreResultsIdentical(
            List<MatchResult> a, List<MatchResult> b)
        {
            if (a.Count != b.Count) return false;

            for (int i = 0; i < a.Count; i++)
            {
                // Порядковый номер не сравниваем — он строится одинаково
                bool same = a[i].Value == b[i].Value
                            && a[i].Position == b[i].Position
                            && a[i].Length == b[i].Length;
                if (!same) return false;
            }
            return true;
        }
    }
}
