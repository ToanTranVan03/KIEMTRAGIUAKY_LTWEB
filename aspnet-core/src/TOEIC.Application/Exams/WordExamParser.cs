using Abp.Domain.Repositories;
using Abp.UI;
using TOEIC.AppEntities;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TOEIC.Exams
{
    /// <summary>
    /// Parses Word (.docx) files with specific tags to extract TOEIC Reading exam data.
    /// Tags: [EXAM_TITLE], [DURATION], [PART:5], [PART:6], [PART:7],
    ///        [PASSAGE], [END_PASSAGE], [Q:101], [A:...], [B:...], [C:...], [D:...], [KEY:B], [SHUFFLE:FALSE]
    /// </summary>
    public class WordExamParser
    {
        public class ParseResult
        {
            public string Title { get; set; }
            public int Duration { get; set; }
            public List<Passage> Passages { get; set; } = new();
            public List<Question> Questions { get; set; } = new();
            public List<string> Errors { get; set; } = new();
        }

        public ParseResult Parse(Stream fileStream)
        {
            var result = new ParseResult();
            try
            {
                using var doc = WordprocessingDocument.Open(fileStream, false);
                var body = doc.MainDocumentPart?.Document?.Body;
                if (body == null)
                {
                    result.Errors.Add("File Word không có nội dung.");
                    return result;
                }

                var lines = body.Elements<Paragraph>()
                    .Select(p => p.InnerText.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                int currentPart = 0;
                Passage currentPassage = null;
                Question currentQuestion = null;
                bool inPassage = false;
                var passageContent = new List<string>();

                foreach (var line in lines)
                {
                    // [EXAM_TITLE]
                    var matchTitle = Regex.Match(line, @"\[EXAM_TITLE\]\s*(.+)", RegexOptions.IgnoreCase);
                    if (matchTitle.Success)
                    {
                        result.Title = matchTitle.Groups[1].Value.Trim();
                        continue;
                    }

                    // [DURATION]
                    var matchDuration = Regex.Match(line, @"\[DURATION\]\s*(\d+)", RegexOptions.IgnoreCase);
                    if (matchDuration.Success)
                    {
                        result.Duration = int.Parse(matchDuration.Groups[1].Value);
                        continue;
                    }

                    // [PART:5], [PART:6], [PART:7]
                    var matchPart = Regex.Match(line, @"\[PART\s*:\s*(\d+)\]", RegexOptions.IgnoreCase);
                    if (matchPart.Success)
                    {
                        currentPart = int.Parse(matchPart.Groups[1].Value);
                        continue;
                    }

                    // [PASSAGE]
                    if (Regex.IsMatch(line, @"\[PASSAGE\]", RegexOptions.IgnoreCase))
                    {
                        inPassage = true;
                        passageContent.Clear();
                        continue;
                    }

                    // [END_PASSAGE]
                    if (Regex.IsMatch(line, @"\[END_PASSAGE\]", RegexOptions.IgnoreCase))
                    {
                        inPassage = false;
                        currentPassage = new Passage
                        {
                            Content = string.Join("\n", passageContent)
                        };
                        result.Passages.Add(currentPassage);
                        continue;
                    }

                    if (inPassage)
                    {
                        passageContent.Add(line);
                        continue;
                    }

                    // [Q:101]
                    var matchQ = Regex.Match(line, @"\[Q\s*:\s*(\d+)\]\s*(.*)", RegexOptions.IgnoreCase);
                    if (matchQ.Success)
                    {
                        currentQuestion = new Question
                        {
                            QuestionNo = int.Parse(matchQ.Groups[1].Value),
                            Content = matchQ.Groups[2].Value.Trim(),
                            PartNumber = currentPart,
                            IsShuffle = true // default
                        };
                        // Assign passage if Part 6 or 7
                        if ((currentPart == 6 || currentPart == 7) && currentPassage != null)
                        {
                            // Will link later
                            currentQuestion.PassageId = -(result.Passages.IndexOf(currentPassage) + 1);
                        }
                        result.Questions.Add(currentQuestion);
                        continue;
                    }

                    // [A:...], [B:...], [C:...], [D:...]
                    var matchOpt = Regex.Match(line, @"\[([A-D])\s*:\s*(.+)\]", RegexOptions.IgnoreCase);
                    if (matchOpt.Success && currentQuestion != null)
                    {
                        var opt = matchOpt.Groups[1].Value.ToUpper();
                        var val = matchOpt.Groups[2].Value.Trim();
                        switch (opt)
                        {
                            case "A": currentQuestion.OptionA = val; break;
                            case "B": currentQuestion.OptionB = val; break;
                            case "C": currentQuestion.OptionC = val; break;
                            case "D": currentQuestion.OptionD = val; break;
                        }
                        continue;
                    }

                    // [KEY:B]
                    var matchKey = Regex.Match(line, @"\[KEY\s*:\s*([A-D])\]", RegexOptions.IgnoreCase);
                    if (matchKey.Success && currentQuestion != null)
                    {
                        currentQuestion.CorrectAnswer = matchKey.Groups[1].Value.ToUpper();
                        continue;
                    }

                    // [SHUFFLE:FALSE]
                    var matchShuffle = Regex.Match(line, @"\[SHUFFLE\s*:\s*(TRUE|FALSE)\]", RegexOptions.IgnoreCase);
                    if (matchShuffle.Success && currentQuestion != null)
                    {
                        currentQuestion.IsShuffle = matchShuffle.Groups[1].Value.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                }

                // Validations
                if (string.IsNullOrEmpty(result.Title))
                    result.Errors.Add("Thiếu tag [EXAM_TITLE]");

                if (result.Duration <= 0)
                    result.Errors.Add("Thiếu hoặc sai tag [DURATION]");

                foreach (var q in result.Questions)
                {
                    if (string.IsNullOrEmpty(q.CorrectAnswer))
                        result.Errors.Add($"Câu {q.QuestionNo}: Thiếu đáp án đúng [KEY]");
                    if (string.IsNullOrEmpty(q.OptionA) || string.IsNullOrEmpty(q.OptionB) ||
                        string.IsNullOrEmpty(q.OptionC) || string.IsNullOrEmpty(q.OptionD))
                        result.Errors.Add($"Câu {q.QuestionNo}: Thiếu đáp án A/B/C/D");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Lỗi đọc file Word: {ex.Message}");
            }

            return result;
        }
    }
}
