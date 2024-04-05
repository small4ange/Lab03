using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        using (TcpClient client = new TcpClient("127.0.0.1", 8000))
        using (NetworkStream stream = client.GetStream())
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (true)
            {
                string userInput = "";
                Console.Write("Enter action (1 - get a file, 2 - create a file, 3 - delete a file): > ");
                string command = Console.ReadLine();
                userInput += command;
                if (command == "exit")
                {
                    writer.WriteLine(userInput);
                    Console.WriteLine("The request was sent.");
                    reader.Close();
                    writer.Close();
                    client.Close();
                    stream.Close();
                    break;
                }
                Console.Write("Enter filename: > ");
                string filename = Console.ReadLine();
                userInput += "|" + filename;

                if (command == "1") //GET
                {
                    //отправляем запрос на сервер
                    writer.WriteLine(userInput);
                    Console.WriteLine("The request was sent.");

                    //Считывание ответа сервера 200 или 404
                    string response = reader.ReadLine();

                    if (response == "200")
                    {
                        Console.WriteLine("The content of the file is: " + reader.ReadLine());
                    }
                    else if (response == "404")
                    {
                        Console.WriteLine("The response says that the file was not found!");
                    }
                }
                else if (command == "2") //PUT
                {
                    //запрашиваем у клиента текст для файла
                    Console.Write("Enter file content: > ");
                    string content = Console.ReadLine();
                    userInput += "|" + content;

                    //отправляем запрос на сервер
                    writer.WriteLine(userInput);
                    Console.WriteLine("The request was sent.");

                    //Считывание ответа сервера 200 или 403
                    string response = reader.ReadLine();
                    Console.WriteLine(response);
                    if (response == "200")
                    {
                        Console.WriteLine("The response says that file was created!");
                    }
                    else if (response == "403")
                    {
                        Console.WriteLine("The response says that creating the file was forbidden!");
                    }

                }
                else if (command == "3")
                {//DELETE
                    //отправляем запрос на сервер
                    writer.WriteLine(userInput);
                    Console.WriteLine("The request was sent.");

                    //Считывание ответа сервера 200 или 404
                    string response = reader.ReadLine();

                    if (response == "200")
                    {
                        Console.WriteLine("The response says that the file was succesfully deleted");
                    }
                    else if (response == "404")
                    {
                        Console.WriteLine("The response says that the file was not found!");
                    }
                } else { }
            }
            
        }
    }
}

