namespace InvestNaijaAuth.DTO_s
{
    public class CreatePortfolioDTO
    {
        public Guid UserId { get; set; }
        public string Symbol { get; set; } = string.Empty;
    }
}
