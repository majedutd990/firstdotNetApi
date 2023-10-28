namespace DotnetAPI.DTOs
{
    partial class UserForLogInConfirmationDto
    {
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public UserForLogInConfirmationDto()
        {
            PasswordHash ??= Array.Empty<byte>();
            PasswordSalt ??= Array.Empty<byte>();
        }
    }
}