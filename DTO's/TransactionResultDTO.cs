using System.Security.Principal;

namespace InvestNaijaAuth.DTO_s
{
    public class TransactionResultDTO
    {
        public Guid TransactionId { get; set; }

        public decimal Balance { get; set; }
    }
}
