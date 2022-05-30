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
                    .ForMember(d => d.alternativeMobilePhone, o => o.MapFrom(s => s["amp"]))
                    .ForMember(d => d.addressPostalCode, o => o.MapFrom(s => s["apc"]))
                    .ForMember(d => d.addressProvince, o => o.MapFrom(s => s["apr"]))
                    .ForMember(d => d.addressStreetName, o => o.MapFrom(s => s["asn"]))
                    .ForMember(d => d.cloudToken, o => o.MapFrom(s => s["cid"]))
                    .ForMember(d => d.contractId, o => o.MapFrom(s => s["contid"]))
                    .ForMember(d => d.email, o => o.MapFrom(s => s["em"]))
                    .ForMember(d => d.firstSurname, o => o.MapFrom(s => s["fs"]))
                    .ForMember(d => d.mainMobilePhone, o => o.MapFrom(s => s["mmp"]))
                    //.ForMember(d => d.authorizationToken, o => o.MapFrom(s => s["mui"]))
                    .ForMember(d => d.names, o => o.MapFrom(s => s["na"]))
                    .ForMember(d => d.nif, o => o.MapFrom(s => s["nif"]))
                    .ForMember(d => d.operatingSystem, o => o.MapFrom(s => s["os"]))
                    .ForMember(d => d.secondSurname, o => o.MapFrom(s => s["ss"]))
                    .ForMember(d => d.userName, o => o.MapFrom(s => s["un"]))
                    .ForMember(d => d.password, o => o.MapFrom(s => s["pw"]))
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

        public MapperConfiguration configContract()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, Contract>()
                    .ForMember(d => d.address, o => o.MapFrom(s => s["addr"]))
                    .ForMember(d => d.contractId, o => o.MapFrom(s => s["cont_id"]))
                    .ForMember(d => d.description1, o => o.MapFrom(s => s["desc1"]))
                    .ForMember(d => d.description2, o => o.MapFrom(s => s["desc2"]))
                    .ForMember(d => d.email, o => o.MapFrom(s => s["email"]))
                    .ForMember(d => d.imagePath, o => o.MapFrom(s => s["image"]))
                    .ForMember(d => d.latitude, o => o.MapFrom(s => s["lt"]))
                    .ForMember(d => d.longitude, o => o.MapFrom(s => s["lg"]))
                    .ForMember(d => d.phone, o => o.MapFrom(s => s["phone"]))
                    .ForMember(d => d.radius, o => o.MapFrom(s => s["rad"]))
                    .ForMember(d => d.wsoper, o => o.MapFrom(s => s["wsoper"]))
                    .ForMember(d => d.wsuser, o => o.MapFrom(s => s["wsuser"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configZone()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ZoneInfo>()
                    .ForMember(d => d.latitude, o => o.MapFrom(s => s["lt"]))
                    .ForMember(d => d.longitude, o => o.MapFrom(s => s["lg"]))
                    .ForMember(d => d.streetname, o => o.MapFrom(s => s["streetname"]))
                    .ForMember(d => d.streetno, o => o.MapFrom(s => s["streetno"]))
                    .ForMember(d => d.zone, o => o.MapFrom(s => s["zone"]))
                    .ForMember(d => d.zonecolor, o => o.MapFrom(s => s["zonecolor"]))
                    .ForMember(d => d.zonename, o => o.MapFrom(s => s["zonename"]))
                    .ForMember(d => d.sector, o => o.MapFrom(s => s["sector"]))
                    .ForMember(d => d.sectorcolor, o => o.MapFrom(s => s["sectorcolor"]))
                    .ForMember(d => d.sectorname, o => o.MapFrom(s => s["sectorname"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configStreets()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, StreetsInfo>()
                    .ForMember(d => d.datetime, o => o.MapFrom(s => s["t"]))
                    .ForMember(d => d.streetsNumber, o => o.MapFrom(s => s["st_no"]))
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.streetlist, o => o.MapFrom(s => s["streetlist"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configSectors()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, SectorsInfo>()
                    .ForMember(d => d.sectorsNumber, o => o.MapFrom(s => s["sectorsNumber"]))
                    .ForMember(d => d.sectorlist, o => o.MapFrom(s => s["sectorlist"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configStreetsFull()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, StreetsFullInfo>()
                    .ForMember(d => d.streetsFullNumber, o => o.MapFrom(s => s["streetsFullNumber"]))
                    .ForMember(d => d.streetsFulllist, o => o.MapFrom(s => s["streetsFulllist"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configPlace()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, PlaceInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.response, o => o.MapFrom(s => s["response"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configStep()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, Step>()
                    .ForMember(d => d.time, o => o.MapFrom(s => s["t"]))
                    .ForMember(d => d.quantity, o => o.MapFrom(s => s["q"]))
                    .ForMember(d => d.datetime, o => o.MapFrom(s => s["d"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configParkingSteps()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ParkingStepsInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["ad"]))
                    .ForMember(d => d.operationType, o => o.MapFrom(s => s["o"]))
                    .ForMember(d => d.payAmountMin, o => o.MapFrom(s => s["q1"]))
                    .ForMember(d => d.payAmountMax, o => o.MapFrom(s => s["q2"]))
                    .ForMember(d => d.timeAmountMin, o => o.MapFrom(s => s["t1"]))
                    .ForMember(d => d.timeAmountMax, o => o.MapFrom(s => s["t2"]))
                    .ForMember(d => d.dateMin, o => o.MapFrom(s => s["d1"]))
                    .ForMember(d => d.dateMax, o => o.MapFrom(s => s["d2"]))
                    .ForMember(d => d.dateInitial, o => o.MapFrom(s => s["di"]))
                    .ForMember(d => d.accumulatedQuantity, o => o.MapFrom(s => s["aq"]))
                    .ForMember(d => d.accumulatedTime, o => o.MapFrom(s => s["at"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configParkingTime()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ParkingTimeInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["ad"]))
                    .ForMember(d => d.operationType, o => o.MapFrom(s => s["o"]))
                    .ForMember(d => d.payAmount, o => o.MapFrom(s => s["q"]))
                    .ForMember(d => d.date, o => o.MapFrom(s => s["d"]))
                    .ForMember(d => d.dateInitial, o => o.MapFrom(s => s["di"]))
                    .ForMember(d => d.accumulatedQuantity, o => o.MapFrom(s => s["aq"]))
                    .ForMember(d => d.accumulatedTime, o => o.MapFrom(s => s["at"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configParkingMoney()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ParkingMoneyInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["ad"]))
                    .ForMember(d => d.operationType, o => o.MapFrom(s => s["o"]))
                    .ForMember(d => d.time, o => o.MapFrom(s => s["t"]))
                    .ForMember(d => d.date, o => o.MapFrom(s => s["d"]))
                    .ForMember(d => d.dateInitial, o => o.MapFrom(s => s["di"]))
                    .ForMember(d => d.accumulatedQuantity, o => o.MapFrom(s => s["aq"]))
                    .ForMember(d => d.accumulatedTime, o => o.MapFrom(s => s["at"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }
        public MapperConfiguration configUnParkingQuery()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, UnParkingQueryInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["ad"]))
                    .ForMember(d => d.tariffTime, o => o.MapFrom(s => s["t"]))
                    .ForMember(d => d.payAmount, o => o.MapFrom(s => s["q"]))
                    .ForMember(d => d.dateInitial, o => o.MapFrom(s => s["d1"]))
                    .ForMember(d => d.dateEnd, o => o.MapFrom(s => s["d2"]))
                    .ForMember(d => d.moneyReturned, o => o.MapFrom(s => s["moneyReturned"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configParkingStatusRotation()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ParkingStatusRotationInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.status, o => o.MapFrom(s => s["sta"]))
                    .ForMember(d => d.extension, o => o.MapFrom(s => s["ex"]))
                    .ForMember(d => d.tariffId, o => o.MapFrom(s => s["id"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["ad"]))
                    .ForMember(d => d.operationType, o => o.MapFrom(s => s["o"]))
                    .ForMember(d => d.dateInitial, o => o.MapFrom(s => s["di"]))
                    .ForMember(d => d.dateEnd, o => o.MapFrom(s => s["df"]))
                    .ForMember(d => d.accumulatedQuantity, o => o.MapFrom(s => s["aq"]))
                    .ForMember(d => d.accumulatedTime, o => o.MapFrom(s => s["at"]))
                    .ForMember(d => d.sector, o => o.MapFrom(s => s["g"]))
                    .ForMember(d => d.sectorcolor, o => o.MapFrom(s => s["sectorcolor"]))
                    .ForMember(d => d.sectorname, o => o.MapFrom(s => s["sectorname"]))
                    .ForMember(d => d.zone, o => o.MapFrom(s => s["zone"]))
                    .ForMember(d => d.zonecolor, o => o.MapFrom(s => s["zonecolor"]))
                    .ForMember(d => d.zonename, o => o.MapFrom(s => s["zonename"]))
                    .ForMember(d => d.longitude, o => o.MapFrom(s => s["lg"]))
                    .ForMember(d => d.latitude, o => o.MapFrom(s => s["lt"]))
                    .ForMember(d => d.reference, o => o.MapFrom(s => s["re"]))
                    .ForMember(d => d.refundable, o => o.MapFrom(s => s["rfd"]))
                    .ForMember(d => d.operationDate, o => o.MapFrom(s => s["od"]))
                    .ForMember(d => d.streetname, o => o.MapFrom(s => s["streetname"]))
                    .ForMember(d => d.streetno, o => o.MapFrom(s => s["streetno"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configParkingStatusResident()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ParkingStatusResidentInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.status, o => o.MapFrom(s => s["sta"]))
                    .ForMember(d => d.extension, o => o.MapFrom(s => s["ex"]))
                    .ForMember(d => d.tariffId, o => o.MapFrom(s => s["id"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["ad"]))
                    .ForMember(d => d.operationType, o => o.MapFrom(s => s["o"]))
                    .ForMember(d => d.dateInitial, o => o.MapFrom(s => s["di"]))
                    .ForMember(d => d.dateEnd, o => o.MapFrom(s => s["df"]))
                    .ForMember(d => d.accumulatedQuantity, o => o.MapFrom(s => s["aq"]))
                    .ForMember(d => d.accumulatedTime, o => o.MapFrom(s => s["at"]))
                    .ForMember(d => d.sector, o => o.MapFrom(s => s["g"]))
                    .ForMember(d => d.sectorcolor, o => o.MapFrom(s => s["sectorcolor"]))
                    .ForMember(d => d.sectorname, o => o.MapFrom(s => s["sectorname"]))
                    .ForMember(d => d.zone, o => o.MapFrom(s => s["zone"]))
                    .ForMember(d => d.zonecolor, o => o.MapFrom(s => s["zonecolor"]))
                    .ForMember(d => d.zonename, o => o.MapFrom(s => s["zonename"]))
                    .ForMember(d => d.longitude, o => o.MapFrom(s => s["lg"]))
                    .ForMember(d => d.latitude, o => o.MapFrom(s => s["lt"]))
                    .ForMember(d => d.reference, o => o.MapFrom(s => s["re"]))
                    .ForMember(d => d.refundable, o => o.MapFrom(s => s["rfd"]))
                    .ForMember(d => d.operationDate, o => o.MapFrom(s => s["od"]))
                    .ForMember(d => d.streetname, o => o.MapFrom(s => s["streetname"]))
                    .ForMember(d => d.streetno, o => o.MapFrom(s => s["streetno"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configParkingStatusTariffs()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, ParkingStatusTariffInfo>()
                    .ForMember(d => d.tariffId, o => o.MapFrom(s => s["tarid"]))
                    .ForMember(d => d.tariffDescription, o => o.MapFrom(s => s["tardesc"]))
                    .ForMember(d => d.tariffType, o => o.MapFrom(s => s["tarad"]))
                    .ForMember(d => d.tariffRefundable, o => o.MapFrom(s => s["tarrfd"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }

        public MapperConfiguration configUserRecharge()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, UserRechargeInfo>()
                    .ForMember(d => d.result, o => o.MapFrom(s => s["r"]))
                    .ForMember(d => d.orderId, o => o.MapFrom(s => s["or"]))
                    .ForMember(d => d.urlResponse, o => o.MapFrom(s => s["mu"]))
                    .ForMember(d => d.urlPayTpv, o => o.MapFrom(s => s["up"]))
                    ;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }
        


    }
}