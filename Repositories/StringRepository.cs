using StringAnalyzer.Models;
using StringAnalyzer.Utils;
using System.Text.RegularExpressions;
namespace StringAnalyzer.Repositories
{
    public class StringRepository : IStringRepository
    {
        public StringRepository()
        {
            using (var context = new ApiContext())
            {
                context.Database.EnsureCreated();
            }
        }

        public StringProperty? CreateStringAnalysis(string stringValue)
        {
            try
            {
                using (var context = new ApiContext())
                {
                    string strValue = stringValue.ToLower();
                    string palindromeString = Regex.Replace(strValue.Replace(" ", ""), "[^a-zA-Z0-9]", "");
                    string sha256Id = StringUtils.GenerateSHA256HashValue(stringValue);

                    StringProperty newStringProperty = new StringProperty()
                    {
                        Id = sha256Id,
                        Value = stringValue,
                        Length = stringValue.Length,
                        IsPalindrome = StringUtils.IsPalindrome(palindromeString),
                        UniqueCharacters = StringUtils.UniqueCharacterCount(palindromeString),
                        WordCount = StringUtils.WordCount(strValue),
                        Sha256Hash = sha256Id,
                        CharacterFrequencyMap = StringUtils.CharacterFrequency(palindromeString),
                        CreatedAt = DateTime.Now,
                    };

                    context.StringProperties.Add(newStringProperty);
                    context.SaveChanges();

                    return newStringProperty;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error message: {ex.Message}");
                return null;
            }
        }

        public StringProperty? GetStringAnalysisByValue(string value)
        {
            try
            {
                using (var context = new ApiContext())
                {
                    StringProperty? strProp = context.StringProperties.FirstOrDefault<StringProperty>(sp => sp.Value == value);
                    return strProp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error message: {ex.Message}");
                return null;
            }
        }

        public ICollection<StringProperty>? GetAllStringAnalysis()
        {
            try
            {
                using (var context = new ApiContext())
                {
                    return context.StringProperties.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error message: {ex.Message}");
                return null;
            }
        }

        public string DeleteStringAnalysis(string value)
        {
            try
            {
                using (var context = new ApiContext())
                {
                    StringProperty? stringProperty = context.StringProperties.FirstOrDefault<StringProperty>(sp => sp.Value == value);
                    if (stringProperty != null)
                    {
                        context.StringProperties.Remove(stringProperty);
                        context.SaveChanges();

                        return "deleted";
                    }

                    return "not deleted";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error message: {ex.Message}");
                return "not deleted";
            }
        }
    }
}
