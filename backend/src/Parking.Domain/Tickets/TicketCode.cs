using System.Security.Cryptography;

namespace Parking.Domain.Tickets;

public static class TicketCode
{
    public const int Length = 10;

    // Crockford Base32 alphabet: no I, L, O, U to avoid transcription mistakes.
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    public static string New() => RandomNumberGenerator.GetString(Alphabet, Length);
}
