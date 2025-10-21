using Microsoft.AspNetCore.Mvc;
using StringAnalyzer.Models;
using StringAnalyzer.Repositories;
using StringAnalyzer.Utils;
using System.Collections.Specialized;
using System.Web;

namespace StringAnalyzer.Controllers
{
    [ApiController]
    [Route("strings")]
    public class StringController : ControllerBase
    {
        readonly IStringRepository _stringRepository;

        public StringController(IStringRepository stringRepository)
        {
            _stringRepository = stringRepository;
        }

        [Route("")]
        [HttpPost]
        public IActionResult Analyze([FromBody] CreateRequestBody requestBody)
        {
            if (requestBody == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Invalid request body or missing \"value\" field");
            }

            string value = requestBody.value;

            StringProperty? stringProperty = _stringRepository.CreateStringAnalysis(value);

            if (stringProperty == null)
            {
                return StatusCode(StatusCodes.Status409Conflict, "String already exists in the system");
            }

            var stringAnalysis = new
            {
                id = stringProperty.Id,
                value = stringProperty.Value,
                properties = new
                {
                    length = stringProperty.Length,
                    is_palindrome = stringProperty.IsPalindrome,
                    unique_characters = stringProperty.UniqueCharacters,
                    word_count = stringProperty.WordCount,
                    sha256_hash = stringProperty.Sha256Hash,
                    character_frequency_map = stringProperty.CharacterFrequencyMap,
                },
                created_at = stringProperty.CreatedAt
            };

            return StatusCode(StatusCodes.Status201Created, stringAnalysis);
        }

        [Route("{string_value}")]
        [HttpGet]
        public IActionResult GetStringAnalysis(string string_value)
        {
            StringProperty? stringProperty = _stringRepository.GetStringAnalysisByValue(string_value);

            if (stringProperty == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "String does not exist in the system");
            }

            var stringAnalysis = new
            {
                id = stringProperty.Id,
                value = stringProperty.Value,
                properties = new
                {
                    length = stringProperty.Length,
                    is_palindrome = stringProperty.IsPalindrome,
                    unique_characters = stringProperty.UniqueCharacters,
                    word_count = stringProperty.WordCount,
                    sha256_hash = stringProperty.Sha256Hash,
                    character_frequency_map = stringProperty.CharacterFrequencyMap,
                },
                created_at = stringProperty.CreatedAt
            };

