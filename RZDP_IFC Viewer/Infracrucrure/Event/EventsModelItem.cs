namespace IFC_Table_View.Infracrucrure
{
    public class PropertyReferenceChangedEventArg : EventArgs
    {
        public bool IsContainPropertyDownTreeReference { get; set; }

        public PropertyReferenceChangedEventArg(bool IsContainPropertyDownTreeReference)
        {
            this.IsContainPropertyDownTreeReference = IsContainPropertyDownTreeReference;
        }
    }

    public class PropertyExpandChangedEventArg : EventArgs
    {
        public bool IsExpandTree { get; set; }

        public PropertyExpandChangedEventArg(bool IsExpandTree)
        {
            this.IsExpandTree = IsExpandTree;
        }
    }
}