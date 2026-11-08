using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Users;

namespace Parking.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).HasMaxLength(User.UsernameMaxLength).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(User.FullNameMaxLength).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(User.PasswordHashMaxLength).IsRequired();
        builder.Ignore(u => u.Permissions);
        builder.Ignore(u => u.HasLegacyPassword);
        builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName(IndexNames.UserUsername);
        builder.HasIndex(u => new { u.Role, u.IsActive });
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TokenHash).HasMaxLength(RefreshToken.TokenHashLength).IsFixedLength().IsRequired();
        builder.Property(r => r.ReplacedByTokenHash).HasMaxLength(RefreshToken.TokenHashLength).IsFixedLength();
        builder.Property(r => r.CreatedByIp).HasMaxLength(RefreshToken.IpMaxLength);
        builder.Property(r => r.RevokedReason).HasMaxLength(RefreshToken.ReasonMaxLength);
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.TokenHash).IsUnique().HasDatabaseName("ux_refresh_tokens_token_hash");
        builder.HasIndex(r => new { r.UserId, r.RevokedAt, r.ExpiresAt }).HasDatabaseName("ix_refresh_tokens_user_active");
        builder.HasIndex(r => r.FamilyId).HasDatabaseName("ix_refresh_tokens_family");
    }
}
