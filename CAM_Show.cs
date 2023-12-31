﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Drawing;
using System.Windows;

// Понять, как можно получить кадры за прошедший час
// Воспользоваться готовым функционалом
// По хорошему бы еще найти запрос для получения видоса

namespace VideoCaptureApplication
{
    public class CAM_Show : CAM_Client
    {
        private static CancellationTokenSource? sourceToken;

        public CAM_Show(XElement channelInfoElement) : base(channelInfoElement)
        {
        }

        public event ImageReadyHandler ImageReady;
        public void ImageProcessing(Bitmap image)
        {
            ImageReady?.Invoke(image);
        }
        public static void ErrorMessageRequest(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static string GetHttpRequest(string id, string login = "root", int resolutionX = 640, int resolutionY = 480, int fps = 25)
        {
            return $"{server_url}mobile?login={login}&channelid={id}&resolutionX={resolutionX}&resolutionY={resolutionY}&fps={fps}";
        }

        private static string GetArchiveUrl(string id, DateTime? time_in, string login="root", int resolutionX = 640, int resolutionY = 480, int fps = 25, string mode = "archive")
        {
            return $"{server_url}mobile?login={login}&channelid={id}&resolutionX={resolutionX}&resolutionY={resolutionY}&fps={fps}&mode={mode}&starttime={time_in}";
        }

        public void StartTranslation(bool flag = false, DateTime? time_in = null) // true - archive, false - IRT
        {
            StopTranslation();
            sourceToken = new CancellationTokenSource();
            var token = sourceToken.Token;

            Task.Run(() => GetCameraTranslationResponse(token, flag, time_in));  
        }

        public void StopTranslation()
        {
            if (sourceToken != null)
                sourceToken.Cancel();
        }

        private async Task GetCameraTranslationResponse(CancellationToken token, bool flag, DateTime? time_in)
        {
            string videoUrl;
            if (!flag)
                videoUrl = GetHttpRequest(Id);
            else
                videoUrl = GetArchiveUrl(Id, time_in);
            WebRequest request_camera = WebRequest.Create(videoUrl);
            if (!flag)
                request_camera.Timeout = 10000;
            WebResponse response_camera;
            try
            {
                response_camera = await request_camera.GetResponseAsync();
            }
            catch (WebException e)
            {
                ErrorMessageRequest(e.Message);
                return;
            }

            await CameraProcessing(response_camera, token);
        }

        private async Task CameraProcessing(WebResponse response, CancellationToken token)
        {
            using (var stream = response.GetResponseStream())
            {
                byte[] frameData = new byte[1024 * 1024];
                int size = 0;
                using (var reader = new BinaryReader(stream))
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            const int dataLength = 5000;
                            byte[] data = reader.ReadBytes(dataLength);
                            Array.Copy(data, 0, frameData, size, data.Length);
                            size += data.Length;

                            int frameStart = FindFrame(frameData, size);
                            Array.Copy(frameData, frameStart, frameData, 0, size - frameStart);
                            size -= frameStart;
                        }
                        catch (IOException e)
                        {
                            ErrorMessageRequest(e.Message);
                            break;
                        }
                    }
                }
            }
            response.Close();
        }

        private int FindFrame(byte[] frameData, int size)
        {
            const string boundary = "--myboundary";
            byte[] bytes = Encoding.UTF8.GetBytes(boundary);
            int frameStart = InputIndex(frameData, bytes, 0, size);
            int frameEnd = InputIndex(frameData, bytes, frameStart + 1, size);

            if (frameEnd == -1)
                return frameStart > 0 ? frameStart : 0;

            byte[] separator = { 0x0d, 0x0a, 0x0d, 0x0a };
            int imageStart = InputIndex(frameData, separator, frameStart);

            byte[] imageBytes = new byte[frameEnd - imageStart - separator.Length];
            Array.Copy(frameData, imageStart + separator.Length, imageBytes, 0, frameEnd - imageStart - separator.Length);

            using (var ms = new MemoryStream(imageBytes))
            {
                Bitmap image = new Bitmap(ms);
                ImageProcessing(image);
            }

            return frameEnd;
        }

        private static int InputIndex(byte[] array, byte[] targetArray, int start = 0, int end = -1)
        {
            end = end == -1 ? array.Length : end;
            int targetLength = targetArray.Length;

            for (int i = start; i < end - targetLength; i++)
            {
                if (array[i] == targetArray[0])
                {
                    bool found = true;
                    for (int j = 1; j < targetLength; j++)
                    {
                        if (array[i + j] != targetArray[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}

