using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StreamingUrlFetcherRestServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private static string executableName = "animdl";


        [HttpGet("Test")]
        public async Task<ActionResult<string>> GetTEst()
        {
            return "zweihundert";
        }


        #region Get Index of Series/Movies of AnimDL


        [HttpGet("GetStreamIndexes")]
        public async Task<ActionResult<string>> GetStreamIndexes(string seriesName)
        {
            Console.WriteLine($"Access for {seriesName}");
            string output = await ExecuteStreamIndexGrabFromAnimDl(seriesName);

            return output;
        }

        private async Task<string> ExecuteStreamIndexGrabFromAnimDl(string seriesName)
        {
            string arguments = $"grab \"{seriesName}\"";

            ProcessStartInfo processInfo = new()
            {
                FileName = executableName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            StringBuilder outputBuilder = new();
            using (Process process = new())
            {
                process.StartInfo = processInfo;
                process.EnableRaisingEvents = true;

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                try
                {
                    process.Start();
                    process.BeginErrorReadLine();

                    await Task.WhenAny(Task.Delay(5000), process.WaitForExitAsync());

                    if (!process.HasExited)
                    {
                        process.Kill();
                    }

                    await process.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    outputBuilder.AppendLine($"Exception occurred: {ex.Message}");
                }

                StringBuilder output = new();
                int count = 0;

                foreach (string zeile in outputBuilder.ToString().Split("\n"))
                {
                    if (count >= 6)
                    {
                        if (!string.IsNullOrEmpty(zeile) || !string.IsNullOrWhiteSpace(zeile))
                        {
                            output.AppendLine(zeile.Replace("   ", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty));
                        }
                    }
                    count++;
                }

                StringBuilder filteredOutput = new();
                string input = MergeLinesStartingWithNumber(output.ToString());

                List<StreamElement> elements = ConvertStreamDataToElements(input);
                string ElementsAsJson = JsonSerializer.Serialize(elements);

                return ElementsAsJson;
            }
        }

        private static List<StreamElement> ConvertStreamDataToElements(string filteredStreamData)
        {
            List<StreamElement> output = [];
            string[] rawElements = filteredStreamData.Split("\n");

            foreach (string rawElement in rawElements)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(rawElement))
                    {
                        string[] PartIndex = rawElement.Split(". ");
                        int IndexPart = int.Parse(PartIndex[0]);

                        string[] PartTitleUrl = rawElement.Replace($"{IndexPart}. ", string.Empty).Split(" / ");
                        string TitlePart = PartTitleUrl[0];

                        string UrlPart = PartTitleUrl[1].Trim();
                        output.Add(new StreamElement(IndexPart, TitlePart, UrlPart));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(rawElement + "\n");
                }
            }

            return output;
        }

        static string MergeLinesStartingWithNumber(string inputText)
        {
            string[] lines = inputText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Regex numberPattern = new Regex(@"^\d+\.");

            string result = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                if (numberPattern.IsMatch(lines[i]))
                {
                    result += lines[i].Trim();

                    if (i + 1 < lines.Length && !numberPattern.IsMatch(lines[i + 1]))
                    {
                        result += " " + lines[i + 1].Trim();
                        i++;
                    }

                    result += "\n";
                }
            }

            return result;
        }

        public class StreamElement
        {
            public StreamElement(int index, string title, string url)
            {
                Index = index;
                Title = title;
                Url = url;
            }

            public int Index { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }

        }

        #endregion  Get Index of Series/Movies of AnimDL

        #region Get StreamEpisodes of AnimDl

        [HttpGet("GetStreamEpisodes")]
        public async Task<ActionResult<string>> GetStreamEpisodes(string seriesName, int streamIndex)
        {
            Console.WriteLine($"Stream access for {seriesName}");
            string output = await ExecuteEpisodeGrabFromAnimDl(seriesName, streamIndex);

            StringBuilder CompleteJson = ProcessRawEpisodesData(output);
            string CompleteJsonOutput = CompleteJson.ToString();

            return CompleteJsonOutput;
        }

        private static StringBuilder ProcessRawEpisodesData(string output)
        {
            List<string> StreamElements = output.Split("\n").ToList();
            StreamElements.RemoveAt(StreamElements.Count - 1);

            string[] StreamElementsFiltered = StreamElements.ToArray();
            StringBuilder CompleteJson = new();

            int ElementCounter = 0;
            CompleteJson.AppendLine("[");

            foreach (string streamElement in StreamElementsFiltered)
            {
                ElementCounter++;

                if (ElementCounter >= StreamElementsFiltered.Length)
                {
                    CompleteJson.AppendLine(streamElement);
                }
                else
                {
                    CompleteJson.AppendLine($"{streamElement},");
                }
            }
            CompleteJson.AppendLine("]");
            return CompleteJson;
        }

        private async Task<string> ExecuteEpisodeGrabFromAnimDl(string seriesName, int streamIndex)
        {
            string arguments = $"grab \"{seriesName}\"";

            if (streamIndex != 0)
            {
                arguments = $"grab \"{seriesName}\" --index {streamIndex}";
            }

            ProcessStartInfo processInfo = new()
            {
                FileName = executableName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            StringBuilder outputBuilder = new();
            using (Process process = new())
            {
                process.StartInfo = processInfo;
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();

                await Task.Delay(5000);
                if (!process.HasExited)
                {
                    process.Kill();
                }

                process.WaitForExit();
                return outputBuilder.ToString();
            }
        }

        #endregion Get StreamEpisodes of AnimDl

    }
}