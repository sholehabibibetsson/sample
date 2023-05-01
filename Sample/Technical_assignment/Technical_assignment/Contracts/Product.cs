using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;

namespace Technical_assignment.Contracts
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public List<DayOfWeek> DeliveryDays { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ProductType Type { get; set; }
        public int DaysInAdvance { get; set; }
    }

    public enum ProductType
    {
        Normal,
        External,
        Temporary
    }

}
