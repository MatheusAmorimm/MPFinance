using System;

namespace MPFinance.Domain.Entities
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        // Propriedade de navegação do Entity Framework
        public User? User { get; set; }
    }
}