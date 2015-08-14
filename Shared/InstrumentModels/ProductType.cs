namespace Shared.InstrumentModels
{
    public class ProductType
    {
        public string Name { get; set; }

        public string ProductId { get; set; }
    }

    public class ProductDetailType
    {
        public string Name { get; set; }

        public string ProductId { get; set; }

        public string TypeCode { get; set; }
    }
}