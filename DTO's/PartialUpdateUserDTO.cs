namespace User_Authapi.DTO_s
{
    public record PartialUpdateUserDTO
    {  
        
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
        public string? HashedPassword { get; set; }
    }
}
