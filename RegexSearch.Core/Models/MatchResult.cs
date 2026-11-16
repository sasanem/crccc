namespace RegexSearch.Core.Models
{
    /// <summary>
    /// Класс с информацией о каждой найденной подстроке.
    /// </summary>
    public class MatchResult
    {
        public int Number { get; set; }                   // Порядковый номер совпадения
        public string Value { get; set; } = string.Empty; // Найденная подстрока
        public int Position { get; set; }                 // Позиция начала в тексте (отсчёт от нуля)
        public int Length { get; set; }                   // Длина найденной подстроки
    }
}
