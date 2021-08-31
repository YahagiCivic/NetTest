using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatSystem;

namespace ChatSystem
{
    class main
    {
        static ChatSystem chatSystem;
        const Int32 portNo = 11000;
        const string EOF = "<EOF>";
        static readonly int maxLength = 200 + EOF.Length;
        static ChatSystem.ConnectMode connectMode;

        static bool isBot = false;
        static Random rand = new Random();
        enum FunctionMode { chat, bot, janken, jankenBOT, shiritori };
        static FunctionMode functionMode = FunctionMode.chat;

        static void Main(string[] args)
        {
            chatSystem = new ChatSystem(maxLength);
            Console.WriteLine($"this hostName is {chatSystem.hostName}.");
            functionMode = SelectFunction();
            connectMode = SelectMode();
            switch (functionMode)
            {
                case FunctionMode.chat:
                    InChat();
                    break;
                case FunctionMode.bot:
                    InChatBot();
                    break;
                case FunctionMode.janken:
                    InRPS();
                    break;
                default:
                    Console.WriteLine("not suported");
                    break;
            }
        }
        static FunctionMode SelectFunction()
        {
            Console.WriteLine("Select Function\n0= chat\n1=bot\n2=janken\n3=jankenBOT\n4=shiritori");
            int select = int.Parse(Console.ReadLine());
            FunctionMode[] function = { FunctionMode.chat, FunctionMode.bot, FunctionMode.janken, FunctionMode.jankenBOT, FunctionMode.shiritori };
            return function[select];
        }
        static ChatSystem.ConnectMode SelectMode()
        {
            ChatSystem.ConnectMode connectMode = ChatSystem.ConnectMode.host;
            Console.Write("Select Mode: 0=Host,1=Client\n");
            int select = int.Parse(Console.ReadLine());
            switch (select)
            {
                case 0: //Host
                    Console.WriteLine("Running Host mode");
                    InitializeHost();
                    connectMode = ChatSystem.ConnectMode.host;
                    break;
                case 1: //Client
                    Console.WriteLine("Running Client mode");
                    InitializeClient();
                    connectMode = ChatSystem.ConnectMode.client;
                    break;
                default:
                    Console.WriteLine("ERROR undefind");
                    break;
            }
            return connectMode;
        }
        static void InitializeHost()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(chatSystem.hostName);
            foreach (var addresslist in ipHostInfo.AddressList)
            {
                Console.WriteLine($"found own address:{addresslist.ToString()}");
            }
            Console.Write($"Select address to listen(0 - {ipHostInfo.AddressList.Length - 1}):");
            IPAddress ipAddress = ipHostInfo.AddressList[int.Parse(Console.ReadLine())];
            ChatSystem.EResult re = chatSystem.InitializeHost(ipAddress, portNo);
            if (re != ChatSystem.EResult.success)
            {
                Console.WriteLine($"failed to initialize,ERROR={re.ToString()}");
            }
        }
        static void InitializeClient()
        {
            Console.Write("Input IP address to connect:");
            var ipAddress = IPAddress.Parse(Console.ReadLine());
            ChatSystem.EResult re = chatSystem.InitializeClient(ipAddress, portNo);
            if (re == ChatSystem.EResult.success)
            {
                Console.WriteLine($"Connected host {ipAddress.ToString()}");
            }
            else
            {
                Console.WriteLine($"failed to connect to host,ERROR={chatSystem.resultMessage}");
            }
        }
        static void InChatBot()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            string received = string.Empty;
            string reply = string.Empty;
            string[] randomRep = { "私それ気になる！", "そうだね！", "分かるわぁそれ！", "いいね！", "今北産業" };
            while (true)
            {
                if (turn)
                {   // 受信
                    received = string.Empty;
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                            reply = received;
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    string inputSt = string.Empty;
                    Console.Write("送るメッセージ：");
                    if (connectMode == ChatSystem.ConnectMode.host)
                    {   // Host
                        //特定の単語を含む文章に対しては特定の文章で返される
                        if (reply.Contains("おはよう") || reply.Contains("おはようございます") || reply.Contains("Good morning") == true)
                        {
                            inputSt = "おはよう！";
                            Console.Write(inputSt);
                        }
                        else if (reply.Contains("こんにちは") || reply.Contains("Hello") == true)
                        {
                            inputSt = "こんにちは！";
                            Console.Write(inputSt);
                        }
                        else if (reply.Contains("こんばんは") || reply.Contains("Good evening") == true)
                        {
                            inputSt = "こんにちは！";
                            Console.Write(inputSt);
                        }
                        else if (reply.Contains("すき") || reply.Contains("好き") || reply.Contains("I love you") == true)
                        {
                            inputSt = "私も好き！";
                            Console.Write(inputSt);
                        }
                        else if (reply.Contains("うんこ") || reply.Contains("poop") == true)
                        {
                            inputSt = "うんこもりもり森鴎外！";
                            Console.Write(inputSt);
                        }
                        else
                        {
                            //上記の特定メッセージ以外はリストの中からランダムで返事を返す
                            Random myObject = new Random();
                            int ranNum = myObject.Next(0, 5);
                            inputSt = (randomRep[ranNum]);
                            Console.Write(inputSt);
                        }
                    }
                    else
                    {   // Client
                        inputSt = Console.ReadLine();    // 入力文字で送信
                        if (inputSt.Length > maxLength)
                        {
                            inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                        }
                    }

                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static void InChat()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt = Console.ReadLine();    // 入力文字で送信
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static void InRPS()
        {
            string RHand = string.Empty;
            string SHand = string.Empty;
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                            RHand = received;
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                    //ジャッジ
                    if (RHand.Contains("グー") && SHand.Contains("パー") == true)
                    {
                        Console.WriteLine("あなたの勝ち！");
                    }
                    else if (RHand.Contains("グー") && SHand.Contains("チョキ") == true)
                    {
                        Console.WriteLine("あなたの負け！");
                    }
                    else if (RHand.Contains("グー") && SHand.Contains("グー") == true)
                    {
                        Console.WriteLine("あいこ！");
                    }
                    else if (RHand.Contains("チョキ") && SHand.Contains("グー") == true)
                    {
                        Console.WriteLine("あなたの勝ち！");
                    }
                    else if (RHand.Contains("チョキ") && SHand.Contains("パー") == true)
                    {
                        Console.WriteLine("あなたの負け！");
                    }
                    else if (RHand.Contains("チョキ") && SHand.Contains("チョキ") == true)
                    {
                        Console.WriteLine("あいこ！");
                    }
                    else if (RHand.Contains("パー") && SHand.Contains("チョキ") == true)
                    {
                        Console.WriteLine("あなたの勝ち！");
                    }
                    else if (RHand.Contains("パー") && SHand.Contains("グー") == true)
                    {
                        Console.WriteLine("あなたの負け！");
                    }
                    else if (RHand.Contains("パー") && SHand.Contains("パー") == true)
                    {
                        Console.WriteLine("あいこ！");
                    }
                }
                else
                {   // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt = Hand();    // 入力文字で送信
                    SHand = inputSt;
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static string Hand()
        {
            string select = "";
            while (true)
            {
                Console.Write("出す手を選んでね！[0～2を入力してね！終わりたいときは空白でEnter！]\n{0:グー,1:チョキ,2:パー}:");

                if(isBot)
                {
                    select = rand.Next(0, 3).ToString();
                    Console.WriteLine(select);
                }
                else
                {
                    select = Console.ReadLine();

                }

                if(select == "0")
                {
                    return "グー";
                }
                else if(select == "1")
                {
                    return "チョキ";
                }
                else if(select == "2")
                {
                    return "パー";
                }
                else
                {
                    return "\0";
                }
            }

        }
        static void InCWords()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt = Console.ReadLine();    // 入力文字で送信
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }
    }
}