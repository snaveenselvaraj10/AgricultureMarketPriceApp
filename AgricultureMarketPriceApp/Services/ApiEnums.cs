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
        Cotton
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
                _ => null,
            };
        }
    }
}
