using Messenger;

Console.WriteLine($"Please enter your nickname ( blank for guest )");
Console.Write("> ");

string? nickname = Console.ReadLine();

nickname ??= $"guest-{new Random().Next(1, 99)}";
nickname = nickname == "" ? $"guest-{new Random().Next(1, 99)}" : nickname;

Client client = new Client(nickname);

client.Connect();


while(true) {

    Console.WriteLine($"What would you like to send?   ( \"/help\" for help)");

    Console.Write("> ");

    string? input = Console.ReadLine();

    if(input is null) continue;

    else if(input.Trim() == "/help") {

        Console.WriteLine();

        Console.WriteLine("/[recipient name] [message] to send a message to a specific user");
        Console.WriteLine("/quit to quit the program");
    }
    else if(input.Trim() == "/quit") {
        break;
    }
    else if(input.Trim().Contains("/")) {
        string recipient = "";
        int i = 0;
        for(; i < input.Length; i++) {
            if(input[i] == '/') continue;
            if(input[i] == ' ') break;
            recipient += input[i];
        }

        string message = input[i++..];

        Console.WriteLine($"The recipeint is: {recipient}, and the message is: {message}");

        client.Send(message, recipient);

    }



    else client.Send(input.Trim());
}

Console.WriteLine("Client disconnected.");
