using SafetySentinel.Models;
using System;
using System.Collections.Generic;

namespace SafetySentinel.Data
{
    public static class SeedData
    {
        public static List<CountryProfile> GetCountries()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new List<CountryProfile>
            {
                // ===== AFRICA =====
                new() { CountryCode="ZA", CountryName="South Africa", Latitude=-30.56, Longitude=22.94, OverallSafetyScore=72, PhysicalSecurity=85, PoliticalStability=62, EconomicFreedom=58, DigitalSovereignty=45, HealthEnvironment=55, SocialCohesion=70, MobilityExit=35, Infrastructure=65, CbdcStatus="research", CashFreedomScore=30, SurveillanceScore=50, GenocideStage=6, InternetFreedomScore=35, UpdatedAt=now },
                new() { CountryCode="NG", CountryName="Nigeria", Latitude=9.08, Longitude=8.68, OverallSafetyScore=75, PhysicalSecurity=80, PoliticalStability=65, EconomicFreedom=70, DigitalSovereignty=60, HealthEnvironment=70, SocialCohesion=72, MobilityExit=55, Infrastructure=75, CbdcStatus="active", CashFreedomScore=45, SurveillanceScore=55, GenocideStage=5, InternetFreedomScore=50, UpdatedAt=now },
                new() { CountryCode="KE", CountryName="Kenya", Latitude=-0.02, Longitude=37.91, OverallSafetyScore=55, PhysicalSecurity=60, PoliticalStability=50, EconomicFreedom=48, DigitalSovereignty=45, HealthEnvironment=55, SocialCohesion=52, MobilityExit=40, Infrastructure=58, CbdcStatus="research", CashFreedomScore=35, SurveillanceScore=50, GenocideStage=4, InternetFreedomScore=40, UpdatedAt=now },
                new() { CountryCode="EG", CountryName="Egypt", Latitude=26.82, Longitude=30.80, OverallSafetyScore=58, PhysicalSecurity=50, PoliticalStability=65, EconomicFreedom=62, DigitalSovereignty=65, HealthEnvironment=48, SocialCohesion=55, MobilityExit=50, Infrastructure=52, CbdcStatus="pilot", CashFreedomScore=40, SurveillanceScore=70, GenocideStage=3, InternetFreedomScore=60, UpdatedAt=now },
                new() { CountryCode="ET", CountryName="Ethiopia", Latitude=9.15, Longitude=40.49, OverallSafetyScore=70, PhysicalSecurity=75, PoliticalStability=72, EconomicFreedom=65, DigitalSovereignty=55, HealthEnvironment=68, SocialCohesion=75, MobilityExit=60, Infrastructure=70, CbdcStatus="none", CashFreedomScore=25, SurveillanceScore=60, GenocideStage=7, InternetFreedomScore=65, UpdatedAt=now },
                new() { CountryCode="GH", CountryName="Ghana", Latitude=7.95, Longitude=-1.02, OverallSafetyScore=35, PhysicalSecurity=35, PoliticalStability=25, EconomicFreedom=40, DigitalSovereignty=30, HealthEnvironment=42, SocialCohesion=30, MobilityExit=38, Infrastructure=45, CbdcStatus="pilot", CashFreedomScore=35, SurveillanceScore=30, GenocideStage=2, InternetFreedomScore=25, UpdatedAt=now },
                new() { CountryCode="TZ", CountryName="Tanzania", Latitude=-6.37, Longitude=34.89, OverallSafetyScore=42, PhysicalSecurity=40, PoliticalStability=45, EconomicFreedom=50, DigitalSovereignty=40, HealthEnvironment=50, SocialCohesion=38, MobilityExit=42, Infrastructure=52, CbdcStatus="none", CashFreedomScore=20, SurveillanceScore=45, GenocideStage=3, InternetFreedomScore=50, UpdatedAt=now },
                new() { CountryCode="RW", CountryName="Rwanda", Latitude=-1.94, Longitude=29.87, OverallSafetyScore=40, PhysicalSecurity=25, PoliticalStability=60, EconomicFreedom=35, DigitalSovereignty=55, HealthEnvironment=35, SocialCohesion=30, MobilityExit=45, Infrastructure=35, CbdcStatus="none", CashFreedomScore=30, SurveillanceScore=65, GenocideStage=4, InternetFreedomScore=55, UpdatedAt=now },
                new() { CountryCode="MZ", CountryName="Mozambique", Latitude=-18.67, Longitude=35.53, OverallSafetyScore=62, PhysicalSecurity=70, PoliticalStability=58, EconomicFreedom=60, DigitalSovereignty=35, HealthEnvironment=65, SocialCohesion=55, MobilityExit=50, Infrastructure=68, CbdcStatus="none", CashFreedomScore=20, SurveillanceScore=35, GenocideStage=4, InternetFreedomScore=35, UpdatedAt=now },
                new() { CountryCode="ZW", CountryName="Zimbabwe", Latitude=-19.02, Longitude=29.15, OverallSafetyScore=68, PhysicalSecurity=60, PoliticalStability=75, EconomicFreedom=80, DigitalSovereignty=55, HealthEnvironment=70, SocialCohesion=62, MobilityExit=65, Infrastructure=72, CbdcStatus="none", CashFreedomScore=50, SurveillanceScore=60, GenocideStage=5, InternetFreedomScore=55, UpdatedAt=now },
                new() { CountryCode="BW", CountryName="Botswana", Latitude=-22.33, Longitude=24.68, OverallSafetyScore=22, PhysicalSecurity=25, PoliticalStability=15, EconomicFreedom=20, DigitalSovereignty=18, HealthEnvironment=30, SocialCohesion=20, MobilityExit=25, Infrastructure=28, CbdcStatus="none", CashFreedomScore=15, SurveillanceScore=20, GenocideStage=1, InternetFreedomScore=20, UpdatedAt=now },
                new() { CountryCode="NA", CountryName="Namibia", Latitude=-22.96, Longitude=18.49, OverallSafetyScore=28, PhysicalSecurity=30, PoliticalStability=20, EconomicFreedom=28, DigitalSovereignty=22, HealthEnvironment=32, SocialCohesion=28, MobilityExit=30, Infrastructure=35, CbdcStatus="none", CashFreedomScore=18, SurveillanceScore=22, GenocideStage=2, InternetFreedomScore=22, UpdatedAt=now },
                new() { CountryCode="AO", CountryName="Angola", Latitude=-11.20, Longitude=17.87, OverallSafetyScore=58, PhysicalSecurity=55, PoliticalStability=62, EconomicFreedom=65, DigitalSovereignty=40, HealthEnvironment=60, SocialCohesion=50, MobilityExit=52, Infrastructure=62, CbdcStatus="none", CashFreedomScore=30, SurveillanceScore=45, GenocideStage=3, InternetFreedomScore=45, UpdatedAt=now },
                new() { CountryCode="CD", CountryName="DR Congo", Latitude=-4.04, Longitude=21.76, OverallSafetyScore=82, PhysicalSecurity=88, PoliticalStability=80, EconomicFreedom=75, DigitalSovereignty=50, HealthEnvironment=82, SocialCohesion=78, MobilityExit=70, Infrastructure=85, CbdcStatus="none", CashFreedomScore=20, SurveillanceScore=35, GenocideStage=8, InternetFreedomScore=55, UpdatedAt=now },

                // ===== ASIA =====
                new() { CountryCode="CN", CountryName="China", Latitude=35.86, Longitude=104.20, OverallSafetyScore=55, PhysicalSecurity=20, PoliticalStability=70, EconomicFreedom=65, DigitalSovereignty=90, HealthEnvironment=35, SocialCohesion=45, MobilityExit=60, Infrastructure=25, CbdcStatus="active", CashFreedomScore=80, SurveillanceScore=95, GenocideStage=6, InternetFreedomScore=88, UpdatedAt=now },
                new() { CountryCode="IN", CountryName="India", Latitude=20.59, Longitude=78.96, OverallSafetyScore=52, PhysicalSecurity=55, PoliticalStability=45, EconomicFreedom=48, DigitalSovereignty=60, HealthEnvironment=55, SocialCohesion=58, MobilityExit=42, Infrastructure=50, CbdcStatus="pilot", CashFreedomScore=55, SurveillanceScore=65, GenocideStage=5, InternetFreedomScore=55, UpdatedAt=now },
                new() { CountryCode="JP", CountryName="Japan", Latitude=36.20, Longitude=138.25, OverallSafetyScore=12, PhysicalSecurity=8, PoliticalStability=15, EconomicFreedom=18, DigitalSovereignty=25, HealthEnvironment=10, SocialCohesion=12, MobilityExit=15, Infrastructure=8, CbdcStatus="pilot", CashFreedomScore=20, SurveillanceScore=30, GenocideStage=1, InternetFreedomScore=18, UpdatedAt=now },
                new() { CountryCode="SG", CountryName="Singapore", Latitude=1.35, Longitude=103.82, OverallSafetyScore=15, PhysicalSecurity=5, PoliticalStability=20, EconomicFreedom=12, DigitalSovereignty=40, HealthEnvironment=8, SocialCohesion=15, MobilityExit=10, Infrastructure=5, CbdcStatus="pilot", CashFreedomScore=45, SurveillanceScore=60, GenocideStage=1, InternetFreedomScore=35, UpdatedAt=now },
                new() { CountryCode="KR", CountryName="South Korea", Latitude=35.91, Longitude=127.77, OverallSafetyScore=18, PhysicalSecurity=12, PoliticalStability=22, EconomicFreedom=15, DigitalSovereignty=28, HealthEnvironment=12, SocialCohesion=18, MobilityExit=15, Infrastructure=10, CbdcStatus="pilot", CashFreedomScore=35, SurveillanceScore=40, GenocideStage=1, InternetFreedomScore=28, UpdatedAt=now },
                new() { CountryCode="TH", CountryName="Thailand", Latitude=15.87, Longitude=100.99, OverallSafetyScore=38, PhysicalSecurity=35, PoliticalStability=50, EconomicFreedom=35, DigitalSovereignty=42, HealthEnvironment=30, SocialCohesion=35, MobilityExit=28, Infrastructure=32, CbdcStatus="research", CashFreedomScore=25, SurveillanceScore=45, GenocideStage=2, InternetFreedomScore=42, UpdatedAt=now },
                new() { CountryCode="PH", CountryName="Philippines", Latitude=12.88, Longitude=121.77, OverallSafetyScore=48, PhysicalSecurity=55, PoliticalStability=45, EconomicFreedom=42, DigitalSovereignty=35, HealthEnvironment=48, SocialCohesion=42, MobilityExit=35, Infrastructure=50, CbdcStatus="research", CashFreedomScore=25, SurveillanceScore=40, GenocideStage=3, InternetFreedomScore=35, UpdatedAt=now },
                new() { CountryCode="ID", CountryName="Indonesia", Latitude=-0.79, Longitude=113.92, OverallSafetyScore=42, PhysicalSecurity=40, PoliticalStability=38, EconomicFreedom=42, DigitalSovereignty=38, HealthEnvironment=45, SocialCohesion=48, MobilityExit=35, Infrastructure=45, CbdcStatus="research", CashFreedomScore=25, SurveillanceScore=40, GenocideStage=3, InternetFreedomScore=42, UpdatedAt=now },
                new() { CountryCode="PK", CountryName="Pakistan", Latitude=30.38, Longitude=69.35, OverallSafetyScore=68, PhysicalSecurity=75, PoliticalStability=70, EconomicFreedom=62, DigitalSovereignty=55, HealthEnvironment=65, SocialCohesion=68, MobilityExit=58, Infrastructure=65, CbdcStatus="research", CashFreedomScore=30, SurveillanceScore=55, GenocideStage=5, InternetFreedomScore=60, UpdatedAt=now },
                new() { CountryCode="IL", CountryName="Israel", Latitude=31.05, Longitude=34.85, OverallSafetyScore=45, PhysicalSecurity=55, PoliticalStability=42, EconomicFreedom=25, DigitalSovereignty=50, HealthEnvironment=18, SocialCohesion=48, MobilityExit=20, Infrastructure=15, CbdcStatus="pilot", CashFreedomScore=40, SurveillanceScore=70, GenocideStage=4, InternetFreedomScore=30, UpdatedAt=now },
                new() { CountryCode="AE", CountryName="UAE", Latitude=23.42, Longitude=53.85, OverallSafetyScore=18, PhysicalSecurity=8, PoliticalStability=25, EconomicFreedom=15, DigitalSovereignty=55, HealthEnvironment=12, SocialCohesion=20, MobilityExit=15, Infrastructure=8, CbdcStatus="pilot", CashFreedomScore=45, SurveillanceScore=75, GenocideStage=2, InternetFreedomScore=50, UpdatedAt=now },
                new() { CountryCode="SA", CountryName="Saudi Arabia", Latitude=23.89, Longitude=45.08, OverallSafetyScore=30, PhysicalSecurity=18, PoliticalStability=42, EconomicFreedom=28, DigitalSovereignty=58, HealthEnvironment=20, SocialCohesion=35, MobilityExit=25, Infrastructure=15, CbdcStatus="research", CashFreedomScore=40, SurveillanceScore=78, GenocideStage=2, InternetFreedomScore=62, UpdatedAt=now },
                new() { CountryCode="AF", CountryName="Afghanistan", Latitude=33.94, Longitude=67.71, OverallSafetyScore=92, PhysicalSecurity=95, PoliticalStability=90, EconomicFreedom=85, DigitalSovereignty=60, HealthEnvironment=88, SocialCohesion=85, MobilityExit=90, Infrastructure=92, CbdcStatus="none", CashFreedomScore=15, SurveillanceScore=30, GenocideStage=8, InternetFreedomScore=65, UpdatedAt=now },
                new() { CountryCode="IQ", CountryName="Iraq", Latitude=33.22, Longitude=43.68, OverallSafetyScore=72, PhysicalSecurity=78, PoliticalStability=70, EconomicFreedom=65, DigitalSovereignty=50, HealthEnvironment=68, SocialCohesion=72, MobilityExit=62, Infrastructure=70, CbdcStatus="none", CashFreedomScore=22, SurveillanceScore=50, GenocideStage=6, InternetFreedomScore=55, UpdatedAt=now },
                new() { CountryCode="MM", CountryName="Myanmar", Latitude=21.91, Longitude=95.96, OverallSafetyScore=85, PhysicalSecurity=88, PoliticalStability=85, EconomicFreedom=78, DigitalSovereignty=65, HealthEnvironment=80, SocialCohesion=82, MobilityExit=80, Infrastructure=82, CbdcStatus="none", CashFreedomScore=20, SurveillanceScore=55, GenocideStage=9, InternetFreedomScore=70, UpdatedAt=now },

                // ===== EUROPE =====
                new() { CountryCode="GB", CountryName="United Kingdom", Latitude=55.38, Longitude=-3.44, OverallSafetyScore=22, PhysicalSecurity=25, PoliticalStability=20, EconomicFreedom=18, DigitalSovereignty=35, HealthEnvironment=15, SocialCohesion=28, MobilityExit=12, Infrastructure=15, CbdcStatus="research", CashFreedomScore=40, SurveillanceScore=65, GenocideStage=1, InternetFreedomScore=25, UpdatedAt=now },
                new() { CountryCode="DE", CountryName="Germany", Latitude=51.17, Longitude=10.45, OverallSafetyScore=18, PhysicalSecurity=18, PoliticalStability=15, EconomicFreedom=15, DigitalSovereignty=28, HealthEnvironment=12, SocialCohesion=22, MobilityExit=10, Infrastructure=10, CbdcStatus="research", CashFreedomScore=22, SurveillanceScore=35, GenocideStage=1, InternetFreedomScore=18, UpdatedAt=now },
                new() { CountryCode="FR", CountryName="France", Latitude=46.23, Longitude=2.21, OverallSafetyScore=25, PhysicalSecurity=28, PoliticalStability=22, EconomicFreedom=22, DigitalSovereignty=30, HealthEnvironment=15, SocialCohesion=32, MobilityExit=12, Infrastructure=12, CbdcStatus="research", CashFreedomScore=30, SurveillanceScore=42, GenocideStage=1, InternetFreedomScore=22, UpdatedAt=now },
                new() { CountryCode="PT", CountryName="Portugal", Latitude=39.40, Longitude=-8.22, OverallSafetyScore=12, PhysicalSecurity=10, PoliticalStability=12, EconomicFreedom=15, DigitalSovereignty=18, HealthEnvironment=10, SocialCohesion=12, MobilityExit=8, Infrastructure=15, CbdcStatus="research", CashFreedomScore=20, SurveillanceScore=25, GenocideStage=1, InternetFreedomScore=15, UpdatedAt=now },
                new() { CountryCode="CH", CountryName="Switzerland", Latitude=46.82, Longitude=8.23, OverallSafetyScore=8, PhysicalSecurity=5, PoliticalStability=5, EconomicFreedom=5, DigitalSovereignty=12, HealthEnvironment=5, SocialCohesion=8, MobilityExit=5, Infrastructure=5, CbdcStatus="research", CashFreedomScore=10, SurveillanceScore=18, GenocideStage=1, InternetFreedomScore=10, UpdatedAt=now },
                new() { CountryCode="SE", CountryName="Sweden", Latitude=60.13, Longitude=18.64, OverallSafetyScore=18, PhysicalSecurity=20, PoliticalStability=12, EconomicFreedom=10, DigitalSovereignty=22, HealthEnvironment=8, SocialCohesion=22, MobilityExit=8, Infrastructure=8, CbdcStatus="pilot", CashFreedomScore=60, SurveillanceScore=30, GenocideStage=1, InternetFreedomScore=12, UpdatedAt=now },
                new() { CountryCode="NL", CountryName="Netherlands", Latitude=52.13, Longitude=5.29, OverallSafetyScore=15, PhysicalSecurity=15, PoliticalStability=12, EconomicFreedom=12, DigitalSovereignty=22, HealthEnvironment=10, SocialCohesion=18, MobilityExit=8, Infrastructure=8, CbdcStatus="research", CashFreedomScore=25, SurveillanceScore=32, GenocideStage=1, InternetFreedomScore=15, UpdatedAt=now },
                new() { CountryCode="PL", CountryName="Poland", Latitude=51.92, Longitude=19.15, OverallSafetyScore=18, PhysicalSecurity=15, PoliticalStability=22, EconomicFreedom=15, DigitalSovereignty=20, HealthEnvironment=15, SocialCohesion=20, MobilityExit=12, Infrastructure=18, CbdcStatus="research", CashFreedomScore=18, SurveillanceScore=28, GenocideStage=1, InternetFreedomScore=18, UpdatedAt=now },
                new() { CountryCode="RU", CountryName="Russia", Latitude=61.52, Longitude=105.32, OverallSafetyScore=65, PhysicalSecurity=55, PoliticalStability=72, EconomicFreedom=68, DigitalSovereignty=78, HealthEnvironment=50, SocialCohesion=58, MobilityExit=70, Infrastructure=52, CbdcStatus="pilot", CashFreedomScore=55, SurveillanceScore=82, GenocideStage=4, InternetFreedomScore=75, UpdatedAt=now },
                new() { CountryCode="UA", CountryName="Ukraine", Latitude=48.38, Longitude=31.17, OverallSafetyScore=82, PhysicalSecurity=92, PoliticalStability=72, EconomicFreedom=68, DigitalSovereignty=50, HealthEnvironment=75, SocialCohesion=62, MobilityExit=78, Infrastructure=85, CbdcStatus="research", CashFreedomScore=30, SurveillanceScore=45, GenocideStage=5, InternetFreedomScore=35, UpdatedAt=now },
                new() { CountryCode="TR", CountryName="Turkey", Latitude=38.96, Longitude=35.24, OverallSafetyScore=48, PhysicalSecurity=42, PoliticalStability=55, EconomicFreedom=52, DigitalSovereignty=50, HealthEnvironment=35, SocialCohesion=48, MobilityExit=38, Infrastructure=35, CbdcStatus="research", CashFreedomScore=35, SurveillanceScore=58, GenocideStage=3, InternetFreedomScore=55, UpdatedAt=now },
                new() { CountryCode="HU", CountryName="Hungary", Latitude=47.16, Longitude=19.50, OverallSafetyScore=22, PhysicalSecurity=18, PoliticalStability=30, EconomicFreedom=20, DigitalSovereignty=22, HealthEnvironment=18, SocialCohesion=25, MobilityExit=15, Infrastructure=18, CbdcStatus="research", CashFreedomScore=18, SurveillanceScore=30, GenocideStage=1, InternetFreedomScore=22, UpdatedAt=now },
                new() { CountryCode="ES", CountryName="Spain", Latitude=40.46, Longitude=-3.75, OverallSafetyScore=15, PhysicalSecurity=15, PoliticalStability=15, EconomicFreedom=18, DigitalSovereignty=20, HealthEnvironment=10, SocialCohesion=18, MobilityExit=10, Infrastructure=12, CbdcStatus="research", CashFreedomScore=22, SurveillanceScore=28, GenocideStage=1, InternetFreedomScore=15, UpdatedAt=now },
                new() { CountryCode="IT", CountryName="Italy", Latitude=41.87, Longitude=12.57, OverallSafetyScore=20, PhysicalSecurity=22, PoliticalStability=18, EconomicFreedom=20, DigitalSovereignty=22, HealthEnvironment=12, SocialCohesion=22, MobilityExit=12, Infrastructure=15, CbdcStatus="research", CashFreedomScore=28, SurveillanceScore=30, GenocideStage=1, InternetFreedomScore=18, UpdatedAt=now },

                // ===== NORTH AMERICA =====
                new() { CountryCode="US", CountryName="United States", Latitude=37.09, Longitude=-95.71, OverallSafetyScore=25, PhysicalSecurity=32, PoliticalStability=28, EconomicFreedom=18, DigitalSovereignty=30, HealthEnvironment=15, SocialCohesion=30, MobilityExit=8, Infrastructure=12, CbdcStatus="research", CashFreedomScore=25, SurveillanceScore=55, GenocideStage=2, InternetFreedomScore=20, UpdatedAt=now },
                new() { CountryCode="CA", CountryName="Canada", Latitude=56.13, Longitude=-106.35, OverallSafetyScore=15, PhysicalSecurity=12, PoliticalStability=15, EconomicFreedom=12, DigitalSovereignty=22, HealthEnvironment=10, SocialCohesion=15, MobilityExit=8, Infrastructure=10, CbdcStatus="research", CashFreedomScore=30, SurveillanceScore=38, GenocideStage=1, InternetFreedomScore=18, UpdatedAt=now },
                new() { CountryCode="MX", CountryName="Mexico", Latitude=23.63, Longitude=-102.55, OverallSafetyScore=62, PhysicalSecurity=72, PoliticalStability=55, EconomicFreedom=48, DigitalSovereignty=38, HealthEnvironment=42, SocialCohesion=55, MobilityExit=35, Infrastructure=48, CbdcStatus="research", CashFreedomScore=22, SurveillanceScore=42, GenocideStage=3, InternetFreedomScore=35, UpdatedAt=now },

                // ===== SOUTH AMERICA =====
                new() { CountryCode="BR", CountryName="Brazil", Latitude=-14.24, Longitude=-51.93, OverallSafetyScore=55, PhysicalSecurity=68, PoliticalStability=45, EconomicFreedom=48, DigitalSovereignty=38, HealthEnvironment=42, SocialCohesion=52, MobilityExit=32, Infrastructure=45, CbdcStatus="pilot", CashFreedomScore=35, SurveillanceScore=42, GenocideStage=3, InternetFreedomScore=30, UpdatedAt=now },
                new() { CountryCode="AR", CountryName="Argentina", Latitude=-38.42, Longitude=-63.62, OverallSafetyScore=45, PhysicalSecurity=42, PoliticalStability=42, EconomicFreedom=58, DigitalSovereignty=30, HealthEnvironment=35, SocialCohesion=40, MobilityExit=32, Infrastructure=42, CbdcStatus="none", CashFreedomScore=25, SurveillanceScore=28, GenocideStage=2, InternetFreedomScore=22, UpdatedAt=now },
                new() { CountryCode="CL", CountryName="Chile", Latitude=-35.68, Longitude=-71.54, OverallSafetyScore=25, PhysicalSecurity=28, PoliticalStability=22, EconomicFreedom=18, DigitalSovereignty=22, HealthEnvironment=18, SocialCohesion=28, MobilityExit=15, Infrastructure=18, CbdcStatus="research", CashFreedomScore=22, SurveillanceScore=28, GenocideStage=1, InternetFreedomScore=18, UpdatedAt=now },
                new() { CountryCode="CO", CountryName="Colombia", Latitude=4.57, Longitude=-74.30, OverallSafetyScore=52, PhysicalSecurity=62, PoliticalStability=45, EconomicFreedom=42, DigitalSovereignty=32, HealthEnvironment=40, SocialCohesion=48, MobilityExit=30, Infrastructure=42, CbdcStatus="none", CashFreedomScore=20, SurveillanceScore=35, GenocideStage=3, InternetFreedomScore=28, UpdatedAt=now },
                new() { CountryCode="VE", CountryName="Venezuela", Latitude=6.42, Longitude=-66.59, OverallSafetyScore=82, PhysicalSecurity=85, PoliticalStability=82, EconomicFreedom=88, DigitalSovereignty=60, HealthEnvironment=80, SocialCohesion=78, MobilityExit=75, Infrastructure=82, CbdcStatus="active", CashFreedomScore=55, SurveillanceScore=60, GenocideStage=5, InternetFreedomScore=65, UpdatedAt=now },
                new() { CountryCode="UY", CountryName="Uruguay", Latitude=-32.52, Longitude=-55.77, OverallSafetyScore=15, PhysicalSecurity=18, PoliticalStability=10, EconomicFreedom=12, DigitalSovereignty=15, HealthEnvironment=12, SocialCohesion=15, MobilityExit=12, Infrastructure=15, CbdcStatus="none", CashFreedomScore=15, SurveillanceScore=18, GenocideStage=1, InternetFreedomScore=12, UpdatedAt=now },
                new() { CountryCode="PE", CountryName="Peru", Latitude=-9.19, Longitude=-75.02, OverallSafetyScore=45, PhysicalSecurity=48, PoliticalStability=48, EconomicFreedom=38, DigitalSovereignty=28, HealthEnvironment=42, SocialCohesion=45, MobilityExit=32, Infrastructure=42, CbdcStatus="none", CashFreedomScore=18, SurveillanceScore=28, GenocideStage=2, InternetFreedomScore=25, UpdatedAt=now },

                // ===== OCEANIA =====
                new() { CountryCode="AU", CountryName="Australia", Latitude=-25.27, Longitude=133.78, OverallSafetyScore=15, PhysicalSecurity=12, PoliticalStability=12, EconomicFreedom=12, DigitalSovereignty=28, HealthEnvironment=10, SocialCohesion=15, MobilityExit=8, Infrastructure=8, CbdcStatus="research", CashFreedomScore=40, SurveillanceScore=52, GenocideStage=1, InternetFreedomScore=22, UpdatedAt=now },
                new() { CountryCode="NZ", CountryName="New Zealand", Latitude=-40.90, Longitude=174.89, OverallSafetyScore=10, PhysicalSecurity=8, PoliticalStability=8, EconomicFreedom=8, DigitalSovereignty=15, HealthEnvironment=8, SocialCohesion=10, MobilityExit=8, Infrastructure=8, CbdcStatus="research", CashFreedomScore=25, SurveillanceScore=28, GenocideStage=1, InternetFreedomScore=12, UpdatedAt=now },
            };
        }

