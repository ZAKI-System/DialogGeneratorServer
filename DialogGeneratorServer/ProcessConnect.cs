using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DialogGeneratorServer.Exceptions;

namespace DialogGeneratorServer
{
    internal class ProcessConnect
    {
        /// <summary>
        /// DialogGeneratorを起動し、ウィンドウキャプチャした画像を返す
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Bitmap GetDialogBitmap(string title, string message, int button, int icon)
        {
            Logger.LogDebug("Create new Process");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Config.GetString("ExeFileName", "DialogGenerator.exe"),
                    Arguments = "remote",
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };

            try
            {
                Logger.LogDebug("Start process");
                process.Start();

                // Pipe接続
                Logger.LogDebug("Prepare NamedPipeClientStream");
                using (var client = new NamedPipeClientStream(process.ProcessName))
                {
                    Logger.LogDebug("Waiting Pipe Connect(..)");
                    client.Connect(Config.GetInt("PipeTimeout", 10000));

                    Logger.LogDebug("Sending data");
                    SendData(client, message ?? string.Empty);
                    SendData(client, title ?? string.Empty);
                    SendData(client, button.ToString());
                    SendData(client, icon.ToString());
                    Logger.LogDebug("Send OK");
                }

                // Handle check
                Logger.LogDebug("Search process handles");
                List<IntPtr> handles = new List<IntPtr>();
                for (int i = 0; i < Config.GetInt("HandleCheckCount", 10); i++)
                {
                    Logger.LogDebug("Waiting process check:" + i);
                    Thread.Sleep(Config.GetInt("HandleCheckInterval", 100));

                    Logger.LogDebug("Process exit check:" + i);
                    if (process.HasExited) throw new Exception("キャプチャ前にプロセスが終了しました。");

                    Logger.LogDebug("Call GetProcessHandles(..)");
                    handles = WindowHandle.GetProcessHandles(process);

                    if (handles.Count >= 1) break;
                }
                if (handles.Count == 0) throw new Exception("ウィンドウが見つかりませんでした。");

                // 待機
                Logger.LogDebug("Waiting window stable");
                Thread.Sleep(Config.GetInt("WindowWaitTime", 500));

                Logger.LogDebug("Call CaptureWindow(..)");
                Bitmap bitmap = ScreenCapture.CaptureWindow(handles.First());

                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.LogError("in catch block");
                if (ex is TimeoutException)
                {
                    throw new Exception("DialogGeneratorのプロセスの接続に失敗しました。", ex);
                }
                throw;
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }

                Logger.LogDebug("Read stderr");
                string stdErr = process.StandardError.ReadToEnd();

                Logger.LogDebug("Dispose process");
                process.Dispose();
                process.Close();

                if (!string.IsNullOrEmpty(stdErr))
                {
                    throw new ProcessException("プロセス実行中にエラーが発生しました。", new ProcessException(stdErr));
                }
            }
        }

        private static void SendData(NamedPipeClientStream pipe, string data)
        {
            // データを文字列からバイト配列に変換
            byte[] buffer = Encoding.UTF8.GetBytes(data);

            // データの長さを送信
            pipe.Write(BitConverter.GetBytes(buffer.Length), 0, 4);

            // データを送信
            pipe.Write(buffer, 0, buffer.Length);
        }
    }
}
