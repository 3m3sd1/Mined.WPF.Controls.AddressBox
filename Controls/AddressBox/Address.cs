namespace Mined.WPF.Controls
{
    public class Address
    {
        public string Country { get; set; }
        public string StateOrTerritory { get; set; }
        public string Locality { get; set; }
        public string PostCode { get; set; }
        public string StreetName { get; set; }
        public string StreetNo { get; set; }

        public bool IsComplete
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Country) && !string.IsNullOrWhiteSpace(StateOrTerritory)
                                                           && !string.IsNullOrWhiteSpace(Locality)
                                                           && !string.IsNullOrWhiteSpace(PostCode)
                                                           && !string.IsNullOrWhiteSpace(StreetName)
                                                           && !string.IsNullOrWhiteSpace(StreetNo);
            }
        }

        public override string ToString()
        {
            return IsComplete? $"{StreetNo} {StreetName}, {Locality} {StateOrTerritory} {PostCode}, {Country}" : string.Empty;
        }


    }
}