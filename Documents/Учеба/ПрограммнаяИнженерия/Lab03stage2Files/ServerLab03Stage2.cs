using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

var tcpListener = new TcpListener(IPAddress.Any, 8888);

try
{
    tcpListener.Start();    // запускаем сервер
    Console.WriteLine("Server started!");

    while (true)
    {
        try
        {
            // получаем подключение в виде TcpClient
            var tcpClient = await tcpListener.AcceptTcpClientAsync();

            // создаем новый поток для обслуживания нового клиента
            new Thread(async () => await ProcessClientAsync(tcpClient)).Start();
        } catch
        {
            break;
        }
        
    }
}
finally
{
    tcpListener.Stop();
}
// обрабатываем клиент
async Task ProcessClientAsync(TcpClient tcpClient)
{
    string filePathServer = @"C:\Users\Katka\source\repos\ServerLab3stage2\ServerLab3stage2\Lab3stage2Files";
    string filePathClient = @"C:\Users\Katka\source\repos\ClientLab3stage2\ClientLab3stage2\client\data";
    string filePathFilesDB = @"C:\Users\Katka\source\repos\ServerLab3stage2\ServerLab3stage2\FilesDB.txt";

    

    var stream = tcpClient.GetStream();
    using (stream)
    using (BinaryReader reader = new BinaryReader(stream))
    using (BinaryWriter writer = new BinaryWriter(stream))
    {
        SortedDictionary<int, string> serverFiles = new SortedDictionary<int, string>();
        //заполнение словаря файлами хранящимися на сервере
        ReadAllServerFiles(filePathFilesDB, serverFiles);
        while (true)
        {
            string command = reader.ReadString(); //получаем команду
            string serverFileName = "";
            string userFileName = "";
            string serverFilePath = "";
            int serverFileId;
            switch (command)
            {
                case "1"://get a file
                    string nameorId = reader.ReadString(); //считываем команду по имени либо айди
                    //считываем серверное имя файла, если передан айди находим серверное имя файла по методу
                    if (nameorId == "1")// By Name
                    {
                        serverFileName = reader.ReadString();
                    }
                    else if (nameorId == "2")// By ID
                    {
                        serverFileId = int.Parse(reader.ReadString());
                        serverFileName = GetFileNameById(serverFileId, serverFiles);
                        
                    }
                    //путь к серверному файлу
                    serverFilePath = Path.Combine(filePathServer, serverFileName);//путь к файлу внутри сервера
                    //отправка серверного файла клиенту
                    if (File.Exists(serverFilePath)) 
                    { 
                        writer.Write(200);
                        writer.Flush();

                        SendFile(serverFilePath, writer);

                        //удаление файла из словаря
                        foreach (var serverFile in serverFiles)
                        {
                            if (serverFile.Value == serverFileName)
                            {
                                serverFiles.Remove(serverFile.Key);
                                break;
                            }
                        }

                    } else { writer.Write(404); writer.Flush(); }
                    UpdateFilesDb(filePathFilesDB, serverFiles);
                    break;
                case "2": //save a file

                    userFileName = reader.ReadString(); 
                    string userFilePath = Path.Combine(filePathClient, userFileName);//откуда брать файл клиента
                    serverFileName = reader.ReadString();
                    serverFilePath = Path.Combine(filePathServer, serverFileName);//куда сохранить файл клиента на сервере

                    if (serverFileName == "" || serverFileName == "\n")
                    {
                        serverFilePath = Path.Combine(filePathServer, userFileName); //если имя файла для хранения на серваке не указано, оставляем имя пользователя
                    }
                    if (!File.Exists(userFilePath))
                    {
                        writer.Write(404);
                        writer.Flush();
                        break;
                    }
                    try { 
                        
                        ReceiveFile(serverFilePath, reader);//получение файла от клиента и сохранение на сервер
                        

                        //достаем айдишник как ключ последнего элемента отсортированного словаря + 1
                        int id = serverFiles.LastOrDefault().Key + 1;
                        Console.WriteLine($"serverFiles Last Key = {serverFiles.LastOrDefault().Key}");
                        if (serverFiles.LastOrDefault().Key == 0) { id = 1; }
                        if (serverFileName != "")
                        {
                            serverFiles.Add(id, serverFileName);
                        } else
                        {
                            serverFiles.Add(id, userFileName);
                        }
                        
                        
                        writer.Write(200);
                        writer.Flush();
                        writer.Write(id);
                        writer.Flush();

                    } catch
                    {
                        writer.Write(404);
                        writer.Flush();
                       
                    }
                    UpdateFilesDb(filePathFilesDB, serverFiles);
                    break;
                case "3": //delete
                    string nameorIdDel = reader.ReadString(); //считываем команду по имени либо айди

                    if (nameorIdDel == "1")// By Name
                    {
                        serverFileName = reader.ReadString();
                    }
                    else if (nameorIdDel == "2")// By ID
                    {
                        serverFileId = int.Parse(reader.ReadString());
                        serverFileName = GetFileNameById(serverFileId, serverFiles);
                        
                    }
                    serverFilePath = Path.Combine(filePathServer, serverFileName); //папка с файлом из сервера

                    if (!File.Exists(serverFilePath))
                    {
                        writer.Write(404);
                        writer.Flush();
                    }
                    else
                    {
                        File.Delete(serverFilePath);
                        //удаление файла из словаря
                        foreach (var serverFile in serverFiles)
                        {
                            if (serverFile.Value == serverFileName)
                            {
                                serverFiles.Remove(serverFile.Key);
                                break;
                            }
                        }

                        writer.Write(200);
                        writer.Flush();
                    }
                    UpdateFilesDb(filePathFilesDB, serverFiles);
                    break;
                case "exit":
                    UpdateFilesDb(filePathFilesDB, serverFiles);
                    tcpClient.Close();
                    stream.Close();
                    reader.Close();
                    writer.Close();
                    tcpListener.Stop();
                    return;

            }
        }
    }

    //метод добычи имени файла по айдишнику из текстового файла сервера
    static string GetFileNameById(int fileId, SortedDictionary<int, string> serverFile)
    {
        foreach (var line in serverFile)
        {
            if (line.Key == fileId)
            {
                return line.Value;
            }
        }
        return "";

    }

    //метод заполнения словаря
    static void ReadAllServerFiles(string filePathFilesDB, SortedDictionary<int, string> serverFiles)
    {
        if (!File.Exists(filePathFilesDB))
        {
            File.Create(filePathFilesDB);
        }
        var lines = File.ReadAllLines(filePathFilesDB);//массив строк 

        foreach (var line in lines)
        {
            var parts = line.Split('|');
            if (parts[0] != null && parts[0] != "" && parts[0] !="\n")
            {
                serverFiles.Add(int.Parse(parts[0]), parts[1]);
                Console.WriteLine($"{line} , {parts[0]}, {parts[1]}");
            }
            
            
        }
    }
}

