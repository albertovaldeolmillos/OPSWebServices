using AutoMapper;
using OPSWebServicesAPI.Models;
using System.Collections;

namespace OPSWebServicesAPI.Helpers
{
    public class ConfigMapModel
    {
        MapperConfiguration mapperConfiguration { get; set; }

        public MapperConfiguration configOperation()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, Operation>()
                    .ForMember(d => d.bonus, o => o.MapFrom(s => s["bns"]))
                    .ForMember(d => d.contractId, o => o.MapFrom(s => s["contid"]))
                    .ForMember(d => d.contractName, o => o.MapFrom(s => s["contname"]))
                    .ForMember(d => d.parkingEndDate, o => o.MapFrom(s => s["ed"]))
                    .ForMember(d => d.farticle, o => o.MapFrom(s => s["farticle"]))
                    .ForMember(d => d.fcolor, o => o.MapFrom(s => s["fcolor"]))
                    .ForMember(d => d.fmake, o => o.MapFrom(s => s["fmake"]))
                    .ForMember(d => d.fineNumber, o => o.MapFrom(s => s["fn"]))
                    .ForMember(d => d.fineProcessingDate, o => o.MapFrom(s => s["fpd"]))
                    .ForMember(d => d.fineStatus, o => o.MapFrom(s => s["fs"]))
                    .ForMember(d => d.fstreet, o => o.MapFrom(s => s["fstreet"]))
                    .ForMember(d => d.fstrnum, o => o.MapFrom(s => s["fstrnum"]))
                    .ForMember(d => d.operationNumber, o => o.MapFrom(s => s["on"]))
                    .ForMember(d => d.operationType, o => o.MapFrom(s => s["ot"]))
                    .ForMember(d => d.paymentAmount, o => o.MapFrom(s => s["pa"]))
                    .ForMember(d => d.plate, o => o.MapFrom(s => s["pl"]))
                    .ForMember(d => d.paymentMethod, o => o.MapFrom(s => s["pm"]))
                    .ForMember(d => d.postPaid, o => o.MapFrom(s => s["pp"]))
                    .ForMember(d => d.rechargeDate, o => o.MapFrom(s => s["rd"]))
                    .ForMember(d => d.parkingStartDate, o => o.MapFrom(s => s["sd"]))
                    .ForMember(d => d.status, o => o.MapFrom(s => s["sta"]))
                    .ForMember(d => d.zone, o => o.MapFrom(s => s["zo"]))
                    .ForMember(d => d.zonecolor, o => o.MapFrom(s => s["zonecolor"]))
                    .ForMember(d => d.zonename, o => o.MapFrom(s => s["zonename"]));
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configUser()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, User>()
                    .ForMember(d => d.addressBuildingNumber, o => o.MapFrom(s => s["abn"]))
                    .ForMember(d => d.addressCity, o => o.MapFrom(s => s["aci"]))
                    .ForMember(d => d.addressDepartmentDoor, o => o.MapFrom(s => s["add"]))
                    .ForMember(d => d.addressDepartmentFloor, o => o.MapFrom(s => s["adf"]))
                    .ForMember(d => d.addressLetterNumber, o => o.MapFrom(s => s["adl"]))
                    .ForMember(d => d.addressDepartmentStair, o => o.MapFrom(s => s["ads"]))
                    .ForMember(d => d.addressMobilePhone, o => o.MapFrom(s => s["amp"]))
                    .ForMember(d => d.addressPostalCode, o => o.MapFrom(s => s["apc"]))
                    .ForMember(d => d.addressProvince, o => o.MapFrom(s => s["apr"]))
                    .ForMember(d => d.addressStreetName, o => o.MapFrom(s => s["asn"]))
                    .ForMember(d => d.cloudToken, o => o.MapFrom(s => s["cid"]))
                    .ForMember(d => d.contractId, o => o.MapFrom(s => s["contid"]))
                    .ForMember(d => d.email, o => o.MapFrom(s => s["em"]))
                    .ForMember(d => d.firstSurname, o => o.MapFrom(s => s["fs"]))
                    .ForMember(d => d.mainMobilePhone, o => o.MapFrom(s => s["mmp"]))
                    .ForMember(d => d.authorizationToken, o => o.MapFrom(s => s["mui"]))
                    .ForMember(d => d.names, o => o.MapFrom(s => s["na"]))
                    .ForMember(d => d.nif, o => o.MapFrom(s => s["nif"]))
                    .ForMember(d => d.operatingSystem, o => o.MapFrom(s => s["os"]))
                    .ForMember(d => d.secondSurname, o => o.MapFrom(s => s["ss"]))
                    .ForMember(d => d.userName, o => o.MapFrom(s => s["un"]))
                    .ForMember(d => d.version, o => o.MapFrom(s => s["v"]))
                    .ForMember(d => d.validateConditions, o => o.MapFrom(s => s["val"]))
                    //.ForMember(d => d.notifications, o => o.MapFrom(s => s["notifications"]))
                    //.ForMember(d => d.plates, o => o.MapFrom(s => s["plates"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configNotifications()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, Notification>()
                    .ForMember(d => d.balance, o => o.MapFrom(s => s["ba"]))
                    .ForMember(d => d.fineNotifications, o => o.MapFrom(s => s["fn"]))
                    .ForMember(d => d.quantityBalance, o => o.MapFrom(s => s["q_ba"]))
                    .ForMember(d => d.rechargeNotifications, o => o.MapFrom(s => s["re"]))
                    .ForMember(d => d.minutesBeforeUnparking, o => o.MapFrom(s => s["t_unp"]))
                    .ForMember(d => d.unparkingNotifications, o => o.MapFrom(s => s["unp"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configPlate()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, Plate>()
                    .ForMember(d => d.plate, o => o.MapFrom(s => s["p"]))
                    .ForMember(d => d.status, o => o.MapFrom(s => s["stp"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }
    }
}