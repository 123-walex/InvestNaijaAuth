using System;
using System.Runtime.CompilerServices;
using AutoMapper;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Entities;

namespace InvestNaijaAuth.Mappings
{
    public class MappingProfile : Profile 
    {
        public MappingProfile()
        {   
            
            CreateMap<SignupDTO, User>();
           
        }
    }
}
