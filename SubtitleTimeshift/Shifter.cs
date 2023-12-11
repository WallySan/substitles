using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        async static public Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            //throw new NotImplementedException();
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            string remainder = "";

            //Enquanto tiver conteudo, vai lendo de 1024 em 1024
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                string text = remainder + Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                string pattern = @"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{1,2},\d{3}";

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (Regex.IsMatch(lines[i], pattern))
                    {
                        string shiftedTiming = CorrigeTempo(timeSpan, lines, i);
                        string content = shiftedTiming;
                        contentToFile(output, content);
                    }
                    else
                    {
                        string content = lines[i];
                        contentToFile(output, content);
                    }
                }

                remainder = lines[lines.Length - 1];

            }

        }

        private static void contentToFile(Stream output, string content)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(content + "\r\n");
            output.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        private static string CorrigeTempo(TimeSpan timeSpan, string[] lines, int i)
        {
            string[] timing = lines[i].Split(new string[] { " --> " }, StringSplitOptions.None);

            string[] formats = new string[] { "hh\\:mm\\:ss\\,fff", "h\\:mm\\:ss\\,fff", "hh\\:m\\:ss\\,fff", "h\\:mm\\:s\\,fff" };


            TimeSpan startTime;
            TimeSpan.TryParseExact(timing[0], formats, null, out startTime);

            TimeSpan endTime;
            TimeSpan.TryParseExact(timing[1], formats, null, out endTime);

            TimeSpan newStartTime = startTime + timeSpan;

            TimeSpan newEndTime = endTime + timeSpan;

            string shiftedTiming = $"{(int)newStartTime.TotalHours:D2}:{newStartTime.Minutes:D2}:{newStartTime.Seconds:D2}.{newStartTime.Milliseconds:D3} --> {(int)newEndTime.TotalHours:D2}:{newEndTime.Minutes:D2}:{newEndTime.Seconds:D2}.{newEndTime.Milliseconds:D3}";
            return shiftedTiming;
        }
    }
}
