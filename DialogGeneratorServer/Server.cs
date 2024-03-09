using DialogGeneratorServer.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DialogGeneratorServer
{
    internal class Server
    {
        private HttpListener listener;

        public Server()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
        }

        public void Start()
        {
            Logger.LogDebug("Starting server");
            listener.Start();
            _ = ListenRequest();
        }

        public void Stop()
        {
            Logger.LogDebug("Stopping server");
            listener.Stop();
        }

        public void Dispose()
        {
            //
        }

        private async Task ListenRequest()
        {
            Logger.LogDebug("ListenRequest started");
            while (listener.IsListening)
            {
                Logger.LogDebug("Waiting GetContextAsync()");
                HttpListenerContext context = await listener.GetContextAsync();

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // 接続情報
                Logger.Log("Conn from: " + request.RemoteEndPoint.ToString());
                Logger.Log($"Req addr: HTTP/{request.ProtocolVersion} {request.HttpMethod} {request.RawUrl}");
                string header = "";
                foreach (string key in request.Headers.AllKeys)
                {
                    header += key + ": " + request.Headers[key] + "\n";
                }
                Logger.LogDebug("Headers: " + header);

                // 処理
                try
                {
                    if (request.HttpMethod == "GET")
                    {
                        Logger.LogDebug("in GET method");
                        var sb = new StringBuilder();
                        sb.AppendLine("Request parameter");
                        sb.AppendLine("JSON UTF-8");
                        sb.AppendLine("{");
                        sb.AppendLine("\"title\": \"タイトル文字列\",");
                        sb.AppendLine("\"description\": \"内容文字列\",");
                        sb.AppendLine("\"button\": ,");
                        sb.AppendLine("\"icon\": ,");
                        sb.AppendLine("}");
                        sb.AppendLine("buttonに指定可能な数値:");
                        var buttonValue = Enum.GetValues(typeof(MessageBoxButtons));
                        var buttonKey = Enum.GetNames(typeof(MessageBoxButtons));
                        for (int i = 0; i < buttonValue.Length; i++)
                        {
                            sb.AppendLine($"{buttonKey[i]}: {(int)buttonValue.GetValue(i)}");
                        }
                        sb.AppendLine("iconに指定可能な数値:");
                        var iconValue = Enum.GetValues(typeof(MessageBoxIcon));
                        var iconKey = Enum.GetNames(typeof(MessageBoxIcon));
                        for (int i = 0; i < iconValue.Length; i++)
                        {
                            sb.AppendLine($"{iconKey[i]}: {(int)iconValue.GetValue(i)}");
                        }

                        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());

                        response.StatusCode = 200;
                        response.ContentType = "text/plain";
                        response.ContentLength64 = bytes.Length;
                        response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        Logger.LogDebug("in POST method");

                        // Request解析
                        Logger.LogDebug("Deserialize data");
                        RequestData data = new RequestData();
                        var reqSerialzer = new DataContractJsonSerializer(typeof(RequestData));
                        data = (RequestData)reqSerialzer.ReadObject(request.InputStream);
                        if (data == null) throw new RequestDataException("入力がnullです。");
                        if (!Enum.IsDefined(typeof(MessageBoxButtons), data.Button)) throw new RequestDataException("button値を変換できません。");
                        if (!Enum.IsDefined(typeof(MessageBoxIcon), data.Icon)) throw new RequestDataException("icon値を変換できません。");

                        // ウィンドウキャプチャ
                        Logger.LogDebug("Call GetDialogBitmap(..)");
                        Bitmap image = ProcessConnect.GetDialogBitmap(data.Title, data.Description, data.Button, data.Icon);

                        // 画像変換
                        Logger.LogDebug("Converting Bitmap");
                        byte[] imageBytes;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Png);
                            imageBytes = ms.ToArray();
                        }

                        // Response書き込み
                        Logger.LogDebug("Writing response");
                        response.ContentType = "image/png";
                        response.ContentLength64 = imageBytes.Length;
                        response.OutputStream.Write(imageBytes, 0, imageBytes.Length);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    int status = 500;
                    string text;
                    if (ex is SerializationException)
                    {
                        status = 400;
                        text = new RequestDataException("入力が正しくありません。", ex).ToString();
                    }
                    else if (ex is RequestDataException)
                    {
                        status = 400;
                        text = ex.ToString();
                    }
                    else
                    {
                        text = ex.ToString();
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(text);

                    if (ex is HttpListenerException)
                    {
                        //
                    }
                    else
                    {
                        response.StatusCode = status;
                        response.ContentType = "text/plain";
                        response.ContentLength64 = bytes.Length;
                        response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                }

                response.Close();
            }
        }
    }
}
