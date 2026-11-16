using System.Collections.Generic;
using System.Text.RegularExpressions;
using RegexSearch.Core.Models;

namespace RegexSearch.Core.Services
{
    /// <summary>
    /// Сервис распознавания подстрок. Реализует два независимых режима:
    /// 1) поиск с помощью регулярных выражений (System.Text.RegularExpressions);
    /// 2) поиск с помощью конечного автомата (самостоятельная реализация).
    /// Результаты обоих режимов должны совпадать при одинаковых входных данных.
    /// </summary>
    public class RecognitionService
    {
        // Регулярное выражение для ИНН организации (вариант 11): 10 цифр с границами слова.
        // Вынесено в статическое поле: шаблон компилируется один раз за время работы программы.
        private static readonly Regex InnRegex = new Regex(@"\b\d{10}\b");

        /// <summary>
        /// Режим 1. Поиск всех ИНН с использованием регулярных выражений.
        /// </summary>
        public List<MatchResult> FindByRegex(string text)
        {
            var results = new List<MatchResult>();
            if (string.IsNullOrEmpty(text)) return results;

            int counter = 1; // порядковый номер совпадения

            // Matches() возвращает коллекцию всех совпадений шаблона в тексте
            foreach (Match match in InnRegex.Matches(text))
            {
                results.Add(new MatchResult
                {
                    Number = counter++,
                    Value = match.Value,    // найденная подстрока (10 цифр)
                    Position = match.Index, // позиция начала в тексте (отсчёт от нуля)
                    Length = match.Length   // длина совпадения (всегда 10)
                });
            }

            return results;
        }

        /// <summary>
        /// Режим 2. Поиск всех ИНН с помощью конечного автомата.
        /// </summary>
        public List<MatchResult> FindByAutomaton(string text)
        {
            var results = new List<MatchResult>();
            if (string.IsNullOrEmpty(text)) return results;

            int counter = 1;

            // Перебираем все позиции текста как кандидатов на начало последовательности
            for (int i = 0; i < text.Length; i++)
            {
                // ЛЕВАЯ ГРАНИЦА СЛОВА (\b слева): перед первой цифрой не должно
                // быть букв, цифр или подчёркивания
                if (i > 0 && IsWordChar(text[i - 1])) continue;

                // Попытка имеет смысл, только если текущий символ — цифра
                if (!char.IsDigit(text[i])) continue;

                // Запускаем автомат с начального состояния
                State state = State.Start;
                int j = i;

                // Читаем символы, пока попытка не завершится успехом или ошибкой
                while (j < text.Length && state != State.Done && state != State.Error)
                {
                    state = Transition(state, text[j]); // функция переходов
                    j++;
                }

                // Попытка успешна, только если автомат в принимающем состоянии
                if (state == State.Done)
                {
                    // ПРАВАЯ ГРАНИЦА СЛОВА (\b справа): после 10-й цифры должен
                    // стоять не \w-символ или конец текста
                    bool rightOk = j >= text.Length || !IsWordChar(text[j]);

                    if (rightOk)
                    {
                        results.Add(new MatchResult
                        {
                            Number = counter++,
                            Value = text.Substring(i, 10),
                            Position = i,
                            Length = 10
                        });

                        // Продолжаем поиск ПОСЛЕ найденной подстроки,
                        // чтобы не находить одно вхождение дважды
                        i = j - 1; // компенсируем i++ на следующей итерации
                    }
                }
                // Если state == Error — попытка не удалась, идём к следующей позиции
            }

            return results;
        }

        /// <summary>
        /// Функция переходов автомата: по текущему состоянию и входному символу
        /// возвращает следующее состояние. Реализована через switch-case,
        /// как требует методичка.
        /// </summary>
        private static State Transition(State state, char c)
        {
            bool isDigit = char.IsDigit(c); // \d — десятичная цифра

            switch (state)
            {
                case State.Start: return isDigit ? State.D1 : State.Error;
                case State.D1:    return isDigit ? State.D2 : State.Error;
                case State.D2:    return isDigit ? State.D3 : State.Error;
                case State.D3:    return isDigit ? State.D4 : State.Error;
                case State.D4:    return isDigit ? State.D5 : State.Error;
                case State.D5:    return isDigit ? State.D6 : State.Error;
                case State.D6:    return isDigit ? State.D7 : State.Error;
                case State.D7:    return isDigit ? State.D8 : State.Error;
                case State.D8:    return isDigit ? State.D9 : State.Error;
                case State.D9:    return isDigit ? State.Done : State.Error;

                // Терминальные состояния — обработка одной попытки завершена
                case State.Done:  return State.Done;
                case State.Error: return State.Error;

                default:          return State.Error;
            }
        }

        /// <summary>
        /// Проверка, является ли символ «словесным» (\w в регулярных выражениях):
        /// буква, цифра или подчёркивание. char.IsLetterOrDigit учитывает кириллицу,
        /// поэтому поведение совпадает с \b в .NET.
        /// </summary>
        private static bool IsWordChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
    }
}