static void SendFile(string filePath, BinaryWriter writer)
{
    try
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        writer.Write(Path.GetFileName(filePath));
        writer.Write(fileBytes.Length);
        writer.Write(fileBytes);
        File.Delete(filePath);
        //return 200;
    }
    catch (Exception ex)
    {
        //return 404;
    }
}

static int ReceiveFile(string clientFolder, BinaryReader reader)
{
    try
    {
        string fileName = reader.ReadString();
        int fileSize = reader.ReadInt32();
        byte[] fileBytes = reader.ReadBytes(fileSize);
        string fullPath = Path.Combine(clientFolder, fileName);
        File.WriteAllBytes(clientFolder, fileBytes);

        return 200;
    }
    catch (Exception ex)
    {
        return 400;
    }
}
//перезапись в бд
static void UpdateFilesDb(string filePathFilesDB, SortedDictionary <int, string> serverFiles)
{
    // перезаписываем файл с айди-именем тк мы файл удалили 
    using (StreamWriter sw = new StreamWriter(File.Open(filePathFilesDB, FileMode.Create)))
    {
        foreach (KeyValuePair<int, string> kvp in serverFiles)
        {
            // запись пары ключ-значение, разделенной символом '|'
            sw.WriteLine($"{kvp.Key}|{kvp.Value}");
        }
    }
}