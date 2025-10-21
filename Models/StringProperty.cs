namespace StringAnalyzer.Models
{
    public class StringProperty
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public int Length { get; set; }
        public bool IsPalindrome { get; set; }
        public int UniqueCharacters { get; set; }
        public int WordCount { get; set; } = 0;
        public string Sha256Hash { get; set; } = string.Empty;
        public Dictionary<char, int> CharacterFrequencyMap { get; set; } = new();
        public DateTime CreatedAt { get; set; }

    }
}
