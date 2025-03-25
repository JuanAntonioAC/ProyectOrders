using AutoMapper;
using OrderServices.Data.DTOS;
using OrderServices.Data.Models;

namespace OrderServices.Mappers
{
    public class OrderMapper :  Profile
    {
        public OrderMapper()
        {
            CreateMap<Order, UpdateOrderDto>().ReverseMap();
            CreateMap<Order, CreateOrderDto>().ReverseMap();
        }
    }
}
