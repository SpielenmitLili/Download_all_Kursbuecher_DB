//
//  _     _ _ _               _
// | |   (_) (_)___ _   _ ___| |_ ___ _ __ ___
// | |   | | | / __| | | / __| __/ _ \ '_ ` _ \
// | |___| | | \__ \ |_| \__ \ ||  __/ | | | | |
// |_____|_|_|_|___/\__, |___/\__\___|_| |_| |_|
//                  |___/
//
// This program was created with ❤️ by Lili Urban in Limburg(Lahn)
// Contact: info@lili-urban.net
//
//

// Lädt automatisiert alle Kursbücher die unter https://kursbuch.bahn.de/ angeboten werden herunter und prüft bei weiteren Läufen anhand von Hashes, ob neue Versionen verfügbar sind. 

using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Web;

namespace Download_Kursbuecher_DB
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            string userName = Environment.UserName;
            string machineName = Environment.MachineName;
            string processName = Process.GetCurrentProcess().ProcessName;

            Console.WriteLine("#############################################################################");
            Console.WriteLine(currentTime + " | Process started for user " + userName + " on " + machineName + " in program " + processName);
            Console.WriteLine(currentTime + " | " + processName + " by Lili Urban started!");
            Console.WriteLine("#############################################################################");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");

            try
            {
                await StartDownload();
            }
            catch (Exception ex)
            {
                ErrorHandler(ex, machineName, userName, processName);
            }

            currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine("#############################################################################");
            Console.WriteLine(currentTime + " | Process completed for user " + userName + " on " + machineName + " in program " + processName);
            Console.WriteLine(currentTime + " | " + processName + " by Lili Urban ended!");
            Console.WriteLine("#############################################################################");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
        }

        static async Task StartDownload()
        {
            string alleStrecken = "https://kursbuch.bahn.de/hafas/kbview.exe/dn?rt=1&dosearch=1&searchmode=tableplus&table_nr=%20&controlpattern=P.ddd&mainframe=utable&tocinfo=reg_tab";
            string zielOrdner = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Download");

            if (!Directory.Exists(zielOrdner))
            {
                Directory.CreateDirectory(zielOrdner);
            }

            string hashListe = Path.Combine(zielOrdner, "hashes.csv");
            var bekannteHashes = LoadHashIndex(hashListe);
            object hashLock = new();
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            using var client = new HttpClient(handler);
            // Natürlicher Useragent um evtl. Sperren zu umgehen
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36");

            Console.WriteLine("Eine Liste von verfügbaren Kursbuchstrecken wird erstellt...");

            string alleStreckenHTML = await client.GetStringAsync(alleStrecken);
            var streckenURLs = ExtractKursbuchlistLinks(alleStreckenHTML);

            Console.WriteLine($"{streckenURLs.Count} verfügbare Kursbuchstrecken gefunden!");

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            await Parallel.ForEachAsync(streckenURLs, options, async (streckenURL, ct) => {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"Kursbuchstrecke unter {streckenURL} wird gerade verarbeitet");
                    Console.ResetColor();

                    string einzelneStreckeHTML = await client.GetStringAsync(streckenURL, ct);
                    var pdfLinks = ExtractPdfLinks(einzelneStreckeHTML);

                    foreach (var pdfUrl in pdfLinks)
                    {
                        try
                        {
                            await DownloadPdf(client, pdfUrl, zielOrdner, hashListe, bekannteHashes, hashLock, ct);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Fehler beim Download des Kursbuchs: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Fehler beim Auslesen der Liste an PDFs für die entsprechende Kursbuchstrecke: {ex.Message}");
                    Console.ResetColor();
                }
            });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Der Download der Kursbuchstrecken wurde erfolgreich abgeschlossen!");
            Console.ResetColor();
        }

        static List<string> ExtractKursbuchlistLinks(string html)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var doc = new HtmlDocument();
            
            doc.LoadHtml(html);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");

            if (links == null)
            {
                return result.ToList();
            }

            foreach (var link in links)
            {
                string href = WebUtility.HtmlDecode(link.GetAttributeValue("href", ""));

                if (!href.Contains("table_nr=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(href);
            }

            return result.ToList();
        }

        static List<string> ExtractPdfLinks(string html)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var doc = new HtmlDocument();

            doc.LoadHtml(html);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");

            if (links == null)
            {
                return result.ToList();
            }

            foreach (var link in links)
            {
                string href = WebUtility.HtmlDecode(link.GetAttributeValue("href", ""));

                if (!href.Contains(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(href);
            }

            return result.ToList();
        }

        static async Task DownloadPdf(HttpClient client, string pdfUrl, string zielOrdner, string hashFile, Dictionary<string, string> knownHashes, object hashLock, CancellationToken cancellationToken)
        {
            string fileName;

            try
            {
                var uri = new Uri(pdfUrl);
                var query = HttpUtility.ParseQueryString(uri.Query);

                fileName = query["filename"];

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = Path.GetFileName(uri.AbsolutePath);
                }
            }
            catch
            {
                fileName = Path.GetFileName(pdfUrl);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Datei {fileName} wird geladen...");
            Console.ResetColor();

            byte[] data = await client.GetByteArrayAsync(pdfUrl, cancellationToken);
            string hash = GetSha256Hash(data);

            lock (hashLock)
            {
                if (knownHashes.TryGetValue(hash, out string? existingFile))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Datei {fileName} ist bereits vorhanden unter {existingFile}!");
                    Console.ResetColor();
                    return;
                }
            }

            string zielPath = Path.Combine(zielOrdner, fileName);

            if (File.Exists(zielPath))
            {
                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);

                zielPath = Path.Combine(zielOrdner, $"{name}_{hash[..16]}{ext}");
            }

            await File.WriteAllBytesAsync(zielPath, data, cancellationToken);

            lock (hashLock)
            {
                knownHashes[hash] = Path.GetFileName(zielPath);
                AppendHashIndex(hashFile, Path.GetFileName(zielPath), hash, pdfUrl);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Erfolgreich heruntergeladen: {Path.GetFileName(zielPath)}");
            Console.ResetColor();
        }

        static string GetSha256Hash(byte[] data)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(data));
        }

        static Dictionary<string, string> LoadHashIndex(string csvFile)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(csvFile))
            {
                return result;
            }

            foreach (var line in File.ReadLines(csvFile).Skip(1))
            {
                var splitted = line.Split(';');

                if (splitted.Length < 2)
                {
                    continue;
                }

                string fileName = splitted[0];
                string hash = splitted[1];

                if (!result.ContainsKey(hash))
                {
                    result.Add(hash, fileName);
                }
            }

            return result;
        }

        static void AppendHashIndex(string csvFile, string fileName, string sha256, string url)
        {
            bool createHeader = !File.Exists(csvFile);
            using var writer = new StreamWriter(csvFile, append: true);

            if (createHeader)
            {
                writer.WriteLine("Datei;SHA256;URL;DownloadDatum");
            }

            writer.WriteLine($"{fileName};{sha256};{url};{DateTime.UtcNow:O}");
        }

        static void ErrorHandler(Exception error, string machineName, string userName, string processName)
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine("#############################################################################");
            Console.WriteLine(currentTime + " | Error occured for user " + userName + " on " + machineName + " in program " + processName);
            Console.WriteLine(currentTime + " | Error: " + error.ToString());
            Console.WriteLine("#############################################################################");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.ResetColor();
        }
    }
}