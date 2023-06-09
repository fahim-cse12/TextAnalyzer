using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace TextAnalysisService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TextAnalyzerController : ControllerBase
    {
        [HttpPost("analyze")]
        public ActionResult<TextAnalysisResult> AnalyzeText(TextAnalysisInput input)
        {
            // Data validation
            if (string.IsNullOrEmpty(input.Text))
            {
                return BadRequest("Input text is required.");
            }

            // Remove spaces and punctuation
            var textWithoutSpacesAndPunctuation = Regex.Replace(input.Text, @"\s+|[\p{P}-[.]]+", "");

            // Calculate character count
            var charCount = textWithoutSpacesAndPunctuation.Length;

            // Calculate word count
            var words = input.Text.Split(' ');
            var wordCount = words.Length;

            // Calculate sentence count
            var sentenceCount = input.Text.Split('.').Length - 1;

            // Calculate most frequent word and its frequency
            var wordFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var word in words)
            {
                var cleanedWord = Regex.Replace(word, @"[\p{P}-[.]]+", ""); // Remove punctuation
                if (wordFrequencies.ContainsKey(cleanedWord))
                {
                    wordFrequencies[cleanedWord]++;
                }
                else
                {
                    wordFrequencies[cleanedWord] = 1;
                }
            }
            var mostFrequentWord = wordFrequencies.OrderByDescending(x => x.Value).FirstOrDefault();

            // Calculate longest word and its length
            var longestWord = words.OrderByDescending(x => Regex.Replace(x, @"[\p{P}-[.]]+", "").Length).FirstOrDefault();

            var result = new TextAnalysisResult
            {
                CharCount = charCount,
                WordCount = wordCount,
                SentenceCount = sentenceCount,
                MostFrequentWord = mostFrequentWord.Value != null ? new WordFrequency
                {
                    Word = mostFrequentWord.Key,
                    Frequency = mostFrequentWord.Value
                } : null,
                LongestWord = longestWord != null ? new WordLength
                {
                    Word = longestWord,
                    Length = Regex.Replace(longestWord, @"[\p{P}-[.]]+", "").Length
                } : null
            };

            return Ok(result);
        }

        [HttpPost("similarities")]
        public IActionResult CalculateTextSimilarity([FromBody] TextSimilarityInput input)
        {
            // Validate input
            if (string.IsNullOrEmpty(input.Text1) || string.IsNullOrEmpty(input.Text2))
            {
                return BadRequest("Both text1 and text2 must be provided.");
            }

            // Split the input texts into individual words
            var words1 = Regex.Replace(input.Text1.ToLower(), @"[\p{P}-[.]]+", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var words2 = Regex.Replace(input.Text2.ToLower(), @"[\p{P}-[.]]+", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Calculate unique word counts for text1 and text2
            string[] uniqueWords1 = words1;
            string[] uniqueWords2 = words2;

            // Calculate the count of unique words that appear in both text1 and text2

            int commonWordCountForText1 = CountCommonWords(uniqueWords1, uniqueWords2);
            int commonWordCountForText2 = CountCommonWords(uniqueWords2, uniqueWords1);


            // Calculate similarity percentage for text1
            var similarityPercentageText1 = (commonWordCountForText1 * 100.0) / uniqueWords1.Count();

            // Calculate similarity percentage for text2
            var similarityPercentageText2 = (commonWordCountForText2 * 100.0) / uniqueWords2.Count();

            // Calculate the average similarity percentage
            var averageSimilarityPercentage = (similarityPercentageText1 + similarityPercentageText2) / 2;

            // Create the similarity result object
            var result = new TextSimilarityResult
            {
                Similarity = Math.Round(averageSimilarityPercentage, 2)
            };

            return Ok(result);
        }    

        private int CountCommonWords(string[] text1, string[] text2)
        {         
            // Count the common words
            int commonWordCount = 0;
            foreach (string word1 in text1)
            {
                foreach (string word2 in text2)
                {
                    if (word1.Equals(word2, StringComparison.OrdinalIgnoreCase))
                    {
                        commonWordCount++;
                        break;
                    }
                }
            }

            return commonWordCount;
        }


    }

    public class TextAnalysisInput
    {
        public string Text { get; set; }
    }

    public class TextAnalysisResult
    {
        public int CharCount { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public WordFrequency MostFrequentWord { get; set; }
        public WordLength LongestWord { get; set; }
    }

    public class WordFrequency
    {
        public string Word { get; set; }
        public int Frequency { get; set; }
    }

    public class WordLength
    {
        public string Word { get; set; }
        public int Length { get; set; }
    }

    public class TextSimilarityInput
    {
        public string Text1 { get; set; }
        public string Text2 { get; set; }

        
    }

    public class TextSimilarityResult
    {
        public double Similarity { get; set; }
    }


}
