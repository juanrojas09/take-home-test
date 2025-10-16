using System.ComponentModel.DataAnnotations.Schema;
using Fundo.Applications.Domain.Common;

namespace Fundo.Applications.Domain.Entities;

[Table("users")]
public class Users : Entity<int>
{
    [Column("email")]
    public string Email { get; private set; } = null!;
    
    [Column("password")]
    public string Password { get; private set; } = null!;
    
    [Column("first_name")]
    public string FirstName { get; private set; } = null!;
    
    [Column("last_name")]
    public string LastName { get; private set; } = null!;
    
    public Roles Role { get; private set; } = null!;
    
    [ForeignKey("RoleId")]
    [Column("role_id")]
    public int RoleId { get; private set; }

    

    protected Users() { }

    
    private Users(string email, string password, string firstName, string lastName, int roleId)
    {
        Email = email;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
        RoleId = roleId;
    }
    
    
    public static Users CreateNew(string email, string password, string firstName, string lastName, int roleId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty.");
        }

        return new Users(
            email,
            password,
            firstName,
            lastName,
            roleId
        );
    }

    
    public void UpdatePersonalInfo(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
    }

    public void ChangePassword(string newPassword)
    {
        Password = newPassword;
    }
    
    
    
    public static string GeneratePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
    
    public static bool ValidateLoginPassword(string inputPassword, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}