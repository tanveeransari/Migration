using System.Text;

namespace Shared.InstrumentModels
{
    public class ProductClass
    {
        // keys Channel | Product | Type (F,S,P)| Security ID | Sd | Message-specific value
        public string RawRedisString { get; set; }

        public string Name { get; set; }

        public string Channel { get; set; }

        public bool Equals(ProductClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ProductClass)) return false;
            return Equals((ProductClass)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public class Product
    {
        // keys Channel | Product | Type (F,S,P)| Security ID | Sd | Message-specific value
        public string RawRedisString { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public string Class { get; set; }

        public string Type { get; set; }

        public string Channel { get; set; }

        //get "9|GE|F|GEM3|806301|B|5"
        public string GetBookSubscribeKey()
        {
            StringBuilder sb = new StringBuilder(50);
            sb.Append(Channel);
            sb.Append("|");
            sb.Append(Class);
            sb.Append("|");
            sb.Append(Type);
            sb.Append("|");
            sb.Append(Name);
            sb.Append("|");
            sb.Append(Id);
            sb.Append("|B|5");

            return sb.ToString();
        }

        /*
         here is an example of a Trade Message key:
9|GE|F|GEH7|801250|Tb|3|9826|10|60549000
channel|product|symbol-type|symbol|sid|Tb|trade-count|price|qty|time

this message will query all "buy" trades hence the "b" but if you want ALL trades, you type T*

here is an example of a Statistics Message key:
9|GE|F|GEU6|801355|ST|LBBP
         */

        //get "9|GE|F|GEM3|806301|B|5"
        public string GetTradeSubscribeKey()
        {
            StringBuilder sb = new StringBuilder(50);
            sb.Append(Channel);
            sb.Append("|");
            sb.Append(Class);
            sb.Append("|");
            sb.Append(Type);
            sb.Append("|");
            sb.Append(Name);
            sb.Append("|");
            sb.Append(Id);
            sb.Append("|T");

            return sb.ToString();
        }

        public string GetLastTradeSubscribeKey()
        {
            //"9|GE|F|GEZ2|812201|L" (KEY for last trade on symbol GEZ2)
            StringBuilder sb = new StringBuilder(50);
            sb.Append(Channel);
            sb.Append("|");
            sb.Append(Class);
            sb.Append("|");
            sb.Append(Type);
            sb.Append("|");
            sb.Append(Name);
            sb.Append("|");
            sb.Append(Id);
            sb.Append("|L");

            return sb.ToString();
        }

        public string GetStatisticsSubscribeKey()
        {
            StringBuilder sb = new StringBuilder(50);
            sb.Append(Channel);
            sb.Append("|");
            sb.Append(Class);
            sb.Append("|");
            sb.Append(Type);
            sb.Append("|");
            sb.Append(Name);
            sb.Append("|");
            sb.Append(Id);
            sb.Append("|ST");

            return sb.ToString();
        }
    }
}