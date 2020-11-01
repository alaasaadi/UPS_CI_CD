using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using UPS.Core.Attributes;

namespace UPS.Core.Models
{
    public class Employee : IModel
    {
        [JsonPropertyName("id")]
        [SearchableField(FieldName:"id", ExcludeDefaultValue: true, ExcludedValues: new object[] {0})]
        [Export]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        [SearchableField("name")]
        [Export]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        [SearchableField("email")]
        [Export]
        public string Email { get; set; }

        [JsonPropertyName("gender")]
        [SearchableField("gender")]
        [Export]
        public Gender? Gender { get; set; }

        [JsonPropertyName("status")]
        [SearchableField("status")]
        [Export]
        public Status? Status { get; set; }

        [JsonPropertyName("created_at")]
        [DisplayName("Create Time")]
        [Export]
        public DateTime? CreateTime { get; set; }

        [JsonPropertyName("updated_at")]
        [DisplayName("Update Time")]
        [Export]
        public DateTime? UpdateTime { get; set; }


        public int CompareTo(object obj)
        { 
            if (obj == null || !(obj is Employee)) 
                throw new ArgumentException("Can not compare to null or non-Employee object");

            Employee that = (obj as Employee);

            // comparing two employee objects based on UpdateTime property in order handle data concurrency            
            if (this.UpdateTime == that.UpdateTime) 
                return 0;
            else if (this.UpdateTime < that.UpdateTime) 
                return -1;
            else 
                return 1;
        }
    }
}
