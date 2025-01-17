namespace IFC_Table_View.HelperIFC
{
    internal static class HelreptReplaceSymbols
    {
        public static void ReplacingSymbols(ref string nameAssembl)
        {
            string characters = @"\./:{}[]|;<>?'~";

            for (int i = 0; i < characters.Length; i++)
            {
                if (nameAssembl.Contains(characters[i]))
                {
                    nameAssembl = nameAssembl.Replace(characters[i], '_');
                }
            }

            nameAssembl = nameAssembl.Replace("__", "_");

            if (nameAssembl[nameAssembl.Length - 1] == '_')
            {
                nameAssembl = nameAssembl.Remove(nameAssembl.Length - 1);
            }
        }
    }
}