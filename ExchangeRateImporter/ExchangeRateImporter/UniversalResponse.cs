using System.Collections.Generic;
using System.Xml.Serialization;


namespace ExchangeRateImporter
{
    [XmlRoot("UniversalResponse", Namespace = "http://www.cargowise.com/Schemas/Universal/2011/11")]
    public class UniversalResponse
    {
        [XmlElement("Status")]
        public string Status { get; set; }

        [XmlElement("Data")]
        public Data Data { get; set; }

        [XmlElement("MessageNumberCollection")]
        public MessageNumberCollection MessageNumberCollection { get; set; }

        [XmlElement("ProcessingLog")]
        public string ProcessingLog { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("xmlns")]
        public string Xmlns { get; set; }
    }

    public class Data
    {
        [XmlElement("UniversalShipment")]
        public UniversalShipment UniversalShipment { get; set; }
    }

    public class UniversalShipment
    {
        [XmlElement("Shipment")]
        public Shipment Shipment { get; set; }
    }

    public class Shipment
    {
        [XmlElement("DataContext")]
        public DataContext DataContext { get; set; }

        [XmlElement("ActualChargeable")]
        public string ActualChargeable { get; set; }

        [XmlElement("AdditionalTerms")]
        public string AdditionalTerms { get; set; }

        [XmlElement("GoodsDescription")]
        public string GoodsDescription { get; set; }

        [XmlElement("CompanyTariffLevelOverride")]
        public string CompanyTariffLevelOverride { get; set; }

        [XmlElement("DocumentedChargeable")]
        public string DocumentedChargeable { get; set; }

        [XmlElement("DocumentedVolume")]
        public string DocumentedVolume { get; set; }

        [XmlElement("DocumentedWeight")]
        public string DocumentedWeight { get; set; }
    }

    public class DataContext
    {
        [XmlElement("DataSourceCollection")]
        public DataSourceCollection DataSourceCollection { get; set; }

        [XmlElement("Company")]
        public Company Company { get; set; }

        [XmlElement("DataProvider")]
        public string DataProvider { get; set; }

        [XmlElement("EnterpriseID")]
        public string EnterpriseID { get; set; }

        [XmlElement("ServerID")]
        public string ServerID { get; set; }
    }

    public class DataSourceCollection
    {
        [XmlElement("DataSource")]
        public DataSource DataSource { get; set; }
    }

    public class DataSource
    {
        [XmlElement("Type")]
        public string Type { get; set; }

        [XmlElement("Key")]
        public string Key { get; set; }
    }

    public class Company
    {
        [XmlElement("Code")]
        public string Code { get; set; }

        [XmlElement("Country")]
        public Country Country { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }
    }

    public class Country
    {
        [XmlElement("Code")]
        public string Code { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }
    }

    public class MessageNumberCollection
    {
        [XmlElement("MessageNumber")]
        public List<MessageNumber> MessageNumbers { get; set; }
    }

    public class MessageNumber
    {
        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

}

