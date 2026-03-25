using AutoMapper;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Domain.Entities;

namespace FamilyLedger.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Transaction, TransactionResponse>()
            .ForCtorParam(nameof(TransactionResponse.LoggedBy), o => o.MapFrom(_ => new MemberSummary(Guid.Empty, "Unknown")));
    }
}
