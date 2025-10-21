using StringAnalyzer.Models;

namespace StringAnalyzer.Repositories
{
    public interface IStringRepository
    {
        public StringProperty? CreateStringAnalysis(string stringValue);
        public StringProperty? GetStringAnalysisByValue(string stringValue);
        public ICollection<StringProperty>? GetAllStringAnalysis();
        public string DeleteStringAnalysis(string stringValue);
    }
}
