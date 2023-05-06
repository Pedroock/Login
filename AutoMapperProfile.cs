using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Login.Models;
using Login.Dtos.User;

namespace Login
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<x, y>();
            CreateMap<User, GetUserDto>();
        }
    }
}