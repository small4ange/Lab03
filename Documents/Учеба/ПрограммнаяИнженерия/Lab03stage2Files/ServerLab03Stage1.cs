using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ServerLab03
{
    static void Main(string[] args)
    {
        TcpListener server = null;

        server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000); //создаем листенер
        server.Start();//запускаем сервер
        Console.WriteLine(" Server started! ");

        string folderPath = @"C:\Users\Katka\source\repos\ServerLab03\ServerLab03\Lab03Files";
        
        using (TcpClient client = server.AcceptTcpClient())
        using (NetworkStream stream = client.GetStream())
        {
            //потоки для получения и отправки информации
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            //Console.WriteLine("Client connected. Waiting for command...");

            while (true)
            {
                
                string[] userInput = reader.ReadLine().Split('|');
                Console.WriteLine(userInput[0]);

                string command = userInput[0];//считываем команду от клиента
                //Проверка на окончание работы сервера
                if (command == "exit")
                {
                    writer.Close();
                    reader.Close();
                    client.Close();
                    server.Stop();
                    break;
                }
                string filename = userInput[1];// имя файла клиента
                string fullPath = Path.Combine(folderPath, filename);
                Console.WriteLine($"{filename}");

                //switch case для ответа на команду клиента
                switch (command)
                {
                    case "1": //GET
                        if (!File.Exists(fullPath)) { writer.WriteLine("404"); }
                        else
                        {
                            writer.WriteLine("200");
                            writer.WriteLine(File.ReadAllText(fullPath));

                        }
                        break;

                    case "2": //PUT
                        string content = userInput[2];

                        Console.WriteLine(fullPath);

                        try
                        {
                            if (File.Exists(fullPath)) { writer.WriteLine("403"); }
                            else
                            {
                                File.WriteAllText(fullPath, content);
                                writer.WriteLine("200");
                            }

                        }
                        catch
                        {
                            writer.WriteLine("403");
                        }
                        break;

                    case "3": //DELETE
                        if (!File.Exists(fullPath))
                        {
                            writer.WriteLine("404");
                        }
                        else
                        {
                            try
                            {
                                File.Delete(fullPath);
                                writer.WriteLine("200");
                            }
                            catch
                            {
                                writer.WriteLine("404");
                            }
                        }
                        break;

                }

            }

        }
    }
}
