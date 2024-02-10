using System.Text;

namespace Messenger;

public static class Serializer {

    public static string Serialize(Packet @packet) {
        return @packet.ToString();
    }

    public static Packet Deserialize(string @string) {

        // Convert @string to list of tokens
        List<Token> tokens = Scanner(@string);

        // Convert list of tokens to useable c# object
        Packet packet = Parser(tokens);

        // Return the packet object 
        return packet;
    }

    private enum TokenType {
        KEY,
        VALUE,
        LEFT_BRACKET,
        RIGHT_BRACKET
    }

    // Simple record type for represent a Token ( what the scanner creates )
    private record Token(TokenType Type, string? Data);

    // Responsible for turning a string into a list of tokens, which then can be parsed
    // These tokens can contain data for use in an identifier
    // This data may be null, and the token will just be used to identify a marker or symbol
    private static List<Token> Scanner(string @string) {

        List<Token> tokens = new();

        int _current = 0; 

        var next = (int n = 1) => _current += n;
        var current = () => @string[_current];

        var consume = (Token token) => {
            next();
            return token;
        };

        for(; _current < @string.Length;) {

            Func<Token> token = current() switch {

                '{' => () => consume(new Token(TokenType.LEFT_BRACKET, null)),

                '}' => () => consume(new Token(TokenType.RIGHT_BRACKET, null)),

                _ => () => {

                    StringBuilder stringBuilder = new();

                    while(!"{}".Contains(current())) {
                        stringBuilder.Append(current());
                        next();
                    }

                    if(!tokens.Any()) {
                        return new Token(TokenType.KEY, stringBuilder.ToString());
                    }

                    TokenType type = tokens.Last().Type switch {
                        TokenType.LEFT_BRACKET => TokenType.VALUE,
                        _ => TokenType.KEY
                    };

                    return new Token(type, stringBuilder.ToString());
                }
            };

            tokens.Add(token());
        }

        return tokens;
    }

    private static Packet Parser(List<Token> tokens) {
        // Create a Packet Object
        Packet packet = new();

        // Get properties of Packet
        var properties = typeof(Packet).GetProperties(); 

        int _current = 0;
        var current = () => tokens[_current];
        var next = (int n = 1) => _current += n;

        for(; _current < tokens.Count;) {

            if(current().Type == TokenType.KEY) {
                // Store property info of corresponding key
                var property = properties.FirstOrDefault(x => x.Name == current().Data);
                
                if(property is null) throw new MissingFieldException("Could not find the correct property, due to an error with the packet");

                // This will skip from KEY -> LEFT_BRACKET -> VALUE
                next(2);

                if(current().Type == TokenType.RIGHT_BRACKET) {
                    break;
                }

                // Must be a VALUE and therefor has to contain a non-null string
                if(current().Data is null) throw new NullReferenceException($"The token of type: {current().Type} did not contain a valid VALUE");
                
                string value = current().Data!;

                // Must cast to Guid type instead of string 
                if(property.PropertyType == typeof(Guid)) {

                    if(!Guid.TryParse(value, out Guid @out)) {
                        // Must not be a valid Guid, throw
                        throw new InvalidCastException("Could not parse string to Guid");
                    }

                    // Use the property setter ( type: Guid )
                    property.SetValue(packet, @out);
                }

                // Must cast to PacketRequestType type instead of string
                else if(property.PropertyType == typeof(PacketRequestType)) {
                    if(!Enum.TryParse(typeof(PacketRequestType), value, true, out var @out)) {
                        // Must not be a valid bool, panic
                        throw new InvalidCastException("Could not parse string to bool");
                    }

                    // Use the property setter ( type: bool )
                    property.SetValue(packet, @out);
                }

                // Use the property setter ( type: string )
                else {
                    property.SetValue(packet, value);
                }

            }

            // Make sure to increment before the next iteration
            next();
        }

        return packet;
    }

}


