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
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid request body or missing \"value\" field");
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
        public IActionResult GetAllStringsByFilter(
            [FromQuery] string? is_palindrome, 
            [FromQuery] int? min_length, 
            [FromQuery] int? max_length, 
            [FromQuery] int? word_count, 
            [FromQuery] string? contains_character )
        {
            ICollection<StringProperty>? stringProperties = _stringRepository.GetAllStringAnalysis();
            
            if (stringProperties == null || stringProperties.Count == 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, "No strings exist in the system");
            }
            
            var filteredStringProperties = stringProperties.AsQueryable();

            Dictionary<string, object> filtersApplied = new();

            bool? isPalindromeBool = null;
            if (!string.IsNullOrEmpty(is_palindrome))
            {
                if (is_palindrome.ToLower() == "true")
                    isPalindromeBool = true;
                else if (is_palindrome.ToLower() == "false")
                    isPalindromeBool = false;
                else
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid query parameter values or types");

                filteredStringProperties = filteredStringProperties.Where(sp => sp.IsPalindrome == isPalindromeBool);
                filtersApplied["is_palindrome"] = isPalindromeBool;
            }

            if (min_length.HasValue)
            {
                if (min_length < 0)
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid query parameter values or types");

                filteredStringProperties = filteredStringProperties.Where(sp => sp.Length >= min_length);
                filtersApplied["min_length"] = min_length;
            }

            if (max_length.HasValue)
            {
                if (max_length < 0)
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid query parameter values or types");
                
                filteredStringProperties = filteredStringProperties.Where(sp => sp.Length <= max_length);
                filtersApplied["max_length"] = max_length;
            }
            
            if (word_count.HasValue)
            {
                if (word_count < 0)
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid query parameter values or types");

                filteredStringProperties = filteredStringProperties.Where(sp => sp.WordCount == word_count);
                filtersApplied["word_count"] = word_count;
            }

            if (!string.IsNullOrEmpty(contains_character))
            {
                if (contains_character.Length < 1)
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid query parameter values or types");

                filteredStringProperties = filteredStringProperties.Where(sp => sp.CharacterFrequencyMap.ContainsKey(contains_character[0]));
                filtersApplied["contains_character"] = contains_character;
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

            return Ok(new
            {
                data = allStringAnalyses.ToList(),
                count = allStringAnalyses.Count(),
                filters_applied = filtersApplied
            });
        }

        [HttpGet("filter-by-natural-language")]
        public IActionResult GetStringAnalysisByNaturalLanguageFilter([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return StatusCode(StatusCodes.Status400BadRequest, "Unable to parse natural language query");

            ICollection<StringProperty> ? stringProperties = _stringRepository.GetAllStringAnalysis();
            var filteredStringProperties = stringProperties.AsQueryable();

            Dictionary<string, object> parsedFilters = new();

            switch(query.Trim().ToLower())
            {
                case "all single word palindromic strings":
                    filteredStringProperties = filteredStringProperties.Where(sp => sp.IsPalindrome.ToString().ToLower() == "true" && sp.WordCount == 1);
                    parsedFilters["is_palindrome"] = true;
                    parsedFilters["word_count"] = 1;
                    break;

                case "strings longer than 10 characters":
                    filteredStringProperties = filteredStringProperties.Where(sp => sp.Length > 10);
                    parsedFilters["min_length"] = 11;
                    break;

                case "palindromic strings that contain the first vowel":
                    filteredStringProperties = filteredStringProperties.Where(sp => sp.IsPalindrome.ToString().ToLower() == "true" && sp.CharacterFrequencyMap.ContainsKey('a'));
                    parsedFilters["is_palindrome"] = true;
                    parsedFilters["contains_character"] = "a";
                    break;

                case "strings containing the letter z":
                    filteredStringProperties = filteredStringProperties.Where(sp => sp.CharacterFrequencyMap.ContainsKey('z'));
                    parsedFilters["contains_character"] = "z";
                    break;

                default:
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

            return Ok(new
            {
                data = allStringAnalyses.ToList(),
                count = allStringAnalyses.Count(),
                interpreted_query = new
                {
                    original = query,
                    parsed_filters = parsedFilters
                }
            });
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
    }
}
