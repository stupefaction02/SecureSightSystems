using SecureSightSystems.Core.Extensions;
using SecureSightSystems.Core.Models;
using SecureSightSystems.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SecureSightSystems.Core.Models;

namespace SecureSightSystems.Core
{
    public class FrameController : IDisposable
    {
        #region Events

        public event Action<Bitmap> FrameReceived;

        public event Action NoDataOccured;

        public event Action<Exception> ReadNetworkStreamError;

        public event Action<Exception> HostIsUnreachableError;

        /// <summary>
        /// Fires when source has been changed successufully
        /// </summary>
        public event Action<ChannelMetadata> SourceChanged;

        #endregion

        public Dispatcher Dispatcher { get; set; }

        private readonly IApiClient apiClient;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly static int IMAGE_BUFFER_SIZE = 1024 * 1024;

        private readonly static byte[] JPEG_HEADER_BYTES = new byte[] { 0xFF, 0xD8, 0xFF };

        private bool isRunning = false;

        private int CHUNK_SIZE = 1024;

        private Thread FrameUpdateThread;

        public FrameController(IApiClient apiClient, ILoggerFactory loggerFactory)
        {
            this.apiClient = apiClient;
            this.loggerFactory = loggerFactory;

            logger = loggerFactory.CreateLogger(nameof(FrameController));
        }

        CancellationTokenSource startReceivingDataCancelationTokenSource;
        Task currentTask;
        public async Task SetChannelAsync(ChannelMetadata ch)
        {
            var sw = new Stopwatch();

            sw.Start();

            Reset();

            Stream networkStream = null;
            try
            {
                networkStream = await apiClient.GetVideoDataAsync(ch.ChannelId, ch.Resolution.X, ch.Resolution.Y);

                startReceivingDataCancelationTokenSource = new CancellationTokenSource();

                currentTask = new Task(async () => await StartReceivingData(networkStream, ch, startReceivingDataCancelationTokenSource.Token), TaskCreationOptions.LongRunning);

                currentTask.Start();
            }
            catch (HttpRequestException ex)
            {
                logger.LogDebug($"apiClient.GetVideoDataAsync() returns exception. Can't reach host. Exception: {ex.Message}");
                HostIsUnreachableError?.Invoke(ex);
            }

            sw.Stop();
            logger.LogInformation($"SetChannelAsync {sw.ElapsedMilliseconds}");
        }

        private Stream videoStream;

        private async Task StartReceivingData(Stream videoStream, ChannelMetadata source, CancellationToken token)
        {
            isRunning = true;

            int position = 0;

            int totalReadBytes = 0;

            byte[] imageBuffer = new byte[IMAGE_BUFFER_SIZE];

            int boundaryIndex = -1;

            int imageHeaderIndex = -1;

            int remainingBytes = 0;

            int readCounts = 0;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] buffer = new byte[CHUNK_SIZE];
                    int readBytes = videoStream.Read(buffer, 0, CHUNK_SIZE);

                    readCounts++;

                    if (readBytes == 0)
                    {
                        NoDataOccured?.Invoke();

                        logger.LogWarning($"No data received. Channel.Id={source.ChannelId}, Channel.Name={source.Name}");

                        await Task.Delay(3000);
                    }

                    Array.Copy(buffer, 0, imageBuffer, totalReadBytes, readBytes);
                    totalReadBytes += readBytes;

                    remainingBytes = (totalReadBytes - position);
                    if (imageHeaderIndex == -1 && remainingBytes != 0)
                    {
                        remainingBytes = totalReadBytes - position;
                        imageHeaderIndex = imageBuffer.FindSubArray(JPEG_HEADER_BYTES, position, remainingBytes);

                        if (imageHeaderIndex != -1)
                            position = imageHeaderIndex + JPEG_HEADER_BYTES.Length;
                        else
                            position = totalReadBytes;

                        remainingBytes = (totalReadBytes - position);
                    }

                    remainingBytes = (totalReadBytes - position);
                    while (imageHeaderIndex != -1 && boundaryIndex == -1 && remainingBytes != 0)
                    {
                        boundaryIndex = imageBuffer.FindSubArray(JPEG_HEADER_BYTES, position, remainingBytes);

                        if (boundaryIndex == -1)
                            position = totalReadBytes;

                        remainingBytes = (totalReadBytes - position);
                    }

                    // If it founds the frame
                    if (boundaryIndex != -1 && imageHeaderIndex != -1)
                    {
                        int length = boundaryIndex - imageHeaderIndex;

                        Stream imageStream = new MemoryStream(imageBuffer, imageHeaderIndex, length);
                        var bitmap = (Bitmap)Image.FromStream(imageStream);

                        FrameReceived?.Invoke(bitmap);

                        position = boundaryIndex + 0;
                        remainingBytes = (totalReadBytes - position);
                        Array.Copy(imageBuffer, position, imageBuffer, 0, remainingBytes);

                        totalReadBytes = remainingBytes;
                        position = 0;

                        imageHeaderIndex = -1;
                        boundaryIndex = -1;
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine("TaskCanceledException");

                logger.LogDebug($"videoStream.Read() returns exception. Read stream failure. Method={nameof(StartReceivingData)}");

                ReadNetworkStreamError?.Invoke(ex);

                Dispose();
            }
            finally
            {
                videoStream.Close();
                videoStream.Dispose();
            }
        }

        private void Reset()
        {
            startReceivingDataCancelationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            if (videoStream != null)
                videoStream.Dispose();
        }
    }
}
