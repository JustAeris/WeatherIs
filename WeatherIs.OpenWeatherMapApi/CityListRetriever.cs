using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherIs.OpenWeatherMapApi.Models;

namespace WeatherIs.OpenWeatherMapApi
{
    public static class CityListRetriever
    {
        private const string FileName = "city.list.min.json.gz";
        
        public static IList<CityListItem> CityList { get; private set; }

        public static async Task RetrieveCityList()
        {
            using var client = new WebClient();
	
            client.DownloadFile($"http://bulk.openweathermap.org/sample/{FileName}", FileName);
	
            var decompressedFile = DecompressGZip(new FileInfo(FileName));
	
            var json = await File.ReadAllTextAsync(decompressedFile);

            CityList = JsonConvert.DeserializeObject<IList<CityListItem>>(json);
        }

        private static string DecompressGZip(FileInfo fileToDecompress)
        {
            using var originalFileStream = fileToDecompress.OpenRead();
            var currentFileName = fileToDecompress.FullName;
            var newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

            using var decompressedFileStream = File.Create(newFileName);
            using var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
            
            decompressionStream.CopyTo(decompressedFileStream);

            return newFileName;
        }
    }
}