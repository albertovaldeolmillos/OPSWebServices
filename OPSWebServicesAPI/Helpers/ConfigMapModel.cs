using AutoMapper;
using OPSWebServicesAPI.Models;
using System.Collections;

namespace OPSWebServicesAPI.Helpers
{
    public class ConfigMapModel
    {
        MapperConfiguration mapperConfiguration { get; set; }

        public MapperConfiguration configO()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SortedList, O>()
                    .ForMember(d => d.bns, o => o.MapFrom(s => s["bns"]))
                    .ForMember(d => d.contid, o => o.MapFrom(s => s["contid"]))
                    .ForMember(d => d.contname, o => o.MapFrom(s => s["contname"]))
                    .ForMember(d => d.ed, o => o.MapFrom(s => s["ed"]))
                    .ForMember(d => d.farticle, o => o.MapFrom(s => s["farticle"]))
                    .ForMember(d => d.fcolor, o => o.MapFrom(s => s["fcolor"]))
                    .ForMember(d => d.fmake, o => o.MapFrom(s => s["fmake"]))
                    .ForMember(d => d.fn, o => o.MapFrom(s => s["fn"]))
                    .ForMember(d => d.fpd, o => o.MapFrom(s => s["fpd"]))
                    .ForMember(d => d.fs, o => o.MapFrom(s => s["fs"]))
                    .ForMember(d => d.fstreet, o => o.MapFrom(s => s["fstreet"]))
                    .ForMember(d => d.fstrnum, o => o.MapFrom(s => s["fstrnum"]))
                    .ForMember(d => d.on, o => o.MapFrom(s => s["on"]))
                    .ForMember(d => d.ot, o => o.MapFrom(s => s["ot"]))
                    .ForMember(d => d.pa, o => o.MapFrom(s => s["pa"]))
                    .ForMember(d => d.pl, o => o.MapFrom(s => s["pl"]))
                    .ForMember(d => d.pm, o => o.MapFrom(s => s["pm"]))
                    .ForMember(d => d.pp, o => o.MapFrom(s => s["pp"]))
                    .ForMember(d => d.rd, o => o.MapFrom(s => s["rd"]))
                    .ForMember(d => d.sd, o => o.MapFrom(s => s["sd"]))
                    .ForMember(d => d.sta, o => o.MapFrom(s => s["sta"]))
                    .ForMember(d => d.zo, o => o.MapFrom(s => s["zo"]))
                    .ForMember(d => d.zonecolor, o => o.MapFrom(s => s["zonecolor"]))
                    .ForMember(d => d.zonename, o => o.MapFrom(s => s["zonename"]));
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });
            return config;
        }
    }
}