            return Ok(stringAnalysis);
        }

        [HttpGet]
        public IActionResult GetAllStringsByFilter([FromQuery] string? is_palindrome, [FromQuery] int? min_length, [FromQuery] int? max_length, [FromQuery] int? word_count, [FromQuery] string? contains_character )
        {
            ICollection<StringProperty>? stringProperties = _stringRepository.GetAllStringAnalysis();
            if (stringProperties == null || stringProperties.Count == 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, "No strings exist in the system");
            }
            
            var filteredStringProperties = stringProperties.AsQueryable();

            if (!string.IsNullOrEmpty(is_palindrome) && (is_palindrome == "true" || is_palindrome == "false"))
                filteredStringProperties = filteredStringProperties.Where(sp => sp.IsPalindrome.ToString().ToLower() == is_palindrome.ToLower());
            else
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid query parameter values or types");

            if (min_length.HasValue && min_length > 0)
                filteredStringProperties = filteredStringProperties.Where(sp => sp.Length >= min_length);

            if (max_length.HasValue && max_length > 0)
                filteredStringProperties = filteredStringProperties.Where(sp => sp.Length <= min_length);

            if (word_count.HasValue && word_count > 0)
                filteredStringProperties = filteredStringProperties.Where(sp => sp.WordCount == word_count);

            if (!string.IsNullOrEmpty(contains_character) && contains_character.Length == 1)
                filteredStringProperties = filteredStringProperties.Where(sp => sp.CharacterFrequencyMap.ContainsKey(contains_character[0]));

            var allStringAnalyses = filteredStringProperties.Select(stringProperty => new
            {
                id = stringProperty.Id,
                value = stringProperty.Value,
                properties = new
                {
                    length = stringProperty.Length,
                    is_palindrome = stringProperty.IsPalindrome,
                    unique_characters = stringProperty.UniqueCharacters,
                    word_count = stringProperty.WordCount,
                    sha256_hash = stringProperty.Sha256Hash,
                    character_frequency_map = stringProperty.CharacterFrequencyMap,
                },
                created_at = stringProperty.CreatedAt
            });

            return Ok(allStringAnalyses);
        }

        [HttpGet("filter-by-natural-language")]
        public IActionResult GetStringAnalysisByNaturalLanguageFilter([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
                return StatusCode(StatusCodes.Status400BadRequest, "Unable to parse natural language query");

            //NameValueCollection queryString = HttpUtility.ParseQueryString(query);
            //if (queryString.Count == 0)
            //    return StatusCode(StatusCodes.Status400BadRequest, "Unable to parse natural language query");

            ICollection<StringProperty> ? stringProperties = _stringRepository.GetAllStringAnalysis();
            var filteredStringProperties = stringProperties.AsQueryable();

            if (query == "all single word palindromic strings")
            {
                filteredStringProperties = filteredStringProperties.Where(sp => sp.IsPalindrome.ToString().ToLower() == "true" && sp.WordCount == 1);
            }
            else if (query == "strings longer than 10 characters")
            {
                filteredStringProperties = filteredStringProperties.Where(sp => sp.Length > 10);
            }
            else if (query == "palindromic strings that contain the first vowel")
            {
                filteredStringProperties = filteredStringProperties.Where(sp => sp.IsPalindrome.ToString().ToLower() == "true" && sp.CharacterFrequencyMap.ContainsKey('a'));
            }
            else if (query == "strings containing the letter z")
            {
                filteredStringProperties = filteredStringProperties.Where(sp => sp.CharacterFrequencyMap.ContainsKey('z'));
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Unable to parse natural language query");
            }

            var allStringAnalyses = filteredStringProperties.Select(stringProperty => new
            {
                id = stringProperty.Id,
                value = stringProperty.Value,
                properties = new
                {
                    length = stringProperty.Length,
                    is_palindrome = stringProperty.IsPalindrome,
                    unique_characters = stringProperty.UniqueCharacters,
                    word_count = stringProperty.WordCount,
                    sha256_hash = stringProperty.Sha256Hash,
                    character_frequency_map = stringProperty.CharacterFrequencyMap,
                },
                created_at = stringProperty.CreatedAt
            });

            return Ok(allStringAnalyses);
        }

        [Route("{string_value}")]
        [HttpDelete]
        public IActionResult DeleteStringAnalysis([FromRoute] string string_value)
        {
            if (string.IsNullOrEmpty(string_value))
                return StatusCode(StatusCodes.Status400BadRequest, "This is a bad request");

            if (_stringRepository.GetStringAnalysisByValue(string_value) == null)
                return StatusCode(StatusCodes.Status404NotFound, "String does not exist in the system");

            _stringRepository.DeleteStringAnalysis(string_value);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [Route("all")]
        [HttpGet]
        public IActionResult GetAllStringAnalysis()
        {
            ICollection<StringProperty>? stringProperties = _stringRepository.GetAllStringAnalysis();
            if (stringProperties == null || stringProperties.Count == 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, "No strings exist in the system");
            }
            var allStringAnalyses = stringProperties.Select(stringProperty => new
            {
                id = stringProperty.Id,
                value = stringProperty.Value,
                properties = new
                {
                    length = stringProperty.Length,
                    is_palindrome = stringProperty.IsPalindrome,
                    unique_characters = stringProperty.UniqueCharacters,
                    word_count = stringProperty.WordCount,
                    sha256_hash = stringProperty.Sha256Hash,
                    character_frequency_map = stringProperty.CharacterFrequencyMap,
                },
                created_at = stringProperty.CreatedAt
            });
            return Ok(allStringAnalyses);
        }
    }
}
