using Messenger;

Console.WriteLine($"Please enter your nickname ( blank for guest )");
Console.Write("> ");

string? nickname = Console.ReadLine();

nickname ??= $"guest-{new Random().Next(1, 99)}";
nickname = nickname == "" ? $"guest-{new Random().Next(1, 99)}" : nickname;

Client client = new Client(nickname);

client.Connect();


while(true) {

    Console.WriteLine($"What would you like to send?   ( \"/quit\" to quit )");

    Console.Write("> ");

    string? input = Console.ReadLine();

    if(input is null) continue;

    else if(input.Trim() == "/quit") {
        break;
    }

    client.Send(input.Trim());
}

Console.WriteLine("Client disconnected.");
