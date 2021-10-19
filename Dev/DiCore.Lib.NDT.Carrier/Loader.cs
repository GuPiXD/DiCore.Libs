using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace DiCore.Lib.NDT.Carrier
{
    public class Loader: ICarrierLoader
    {
        internal static readonly Guid SensorCarrierId = Guid.Parse("4d556a50-af43-401b-ac3c-44a279ada601");
        private readonly string connectionString;
        private IMapper mapper;

        public Loader()
        {
            connectionString = $"Host={DbSettings.Server};Database=UniscanBdi;Username={DbSettings.Login};Password={DbSettings.Password}";

            CreateMapper();
        }

        private Carrier InnerLoad(int oldCarrierId)
        {
            return Select(oldCarrierId);
        }

        private Carrier Select(int oldCarrierId)
        {
            using (var connection = CreateConnection())
            using (var cmd = connection.CreateCommand())
            {
                Carrier value;
                try
                {
                    connection.Open();

                    var condition = "@> '{\"OldId\": " + oldCarrierId + "}'";
                    cmd.CommandText = $"SELECT \"Id\", \"Item\" FROM settings.\"Settings\" WHERE \"ClassId\" = @ClassId AND \"Settings\".\"Item\" {condition}";
                    
                    cmd.Parameters.AddWithValue("ClassId", NpgsqlDbType.Uuid, SensorCarrierId);

                    var reader = cmd.ExecuteReader();
                    if (!reader.Read()) return default(Carrier);

                    value = PrepareValue(reader);

                    reader.Close();
                    connection.Close();
                }
                catch
                {
                    return null;
                }
                
                return value;
            }
        }

        private Carrier PrepareValue(NpgsqlDataReader reader)
        {
            Carrier value;

            if (!reader.HasRows) return default(Carrier);

            var id = reader.GetGuid(0);
            var items = reader.GetString(1);

            if (String.IsNullOrEmpty(items))
            {
                value = default(Carrier);
            }
            else
            {
                var valueDto = JsonConvert.DeserializeObject<CarrierDto>(items);

                valueDto.Id = id;
                valueDto.ClassId = SensorCarrierId;
                value = mapper.Map<Carrier>(valueDto);
            }
            return value;
        }

        private CarrierDto GetDtoValue(NpgsqlDataReader reader)
        {
            if (!reader.HasRows) return default(CarrierDto);

            var id = reader.GetGuid(0);
            var items = reader.GetString(1);

            if (String.IsNullOrEmpty(items))
            {
                return default(CarrierDto);
            }

            var valueDto = JsonConvert.DeserializeObject<CarrierDto>(items);

            valueDto.Id = id;
            valueDto.ClassId = SensorCarrierId;
            return valueDto;
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(connectionString);
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
                    .ForMember(dest => dest.Items, op => op.MapFrom(item => item.Select(value => new SensorDto()
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
                    .ForMember(dest => dest.SensorCount, opt => opt.Ignore())
                    .ForMember(dest => dest.ID, op => op.Ignore());
            });

            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();
        }

        public Carrier Load(int oldCarrierId)
        {
            return InnerLoad(oldCarrierId);
        }

        public IEnumerable<CarrierDto> GetAllCarriers()
        {
            using (var connection = CreateConnection())
            using (var cmd = connection.CreateCommand())
            {
                try
                {
                    connection.Open();
                }
                catch
                {
                    yield break;
                }

                cmd.CommandText = $"SELECT \"Id\", \"Item\" FROM settings.\"Settings\" WHERE \"ClassId\" = @ClassId";

                cmd.Parameters.AddWithValue("ClassId", NpgsqlDbType.Uuid, SensorCarrierId);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    yield return GetDtoValue(reader);
                }

                reader.Close();
                connection.Close();
            }
        }
    }
}
