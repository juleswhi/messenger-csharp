using System.Net;
using System.Net.Sockets;
using static Messenger.PacketRequestType;

namespace Messenger;

public class Server {

    private static TcpListener? listener = null;
    private static readonly string _defaultIp = @"127.0.0.1"; 

    private Dictionary<Guid, string> GuidToNicknameMap = new ();
    private Dictionary<Guid, NetworkStream> GuidToStreamMap = new ();

    public void Run(string? ip = null, int port = 6969) {
        ip ??= _defaultIp;

        IPAddress address = IPAddress.Parse(ip);

        listener = new TcpListener(address, port);

        // Start the server
        listener.Start();

        Console.WriteLine(@"Server Started.");
        Console.WriteLine(@"Waiting for connection...");

        // In order to read data

        // Listening loop  
        while(true) {
            // Grab all the clients
            TcpClient client = listener.AcceptTcpClient();

            new Thread(() => HandleClient(client)).Start();
        }
    }

    private void HandleClient(TcpClient client) {

        byte[] bytes = new byte[256];

        string? data = null;

        NetworkStream stream = client.GetStream();

        int i;

        while((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

            // Parse data to packet
            if(data is null) {
                throw new NullReferenceException("Expected data packet was null");
            }

            Packet packet = Packet.FromString(data!);

            GuidToStreamMap[packet.Sender] = stream;

            if(packet.RequestType == PacketRequestType.HANDSHAKE) {
                Console.WriteLine($"{packet.Data} joined.");
                // Add kvp to dict
                GuidToNicknameMap[packet.Sender] = packet.Data;
                // Send a packet back ( Confirmation )
                byte[] confirmationPacket = new Packet() { RequestType = CONFIRMATION }.ToBytes();

                // Send the confirmation packet
                stream.Write(confirmationPacket, 0, confirmationPacket.Length);
            }

            // Return a list of all nicknames
            else if(packet.RequestType == PacketRequestType.USERS) {
                // nicknames separated by ','
                string users = "";
                foreach(var (_, val) in GuidToNicknameMap) {
                    users += $"{val},";
                }

                byte[] nicknamePacket = new Packet(packet.Sender, users){ RequestType = PacketRequestType.USERS }.ToBytes();

                stream.Write(nicknamePacket, 0, nicknamePacket.Length);
            }

            else if(packet.RequestType == PacketRequestType.MESSAGE) {

                // Send packet to all others clients

                if(!string.IsNullOrEmpty(packet.Recipient)) {
                    // Grab the network stream of the recipient
                    NetworkStream recipientStream = GuidToStreamMap[
                        // Search for the nickname 
                        // Grab the correspinding guid
                        GuidToNicknameMap.FirstOrDefault(x => x.Value == packet.Recipient).Key
                    ];

                    byte[] recipientPacket = packet.ToBytes();
                    recipientStream.Write(recipientPacket, 0, recipientPacket.Length);
                    Console.WriteLine($"{GuidToNicknameMap[packet.Sender]} TO {packet.Recipient}: {packet.Data}");
                }

                else {
                    byte[] outgoingPacket = new Packet(packet.Sender, $"{GuidToNicknameMap[packet.Sender]}: {packet.Data}"){ RequestType = MESSAGE }.ToBytes();
                    foreach(var (_, clientStream)  in GuidToStreamMap) {
                        clientStream.Write(outgoingPacket, 0, outgoingPacket.Length);
                    }
                    Console.WriteLine($"{GuidToNicknameMap[packet.Sender]}: {packet.Data}");
                }


            }
        }
    }
}
