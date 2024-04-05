using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;


// создание клиента
using TcpClient tcpClient = new TcpClient(); 
await tcpClient.ConnectAsync("127.0.0.1", 8888);
var stream = tcpClient.GetStream();


string userFilePath = @"C:\Users\Katka\source\repos\ClientLab3stage2\ClientLab3stage2\client\data";

using (stream)
using (BinaryReader reader = new BinaryReader(stream))
using (BinaryWriter writer = new BinaryWriter(stream))
{
    while (true)
    {
        
        Console.Write("Enter action (1 - get a file, 2 - save a file, 3 - delete a file): >");
        string command = Console.ReadLine();
        
        writer.Write(command);
        writer.Flush();
        switch (command)
        {
            case "1"://get a file
                Console.Write("Do you want to get the file by name or by id (1 - name, 2 - id):");
                string NameorId = Console.ReadLine(); //по имени или по айди
                writer.Write(NameorId);
                writer.Flush();

                switch (NameorId)
                {
                    case "1": //byName
                        Console.Write("Enter name: > ");
                        writer.Write(Console.ReadLine());//serverFileName
                        writer.Flush();
                        Console.WriteLine("The request was sent.");

                        
                        int responseGet1 = reader.ReadInt32();
                        if (responseGet1 == 200)
                        {
                            Console.Write("The file was downloaded! Specify a name for it: > ");
                            //запись нового имени для файла
                            string userFileNameToSave = Console.ReadLine();
                            //получение серверного файла и запись его в папку клиента
                            ReceiveFile(Path.Combine(userFilePath, userFileNameToSave), reader);
                            Console.WriteLine("File saved on the hard drive!");
                        }
                        else if (responseGet1 == 404)
                        {
                            Console.WriteLine("The response says that this file is not found!");
                        }

                        break;
                    case "2": //byID
                        Console.Write("Enter ID: > ");
                        string serverFileID = Console.ReadLine();
                        writer.Write(serverFileID);
                        writer.Flush();
                        Console.WriteLine("The request was sent.");

                        int responseGet2 = reader.ReadInt32();
                        if (responseGet2 == 200)
                        {
                            Console.Write("The file was downloaded! Specify a name for it: > ");
                            string userFileNameToSave = Console.ReadLine();
                            ReceiveFile(Path.Combine(userFilePath, userFileNameToSave), reader);
                            
                            Console.WriteLine("File saved on the hard drive!");
                        }
                        else if (responseGet2 == 404)
                        {
                            Console.WriteLine("The response says that this file is not found!");
                        }
                        break;
                }
                break;
            case "2": //save a file
                Console.Write("Enter name of the file: > ");
                string userFileName = Console.ReadLine();
                writer.Write(userFileName);
                writer.Flush();
                Console.Write("Enter name of the file to be saved on server: > ");
                string serverFileName = Console.ReadLine();
                writer.Write(serverFileName);
                writer.Flush();
                Console.WriteLine("The request was sent.");

                //отправка файла от клиента серверу
                string fullPathToSaveOnServer = Path.Combine(userFilePath, userFileName);
                SendFile(fullPathToSaveOnServer, writer);
                
                int responseSave = reader.ReadInt32();
                if (responseSave == 200)
                {
                    int IdOfFile = reader.ReadInt32();
                    Console.WriteLine($"Response says that file is saved! ID = {IdOfFile}");
                }
                else if (responseSave == 404)
                {
                    Console.WriteLine("The response says that the file can't be created");
                }
                break;
            case "3": //delete
                Console.Write("Do you want to delete the file by name or by id (1 - name, 2 - id):");
                string NameorIdDel = Console.ReadLine();
                writer.Write(NameorIdDel);
                writer.Flush();

                switch (NameorIdDel)
                {
                    case "1": //byName
                        Console.Write("Enter name: > ");
                        writer.Write(Console.ReadLine());//serverFileName
                        writer.Flush();
                        Console.WriteLine("The request was sent.");

                    break;
                    case "2": //byID
                        Console.Write("Enter ID: > ");
                        string serverFileID = Console.ReadLine();
                        writer.Write(serverFileID);
                        writer.Flush();
                        Console.WriteLine("The request was sent.");
                    break;
                }
                int responseDel = reader.ReadInt32();
                if (responseDel == 200)
                {
                    Console.WriteLine("The response says that this file was deleted successfully!");
                } else
                {
                    Console.WriteLine("The response says that this file is not found!");
                }
                break;
            case "exit":
                tcpClient.Close();
                stream.Close();
                reader.Close();
                writer.Close();
                break;
        }
        if (tcpClient.Connected == false) { break; }


    }

}
static int SendFile(string filePath, BinaryWriter writer)
{
    try
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        writer.Write(Path.GetFileName(filePath));
        writer.Write(fileBytes.Length);
        writer.Write(fileBytes);
        File.Delete(filePath);
        return 200;
    }
    catch (Exception ex)
    {
        return 404;
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
    catch
    {
        return 404;
    }

}
