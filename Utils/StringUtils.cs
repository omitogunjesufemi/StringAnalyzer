using System.Security.Cryptography;
using System.Text;
namespace StringAnalyzer.Utils
{
    public class CreateRequestBody
    {
        public string value { get; set; }
    }

    public class StringUtils
    {
        // Bool of whether the string is a palindrome
        public static bool IsPalindrome(string value)
        { 
            if (string.IsNullOrEmpty(value)) return false;

            if (value.Length == 1) return true;

            for (int i = 0; i < value.Length/2; i++)
            {
                if (value[i] != value[value.Length - i - 1])
                    return false;
            }

            return true;
        }

        // SHA256 hash value of the string
        public static string GenerateSHA256HashValue(string value)
        {
            using (SHA256 sHA256 = SHA256.Create())
            {
                byte[] hashBytes = sHA256.ComputeHash(Encoding.UTF8.GetBytes(value));

                StringBuilder hashStringBuilder = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("X2").ToLower());
                }

                return hashStringBuilder.ToString();
            }
        }

        // Count of distinct characters in the string
        public static int UniqueCharacterCount(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            HashSet<char> valueChars = new HashSet<char>();

            foreach (char c in value)
            {
                valueChars.Add(c);
            }

            return valueChars.Count;
        }

        // Number of words separated by whitespace
        public static int WordCount(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            string[] valueSplit = value.Split(" ");
            
            if (valueSplit.Length == 0) return 0;
            
            return valueSplit.Length;
        }

        // Frequency of each character in the string
        public static Dictionary<char, int>? CharacterFrequency(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            Dictionary<char, int> charFrequency = new Dictionary<char, int>();

            foreach (char item in value)
            {
                if (charFrequency.ContainsKey(item))
                {
                    charFrequency[item]++;
                }
                else
                {
                    charFrequency[item] = 1;
                }
            }

            return charFrequency;
        }
    }
}
