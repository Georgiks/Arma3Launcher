using Arma3Launcher.Frontend.Console;
using Arma3Launcher.Model.Index;
using Arma3Launcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arma3Launcher.Core
{
    class ConcurrentAddonDownloader
    {
        SemaphoreSlim Semaphore;
        Printer Printer;

        public ConcurrentAddonDownloader(Printer printer, int maxConcurrency = 5)
        {
            Semaphore = new SemaphoreSlim(5);
            Printer = printer;
        }

        public async Task DownloadFiles(List<AddonFile> files, string source, DirectoryInfo modpackDirectory)
        {
            FileDownloadStatus[] downloaded = new FileDownloadStatus[files.Count];
            for (int i = 0; i < downloaded.Length; i++)
            {
                downloaded[i] = new FileDownloadStatus(files[i].Size);
            }
            var downloadTask = DownloadWorker(files, source, modpackDirectory, downloaded);
            await PrintFilesDownload(downloadTask, downloaded);
        }

        private async Task DownloadWorker(List<AddonFile> files, string source, DirectoryInfo modpackDirectory, FileDownloadStatus[] states)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < files.Count; i++)
            {
                await Semaphore.WaitAsync();

                AddonFile f = files[i];
                string localPath = Path.Combine(modpackDirectory.FullName, f.Path);
                string remotePath = Path.Combine(source, f.Path);

                using (var c = new WebClient())
                {
                    c.DownloadProgressChanged += states[i].DownloadProgressHandler;
                    c.DownloadFileCompleted += DownloadCompleted;
                    string dir = Path.GetDirectoryName(localPath);

                    Directory.CreateDirectory(dir);
                    var t = c.DownloadFileTaskAsync(
                            remotePath,
                            localPath
                        );
                    tasks.Add(t);
                }
            }
            await Task.WhenAll(tasks);
        }

        private void DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Semaphore.Release();
        }

        private async Task PrintFilesDownload(Task downloadTask, FileDownloadStatus[] states)
        {
            bool exit = false;
            while (true)
            {
                int finished = states.Where(s => s.Downloaded == s.TotalSize).Count();
                long total = states.Select(s => s.TotalSize).Sum();
                long current = states.Select(s => s.Downloaded).Sum();
                double percentage = current / (double)total * 100;
                Printer?.Print($"\r [{percentage:0.00} %] {MiscUtils.BytesToString(current)}/{MiscUtils.BytesToString(total)} ({finished} / {states.Length} Files)   ");

                if (exit)
                {
                    Printer?.PrintLine("");
                    break;
                }

                if (await Task.WhenAny(downloadTask, Task.Delay(500)) == downloadTask)
                {
                    await downloadTask;
                    exit = true;
                }
            }
        }

        private class FileDownloadStatus
        {
            public long TotalSize;
            public long Downloaded;

            public FileDownloadStatus(long fileSize)
            {
                TotalSize = fileSize;
            }

            public void DownloadProgressHandler(object sender, DownloadProgressChangedEventArgs e)
            {
                Downloaded = e.BytesReceived;
            }
        }
    }
}
