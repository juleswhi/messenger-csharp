using System.Text;
namespace Messenger; 

public enum PacketRequestType {
    HANDSHAKE,
    MESSAGE,
    CONFIRMATION
}

public class Packet {

    public Packet() {
        Id = Guid.NewGuid();
        Data = "";
    }

    public Packet(Guid sender, string data = "") : this() {
        ( Sender, Data ) = ( sender, data );
    }

    // If an initial connection
    public PacketRequestType RequestType { get; set; }

    // ID of the packet for Identification
    public Guid Id { get;set; }

    // Sender of the packet
    public Guid Sender { get; set; } 

    // The data to be passed through ( message ) 
    public string Data { get; set; }

    // Receiver of the packet
    public string? Recipient { get; set; } = null;

    public override string ToString() {
        // Create the builder
        StringBuilder stringBuilder = new StringBuilder();

        // Get the properties
        var props = typeof(Packet).GetProperties();

        // Loop through properties and add their kvp to string
        foreach(var prop in props) {
            stringBuilder.Append($"{prop.Name}{{{prop.GetValue(this)}}}");
        }

        // Return the final string
        return stringBuilder.ToString();
    }

    public byte[] ToBytes() {
        return System.Text.Encoding.ASCII.GetBytes(ToString());
    }

    public static Packet FromString(string @string) {
        return Serializer.Deserialize(@string);
    }
}