        public static List<AlertCategory> GetAlertCategories()
        {
            return new List<AlertCategory>
            {
                // PHYSICAL SECURITY (9)
                new() { Domain="Physical", Category="Hijacking attempt nearby", Description="Hijacking attempt within 5km radius", Enabled=true, SeverityWeight=95, ScanFrequency="realtime" },
                new() { Domain="Physical", Category="Armed robbery trend", Description="Armed robbery trend increase in area", Enabled=true, SeverityWeight=90, ScanFrequency="daily" },
                new() { Domain="Physical", Category="Home invasion", Description="Home invasion reported in neighborhood", Enabled=true, SeverityWeight=92, ScanFrequency="realtime" },
                new() { Domain="Physical", Category="Farm attack", Description="Farm attack reported in region", Enabled=true, SeverityWeight=98, ScanFrequency="realtime" },
                new() { Domain="Physical", Category="Mass shooting / terrorism", Description="Mass shooting or terrorism event", Enabled=true, SeverityWeight=99, ScanFrequency="realtime" },
                new() { Domain="Physical", Category="Ethnic violence", Description="Ethnic or racial violence incident", Enabled=true, SeverityWeight=95, ScanFrequency="daily" },
                new() { Domain="Physical", Category="Police stand-down", Description="Police strike or stand-down event", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Physical", Category="Security company failure", Description="Private security company failure or collapse", Enabled=true, SeverityWeight=80, ScanFrequency="daily" },
                new() { Domain="Physical", Category="Road blockade / protest", Description="Road blockade or protest blocking routes", Enabled=true, SeverityWeight=75, ScanFrequency="realtime" },

                // POLITICAL (10)
                new() { Domain="Political", Category="Expropriation law change", Description="Land expropriation without compensation legislation", Enabled=true, SeverityWeight=95, ScanFrequency="daily" },
                new() { Domain="Political", Category="Genocide stage escalation", Description="Genocide Watch stage increase", Enabled=true, SeverityWeight=99, ScanFrequency="daily" },
                new() { Domain="Political", Category="Election violence risk", Description="Electoral violence risk window approaching", Enabled=true, SeverityWeight=85, ScanFrequency="weekly" },
                new() { Domain="Political", Category="Constitutional amendment", Description="Constitutional change targeting minorities", Enabled=true, SeverityWeight=90, ScanFrequency="daily" },
                new() { Domain="Political", Category="Hate speech escalation", Description="Political hate speech targeting specific demographics", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Political", Category="Judicial independence threat", Description="Judicial independence under threat", Enabled=true, SeverityWeight=82, ScanFrequency="weekly" },
                new() { Domain="Political", Category="Press freedom decline", Description="Press freedom restrictions or journalist targeting", Enabled=true, SeverityWeight=78, ScanFrequency="weekly" },
                new() { Domain="Political", Category="Corruption spike", Description="Major corruption scandal or governance breakdown", Enabled=true, SeverityWeight=75, ScanFrequency="daily" },
                new() { Domain="Political", Category="State of emergency", Description="State of emergency or martial law declared", Enabled=true, SeverityWeight=98, ScanFrequency="realtime" },
                new() { Domain="Political", Category="Opposition crackdown", Description="Political opposition crackdown or arrests", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },

                // ECONOMIC (10)
                new() { Domain="Economic", Category="CBDC mandate", Description="CBDC mandatory adoption announcement", Enabled=true, SeverityWeight=92, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Cash ban progress", Description="Cash elimination legislation advancement", Enabled=true, SeverityWeight=90, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Capital controls", Description="New capital controls or forex restrictions", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Currency crisis", Description="Currency devaluation or volatility spike", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Bank instability", Description="Banking system instability or bank run", Enabled=true, SeverityWeight=90, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Gold/crypto restriction", Description="Precious metals or cryptocurrency ownership restrictions", Enabled=true, SeverityWeight=82, ScanFrequency="weekly" },
                new() { Domain="Economic", Category="Property seizure risk", Description="Property rights erosion or seizure risk", Enabled=true, SeverityWeight=92, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Tax regime change", Description="Significant tax burden increase", Enabled=true, SeverityWeight=72, ScanFrequency="weekly" },
                new() { Domain="Economic", Category="Forex transfer block", Description="Foreign exchange transfer restrictions", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Economic", Category="Inflation spike", Description="Inflation rate spike above threshold", Enabled=true, SeverityWeight=78, ScanFrequency="daily" },

                // DIGITAL (10)
                new() { Domain="Digital", Category="Mass surveillance expansion", Description="Government surveillance capability expansion", Enabled=true, SeverityWeight=88, ScanFrequency="weekly" },
                new() { Domain="Digital", Category="Spyware deployment", Description="Spyware deployment (Pegasus, etc.) detected", Enabled=true, SeverityWeight=95, ScanFrequency="daily" },
                new() { Domain="Digital", Category="VPN ban/restriction", Description="VPN restriction or ban enacted", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Digital", Category="Digital ID mandate", Description="Mandatory digital ID requirement introduced", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Digital", Category="Social credit indicator", Description="Social credit system indicators detected", Enabled=true, SeverityWeight=92, ScanFrequency="weekly" },
                new() { Domain="Digital", Category="Encryption ban", Description="Encryption restriction or ban attempts", Enabled=true, SeverityWeight=90, ScanFrequency="weekly" },
                new() { Domain="Digital", Category="Biometric collection", Description="Expanded biometric data collection mandate", Enabled=true, SeverityWeight=85, ScanFrequency="weekly" },
                new() { Domain="Digital", Category="Internet censorship", Description="Internet censorship level increase", Enabled=true, SeverityWeight=82, ScanFrequency="daily" },
                new() { Domain="Digital", Category="AI integration mandate", Description="Mandatory AI integration in governance", Enabled=true, SeverityWeight=78, ScanFrequency="weekly" },
                new() { Domain="Digital", Category="Smart city surveillance", Description="Smart city surveillance infrastructure expansion", Enabled=true, SeverityWeight=80, ScanFrequency="weekly" },

                // HEALTH / ENVIRONMENT (10)
                new() { Domain="Health", Category="Disease outbreak", Description="Local disease outbreak detected", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Health", Category="Pandemic declaration", Description="Pandemic declared by WHO or national authority", Enabled=true, SeverityWeight=95, ScanFrequency="realtime" },
                new() { Domain="Health", Category="Water contamination", Description="Water supply contamination event", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Health", Category="Load shedding Stage 4+", Description="Extended load shedding at Stage 4 or above", Enabled=true, SeverityWeight=78, ScanFrequency="realtime" },
                new() { Domain="Health", Category="Food supply disruption", Description="Food supply chain disruption or shortage", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Health", Category="Air quality emergency", Description="Severe air quality emergency", Enabled=true, SeverityWeight=72, ScanFrequency="daily" },
                new() { Domain="Health", Category="Natural disaster warning", Description="Natural disaster warning issued", Enabled=true, SeverityWeight=90, ScanFrequency="realtime" },
                new() { Domain="Health", Category="Hospital system overload", Description="Hospital system capacity exceeded", Enabled=true, SeverityWeight=82, ScanFrequency="daily" },
                new() { Domain="Health", Category="Medication shortage", Description="Critical medication shortage", Enabled=true, SeverityWeight=78, ScanFrequency="weekly" },
                new() { Domain="Health", Category="Sewage system failure", Description="Sewage or sanitation system failure", Enabled=true, SeverityWeight=75, ScanFrequency="daily" },

                // SOCIAL (10)
                new() { Domain="Social", Category="Xenophobic attack", Description="Xenophobic violence incident", Enabled=true, SeverityWeight=92, ScanFrequency="realtime" },
                new() { Domain="Social", Category="Service delivery riot", Description="Service delivery protest turned violent", Enabled=true, SeverityWeight=82, ScanFrequency="realtime" },
                new() { Domain="Social", Category="General strike", Description="General strike or mass work stoppage", Enabled=true, SeverityWeight=78, ScanFrequency="daily" },
                new() { Domain="Social", Category="Ethnic tension escalation", Description="Ethnic tension escalation in region", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Social", Category="Land invasion", Description="Land invasion or illegal occupation", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Social", Category="Vigilante action", Description="Community vigilante action or mob justice", Enabled=true, SeverityWeight=82, ScanFrequency="realtime" },
                new() { Domain="Social", Category="Migrant crisis", Description="Migrant crisis pressure increase", Enabled=true, SeverityWeight=72, ScanFrequency="weekly" },
                new() { Domain="Social", Category="Religious conflict", Description="Religious conflict incident", Enabled=true, SeverityWeight=80, ScanFrequency="daily" },
                new() { Domain="Social", Category="Youth unemployment spike", Description="Youth unemployment rate spike", Enabled=true, SeverityWeight=68, ScanFrequency="weekly" },
                new() { Domain="Social", Category="Social media incitement", Description="Social media incitement to violence", Enabled=true, SeverityWeight=85, ScanFrequency="realtime" },

                // MOBILITY (10)
                new() { Domain="Mobility", Category="Passport law change", Description="Passport law or regulation change", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Mobility", Category="Visa requirement change", Description="Visa requirement change for destination", Enabled=true, SeverityWeight=82, ScanFrequency="daily" },
                new() { Domain="Mobility", Category="Airport closure", Description="Airport closure or flight restrictions", Enabled=true, SeverityWeight=90, ScanFrequency="realtime" },
                new() { Domain="Mobility", Category="Border closure", Description="International border closure", Enabled=true, SeverityWeight=95, ScanFrequency="realtime" },
                new() { Domain="Mobility", Category="Airline route cancelled", Description="Key airline route cancellation", Enabled=true, SeverityWeight=72, ScanFrequency="daily" },
                new() { Domain="Mobility", Category="US immigration change", Description="Immigration policy change affecting US entry", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Mobility", Category="Green Card regulation", Description="Green Card regulation change", Enabled=true, SeverityWeight=90, ScanFrequency="daily" },
                new() { Domain="Mobility", Category="Travel ban", Description="Travel ban imposition affecting routes", Enabled=true, SeverityWeight=92, ScanFrequency="realtime" },
                new() { Domain="Mobility", Category="Consulate closure", Description="Embassy or consulate closure", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Mobility", Category="Emergency evacuation", Description="Emergency evacuation advisory issued", Enabled=true, SeverityWeight=98, ScanFrequency="realtime" },

                // INFRASTRUCTURE (10)
                new() { Domain="Infra", Category="Grid collapse", Description="National power grid collapse", Enabled=true, SeverityWeight=95, ScanFrequency="realtime" },
                new() { Domain="Infra", Category="Telecoms outage", Description="Major telecommunications carrier outage", Enabled=true, SeverityWeight=82, ScanFrequency="realtime" },
                new() { Domain="Infra", Category="Fuel shortage", Description="Fuel shortage or supply disruption", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Infra", Category="Road collapse", Description="Major road collapse or critical accident", Enabled=true, SeverityWeight=72, ScanFrequency="realtime" },
                new() { Domain="Infra", Category="Banking system failure", Description="Banking system outage or failure", Enabled=true, SeverityWeight=88, ScanFrequency="realtime" },
                new() { Domain="Infra", Category="Internet backbone damage", Description="Internet backbone or undersea cable damage", Enabled=true, SeverityWeight=85, ScanFrequency="realtime" },
                new() { Domain="Infra", Category="Water treatment failure", Description="Water treatment plant failure", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Infra", Category="Rail system stoppage", Description="Rail system stoppage or derailment", Enabled=true, SeverityWeight=68, ScanFrequency="daily" },
                new() { Domain="Infra", Category="Port closure", Description="Major port closure affecting supply chain", Enabled=true, SeverityWeight=75, ScanFrequency="daily" },
                new() { Domain="Infra", Category="Supply chain failure", Description="Critical supply chain failure", Enabled=true, SeverityWeight=82, ScanFrequency="daily" },

                // PREPPER / SOVEREIGNTY (10)
                new() { Domain="Sovereignty", Category="Gun confiscation", Description="Government gun confiscation move", Enabled=true, SeverityWeight=92, ScanFrequency="daily" },
                new() { Domain="Sovereignty", Category="Self-defense law change", Description="Self-defense law weakening", Enabled=true, SeverityWeight=85, ScanFrequency="weekly" },
                new() { Domain="Sovereignty", Category="Property right erosion", Description="Homestead or property right erosion", Enabled=true, SeverityWeight=88, ScanFrequency="daily" },
                new() { Domain="Sovereignty", Category="Food sovereignty threat", Description="Seed or food sovereignty restriction", Enabled=true, SeverityWeight=82, ScanFrequency="weekly" },
                new() { Domain="Sovereignty", Category="Off-grid restriction", Description="Off-grid living restriction enacted", Enabled=true, SeverityWeight=78, ScanFrequency="weekly" },
                new() { Domain="Sovereignty", Category="Alt currency ban", Description="Barter or alternative currency ban", Enabled=true, SeverityWeight=85, ScanFrequency="daily" },
                new() { Domain="Sovereignty", Category="Emergency freq restriction", Description="Emergency communications frequency restriction", Enabled=true, SeverityWeight=75, ScanFrequency="weekly" },
                new() { Domain="Sovereignty", Category="Energy independence threat", Description="Solar or energy independence restriction", Enabled=true, SeverityWeight=78, ScanFrequency="weekly" },
                new() { Domain="Sovereignty", Category="Rainwater collection ban", Description="Rainwater collection ban or restriction", Enabled=true, SeverityWeight=72, ScanFrequency="weekly" },
                new() { Domain="Sovereignty", Category="Community defense threat", Description="Community defense or militia restriction", Enabled=true, SeverityWeight=80, ScanFrequency="weekly" },
            };
        }

        public static List<CrimeHotspot> GetSouthAfricaHotspots()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new List<CrimeHotspot>
            {
                // Gauteng
                new() { Latitude=-26.2041, Longitude=28.0473, RadiusMeters=3000, CrimeType="Hijacking", Severity=92, TimePattern="17:00-21:00", Precinct="Johannesburg Central", LocationName="Johannesburg CBD", IncidentCount90d=185, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-26.1076, Longitude=28.0567, RadiusMeters=2500, CrimeType="Hijacking", Severity=88, TimePattern="06:00-08:00,17:00-20:00", Precinct="Sandton", LocationName="Sandton / Grayston", IncidentCount90d=95, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-26.2708, Longitude=28.1123, RadiusMeters=2000, CrimeType="Armed Robbery", Severity=85, TimePattern="18:00-23:00", Precinct="Alberton", LocationName="Alberton Mall Area", IncidentCount90d=72, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-25.7479, Longitude=28.2293, RadiusMeters=3000, CrimeType="Hijacking", Severity=82, TimePattern="06:30-08:30,16:00-19:00", Precinct="Pretoria Central", LocationName="Pretoria CBD", IncidentCount90d=110, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-26.1496, Longitude=28.0081, RadiusMeters=2500, CrimeType="Home Invasion", Severity=78, TimePattern="22:00-04:00", Precinct="Randburg", LocationName="Randburg / Ferndale", IncidentCount90d=45, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-26.3352, Longitude=27.8580, RadiusMeters=3000, CrimeType="Armed Robbery", Severity=80, TimePattern="All hours", Precinct="Soweto", LocationName="Soweto", IncidentCount90d=150, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-26.1870, Longitude=28.1900, RadiusMeters=2000, CrimeType="Hijacking", Severity=84, TimePattern="17:00-20:00", Precinct="Boksburg", LocationName="Boksburg / East Rand Mall", IncidentCount90d=68, CountryCode="ZA", Active=true, UpdatedAt=now },

                // KwaZulu-Natal
                new() { Latitude=-29.8587, Longitude=31.0218, RadiusMeters=3000, CrimeType="Hijacking", Severity=85, TimePattern="16:00-21:00", Precinct="Durban Central", LocationName="Durban CBD", IncidentCount90d=130, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-29.7806, Longitude=30.8310, RadiusMeters=2500, CrimeType="Armed Robbery", Severity=78, TimePattern="18:00-23:00", Precinct="Pinetown", LocationName="Pinetown", IncidentCount90d=55, CountryCode="ZA", Active=true, UpdatedAt=now },

                // Western Cape
                new() { Latitude=-33.9249, Longitude=18.4241, RadiusMeters=2000, CrimeType="Armed Robbery", Severity=75, TimePattern="20:00-02:00", Precinct="Cape Town Central", LocationName="Cape Town CBD", IncidentCount90d=95, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-34.0551, Longitude=18.4696, RadiusMeters=3000, CrimeType="Gang Violence", Severity=90, TimePattern="All hours", Precinct="Mitchell's Plain", LocationName="Cape Flats / Mitchell's Plain", IncidentCount90d=220, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-33.9608, Longitude=18.5100, RadiusMeters=2500, CrimeType="Gang Violence", Severity=88, TimePattern="All hours", Precinct="Nyanga", LocationName="Nyanga / Gugulethu", IncidentCount90d=195, CountryCode="ZA", Active=true, UpdatedAt=now },

                // Eastern Cape
                new() { Latitude=-33.7150, Longitude=25.5680, RadiusMeters=2500, CrimeType="Armed Robbery", Severity=72, TimePattern="19:00-01:00", Precinct="Gqeberha Central", LocationName="Gqeberha (Port Elizabeth)", IncidentCount90d=65, CountryCode="ZA", Active=true, UpdatedAt=now },

                // Free State
                new() { Latitude=-29.1170, Longitude=26.2140, RadiusMeters=2000, CrimeType="Hijacking", Severity=68, TimePattern="17:00-20:00", Precinct="Bloemfontein", LocationName="Bloemfontein CBD", IncidentCount90d=42, CountryCode="ZA", Active=true, UpdatedAt=now },

                // Farm attacks
                new() { Latitude=-25.4500, Longitude=28.7500, RadiusMeters=15000, CrimeType="Farm Attack", Severity=95, TimePattern="02:00-05:00", Precinct="Cullinan", LocationName="Cullinan / Bronkhorstspruit farming area", IncidentCount90d=8, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-27.0000, Longitude=29.5000, RadiusMeters=20000, CrimeType="Farm Attack", Severity=92, TimePattern="01:00-04:00", Precinct="Vryheid", LocationName="Northern KZN farming corridor", IncidentCount90d=12, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-29.6000, Longitude=27.0000, RadiusMeters=18000, CrimeType="Farm Attack", Severity=88, TimePattern="22:00-04:00", Precinct="Bethlehem", LocationName="Eastern Free State farms", IncidentCount90d=6, CountryCode="ZA", Active=true, UpdatedAt=now },
                new() { Latitude=-25.0800, Longitude=29.4600, RadiusMeters=20000, CrimeType="Farm Attack", Severity=90, TimePattern="01:00-05:00", Precinct="Marble Hall", LocationName="Limpopo farming belt", IncidentCount90d=10, CountryCode="ZA", Active=true, UpdatedAt=now },
            };
        }

        public static List<ExitPlanItem> GetExitPlanItems()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new List<ExitPlanItem>
            {
                // Plan A: USA (Primary)
                new() { PlanName="Plan A: USA (Primary)", Category="Documents", SortOrder=1, TaskTitle="Valid Passport", TaskDescription="Ensure SA passport is valid for 6+ months beyond travel date", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Documents", SortOrder=2, TaskTitle="US Green Card / Visa", TaskDescription="Green Card valid and re-entry permit current", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Documents", SortOrder=3, TaskTitle="Birth certificates", TaskDescription="Certified copies for all family members", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Documents", SortOrder=4, TaskTitle="Marriage certificate", TaskDescription="Certified copy of marriage certificate", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Documents", SortOrder=5, TaskTitle="Medical records", TaskDescription="Complete medical records and vaccination cards", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Documents", SortOrder=6, TaskTitle="Academic records", TaskDescription="Certified academic transcripts and qualifications", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Financial", SortOrder=7, TaskTitle="US bank account", TaskDescription="Active US bank account with emergency funds", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Financial", SortOrder=8, TaskTitle="Transfer funds", TaskDescription="Transfer emergency fund (6 months expenses) to US account", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Financial", SortOrder=9, TaskTitle="Crypto wallet", TaskDescription="Hardware wallet with diversified crypto holdings", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Financial", SortOrder=10, TaskTitle="Cash reserve", TaskDescription="USD cash reserve accessible ($5,000 minimum)", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Logistics", SortOrder=11, TaskTitle="Flight routes mapped", TaskDescription="3 alternative flight routes JNB?US mapped", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Logistics", SortOrder=12, TaskTitle="US accommodation", TaskDescription="First 30 days accommodation arranged", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Logistics", SortOrder=13, TaskTitle="Vehicle plan", TaskDescription="Vehicle purchase or rental plan for US arrival", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Logistics", SortOrder=14, TaskTitle="Go-bag packed", TaskDescription="Go-bag with 72-hour essentials ready", CreatedAt=now },
                new() { PlanName="Plan A: USA (Primary)", Category="Logistics", SortOrder=15, TaskTitle="Pet logistics", TaskDescription="Pet transport and quarantine requirements sorted", CreatedAt=now },

                // Plan B: Portugal (Backup)
                new() { PlanName="Plan B: Portugal (Backup)", Category="Documents", SortOrder=1, TaskTitle="Valid Passport", TaskDescription="SA passport valid for Schengen entry", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Documents", SortOrder=2, TaskTitle="D7 Visa application", TaskDescription="Portugal D7 passive income visa prepared", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Documents", SortOrder=3, TaskTitle="NIF number", TaskDescription="Portuguese tax number (NIF) obtained", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Documents", SortOrder=4, TaskTitle="Health insurance", TaskDescription="EU-valid health insurance policy", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Financial", SortOrder=5, TaskTitle="EU bank account", TaskDescription="European bank account opened (e.g., Wise, N26)", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Financial", SortOrder=6, TaskTitle="Proof of income", TaskDescription="Passive income documentation for visa", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Financial", SortOrder=7, TaskTitle="EUR cash reserve", TaskDescription="EUR cash reserve (€3,000 minimum)", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Logistics", SortOrder=8, TaskTitle="Accommodation research", TaskDescription="Lisbon/Porto accommodation options researched", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Logistics", SortOrder=9, TaskTitle="Flight routes", TaskDescription="JNB?LIS flight routes and alternatives mapped", CreatedAt=now },
                new() { PlanName="Plan B: Portugal (Backup)", Category="Logistics", SortOrder=10, TaskTitle="Language basics", TaskDescription="Basic Portuguese learning started", CreatedAt=now },

                // Plan C: Emergency Evacuation
                new() { PlanName="Plan C: Emergency Evacuation", Category="Immediate", SortOrder=1, TaskTitle="Go-bag ready", TaskDescription="72-hour go-bag at front door — passports, cash, meds, chargers", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Immediate", SortOrder=2, TaskTitle="Vehicle fueled", TaskDescription="Vehicle always above half tank", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Immediate", SortOrder=3, TaskTitle="Cash stash", TaskDescription="ZAR, USD, EUR emergency cash accessible within 5 minutes", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Immediate", SortOrder=4, TaskTitle="Family rally point", TaskDescription="Family emergency meeting point agreed", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Routes", SortOrder=5, TaskTitle="Route to BW border", TaskDescription="Fastest route to Botswana border (Kopfontein)", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Routes", SortOrder=6, TaskTitle="Route to MZ border", TaskDescription="Route to Mozambique border (Komatipoort)", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Routes", SortOrder=7, TaskTitle="Route to NA border", TaskDescription="Route to Namibia border (Vioolsdrif)", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Routes", SortOrder=8, TaskTitle="Airport emergency route", TaskDescription="Fastest route to OR Tambo with alternatives", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Comms", SortOrder=9, TaskTitle="Emergency contacts list", TaskDescription="Printed list — embassy numbers, family, lawyer", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Comms", SortOrder=10, TaskTitle="Satellite phone / eSIM", TaskDescription="Backup communication method (Starlink, eSIM, sat phone)", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Comms", SortOrder=11, TaskTitle="Signal/WhatsApp groups", TaskDescription="Emergency family group chats tested", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Survival", SortOrder=12, TaskTitle="First aid kit", TaskDescription="Comprehensive first aid kit in vehicle", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Survival", SortOrder=13, TaskTitle="Water purification", TaskDescription="Portable water filter/purification tablets", CreatedAt=now },
                new() { PlanName="Plan C: Emergency Evacuation", Category="Survival", SortOrder=14, TaskTitle="Self-defense plan", TaskDescription="Legal self-defense options prepared", CreatedAt=now },
            };
        }
    }
}
