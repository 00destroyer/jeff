﻿using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MangaRipper.Core
{
    /// <summary>
    /// Support download web page to string and image file to folder.
    /// </summary>
    public class Downloader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private HttpWebRequest CreateRequest(string url)
        {
            var uri = new Uri(url);
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Credentials = CredentialCache.DefaultCredentials;
            return request;
        }

        /// <summary>
        /// Download single web page to string.
        /// </summary>
        /// <param name="url">The url to download</param>
        /// <returns></returns>
        public async Task<string> DownloadStringAsync(string url)
        {
            logger.Info("> DownloadStringAsync: {0}", url);
            var request = CreateRequest(url);
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// Download a list of web page.
        /// </summary>
        /// <param name="urls">List of url</param>
        /// <param name="progress">Progress report callback</param>
        /// <param name="cancellationToken">Cancellation control</param>
        /// <returns></returns>
        internal async Task<string> DownloadStringAsync(IEnumerable<string> urls, IProgress<int> progress, CancellationToken cancellationToken)
        {
            logger.Info("> DownloadStringAsync - Total: {0}", urls.Count());
            var sb = new StringBuilder();
            int count = 0;
            progress.Report(count);
            foreach (var url in urls)
            {
                string input = await DownloadStringAsync(url);
                sb.Append(input);
                cancellationToken.ThrowIfCancellationRequested();
                progress.Report(count++);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Download file and save to folder
        /// </summary>
        /// <param name="url">The url to download</param>
        /// <param name="fileName">Save to filename</param>
        /// <param name="cancellationToken">Cancellation control</param>
        /// <returns></returns>
        public async Task DownloadFileAsync(string url, string fileName, CancellationToken cancellationToken)
        {
            logger.Info("> DownloadFileAsync: {0} - {1}", url, fileName);
            var request = CreateRequest(url);
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                using (var streamReader = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    await responseStream.CopyToAsync(streamReader, 81920, cancellationToken);
                }
            }
        }
    }
}
