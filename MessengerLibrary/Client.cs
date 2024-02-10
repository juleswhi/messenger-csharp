using System.Net.Sockets;
using static Messenger.PacketRequestType;
using static Messenger.ConnectionResult;

namespace Messenger; 

public enum ConnectionResult {
    SUCCESS,
    FAILURE
}

public class Client {

    public string Nickname { get; set; }
    public Guid Id { get; set; } 

    private static readonly string _defaultIp = @"127.0.0.1"; 

    private TcpClient _client = new();

    public Client() {
        Nickname = "default";
        Id = Guid.NewGuid();
    }

    public Client(string nickname) : this() {
        Nickname = nickname;
    }

    public ConnectionResult Connect(string? ip = null, int port = 6969) {

        ip ??= _defaultIp;

        // This should really be disposed of fr
        _client = new TcpClient(ip, port);

        // Grab the stream from the client rq
        NetworkStream stream = _client.GetStream();

        // Create the initial connection packet
        // Get the serialized bytes 
        byte[] packet = new Packet(Id, Nickname){ RequestType = HANDSHAKE }.ToBytes();

        // Send the packet to the TCP server
        stream.Write(packet, 0, packet.Length);

        // Console.WriteLine($"Sent bytes of length: {packet.Length} to server at: {ip}, port: {port}");


        // Create buffer for response
        packet = new byte[256];

        // Int represents the number of bytes in buffer
        Int32 bytes = stream.Read(packet, 0, packet.Length);

        // turn the bytes into a useable Packet
        Packet response = Packet.FromString(System.Text.Encoding.ASCII.GetString(packet, 0, bytes));

        // Something weird happenbed here
        if(response.RequestType != CONFIRMATION) {
            throw new Exception("Could not connect to server."); 
        }

        // Confirm to the user they are connected
        Console.WriteLine($"Connected to server as: {Nickname}");

        new Thread(() => HandleMessages()).Start();

        return SUCCESS;
    }

    private void HandleMessages() {

        if(_client is null) {
            return;
        }

        while(true) {
            NetworkStream stream = _client.GetStream();

            byte[] bytes = new byte[256];

            int i;

            while((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
                Packet packet = Packet.FromString(System.Text.Encoding.ASCII.GetString(bytes, 0, i));
                Console.CursorLeft = 0;
                Console.WriteLine($"{packet.Data}");
                Console.Write("> ");
            }
        }
    }

    public void Send(string message) {
        NetworkStream stream = _client.GetStream();

        byte[] packet = new Packet(Id, message){ RequestType = MESSAGE }.ToBytes();

        stream.Write(packet, 0, packet.Length);
    }
}
