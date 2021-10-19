using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AutoMapper;
using LiteDB;

namespace DiCore.Lib.NDT.Carrier
{
    public class LocalLoader:ICarrierLoader
    {
        private LiteCollection<CarrierDto> carrierCollection;
        private const string DbName = "Carriers.db";

        private static IMapper mapper;

        public string DbFilePath { get; }   

        public LocalLoader()
        {
            var localCarrierDbPath = DbSettings.LocalCarrierDbPath;
            DbFilePath = Path.Combine(String.IsNullOrEmpty(localCarrierDbPath)?Application.StartupPath:localCarrierDbPath, DbName);

            if (File.Exists(DbFilePath))
            {
                var localDb = new LiteDatabase(DbFilePath);
                carrierCollection = localDb.GetCollection<CarrierDto>();
                carrierCollection.EnsureIndex(carrier => carrier.OldId);
            }
            else
            {
                carrierCollection = null;
            }

            CreateMapper();
        }

        public LocalLoader(string dbFilePath)
        {
            DbFilePath = dbFilePath;

            if (File.Exists(DbFilePath))
            {
                var localDb = new LiteDatabase(DbFilePath);
                carrierCollection = localDb.GetCollection<CarrierDto>();
                carrierCollection.EnsureIndex(carrier => carrier.OldId);
            }
            else
            {
                carrierCollection = null;
            }

            CreateMapper();
        }

        private void CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Carrier, CarrierDto>()
                    .ForMember(dest => dest.Id, op => op.MapFrom(item => item.ID))
                    .ForMember(dest => dest.Circle, op => op.MapFrom(item => item.Circle))
                    .ForMember(dest => dest.Description, op => op.MapFrom(item => item.Description))
                    .ForMember(dest => dest.Diametr, op => op.MapFrom(item => item.Diametr))
                    .ForMember(dest => dest.Name, op => op.MapFrom(item => item.Name))
                    .ForMember(dest => dest.OldId, op => op.MapFrom(item => item.OldId))
                    .ForMember(dest => dest.PigType, op => op.MapFrom(item => item.PigType))
                    .ForMember(dest => dest.MinSpeed, op => op.MapFrom(item => item.MinSpeed))
                    .ForMember(dest => dest.MaxSpeed, op => op.MapFrom(item => item.MaxSpeed))
                    .ForMember(dest => dest.Polymorph, op => op.MapFrom(item => item.Polymorph))
                    .ForMember(dest => dest.Items, op => op.MapFrom(item => item.ToList().Select(value => new SensorDto()
                    {
                        Primary = value.Primary,
                        LogicalNumber = value.LogicalNumber,
                        PhysicalNumber = value.PhysicalNumber,
                        OpposedLogicalNumber = value.OpposedLogicalNumber,
                        GroupNumber = value.GroupNumber,
                        SkiNumber = value.SkiNumber,
                        Delay = value.Delay,
                        Dx = value.Dx,
                        Dy = value.Dy,
                        Angle = value.Angle,
                        Angle2 = value.Angle2,
                        Bodynum = value.Bodynum,
                        SinAngle2 = value.SinAngle2,
                        CosAngle2 = value.CosAngle2,
                        DirectionCode = value.DirectionCode,
                        DirectionInitialCode = value.DirectionInitialCode
                    }).ToArray()))
                    .ForMember(dest => dest.ClassId, op => op.Ignore());

                cfg.CreateMap<CarrierDto, Carrier>()
                    .ConstructUsing(x => new Carrier(x.Id, x.Items.Select(i => new Sensor()
                     {
                            Dx = i.Dx,
                            Dy = i.Dy,
                            Angle = i.Angle,
                            Delay = i.Delay,
                            Angle2 = i.Angle2,
                            Bodynum = i.Bodynum,
                            Primary = i.Primary,
                            CosAngle2 = i.CosAngle2,
                            SinAngle2 = i.SinAngle2,
                            SkiNumber = i.SkiNumber,
                            GroupNumber = i.GroupNumber,
                            DirectionCode = i.DirectionCode,
                            LogicalNumber = i.LogicalNumber,
                            PhysicalNumber = i.PhysicalNumber,
                            DirectionInitialCode = i.DirectionInitialCode,
                            OpposedLogicalNumber = i.OpposedLogicalNumber
                        }).ToArray()))
                    .ForMember(dest => dest.Circle, opt => opt.MapFrom(item => item.Circle))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(item => item.Description))
                    .ForMember(dest => dest.Diametr, opt => opt.MapFrom(item => item.Diametr))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(item => item.Name))
                    .ForMember(dest => dest.PigType, opt => opt.MapFrom(item => item.PigType))
                    .ForMember(dest => dest.OldId, opt => opt.MapFrom(item => item.OldId))
                    .ForMember(dest => dest.MinSpeed, op => op.MapFrom(item => item.MinSpeed))
                    .ForMember(dest => dest.MaxSpeed, op => op.MapFrom(item => item.MaxSpeed))
                    .ForMember(dest => dest.Polymorph, op => op.MapFrom(item => item.Polymorph))
                    .ForMember(dest => dest.SensorCount, opt => opt.MapFrom(item => item.Items.Length));
            });

            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();
        }

        private Carrier InnerLoad(int oldCarrierId)
        {
            var carrierDto = carrierCollection?.FindOne(carrier => carrier.OldId == oldCarrierId);
            if (carrierDto == null) return null;

            return mapper.Map<Carrier>(carrierDto);
        }

        public Carrier Load(int oldCarrierId)
        {
            return InnerLoad(oldCarrierId);
        }

        public void AddCarrier(CarrierDto addedCarrier)
        {
            if (carrierCollection == null)
            {
                var localDb       = new LiteDatabase(DbFilePath);
                carrierCollection = localDb.GetCollection<CarrierDto>();
            }

            carrierCollection.Insert(addedCarrier);
        }
    }
}
