using AutoMapper;
using ClienteCadastro.Application.DTOs;
using ClienteCadastro.Domain.Entities;

namespace ClienteCadastro.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Cliente -> ClienteDTO
            CreateMap<Cliente, ClienteDTO>();
            
            // ClienteDTO -> Cliente
            CreateMap<ClienteDTO, Cliente>();
            
            // Endereco -> EnderecoDTO
            CreateMap<Endereco, EnderecoDTO>();
            
            // EnderecoDTO -> Endereco
            CreateMap<EnderecoDTO, Endereco>();
        }
    }
}
