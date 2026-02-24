using AutoMapper;
using DotnetProject.Core.Features.UserInfo.DTO;
using DotnetProject.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetProject.Infrastructure.Postgresql.Mappings.Collaboration
{
    public class CollabMappingProfile : Profile
    {
        public CollabMappingProfile()
        {
            CreateMap<AxonsCollabDTO, AxonsCollab>();
            CreateMap<AxonsCollab, AxonsCollabDTO>();
        }
    }
}
