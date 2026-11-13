using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Parking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VehicleCatalogSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----------------------------------------------------------------
            // Insert brands (INSERT IGNORE skips if already present)
            // ----------------------------------------------------------------
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_brands (name) VALUES
                -- Japonesas
                ('Toyota'),
                ('Honda'),
                ('Nissan'),
                ('Mazda'),
                ('Mitsubishi'),
                ('Subaru'),
                ('Suzuki'),
                ('Isuzu'),
                ('Daihatsu'),
                ('Lexus'),
                ('Infiniti'),
                ('Acura'),
                -- Coreanas
                ('Hyundai'),
                ('Kia'),
                ('Genesis'),
                ('SsangYong'),
                -- Alemãs
                ('BMW'),
                ('Mercedes-Benz'),
                ('Volkswagen'),
                ('Audi'),
                ('Porsche'),
                ('Opel'),
                ('Smart'),
                -- Americanas
                ('Ford'),
                ('Chevrolet'),
                ('Dodge'),
                ('Jeep'),
                ('Chrysler'),
                ('Cadillac'),
                ('Buick'),
                ('GMC'),
                ('Lincoln'),
                ('Tesla'),
                ('RAM'),
                -- Britânicas
                ('Land Rover'),
                ('Range Rover'),
                ('Jaguar'),
                ('Bentley'),
                ('Rolls-Royce'),
                ('Aston Martin'),
                ('Mini'),
                -- Italianas
                ('Fiat'),
                ('Alfa Romeo'),
                ('Ferrari'),
                ('Lamborghini'),
                ('Maserati'),
                ('Lancia'),
                -- Francesas
                ('Renault'),
                ('Peugeot'),
                ('Citroën'),
                ('DS Automobiles'),
                -- Suecas
                ('Volvo'),
                ('Saab'),
                -- Espanholas
                ('SEAT'),
                ('Cupra'),
                -- Checas
                ('Škoda'),
                -- Romenas
                ('Dacia'),
                -- Russas / Soviéticas
                ('Lada'),
                -- Chinesas
                ('BYD'),
                ('Great Wall'),
                ('Chery'),
                ('Geely'),
                ('Haval'),
                ('MG'),
                ('JAC'),
                ('Foton'),
                ('FAW'),
                ('BAIC'),
                ('Dongfeng'),
                ('Changan'),
                ('Lynk & Co'),
                ('Polestar'),
                -- Indianas
                ('Tata'),
                ('Mahindra'),
                -- Comerciais / Camiões
                ('Mercedes-Benz Trucks'),
                ('Volvo Trucks'),
                ('MAN'),
                ('Scania'),
                ('DAF'),
                ('Iveco'),
                ('FUSO'),
                ('Hino'),
                ('UD Trucks'),
                ('Renault Trucks'),
                -- Motociclos
                ('Yamaha'),
                ('Honda Motos'),
                ('Kawasaki'),
                ('Suzuki Motos'),
                ('Harley-Davidson'),
                ('KTM'),
                ('BMW Motorrad'),
                ('Ducati'),
                ('Triumph'),
                ('Royal Enfield'),
                -- Outros
                ('Alfa')
                ;
                """);

            // ----------------------------------------------------------------
            // Insert models per brand using a subquery for the brand id
            // INSERT IGNORE skips duplicates (ux on brand_id + name)
            // ----------------------------------------------------------------

            // Toyota
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Corolla'            AS name UNION ALL
                      SELECT 'Corolla Cross'      UNION ALL
                      SELECT 'Camry'              UNION ALL
                      SELECT 'Yaris'              UNION ALL
                      SELECT 'Hilux'              UNION ALL
                      SELECT 'Hilux SW4'          UNION ALL
                      SELECT 'RAV4'               UNION ALL
                      SELECT 'Land Cruiser'       UNION ALL
                      SELECT 'Land Cruiser Prado' UNION ALL
                      SELECT 'FJ Cruiser'         UNION ALL
                      SELECT 'Fortuner'           UNION ALL
                      SELECT 'Prado'              UNION ALL
                      SELECT 'Avanza'             UNION ALL
                      SELECT 'Rush'               UNION ALL
                      SELECT 'Prius'              UNION ALL
                      SELECT 'C-HR'               UNION ALL
                      SELECT 'Venza'              UNION ALL
                      SELECT 'Sequoia'            UNION ALL
                      SELECT 'Tundra'             UNION ALL
                      SELECT 'Tacoma'             UNION ALL
                      SELECT 'HiAce'              UNION ALL
                      SELECT 'HiAce Van'          UNION ALL
                      SELECT 'Innova'             UNION ALL
                      SELECT 'Aygo'               UNION ALL
                      SELECT 'bZ4X'               UNION ALL
                      SELECT 'GR86'               UNION ALL
                      SELECT 'GR Supra'           UNION ALL
                      SELECT 'Sienna'             UNION ALL
                      SELECT 'Estima'             UNION ALL
                      SELECT 'Alphard'            UNION ALL
                      SELECT 'Vellfire'           UNION ALL
                      SELECT 'Fielder'            UNION ALL
                      SELECT 'Auris'              UNION ALL
                      SELECT 'RAV4 Hybrid'        UNION ALL
                      SELECT 'Corolla Hybrid'
                     ) m ON b.name = 'Toyota';
                """);

            // Honda
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Civic'          AS name UNION ALL
                      SELECT 'Accord'         UNION ALL
                      SELECT 'CR-V'           UNION ALL
                      SELECT 'HR-V'           UNION ALL
                      SELECT 'Pilot'          UNION ALL
                      SELECT 'Passport'       UNION ALL
                      SELECT 'Ridgeline'      UNION ALL
                      SELECT 'Jazz'           UNION ALL
                      SELECT 'Fit'            UNION ALL
                      SELECT 'Insight'        UNION ALL
                      SELECT 'Odyssey'        UNION ALL
                      SELECT 'Element'        UNION ALL
                      SELECT 'Stream'         UNION ALL
                      SELECT 'Freed'          UNION ALL
                      SELECT 'BR-V'           UNION ALL
                      SELECT 'WR-V'           UNION ALL
                      SELECT 'ZR-V'           UNION ALL
                      SELECT 'e:Ny1'          UNION ALL
                      SELECT 'Prologue'       UNION ALL
                      SELECT 'City'           UNION ALL
                      SELECT 'Brio'
                     ) m ON b.name = 'Honda';
                """);

            // Nissan
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Almera'       AS name UNION ALL
                      SELECT 'Sentra'       UNION ALL
                      SELECT 'Altima'       UNION ALL
                      SELECT 'Maxima'       UNION ALL
                      SELECT 'Juke'         UNION ALL
                      SELECT 'Qashqai'      UNION ALL
                      SELECT 'X-Trail'      UNION ALL
                      SELECT 'Pathfinder'   UNION ALL
                      SELECT 'Murano'       UNION ALL
                      SELECT 'Armada'       UNION ALL
                      SELECT 'Patrol'       UNION ALL
                      SELECT 'Frontier'     UNION ALL
                      SELECT 'Navara'       UNION ALL
                      SELECT 'Titan'        UNION ALL
                      SELECT 'Note'         UNION ALL
                      SELECT 'Micra'        UNION ALL
                      SELECT 'March'        UNION ALL
                      SELECT 'Kicks'        UNION ALL
                      SELECT 'Rogue'        UNION ALL
                      SELECT 'Leaf'         UNION ALL
                      SELECT 'Ariya'        UNION ALL
                      SELECT 'GT-R'         UNION ALL
                      SELECT '370Z'         UNION ALL
                      SELECT '350Z'
                     ) m ON b.name = 'Nissan';
                """);

            // Mazda
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Mazda2'      AS name UNION ALL
                      SELECT 'Mazda3'      UNION ALL
                      SELECT 'Mazda6'      UNION ALL
                      SELECT 'CX-3'        UNION ALL
                      SELECT 'CX-30'       UNION ALL
                      SELECT 'CX-5'        UNION ALL
                      SELECT 'CX-60'       UNION ALL
                      SELECT 'CX-70'       UNION ALL
                      SELECT 'CX-80'       UNION ALL
                      SELECT 'CX-90'       UNION ALL
                      SELECT 'BT-50'       UNION ALL
                      SELECT 'MX-5'        UNION ALL
                      SELECT 'MX-30'       UNION ALL
                      SELECT 'RX-8'
                     ) m ON b.name = 'Mazda';
                """);

            // Mitsubishi
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'L200'           AS name UNION ALL
                      SELECT 'Pajero'         UNION ALL
                      SELECT 'Pajero Sport'   UNION ALL
                      SELECT 'Outlander'      UNION ALL
                      SELECT 'Eclipse Cross'  UNION ALL
                      SELECT 'ASX'            UNION ALL
                      SELECT 'Galant'         UNION ALL
                      SELECT 'Lancer'         UNION ALL
                      SELECT 'Mirage'         UNION ALL
                      SELECT 'Xpander'        UNION ALL
                      SELECT 'Triton'         UNION ALL
                      SELECT 'Montero'        UNION ALL
                      SELECT 'Colt'
                     ) m ON b.name = 'Mitsubishi';
                """);

            // Subaru
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Impreza'   AS name UNION ALL
                      SELECT 'Legacy'    UNION ALL
                      SELECT 'Outback'   UNION ALL
                      SELECT 'Forester'  UNION ALL
                      SELECT 'XV'        UNION ALL
                      SELECT 'Crosstrek' UNION ALL
                      SELECT 'Ascent'    UNION ALL
                      SELECT 'BRZ'       UNION ALL
                      SELECT 'WRX'
                     ) m ON b.name = 'Subaru';
                """);

            // Suzuki
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Swift'      AS name UNION ALL
                      SELECT 'Vitara'     UNION ALL
                      SELECT 'S-Cross'    UNION ALL
                      SELECT 'Jimny'      UNION ALL
                      SELECT 'Grand Vitara' UNION ALL
                      SELECT 'Baleno'     UNION ALL
                      SELECT 'Ignis'      UNION ALL
                      SELECT 'Ciaz'       UNION ALL
                      SELECT 'Ertiga'     UNION ALL
                      SELECT 'XL7'        UNION ALL
                      SELECT 'Carry'      UNION ALL
                      SELECT 'Alto'
                     ) m ON b.name = 'Suzuki';
                """);

            // Isuzu
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'D-Max'     AS name UNION ALL
                      SELECT 'MU-X'      UNION ALL
                      SELECT 'Trooper'   UNION ALL
                      SELECT 'Rodeo'     UNION ALL
                      SELECT 'NPR'       UNION ALL
                      SELECT 'NQR'       UNION ALL
                      SELECT 'FRR'       UNION ALL
                      SELECT 'FSR'
                     ) m ON b.name = 'Isuzu';
                """);

            // Hyundai
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'i10'          AS name UNION ALL
                      SELECT 'i20'          UNION ALL
                      SELECT 'i30'          UNION ALL
                      SELECT 'i40'          UNION ALL
                      SELECT 'Elantra'      UNION ALL
                      SELECT 'Sonata'       UNION ALL
                      SELECT 'Azera'        UNION ALL
                      SELECT 'Accent'       UNION ALL
                      SELECT 'Verna'        UNION ALL
                      SELECT 'Tucson'       UNION ALL
                      SELECT 'Santa Fe'     UNION ALL
                      SELECT 'Palisade'     UNION ALL
                      SELECT 'Venue'        UNION ALL
                      SELECT 'Kona'         UNION ALL
                      SELECT 'Creta'        UNION ALL
                      SELECT 'Staria'       UNION ALL
                      SELECT 'H-1'          UNION ALL
                      SELECT 'H100'         UNION ALL
                      SELECT 'Porter'       UNION ALL
                      SELECT 'Ioniq'        UNION ALL
                      SELECT 'Ioniq 5'      UNION ALL
                      SELECT 'Ioniq 6'      UNION ALL
                      SELECT 'Ioniq 7'      UNION ALL
                      SELECT 'NEXO'         UNION ALL
                      SELECT 'Veloster'     UNION ALL
                      SELECT 'Equus'
                     ) m ON b.name = 'Hyundai';
                """);

            // Kia
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Picanto'    AS name UNION ALL
                      SELECT 'Rio'        UNION ALL
                      SELECT 'Cerato'     UNION ALL
                      SELECT 'Stinger'    UNION ALL
                      SELECT 'K5'         UNION ALL
                      SELECT 'K8'         UNION ALL
                      SELECT 'Stonic'     UNION ALL
                      SELECT 'Seltos'     UNION ALL
                      SELECT 'Sportage'   UNION ALL
                      SELECT 'Sorento'    UNION ALL
                      SELECT 'Telluride'  UNION ALL
                      SELECT 'Mohave'     UNION ALL
                      SELECT 'Carnival'   UNION ALL
                      SELECT 'Niro'       UNION ALL
                      SELECT 'EV6'        UNION ALL
                      SELECT 'EV9'        UNION ALL
                      SELECT 'Soul'       UNION ALL
                      SELECT 'Carens'     UNION ALL
                      SELECT 'Ceed'
                     ) m ON b.name = 'Kia';
                """);

            // BMW
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT '1 Series'  AS name UNION ALL
                      SELECT '2 Series'  UNION ALL
                      SELECT '3 Series'  UNION ALL
                      SELECT '4 Series'  UNION ALL
                      SELECT '5 Series'  UNION ALL
                      SELECT '6 Series'  UNION ALL
                      SELECT '7 Series'  UNION ALL
                      SELECT '8 Series'  UNION ALL
                      SELECT 'X1'        UNION ALL
                      SELECT 'X2'        UNION ALL
                      SELECT 'X3'        UNION ALL
                      SELECT 'X4'        UNION ALL
                      SELECT 'X5'        UNION ALL
                      SELECT 'X6'        UNION ALL
                      SELECT 'X7'        UNION ALL
                      SELECT 'Z3'        UNION ALL
                      SELECT 'Z4'        UNION ALL
                      SELECT 'M2'        UNION ALL
                      SELECT 'M3'        UNION ALL
                      SELECT 'M4'        UNION ALL
                      SELECT 'M5'        UNION ALL
                      SELECT 'M8'        UNION ALL
                      SELECT 'iX'        UNION ALL
                      SELECT 'iX1'       UNION ALL
                      SELECT 'iX3'       UNION ALL
                      SELECT 'i3'        UNION ALL
                      SELECT 'i4'        UNION ALL
                      SELECT 'i5'        UNION ALL
                      SELECT 'i7'
                     ) m ON b.name = 'BMW';
                """);

            // Mercedes-Benz
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'A-Class'    AS name UNION ALL
                      SELECT 'B-Class'    UNION ALL
                      SELECT 'C-Class'    UNION ALL
                      SELECT 'CLA'        UNION ALL
                      SELECT 'CLS'        UNION ALL
                      SELECT 'E-Class'    UNION ALL
                      SELECT 'S-Class'    UNION ALL
                      SELECT 'GLA'        UNION ALL
                      SELECT 'GLB'        UNION ALL
                      SELECT 'GLC'        UNION ALL
                      SELECT 'GLE'        UNION ALL
                      SELECT 'GLS'        UNION ALL
                      SELECT 'G-Class'    UNION ALL
                      SELECT 'AMG GT'     UNION ALL
                      SELECT 'SL'         UNION ALL
                      SELECT 'SLC'        UNION ALL
                      SELECT 'V-Class'    UNION ALL
                      SELECT 'Marco Polo' UNION ALL
                      SELECT 'Sprinter'   UNION ALL
                      SELECT 'Vito'       UNION ALL
                      SELECT 'Viano'      UNION ALL
                      SELECT 'EQA'        UNION ALL
                      SELECT 'EQB'        UNION ALL
                      SELECT 'EQC'        UNION ALL
                      SELECT 'EQE'        UNION ALL
                      SELECT 'EQS'
                     ) m ON b.name = 'Mercedes-Benz';
                """);

            // Volkswagen
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Polo'        AS name UNION ALL
                      SELECT 'Golf'        UNION ALL
                      SELECT 'Golf GTI'    UNION ALL
                      SELECT 'Golf R'      UNION ALL
                      SELECT 'Jetta'       UNION ALL
                      SELECT 'Passat'      UNION ALL
                      SELECT 'Arteon'      UNION ALL
                      SELECT 'Phaeton'     UNION ALL
                      SELECT 'Touareg'     UNION ALL
                      SELECT 'Tiguan'      UNION ALL
                      SELECT 'T-Cross'     UNION ALL
                      SELECT 'T-Roc'       UNION ALL
                      SELECT 'Taos'        UNION ALL
                      SELECT 'Touran'      UNION ALL
                      SELECT 'Sharan'      UNION ALL
                      SELECT 'Caddy'       UNION ALL
                      SELECT 'Transporter' UNION ALL
                      SELECT 'Crafter'     UNION ALL
                      SELECT 'Amarok'      UNION ALL
                      SELECT 'ID.3'        UNION ALL
                      SELECT 'ID.4'        UNION ALL
                      SELECT 'ID.5'        UNION ALL
                      SELECT 'ID.6'        UNION ALL
                      SELECT 'ID.7'        UNION ALL
                      SELECT 'Beetle'      UNION ALL
                      SELECT 'Up!'
                     ) m ON b.name = 'Volkswagen';
                """);

            // Audi
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'A1'      AS name UNION ALL
                      SELECT 'A3'      UNION ALL
                      SELECT 'A4'      UNION ALL
                      SELECT 'A5'      UNION ALL
                      SELECT 'A6'      UNION ALL
                      SELECT 'A7'      UNION ALL
                      SELECT 'A8'      UNION ALL
                      SELECT 'Q2'      UNION ALL
                      SELECT 'Q3'      UNION ALL
                      SELECT 'Q4 e-tron' UNION ALL
                      SELECT 'Q5'      UNION ALL
                      SELECT 'Q7'      UNION ALL
                      SELECT 'Q8'      UNION ALL
                      SELECT 'e-tron'  UNION ALL
                      SELECT 'e-tron GT' UNION ALL
                      SELECT 'TT'      UNION ALL
                      SELECT 'TTS'     UNION ALL
                      SELECT 'R8'      UNION ALL
                      SELECT 'RS3'     UNION ALL
                      SELECT 'RS4'     UNION ALL
                      SELECT 'RS5'     UNION ALL
                      SELECT 'RS6'     UNION ALL
                      SELECT 'RS7'     UNION ALL
                      SELECT 'S3'      UNION ALL
                      SELECT 'S4'      UNION ALL
                      SELECT 'S5'      UNION ALL
                      SELECT 'S6'      UNION ALL
                      SELECT 'S8'
                     ) m ON b.name = 'Audi';
                """);

            // Porsche
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT '911'         AS name UNION ALL
                      SELECT '718 Boxster' UNION ALL
                      SELECT '718 Cayman' UNION ALL
                      SELECT 'Taycan'      UNION ALL
                      SELECT 'Panamera'    UNION ALL
                      SELECT 'Cayenne'     UNION ALL
                      SELECT 'Macan'
                     ) m ON b.name = 'Porsche';
                """);

            // Opel
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Corsa'    AS name UNION ALL
                      SELECT 'Astra'    UNION ALL
                      SELECT 'Insignia' UNION ALL
                      SELECT 'Mokka'    UNION ALL
                      SELECT 'Crossland' UNION ALL
                      SELECT 'Grandland' UNION ALL
                      SELECT 'Zafira'   UNION ALL
                      SELECT 'Meriva'   UNION ALL
                      SELECT 'Combo'    UNION ALL
                      SELECT 'Vivaro'   UNION ALL
                      SELECT 'Movano'
                     ) m ON b.name = 'Opel';
                """);

            // Ford
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Fiesta'      AS name UNION ALL
                      SELECT 'Focus'       UNION ALL
                      SELECT 'Fusion'      UNION ALL
                      SELECT 'Mondeo'      UNION ALL
                      SELECT 'Mustang'     UNION ALL
                      SELECT 'Mustang Mach-E' UNION ALL
                      SELECT 'Ranger'      UNION ALL
                      SELECT 'F-150'       UNION ALL
                      SELECT 'F-250'       UNION ALL
                      SELECT 'F-350'       UNION ALL
                      SELECT 'Explorer'    UNION ALL
                      SELECT 'Expedition'  UNION ALL
                      SELECT 'Escape'      UNION ALL
                      SELECT 'Edge'        UNION ALL
                      SELECT 'Bronco'      UNION ALL
                      SELECT 'Maverick'    UNION ALL
                      SELECT 'EcoSport'    UNION ALL
                      SELECT 'Puma'        UNION ALL
                      SELECT 'Kuga'        UNION ALL
                      SELECT 'Galaxy'      UNION ALL
                      SELECT 'S-Max'       UNION ALL
                      SELECT 'Transit'     UNION ALL
                      SELECT 'Transit Connect' UNION ALL
                      SELECT 'Transit Custom' UNION ALL
                      SELECT 'Tourneo'
                     ) m ON b.name = 'Ford';
                """);

            // Chevrolet
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Spark'       AS name UNION ALL
                      SELECT 'Aveo'        UNION ALL
                      SELECT 'Cruze'       UNION ALL
                      SELECT 'Malibu'      UNION ALL
                      SELECT 'Impala'      UNION ALL
                      SELECT 'Camaro'      UNION ALL
                      SELECT 'Corvette'    UNION ALL
                      SELECT 'Equinox'     UNION ALL
                      SELECT 'Trax'        UNION ALL
                      SELECT 'Trailblazer' UNION ALL
                      SELECT 'Blazer'      UNION ALL
                      SELECT 'Traverse'    UNION ALL
                      SELECT 'Tahoe'       UNION ALL
                      SELECT 'Suburban'    UNION ALL
                      SELECT 'Colorado'    UNION ALL
                      SELECT 'Silverado'   UNION ALL
                      SELECT 'S10'         UNION ALL
                      SELECT 'Captiva'     UNION ALL
                      SELECT 'Orlando'     UNION ALL
                      SELECT 'Spin'        UNION ALL
                      SELECT 'Tracker'     UNION ALL
                      SELECT 'Montana'
                     ) m ON b.name = 'Chevrolet';
                """);

            // Dodge / Jeep / RAM
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Charger'   AS name UNION ALL
                      SELECT 'Challenger' UNION ALL
                      SELECT 'Durango'   UNION ALL
                      SELECT 'Journey'   UNION ALL
                      SELECT 'Grand Caravan' UNION ALL
                      SELECT 'Ram 1500'  UNION ALL
                      SELECT 'Ram 2500'
                     ) m ON b.name = 'Dodge';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Wrangler'      AS name UNION ALL
                      SELECT 'Cherokee'      UNION ALL
                      SELECT 'Grand Cherokee' UNION ALL
                      SELECT 'Commander'     UNION ALL
                      SELECT 'Renegade'      UNION ALL
                      SELECT 'Compass'       UNION ALL
                      SELECT 'Gladiator'     UNION ALL
                      SELECT 'Avenger'
                     ) m ON b.name = 'Jeep';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT '1500'  AS name UNION ALL
                      SELECT '2500'  UNION ALL
                      SELECT '3500'  UNION ALL
                      SELECT 'ProMaster'
                     ) m ON b.name = 'RAM';
                """);

            // Land Rover / Range Rover
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Defender'       AS name UNION ALL
                      SELECT 'Discovery'      UNION ALL
                      SELECT 'Discovery Sport' UNION ALL
                      SELECT 'Freelander'     UNION ALL
                      SELECT 'Range Rover Sport' UNION ALL
                      SELECT 'Range Rover Evoque' UNION ALL
                      SELECT 'Range Rover Velar'
                     ) m ON b.name = 'Land Rover';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Range Rover'       AS name UNION ALL
                      SELECT 'Range Rover Sport' UNION ALL
                      SELECT 'Range Rover Evoque' UNION ALL
                      SELECT 'Range Rover Velar'  UNION ALL
                      SELECT 'Range Rover SV'
                     ) m ON b.name = 'Range Rover';
                """);

            // Jaguar
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'XE'      AS name UNION ALL
                      SELECT 'XF'      UNION ALL
                      SELECT 'XJ'      UNION ALL
                      SELECT 'F-Type'  UNION ALL
                      SELECT 'E-Pace'  UNION ALL
                      SELECT 'F-Pace'  UNION ALL
                      SELECT 'I-Pace'
                     ) m ON b.name = 'Jaguar';
                """);

            // Fiat
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Punto'     AS name UNION ALL
                      SELECT 'Bravo'     UNION ALL
                      SELECT 'Brava'     UNION ALL
                      SELECT '500'       UNION ALL
                      SELECT '500X'      UNION ALL
                      SELECT '500L'      UNION ALL
                      SELECT 'Tipo'      UNION ALL
                      SELECT 'Stilo'     UNION ALL
                      SELECT 'Doblo'     UNION ALL
                      SELECT 'Fiorino'   UNION ALL
                      SELECT 'Ducato'    UNION ALL
                      SELECT 'Scudo'     UNION ALL
                      SELECT 'Uno'       UNION ALL
                      SELECT 'Palio'     UNION ALL
                      SELECT 'Siena'     UNION ALL
                      SELECT 'Strada'    UNION ALL
                      SELECT 'Toro'      UNION ALL
                      SELECT 'Pulse'     UNION ALL
                      SELECT 'Fastback'  UNION ALL
                      SELECT 'Cronos'
                     ) m ON b.name = 'Fiat';
                """);

            // Alfa Romeo
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Giulia'      AS name UNION ALL
                      SELECT 'Giulietta'   UNION ALL
                      SELECT 'Stelvio'     UNION ALL
                      SELECT 'Tonale'      UNION ALL
                      SELECT 'Mito'        UNION ALL
                      SELECT '147'         UNION ALL
                      SELECT '156'         UNION ALL
                      SELECT '159'         UNION ALL
                      SELECT 'Spider'      UNION ALL
                      SELECT '4C'
                     ) m ON b.name = 'Alfa Romeo';
                """);

            // Renault
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Clio'        AS name UNION ALL
                      SELECT 'Megane'      UNION ALL
                      SELECT 'Laguna'      UNION ALL
                      SELECT 'Talisman'    UNION ALL
                      SELECT 'Captur'      UNION ALL
                      SELECT 'Kadjar'      UNION ALL
                      SELECT 'Koleos'      UNION ALL
                      SELECT 'Austral'     UNION ALL
                      SELECT 'Arkana'      UNION ALL
                      SELECT 'Zoe'         UNION ALL
                      SELECT 'Twingo'      UNION ALL
                      SELECT 'Sandero'     UNION ALL
                      SELECT 'Duster'      UNION ALL
                      SELECT 'Symbol'      UNION ALL
                      SELECT 'Logan'       UNION ALL
                      SELECT 'Scenic'      UNION ALL
                      SELECT 'Grand Scenic' UNION ALL
                      SELECT 'Espace'      UNION ALL
                      SELECT 'Trafic'      UNION ALL
                      SELECT 'Master'      UNION ALL
                      SELECT 'Kangoo'      UNION ALL
                      SELECT 'Express'     UNION ALL
                      SELECT 'Alaskan'     UNION ALL
                      SELECT 'Duster Oroch'
                     ) m ON b.name = 'Renault';
                """);

            // Peugeot
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT '106'      AS name UNION ALL
                      SELECT '107'      UNION ALL
                      SELECT '108'      UNION ALL
                      SELECT '206'      UNION ALL
                      SELECT '207'      UNION ALL
                      SELECT '208'      UNION ALL
                      SELECT '301'      UNION ALL
                      SELECT '307'      UNION ALL
                      SELECT '308'      UNION ALL
                      SELECT '405'      UNION ALL
                      SELECT '406'      UNION ALL
                      SELECT '407'      UNION ALL
                      SELECT '408'      UNION ALL
                      SELECT '508'      UNION ALL
                      SELECT '2008'     UNION ALL
                      SELECT '3008'     UNION ALL
                      SELECT '4008'     UNION ALL
                      SELECT '5008'     UNION ALL
                      SELECT 'Expert'   UNION ALL
                      SELECT 'Partner'  UNION ALL
                      SELECT 'Boxer'    UNION ALL
                      SELECT 'Traveller'
                     ) m ON b.name = 'Peugeot';
                """);

            // Citroën
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'C1'       AS name UNION ALL
                      SELECT 'C2'       UNION ALL
                      SELECT 'C3'       UNION ALL
                      SELECT 'C3 Aircross' UNION ALL
                      SELECT 'C4'       UNION ALL
                      SELECT 'C4 Cactus' UNION ALL
                      SELECT 'C4 Picasso' UNION ALL
                      SELECT 'C5'       UNION ALL
                      SELECT 'C5 Aircross' UNION ALL
                      SELECT 'C5 X'     UNION ALL
                      SELECT 'Berlingo' UNION ALL
                      SELECT 'Dispatch' UNION ALL
                      SELECT 'Jumpy'    UNION ALL
                      SELECT 'Jumper'   UNION ALL
                      SELECT 'Nemo'     UNION ALL
                      SELECT 'Saxo'     UNION ALL
                      SELECT 'Xsara'
                     ) m ON b.name = 'Citroën';
                """);

            // Volvo
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'S40'   AS name UNION ALL
                      SELECT 'S60'   UNION ALL
                      SELECT 'S80'   UNION ALL
                      SELECT 'S90'   UNION ALL
                      SELECT 'V40'   UNION ALL
                      SELECT 'V60'   UNION ALL
                      SELECT 'V70'   UNION ALL
                      SELECT 'V90'   UNION ALL
                      SELECT 'XC40'  UNION ALL
                      SELECT 'XC60'  UNION ALL
                      SELECT 'XC70'  UNION ALL
                      SELECT 'XC90'  UNION ALL
                      SELECT 'C30'   UNION ALL
                      SELECT 'C40'   UNION ALL
                      SELECT 'EX30'  UNION ALL
                      SELECT 'EX90'
                     ) m ON b.name = 'Volvo';
                """);

            // SEAT / Cupra
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Ibiza'    AS name UNION ALL
                      SELECT 'Leon'     UNION ALL
                      SELECT 'Ateca'    UNION ALL
                      SELECT 'Tarraco'  UNION ALL
                      SELECT 'Arona'    UNION ALL
                      SELECT 'Toledo'   UNION ALL
                      SELECT 'Exeo'     UNION ALL
                      SELECT 'Alhambra'
                     ) m ON b.name = 'SEAT';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Formentor' AS name UNION ALL
                      SELECT 'Born'      UNION ALL
                      SELECT 'Ateca'     UNION ALL
                      SELECT 'Tavascan'
                     ) m ON b.name = 'Cupra';
                """);

            // Škoda
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Fabia'    AS name UNION ALL
                      SELECT 'Scala'    UNION ALL
                      SELECT 'Octavia'  UNION ALL
                      SELECT 'Superb'   UNION ALL
                      SELECT 'Kamiq'    UNION ALL
                      SELECT 'Karoq'    UNION ALL
                      SELECT 'Kodiaq'   UNION ALL
                      SELECT 'Enyaq'    UNION ALL
                      SELECT 'Rapid'    UNION ALL
                      SELECT 'Roomster' UNION ALL
                      SELECT 'Citigo'
                     ) m ON b.name = 'Škoda';
                """);

            // Dacia
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Sandero'    AS name UNION ALL
                      SELECT 'Logan'      UNION ALL
                      SELECT 'Duster'     UNION ALL
                      SELECT 'Jogger'     UNION ALL
                      SELECT 'Spring'     UNION ALL
                      SELECT 'Dokker'     UNION ALL
                      SELECT 'Lodgy'      UNION ALL
                      SELECT 'Pick-Up'
                     ) m ON b.name = 'Dacia';
                """);

            // Mini
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Cooper'           AS name UNION ALL
                      SELECT 'Cooper S'          UNION ALL
                      SELECT 'Cooper SE'         UNION ALL
                      SELECT 'Clubman'           UNION ALL
                      SELECT 'Countryman'        UNION ALL
                      SELECT 'Paceman'           UNION ALL
                      SELECT 'Convertible'       UNION ALL
                      SELECT 'John Cooper Works'
                     ) m ON b.name = 'Mini';
                """);

            // Tesla
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Model S'   AS name UNION ALL
                      SELECT 'Model 3'   UNION ALL
                      SELECT 'Model X'   UNION ALL
                      SELECT 'Model Y'   UNION ALL
                      SELECT 'Cybertruck' UNION ALL
                      SELECT 'Roadster'  UNION ALL
                      SELECT 'Semi'
                     ) m ON b.name = 'Tesla';
                """);

            // BYD
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Atto 3'  AS name UNION ALL
                      SELECT 'Han'     UNION ALL
                      SELECT 'Tang'    UNION ALL
                      SELECT 'Song Plus' UNION ALL
                      SELECT 'Dolphin' UNION ALL
                      SELECT 'Seal'    UNION ALL
                      SELECT 'Seagull' UNION ALL
                      SELECT 'Destroyer 05' UNION ALL
                      SELECT 'Shark'
                     ) m ON b.name = 'BYD';
                """);

            // Haval / Great Wall
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'H1'   AS name UNION ALL
                      SELECT 'H2'   UNION ALL
                      SELECT 'H4'   UNION ALL
                      SELECT 'H6'   UNION ALL
                      SELECT 'H9'   UNION ALL
                      SELECT 'Jolion' UNION ALL
                      SELECT 'Dargo'
                     ) m ON b.name = 'Haval';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Poer'  AS name UNION ALL
                      SELECT 'Steed' UNION ALL
                      SELECT 'P-Series'
                     ) m ON b.name = 'Great Wall';
                """);

            // MG
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'MG3'    AS name UNION ALL
                      SELECT 'MG5'    UNION ALL
                      SELECT 'MG6'    UNION ALL
                      SELECT 'ZS'     UNION ALL
                      SELECT 'ZS EV'  UNION ALL
                      SELECT 'HS'     UNION ALL
                      SELECT 'RX5'    UNION ALL
                      SELECT 'Marvel R' UNION ALL
                      SELECT '4'      UNION ALL
                      SELECT 'Cyberster'
                     ) m ON b.name = 'MG';
                """);

            // Chery / Changan / Geely
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Tiggo 2'   AS name UNION ALL
                      SELECT 'Tiggo 4'   UNION ALL
                      SELECT 'Tiggo 7'   UNION ALL
                      SELECT 'Tiggo 8'   UNION ALL
                      SELECT 'Arrizo 5'  UNION ALL
                      SELECT 'Arrizo 6'  UNION ALL
                      SELECT 'QQ'
                     ) m ON b.name = 'Chery';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'CS35 Plus' AS name UNION ALL
                      SELECT 'CS55 Plus' UNION ALL
                      SELECT 'CS75 Plus' UNION ALL
                      SELECT 'UNI-T'     UNION ALL
                      SELECT 'UNI-K'     UNION ALL
                      SELECT 'Hunter'    UNION ALL
                      SELECT 'Alsvin'
                     ) m ON b.name = 'Changan';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Coolray'   AS name UNION ALL
                      SELECT 'Azkarra'   UNION ALL
                      SELECT 'Okavango'  UNION ALL
                      SELECT 'Boyue'     UNION ALL
                      SELECT 'Emgrand'   UNION ALL
                      SELECT 'Atlas'
                     ) m ON b.name = 'Geely';
                """);

            // JAC / Foton / FAW
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'T6'      AS name UNION ALL
                      SELECT 'T8'      UNION ALL
                      SELECT 'J7'      UNION ALL
                      SELECT 'S2'      UNION ALL
                      SELECT 'S3'      UNION ALL
                      SELECT 'S4'      UNION ALL
                      SELECT 'X200'    UNION ALL
                      SELECT 'iEV7S'
                     ) m ON b.name = 'JAC';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Aumark'   AS name UNION ALL
                      SELECT 'Tunland'  UNION ALL
                      SELECT 'View'     UNION ALL
                      SELECT 'Toano'
                     ) m ON b.name = 'Foton';
                """);

            // Tata / Mahindra
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Nexon'     AS name UNION ALL
                      SELECT 'Harrier'   UNION ALL
                      SELECT 'Safari'    UNION ALL
                      SELECT 'Punch'     UNION ALL
                      SELECT 'Tiago'     UNION ALL
                      SELECT 'Tigor'     UNION ALL
                      SELECT 'Altroz'    UNION ALL
                      SELECT 'Xenon'     UNION ALL
                      SELECT 'Hexa'      UNION ALL
                      SELECT 'Sumo'      UNION ALL
                      SELECT 'Indica'
                     ) m ON b.name = 'Tata';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Scorpio'   AS name UNION ALL
                      SELECT 'XUV300'    UNION ALL
                      SELECT 'XUV400'    UNION ALL
                      SELECT 'XUV700'    UNION ALL
                      SELECT 'Thar'      UNION ALL
                      SELECT 'Bolero'    UNION ALL
                      SELECT 'Marazzo'   UNION ALL
                      SELECT 'Alturas'
                     ) m ON b.name = 'Mahindra';
                """);

            // Lexus
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'IS'    AS name UNION ALL
                      SELECT 'ES'    UNION ALL
                      SELECT 'GS'    UNION ALL
                      SELECT 'LS'    UNION ALL
                      SELECT 'LC'    UNION ALL
                      SELECT 'RC'    UNION ALL
                      SELECT 'UX'    UNION ALL
                      SELECT 'NX'    UNION ALL
                      SELECT 'RX'    UNION ALL
                      SELECT 'GX'    UNION ALL
                      SELECT 'LX'    UNION ALL
                      SELECT 'RZ'
                     ) m ON b.name = 'Lexus';
                """);

            // Infiniti
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Q30'  AS name UNION ALL
                      SELECT 'Q50'  UNION ALL
                      SELECT 'Q60'  UNION ALL
                      SELECT 'QX30' UNION ALL
                      SELECT 'QX50' UNION ALL
                      SELECT 'QX55' UNION ALL
                      SELECT 'QX60' UNION ALL
                      SELECT 'QX80'
                     ) m ON b.name = 'Infiniti';
                """);

            // Cadillac
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'CT4'       AS name UNION ALL
                      SELECT 'CT5'       UNION ALL
                      SELECT 'XT4'       UNION ALL
                      SELECT 'XT5'       UNION ALL
                      SELECT 'XT6'       UNION ALL
                      SELECT 'Escalade'  UNION ALL
                      SELECT 'Lyriq'     UNION ALL
                      SELECT 'CTS'       UNION ALL
                      SELECT 'ATS'       UNION ALL
                      SELECT 'SRX'
                     ) m ON b.name = 'Cadillac';
                """);

            // GMC
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Sierra'    AS name UNION ALL
                      SELECT 'Canyon'    UNION ALL
                      SELECT 'Terrain'   UNION ALL
                      SELECT 'Equinox'   UNION ALL
                      SELECT 'Acadia'    UNION ALL
                      SELECT 'Yukon'     UNION ALL
                      SELECT 'Suburban'  UNION ALL
                      SELECT 'Hummer EV'
                     ) m ON b.name = 'GMC';
                """);

            // Lada
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Niva'      AS name UNION ALL
                      SELECT 'Niva Travel' UNION ALL
                      SELECT 'Vesta'     UNION ALL
                      SELECT 'Granta'    UNION ALL
                      SELECT 'Largus'    UNION ALL
                      SELECT 'XRay'
                     ) m ON b.name = 'Lada';
                """);

            // SsangYong
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Rexton'    AS name UNION ALL
                      SELECT 'Tivoli'    UNION ALL
                      SELECT 'Korando'   UNION ALL
                      SELECT 'Actyon'    UNION ALL
                      SELECT 'Rodius'    UNION ALL
                      SELECT 'Musso'
                     ) m ON b.name = 'SsangYong';
                """);

            // Polestar
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Polestar 2' AS name UNION ALL
                      SELECT 'Polestar 3' UNION ALL
                      SELECT 'Polestar 4'
                     ) m ON b.name = 'Polestar';
                """);

            // Trucks (Commercial vehicles models)
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Actros'    AS name UNION ALL
                      SELECT 'Arocs'     UNION ALL
                      SELECT 'Atego'     UNION ALL
                      SELECT 'Axor'      UNION ALL
                      SELECT 'Unimog'
                     ) m ON b.name = 'Mercedes-Benz Trucks';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'FH'    AS name UNION ALL
                      SELECT 'FM'    UNION ALL
                      SELECT 'FMX'   UNION ALL
                      SELECT 'FL'    UNION ALL
                      SELECT 'FE'
                     ) m ON b.name = 'Volvo Trucks';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'TGX'   AS name UNION ALL
                      SELECT 'TGS'   UNION ALL
                      SELECT 'TGM'   UNION ALL
                      SELECT 'TGL'
                     ) m ON b.name = 'MAN';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'R-Series'  AS name UNION ALL
                      SELECT 'S-Series'  UNION ALL
                      SELECT 'P-Series'  UNION ALL
                      SELECT 'G-Series'
                     ) m ON b.name = 'Scania';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'XF'     AS name UNION ALL
                      SELECT 'XG'     UNION ALL
                      SELECT 'CF'     UNION ALL
                      SELECT 'LF'
                     ) m ON b.name = 'DAF';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Stralis'  AS name UNION ALL
                      SELECT 'Trakker'  UNION ALL
                      SELECT 'Eurocargo' UNION ALL
                      SELECT 'Daily'
                     ) m ON b.name = 'Iveco';
                """);

            // FUSO / Hino / UD
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Canter'   AS name UNION ALL
                      SELECT 'Fighter'  UNION ALL
                      SELECT 'Super Great'
                     ) m ON b.name = 'FUSO';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT '300'   AS name UNION ALL
                      SELECT '500'   UNION ALL
                      SELECT '700'
                     ) m ON b.name = 'Hino';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Croner'   AS name UNION ALL
                      SELECT 'Quon'     UNION ALL
                      SELECT 'Condor'
                     ) m ON b.name = 'UD Trucks';
                """);

            // Motociclos
            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'YZF-R1'    AS name UNION ALL
                      SELECT 'YZF-R6'    UNION ALL
                      SELECT 'MT-07'     UNION ALL
                      SELECT 'MT-09'     UNION ALL
                      SELECT 'XSR700'    UNION ALL
                      SELECT 'Tracer 9'  UNION ALL
                      SELECT 'Ténéré 700' UNION ALL
                      SELECT 'TMAX'      UNION ALL
                      SELECT 'XMAX'      UNION ALL
                      SELECT 'FZ-S'
                     ) m ON b.name = 'Yamaha';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'CBR600RR'    AS name UNION ALL
                      SELECT 'CBR1000RR'   UNION ALL
                      SELECT 'CB500F'      UNION ALL
                      SELECT 'CB650R'      UNION ALL
                      SELECT 'CB1000R'     UNION ALL
                      SELECT 'Africa Twin'  UNION ALL
                      SELECT 'X-ADV'       UNION ALL
                      SELECT 'Forza 350'   UNION ALL
                      SELECT 'PCX'
                     ) m ON b.name = 'Honda Motos';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Ninja 400'   AS name UNION ALL
                      SELECT 'Ninja 650'   UNION ALL
                      SELECT 'Ninja ZX-10R' UNION ALL
                      SELECT 'Z650'        UNION ALL
                      SELECT 'Z900'        UNION ALL
                      SELECT 'Versys 650'  UNION ALL
                      SELECT 'W800'        UNION ALL
                      SELECT 'KLX300'
                     ) m ON b.name = 'Kawasaki';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Sportster S'  AS name UNION ALL
                      SELECT 'Iron 883'     UNION ALL
                      SELECT 'Fat Boy'      UNION ALL
                      SELECT 'Road King'    UNION ALL
                      SELECT 'Street Glide' UNION ALL
                      SELECT 'Pan America'  UNION ALL
                      SELECT 'Nightster'
                     ) m ON b.name = 'Harley-Davidson';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Duke 390'  AS name UNION ALL
                      SELECT 'Duke 790'  UNION ALL
                      SELECT 'RC 390'    UNION ALL
                      SELECT '890 Adventure' UNION ALL
                      SELECT '1290 Super Duke'
                     ) m ON b.name = 'KTM';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Panigale V4'  AS name UNION ALL
                      SELECT 'Monster'      UNION ALL
                      SELECT 'Multistrada'  UNION ALL
                      SELECT 'Scrambler'    UNION ALL
                      SELECT 'SuperSport'
                     ) m ON b.name = 'Ducati';
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO vehicle_models (brand_id, name)
                SELECT b.id, m.name FROM vehicle_brands b
                JOIN (SELECT 'Street Twin'   AS name UNION ALL
                      SELECT 'Bonneville T120' UNION ALL
                      SELECT 'Tiger 900'      UNION ALL
                      SELECT 'Speed Triple'   UNION ALL
                      SELECT 'Trident 660'
                     ) m ON b.name = 'Triumph';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove only the models added by this migration (safe: won't touch user-created data)
            migrationBuilder.Sql("""
                DELETE vm FROM vehicle_models vm
                INNER JOIN vehicle_brands vb ON vm.brand_id = vb.id
                WHERE vb.name IN (
                    'Toyota','Honda','Nissan','Mazda','Mitsubishi','Subaru','Suzuki','Isuzu','Daihatsu',
                    'Lexus','Infiniti','Acura','Hyundai','Kia','Genesis','SsangYong',
                    'BMW','Mercedes-Benz','Volkswagen','Audi','Porsche','Opel','Smart',
                    'Ford','Chevrolet','Dodge','Jeep','Chrysler','Cadillac','Buick','GMC','Lincoln','Tesla','RAM',
                    'Land Rover','Range Rover','Jaguar','Bentley','Rolls-Royce','Aston Martin','Mini',
                    'Fiat','Alfa Romeo','Ferrari','Lamborghini','Maserati','Lancia',
                    'Renault','Peugeot','Citroën','DS Automobiles',
                    'Volvo','Saab','SEAT','Cupra','Škoda','Dacia','Lada',
                    'BYD','Great Wall','Chery','Geely','Haval','MG','JAC','Foton','FAW','BAIC','Dongfeng','Changan',
                    'Lynk & Co','Polestar',
                    'Tata','Mahindra',
                    'Mercedes-Benz Trucks','Volvo Trucks','MAN','Scania','DAF','Iveco','FUSO','Hino','UD Trucks','Renault Trucks',
                    'Yamaha','Honda Motos','Kawasaki','Suzuki Motos','Harley-Davidson','KTM','BMW Motorrad','Ducati','Triumph','Royal Enfield'
                );
                """);

            migrationBuilder.Sql("""
                DELETE FROM vehicle_brands WHERE name IN (
                    'Toyota','Honda','Nissan','Mazda','Mitsubishi','Subaru','Suzuki','Isuzu','Daihatsu',
                    'Lexus','Infiniti','Acura','Hyundai','Kia','Genesis','SsangYong',
                    'BMW','Mercedes-Benz','Volkswagen','Audi','Porsche','Opel','Smart',
                    'Ford','Chevrolet','Dodge','Jeep','Chrysler','Cadillac','Buick','GMC','Lincoln','Tesla','RAM',
                    'Land Rover','Range Rover','Jaguar','Bentley','Rolls-Royce','Aston Martin','Mini',
                    'Fiat','Alfa Romeo','Ferrari','Lamborghini','Maserati','Lancia',
                    'Renault','Peugeot','Citroën','DS Automobiles',
                    'Volvo','Saab','SEAT','Cupra','Škoda','Dacia','Lada',
                    'BYD','Great Wall','Chery','Geely','Haval','MG','JAC','Foton','FAW','BAIC','Dongfeng','Changan',
                    'Lynk & Co','Polestar',
                    'Tata','Mahindra',
                    'Mercedes-Benz Trucks','Volvo Trucks','MAN','Scania','DAF','Iveco','FUSO','Hino','UD Trucks','Renault Trucks',
                    'Yamaha','Honda Motos','Kawasaki','Suzuki Motos','Harley-Davidson','KTM','BMW Motorrad','Ducati','Triumph','Royal Enfield',
                    'Alfa'
                );
                """);
        }
    }
}

