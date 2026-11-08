using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Parking.Application.Common.Exceptions;
using Parking.Infrastructure.Persistence.Configurations;

namespace Parking.Infrastructure.Persistence;

/// <summary>Turns unique-index violations into meaningful conflicts (the database is the final arbiter under concurrency).</summary>
internal static partial class DbExceptionTranslator
{
    private const int DuplicateEntry = 1062;

    private static readonly Dictionary<string, (string Code, string Message)> UniqueIndexMessages = new(StringComparer.OrdinalIgnoreCase)
    {
        [IndexNames.TicketActiveSpace] = ("ticket.space_not_free", "O espaço já está ocupado."),
        [IndexNames.TicketActiveCustomer] = ("ticket.customer_has_open_ticket", "Este cliente já tem um ticket aberto."),
        [IndexNames.TicketActiveVehicle] = ("ticket.vehicle_has_open_ticket", "Esta viatura já tem um ticket aberto."),
        [IndexNames.UserUsername] = ("user.username_taken", "Já existe um utilizador com este nome."),
        [IndexNames.SpaceCode] = ("space.code_taken", "Já existe um espaço com este código."),
        [IndexNames.BrandName] = ("catalog.brand_exists", "Já existe uma marca com este nome."),
        [IndexNames.ModelBrandName] = ("catalog.model_exists", "Esta marca já tem um modelo com este nome."),
    };

    public static bool TryTranslate(DbUpdateException exception, [NotNullWhen(true)] out ConflictException? conflict)
    {
        conflict = null;
        if (exception.InnerException is not MySqlException { Number: DuplicateEntry } mySqlException)
        {
            return false;
        }

        var match = DuplicateKeyPattern().Match(mySqlException.Message);
        var index = match.Success ? match.Groups["index"].Value : string.Empty;
        conflict = UniqueIndexMessages.TryGetValue(index, out var mapped)
            ? new ConflictException(mapped.Code, mapped.Message)
            : new ConflictException("conflict.duplicate", "O registo foi alterado em simultâneo. Tente novamente.");
        return true;
    }

    [GeneratedRegex(@"for key '(?:[^'.]+\.)?(?<index>[^']+)'", RegexOptions.CultureInvariant)]
    private static partial Regex DuplicateKeyPattern();
}
