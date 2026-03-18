using AutoMapper;
using VBSPOSS.Data.Models;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Menu, MenuRoleView>();
            CreateMap<MenuRoleView, MenuRolesViewModel>();

            CreateMap<ListOfValue, ListOfValueViewModel>();
            CreateMap<ListOfValueViewModel, ListOfValue>();

            CreateMap<Permission, PermissionModel>();
            CreateMap<UserView, UserModel>();
            CreateMap<UserModel, User>();

            CreateMap<ListOfPos, ListOfPosViewModel>();
            CreateMap<ListOfPosViewModel, ListOfPos>();

            CreateMap<ListOfCommune, ListOfCommuneViewModel>();
            CreateMap<ListOfCommuneViewModel, ListOfCommune>();
            
            CreateMap<UserIDCMaster, UserIDCMasterViewModel>();
            CreateMap<UserIDCMasterViewModel, UserIDCMaster>();

            CreateMap<UserManagementIDC, UserManagementIDCViewModel>();
            CreateMap<UserManagementIDCViewModel, UserManagementIDC>();

            CreateMap<PosRepresentative, PosRepresentativeViewModel>()
               .ForMember(dest => dest.MainPosName, opt => opt.MapFrom(src => src.MainPosName))
               .ForMember(dest => dest.PosName, opt => opt.MapFrom(src => src.PosName))
               .ForMember(dest => dest.StaffPosName, opt => opt.MapFrom(src => src.StaffPosName))
               .ForMember(dest => dest.StaffDepartmentName, opt => opt.MapFrom(src => src.StaffDepartmentName))
               .ForMember(dest => dest.StaffPositionName, opt => opt.MapFrom(src => src.StaffPositionName))
               .ForMember(dest => dest.DateOfBirthText, opt => opt.MapFrom(src => src.DateOfBirth.HasValue ? src.DateOfBirth.Value.ToString("dd/MM/yyyy") : ""))
               .ForMember(dest => dest.EffectDateText, opt => opt.MapFrom(src => src.EffectDate.HasValue ? src.EffectDate.Value.ToString("dd/MM/yyyy") : ""))
               .ForMember(dest => dest.ExpireDateText, opt => opt.MapFrom(src => src.ExpireDate.ToString("dd/MM/yyyy")))
               .ForMember(dest => dest.GendersText, opt => opt.MapFrom(src => src.Genders == "M" ? "Nam" : (src.Genders == "F" ? "Nữ" : "")))
               .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status == 1 ? "Hoạt động" : "Đóng"))
               .ForMember(dest => dest.RepresentativeTypeText, opt => opt.MapFrom(src => src.RepresentativeType == "1" ? "Chính" : "Phụ"))
               .ForMember(dest => dest.OrderNo, opt => opt.Ignore());
            CreateMap<PosRepresentativeViewModel, PosRepresentative>()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id > 0))
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            CreateMap<InterestRateConfigMaster, InterestRateConfigMasterModel>();
            CreateMap<InterestRateConfigMasterModel, InterestRateConfigMaster>();

            CreateMap<NotiTemp, NotiTempViewModel>();
            CreateMap<NotiTempViewModel, NotiTemp>();


            CreateMap<InterestRateConfigMaster, TideRateConfigureViewModel>();
            CreateMap<TideRateConfigureViewModel, InterestRateConfigMaster>();


            CreateMap<InterestRateTermDetail, TideTermViewModel>();
            CreateMap<TideTermViewModel, InterestRateTermDetail>();

            CreateMap<StaffVbspInforViewModel, PosRepresentativeViewModel>();
            CreateMap<PosRepresentativeViewModel, StaffVbspInforViewModel>();

           
            CreateMap<AddCasaProductViewModel, InterestRateConfigMaster>()
               .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
               .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
               .ForMember(dest => dest.Status, opt => opt.Ignore())
               .ForMember(dest => dest.StatusUpdateCore, opt => opt.Ignore())
               .ForMember(dest => dest.PosName, opt => opt.Ignore())
               .ForMember(dest => dest.AccountSubTypeName, opt => opt.Ignore())
               .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiredDate));

            CreateMap<InterestRateConfigMasterView, InterestRateConfigMasterModel>()
     .ForMember(dest => dest.ProductList, opt => opt.MapFrom(src => src.ProductList ?? ""))  // Đã có, nhưng confirm tên src.ProductList đúng
     .ForMember(dest => dest.IsSelected, opt => opt.Ignore());  // Ignore vì default false ở code
            // Reverse map nếu cần (từ Model sang View entity, ví dụ cho Save/Update)
            CreateMap<InterestRateConfigMasterModel, InterestRateConfigMasterView>();


            CreateMap<InterestRateConfigMasterView, InterestRateConfigMasterViewModel>();



            CreateMap<ProductParameter, ProductParameterComparisonViewModel>()
    .ForMember(dest => dest.ProductGroupDisplay, opt => opt.MapFrom(src => src.ProductGroupDisplay))
    .ForMember(dest => dest.CurrentApplyPos, opt => opt.MapFrom(src => src.ApplyPosDisplay))
 
    ;



            CreateMap<AddCasaProductViewModel, InterestRateConfigMasterViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ProductCode))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.AccountTypeCode, opt => opt.MapFrom(src => src.AccountTypeCode))
                .ForMember(dest => dest.AccountTypeName, opt => opt.MapFrom(src => src.AccountTypeName))
                .ForMember(dest => dest.AccountSubTypeCode, opt => opt.MapFrom(src => src.AccountSubTypeCode))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.DebitCreditFlag, opt => opt.MapFrom(src => src.DebitCreditFlag))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiredDate))
                .ForMember(dest => dest.CircularRefNum, opt => opt.MapFrom(src => src.CircularRefNum))
                .ForMember(dest => dest.CircularDate, opt => opt.MapFrom(src => src.CircularDate))
                .ForMember(dest => dest.PosCode, opt => opt.MapFrom(src => src.PosCode))
                .ForMember(dest => dest.InterestRate, opt => opt.MapFrom(src => src.InterestRate))
                .ForMember(dest => dest.NewInterestRate, opt => opt.MapFrom(src => src.NewInterestRate))
                .ForMember(dest => dest.PenalRate, opt => opt.MapFrom(src => src.PenalRate))
                .ForMember(dest => dest.AmountSlab, opt => opt.MapFrom(src => src.AmoutSlab))
                .ForMember(dest => dest.DocumentId, opt => opt.MapFrom(src => src.DocumentId))
                .ForMember(dest => dest.IdList, opt => opt.MapFrom(src => src.IdList))  // Nếu có
                .ForMember(dest => dest.ApplyPosList, opt => opt.MapFrom(src => src.ApplyPosList));  // Nếu có
             

        }            
    }
}
