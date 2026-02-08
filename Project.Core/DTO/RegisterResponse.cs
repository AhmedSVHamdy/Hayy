namespace Project.Core.DTO
{
    public class RegisterResponse
    {
      
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool RequiresEmailConfirmation { get; set; }
    }
}
