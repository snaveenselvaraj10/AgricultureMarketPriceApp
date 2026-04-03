namespace AgricultureMarketPriceApp.Services
{
    public enum StateEnum
    {
        Unknown,
        TamilNadu,
        Karnataka,
        Kerala,
        AndhraPradesh,
        Telangana,
        Maharashtra,
        Gujarat,
        Punjab,
        Haryana,
        UttarPradesh,
        Bihar,
        WestBengal
        // add more as needed
    }

    public enum CommodityEnum
    {
        Unknown,
        Onion,
        Potato,
        Tomato,
        Rice,
        Wheat,
        Maize,
        Cotton,
        Coconut,
        Yam_Ratalu,
        TamarindFruit,
        Banana,
        Mango,
        Arecanut,
        Sugarcane,
        Groundnut,
        Garlic,
        Ginger,
        Brinjal,
        Capsicum,
        Cucumber
        // add more as needed
    }

    // District enum to support district-level filtering (add entries as needed)
    public enum DistrictEnum
    {
        Unknown,
        Vellore,
        Dindigul,
        Madurai,
        Coimbatore,
        Chennai,
        Salem,
        Erode,
        Tirunelveli,
        Thanjavur,
        Kanyakumari,
        Tiruchirappalli,
        Thoothukudi
        // add more as needed
    }

    public static class ApiEnumExtensions
    {
        public static string ToApiState(this StateEnum state)
        {
            return state switch
            {
                StateEnum.TamilNadu => "Tamil Nadu",
                StateEnum.Karnataka => "Karnataka",
                StateEnum.Kerala => "Kerala",
                StateEnum.AndhraPradesh => "Andhra Pradesh",
                StateEnum.Telangana => "Telangana",
                StateEnum.Maharashtra => "Maharashtra",
                StateEnum.Gujarat => "Gujarat",
                StateEnum.Punjab => "Punjab",
                StateEnum.Haryana => "Haryana",
                StateEnum.UttarPradesh => "Uttar Pradesh",
                StateEnum.Bihar => "Bihar",
                StateEnum.WestBengal => "West Bengal",
                _ => null,
            };
        }

        public static string ToApiCommodity(this CommodityEnum commodity)
        {
            return commodity switch
            {
                CommodityEnum.Onion => "Onion",
                CommodityEnum.Potato => "Potato",
                CommodityEnum.Tomato => "Tomato",
                CommodityEnum.Rice => "Rice",
                CommodityEnum.Wheat => "Wheat",
                CommodityEnum.Maize => "Maize",
                CommodityEnum.Cotton => "Cotton",
                CommodityEnum.Coconut => "Coconut",
                CommodityEnum.Yam_Ratalu => "Yam (Ratalu)",
                CommodityEnum.TamarindFruit => "Tamarind Fruit",
                CommodityEnum.Banana => "Banana",
                CommodityEnum.Mango => "Mango",
                CommodityEnum.Arecanut => "Arecanut",
                CommodityEnum.Sugarcane => "Sugarcane",
                CommodityEnum.Groundnut => "Groundnut",
                CommodityEnum.Garlic => "Garlic",
                CommodityEnum.Ginger => "Ginger",
                CommodityEnum.Brinjal => "Brinjal",
                CommodityEnum.Capsicum => "Capsicum",
                CommodityEnum.Cucumber => "Cucumber",
                _ => null,
            };
        }

        // Return a list of districts (API display names) for a given state enum.
        public static List<string> GetDistrictsForState(this StateEnum state)
        {
            return state switch
            {
                StateEnum.TamilNadu => new List<string>
                {
                    "Ariyalur","Chennai","Coimbatore","Cuddalore","Dharmapuri","Dindigul","Erode",
                    "Kallakurichi","Kanchipuram","Kanyakumari","Karur","Krishnagiri","Madurai","Nagapattinam",
                    "Namakkal","Perambalur","Pudukkottai","Ramanathapuram","Ranipet","Salem","Sivaganga",
                    "Tenkasi","Thanjavur","Theni","Thoothukudi","Tiruchirappalli","Tirunelveli","Tirupathur",
                    "Tiruppur","Tiruvallur","Tiruvannamalai","Tiruvarur","Vellore","Viluppuram","Virudhunagar"
                },
                StateEnum.Karnataka => new List<string> { "Bengaluru Urban", "Bengaluru Rural", "Mysuru", "Dharwad" },
                StateEnum.Kerala => new List<string> { "Thiruvananthapuram", "Kollam", "Ernakulam", "Kozhikode" },
                _ => new List<string>()
            };
        }

        public static string ToApiDistrict(this DistrictEnum district)
        {
            return district switch
            {
                DistrictEnum.Vellore => "Vellore",
                DistrictEnum.Dindigul => "Dindigul",
                DistrictEnum.Madurai => "Madurai",
                DistrictEnum.Coimbatore => "Coimbatore",
                DistrictEnum.Chennai => "Chennai",
                DistrictEnum.Salem => "Salem",
                DistrictEnum.Erode => "Erode",
                DistrictEnum.Tirunelveli => "Tirunelveli",
                DistrictEnum.Thanjavur => "Thanjavur",
                DistrictEnum.Kanyakumari => "Kanyakumari",
                DistrictEnum.Tiruchirappalli => "Tiruchirappalli",
                DistrictEnum.Thoothukudi => "Thoothukudi",
                _ => null,
            };
        }
    }
}
